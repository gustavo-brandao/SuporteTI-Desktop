param(
    [string]$Dotnet = 'dotnet',
    [string]$Destino,
    [ValidatePattern('^[A-Fa-f0-9]{40}$')][string]$CertificadoThumbprint,
    [uri]$ServidorCarimbo
)

$ErrorActionPreference = 'Stop'
$projeto = Split-Path $PSScriptRoot -Parent
$arquivoProjeto = Join-Path $projeto 'SuporteTI.Grafico.csproj'
[xml]$config = Get-Content -LiteralPath $arquivoProjeto -Raw
$versao = $config.Project.PropertyGroup.Version
if (-not $Destino) { $Destino = Join-Path $projeto 'outputs' }
$destinoCompleto = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Destino)
$nome = "SuporteTI-Grafico-$versao-win-x64"
$zip = Join-Path $destinoCompleto ($nome + '.zip')
$hash = $zip + '.sha256'
if ((Test-Path -LiteralPath $zip) -or (Test-Path -LiteralPath $hash)) {
    throw 'Ja existe um artefato com essa versao no destino. Use outra pasta para repetir a compilacao.'
}

if ([bool]$CertificadoThumbprint -ne [bool]$ServidorCarimbo -or
    ($ServidorCarimbo -and (-not $ServidorCarimbo.IsAbsoluteUri -or $ServidorCarimbo.Scheme -notin 'http','https'))) {
    throw 'Informe o certificado de assinatura e uma URL de carimbo HTTP ou HTTPS juntos.'
}

function Add-AssinaturaPacote {
    param([string]$Arquivo)
    $opcoes = @{ LiteralPath=$Arquivo; Certificate=$certificado; HashAlgorithm='SHA256'; TimestampServer=$ServidorCarimbo.AbsoluteUri }
    $assinatura = Set-AuthenticodeSignature @opcoes
    if ($assinatura.Status -ne 'Valid' -or -not $assinatura.TimeStamperCertificate) {
        throw "Assinatura ou carimbo inválido: $Arquivo ($($assinatura.Status))."
    }
}

$temporaria = Join-Path $destinoCompleto ('.publicar-' + [guid]::NewGuid().ToString('N'))
$pacote = Join-Path $temporaria $nome
[void][IO.Directory]::CreateDirectory($pacote)
try {
    if ($CertificadoThumbprint) {
        $certificado = Get-Item -LiteralPath "Cert:\CurrentUser\My\$CertificadoThumbprint"
        if (-not $certificado.HasPrivateKey -or $certificado.EnhancedKeyUsageList.ObjectId.Value -notcontains '1.3.6.1.5.5.7.3.3') {
            throw 'O certificado não tem chave privada de assinatura de código.'
        }
        $fontes = Join-Path $temporaria 'Fontes'
        $grafico = Join-Path $fontes 'Grafico'
        [void][IO.Directory]::CreateDirectory($grafico)
        Copy-Item -LiteralPath (Join-Path $projeto 'Modulos'), (Join-Path $projeto 'Recursos') -Destination $grafico -Recurse
        Copy-Item -LiteralPath $arquivoProjeto -Destination $grafico
        Get-ChildItem -LiteralPath $projeto -File -Filter '*.cs' | Copy-Item -Destination $grafico
        $repo = Split-Path $projeto -Parent
        $vinculos = @($config.Project.ItemGroup.None | ForEach-Object { $_.Include -split ';' } | Where-Object { $_ -like '../*' })
        foreach ($relativo in $vinculos) {
            $alvo = Join-Path $fontes $relativo.Substring(3)
            [void][IO.Directory]::CreateDirectory((Split-Path $alvo -Parent))
            Copy-Item -LiteralPath (Join-Path $repo $relativo.Substring(3)) -Destination $alvo
        }
        foreach ($script in (Get-ChildItem -LiteralPath $fontes -Recurse -File -Filter '*.ps1')) {
            Add-AssinaturaPacote $script.FullName
        }
        $arquivoProjeto = Join-Path $grafico ([IO.Path]::GetFileName($arquivoProjeto))
    }
    & $Dotnet publish $arquivoProjeto -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o $pacote
    if ($LASTEXITCODE -ne 0) { throw "Publicacao falhou (codigo $LASTEXITCODE)." }
    $documentacao = Join-Path $pacote 'Documentacao'
    [void][IO.Directory]::CreateDirectory($documentacao)
    Copy-Item -LiteralPath (Join-Path $projeto 'README.md'), (Join-Path $projeto 'LICENSE') -Destination $documentacao
    Copy-Item -LiteralPath (Join-Path $projeto 'Testes/HOMOLOGACAO.md') -Destination $documentacao
    if ($CertificadoThumbprint) { Add-AssinaturaPacote (Join-Path $pacote 'Suporte TI Grafico.exe') }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zipTemporario = Join-Path $temporaria ($nome + '.zip')
    [IO.Compression.ZipFile]::CreateFromDirectory($pacote, $zipTemporario)
    $digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $zipTemporario).Hash.ToLowerInvariant()
    [IO.File]::Move($zipTemporario, $zip)
    [IO.File]::WriteAllText($hash, "$digest  $nome.zip`r`n")
    Get-Item -LiteralPath $zip, $hash
} finally {
    [IO.Directory]::Delete($temporaria, $true)
}
