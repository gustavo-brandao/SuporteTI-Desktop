param(
    [Parameter(Mandatory)]
    [string]$Zip,
    [switch]$ExigirCim,
    [switch]$Janela,
    [string]$ReferenciaTerminal,
    [string]$ZipAnterior,
    [switch]$ExigirAssinatura,
    [switch]$Uac,
    [string]$Catalogo
)

$ErrorActionPreference = 'Stop'
if ($Uac -and -not $Janela) { throw 'O teste UAC exige a opção Janela e uma sessão interativa.' }
$arquivo = (Resolve-Path -LiteralPath $Zip).Path
$digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $arquivo).Hash
$registro = (Get-Content -LiteralPath ($arquivo + '.sha256') -Raw).Trim()
if ($registro -ne "$digest  $([IO.Path]::GetFileName($arquivo))") {
    throw 'O ZIP diverge do registro SHA-256.'
}

function Wait-Interface {
    param([scriptblock]$Procurar, [string]$Descricao, [int]$Segundos = 10)
    $limite = [Diagnostics.Stopwatch]::StartNew()
    do {
        $valor = & $Procurar
        if ($valor) { return $valor }
        Start-Sleep -Milliseconds 150
    } while ($limite.Elapsed.TotalSeconds -lt $Segundos)
    throw "Tempo limite na interface: $Descricao."
}

function Find-Controle {
    param($Raiz, [string]$Nome)
    $condicao = [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, $Nome)
    $Raiz.FindFirst([Windows.Automation.TreeScope]::Descendants, $condicao)
}

