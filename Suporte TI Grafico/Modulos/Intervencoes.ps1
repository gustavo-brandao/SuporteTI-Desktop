# Intervenções da edição gráfica. A interface confirma a ação antes da chamada.
function Get-FilasLocais {
    Get-Printer -ErrorAction Stop | Where-Object { $_.Type -eq 'Local' } | Sort-Object Name
}

function Get-ConsultaIntervencao {
    param([string]$Id)
    switch ($Id) {
        'adaptadores' {
            Get-Lista 'Adaptadores físicos' {
                $adaptadores = @(Get-NetAdapter -Physical -ErrorAction Stop | Sort-Object Name)
                if (-not $adaptadores.Count) { throw 'Nenhum adaptador físico disponível.' }
                foreach ($adaptador in $adaptadores) {
                    $ips = @(Get-NetIPAddress -InterfaceIndex $adaptador.ifIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue)
                    $ipv4 = if ($ips.Count) { ($ips.IPAddress -join ', ') } else { 'sem IPv4' }
                    @{ nome = "$($adaptador.Name) · $($adaptador.Status) · $ipv4"; valor = ([guid]$adaptador.InterfaceGuid).ToString('D') }
                }
            }
        }
        'filaslocais' {
            Get-Lista 'Filas locais' {
                $filas = @(Get-FilasLocais)
                if (-not $filas.Count) { throw 'Nenhuma fila local de impressão disponível.' }
                foreach ($fila in $filas) { @{ nome = "$($fila.Name) · $($fila.PortName)"; valor = $fila.Name } }
            }
        }
        'chromecache' {
            Get-Lista 'Cache do Chrome' {
                . (Join-Path $PSScriptRoot 'Coletores/Manutencao.ps1')
                function Test-SuporteAdmin {
                    $identidade = [Security.Principal.WindowsIdentity]::GetCurrent()
                    try { ([Security.Principal.WindowsPrincipal]::new($identidade)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator) }
                    finally { $identidade.Dispose() }
                }
                Invoke-SuporteCacheChrome | ForEach-Object { @{ nome = 'Resultado'; valor = $_ } }
            }
        }
        'gpupdateuser' {
            Get-Lista 'Atualização de políticas do usuário' {
                $exe = Join-Path ([Environment]::SystemDirectory) 'gpupdate.exe'
                $saida = @(& $exe '/target:user' '/wait:120')
                $codigo = $LASTEXITCODE
                if ($codigo -ne 0) { throw ("gpupdate retornou $codigo. " + ($saida -join ' ')) }
                foreach ($linha in $saida) {
                    if ($linha.Trim()) { @{ nome = 'Saída do Windows'; valor = $linha.Trim() } }
                }
            }
        }
    }
}

function Get-EstadoIPv4 {
    param([int]$Indice)
    $interface = Get-NetIPInterface -InterfaceIndex $Indice -AddressFamily IPv4 -ErrorAction Stop
    $enderecos = @(Get-NetIPAddress -InterfaceIndex $Indice -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        ForEach-Object { "$($_.IPAddress)/$($_.PrefixLength)" })
    $rotas = @(Get-NetRoute -InterfaceIndex $Indice -AddressFamily IPv4 -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty NextHop -Unique)
    $dns = @(Get-DnsClientServerAddress -InterfaceIndex $Indice -AddressFamily IPv4 -ErrorAction Stop |
        Select-Object -ExpandProperty ServerAddresses)
    'DHCP: {0} · IPv4: {1} · Gateway: {2} · DNS: {3}' -f $interface.Dhcp, ($enderecos -join ', '), ($rotas -join ', '), ($dns -join ', ')
}

