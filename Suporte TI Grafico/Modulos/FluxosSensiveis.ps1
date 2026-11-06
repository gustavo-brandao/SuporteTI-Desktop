# Diálogos locais do processo elevado. A senha nunca compõe o resultado.
function New-FormularioElevado {
    param([string]$Titulo, [int]$Altura)
    Add-Type -AssemblyName System.Windows.Forms, System.Drawing
    $formulario = [Windows.Forms.Form]::new()
    $formulario.Text = $Titulo
    $formulario.ClientSize = [Drawing.Size]::new(680, $Altura)
    $formulario.StartPosition = 'CenterScreen'
    $formulario.FormBorderStyle = 'FixedDialog'
    $formulario.AutoScaleMode = 'Font'
    $formulario.MaximizeBox = $false
    $formulario.MinimizeBox = $false
    $formulario.TopMost = $true
    $formulario
}

function Add-BotoesElevados {
    param($Formulario, [string]$Acao, [int]$Y)
    $confirmar = [Windows.Forms.Button]::new()
    $confirmar.Text = $Acao
    $confirmar.SetBounds(322, $Y, 210, 30)
    $confirmar.DialogResult = 'OK'
    $cancelar = [Windows.Forms.Button]::new()
    $cancelar.Text = '&Cancelar'
    $cancelar.SetBounds(540, $Y, 120, 30)
    $cancelar.DialogResult = 'Cancel'
    $Formulario.Controls.AddRange(@($confirmar, $cancelar))
    $Formulario.CancelButton = $cancelar
    $confirmar
}

function Select-PerfilElevado {
    param([object[]]$Planos)
    $formulario = New-FormularioElevado 'Revisar perfis locais' 358
    try {
        $lista = [Windows.Forms.ComboBox]::new()
        $lista.AccessibleName = 'Conta e SID do perfil candidato'
        $lista.DropDownStyle = 'DropDownList'
        $lista.SetBounds(18, 16, 642, 28)
        foreach ($plano in $Planos) { [void]$lista.Items.Add("$($plano.Conta) · $($plano.SID)") }
        $detalhes = [Windows.Forms.TextBox]::new()
        $detalhes.AccessibleName = 'Pastas, Registro e bloqueios da seleção'
        $detalhes.SetBounds(18, 54, 642, 240)
        $detalhes.Multiline = $detalhes.ReadOnly = $true
        $detalhes.ScrollBars = 'Vertical'
        $formulario.Controls.AddRange(@($lista, $detalhes))
        $confirmar = Add-BotoesElevados $formulario '&Revisar exclusão…' 310
        $lista.Add_SelectedIndexChanged({
            $plano = $Planos[$lista.SelectedIndex]
            $linhas = @("Conta: $($plano.Conta)", "SID: $($plano.SID)")
            $linhas += @($plano.Pastas | ForEach-Object { "Pasta a excluir: $_" })
            $linhas += @($plano.Chaves | ForEach-Object { "Registro: $($_.Nome) → $($_.Caminho)" })
            $linhas += @($plano.Incertas | ForEach-Object { "Pasta a verificar: $_" })
            $linhas += @($plano.Bloqueios | ForEach-Object { "Bloqueio: $_" })
            $detalhes.Lines = $linhas
            $confirmar.Enabled = -not $plano.Bloqueios.Count
        }.GetNewClosure())
        $lista.SelectedIndex = 0
        if ($formulario.ShowDialog() -ne 'OK') { return }
        $plano = $Planos[$lista.SelectedIndex]
        if ($plano.Bloqueios.Count) { throw ($plano.Bloqueios -join ' ') }
        $confirmacao = "Os arquivos e as configurações locais listados serão excluídos. Confirme que tem uma cópia dos dados necessários.`r`n`r`n" + $detalhes.Text
        if ([Windows.Forms.MessageBox]::Show($confirmacao, 'Excluir dados locais?', 'YesNo', 'Warning', 'Button2') -eq 'Yes') {
            $plano
        }
    } finally { $formulario.Dispose() }
}

function Set-LogonGrafico {
    param($Chave)
    Assert-SuporteLogonConfiguravel $Chave
    $formulario = New-FormularioElevado 'Configurar login automático · administrador' 198
    $senha = $null
    try {
        $rotulos = @('&Usuário', '&Domínio (vazio = conta local)', '&Senha')
        $limites = @(104, 253, 256)
        $campos = for ($i = 0; $i -lt $rotulos.Count; $i++) {
            $y = 16 + $i * 37
            $rotulo = [Windows.Forms.Label]::new()
            $rotulo.Text = $rotulos[$i]
            $rotulo.SetBounds(18, $y + 3, 230, 25)
            $formulario.Controls.Add($rotulo)
            $entrada = [Windows.Forms.TextBox]::new()
            $entrada.AccessibleName = $rotulos[$i].Replace('&','')
            $entrada.MaxLength = $limites[$i]
            $entrada.SetBounds(250, $y, 410, 26)
            $formulario.Controls.Add($entrada)
            $entrada
        }
        $campos[2].UseSystemPasswordChar = $true
        $campos[2].ShortcutsEnabled = $false
        [void](Add-BotoesElevados $formulario '&Revisar configuração…' 151)
        if ($formulario.ShowDialog() -ne 'OK') { return $false }
        $usuario = $campos[0].Text.Trim()
        $dominio = $campos[1].Text.Trim()
        if (-not $usuario -or $usuario -match '[\\/@\x00-\x1f]') { throw 'Informe apenas o nome do usuário, sem domínio.' }
        if ($dominio -in '', '.') { $dominio = [Environment]::MachineName }
        if ($dominio -notmatch '^[a-zA-Z0-9][a-zA-Z0-9_.-]{0,252}$') { throw 'Nome de domínio inválido.' }
        if (-not $campos[2].TextLength) { throw 'Informe a senha da conta.' }
        $senha = [Security.SecureString]::new()
        $caracteres = $campos[2].Text.ToCharArray()
        try { foreach ($caractere in $caracteres) { $senha.AppendChar($caractere) } }
        finally {
            [Array]::Clear($caracteres, 0, $caracteres.Length)
            $campos[2].Clear()
        }
        $senha.MakeReadOnly()
        if ([Windows.Forms.MessageBox]::Show("Configurar o login automático para $dominio\$usuario?", 'Confirmar conta', 'YesNo', 'Warning', 'Button2') -ne 'Yes') {
            return $false
        }
        Assert-SuporteLogonConfiguravel $Chave
        Enable-SuporteLogon $Chave $usuario $dominio $senha
        $true
    } finally {
        if ($senha) { $senha.Dispose() }
        $formulario.Dispose()
    }
}
