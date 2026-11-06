# Diagnósticos locais e exportações no contexto do usuário atual.
function Invoke-Gpresult {
    param([string]$Relatorio)
    $inicio = New-Object Diagnostics.ProcessStartInfo
    $inicio.FileName = Join-Path ([Environment]::SystemDirectory) 'gpresult.exe'
    $inicio.Arguments = '/SCOPE USER /R'
    if ($Relatorio) {
        # O gpresult limita o argumento de /H a 127 caracteres.
        $inicio.WorkingDirectory = [IO.Path]::GetDirectoryName($Relatorio)
        $inicio.Arguments = '/SCOPE USER /H "' + [IO.Path]::GetFileName($Relatorio) + '"'
    }
    $inicio.UseShellExecute = $false
    $inicio.CreateNoWindow = $true
    $inicio.RedirectStandardOutput = $true
    $inicio.RedirectStandardError = $true
    $oem = [Text.Encoding]::GetEncoding([Globalization.CultureInfo]::CurrentCulture.TextInfo.OEMCodePage)
    $inicio.StandardOutputEncoding = $oem
    $inicio.StandardErrorEncoding = $oem
    $processo = [Diagnostics.Process]::Start($inicio)
    try {
        $saida = $processo.StandardOutput.ReadToEndAsync()
        $erros = $processo.StandardError.ReadToEndAsync()
        if (-not $processo.WaitForExit(30000)) {
            $processo.Kill()
            throw 'O gpresult excedeu o limite de 30 segundos.'
        }
        if ($processo.ExitCode -ne 0) {
            throw ('gpresult retornou {0}. {1} {2}' -f $processo.ExitCode, $saida.Result.Trim(), $erros.Result.Trim()).Trim()
        }
        $saida.Result -split '\r?\n'
    } finally { $processo.Dispose() }
}

function Get-EventosAtendimento {
    param([string]$Canal, [switch]$SomenteAlertas, [switch]$CodigosUpdate)
    $filtro = @{ LogName = $Canal; StartTime = (Get-Date).AddHours(-24) }
    if ($SomenteAlertas) { $filtro.Level = 1,2,3 }
    try { $eventos = @(Get-WinEvent -FilterHashtable $filtro -MaxEvents 30 -ErrorAction Stop) }
    catch {
        if ($_.FullyQualifiedErrorId -like 'NoMatchingEventsFound*') { return }
        throw
    }
    foreach ($evento in $eventos) {
        $mensagem = ([string]$evento.Message -replace '\s+', ' ').Trim()
        if ($mensagem.Length -gt 700) { $mensagem = $mensagem.Substring(0, 700) + '…' }
        if (-not $mensagem) { $mensagem = 'Mensagem indisponível.' }
        @{ nome = ('{0:dd/MM HH:mm:ss} · evento {1}' -f $evento.TimeCreated, $evento.Id); valor = "$($evento.ProviderName) · $mensagem" }
        if ($CodigosUpdate) {
            $dados = Get-SuporteDadosEvento $evento
            $bruto = ([string]$dados['errorCode']).Trim()
            $codigo = 0L
            if ($bruto -match '^0x([0-9a-f]{1,8})$') { $codigo = [Convert]::ToUInt32($Matches[1], 16) }
            elseif ([long]::TryParse($bruto, [ref]$codigo) -and $codigo -ge [int]::MinValue -and $codigo -lt 0) { $codigo += 4294967296L }
            if ($codigo -ge 2147483648L -and $codigo -le [uint32]::MaxValue) {
                @{ nome = 'Código · HRESULT'; valor = ('0x{0:X8}' -f $codigo); familia = 'HRESULT' }
            }
        }
    }
}

function Get-FilasAtendimento {
    $spooler = Get-Service -Name Spooler -ErrorAction Stop
    @{ nome = 'Spooler'; valor = $(if ($spooler.Status -eq 'Running') { 'Em execução' } else { "Serviço $($spooler.Status). Filas não consultadas." }) }
    if ($spooler.Status -ne 'Running') { return }
    $filas = @(Get-Printer -ErrorAction Stop | Sort-Object Name)
    if (-not $filas.Count) { @{ nome = 'Filas'; valor = 'Nenhuma impressora visível nesta sessão.' } }
    foreach ($fila in $filas) {
        $situacao = [string]$fila.PrinterStatus
        try {
            $trabalhos = @(Get-PrintJob -PrinterObject $fila -ErrorAction Stop)
            $problemas = @($trabalhos | Where-Object { [string]$_.JobStatus -match 'Error|Offline|PaperOut|Blocked|UserIntervention' }).Count
            $pausados = @($trabalhos | Where-Object { [string]$_.JobStatus -match 'Paused' }).Count
            @{ nome = $fila.Name; valor = "$situacao · porta $($fila.PortName) · $($trabalhos.Count) trabalhos, $problemas com problema, $pausados pausados." }
        } catch {
            @{ nome = $fila.Name; valor = "$situacao · porta $($fila.PortName) · Trabalhos indisponíveis." }
        }
    }
}