function Invoke-IPv4Grafico {
    param([string]$Id, $Dados, [Collections.Generic.List[object]]$Campos)
    . (Join-Path $PSScriptRoot 'Coletores/Rede.ps1')
    $guid = [guid]::Empty
    if (-not [guid]::TryParse([string]$Dados.Alvo, [ref]$guid) -or $guid -eq [guid]::Empty) { throw 'Adaptador inválido.' }
    $alvos = @(Get-NetAdapter -Physical -ErrorAction Stop | Where-Object { [guid]$_.InterfaceGuid -eq $guid })
    if ($alvos.Count -ne 1) { throw 'O adaptador físico selecionado não está mais disponível.' }
    $alvo = $alvos[0]
    $indice = [int]$alvo.ifIndex
    if ($Id -eq 'netstatic') {
        $config = Get-SuporteIPv4Estatico ([string]$Dados.IPv4) ([string]$Dados.Prefixo) ([string]$Dados.Gateway)
        $dns1 = (ConvertTo-SuporteIPv4 ([string]$Dados.Dns1)).ToString()
        $dns2 = $null
        if ($Dados.Dns2) { $dns2 = (ConvertTo-SuporteIPv4 ([string]$Dados.Dns2)).ToString() }
        if ($dns1 -eq $dns2) { throw 'Informe DNS primário e secundário diferentes.' }
        $comandos = @(
            "interface ipv4 set address name=$indice source=static address=$($config.Endereco) mask=$($config.Mascara) gateway=$($config.Gateway) store=persistent"
            "interface ipv4 set dnsservers name=$indice source=static address=$dns1 validate=no"
        )
        if ($dns2) { $comandos += "interface ipv4 add dnsservers name=$indice address=$dns2 index=2 validate=no" }
    } else {
        $comandos = @("interface ipv4 set address name=$indice source=dhcp", "interface ipv4 set dnsservers name=$indice source=dhcp")
    }
    $Campos.Add(@{ nome = 'Adaptador'; valor = $alvo.Name })
    $Campos.Add(@{ nome = 'Antes'; valor = (Get-EstadoIPv4 $indice) })
    $etapa = 0
    try {
        foreach ($comando in $comandos) {
            $atual = Get-NetAdapter -InterfaceIndex $indice -ErrorAction Stop
            if ([guid]$atual.InterfaceGuid -ne $guid) { throw 'O adaptador mudou durante a operação.' }
            $etapa++
            $nativo = Invoke-VerificacaoNativa 'netsh.exe' $comando 30
            if ($nativo.Codigo -ne 0) { throw "Etapa $etapa de $($comandos.Count) falhou. $($nativo.Texto)" }
        }
        $Campos.Add(@{ nome = 'Resultado'; valor = 'Comandos concluídos. A obtenção de endereço DHCP pode levar alguns instantes.' })
    } finally {
        try {
            $atual = Get-NetAdapter -InterfaceIndex $indice -ErrorAction Stop
            if ([guid]$atual.InterfaceGuid -ne $guid) { throw 'O adaptador não corresponde mais à seleção.' }
            $Campos.Add(@{ nome = 'Depois'; valor = (Get-EstadoIPv4 $indice) })
        } catch {
            $Campos.Add(@{ nome = 'Depois'; valor = "Estado indisponível. $($_.Exception.Message)" })
        }
    }
}

