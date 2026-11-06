# Adaptadores para os coletores da edição de terminal.
function Import-SuporteWindows {
    if (-not ('SuporteTI.MensagensWindows' -as [type])) {
        $dll = Join-Path $PSScriptRoot 'Coletores/SuporteTI.Windows.dll'
        [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes($dll))
    }
}

function Get-SecaoTexto {
    param([string]$Nome, [scriptblock]$Consultar)
    try {
        $linhas = @(& $Consultar | Where-Object { $_ -isnot [string] -or -not [string]::IsNullOrWhiteSpace($_) })
        $prefixo = '[INDISPONIVEL]'
        $avisos = @($linhas | Where-Object { $_ -is [string] -and $_.StartsWith($prefixo) } | ForEach-Object { $_.Substring($prefixo.Length).Trim() })
        $campos = @(foreach ($linha in $linhas) {
            if ($linha -isnot [string]) {
                @{ nome = 'Código · ' + $linha.Familia; valor = $linha.Codigo; familia = $linha.Familia }
            } elseif ($linha.StartsWith($prefixo)) { continue }
            elseif ($linha -match '^(Destino|Respostas|Latencia \(ms\)|Download|Upload|Nome tecnico|O que investigar|Referencia):\s*(.+)$') {
                @{ nome = $Matches[1]; valor = $Matches[2] }
            } else {
                @{ nome = 'Detalhe'; valor = $linha.Trim() }
            }
        })
        if (-not $campos.Count -and -not $avisos.Count) { $avisos = @('O coletor não retornou dados.') }
        $aviso = $null
        if ($avisos.Count) { $aviso = [string]::Join([Environment]::NewLine, [string[]]$avisos) }
        @{ nome = $Nome; campos = $campos; aviso = $aviso }
    } catch {
        @{ nome = $Nome; campos = @(); aviso = $_.Exception.Message }
    }
}

function Get-RedeTriagem {
    param([string]$Id, [string]$Entrada, [string]$Familia)
    switch ($Id) {
        'rede' {
            $adaptadores = Get-Lista 'Adaptadores com IP ativo' {
                Get-CimInstance Win32_NetworkAdapterConfiguration -Filter 'IPEnabled=True' -OperationTimeoutSec 8 |
                    Sort-Object Description | ForEach-Object {
                        $detalhes = @(
                            'IP: ' + ($_.IPAddress -join ', ')
                            'Máscara/prefixo: ' + ($_.IPSubnet -join ', ')
                            'Gateway: ' + ($_.DefaultIPGateway -join ', ')
                            'DNS: ' + ($_.DNSServerSearchOrder -join ', ')
                            'DHCP: ' + $(if ($_.DHCPEnabled) { 'Ativo' } else { 'Desativado' })
                            'MAC: ' + $_.MACAddress
                        )
                        @{ nome = $_.Description; valor = $detalhes -join [Environment]::NewLine }
                    }
            } 'Nenhum adaptador com IP ativo foi listado.'
            $adaptadores
        }
        'dominio' {
            Get-Secao 'Ingresso no Windows' {
                $pc = Get-CimInstance Win32_ComputerSystem -OperationTimeoutSec 5
                [ordered]@{
                    'Computador' = $pc.Name
                    'Ingresso em domínio' = $(if ($pc.PartOfDomain) { 'Sim' } else { 'Não' })
                    'Domínio ou grupo de trabalho' = $pc.Domain
                }
            }
        }
        'ping' {
            . (Join-Path $PSScriptRoot 'Coletores/Rede.ps1')
            Get-SecaoTexto 'Ping ICMP · quatro tentativas' { Invoke-SuportePing $Entrada }
        }
        'velocidade' {
            . (Join-Path $PSScriptRoot 'Coletores/Rede.ps1')
            Get-SecaoTexto 'Download · Cloudflare' { Measure-SuporteVelocidade Download }
            Get-SecaoTexto 'Upload · Cloudflare' { Measure-SuporteVelocidade Upload }
        }
        'falhas' {
            . (Join-Path $PSScriptRoot 'Coletores/Falhas.ps1')
            Get-SecaoTexto 'Triagem de eventos · últimas 24 horas' { Get-SuporteDiagnosticoFalhas -IncluirCodigos }
        }
        'codigofalha' {
            . (Join-Path $PSScriptRoot 'Coletores/Falhas.ps1')
            if ($Familia) {
                Get-SecaoTexto 'Descrição do código' { Get-SuporteExplicacaoCodigo $Familia $Entrada }
            } else {
                Get-CodigoAutomatico $Entrada
            }
        }
    }
}

function Get-CodigoAutomatico {
    param([string]$Entrada)
    $codigo = [uint32]0
    $negativo = 0
    if ($Entrada -match '^0x([0-9a-f]{1,8})$') {
        $codigo = [Convert]::ToUInt32($Matches[1],16)
    } elseif ($Entrada -match '^-[0-9]{1,10}$' -and [int]::TryParse($Entrada,[ref]$negativo) -and $negativo -lt 0) {
        $codigo = [uint32]([long]$negativo + 4294967296)
    } elseif ($Entrada -notmatch '^\d{1,10}$' -or -not [uint32]::TryParse($Entrada,[ref]$codigo)) {
        return @{ nome = 'Pesquisa de código'; campos = @(); aviso = 'Informe um número decimal de 32 bits ou hexadecimal com prefixo 0x.' }
    }
    $hex = '{0:X8}' -f $codigo
    $catalogo = $null
    try { $catalogo = Read-SuporteCatalogoFalhas }
    catch {
        @{ nome = 'Catálogo'; campos = @(); aviso = 'Catálogo offline indisponível. A busca usa as mensagens do Windows quando disponíveis.' }
    }
    $correspondencias = @(foreach ($familia in 'Win32','HRESULT','Excecoes','Paradas') {
        $registrado = $catalogo -and $catalogo.$familia.PSObject.Properties[$hex]
        $mensagemNativa = ($familia -eq 'Win32' -and $codigo -le 65535) -or
            ($familia -eq 'HRESULT' -and $hex.StartsWith('8007'))
        if ($registrado -or $mensagemNativa) {
            $descricao = Find-SuporteCodigoFalha $catalogo $familia $codigo
            if ($descricao) { @{ Familia = $familia; Descricao = $descricao } }
        }
    })
    if (-not $correspondencias.Count) {
        return @{ nome = 'Pesquisa de código'; campos = @(@{ nome = 'Código'; valor = "0x$hex" });
            aviso = 'Código sem correspondência no catálogo ou nas mensagens consultadas.' }
    }
    if ($correspondencias.Count -gt 1) {
        @{ nome = 'Contexto'; campos = @(@{ nome = 'Mais de uma família'; valor = 'O mesmo código tem significados diferentes. Use a origem do erro para identificar a interpretação.' }); aviso = $null }
    }
    foreach ($item in $correspondencias) {
        Get-SecaoTexto $item.Familia { Format-SuporteExplicacaoCodigo $item.Familia $codigo $item.Descricao }
    }
}
