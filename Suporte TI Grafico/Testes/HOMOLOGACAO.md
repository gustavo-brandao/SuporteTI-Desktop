# Homologação do Suporte TI Gráfico

**Versão em testes 1.0.0-beta.3 · Windows x64**

Extraia a beta e a 0.7.0 em pastas separadas. Compare o SHA-256 com o arquivo fornecido ao lado do ZIP. Abra os dois executáveis, confira o título de cada versão e use dados fictícios em todos os cenários de alteração. Registre versão do Windows, arquitetura, escala, tipo de conta, hash do ZIP e resultado de cada cenário.

## Ambientes e cobertura

| Ambiente | Evidência disponível | Aceite de laboratório |
| --- | --- | --- |
| Windows 11 Enterprise 25H2 x64, sessão padrão | Compilação, consultas CIM reais, automação WinForms, ZIP portátil, exportação EVTX e HTML | Repetir em VM limpa e offline, com pasta somente leitura e pendrive |
| Windows 10 x64 | Aguardando homologação | Abertura, CIM, PowerShell 5.1 e rotinas nativas na edição e no build declarados para suporte |
| Windows 11 x64, UAC e conta distinta | Simulações, validação do canal e verificação dos arquivos | Consentir e cancelar, elevar com credenciais distintas, inspecionar a cópia protegida e conferir a limpeza |
| Domínio, UNC e impressoras | Consultas locais e intervenções simuladas | GPO do usuário e do computador, pacote assinado por UNC, bloqueio de adulteração, fila de teste e spooler |
| Notebook, discos e acessibilidade | Ocupação real, SMART parcial e navegação automatizada | Bateria real, SATA/NVMe, DPI de 100%, 125% e 150%, alto contraste, teclado e foco |

## Cenários que alteram o Windows

1. Crie um snapshot da VM e uma conta de teste sem dados reais. Use também um volume descartável e um adaptador de teste. Registre o estado inicial.
2. Em **Atendimento**, crie trabalhos em duas filas locais. Reinicie o spooler e confirme que os trabalhos permanecem. Limpe uma fila e confira o resultado e a outra fila. Exercite o cache do Windows Update com reinício pendente e a atualização das GPO por escopo.
3. Em **Computador**, teste o cache do Chrome com o navegador fechado. Confira que as preferências continuam disponíveis. Em **Rede**, mude para IPv4 fixo e volte para DHCP. Compare endereços, gateway e DNS antes e depois.
4. Em **Administrativo**, execute primeiro os modos de consulta. Depois teste SFC e DISM com reparo e registre seus códigos de saída. Execute CHKDSK completo no volume descartável. Em um volume ocupado, confirme o agendamento na janela nativa, reinicie manualmente e verifique o evento Wininit.
5. Prepare um candidato de perfil de teste com dados descartáveis. Confira o bloqueio com a sessão carregada. Após o logoff, revise conta, SID, pastas e Registro. Cancele uma vez, confirme em nova execução e verifique que as demais contas continuam utilizáveis.
6. Configure Autologon com uma conta dedicada de teste. Confira o próximo logon, desative o recurso e confirme o retorno ao login manual após reiniciar. Verifique que os resultados exportados contêm apenas o estado da operação. Restaure o snapshot.

## Pacote assinado

Use um certificado de assinatura de código confiável e o servidor de carimbo autorizado para ele. O empacotador assina os scripts em uma cópia de compilação antes de incorporar seus hashes ao aplicativo e assina o executável antes de gerar o ZIP.

```powershell
.\Construcao\Publicar.ps1 -CertificadoThumbprint $thumbprint -ServidorCarimbo $url -Destino .\outputs\assinado
.\Testes\Verificar-Pacote.ps1 -Zip .\outputs\assinado\SuporteTI-Grafico-1.0.0-beta.3-win-x64.zip -ExigirAssinatura -ExigirCim -Janela
```

Para verificar a origem UNC, extraia o pacote assinado no compartilhamento e abra pelo caminho UNC. Repita uma consulta administrativa com conta distinta. Em uma cópia descartável, altere um script administrativo e confirme a rejeição. O pacote local sem assinatura cobre a regressão desta beta. A assinatura e os aceites em VM continuam como critérios de promoção.
