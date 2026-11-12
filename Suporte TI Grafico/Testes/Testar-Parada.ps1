#requires -Version 5.1
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0
# Exercita a função de produção com processos inofensivos, sem SFC, UAC ou reparos.
$fonte = Join-Path (Split-Path $PSScriptRoot -Parent) 'Modulos/Administrativo.ps1'
$tokens = $null; $erros = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($fonte, [ref]$tokens, [ref]$erros)
if ($erros.Count) { throw 'Script administrativo com erro de sintaxe.' }
$funcao = $ast.Find({ param($no) $no -is [Management.Automation.Language.FunctionDefinitionAst] -and $no.Name -eq 'Invoke-VerificacaoNativa' }, $true)
. ([scriptblock]::Create($funcao.Extent.Text))
$Operacao = 'sfcverify'
$SinalParada = [byte[]]@(1)
$PedidoParada = [Threading.Tasks.Task]::FromResult([int]1)
$relogio = [Diagnostics.Stopwatch]::StartNew()
$r = Invoke-VerificacaoNativa 'WindowsPowerShell/v1.0/powershell.exe' '-NoProfile -Command "[Console]::WriteLine($PID); Start-Sleep -Seconds 20"' 30
if (-not $r.Interrompida -or $relogio.Elapsed.TotalSeconds -gt 10) { throw "A parada não encerrou o processo a tempo: $($r | ConvertTo-Json -Compress)" }
$pidTeste = 0
if ([int]::TryParse($r.Texto.Trim(), [ref]$pidTeste)) {
    try { $restante = [Diagnostics.Process]::GetProcessById($pidTeste) } catch [ArgumentException] { $restante = $null }
    if ($restante) { $restante.Dispose(); throw 'Processo de teste ficou em execução.' }
}
'OK  Pedido de parada encerra o próprio processo e aguarda sua saída'
$Operacao = 'sfcrepair'
$r = Invoke-VerificacaoNativa 'WindowsPowerShell/v1.0/powershell.exe' '-NoProfile -Command "Start-Sleep -Seconds 20"' 30
if (-not $r.Interrompida) { throw 'Modo reparo SFC não aceita a parada confirmada.' }
'OK  Modo reparo SFC aceita o mesmo pedido explícito de parada'
$Operacao = 'sfcverify'
$PedidoParada = $null
$r = Invoke-VerificacaoNativa 'cmd.exe' '/d /c echo concluido' 5
if ($r.Interrompida -or $r.Codigo -ne 0 -or $r.Texto.Trim() -ne 'concluido') { throw 'Conclusão natural alterada.' }
'OK  Conclusão natural preservada sem pedido'
$PedidoParada = [Threading.Tasks.Task]::FromResult([int]1)
$SinalParada = [byte[]]@(9)
$r = Invoke-VerificacaoNativa 'cmd.exe' '/d /c echo concluido' 5
if ($r.Interrompida -or $r.Codigo -ne 0) { throw 'Sinal inválido aceito.' }
'OK  Byte fora do protocolo não solicita parada'
$SinalParada = [byte[]]@(1)
$Operacao = 'dismrestore'
$r = Invoke-VerificacaoNativa 'cmd.exe' '/d /c echo preservado' 5
if ($r.Interrompida -or $r.Texto.Trim() -ne 'preservado') { throw 'Parada vazou para reparo.' }
'OK  Pedido de parada não alcança operações fora da lista SFC'
'5 testes de parada aprovados.'
