param([Parameter(Mandatory)][string]$Zip, [Parameter(Mandatory)][string]$Saida)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.IO.Compression.FileSystem
$temp=Join-Path ([IO.Path]::GetTempPath()) ('SuporteTI-Medicao-'+[guid]::NewGuid())
$amostras=@()
try {
    [IO.Compression.ZipFile]::ExtractToDirectory((Resolve-Path $Zip).Path,$temp)
    $exe=Join-Path $temp 'Suporte TI Grafico.exe'
    for ($r=1; $r -le 3; $r++) {
        foreach ($modo in 'extracao-vazia','extracao-reutilizada') {
            $inicio=[Diagnostics.ProcessStartInfo]::new($exe)
            $inicio.UseShellExecute=$false
            $inicio.WorkingDirectory=$temp
            $inicio.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR']=Join-Path $temp ('cache-'+$r)
            $tempo=[Diagnostics.Stopwatch]::StartNew()
            $app=[Diagnostics.Process]::Start($inicio)
            try {
                do {
                    $app.Refresh()
                    if ($app.HasExited) { throw 'Aplicação encerrou durante a medição.' }
                    if ($tempo.Elapsed.TotalSeconds -gt 30) { throw 'Limite de abertura excedido.' }
                    Start-Sleep -Milliseconds 20
                } while ($app.MainWindowHandle -eq [IntPtr]::Zero)
                $janela=[Windows.Automation.AutomationElement]::FromHandle($app.MainWindowHandle)
                $cond=[Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty,'Escolher diagnóstico')
                while (-not $janela.FindFirst([Windows.Automation.TreeScope]::Descendants,$cond)) {
                    if ($tempo.Elapsed.TotalSeconds -gt 30) { throw 'Interface não ficou pronta.' }
                    Start-Sleep -Milliseconds 20
                }
                $amostras+=@{repeticao=$r;cenario=$modo;milissegundos=$tempo.Elapsed.TotalMilliseconds}
            } finally {
                if (-not $app.HasExited) {
                    [void]$app.CloseMainWindow()
                    if (-not $app.WaitForExit(5000)) {$app.Kill();$app.WaitForExit()}
                }
                $app.Dispose()
            }
        }
        $tempo=[Diagnostics.Stopwatch]::StartNew()
        $ps=Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe'
        $dados=& $ps -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned -File (Join-Path $temp 'Modulos/Consultar.ps1') -Operacao sistema
        if ($LASTEXITCODE -ne 0) {throw 'Consulta de medição falhou.'}
        $resposta=($dados -join [Environment]::NewLine) | ConvertFrom-Json
        if ($resposta.operacao -ne 'sistema' -or $resposta.estado -notin 'concluido','parcial') {throw 'Consulta sem evidência utilizável.'}
        $amostras+=@{repeticao=$r;cenario='consulta-sistema';milissegundos=$tempo.Elapsed.TotalMilliseconds;estado=$resposta.estado}
    }
    $json=@{pacote=[IO.Path]::GetFileName($Zip);amostras=$amostras} | ConvertTo-Json -Depth 5
    [IO.File]::WriteAllText([IO.Path]::GetFullPath($Saida),$json,[Text.UTF8Encoding]::new($false))
} finally {
    # Somente a pasta temporária criada pelo próprio medidor.
    $raizTemp=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')+'\'
    $alvo=[IO.Path]::GetFullPath($temp)
    if (-not $alvo.StartsWith($raizTemp,[StringComparison]::OrdinalIgnoreCase) -or
        -not [IO.Path]::GetFileName($alvo).StartsWith('SuporteTI-Medicao-')) {throw 'Destino de limpeza inesperado.'}
    if ([IO.Directory]::Exists($alvo)) {[IO.Directory]::Delete($alvo,$true)}
}
