# Funcoes de leitura reaproveitadas de Modulos/Recursos.ps1 da edicao de terminal 0.1.22.
function Get-SuporteCpuPercentual {
    $loads = @(Get-CimInstance Win32_Processor -OperationTimeoutSec 3 -ErrorAction Stop |
        Where-Object { $null -ne $_.LoadPercentage } | Select-Object -ExpandProperty LoadPercentage)
    if ($loads.Count -eq 0) { throw 'Carga de CPU indisponivel.' }
    [int][Math]::Round(($loads | Measure-Object -Average).Average)
}

function Get-SuporteMemoria {
    $os = Get-CimInstance Win32_OperatingSystem -OperationTimeoutSec 3 -ErrorAction Stop
    $total = [double]$os.TotalVisibleMemorySize
    $livre = [double]$os.FreePhysicalMemory
    if ($total -le 0 -or $livre -lt 0 -or $livre -gt $total) { throw 'Dados de memoria indisponiveis.' }
    $usado = $total - $livre
    [pscustomobject]@{
        Percentual = [int][Math]::Round(100 * $usado / $total)
        Resumo = 'RAM: {0:N1} GB usados de {1:N1} GB ({2:N0}%) | Livre: {3:N1} GB' -f ($usado/1MB),($total/1MB),(100*$usado/$total),($livre/1MB)
    }
}
