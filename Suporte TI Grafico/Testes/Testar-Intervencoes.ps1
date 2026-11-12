#requires -Version 5.1
$ErrorActionPreference = 'Stop'
$projeto = Split-Path $PSScriptRoot -Parent
$repo = Split-Path $projeto -Parent
$temp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$raiz = Join-Path $temp ('SuporteTI-TesteIntervencoes-' + [guid]::NewGuid().ToString('N'))
$coletores = Join-Path $raiz 'Coletores'
$systemRootOriginal = $env:SystemRoot
$total = 0
function Confirmar([bool]$Condicao, [string]$Nome) {
    if (-not $Condicao) { throw "Falhou: $Nome" }
    $script:total++
    "OK  $Nome"
}

try {
    [void][IO.Directory]::CreateDirectory($coletores)
    Copy-Item -LiteralPath (Join-Path $projeto 'Modulos/Intervencoes.ps1') -Destination $raiz
    foreach ($arquivo in 'Rede.ps1','Operacoes.ps1','Manutencao.ps1') {
        Copy-Item -LiteralPath (Join-Path $repo "Modulos/$arquivo") -Destination $coletores
    }
    . (Join-Path $raiz 'Intervencoes.ps1')
    $script:estados = @{ Spooler='Running'; bits='Running'; wuauserv='Stopped' }
    $script:reinicios = 0
    $script:tentativas = @()
    function Get-Service {
        param([string]$Name)
        $objeto = [pscustomobject]@{ Status=$script:estados[$Name] }
        $objeto | Add-Member ScriptMethod WaitForStatus {
            param($Status,$Prazo)
            if ($this.Status -ne [string]$Status) { throw 'Estado inesperado.' }
        }
        $objeto
    }
    function Restart-Service { param($Name); $script:reinicios++ }
    function Stop-Service { param($Name); $script:estados[$Name] = 'Stopped' }
    function Start-Service {
        param($Name)
        if ($script:falharRestauracao -and $Name -eq 'bits') { throw 'Restauracao simulada bloqueada.' }
        $script:estados[$Name] = 'Running'
    }
    function Get-Printer {
        [pscustomobject]@{ Name='Fila A'; Type='Local' }
        [pscustomobject]@{ Name='Fila B'; Type='Local' }
        [pscustomobject]@{ Name='Fila externa'; Type='Connection' }
    }
    function Get-PrintJob {
        param($PrinterObject)
        if ($PrinterObject.Name -ne 'Fila A') { throw 'Alvo inesperado.' }
        [pscustomobject]@{ Id=1 }
        [pscustomobject]@{ Id=2 }
    }
    function Remove-PrintJob {
        param($InputObject, $Confirm)
        $script:tentativas += $InputObject.Id
        if ($InputObject.Id -eq 2) { throw 'Cancelamento simulado recusado.' }
    }

    $script:falharRestauracao = $false
    $spooler = Invoke-IntervencaoElevada 'spooler'
    Confirmar ($script:reinicios -eq 1 -and -not $script:tentativas.Count -and -not $spooler.aviso) 'Reiniciar spooler preserva os trabalhos'
    $fila = Invoke-IntervencaoElevada 'spoolerclear' @{ Alvo='Fila A' }
    Confirmar (($script:tentativas -join ',') -eq '1,2' -and $fila.aviso -and $fila.campos[1].valor -match '1 cancelados') 'Limpeza restrita à fila e falha parcial registrada'
    $externa = Invoke-IntervencaoElevada 'spoolerclear' @{ Alvo='Fila externa' }
    Confirmar ($externa.aviso -and -not $externa.campos.Count) 'Fila de conexão recusada'

    $script:guid = [guid]::NewGuid()
    $script:dhcp = 'Disabled'
    $script:comandos = @()
    $script:falharDns = $false
    function Get-NetAdapter { param([switch]$Physical,$InterfaceIndex); [pscustomobject]@{ Name='Ethernet'; ifIndex=7; InterfaceGuid=$script:guid } }
    function Get-NetIPInterface { param($InterfaceIndex,$AddressFamily); [pscustomobject]@{ Dhcp=$script:dhcp } }
    function Get-NetIPAddress { param($InterfaceIndex,$AddressFamily); [pscustomobject]@{ IPAddress='192.0.2.10'; PrefixLength=24 } }
    function Get-NetRoute { param($InterfaceIndex,$AddressFamily,$DestinationPrefix); [pscustomobject]@{ NextHop='192.0.2.1' } }
    function Get-DnsClientServerAddress { param($InterfaceIndex,$AddressFamily); [pscustomobject]@{ ServerAddresses=@('192.0.2.53') } }
    $script:pararSfc = $false
    function Invoke-VerificacaoNativa {
        param($Programa,$Argumentos,$Segundos)
        $script:comandos += $Argumentos
        if ($Argumentos -like '*set address*source=dhcp') { $script:dhcp='Enabled' }
        if ($script:falharDns -and $Argumentos -like '*dnsservers*') { return [pscustomobject]@{ Codigo=1; Texto='Falha DNS simulada' } }
        [pscustomobject]@{ Codigo=0; Texto='Simulado'; Interrompida=$script:pararSfc }
    }
    $dhcp = Invoke-IntervencaoElevada 'netdhcp' @{ Alvo=$script:guid }
    Confirmar ($script:comandos.Count -eq 2 -and $dhcp.campos[1].valor -match 'Disabled' -and $dhcp.campos[-1].valor -match 'Enabled') 'DHCP direcionado ao adaptador com estado antes e depois'
    $script:comandos = @()
    $fixo = @{ Alvo=$script:guid; IPv4='192.0.2.10'; Prefixo='24'; Gateway='192.0.2.1'; Dns1='192.0.2.53'; Dns2='192.0.2.53' }
    $invalido = Invoke-IntervencaoElevada 'netstatic' $fixo
    Confirmar ($invalido.aviso -and -not $script:comandos.Count) 'DNS duplicado bloqueado antes de alterar o endereço'
    $fixo.Dns2 = '192.0.2.54'
    $script:falharDns = $true
    $parcial = Invoke-IntervencaoElevada 'netstatic' $fixo
    Confirmar ($script:comandos.Count -eq 2 -and $parcial.aviso -match 'Falha DNS' -and $parcial.campos[-1].nome -eq 'Depois') 'Falha no DNS preserva diagnóstico após IPv4'

    $script:comandos = @()
    $sfc = Invoke-IntervencaoElevada 'sfcrepair'
    Confirmar ($script:comandos[0] -ceq '/scannow' -and -not $sfc.aviso) 'SFC executa apenas o reparo fixo'
    $dism = Invoke-IntervencaoElevada 'dismrestore'
    Confirmar ($script:comandos[1] -ceq '/Online /Cleanup-Image /RestoreHealth /NoRestart' -and -not $dism.aviso) 'DISM executa RestoreHealth sem reinício automático'
    $invalido = Invoke-IntervencaoElevada 'chkdskfull' $null 'C: & whoami'
    Confirmar ($invalido.aviso -and $script:comandos.Count -eq 2) 'CHKDSK rejeita parâmetros livres no processo elevado'
    $volume = [IO.Path]::GetPathRoot($raiz).TrimEnd('\')
    $chkdsk = Invoke-IntervencaoElevada 'chkdskfull' $null $volume
    Confirmar ($script:comandos[2] -like "*/wait*$volume /f /r" -and $chkdsk.campos[-1].valor -match 'reinicie manualmente') 'CHKDSK usa volume fixo e preserva o agendamento interativo'
    $script:pararSfc = $true
    $sfcParado = Invoke-IntervencaoElevada 'sfcrepair'
    Confirmar ($sfcParado.aviso -match 'incompleto' -and $sfcParado.campos[-1].valor -match 'processo encerrado') 'Reparo interrompido registra resultado incompleto sem anunciar sucesso'
    $script:pararSfc = $false

    $env:SystemRoot = Join-Path $raiz 'WindowsSimulado'
    $download = Join-Path $env:SystemRoot 'SoftwareDistribution/Download'
    [void][IO.Directory]::CreateDirectory($download)
    function Get-Process { param($Name) }
    $script:reinicioPendente = $true
    function Test-Path {
        param($LiteralPath,$PathType)
        if ($LiteralPath -like 'HKLM:*') { return $script:reinicioPendente }
        Microsoft.PowerShell.Management\Test-Path @PSBoundParameters
    }
    $pendente = Invoke-IntervencaoElevada 'cache'
    Confirmar ($pendente.aviso -and [IO.Directory]::Exists($download)) 'Reinício pendente bloqueia mudança no cache'
    $script:reinicioPendente = $false
    $script:falharRestauracao = $true
    $cache = Invoke-IntervencaoElevada 'cache'
    Confirmar ($cache.aviso -match 'restaurar' -and $cache.campos.Count -gt 0 -and -not [IO.Directory]::Exists($download)) 'Cache preservado e falha de restauração relatada'

    . (Join-Path $coletores 'Manutencao.ps1')
    function Test-SuporteAdmin { $false }
    $chrome = Join-Path $raiz 'ChromeSimulado'
    $cacheChrome = Join-Path $chrome 'Default/Cache'
    [void][IO.Directory]::CreateDirectory($cacheChrome)
    [IO.File]::WriteAllText((Join-Path $cacheChrome 'dados.tmp'),'cache')
    [IO.File]::WriteAllText((Join-Path $chrome 'Default/Preferences'),'preferencias')
    $null = Invoke-SuporteCacheChrome -DadosChrome $chrome
    Confirmar (([IO.Directory]::GetFiles($cacheChrome)).Length -eq 0 -and [IO.File]::Exists((Join-Path $chrome 'Default/Preferences'))) 'Chrome preserva preferências fora da pasta Cache'
    "$total testes de intervenção aprovados."
} finally {
    $env:SystemRoot = $systemRootOriginal
    $caminho = [IO.Path]::GetFullPath($raiz)
    if ($caminho.StartsWith($temp, [StringComparison]::OrdinalIgnoreCase) -and [IO.Directory]::Exists($caminho)) {
        [IO.Directory]::Delete($caminho, $true)
    }
}
& (Join-Path $PSScriptRoot 'Testar-Parada.ps1')