function Invoke-IntervencaoElevada {
    param([string]$Id, $Dados, [string]$Volume)
    $campos = [Collections.Generic.List[object]]::new()
    $aviso = $null
    $mutex = $null
    $bloqueado = $false
    try {
        $nomeMutex = if ($Id -in 'autologonenable','autologondisable') { 'Global\SuporteTI.LoginAutomatico' } else { 'Local\SuporteTI.Intervencao' }
        $mutex = [Threading.Mutex]::new($false, $nomeMutex)
        try { $bloqueado = $mutex.WaitOne(0) } catch [Threading.AbandonedMutexException] { $bloqueado = $true }
        if (-not $bloqueado) { throw 'Já existe uma intervenção em andamento nesta sessão.' }
        switch ($Id) {
            'spooler' {
                $servico = Get-Service -Name Spooler -ErrorAction Stop
                $campos.Add(@{ nome = 'Antes'; valor = [string]$servico.Status })
                if ($servico.Status -eq 'Stopped') { Start-Service -Name Spooler -ErrorAction Stop }
                elseif ($servico.Status -eq 'Running') { Restart-Service -Name Spooler -ErrorAction Stop }
                else { throw 'O spooler está em transição. Aguarde e tente novamente.' }
                $servico = Get-Service -Name Spooler -ErrorAction Stop
                $servico.WaitForStatus('Running', [TimeSpan]::FromSeconds(15))
                $campos.Add(@{ nome = 'Depois'; valor = 'Spooler em execução. Nenhum trabalho foi cancelado por esta ação.' })
            }
            'spoolerclear' {
                $filas = @(Get-FilasLocais | Where-Object Name -CEQ ([string]$Dados.Alvo))
                if ($filas.Count -ne 1) { throw 'A fila local selecionada não está mais disponível.' }
                $fila = $filas[0]
                $trabalhos = @(Get-PrintJob -PrinterObject $fila -ErrorAction Stop)
                $removidos = 0
                $falhas = 0
                foreach ($trabalho in $trabalhos) {
                    try { Remove-PrintJob -InputObject $trabalho -Confirm:$false -ErrorAction Stop | Out-Null; $removidos++ }
                    catch { $falhas++ }
                }
                $campos.Add(@{ nome = 'Fila'; valor = $fila.Name })
                $campos.Add(@{ nome = 'Trabalhos'; valor = "$($trabalhos.Count) encontrados · $removidos cancelados · $falhas com falha." })
                if ($falhas) { $aviso = 'Alguns trabalhos não puderam ser cancelados. Consulte novamente a fila.' }
            }
            'cache' {
                . (Join-Path $PSScriptRoot 'Coletores/Operacoes.ps1')
                Invoke-SuporteCache | ForEach-Object { $campos.Add(@{ nome = 'Resultado'; valor = $_ }) }
            }
            'gpupdatecomputer' {
                $nativo = Invoke-VerificacaoNativa 'gpupdate.exe' '/target:computer /wait:120' 150
                $campos.Add(@{ nome = 'Código de saída'; valor = [string]$nativo.Codigo })
                foreach ($linha in ($nativo.Texto -split '[\r\n]+' | Where-Object { $_.Trim() })) {
                    $campos.Add(@{ nome = 'Saída do Windows'; valor = $linha.Trim() })
                }
                if ($nativo.Codigo -ne 0) { $aviso = 'O Windows retornou um erro ao atualizar as políticas.' }
            }
            { $_ -in 'netdhcp','netstatic' } { Invoke-IPv4Grafico $Id $Dados $campos }
            'perfillocal' {
                . (Join-Path $PSScriptRoot 'Coletores/Manutencao.ps1')
                . (Join-Path $PSScriptRoot 'Coletores/Perfis.ps1')
                . (Join-Path $PSScriptRoot 'FluxosSensiveis.ps1')
                $inventario = Get-SuporteInventarioPerfis
                $planos = @(Get-SuportePlanosPerfil $inventario)
                if (-not $planos.Count) {
                    $campos.Add(@{ nome = 'Resultado'; valor = 'Nenhum candidato a perfil duplicado ou temporário encontrado.' })
                    break
                }
                $plano = Select-PerfilElevado $planos
                if (-not $plano) {
                    $campos.Add(@{ nome = 'Resultado'; valor = 'Revisão encerrada sem excluir dados.' })
                    break
                }
                $campos.Add(@{ nome = 'Perfil'; valor = "$($plano.Conta) · $($plano.SID)" })
                Remove-SuportePlanoPerfil $plano $inventario.Raiz
                $campos.Add(@{ nome = 'Resultado'; valor = 'Perfil local removido. No próximo logon, o Windows pode criar um novo perfil conforme a política. Reinicie manualmente se necessário.' })
            }
            { $_ -in 'autologonenable','autologondisable' } {
                . (Join-Path $PSScriptRoot 'Coletores/Logon.ps1')
                . (Join-Path $PSScriptRoot 'FluxosSensiveis.ps1')
                function Import-SuporteWindows {
                    if (-not ('SuporteTI.LogonNativo' -as [type])) {
                        [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes((Join-Path $PSScriptRoot 'Coletores/SuporteTI.Windows.dll')))
                    }
                }
                $chave = Open-SuporteWinlogon -Gravavel
                try {
                    $mensagem = if ($Id -eq 'autologondisable') {
                        Disable-SuporteLogon $chave
                        'Login automático desativado e credencial removida.'
                    } elseif (Set-LogonGrafico $chave) {
                        'Configuração gravada. Confira a autenticação no próximo logon ou reinício manual.'
                    } else { 'Configuração cancelada.' }
                } finally { $chave.Dispose() }
                $campos.Add(@{ nome = 'Resultado'; valor = $mensagem })
            }
            { $_ -in 'sfcrepair','dismrestore','chkdskfull' } {
                $nativo = switch ($Id) {
                    'sfcrepair' { Invoke-VerificacaoNativa 'sfc.exe' '/scannow' 0 }
                    'dismrestore' { Invoke-VerificacaoNativa 'dism.exe' '/Online /Cleanup-Image /RestoreHealth /NoRestart' 0 }
                    'chkdskfull' {
                        if ($Volume -cnotmatch '^[A-Z]:$' -or ([IO.DriveInfo]::new($Volume)).DriveType -ne [IO.DriveType]::Fixed) {
                            throw 'Selecione a letra de um volume fixo local.'
                        }
                        $campos.Add(@{ nome = 'Volume'; valor = $Volume })
                        Invoke-VerificacaoNativa 'cmd.exe' ('/d /c start "CHKDSK" /wait "%SystemRoot%\System32\chkdsk.exe" ' + $Volume + ' /f /r') 0
                    }
                }
                $campos.Add(@{ nome = 'Código de saída'; valor = [string]$nativo.Codigo })
                foreach ($linha in ($nativo.Texto -split '[\r\n]+' | Where-Object { $_.Trim() } | Select-Object -Last 250)) {
                    $campos.Add(@{ nome = 'Saída do Windows'; valor = $linha.Trim().Substring(0, [Math]::Min(1500, $linha.Trim().Length)) })
                }
                if ($Id -eq 'chkdskfull') {
                    $campos.Add(@{ nome = 'Registro do Windows'; valor = 'Veja o log em Aplicativo, fontes Chkdsk e Wininit. Se aceitou agendar na janela do CHKDSK, reinicie manualmente.' })
                }
                if ($nativo.Interrompida) {
                    $campos.Add(@{ nome = 'Execução'; valor = 'Reparo SFC interrompido pelo usuário; processo encerrado.' })
                    $aviso = 'Reparo interrompido e incompleto. Alterações já efetuadas não foram desfeitas. Não é possível concluir o estado de integridade do Windows; revise o CBS.log e execute uma nova verificação.'
                } elseif ($nativo.Codigo -ne 0) { $aviso = 'Confira o código de saída e o registro do Windows para determinar o resultado do reparo.' }
            }
            default { throw 'Intervenção não autorizada.' }
        }
    } catch { $aviso = $_.Exception.Message }
    finally {
        if ($bloqueado) { $mutex.ReleaseMutex() }
        if ($mutex) { $mutex.Dispose() }
    }
    @{ nome = 'Intervenção de suporte'; campos = $campos.ToArray(); aviso = $aviso }
}
