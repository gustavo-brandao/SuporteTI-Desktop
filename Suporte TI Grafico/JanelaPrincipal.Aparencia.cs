using Microsoft.Win32;

namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private readonly Label estadoAparencia = CriarRotulo("", 10, false);

    private Control CriarTelaAparencia()
    {
        var pagina=CriarPagina();
        pagina.AutoScroll=true;
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        pagina.Controls.Add(CriarRotulo("Aparência",24,true),0,0);
        pagina.Controls.Add(CriarRotulo("Escolha o tema do aplicativo. A alteração é aplicada durante o uso.",10,false),0,1);
        var opcoes=new FlowLayoutPanel { Dock=DockStyle.Fill, AutoSize=true, FlowDirection=FlowDirection.TopDown, WrapContents=false, Padding=new Padding(0,16,0,16) };
        foreach(var (modo,descricao) in new[] {(ModoTema.Escuro,"Superfícies escuras e azul de destaque. Padrão da primeira abertura."),
            (ModoTema.Claro,"Superfícies claras com texto escuro e azul de destaque."),
            (ModoTema.Sistema,"Acompanha a preferência do Windows, inclusive quando ela muda.")})
        {
            var cartao=new Cartao { Width=520, Height=92, Margin=new Padding(3,0,3,12) };
            var radio=new RadioButton { Text=modo==ModoTema.Sistema?"Usar tema do sistema":modo.ToString(),
                AccessibleName="Tema "+modo.ToString().ToLowerInvariant(), AutoSize=true, Dock=DockStyle.Top,
                Checked=Estilo.Escolha==modo, Font=new Font("Segoe UI",11,FontStyle.Bold) };
            var detalhe=CriarRotulo(descricao,9,false); detalhe.Dock=DockStyle.Bottom; detalhe.MaximumSize=new Size(480,0);
            cartao.Controls.Add(detalhe); cartao.Controls.Add(radio); opcoes.Controls.Add(cartao);
            radio.CheckedChanged+=(_,_)=>
            {
                if(!radio.Checked) return;
                // Os rádios estão em cartões separados: manter a exclusão explícita.
                foreach(Control outro in opcoes.Controls)
                    foreach(var escolha in outro.Controls.OfType<RadioButton>())
                        if(!ReferenceEquals(escolha,radio)) escolha.Checked=false;
                Estilo.Escolher(modo); AtualizarEstadoAparencia();
            };
        }
        pagina.Controls.Add(opcoes,0,2);
        estadoAparencia.Dock=DockStyle.Top;
        pagina.Controls.Add(estadoAparencia,0,3);
        pagina.SizeChanged+=(_,_)=>
        {
            var largura=Math.Max(200,pagina.ClientSize.Width-pagina.Padding.Horizontal-20);
            foreach(Control cartao in opcoes.Controls)
            {
                cartao.Width=Math.Min(560*pagina.DeviceDpi/96,largura);
                foreach(var detalhe in cartao.Controls.OfType<Label>()) detalhe.MaximumSize=new Size(Math.Max(140,cartao.Width-30),0);
            }
            estadoAparencia.MaximumSize=Size.Empty;
        };
        AtualizarEstadoAparencia();
        return pagina;
    }

    private void AtualizarEstadoAparencia()
    {
        estadoAparencia.Text=Estilo.AltoContraste ? $"Alto contraste do Windows ativo. Preferência mantida: {Estilo.Escolha}." :
            $"Tema atual: {(Estilo.Escuro?"Escuro":"Claro")} · preferência: {Estilo.Escolha}.";
        estadoAparencia.Text+=Environment.NewLine+(Estilo.Aviso??"Preferência do usuário. O pacote pode permanecer em uma pasta sem permissão de escrita.");
    }

    private void IniciarAparencia()
    {
        Estilo.Inicializar();
        HandleCreated+=(_,_)=>Estilo.Aplicar(this);
        SystemEvents.UserPreferenceChanged+=PreferenciaWindowsAlterada;
        Disposed+=(_,_)=>SystemEvents.UserPreferenceChanged-=PreferenciaWindowsAlterada;
        Estilo.Aplicar(this);
    }

    private void PreferenciaWindowsAlterada(object sender, UserPreferenceChangedEventArgs e)
    {
        if(IsDisposed||!IsHandleCreated) return;
        try { BeginInvoke((Action)(()=>{if(IsDisposed)return; Estilo.AtualizarJanelas(); AtualizarEstadoAparencia();})); }
        catch(InvalidOperationException) when(IsDisposed||Disposing) { }
    }
}