function Test-Janela {
    param([string]$Executavel, [switch]$ExigirCim, [string]$Versao, [string]$ExecutavelAnterior, [switch]$EventoSimulado, [switch]$Uac)
    Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
    $inicio = [Diagnostics.ProcessStartInfo]::new($Executavel)
    $inicio.UseShellExecute = $false
    $inicio.WorkingDirectory = Split-Path -Parent $Executavel
    $cache = Join-Path $inicio.WorkingDirectory 'CacheDeTeste'
    $inicio.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR'] = $cache
    $processo = [Diagnostics.Process]::Start($inicio)
    try {
        $janela = Wait-Interface {
            $processo.Refresh()
            if ($processo.HasExited) { throw "Executavel encerrou com codigo $($processo.ExitCode)." }
            if ($processo.MainWindowHandle -ne [IntPtr]::Zero) {
                [Windows.Automation.AutomationElement]::FromHandle($processo.MainWindowHandle)
            }
        } 'abertura do executavel'
        if ($janela.Current.Name -ne "Suporte TI Gráfico | $Versao") { throw 'O título diverge da versão do ZIP.' }
        if ($ExecutavelAnterior) {
            $inicioAnterior = [Diagnostics.ProcessStartInfo]::new($ExecutavelAnterior)
            $inicioAnterior.UseShellExecute = $false
            $inicioAnterior.WorkingDirectory = Split-Path -Parent $ExecutavelAnterior
            $inicioAnterior.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR'] = Join-Path $inicioAnterior.WorkingDirectory 'CacheDeTeste'
            $anterior = [Diagnostics.Process]::Start($inicioAnterior)
            try {
                $tituloAnterior = Wait-Interface {
                    $anterior.Refresh()
                    if ($anterior.HasExited) { throw 'A prévia anterior encerrou antes de abrir a janela.' }
                    $anterior.MainWindowTitle
                } 'prévia anterior'
                if ($tituloAnterior -ne 'Suporte TI Gráfico | 0.7.0' -or $processo.HasExited) {
                    throw 'As duas versões não ficaram abertas lado a lado.'
                }
                'Beta e 0.7.0 abriram lado a lado, com versões distintas no título.'
            } finally {
                if (-not $anterior.HasExited) {
                    [void]$anterior.CloseMainWindow()
                    if (-not $anterior.WaitForExit(5000)) { $anterior.Kill(); $anterior.WaitForExit() }
                }
                $anterior.Dispose()
            }
        }
        function Abrir-Ferramenta {
            param([string]$Grupo, [string]$Area, [string]$Rotulo)
            (Find-Controle $janela $Grupo).GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
            $abas = Wait-Interface { Find-Controle $janela 'Áreas de ferramentas' } 'áreas de navegação'
            (Wait-Interface {
                $crit = [Windows.Automation.AndCondition]::new(
                    [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, $Area),
                    [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::TabItem))
                $abas.FindFirst([Windows.Automation.TreeScope]::Descendants, $crit)
            } $Area).GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
            $seletor = Wait-Interface { Find-Controle $janela 'Escolher diagnóstico' } 'seletor de ferramenta'
            $opcoes = $seletor.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern)
            $opcoes.Expand()
            $itemAlvo = Wait-Interface { Find-Controle $seletor $Rotulo } $Rotulo
            $selecao = $itemAlvo.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern)
            $selecao.Select()
            $opcoes.Collapse()
            if (-not $selecao.Current.IsSelected) {
                throw "A ferramenta selecionada diverge do destino esperado: $Rotulo."
            }
        }
        if ($Catalogo) {
            $itens = Get-Content -LiteralPath $Catalogo -Raw | ConvertFrom-Json
            if ($itens.Count -ne 44) { throw 'A matriz de navegação não contém as 44 ferramentas.' }
            foreach ($item in $itens) {
                try { Abrir-Ferramenta $item.Grupo $item.Area $item.Rotulo }
                catch { throw "Falha ao abrir $($item.Id) em $($item.Grupo) / $($item.Area). $($_.Exception.Message) $($_.ScriptStackTrace)" }
            }
            'As 44 ferramentas foram acessadas por grupo, aba e seletor sem executar ações.'
            $busca = Find-Controle $janela 'Buscar ferramenta'
            $busca.GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).SetValue('codigofalha')
            $lista = Wait-Interface { Find-Controle $janela 'Ferramentas encontradas' } 'resultado da busca'
            $item = Wait-Interface { Find-Controle $lista 'Consultar código de erro' } 'ferramenta encontrada'
            # A seleção da busca pelo teclado também deve abrir o contexto correto.
            $busca.SetFocus()
            Add-Type -AssemblyName System.Windows.Forms
            [Windows.Forms.SendKeys]::SendWait('{ENTER}')
            [void](Wait-Interface { Find-Controle $janela 'Código de erro' } 'pesquisa aberta pela busca')
        }
        $sobre = Wait-Interface { Find-Controle $janela 'Sobre' } 'botao Sobre'
        $sobre.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Wait-Interface { Find-Controle $janela 'Sobre o Suporte TI' } 'tela Sobre')

        (Find-Controle $janela 'Visão geral').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        $consultar = Wait-Interface { Find-Controle $janela 'Consultar computador' } 'botao de consulta'
        $consultar.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        $estados = 'Consulta concluída.', 'Consulta parcial. Confira os avisos na lista.'
        if (-not $ExigirCim) { $estados += 'Dados indisponíveis. Confira os avisos na lista.' }
        $estado = Wait-Interface {
            foreach ($texto in $estados) {
                if (Find-Controle $janela $texto) { return $texto }
            }
        } 'resultado da consulta' 30
        "Abertura, navegacao e consulta pela janela verificadas. $estado"
        [void](Wait-Interface {
            $log = Find-Controle $janela 'Salvar log…'
            if ($log -and $log.Current.IsEnabled) { $log }
        } 'opcao de salvar log')
        (Find-Controle $janela 'Rede').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Wait-Interface { Find-Controle $janela 'Consulte a configuração ou teste a conectividade.' } 'tela Rede')
        Abrir-Ferramenta 'Windows' 'Eventos e códigos' 'Falhas recentes'

        if ($EventoSimulado) {
            (Find-Controle $janela 'Consultar').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
            $link = Wait-Interface {
                $elementos = $janela.FindAll([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.Condition]::TrueCondition)
                foreach ($item in $elementos) {
                    $invocarCodigo = $null
                    if ($item.Current.Name -eq '0x000000EF' -or
                        $item.GetCurrentPropertyValue([Windows.Automation.ValuePattern]::ValueProperty) -eq '0x000000EF') {
                        if ($item.TryGetCurrentPattern([Windows.Automation.InvokePattern]::Pattern, [ref]$invocarCodigo)) {
                            return $item
                        }
                    }
                }
            } 'link do codigo simulado' 30
            $link.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
            $entrada = Wait-Interface { Find-Controle $janela 'Código de erro' } 'campo de pesquisa'
            if ($entrada.GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).Current.Value -ne '0x000000EF') {
                throw 'O clique nao preencheu o codigo da falha.'
            }
            if (Find-Controle $janela 'Família do código') { throw 'A pesquisa ainda mostra um seletor de familia.' }
            [void](Wait-Interface {
                $elementos = $janela.FindAll([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.Condition]::TrueCondition)
                foreach ($item in $elementos) {
                    if ($item.Current.Name -match 'CRITICAL_PROCESS_DIED' -or
                        $item.GetCurrentPropertyValue([Windows.Automation.ValuePattern]::ValueProperty) -match 'CRITICAL_PROCESS_DIED') {
                        return $true
                    }
                }
            } 'explicacao automatica do codigo simulado' 30)
            'Clique no codigo simulado preencheu o valor e exibiu a explicacao da familia do evento automaticamente.'
        }
        Abrir-Ferramenta 'Impressão' 'Filas e spooler' 'Diagnóstico de impressão'

        (Find-Controle $janela 'Relatórios').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        (Wait-Interface { Find-Controle $janela 'Rever último resultado' } 'revisao do ultimo resultado').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Wait-Interface {
            $log = Find-Controle $janela 'Salvar log…'
            if ($log -and $log.Current.IsEnabled) { $log }
        } 'log recuperado')
        Abrir-Ferramenta 'Hardware' 'Discos' 'Saúde dos discos (UAC)'
        if ($Uac) {
            (Find-Controle $janela 'Consultar (UAC)').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
            [void](Wait-Interface {
                foreach ($mensagem in 'Consulta concluída.', 'Consulta parcial. Confira os avisos na lista.') {
                    if (Find-Controle $janela $mensagem) { return $true }
                }
                foreach ($mensagem in 'Solicitação de UAC cancelada.', 'A consulta falhou.', 'Dados indisponíveis. Confira os avisos na lista.') {
                    if (Find-Controle $janela $mensagem) { throw "Consulta de discos pelo UAC não validada. $mensagem" }
                }
            } 'resultado dos discos pelo UAC' 120)
            'Consulta de discos pelo UAC retornou resultado estruturado.'
        }
        Abrir-Ferramenta 'Hardware' 'Desempenho' 'Monitor ao vivo'
        (Wait-Interface { Find-Controle $janela 'Iniciar monitor' } 'iniciar monitor').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Wait-Interface {
            $tudo = $janela.FindAll([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.Condition]::TrueCondition)
            foreach ($item in $tudo) {
                if ($item.Current.Name -match '^CPU: \d+%$') { return $true }
            }
        } 'amostra do monitor')
        (Find-Controle $janela 'Pausar monitor').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Wait-Interface { Find-Controle $janela 'Iniciar monitor' } 'pausa do monitor')
        Abrir-Ferramenta 'Hardware' 'Discos' 'Mapa de disco'
        [void](Wait-Interface { Find-Controle $janela 'Itens da pasta' } 'lista do mapa')
        (Find-Controle $janela 'Analisar volume').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        (Wait-Interface {
            $botao = Find-Controle $janela 'Cancelar leitura'
            if ($botao -and $botao.Current.IsEnabled) { $botao }
        } 'cancelamento do mapa').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Wait-Interface { Find-Controle $janela 'Leitura cancelada.' } 'leitura cancelada')
        'Monitor com amostra real, pausa e cancelamento do mapa verificados.'
        Abrir-Ferramenta 'Impressão' 'Filas e spooler' 'Diagnóstico de impressão'

        $seletor = Find-Controle $janela 'Escolher diagnóstico'
        $opcoes = $seletor.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern)
        $opcoes.Expand()
        foreach ($nome in 'Diagnóstico de impressão', 'Reiniciar spooler (UAC)', 'Limpar fila de impressão (UAC)') {
            [void](Wait-Interface { Find-Controle $seletor $nome } $nome)
        }
        $opcoes.Collapse()
        'Diagnostico, reinicio e limpeza de fila local aparecem como opcoes distintas.'
        Abrir-Ferramenta 'Hardware' 'Discos' 'Volumes e saúde dos discos'
        (Find-Controle $janela 'Consultar').GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Wait-Interface { Find-Controle $janela 'Gráfico' } 'coluna de graficos' 50)
        'Coluna de graficos exibida na consulta de discos.'
    } finally {
        if (-not $processo.HasExited) {
            [void]$processo.CloseMainWindow()
            if (-not $processo.WaitForExit(5000)) {
                $processo.Kill()
                $processo.WaitForExit()
            }
        }
        $processo.Dispose()
    }
}

