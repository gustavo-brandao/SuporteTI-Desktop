# Coletores locais de leitura da area Computador.
function Get-Lista {
    param([string]$Nome, [scriptblock]$Consultar, [string]$Vazio = 'Nenhum item encontrado.')
    try {
        $campos = @(& $Consultar)
        if (-not $campos.Count) { $campos = @(@{ nome = 'Resultado'; valor = $Vazio }) }
        @{ nome = $Nome; campos = $campos; aviso = $null }
    } catch {
        @{ nome = $Nome; campos = @(); aviso = $_.Exception.Message }
    }
}

function Get-Diagnostico {
    param([string]$Id)
    switch ($Id) {
        'desempenho' {
            Get-Secao 'CPU' { [ordered]@{ 'Carga atual' = '{0}%' -f (Get-SuporteCpuPercentual) } }
            Get-Secao 'Memória' { [ordered]@{ 'Leitura atual' = (Get-SuporteMemoria).Resumo } }
            Get-Lista 'Processos por memória' {
                Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 5 | ForEach-Object {
                    @{ nome = $_.ProcessName; valor = '{0:N0} MiB · PID {1}' -f ($_.WorkingSet64 / 1MB), $_.Id }
                }
            }
        }
        'discos' {
            Get-Lista 'Volumes locais' {
                Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' -OperationTimeoutSec 5 | Sort-Object DeviceID | ForEach-Object {
                    $percentual = $null
                    $resumo = if ($_.Size -gt 0 -and $null -ne $_.FreeSpace -and $_.FreeSpace -le $_.Size) {
                        $percentual = 100.0 * (1 - $_.FreeSpace / $_.Size)
                        '{0:P0} ocupado · {1:N1} GiB livres de {2:N1} GiB' -f (1 - $_.FreeSpace / $_.Size), ($_.FreeSpace / 1GB), ($_.Size / 1GB)
                    } else { 'Capacidade indisponível.' }
                    @{ nome = $_.DeviceID; valor = $resumo; percentual = $percentual }
                }
            }
            . (Join-Path $PSScriptRoot 'Coletores/Discos.ps1')
            $saude = Get-Lista 'Saúde dos discos internos' {
                Get-SuporteSmartResumo -Estruturado | ForEach-Object {
                    if ($_ -is [string]) { @{ nome = 'Resultado'; valor = ($_ -replace '^\[INDISPONIVEL\] ', '') } }
                    else {
                        $percentual = $null
                        $medida = 0.0
                        if ([double]::TryParse([string]$_.Desgaste, [Globalization.NumberStyles]::Float,
                            [Globalization.CultureInfo]::InvariantCulture, [ref]$medida) -and
                            -not [double]::IsNaN($medida) -and -not [double]::IsInfinity($medida) -and $medida -ge 0) {
                            $percentual = $medida
                        }
                        @{ nome = $_.Nome; valor = $_.Texto; percentual = $percentual }
                    }
                }
            }
            if (@($saude.campos | Where-Object { $_.valor -match 'Inconclusivo|SMART parcial|Consulta limitada' }).Count) {
                $saude.aviso = 'Parte dos indicadores SMART está indisponível ou limitada.'
            }
            $saude
        }
        'bateria' {
            $saude = Get-Lista 'Saúde da bateria' {
                $baterias = @(Get-CimInstance Win32_Battery -OperationTimeoutSec 4 | Where-Object { $_.PNPDeviceID -like 'ACPI\*' })
                if (-not $baterias.Count) { return }
                $projetos = @(Get-CimInstance -Namespace root/wmi -ClassName BatteryStaticData -OperationTimeoutSec 4)
                $capacidades = @(Get-CimInstance -Namespace root/wmi -ClassName BatteryFullChargedCapacity -OperationTimeoutSec 4)
                foreach ($projeto in $projetos) {
                    $capacidade = $capacidades | Where-Object InstanceName -EQ $projeto.InstanceName | Select-Object -First 1
                    $nome = if ($projeto.DeviceName) { $projeto.DeviceName } else { 'Bateria interna' }
                    if (-not $capacidade -or $projeto.DesignedCapacity -in 0,([uint32]::MaxValue) -or
                        $capacidade.FullChargedCapacity -in 0,([uint32]::MaxValue)) {
                        @{ nome = $nome; valor = 'Capacidade indisponível' }
                        continue
                    }
                    @{ nome = $nome; valor = '{0:P1} da capacidade original · {1:N1} Wh de {2:N1} Wh' -f
                        ($capacidade.FullChargedCapacity / $projeto.DesignedCapacity), ($capacidade.FullChargedCapacity / 1000), ($projeto.DesignedCapacity / 1000) }
                }
                if (-not $projetos.Count) {
                    throw 'O Windows não informou as capacidades da bateria.'
                }
            } 'Nenhuma bateria interna ACPI detectada.'
            if (@($saude.campos | Where-Object valor -EQ 'Capacidade indisponível').Count) {
                $saude.aviso = 'O Windows não informou a capacidade de todas as baterias.'
            }
            $saude
        }
        'dispositivos' {
            Get-Lista 'Dispositivos com código de problema' {
                Get-CimInstance Win32_PnPEntity -Filter 'ConfigManagerErrorCode <> 0' -OperationTimeoutSec 8 |
                    Where-Object { $_.ConfigManagerErrorCode -ne 24 -or $_.Present -ne $false } | Sort-Object Name |
                    ForEach-Object {
                        $nome = if (-not [string]::IsNullOrWhiteSpace($_.Name)) { $_.Name }
                            elseif (-not [string]::IsNullOrWhiteSpace($_.DeviceID)) { $_.DeviceID }
                            else { 'Dispositivo sem identificação' }
                        @{ nome = $nome; valor = 'Código {0} · {1} · {2}' -f $_.ConfigManagerErrorCode, $_.PNPClass, $_.Status }
                    }
            } 'O Windows não listou problemas em dispositivos presentes.'
        }
        'servicos' {
            Get-Lista 'Serviços locais' {
                Get-Service | Sort-Object DisplayName | ForEach-Object {
                    $situacao = switch ([string]$_.Status) {
                        'Running' { 'Em execução' }
                        'Stopped' { 'Parado' }
                        'Paused' { 'Pausado' }
                        default { $_.Status }
                    }
                    @{ nome = $_.DisplayName; valor = '{0} · {1}' -f $situacao, $_.Name }
                }
            }
        }
        'programas' {
            Get-Lista 'Programas registrados no Windows' {
                $paths = @('HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*',
                    'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
                    'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*')
                Get-ItemProperty -Path $paths -ErrorAction SilentlyContinue |
                    Select-Object DisplayName,DisplayVersion,Publisher | Where-Object DisplayName |
                    Sort-Object DisplayName,DisplayVersion,Publisher -Unique | ForEach-Object {
                        $detalhes = ($_.DisplayVersion,$_.Publisher | Where-Object { $_ }) -join ' · '
                        if (-not $detalhes) { $detalhes = 'Versão e fornecedor indisponíveis' }
                        @{ nome = $_.DisplayName; valor = $detalhes }
                    }
            }
        }
        'autologon' {
            Get-Secao 'Login automático' {
                $chave = Open-SuporteWinlogon
                try { $estado = Get-SuporteEstadoLogon $chave } finally { $chave.Dispose() }
                [ordered]@{
                    'Estado' = $(if ($estado.Ativo) { 'Configurado' } else { 'Desativado' })
                    'Limite de logons' = $(if ($estado.Temporario) { 'Configurado' } else { 'Ausente' })
                    'Entrada de senha legada' = $(if ($estado.SenhaLegada) { 'Detectada' } else { 'Ausente' })
                }
            }
        }
    }
}
function Open-SuporteWinlogon {
    param([switch]$Gravavel)
    $raiz = [Microsoft.Win32.RegistryKey]::OpenBaseKey('LocalMachine','Registry64')
    try {
        $chave = $raiz.OpenSubKey('SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon',[bool]$Gravavel)
        if (-not $chave) { throw 'Chave Winlogon indisponivel.' }
        $chave
    } finally { $raiz.Dispose() }
}

