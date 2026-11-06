#requires -Version 5.1
param([string]$Dialogo, [string]$Modulo = (Join-Path $PSScriptRoot '../Modulos/FluxosSensiveis.ps1'))
$ErrorActionPreference = 'Stop'

if ($Dialogo) {
    . $Modulo
    $script:criarFormulario = ${function:New-FormularioElevado}
    $script:controlesValidos = $false
    function New-FormularioElevado {
        param([string]$Titulo, [int]$Altura)
        $formulario = & $script:criarFormulario $Titulo $Altura
        $formulario.Add_Shown({
            param($janela, $evento)
            if ($script:Dialogo -eq 'Perfil') {
                $revisar = $janela.Controls | Where-Object Text -eq '&Revisar exclusão…'
                $detalhes = $janela.Controls | Where-Object AccessibleName -eq 'Pastas, Registro e bloqueios da seleção'
                $script:controlesValidos = $revisar -and -not $revisar.Enabled -and $detalhes.Text -match 'Perfil carregado'
            } else {
                $senha = $janela.Controls | Where-Object AccessibleName -eq 'Senha'
                $script:controlesValidos = $senha -and $senha.UseSystemPasswordChar -and -not $senha.ShortcutsEnabled
            }
        })
        $formulario
    }
    function Assert-SuporteLogonConfiguravel { }
    function Enable-SuporteLogon { throw 'O teste cancelado chamou a alteração.' }
    if ($Dialogo -eq 'Perfil') {
        $plano = [pscustomobject]@{
            Conta='Conta simulada'; SID='S-1-5-21-10-20-30-1001'; Pastas=@('X:\Users\Teste')
            Chaves=@(); Incertas=@(); Bloqueios=@('Perfil carregado.')
        }
        if (Select-PerfilElevado @($plano)) { throw 'O teste cancelado escolheu um perfil.' }
    } elseif (Set-LogonGrafico $null) { throw 'O teste cancelado configurou o login.' }
    if (-not $script:controlesValidos) { throw "Os controles do diálogo $Dialogo não ficaram no estado esperado." }
    exit 0
}

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$powershell = Join-Path ([Environment]::SystemDirectory) 'WindowsPowerShell/v1.0/powershell.exe'
$moduloCompleto = (Resolve-Path -LiteralPath $Modulo).Path
function Find-Elemento($Raiz, [string]$Nome) {
    $criterio = [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, $Nome)
    $Raiz.FindFirst([Windows.Automation.TreeScope]::Descendants, $criterio)
}
foreach ($modo in 'Perfil','Logon') {
    $argumentos = '-NoProfile -NonInteractive -STA -ExecutionPolicy RemoteSigned -File "{0}" -Dialogo {1} -Modulo "{2}"' -f $PSCommandPath,$modo,$moduloCompleto
    $inicio = [Diagnostics.ProcessStartInfo]::new($powershell, $argumentos)
    $inicio.UseShellExecute = $false
    $inicio.CreateNoWindow = $true
    $inicio.RedirectStandardError = $true
    $processo = [Diagnostics.Process]::Start($inicio)
    try {
        $relogio = [Diagnostics.Stopwatch]::StartNew()
        $janela = $null
        $titulo = if ($modo -eq 'Perfil') { 'Revisar perfis locais' } else { 'Configurar login automático · administrador' }
        $criterio = [Windows.Automation.AndCondition]::new(
            [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ProcessIdProperty, $processo.Id),
            [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, $titulo))
        $cancelar = $null
        do {
            if ($processo.HasExited) { throw "O diálogo $modo encerrou com código $($processo.ExitCode). $($processo.StandardError.ReadToEnd())" }
            $janela = [Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Children, $criterio)
            if ($janela) { $cancelar = Find-Elemento $janela 'Cancelar' }
            if (-not $cancelar) { Start-Sleep -Milliseconds 150 }
        } until ($cancelar -or $relogio.Elapsed.TotalSeconds -ge 15)
        if (-not $cancelar) { throw "O diálogo $modo não ficou disponível para interação." }
        $processo.Refresh()
        if (-not $processo.CloseMainWindow()) { throw "Não foi possível fechar o diálogo $modo." }
        if (-not $processo.WaitForExit(5000)) { throw "O cancelamento de $modo não encerrou a janela." }
        if ($processo.ExitCode -ne 0) { throw "O cancelamento de $modo falhou. $($processo.StandardError.ReadToEnd())" }
        "OK  Diálogo simulado de $modo cancelado."
    } finally {
        if (-not $processo.HasExited) { $processo.Kill(); $processo.WaitForExit() }
        $processo.Dispose()
    }
}
