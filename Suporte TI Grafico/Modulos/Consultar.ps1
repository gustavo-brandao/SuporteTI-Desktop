#requires -Version 5.1
param(
    [Parameter(Mandatory)]
    [ValidateSet('sistema','desempenho','discos','bateria','dispositivos','servicos','programas','autologon',
        'rede','dominio','ping','velocidade','falhas','codigofalha','update','filaimpressao','gpusuario','gphtml','eventos','evtx',
        'adaptadores','filaslocais','chromecache','gpupdateuser')]
    [string]$Operacao,
    [string]$Entrada64 = '',
    [ValidateSet('Paradas','Excecoes','Win32','HRESULT')]
    [string]$Familia
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
Set-StrictMode -Version 2.0
. (Join-Path $PSScriptRoot 'Recursos.ps1')

function Get-Secao {
    param([string]$Nome, [scriptblock]$Consultar)
    try {
        $dados = & $Consultar
        $campos = @(
            foreach ($chave in $dados.Keys) {
                $valor = [string]$dados[$chave]
                if (-not [string]::IsNullOrWhiteSpace($valor)) {
                    @{ nome = $chave; valor = $valor }
                }
            }
        )
        $aviso = if ($campos.Count -lt $dados.Count) { 'O Windows não retornou todos os campos.' } else { $null }
        @{ nome = $Nome; campos = $campos; aviso = $aviso }
    } catch {
        @{ nome = $Nome; campos = @(); aviso = $_.Exception.Message }
    }
}

$secoes = @(if ($Operacao -ne 'sistema') {
    . (Join-Path $PSScriptRoot 'Diagnosticos.ps1')
    if ($Operacao -in 'rede','dominio','ping','velocidade','falhas','codigofalha') {
        . (Join-Path $PSScriptRoot 'RedeTriagem.ps1')
        $entrada = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($Entrada64)).Trim()
        Get-RedeTriagem $Operacao $entrada $Familia
    } elseif ($Operacao -in 'update','filaimpressao','gpusuario','gphtml','eventos','evtx') {
        . (Join-Path $PSScriptRoot 'RedeTriagem.ps1')
        . (Join-Path $PSScriptRoot 'Atendimento.ps1')
        $destino = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($Entrada64))
        Get-Atendimento $Operacao $destino
    } elseif ($Operacao -in 'adaptadores','filaslocais','chromecache','gpupdateuser') {
        . (Join-Path $PSScriptRoot 'Intervencoes.ps1')
        Get-ConsultaIntervencao $Operacao
    } else {
        Get-Diagnostico $Operacao
    }
} else { @(
    Get-Secao 'Computador' {
        $pc = Get-CimInstance Win32_ComputerSystem -OperationTimeoutSec 5
        [ordered]@{
            'Nome' = $pc.Name
            'Fabricante' = $pc.Manufacturer
            'Modelo' = $pc.Model
            'Domínio ou grupo de trabalho' = $pc.Domain
            'Ingresso em domínio' = $(if ($null -ne $pc.PartOfDomain) { if ($pc.PartOfDomain) { 'Sim' } else { 'Não' } })
            'RAM instalada' = $(if ($pc.TotalPhysicalMemory -gt 0) { '{0:N1} GB' -f ($pc.TotalPhysicalMemory / 1GB) })
        }
    }
    Get-Secao 'Windows' {
        $os = Get-CimInstance Win32_OperatingSystem -OperationTimeoutSec 5
        [ordered]@{
            'Edição' = $os.Caption
            'Versão' = $os.Version
            'Build' = $os.BuildNumber
            'Arquitetura' = $os.OSArchitecture
            'Última inicialização' = $os.LastBootUpTime
        }
    }
    Get-Secao 'BIOS' {
        $bios = Get-CimInstance Win32_BIOS -OperationTimeoutSec 5
        [ordered]@{
            'Fabricante' = $bios.Manufacturer
            'Versão' = $bios.SMBIOSBIOSVersion
            'Data' = $bios.ReleaseDate
        }
    }
    Get-Secao 'CPU' { [ordered]@{ 'Carga atual' = '{0}%' -f (Get-SuporteCpuPercentual) } }
    Get-Secao 'Memória em uso' { [ordered]@{ 'Leitura atual' = (Get-SuporteMemoria).Resumo } }
) })

$estado = if (-not ($secoes | Where-Object { $_.campos.Count -gt 0 })) { 'indisponivel' }
    elseif ($secoes | Where-Object aviso) { 'parcial' }
    else { 'concluido' }

@{
    versao = 1
    operacao = $Operacao
    estado = $estado
    secoes = $secoes
} | ConvertTo-Json -Depth 5 -Compress