function Get-SuporteEstadoLogon {
    param($Chave)
    [pscustomobject]@{
        Ativo = [string]$Chave.GetValue('AutoAdminLogon') -eq '1'
        Usuario = [string]$Chave.GetValue('DefaultUserName')
        Dominio = [string]$Chave.GetValue('DefaultDomainName')
        SenhaLegada = $Chave.GetValueNames() -contains 'DefaultPassword'
        Temporario = $Chave.GetValueNames() -contains 'AutoLogonCount'
    }
}


function Get-SuporteDiscoClassificacao {
    [CmdletBinding()]
    param([AllowNull()][string]$HealthStatus, [AllowNull()][object]$Wear,
        [AllowNull()][object]$ReadErrorsUncorrected, [AllowNull()][object]$WriteErrorsUncorrected)
    $motivos = New-Object 'System.Collections.Generic.List[string]'
    $estado = 'Inconclusivo'
    if ($HealthStatus -eq 'Healthy') { $estado = 'Sem alertas detectados'; $motivos.Add('Estado nativo informado como Healthy; cobertura limitada.') }
    if ($HealthStatus -eq 'Warning') { $estado = 'Atencao'; $motivos.Add('O Windows informa estado de advertencia.') }
    if ($null -ne $Wear -and [double]$Wear -ge 100) { $estado = 'Atencao'; $motivos.Add('Desgaste informado atingiu ou ultrapassou o limite estimado.') }
    if (($null -ne $ReadErrorsUncorrected -and [double]$ReadErrorsUncorrected -gt 0) -or
        ($null -ne $WriteErrorsUncorrected -and [double]$WriteErrorsUncorrected -gt 0)) {
        $estado = 'Atencao'; $motivos.Add('Ha erros nao corrigidos acumulados; correlacionar com eventos e evolucao.')
    }
    if ($HealthStatus -eq 'Unhealthy') { $estado = 'Critico/degradado'; $motivos.Add('O Windows informa estado Unhealthy. Priorizar protecao dos dados.') }
    if ($motivos.Count -eq 0) { $motivos.Add('Indicadores insuficientes para classificar a saude.') }
    [pscustomobject]@{ Estado=$estado; Motivos=($motivos -join ' '); Limite='Nao e teste de superficie nem garantia de ausencia de defeitos.' }
}


