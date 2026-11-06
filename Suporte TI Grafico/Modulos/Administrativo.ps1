#requires -Version 5.1
param(
    [Parameter(Mandatory)][ValidateSet('diskhealth','gpcomputador','gpcomputerhtml','sfcverify','dismcheck','dismscan','chkdskread',
        'spooler','spoolerclear','cache','gpupdatecomputer','netdhcp','netstatic','sfcrepair','dismrestore','chkdskfull',
        'perfillocal','autologonenable','autologondisable')]
    [string]$Operacao,
    [string]$Volume,
    $Dados
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0
$identidade = [Security.Principal.WindowsIdentity]::GetCurrent()
try {
    if (-not ([Security.Principal.WindowsPrincipal]::new($identidade)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Esta consulta requer elevação pelo Windows.'
    }
} finally { $identidade.Dispose() }

function Invoke-VerificacaoNativa {
    param([string]$Programa, [string]$Argumentos, [int]$Segundos = 1800)
    $inicio = [Diagnostics.ProcessStartInfo]::new()
    $inicio.FileName = Join-Path ([Environment]::SystemDirectory) $Programa
    $inicio.WorkingDirectory = $PSScriptRoot
    $inicio.Arguments = $Argumentos
    $inicio.UseShellExecute = $false
    $inicio.CreateNoWindow = $true
    $inicio.RedirectStandardInput = $true
    $inicio.RedirectStandardOutput = $true
    $inicio.RedirectStandardError = $true
    $oem = [Text.Encoding]::GetEncoding([Globalization.CultureInfo]::CurrentCulture.TextInfo.OEMCodePage)
    $inicio.StandardOutputEncoding = if ($Programa -eq 'sfc.exe') { [Text.Encoding]::Unicode } else { $oem }
    $inicio.StandardErrorEncoding = $oem
    $processo = [Diagnostics.Process]::Start($inicio)
    try {
        $processo.StandardInput.Close()
        $saida = $processo.StandardOutput.ReadToEndAsync()
        $erros = $processo.StandardError.ReadToEndAsync()
        if ($Segundos -eq 0) {
            $processo.WaitForExit()
        } elseif (-not $processo.WaitForExit($Segundos * 1000)) {
            try { $processo.Kill() } catch [InvalidOperationException] { if (-not $processo.HasExited) { throw } }
            throw 'O comando nativo excedeu o tempo limite.'
        }
        [pscustomobject]@{ Codigo = $processo.ExitCode; Texto = ($saida.Result + [Environment]::NewLine + $erros.Result).Trim() }
    } finally { $processo.Dispose() }
}

$html = $null
if ($Operacao -in 'spooler','spoolerclear','cache','gpupdatecomputer','netdhcp','netstatic','sfcrepair','dismrestore','chkdskfull',
    'perfillocal','autologonenable','autologondisable') {
    . (Join-Path $PSScriptRoot 'Intervencoes.ps1')
    $secoes = @(Invoke-IntervencaoElevada $Operacao $Dados $Volume)
} elseif ($Operacao -eq 'diskhealth') {
    . (Join-Path $PSScriptRoot 'Diagnosticos.ps1')
    $secoes = @(Get-Diagnostico 'discos')
} else {
    $campos = @()
    $aviso = $null
    try {
        $nativo = switch ($Operacao) {
            'gpcomputador' { Invoke-VerificacaoNativa 'gpresult.exe' '/SCOPE COMPUTER /R' 30 }
            'gpcomputerhtml' { Invoke-VerificacaoNativa 'gpresult.exe' '/SCOPE COMPUTER /H gpresult-computer.html' 30 }
            'sfcverify' { Invoke-VerificacaoNativa 'sfc.exe' '/verifyonly' }
            'dismcheck' { Invoke-VerificacaoNativa 'dism.exe' '/Online /Cleanup-Image /CheckHealth /NoRestart' }
            'dismscan' { Invoke-VerificacaoNativa 'dism.exe' '/Online /Cleanup-Image /ScanHealth /NoRestart' }
            'chkdskread' {
                if ($Volume -cnotmatch '^[A-Z]:$' -or ([IO.DriveInfo]::new($Volume)).DriveType -ne [IO.DriveType]::Fixed) {
                    throw 'Selecione a letra de um volume fixo local.'
                }
                Invoke-VerificacaoNativa 'chkdsk.exe' $Volume
            }
        }
        $campos += @{ nome = 'Código de saída'; valor = [string]$nativo.Codigo }
        $linhas = @($nativo.Texto -split '[\r\n]+' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
        foreach ($linha in ($linhas | Select-Object -Last 250)) {
            $campos += @{ nome = 'Saída do Windows'; valor = $linha.Substring(0, [Math]::Min(1500, $linha.Length)) }
        }
        if ($nativo.Codigo -ne 0) {
            $aviso = "O comando retornou $($nativo.Codigo). Confira a saída do Windows."
        } elseif ($Operacao -eq 'gpcomputerhtml') {
            $relatorio = Get-Item -LiteralPath (Join-Path $PSScriptRoot 'gpresult-computer.html')
            if ($relatorio.Length -le 0 -or $relatorio.Length -gt 8MB) { throw 'O relatório HTML está vazio ou supera 8 MiB.' }
            $html = [Convert]::ToBase64String([IO.File]::ReadAllBytes($relatorio.FullName))
        }
        if ($linhas.Count -gt 250) {
            $aviso = ($aviso, 'A janela mostra as últimas 250 linhas da saída.' -ne $null) -join ' '
        }
    } catch { $aviso = $_.Exception.Message }
    $secoes = @(@{ nome = 'Consulta administrativa'; campos = $campos; aviso = $aviso })
}
$estado = if (-not ($secoes | Where-Object { $_.campos.Count -gt 0 })) { 'indisponivel' }
    elseif ($secoes | Where-Object aviso) { 'parcial' }
    else { 'concluido' }
@{ resultado = @{ versao = 1; operacao = $Operacao; estado = $estado; secoes = $secoes }; html = $html }
