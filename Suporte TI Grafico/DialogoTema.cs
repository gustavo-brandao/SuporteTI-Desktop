namespace SuporteTI.Grafico;

internal static class DialogoTema
{
    public static DialogResult Mostrar(IWin32Window dono,string texto,string titulo,MessageBoxButtons botoes=MessageBoxButtons.OK,
        MessageBoxIcon icone=MessageBoxIcon.None,MessageBoxDefaultButton padrao=MessageBoxDefaultButton.Button1)
    {
        using var janela=new Form { Text=titulo, StartPosition=FormStartPosition.CenterParent, ClientSize=new Size(620,320),
            AutoScaleMode=AutoScaleMode.Dpi, MinimizeBox=false, MaximizeBox=false, ShowInTaskbar=false, FormBorderStyle=FormBorderStyle.FixedDialog,
            Font=(dono as Control)?.Font??SystemFonts.MessageBoxFont };
        if(dono is Form principal) janela.Icon=principal.Icon;
        var layout=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=1, RowCount=2, Padding=new Padding(20) };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var mensagem=new TextBox { Text=texto, Multiline=true, ReadOnly=true, BorderStyle=BorderStyle.None,
            ScrollBars=ScrollBars.Vertical, Dock=DockStyle.Fill, AccessibleName="Mensagem da confirmação" };
        var acoes=new FlowLayoutPanel { FlowDirection=FlowDirection.RightToLeft, AutoSize=true, Dock=DockStyle.Fill, Padding=new Padding(0,12,0,0) };
        var escolhas=botoes==MessageBoxButtons.YesNo?new[] {("Sim",DialogResult.Yes),("Não",DialogResult.No)}:
            botoes==MessageBoxButtons.OKCancel?new[] {("OK",DialogResult.OK),("Cancelar",DialogResult.Cancel)}:
            new[] {("OK",DialogResult.OK)};
        var lista=new List<Button>();
        foreach(var (nome,resultado) in escolhas)
        {
            var botao=new BotaoModerno { Text=nome, AutoSize=true, DialogResult=resultado, MinimumSize=new Size(92,36) };
            Estilo.Marcar(botao,resultado is DialogResult.OK or DialogResult.Yes?PapelVisual.Primario:PapelVisual.Normal);
            lista.Add(botao);
        }
        for(var i=lista.Count-1;i>=0;i--) acoes.Controls.Add(lista[i]);
        var indice=padrao==MessageBoxDefaultButton.Button2&&lista.Count>1?1:0;
        janela.AcceptButton=lista[indice]; janela.CancelButton=lista[^1];
        janela.Shown+=(_,_)=>lista[indice].Focus();
        layout.Controls.Add(mensagem,0,0); layout.Controls.Add(acoes,0,1); janela.Controls.Add(layout);
        Estilo.Aplicar(janela);
        janela.HandleCreated+=(_,_)=>Estilo.Aplicar(janela);
        return janela.ShowDialog(dono);
    }
}
