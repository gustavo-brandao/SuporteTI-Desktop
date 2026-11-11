param([Parameter(Mandatory)][string]$Zip,[string]$Capturas,[string]$Identificador='beta92',[switch]$TestarVelocidadeInternet)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing,System.Windows.Forms,System.IO.Compression.FileSystem
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class CapturaTema {
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd,IntPtr dc,uint flags);
 [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hwnd,int x,int y,int w,int h,bool paint);
 [DllImport("user32.dll")] public static extern bool RedrawWindow(IntPtr hwnd,IntPtr rect,IntPtr region,uint flags);
 [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hwnd,uint msg,IntPtr w,IntPtr l);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd,uint msg,IntPtr w,IntPtr l);
 [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)] public static extern IntPtr BuscarItem(IntPtr hwnd,uint msg,IntPtr w,string texto);
 [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr hwnd,int attribute,out int value,int size);
 public delegate bool Enumerar(IntPtr hwnd,IntPtr dados);
 [DllImport("user32.dll")] public static extern bool EnumWindows(Enumerar callback,IntPtr dados);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hwnd,System.Text.StringBuilder texto,int maximo);
 public static string[] Titulos(int pid) {
   var titulos=new System.Collections.Generic.List<string>();
   EnumWindows((h,d)=>{uint p;GetWindowThreadProcessId(h,out p);if(p==pid){var s=new System.Text.StringBuilder(512);GetWindowText(h,s,512);if(s.Length>0)titulos.Add(s.ToString());}return true;},IntPtr.Zero);
   return titulos.ToArray();
 }
}
'@
[void][CapturaTema]::SetProcessDPIAware()
$temp=Join-Path ([IO.Path]::GetTempPath()) ('SuporteTI-Temas-'+[guid]::NewGuid())
$app=$null
function Esperar($Funcao) {
    $limite=[Diagnostics.Stopwatch]::StartNew()
    do { $resultado=& $Funcao; if($resultado){return $resultado};Start-Sleep -Milliseconds 100 } while($limite.Elapsed.TotalSeconds -lt 25)
    $chamador=(Get-PSCallStack | Select-Object -Skip 1 -First 1).ScriptLineNumber
    throw "Tempo limite na verificação de aparência, linha $chamador."
}
function Controle($Nome,$Tipo) {
    if($Nome.StartsWith('Tema atual:')) {
        $todos=$janela.FindAll([Windows.Automation.TreeScope]::Descendants,[Windows.Automation.Condition]::TrueCondition)
        foreach($item in $todos){if($item.Current.Name.StartsWith($Nome)){return $item}}
        return $null
    }
    $cond=[Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty,$Nome)
    if($Tipo){$cond=[Windows.Automation.AndCondition]::new($cond,[Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty,$Tipo))}
    $janela.FindFirst([Windows.Automation.TreeScope]::Descendants,$cond)
}
function Acionar($Nome) {
    $botao=Esperar {$c=Controle $Nome;if($c -and $c.Current.IsEnabled -and -not $c.Current.IsOffscreen){$c}}
    $botao.SetFocus()
    if($Nome -eq 'Revisar ação…') {
        # A abertura modal deve ser assíncrona para que o verificador possa inspecionar e cancelar.
        if(-not [CapturaTema]::PostMessage([IntPtr]$botao.Current.NativeWindowHandle,0x00F5,[IntPtr]::Zero,[IntPtr]::Zero)){throw 'Não foi possível abrir a revisão.'}
        return
    }
    $botao.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
}
function Combo($Nome,$Valor) {
    $combo=Esperar {Controle $Nome}
    $combo.SetFocus()
    $h=[IntPtr]$combo.Current.NativeWindowHandle
    $indice=[CapturaTema]::BuscarItem($h,0x0158,[IntPtr](-1),$Valor).ToInt32()
    if($indice -lt 0){throw ('Opção ausente: '+$Nome+' / '+$Valor)}
    foreach($tecla in @([int]0x24)+@(for($i=0;$i -lt $indice;$i++){[int]0x28})){
        [void][CapturaTema]::SendMessage($h,0x0100,[IntPtr]$tecla,[IntPtr]1)
        [void][CapturaTema]::SendMessage($h,0x0101,[IntPtr]$tecla,[IntPtr]::Zero)
    }
    [void](Esperar {[CapturaTema]::SendMessage($h,0x0147,[IntPtr]::Zero,[IntPtr]::Zero).ToInt32() -eq $indice})
}
function Tema($Nome) {
    Acionar 'Configurações'
    $radio=Esperar {Controle ('Tema '+$Nome.ToLowerInvariant())}
    $radio.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    [void](Esperar {
        $caminho=Join-Path $temp 'Preferencias/aparencia.json'
        if(Test-Path -LiteralPath $caminho){(Get-Content -LiteralPath $caminho -Raw | ConvertFrom-Json).tema -eq $Nome}
    })
    [void](Esperar {
        $app.Refresh()
        $script:janela=[Windows.Automation.AutomationElement]::FromHandle($app.MainWindowHandle)
        $todos=$janela.FindAll([Windows.Automation.TreeScope]::Descendants,[Windows.Automation.Condition]::TrueCondition)
        foreach($item in $todos){if($item.Current.Name.StartsWith('Tema atual:') -and $item.Current.Name.Contains('preferência: '+$Nome+'.')){return $true}}
    })
    ConferirTitulo
}
function ConferirTitulo {
    $app.Refresh()
    if($app.MainWindowTitle -notmatch '^Suporte TI Gráfico \| \d+\.\d+\.\d+'){throw 'Título ilegível ou sem versão.'}
    if([CapturaTema]::SendMessage($app.MainWindowHandle,0x007F,[IntPtr]::Zero,[IntPtr]::Zero) -eq [IntPtr]::Zero){throw 'Ícone pequeno ausente no título.'}
    $atual=0
    $hr=[CapturaTema]::DwmGetWindowAttribute($app.MainWindowHandle,20,[ref]$atual,4)
    if($hr -eq 0){
        $modo=(Get-Content -LiteralPath (Join-Path $temp 'Preferencias/aparencia.json') -Raw -ErrorAction SilentlyContinue | ConvertFrom-Json).tema
        $sistemaEscuro=(Get-ItemProperty -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -ErrorAction SilentlyContinue).AppsUseLightTheme -eq 0
        $esperado=($modo -eq 'Escuro' -or (($modo -eq 'Sistema') -and $sistemaEscuro)) -and -not [Windows.Forms.SystemInformation]::HighContrast
        if([bool]$atual -ne $esperado){throw 'A barra de título diverge do tema efetivo.'}
    }
}
function Salvar($Nome,[switch]$Inativa) {
    if(-not $Capturas){return}
    $Nome=$Nome.Replace('beta9',$Identificador)
    $sonda=$null
    if($Inativa){$sonda=[Windows.Forms.Form]::new();$sonda.Text='Verificação de janela inativa';$sonda.Show();$sonda.Activate()}
    else{[void][CapturaTema]::SetForegroundWindow($app.MainWindowHandle);$janela.SetFocus()}
    [void][CapturaTema]::RedrawWindow($app.MainWindowHandle,[IntPtr]::Zero,[IntPtr]::Zero,0x185)
    Start-Sleep -Milliseconds 700
    $r=New-Object CapturaTema+Rect
    if(-not [CapturaTema]::GetWindowRect($app.MainWindowHandle,[ref]$r)){throw 'Janela sem área para captura.'}
    # Excluir apenas o rodapé que identifica a estação.
    $img=[Drawing.Bitmap]::new($r.Right-$r.Left,$r.Bottom-$r.Top-35)
    $g=[Drawing.Graphics]::FromImage($img)
    try {
        $dc=$g.GetHdc()
        try {$ok=[CapturaTema]::PrintWindow($app.MainWindowHandle,$dc,2)} finally {$g.ReleaseHdc($dc)}
        if(-not $ok){throw 'Falha na captura da janela.'}
        $img.Save((Join-Path $Capturas $Nome),[Drawing.Imaging.ImageFormat]::Png)
    } finally {$g.Dispose();$img.Dispose();if($sonda){$sonda.Close();$sonda.Dispose()}}
}
function Fechar {
    if($app){
        if(-not $app.HasExited){[void]$app.CloseMainWindow();if(-not $app.WaitForExit(5000)){$app.Kill();$app.WaitForExit()}}
        $app.Dispose()
    }
}
function Abrir {
    $inicio=[Diagnostics.ProcessStartInfo]::new((Join-Path $temp 'Suporte TI Grafico.exe'))
    $inicio.UseShellExecute=$false;$inicio.WorkingDirectory=$temp
    $inicio.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR']=Join-Path $temp 'Cache'
    $inicio.EnvironmentVariables['SUPORTETI_CONFIG_DIR']=Join-Path $temp 'Preferencias'
    $script:app=[Diagnostics.Process]::Start($inicio)
    $handle=Esperar {$app.Refresh();if($app.HasExited){throw 'Aplicação encerrou ao abrir.'};if($app.MainWindowHandle -ne [IntPtr]::Zero){$app.MainWindowHandle}}
    [void][CapturaTema]::MoveWindow($handle,20,20,1180,800,$true)
    $script:janela=[Windows.Automation.AutomationElement]::FromHandle($handle)
    [void](Esperar {Controle 'Configurações'})
}
try {
    [IO.Compression.ZipFile]::ExtractToDirectory((Resolve-Path $Zip).Path,$temp)
    if($Capturas){[void][IO.Directory]::CreateDirectory([IO.Path]::GetFullPath($Capturas))}
    Abrir
    Acionar 'Configurações'
    [void](Esperar {Controle 'Tema atual: Escuro · preferência: Escuro.'})
    Salvar 'aparencia-beta9.png'
    Acionar 'Contas e políticas'
    Combo 'Escopo da operação' 'Computador';Combo 'Opção da operação' 'Exportar HTML'
    [void](Esperar {Controle 'Selecionado: Exportar GPO do computador (UAC)'})
    Salvar 'gpo-escuro-beta9.png'
    Salvar 'gpo-escuro-inativa-beta9.png' -Inativa
    Tema 'Claro'
    Acionar 'Contas e políticas'
    [void](Esperar {Controle 'Selecionado: Exportar GPO do computador (UAC)'})
    Salvar 'gpo-claro-beta9.png'
    Salvar 'gpo-claro-inativa-beta9.png' -Inativa
    Fechar;Abrir
    Acionar 'Configurações'
    [void](Esperar {Controle 'Tema atual: Claro · preferência: Claro.'})
    'Tema Claro persistiu após fechar e reabrir o ZIP com configuração isolada.'
    Tema 'Escuro'
    Acionar 'Windows'
    Combo 'Escolher diagnóstico' 'Consultar código de erro'
    [void][CapturaTema]::RedrawWindow($app.MainWindowHandle,[IntPtr]::Zero,[IntPtr]::Zero,0x185)
    $entrada=Esperar {Controle 'Código de erro' ([Windows.Automation.ControlType]::Edit)}
    $entrada.GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).SetValue('0xC0000005')
    Acionar 'Consultar'
    [void](Esperar {Controle 'Consulta concluída.'})
    $grade=(Controle 'Resultados da consulta').GetCurrentPattern([Windows.Automation.GridPattern]::Pattern)
    $linhas=$grade.Current.RowCount
    Salvar 'resultados-escuro-beta9.png'
    Tema 'Claro';Acionar 'Windows'
    $grade=(Controle 'Resultados da consulta').GetCurrentPattern([Windows.Automation.GridPattern]::Pattern)
    if($grade.Current.RowCount -ne $linhas -or $linhas -eq 0){throw 'Troca de tema perdeu o resultado.'}
    Salvar 'resultados-claro-beta9.png'
    Tema 'Sistema'
    Salvar 'aparencia-sistema-beta9.png'
    Salvar 'aparencia-sistema-inativa-beta9.png' -Inativa
    Fechar;Abrir
    Acionar 'Configurações'
    $radio=Esperar {Controle 'Tema sistema'}
    if(-not $radio.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Current.IsSelected){throw 'Preferência Sistema não persistiu.'}
    'Escuro, Claro e Sistema passaram; troca preservou GPO e resultado da pesquisa de código.'
    $transformar=$janela.GetCurrentPattern([Windows.Automation.TransformPattern]::Pattern)
    $transformar.Resize(780,600)
    foreach($nome in 'Tema escuro','Tema claro','Tema sistema'){
        $item=Esperar {Controle $nome}
        if($item.Current.IsOffscreen){throw "Opção fora da janela compacta: $nome"}
    }
    'Aparência permaneceu acessível na janela compacta. Configuração real do usuário não foi alterada.'
    $transformar.Resize(1180,800)
    foreach($modo in 'Escuro','Claro','Sistema') {
        Tema $modo
        Acionar 'Hardware'
        $aba=Esperar {Controle 'Desempenho' ([Windows.Automation.ControlType]::TabItem)}
        $aba.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
        Combo 'Escolher diagnóstico' 'Monitor ao vivo'
        Acionar 'Iniciar monitor'
        [void](Esperar {
            $itens=$janela.FindAll([Windows.Automation.TreeScope]::Descendants,[Windows.Automation.Condition]::TrueCondition)
            foreach($item in $itens){if($item.Current.Name -match '^CPU: \d+%$'){return $true}}
        })
        Start-Sleep -Seconds 4
        Salvar ('monitor-'+$modo.ToLowerInvariant()+'-beta9.png')
        $aba=Esperar {Controle 'Discos' ([Windows.Automation.ControlType]::TabItem)}
        $aba.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
        $aba=Esperar {Controle 'Desempenho' ([Windows.Automation.ControlType]::TabItem)}
        $aba.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
        Combo 'Escolher diagnóstico' 'Monitor ao vivo'
        [void](Esperar {Controle 'Iniciar monitor'})
        $transformar.Resize(780,600)
        foreach($nomeAba in 'Desempenho','Discos','Bateria','Dispositivos','Serviços','Programas'){
            $item=Esperar {Controle $nomeAba ([Windows.Automation.ControlType]::TabItem)}
            if($item.Current.IsOffscreen){throw ('Aba fora da janela compacta: '+$nomeAba)}
        }
        Salvar ('abas-compactas-'+$modo.ToLowerInvariant()+'-beta9.png')
        $transformar.Resize(1180,800)
        Acionar 'Rede'
        $aba=Esperar {Controle 'Conectividade' ([Windows.Automation.ControlType]::TabItem)}
        $aba.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
        Combo 'Escolher diagnóstico' 'Velocidade da internet'
        Salvar ('velocidade-'+$modo.ToLowerInvariant()+'-beta9.png')
        $transformar.Resize(780,600)
        [void](Esperar {$c=Controle 'Testar velocidade';-not $c.Current.IsOffscreen})
        Salvar ('velocidade-compacta-'+$modo.ToLowerInvariant()+'-beta9.png')
        $transformar.Resize(1180,800)
        if($modo -eq 'Escuro') {
            Acionar 'Testar velocidade'
            [void](Esperar {Controle 'Cancelar'})
            Acionar 'Cancelar'
            [void](Esperar {Controle 'Teste cancelado'})
            [void](Esperar {$c=Controle 'Testar velocidade';$c.Current.IsEnabled})
            'Cancelamento do teste de velocidade devolveu o controle à interface.'
            if($TestarVelocidadeInternet) {
                Acionar 'Testar velocidade'
                $medicao=[Diagnostics.Stopwatch]::StartNew()
                do {
                    Start-Sleep -Milliseconds 250
                    $c=Controle 'Testar velocidade'
                } while(-not $c.Current.IsEnabled -and $medicao.Elapsed.TotalSeconds -lt 45)
                if(-not $c.Current.IsEnabled){throw 'Teste de velocidade excedeu 45 segundos.'}
                $grade=(Controle 'Resultados da consulta').GetCurrentPattern([Windows.Automation.GridPattern]::Pattern)
                if($grade.Current.RowCount -lt 1 -or -not (Controle 'Salvar log…')){throw 'Medição sem resultado e opção de log.'}
                Salvar 'velocidade-resultado-beta9.png'
                ('Teste HTTPS real finalizou em '+$medicao.Elapsed.TotalSeconds.ToString('N1')+' s, com '+$grade.Current.RowCount+' linhas e log disponível.')
            }
        }
        ('Monitor, troca de abas, seis abas visíveis na janela compacta e painel de velocidade: '+$modo+'.')
    }
    foreach($modo in 'Escuro','Claro','Sistema') {
        Tema $modo
        Acionar 'Contas e políticas'
        Combo 'Escopo da operação' 'Usuário da sessão'
        Combo 'Opção da operação' 'Atualizar políticas'
        [void](Esperar {Controle 'Selecionado: Atualizar GPO do usuário'})
        Acionar 'Revisar ação…'
        $condicao=[Windows.Automation.AndCondition]::new(
            [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ProcessIdProperty,$app.Id),
            [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty,'Atualizar GPO do usuário'))
        $modal=Esperar {[Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Descendants,$condicao)}
        $nao=Esperar {$modal.FindFirst([Windows.Automation.TreeScope]::Descendants,
            [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty,'Não'))}
        [void](Esperar {$nao.Current.HasKeyboardFocus})
        $handle=[IntPtr]$modal.Current.NativeWindowHandle
        if([CapturaTema]::SendMessage($handle,0x007F,[IntPtr]::Zero,[IntPtr]::Zero) -eq [IntPtr]::Zero){throw 'Confirmação sem ícone do aplicativo.'}
        $nao.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        [void](Esperar {-not [Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Descendants,$condicao)})
        ('Confirmação '+$modo+': título e ícone presentes, Não como opção padrão; operação cancelada.')
    }
} catch {
    if($app -and -not $app.HasExited){
        Salvar 'debug-confirmacao-beta9.png'
        [CapturaTema]::Titulos($app.Id) | ForEach-Object {Write-Output ('Título nativo: '+$_)}
        $elementos=[Windows.Automation.AutomationElement]::RootElement.FindAll([Windows.Automation.TreeScope]::Descendants,
            [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ProcessIdProperty,$app.Id))
        foreach($item in $elementos){
            if($item.Current.ControlType -eq [Windows.Automation.ControlType]::Window){Write-Output ('Janela: '+$item.Current.Name)}
            if($item.Current.ControlType -eq [Windows.Automation.ControlType]::Button){Write-Output ('Botão: '+$item.Current.Name+' · habilitado '+$item.Current.IsEnabled)}
        }
    }
    throw
} finally {
    Fechar
    $base=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')+'\'
    $alvo=[IO.Path]::GetFullPath($temp)
    if(-not $alvo.StartsWith($base,[StringComparison]::OrdinalIgnoreCase) -or -not [IO.Path]::GetFileName($alvo).StartsWith('SuporteTI-Temas-')){throw 'Destino de limpeza inesperado.'}
    if([IO.Directory]::Exists($alvo)){[IO.Directory]::Delete($alvo,$true)}
}
