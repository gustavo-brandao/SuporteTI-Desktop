$ErrorActionPreference = 'Stop'
$sistema = [Environment]::SystemDirectory
$env:SystemRoot = $env:windir = [IO.Path]::GetDirectoryName($sistema)
$env:PSModulePath = [IO.Path]::Combine($PSHOME, 'Modules')
$env:Path = "$sistema;$PSHOME"
$dados = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('__DADOS__')) | ConvertFrom-Json
$canal = $null
$pasta = $null
$retorno = $null
try {
    $usuario = [Security.Principal.WindowsIdentity]::GetCurrent()
    try {
        $principal = [Security.Principal.WindowsPrincipal]::new($usuario)
        if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
            throw 'O processo nao recebeu permissoes administrativas.'
        }
    } finally { $usuario.Dispose() }
    $canal = [IO.Pipes.NamedPipeClientStream]::new('.', $dados.Canal, [IO.Pipes.PipeDirection]::InOut,
        [IO.Pipes.PipeOptions]::None, [Security.Principal.TokenImpersonationLevel]::Identification)
    $canal.Connect(10000)

    $acl = [Security.AccessControl.DirectorySecurity]::new()
    $acl.SetAccessRuleProtection($true, $false)
    foreach ($sid in 'S-1-5-32-544','S-1-5-18') {
        $regra = [Security.AccessControl.FileSystemAccessRule]::new([Security.Principal.SecurityIdentifier]::new($sid),
            'FullControl', 'ContainerInherit,ObjectInherit', 'None', 'Allow')
        $acl.AddAccessRule($regra)
    }
    $tempWindows = [IO.Path]::Combine([IO.Path]::GetDirectoryName($sistema), 'Temp')
    $pasta = [IO.Path]::Combine($tempWindows, ('SuporteTIGrafico-' + [guid]::NewGuid().ToString('N')))
    [void][IO.Directory]::CreateDirectory($pasta, $acl)
    $env:TEMP = $env:TMP = $pasta
    $leitor = [IO.BinaryReader]::new($canal, [Text.Encoding]::UTF8, $true)
    try { foreach ($item in $dados.Arquivos) {
        $tamanhoZona = $leitor.ReadInt32()
        if ($tamanhoZona -lt 0 -or $tamanhoZona -gt 16384) { throw 'Marca de origem invalida.' }
        $zona = $leitor.ReadBytes($tamanhoZona)
        $bytes = $leitor.ReadBytes($item.Tamanho)
        if ($zona.Length -ne $tamanhoZona -or $bytes.Length -ne $item.Tamanho) { throw 'Copia incompleta.' }
        $alvo = [IO.Path]::Combine($pasta, $item.Nome)
        [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($alvo))
        [IO.File]::WriteAllBytes($alvo, $bytes)
        if ($zona.Length) { [IO.File]::WriteAllBytes(($alvo + ':Zone.Identifier'), $zona) }
        if ((Get-FileHash -LiteralPath $alvo -Algorithm SHA256).Hash -ne $item.Hash) {
            throw 'A copia protegida diverge do executavel. Extraia novamente o pacote.'
        }
        if ($dados.Rede -and [IO.Path]::GetExtension($alvo) -eq '.ps1' -and
            (Get-AuthenticodeSignature -LiteralPath $alvo).Status -ne 'Valid') {
            throw 'A elevacao por UNC exige assinatura Authenticode valida nos scripts administrativos.'
        }
    } } finally { $leitor.Dispose() }
    $retorno = & ([IO.Path]::Combine($pasta, 'Administrativo.ps1')) -Operacao $dados.Operacao -Volume $dados.Volume -Dados $dados.Parametros -Tema $dados.Tema
} catch {
    $retorno = @{ resultado = @{ versao = 1; operacao = $dados.Operacao; estado = 'indisponivel';
        secoes = @(@{ nome = 'Consulta administrativa'; campos = @(); aviso = $_.Exception.Message }) }; html = $null }
} finally {
    try {
        if ($pasta -and [IO.Directory]::Exists($pasta)) { [IO.Directory]::Delete($pasta, $true) }
    } catch {
        $retorno.resultado.secoes += @{ nome = 'Limpeza'; campos = @(); aviso = "Copia protegida nao removida: $pasta" }
        if ($retorno.resultado.estado -eq 'concluido') { $retorno.resultado.estado = 'parcial' }
    }
    if ($canal) {
        try {
            if ($canal.IsConnected) {
                $texto = $retorno | ConvertTo-Json -Depth 8 -Compress
                $bytes = [Text.Encoding]::UTF8.GetBytes($texto)
                $canal.Write($bytes, 0, $bytes.Length)
                $canal.Flush()
            }
        } finally { $canal.Dispose() }
    }
}