function Exportar-Atendimento {
    param([ValidateSet('gphtml','evtx')][string]$Id, [string]$Destino)
    if ($Destino -notmatch '^(?:[A-Za-z]:[\\/]|\\\\[^\\/]+[\\/][^\\/]+)') { throw 'Escolha uma pasta com caminho absoluto.' }
    $pasta = Join-Path (Join-Path ([IO.Path]::GetFullPath($Destino)) $env:COMPUTERNAME) ((Get-Date -Format 'yyyy-MM-dd_HHmmss') + '_' + [guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory($pasta)
    $campos = @(@{ nome = 'Pasta da exportação'; valor = $pasta })
    $avisos = @()
    if ($Id -eq 'gphtml') {
        $arquivo = Join-Path $pasta 'gpresult-user.html'
        try {
            Invoke-Gpresult $arquivo | Out-Null
            if (-not [IO.File]::Exists($arquivo) -or (Get-Item -LiteralPath $arquivo).Length -eq 0) { throw 'O Windows não gerou o HTML.' }
            $campos += @{ nome = 'HTML do usuário atual'; valor = $arquivo }
        } catch { $avisos += $_.Exception.Message }
    } else {
        $arquivos = @(foreach ($canal in 'Application','System','Setup') {
            $arquivo = Join-Path $pasta ($canal + '.evtx')
            $temporario = Join-Path $pasta ($canal + '.parcial.evtx')
            try {
                $consulta = '*[System[TimeCreated[timediff(@SystemTime) <= 86400000]]]'
                [Diagnostics.Eventing.Reader.EventLogSession]::GlobalSession.ExportLog(
                    $canal, [Diagnostics.Eventing.Reader.PathType]::LogName, $consulta, $temporario)
                $leitor = [Diagnostics.Eventing.Reader.EventLogReader]::new($temporario, [Diagnostics.Eventing.Reader.PathType]::FilePath)
                try { $evento = $leitor.ReadEvent(); if ($evento) { $evento.Dispose() } }
                finally { $leitor.Dispose() }
                $hash = (Get-FileHash -LiteralPath $temporario -Algorithm SHA256).Hash
                [IO.File]::Move($temporario, $arquivo)
                $campos += @{ nome = $canal; valor = $arquivo }
                @{ Arquivo = [IO.Path]::GetFileName($arquivo); Bytes = (Get-Item -LiteralPath $arquivo).Length; SHA256 = $hash }
            } catch {
                $avisos += "$canal · $($_.Exception.Message)"
                if ([IO.File]::Exists($temporario)) { Remove-Item -LiteralPath $temporario -ErrorAction SilentlyContinue }
            }
        })
        if ($arquivos.Count) {
            try {
                $manifesto = Join-Path $pasta 'manifesto.json'
                @{
                    Computador = $env:COMPUTERNAME; Data = (Get-Date).ToString('o')
                    Periodo = 'Últimas 24 horas'; Arquivos = $arquivos
                } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifesto -Encoding UTF8
                $campos += @{ nome = 'Manifesto SHA-256'; valor = $manifesto }
            } catch { $avisos += "Manifesto · $($_.Exception.Message)" }
        }
    }
    $aviso = $null
    if ($avisos.Count) { $aviso = [string]::Join([Environment]::NewLine, [string[]]$avisos) }
    if ($campos.Count -eq 1 -and $aviso) {
        return @{ nome = 'Exportação'; campos = @(); aviso = "$aviso`nPasta: $pasta" }
    }
    @{ nome = 'Exportação'; campos = $campos; aviso = $aviso }
}

function Get-Atendimento {
    param([string]$Id, [string]$Destino)
    switch ($Id) {
        'update' {
            Get-Lista 'Serviços de atualização' {
                Get-Service -Name wuauserv,bits,cryptsvc | ForEach-Object { @{ nome = $_.Name; valor = "$($_.DisplayName) · $($_.Status) · $($_.StartType)" } }
            }
            Get-Secao 'Políticas e reinicialização' {
                $chave = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate'
                $servidor = if (Test-Path -LiteralPath $chave) { (Get-Item -LiteralPath $chave).GetValue('WUServer') }
                $au = Join-Path $chave 'AU'
                $usarWsus = if (Test-Path -LiteralPath $au) { (Get-Item -LiteralPath $au).GetValue('UseWUServer') }
                $pendente = (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending') -or
                    (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired')
                [ordered]@{
                    'Servidor WSUS' = $(if ($servidor) { $servidor } else { 'Sem servidor informado nesta política' })
                    'Política de uso do WSUS' = $(switch ($usarWsus) { 1 { 'Ativada' }; 0 { 'Desativada' }; default { 'Não definida' } })
                    'Reinício pendente' = $(if ($pendente) { 'Detectado' } else { 'Sem indicação nos dois registros consultados' })
                }
            }
            Get-Secao 'Volume do Windows' {
                $volume = [IO.DriveInfo]::new($env:SystemDrive)
                [ordered]@{ 'Espaço livre' = '{0:N1} GiB em {1}' -f ($volume.TotalFreeSpace / 1GB), $env:SystemDrive }
            }
            . (Join-Path $PSScriptRoot 'Coletores/Falhas.ps1')
            Get-Lista 'Eventos do Windows Update · últimas 24 horas' {
                Get-EventosAtendimento 'Microsoft-Windows-WindowsUpdateClient/Operational' -CodigosUpdate
            } 'Nenhum evento no período consultado.'
        }
        'filaimpressao' {
            $filas = Get-Lista 'Diagnóstico de impressão' { Get-FilasAtendimento }
            if (@($filas.campos | Where-Object valor -Like '*Trabalhos indisponíveis.*').Count) {
                $filas.aviso = 'Não foi possível consultar os trabalhos de todas as filas.'
            }
            $filas
        }
        'gpusuario' { Get-SecaoTexto 'Políticas do usuário atual' { Invoke-Gpresult } }
        'eventos' {
            foreach ($canal in 'System','Application') {
                Get-Lista "$canal · avisos e erros das últimas 24 horas" {
                    Get-EventosAtendimento $canal -SomenteAlertas
                } 'Nenhum aviso ou erro no período consultado.'
            }
        }
        { $_ -in 'gphtml','evtx' } {
            try { Exportar-Atendimento $Id $Destino }
            catch { @{ nome = 'Exportação'; campos = @(); aviso = $_.Exception.Message } }
        }
    }
}