$raiz = Join-Path ([IO.Path]::GetTempPath()) ('Suporte TI ' + [guid]::NewGuid().ToString('N'))
try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($arquivo, $raiz)
    $esperados = @('Documentacao', 'Modulos', 'Suporte TI Grafico.exe')
    $itens = @(Get-ChildItem -LiteralPath $raiz -Force | ForEach-Object Name | Sort-Object)
    if (Compare-Object -ReferenceObject $esperados -DifferenceObject $itens) {
        throw "Itens inesperados na raiz do ZIP: $($itens -join ', ')."
    }
    foreach ($item in 'Suporte TI Grafico.exe', 'Modulos/Consultar.ps1', 'Modulos/Recursos.ps1', 'Modulos/Diagnosticos.ps1', 'Modulos/Coletores/Discos.ps1', 'Modulos/Ferramentas/smartmontools/bin/smartctl.exe', 'Modulos/Ferramentas/smartmontools/bin/drivedb.h', 'Modulos/Ferramentas/smartmontools/integridade.json', 'Modulos/Ferramentas/smartmontools/COPYING.txt', 'Modulos/Ferramentas/smartmontools/smartmontools-7.5.tar.gz', 'Documentacao/README.md', 'Documentacao/LICENSE') {
        if (-not [IO.File]::Exists((Join-Path $raiz $item))) { throw "Arquivo ausente no ZIP: $item" }
    }

    $exe = Get-Item -LiteralPath (Join-Path $raiz 'Suporte TI Grafico.exe')
    $inicioExe = [byte[]]::new(16MB)
    $fluxoExe = [IO.File]::OpenRead($exe.FullName)
    try { $lidos = $fluxoExe.Read($inicioExe, 0, $inicioExe.Length) } finally { $fluxoExe.Dispose() }
    if (-not [Text.Encoding]::ASCII.GetString($inicioExe, 0, $lidos).Contains('asInvoker')) {
        throw 'O executavel nao declarou abertura com as permissoes da sessao.'
    }
    $versao = ($exe.VersionInfo.ProductVersion -split '\+', 2)[0]
    if ($versao -notmatch '^\d+\.\d+\.\d+(?:-[A-Za-z0-9.-]+)?$' -or
        [IO.Path]::GetFileName($arquivo) -cne "SuporteTI-Grafico-$versao-win-x64.zip") {
        throw 'A versao do executavel diverge do nome do pacote.'
    }
    if ($ExigirAssinatura) {
        $assinaturaExe = Get-AuthenticodeSignature -LiteralPath $exe.FullName
        if ($assinaturaExe.Status -ne 'Valid' -or -not $assinaturaExe.TimeStamperCertificate) {
            throw 'O executável não tem assinatura válida com carimbo.'
        }
        foreach ($script in (Get-ChildItem -LiteralPath (Join-Path $raiz 'Modulos') -File -Recurse -Filter '*.ps1')) {
            $assinatura = Get-AuthenticodeSignature -LiteralPath $script.FullName
            if ($assinatura.Status -ne 'Valid' -or -not $assinatura.TimeStamperCertificate -or
                $assinatura.SignerCertificate.Thumbprint -ne $assinaturaExe.SignerCertificate.Thumbprint) {
                throw "O script não foi assinado e carimbado pelo mesmo editor: $($script.Name)."
            }
        }
        'Authenticode e carimbos verificados no executável e nos scripts.'
    }

    $powershell = Join-Path ([Environment]::SystemDirectory) 'WindowsPowerShell/v1.0/powershell.exe'
    $administrativo = Join-Path $raiz 'Modulos/Administrativo.ps1'
    if (-not [IO.File]::Exists($administrativo)) { throw 'Coletor administrativo ausente do ZIP.' }
    foreach ($item in 'Rede.ps1','Falhas.ps1','CatalogoFalhas.json','SuporteTI.Windows.dll') {
        if (-not [IO.File]::Exists((Join-Path $raiz ('Modulos/Coletores/' + $item)))) { throw "Coletor ausente no ZIP: $item" }
    }
    $codigo = & $powershell -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned -File (Join-Path $raiz 'Modulos/Consultar.ps1') -Operacao codigofalha -Entrada64 NQ==
    if ($LASTEXITCODE -ne 0) { throw 'Consulta de codigo falhou no ZIP.' }
    $codigo = $codigo | ConvertFrom-Json
    if ($codigo.estado -ne 'concluido' -or 'Win32' -notin $codigo.secoes.nome -or 'Paradas' -notin $codigo.secoes.nome) {
        throw 'A pesquisa automatica nao retornou as duas familias do codigo 5.'
    }
    $resultado = & $powershell -NoLogo -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned -File (Join-Path $raiz 'Modulos/Consultar.ps1') -Operacao sistema
    if ($LASTEXITCODE -ne 0) { throw "Coletor do pacote falhou: codigo $LASTEXITCODE." }
    $resposta = $resultado | ConvertFrom-Json
    if ($resposta.versao -ne 1 -or $resposta.operacao -ne 'sistema' -or
        $resposta.estado -notin 'concluido','parcial','indisponivel' -or $resposta.secoes.Count -ne 5) {
        throw 'O coletor extraido devolveu uma resposta fora do contrato.'
    }
    if ($ExigirCim -and $resposta.estado -eq 'indisponivel') {
        throw 'CIM indisponivel durante o teste do ZIP.'
    }
    "ZIP com tres itens na raiz e coletor extraido verificados. Estado da consulta: $($resposta.estado)."
    $jsonDiscos = & $powershell -NoLogo -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned -File (Join-Path $raiz 'Modulos/Consultar.ps1') -Operacao discos
    if ($LASTEXITCODE -ne 0) { throw "Coletor de discos falhou: codigo $LASTEXITCODE." }
    $discos = $jsonDiscos | ConvertFrom-Json
    if ($discos.operacao -ne 'discos' -or $discos.secoes.Count -ne 2 -or
        ($ExigirCim -and $discos.secoes[0].campos.Count -eq 0)) { throw 'Resposta dos discos invalida.' }
    if ($ExigirCim -and -not @($discos.secoes[0].campos | Where-Object {
        ($_.percentual -is [ValueType]) -and $_.percentual -ge 0 -and $_.percentual -le 100
    }).Count) { throw 'O coletor de discos nao forneceu uma medida valida para o grafico.' }
    "Consulta de discos do ZIP verificada. Estado: $($discos.estado)."
    if ($ReferenciaTerminal) {
        Import-Module (Resolve-Path -LiteralPath $ReferenciaTerminal).Path -ErrorAction Stop
        $terminal = Invoke-SuporteConsulta -Id sistema
        $pares = @(
            @{ Secao = 'Computador'; Campo = 'Nome'; Original = 'Name' }
            @{ Secao = 'Computador'; Campo = 'Modelo'; Original = 'Model' }
            @{ Secao = 'Windows'; Campo = 'Build'; Original = 'BuildNumber' }
            @{ Secao = 'BIOS'; Campo = 'Versão'; Original = 'SMBIOSBIOSVersion' }
        )
        foreach ($par in $pares) {
            $secao = $resposta.secoes | Where-Object nome -EQ $par.Secao
            $campo = $secao.campos | Where-Object nome -EQ $par.Campo
            $padrao = '(?m)^\s*' + $par.Original + '\s*:\s*(.+?)\s*$'
            if (-not $campo -or $terminal.Texto -notmatch $padrao -or $campo.valor.Trim() -ne $Matches[1].Trim()) {
                throw "Resultado diverge da referencia: $($par.Secao) / $($par.Campo)."
            }
        }
        'Nome, modelo, build do Windows e versao da BIOS conferem com o terminal.'
    }
    if ($Janela) {
        $pastaTeste = Join-Path $raiz 'ExportacaoTeste'
        $pasta64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($pastaTeste))
        foreach ($tipo in 'gphtml','evtx') {
            $json = & $powershell -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned -File (Join-Path $raiz 'Modulos/Consultar.ps1') -Operacao $tipo -Entrada64 $pasta64
            if ($LASTEXITCODE -ne 0) { throw "Exportacao $tipo falhou fora do contrato." }
            $exportacao = $json | ConvertFrom-Json
            "Exportacao $tipo em pasta de teste. Estado: $($exportacao.estado)."
            if ($tipo -eq 'gphtml' -and $exportacao.secoes[0].aviso -match 'RSoP') {
                'O gpresult informou indisponibilidade de dados RSoP para a sessao de teste.'
            }
            if ($tipo -eq 'evtx' -and $exportacao.estado -ne 'indisponivel') {
                $item = $exportacao.secoes[0].campos | Where-Object nome -EQ 'Manifesto SHA-256'
                if (-not $item) { throw 'A exportacao EVTX criou arquivos sem manifesto.' }
                $pasta = Split-Path -Parent $item.valor
                $manifesto = Get-Content -LiteralPath $item.valor -Raw | ConvertFrom-Json
                foreach ($arquivoExportado in $manifesto.Arquivos) {
                    $arquivoEvtx = Join-Path $pasta $arquivoExportado.Arquivo
                    if ((Get-FileHash -LiteralPath $arquivoEvtx).Hash -ne $arquivoExportado.SHA256) { throw 'Hash de EVTX divergente.' }
                }
                "Manifesto conferido para $($manifesto.Arquivos.Count) arquivo(s) EVTX."
            }
        }
        if (-not $ExigirAssinatura) {
            Add-Content -LiteralPath (Join-Path $raiz 'Modulos/Coletores/Falhas.ps1') -Value @'

function Get-WinEvent {
    param($FilterHashtable, $MaxEvents)
    $evento = [pscustomobject]@{ LogName='System'; ProviderName='Microsoft-Windows-Kernel-Power'; Id=41; RecordId=1; TimeCreated=Get-Date }
    $evento | Add-Member ScriptMethod ToXml { '<Event><EventData><Data Name="BugcheckCode">239</Data></EventData></Event>' }
    $evento
}
'@
        }
        $anterior = $null
        if ($ZipAnterior) {
            $pastaAnterior = Join-Path $raiz 'PreviaAnterior'
            [IO.Compression.ZipFile]::ExtractToDirectory((Resolve-Path -LiteralPath $ZipAnterior).Path, $pastaAnterior)
            $anterior = Join-Path $pastaAnterior 'Suporte TI Grafico.exe'
        }
        try {
            Test-Janela -Executavel $exe.FullName -ExigirCim:$ExigirCim -Versao $versao -ExecutavelAnterior $anterior -EventoSimulado:(-not $ExigirAssinatura) -Uac:$Uac
        }
        catch { throw ($_.Exception.Message + [Environment]::NewLine + $_.ScriptStackTrace) }
    }
} finally {
    if ([IO.Directory]::Exists($raiz)) { [IO.Directory]::Delete($raiz, $true) }
}
