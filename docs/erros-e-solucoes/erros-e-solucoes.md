# Erros e soluções

Esta página reúne mensagens de erro recorrentes do UniNFe e procedimentos para identificar a causa e restabelecer o funcionamento. Localize a mensagem exibida, abra o item correspondente e execute as verificações na ordem indicada.

Antes de alterar certificados, rede, políticas de segurança ou o Registro do Windows, faça backup e envolva a equipe de infraestrutura. Depois da correção, reinicie o UniNFe ou o serviço do Windows e repita a operação.

## Como usar esta página

1. Copie a mensagem completa do erro. Não procure somente por uma palavra isolada, pois erros de DNS, timeout e TLS podem parecer iguais.
2. Registre qual operação falhou, por exemplo: autorização de NF-e, consulta de status, manifestação, distribuição de DF-e ou serviço de prefeitura.
3. Anote a data e a hora, a empresa configurada, o ambiente de homologação ou produção e se o UniNFe está aberto como aplicativo ou executando como serviço do Windows.
4. Abra o item cuja mensagem mais se aproxima do erro e faça uma verificação por vez. Depois de cada alteração, repita o mesmo teste.
5. Não gere novamente um documento fiscal apenas para testar conectividade. Prefira uma consulta de status do serviço quando ela estiver disponível.
6. Se precisar de ajuda, envie a mensagem completa e os [logs do UniNFe](../referencias/tela-log.md). Uma captura apenas da última linha normalmente não contém contexto suficiente.

## Glossário rápido

| Termo | Significado nesta documentação |
|---|---|
| Assembly ou DLL | Arquivo que contém código e recursos usados pelo UniNFe. Uma DLL ausente, vazia, bloqueada ou de versão incompatível pode impedir o aplicativo de iniciar. |
| Webservice | Serviço disponibilizado por SEFAZ, prefeitura, Receita Federal, banco ou outro órgão para receber consultas e documentos pela internet. |
| Endpoint ou host | Endereço do webservice, como `nfe.fazenda.sp.gov.br`. |
| DNS | Serviço que converte o nome do host em um endereço IP. Se o DNS falhar, o UniNFe não descobre para qual servidor deve se conectar. |
| Porta 443 | Porta normalmente usada pelas conexões HTTPS. A rede pode resolver o host e ainda assim bloquear essa porta. |
| TLS | Protocolo que protege a conexão HTTPS. O cliente e o servidor precisam concordar sobre uma versão e um conjunto de algoritmos. |
| Handshake TLS | Negociação inicial da conexão segura. A falha acontece antes do envio efetivo do XML ao webservice. |
| Schannel | Componente do Windows responsável por TLS, certificados e suítes de cifras para aplicações que usam a segurança do sistema operacional. |
| Suíte de cifras | Conjunto de algoritmos oferecido durante o handshake TLS. Pelo menos uma opção precisa ser aceita pelos dois lados. |
| Certificado A1 | Certificado armazenado em arquivo, normalmente com extensão `.pfx`, protegido por senha. |
| Certificado A3 | Certificado armazenado em token, cartão ou dispositivo criptográfico e acessado por driver, middleware e PIN. |
| LCR ou CRL | Lista de Certificados Revogados publicada pela autoridade certificadora. A SEFAZ consulta essa lista durante algumas validações. |
| `EmProcessamento` | Pasta que pode conter XMLs enviados cuja conclusão ainda está pendente. Os arquivos dessa pasta não devem ser excluídos em massa. |

Para testar o resultado de correções de rede, certificado ou TLS, utilize preferencialmente a [consulta de status do serviço](../servicos/consulta-status-servico.md).

<details id="erro-timeout-conexao-443">
<summary><strong>“Uma tentativa de conexão falhou... o host conectado não respondeu &lt;IP&gt;:443”</strong></summary>

**O que significa:** o nome do servidor provavelmente já foi convertido em um IP, mas a conexão com a porta HTTPS `443` não foi concluída dentro do tempo limite. O XML normalmente ainda não chegou ao webservice. Esse erro é diferente de rejeição fiscal: ele indica falha de comunicação.

**Causa provável:** o UniNFe não conseguiu estabelecer a conexão HTTPS no tempo esperado. A causa pode ser indisponibilidade do webservice, rota do provedor, proxy, firewall, inspeção HTTPS, antivírus ou falha de rede. Embora problemas de DNS possam anteceder a conexão, uma mensagem que já contém um endereço IP normalmente indica que o nome foi resolvido.

**Como diagnosticar:**

1. Identifique no log o nome do host acessado. O IP isolado pode mudar, portanto o teste deve usar o nome do webservice sempre que ele estiver disponível.
2. Abra o PowerShell na mesma máquina do UniNFe e execute:

```powershell
Test-NetConnection "nome-do-host" -Port 443
```

3. Interprete `TcpTestSucceeded`:
   - `True`: a porta respondeu; investigue certificado, TLS ou falha específica do webservice.
   - `False`: a conexão foi bloqueada, não encontrou rota ou o servidor não respondeu.
4. Se o UniNFe roda como serviço, faça o teste na própria máquina do serviço. Um teste feito no computador do desenvolvedor não prova que o servidor do UniNFe possui o mesmo acesso.

**Solução passo a passo:**

1. Confirme se o problema ocorre somente com um órgão ou com todos os serviços.
2. Consulte a disponibilidade da SEFAZ, prefeitura ou Receita Federal e tente novamente alguns minutos depois.
3. Reinicie o UniNFe e, se necessário, o computador e os equipamentos de rede.
4. Confira proxy, firewall e antivírus. Libere o executável do UniNFe e o destino HTTPS na porta 443, conforme a política da empresa.
5. Verifique se há software de segurança bancária, como o Warsaw, interceptando a conexão. Não o remova automaticamente: peça à infraestrutura para atualizar, configurar ou desinstalar temporariamente o componente apenas durante um teste controlado.
6. Teste temporariamente por outra conexão, como uma rede móvel. Se funcionar, solicite ao provedor ou à infraestrutura a análise da rota original.

Se o log também informar que o nome remoto não pôde ser resolvido, siga o diagnóstico específico de DNS apresentado nesta página.

Não fixe no arquivo `hosts` um IP obtido momentaneamente: endereços de webservices podem mudar e essa alteração pode causar novas indisponibilidades.

**Como confirmar a correção:** repita `Test-NetConnection`, execute uma consulta de status no UniNFe e confirme que foi recebido um retorno do webservice. Uma resposta fiscal de indisponibilidade ainda comprova que a comunicação HTTPS foi estabelecida; o que não pode continuar é o timeout local.

</details>

<details id="erro-assembly-zero-bytes-formato-incorreto">
<summary><strong>“Não foi possível carregar arquivo ou assembly ‘0 bytes loaded from System...’” ou “Foi feita uma tentativa de se carregar um programa com um formato incorreto”</strong></summary>

**O que significa:** o .NET tentou abrir uma DLL necessária ao UniNFe e encontrou um arquivo vazio, danificado ou incompatível. `0 bytes loaded` é um indício forte de que o arquivo foi criado ou copiado sem conteúdo. “Formato incorreto” também pode indicar mistura entre bibliotecas de 32 e 64 bits.

**Causa provável:** uma DLL está vazia, corrompida, bloqueada ou incompatível com a arquitetura do processo. Antivírus, cópia incompleta e mistura de arquivos de versões diferentes são causas comuns.

**Como diagnosticar:**

1. Feche o UniNFe antes de examinar ou substituir arquivos.
2. Na pasta de instalação, procure arquivos vazios com o comando abaixo, trocando o caminho pelo caminho real:

```powershell
Get-ChildItem "C:\Caminho\Do\UniNFe" -File -Recurse |
    Where-Object Length -eq 0
```

3. Abra o histórico ou a quarentena do antivírus no mesmo horário do erro.
4. Confirme se DLLs foram copiadas manualmente de outra instalação. Arquivos com o mesmo nome, mas pertencentes a versões diferentes, não devem ser combinados.

**Solução passo a passo:**

1. Feche completamente o UniNFe e preserve uma cópia das configurações.
2. Verifique no antivírus se algum arquivo foi bloqueado ou colocado em quarentena. Cadastre uma exceção restrita à pasta oficial do UniNFe, se a política de segurança permitir.
3. Reinstale ou atualize o UniNFe usando o pacote oficial completo, sem reaproveitar DLLs de outra versão.
4. Se o erro continuar, confira também os eventos e a quarentena do antivírus relacionados à pasta temporária da conta que executa o UniNFe, pois bibliotecas auxiliares podem ser geradas nela. Evite excluir toda a pasta temporária da verificação; quando necessário, crie apenas a exceção mais restrita permitida pelo produto de segurança.

Evite manter o antivírus desativado. Se for indispensável para o diagnóstico, faça um teste breve e controlado com a equipe de infraestrutura e reative a proteção em seguida.

Não baixe a DLL citada em sites de arquivos avulsos. Faça a [atualização completa do UniNFe](../instalacao/atualizacao.md), pois uma DLL normalmente depende de várias outras da mesma versão.

**Como confirmar a correção:** abra o UniNFe, repita a operação original e verifique se nenhuma nova DLL vazia foi criada. Se o arquivo voltar a ficar com tamanho zero, investigue antivírus, permissão de gravação e espaço livre em disco antes de reinstalar novamente.

</details>

<details id="erro-ssl-tls-certificado-a3">
<summary><strong>Erro de conexão SSL/TLS ao usar certificado digital A3</strong></summary>

**O que significa:** o UniNFe precisa acessar a chave privada que está dentro do token ou cartão A3 para autenticar e assinar a comunicação. A falha pode acontecer antes do envio porque o Windows, o driver ou o serviço não conseguiu usar o dispositivo.

**Causa provável:** falha no driver ou middleware do token, acesso ao dispositivo USB, seleção do certificado, PIN armazenado, interferência de software de segurança, Windows desatualizado ou indisponibilidade do webservice.

**Como diagnosticar:**

1. Confirme se o erro ocorre somente com o A3. Se outro certificado ou uma operação sem certificado funciona, a rede básica provavelmente está disponível.
2. Abra o aplicativo do fabricante do token e confirme se ele reconhece o dispositivo e mostra o certificado correto.
3. Confira validade, CNPJ/CPF e presença da chave privada. Ver o certificado na lista do Windows não garante, sozinho, que a chave privada esteja acessível.
4. Se o UniNFe executa como serviço, compare com um teste interativo usando a mesma máquina. Tokens A3 podem exigir PIN ou interface visível e se comportar de forma diferente em serviço.

**Solução passo a passo:**

1. Confirme se o certificado está válido e pode ser utilizado pelo gerenciador do fabricante.
2. Reconecte o token ou a leitora em outra porta USB e evite hubs sem alimentação.
3. Reinstale o driver e o middleware oficiais do fabricante na arquitetura correta para o Windows.
4. Remova temporariamente o PIN salvo na configuração do UniNFe e informe-o quando solicitado.
5. Atualize o Windows e confirme que o TLS 1.2 está habilitado nas opções de segurança da Internet.
6. Verifique se antivírus, firewall, inspeção HTTPS ou software bancário, como o Warsaw, está interferindo no acesso ao certificado. Atualize ou configure o componente antes de considerar uma desinstalação temporária para teste.
7. Teste outro serviço ou aguarde alguns minutos para descartar instabilidade do órgão.

Para uma demonstração das verificações mais comuns com certificado A3, consulte o [vídeo de apoio da Unimake](https://www.youtube.com/watch?v=Uwfrpl5DXtk).

**Como confirmar a correção:** feche e abra o gerenciador do token, confirme que o certificado continua visível e execute uma consulta de status que exija certificado. O teste deve terminar com um retorno do webservice, sem nova mensagem de acesso ao token, PIN ou canal TLS.

</details>

<details id="erro-rejeicao-296-lcr-certificado">
<summary><strong>Rejeição 296 — Certificado de assinatura: erro no acesso à LCR</strong></summary>

**O que significa:** a mensagem veio da SEFAZ depois que ela recebeu o documento. Durante a validação do certificado de assinatura, a SEFAZ tentou consultar a LCR e não conseguiu. Isso é uma rejeição fiscal externa, não uma falha local de leitura do XML pelo UniNFe.

**Causa provável:** ao validar a assinatura, a SEFAZ não conseguiu consultar a Lista de Certificados Revogados (LCR) publicada pela autoridade certificadora. Essa rejeição, isoladamente, não significa que o certificado esteja revogado.

**Solução passo a passo:**

1. Guarde o XML e o retorno com a rejeição 296.
2. Aguarde alguns minutos e reenvie o mesmo documento sem alterar seu conteúdo fiscal.
3. Se a rejeição persistir, verifique a cadeia do certificado e acione a SEFAZ ou a autoridade certificadora, informando o código 296, o horário, a UF e o endereço da LCR.
4. Não substitua o certificado apenas por causa de uma ocorrência isolada. A disponibilidade da LCR é externa ao UniNFe.

Para auxiliar o diagnóstico, abra o certificado no Windows, acesse **Detalhes > Pontos de Distribuição da Lista de Certificados Revogados** e identifique os endereços da LCR. Teste se o endereço pode ser acessado e informe-o à certificadora ou à SEFAZ. Conseguir baixar o arquivo `.crl` localmente confirma a disponibilidade do endereço naquele momento, mas não comprova sozinho que a SEFAZ consegue acessá-lo nem que o certificado está ou não revogado.

**Como confirmar a correção:** o reenvio deve retornar autorização ou outra resposta fiscal diferente da rejeição 296. Não considere o problema resolvido apenas porque a URL da LCR abriu no navegador local.

</details>

<details id="erro-protocolo-seguranca-nao-suportado">
<summary><strong>“The requested security protocol is not supported” ou “O protocolo de segurança solicitado não é suportado”</strong></summary>

**O que significa:** o UniNFe solicitou uma conexão segura, mas o Windows não conseguiu oferecer o protocolo exigido pelo servidor. Na prática, o ambiente costuma estar desatualizado ou com o TLS 1.2 desabilitado por configuração ou política.

**Causa provável:** o Windows não oferece ou não habilitou a versão de TLS exigida pelo webservice.

**Solução passo a passo:**

1. Instale todas as atualizações aplicáveis do Windows.
2. Abra **Opções da Internet > Avançadas > Segurança** e confirme que o TLS 1.2 está habilitado.
3. Reinicie o computador e repita o teste.
4. Em Windows Server administrado por políticas, peça à infraestrutura para conferir as configurações do Schannel e as políticas de TLS.

Não habilite SSL 3.0, TLS 1.0 ou TLS 1.1 como tentativa de correção; são protocolos antigos e não resolvem a exigência de TLS 1.2 do serviço.

**Como confirmar a correção:** reinicie a máquina após alterações no Windows ou no Schannel e execute uma consulta de status. Se a mensagem mudar para erro de certificado, timeout ou resposta fiscal, o protocolo passou a ser aceito e a nova mensagem deve ser tratada separadamente.

</details>

<details id="erro-nome-remoto-nao-resolvido">
<summary><strong>“O nome remoto não pôde ser resolvido: ‘nfe.fazenda.sp.gov.br’”</strong></summary>

**O que significa:** o UniNFe conhece o nome do webservice, mas o DNS não devolveu um endereço IP. Como não existe IP de destino, a conexão HTTPS nem chegou a começar.

**Causa provável:** o DNS configurado na máquina não conseguiu transformar o nome do webservice em um endereço IP, ou o nome está inacessível por falha de rede, proxy ou indisponibilidade externa.

**Como diagnosticar:**

1. Copie somente o host mostrado na mensagem, sem `https://`, barras ou caminho adicional.
2. Execute na máquina do UniNFe:

```powershell
Resolve-DnsName "nfe.fazenda.sp.gov.br"
```

3. Se o comando retornar um ou mais endereços IP, o DNS respondeu naquele momento. Se retornar erro de nome inexistente, timeout ou servidor DNS indisponível, continue com os passos abaixo.
4. Compare o resultado com outra máquina da mesma rede. Se ambas falham, o problema tende a estar no DNS ou provedor; se apenas o servidor do UniNFe falha, investigue a configuração local.

**Solução passo a passo:**

1. Confirme se o endereço pode ser resolvido em outra máquina da mesma rede.
2. Reinicie o UniNFe, o computador e o equipamento de rede.
3. Limpe o cache DNS do Windows com `ipconfig /flushdns` em um terminal administrativo.
4. Peça à infraestrutura para validar o DNS corporativo. Quando permitido, teste temporariamente um resolvedor público confiável, como Cloudflare (`1.1.1.1` e `1.0.0.1`), Google (`8.8.8.8` e `8.8.4.4`) ou Quad9 (`9.9.9.9` e `149.112.112.112`). Se funcionar, corrija o DNS definitivo conforme a política da rede.
5. Revise proxy, firewall, filtro de conteúdo, antivírus e software de segurança bancária, como o Warsaw.
6. Teste por outra conexão, como uma rede 4G ou 5G. Se funcionar, acione o provedor ou o administrador da rede original.

Não grave manualmente o IP do webservice no arquivo `hosts`, pois o endereço pode mudar.

**Como confirmar a correção:** `Resolve-DnsName` deve retornar um IP e, em seguida, `Test-NetConnection "nome-do-host" -Port 443` deve conseguir testar a porta. Depois, execute a consulta de status no UniNFe.

</details>

<details id="erro-assembly-netstandard-2">
<summary><strong>“Não foi possível carregar o arquivo ou assembly ‘netstandard, Version=2.0.0.0...’”</strong></summary>

**O que significa:** uma biblioteca usada pelo UniNFe foi compilada para APIs do .NET Standard, mas o .NET Framework ou o Windows da máquina não forneceu a camada de compatibilidade necessária. Copiar apenas `netstandard.dll` para a pasta não corrige corretamente o ambiente.

**Causa provável:** o Windows ou o .NET Framework instalado não atende às dependências da versão do UniNFe, ou a instalação do aplicativo está incompleta.

**Como diagnosticar:**

1. Execute `winver` e anote a versão do Windows.
2. Abra **Aplicativos e Recursos** ou **Programas e Recursos** e verifique as atualizações instaladas.
3. Confirme se a máquina aceita o instalador oficial do .NET Framework 4.8.1. Se o instalador recusar o sistema operacional, o problema não deve ser contornado com DLLs avulsas.

**Solução passo a passo:**

1. Atualize o Windows completamente.
2. Instale o [.NET Framework 4.8.1 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net481), compatível com a versão atual do UniNFe.
3. Reinicie o computador.
4. Atualize ou reinstale o UniNFe com o pacote oficial completo.

Se o instalador do .NET Framework 4.8.1 informar que o sistema operacional não é compatível, atualize o Windows para uma versão suportada.

Não baixe `netstandard.dll` de sites de terceiros e não copie o arquivo de outra máquina. Além do risco de segurança, a DLL isolada não instala as demais dependências do runtime.

**Como confirmar a correção:** reinicie o Windows, abra o UniNFe e execute uma consulta de status. Se a mensagem citar outra DLL, confirme se a atualização do UniNFe foi completa e trate a nova dependência sem misturar versões.

</details>

<details id="erro-assembly-local-rede-loadfromremotesources">
<summary><strong>“Tentativa de carregar um assembly de um local de rede...” ou referência a `loadFromRemoteSources`</strong></summary>

**O que significa:** o Windows marcou a DLL como proveniente de uma origem remota ou não confiável. Para protegê-lo, o .NET recusou executar aquele código com confiança total.

**Causa provável:** o UniNFe está sendo executado a partir de compartilhamento de rede, ou os arquivos foram marcados pelo Windows como provenientes de outro computador.

**Como identificar a origem:**

1. Confirme o caminho do executável. Caminhos iniciados por `\\servidor\pasta` ou unidades mapeadas indicam execução em rede.
2. Clique com o botão direito no ZIP, EXE ou DLL e abra **Propriedades**. A opção **Desbloquear** indica que o Windows registrou a origem externa.
3. Se o aplicativo foi copiado de um download, desbloqueie preferencialmente o ZIP original antes de extrair. Isso evita desbloquear DLL por DLL.

**Solução preferencial:**

1. Instale e execute o UniNFe em uma pasta local do computador.
2. Feche o aplicativo, abra as propriedades do ZIP ou do instalador baixado e use **Desbloquear**, quando a opção estiver disponível.
3. Extraia ou reinstale novamente os arquivos depois do desbloqueio.

Alterar o `machine.config` para habilitar `<loadFromRemoteSources enabled="true"/>` afeta aplicações .NET da máquina inteira. Use essa alternativa somente quando a execução em rede for obrigatória, após avaliação e backup pela equipe de infraestrutura.

**Alternativa administrativa para ambiente legado:**

1. Feche o UniNFe e faça uma cópia de segurança do `machine.config` correspondente à arquitetura usada:
   - Windows/processo de 32 bits: `%windir%\Microsoft.NET\Framework\<versão>\Config\machine.config`;
   - Windows/processo de 64 bits: `%windir%\Microsoft.NET\Framework64\<versão>\Config\machine.config`.
2. Dentro de `<configuration>`, localize a seção `<runtime>` existente e inclua somente o elemento abaixo. Se a seção não existir, ela deve ser criada uma única vez.

```xml
<configuration>
  <runtime>
    <loadFromRemoteSources enabled="true" />
  </runtime>
</configuration>
```

3. Preserve todos os outros elementos do arquivo, salve a alteração, abra o UniNFe e repita o teste.

Essa opção concede confiança total a assemblies carregados de origem remota. Não a utilize para arquivos cuja procedência e integridade não tenham sido confirmadas.

Consulte também a documentação oficial do [.NET Framework sobre `loadFromRemoteSources`](https://learn.microsoft.com/dotnet/framework/configure-apps/file-schema/runtime/loadfromremotesources-element).

**Como confirmar a correção:** abra o UniNFe a partir da pasta local e repita a operação. Se a execução local funciona e a de rede falha, a causa está confirmada. Se o erro continuar também localmente, revise bloqueio dos arquivos e integridade da instalação.

</details>

<details id="erro-windows-7-netstandard">
<summary><strong>O UniNFe não inicia no Windows 7 e informa erro relacionado a `netstandard`</strong></summary>

**O que significa:** a versão atual do UniNFe depende de uma versão do .NET Framework que não pode ser instalada no Windows 7. Instalar atualizações antigas ou copiar DLLs pode mudar a mensagem, mas não transforma o sistema em um ambiente suportado.

**Causa provável:** o Windows 7 não é compatível com o .NET Framework 4.8.1 utilizado pela versão atual do UniNFe e deixou de receber suporte regular da Microsoft.

**Solução passo a passo:**

1. Faça backup das configurações, certificados A1 e demais arquivos necessários à instalação atual. Nunca exporte um certificado sem proteger o arquivo e a senha.
2. Prepare uma máquina ou servidor com Windows suportado.
3. Instale todas as atualizações do Windows e o .NET Framework 4.8.1.
4. Instale a versão atual do UniNFe pelo pacote oficial.
5. Restaure ou refaça as configurações e valide primeiro em homologação.
6. Teste pastas, permissões, certificado, consulta de status e integração do ERP antes de desativar a máquina antiga.

Não é recomendado manter pacotes antigos específicos do Windows 7 como solução permanente, pois isso não torna o ambiente compatível com a versão atual do aplicativo.

Consulte os [requisitos oficiais do .NET Framework](https://learn.microsoft.com/dotnet/framework/get-started/system-requirements) antes de preparar o novo servidor.

**Como confirmar a correção:** a versão atual do UniNFe deve iniciar no novo ambiente, carregar as configurações e concluir uma consulta de status sem erro de `netstandard`.

</details>

<details id="erro-sat-falhas-recorrentes">
<summary><strong>SAT: falha no sincronismo do relógio, formato incorreto ou retorno vazio</strong></summary>

Este item reúne três problemas diferentes. Antes de alterar DLLs, identifique qual das três mensagens ocorreu.

**Sincronismo de relógio realizado com fracasso**

**O que significa:** o relógio interno do SAT não conseguiu sincronizar com o servidor de horário. Diferença de horário pode impedir a comunicação fiscal.

**Como resolver:** solicite à infraestrutura a liberação da comunicação NTP pela porta UDP 123 e repita o sincronismo no aplicativo do fabricante. Como UDP não funciona como um teste TCP comum, confirme também as regras de saída do firewall e o endereço NTP configurado no equipamento.

**Como confirmar:** o aplicativo do fabricante deve informar sucesso no sincronismo e exibir data e hora compatíveis com o Windows e com o fuso local.

**Foi feita uma tentativa de carregar um programa com um formato incorreto**

**O que significa:** a instalação não corresponde ao SAT ou alguma DLL do fabricante possui arquitetura incompatível.

**Como resolver:** confirme se foi instalada a distribuição do UniNFe destinada ao SAT e se as DLLs do fabricante correspondem à arquitetura exigida pelo equipamento. Consulte a [Central de downloads da Unimake](https://www.unimake.com.br/central-downloads) e não misture arquivos de distribuições diferentes.

**Como confirmar:** reinicie o UniNFe e execute uma consulta do SAT. A DLL deve ser carregada sem a mensagem de formato incorreto.

**Retorno em branco ou vazio**

**O que significa:** o SAT ou a DLL do fabricante não devolveu uma mensagem que o UniNFe pudesse apresentar. Algumas rejeições aparecem somente nos logs do equipamento.

1. Consulte primeiro os logs do aplicativo do fabricante do SAT.
2. Feche o UniNFe.
3. Reinstale o software oficial do equipamento e confirme se as DLLs exigidas pelo fabricante estão disponíveis para o UniNFe.
4. Se o manual do fabricante determinar a cópia das DLLs, faça backup dos arquivos existentes e copie as DLLs da pasta de instalação do software do SAT para a pasta de instalação do UniNFe, respeitando a arquitetura correta.
5. Abra o UniNFe e repita o envio.

Copie DLLs manualmente somente quando o manual do fabricante determinar os arquivos e a arquitetura corretos.

**Como confirmar:** o novo envio deve retornar uma resposta preenchida. Se continuar vazio, reúna o log do UniNFe e o log do aplicativo do SAT no mesmo horário.

</details>

<details id="erro-connection-attempt-failed-443">
<summary><strong>“A connection attempt failed because the connected party did not properly respond... &lt;IP&gt;:443”</strong></summary>

**O que significa:** essa é a versão em inglês do erro de timeout de conexão HTTPS. Ela equivale a “Uma tentativa de conexão falhou... o host conectado não respondeu”. Não é uma segunda causa diferente.

**Solução passo a passo:** siga o diagnóstico completo do item [timeout de conexão na porta 443](#erro-timeout-conexao-443): teste o host com `Test-NetConnection`, verifique a disponibilidade do órgão, revise proxy, firewall, antivírus, software de segurança bancária e rota do provedor, e teste por outra conexão.

**Como confirmar a correção:** a consulta deve receber uma resposta do webservice sem repetir o timeout, independentemente de a mensagem do Windows estar em português ou inglês.

</details>

<details id="erro-host-nao-conhecido-configuracoes">
<summary><strong>“Este host não é conhecido” ao abrir as configurações</strong></summary>

**O que significa:** neste caso, “host” é o próprio computador onde o UniNFe está executando, não necessariamente o endereço da SEFAZ. O Windows ou a rede não conseguiu tratar corretamente o nome da máquina.

**Causa provável:** o nome do computador contém acentos, espaços ou caracteres especiais que impedem sua resolução correta no ambiente.

**Como diagnosticar:**

1. Abra o Prompt de Comando e execute `hostname`.
2. Anote o nome exibido e procure espaços, acentos, sublinhados ou outros caracteres especiais.
3. Confirme com a infraestrutura se a máquina pertence a um domínio Windows, pois a renomeação pode depender de credenciais e políticas do domínio.

**Solução passo a passo:**

1. Peça ao administrador para renomear o computador usando somente letras sem acentos, números e hífen.
2. No Windows, a opção normalmente fica em **Configurações > Sistema > Sobre > Renomear este computador**.
3. Reinicie o Windows.
4. Execute `hostname` novamente e confirme o novo nome.
5. Em domínio, aguarde ou solicite a atualização dos registros DNS e do objeto do computador.

Em domínio Windows, a troca do nome pode exigir credenciais administrativas e atualização dos registros DNS; coordene a alteração com a infraestrutura.

**Como confirmar a correção:** abra novamente as configurações do UniNFe. A tela deve carregar sem “Este host não é conhecido”, e o novo nome deve ser reconhecido pela rede.

</details>

<details id="erro-ssl-tls-certificado-a1">
<summary><strong>Erro de conexão SSL/TLS ao usar certificado digital A1</strong></summary>

**O que significa:** o UniNFe precisa abrir o arquivo PFX e acessar sua chave privada para autenticar a conexão. O certificado aparecer instalado no Windows não garante que o arquivo, a senha e a chave privada estejam acessíveis à conta que executa o UniNFe.

**Causa provável:** arquivo PFX inválido, senha incorreta, cadeia incompleta, certificado expirado, instalação do Windows corrompida, bloqueio de segurança ou incompatibilidade de TLS.

**Como diagnosticar:**

1. Confira a data de validade, o CNPJ/CPF e se o certificado possui chave privada.
2. Confirme qual modo está configurado no UniNFe: certificado instalado no Windows ou arquivo PFX.
3. Se for PFX, confirme o caminho exato, a senha e se o arquivo ainda existe nesse local.
4. Se o UniNFe roda como serviço, identifique a conta do serviço. Testar o PFX com o usuário logado não prova que a conta do serviço tem permissão de leitura.
5. Não envie o PFX e a senha juntos por e-mail, chamado ou mensageiro. O arquivo contém a identidade digital da empresa.

**Solução passo a passo:**

1. Confirme a validade do certificado e a senha do PFX.
2. Prefira configurar o certificado A1 diretamente pelo arquivo PFX no UniNFe, mantendo o arquivo em pasta local protegida e acessível à conta que executa o aplicativo ou serviço.
3. Reinstale as cadeias oficiais da autoridade certificadora, se estiverem ausentes.
4. Atualize o Windows e confirme o TLS 1.2.
5. Revise proxy, firewall, antivírus, inspeção HTTPS e software de segurança bancária, como o Warsaw. Atualize ou configure o componente antes de uma eventual remoção temporária para teste.
6. Teste outro serviço para descartar indisponibilidade temporária da SEFAZ, prefeitura ou Receita Federal.

Ao executar como serviço do Windows, confirme também que a conta do serviço possui acesso ao PFX e à pasta onde ele está armazenado.

**Como confirmar a correção:** reinicie o UniNFe ou o serviço e execute uma consulta de status com a empresa correta. O retorno deve chegar sem mensagem de senha, chave privada, certificado não encontrado ou canal TLS.

</details>

<details id="erro-tela-branca-sem-botoes">
<summary><strong>A tela do UniNFe fica branca e os botões não aparecem</strong></summary>

**O que significa:** o aplicativo abriu, mas o Windows não conseguiu renderizar corretamente elementos da interface. Quando os botões e ícones desaparecem, a ausência ou corrupção das fontes usadas pela interface é uma causa conhecida.

**Causa provável:** fontes usadas pela interface estão ausentes ou corrompidas no Windows.

**Solução passo a passo:**

1. Feche completamente o UniNFe.
2. Baixe o [pacote de fontes disponibilizado pela Unimake](https://www.unimake.com.br/downloads/beta/fontes.zip).
3. Descompacte o ZIP, selecione as fontes e escolha **Instalar para todos os usuários**, se disponível.
4. Autorize a substituição das fontes existentes quando o Windows solicitar.
5. Reinicie o Windows e abra novamente o UniNFe.

Se houver mais de um usuário no computador, prefira **Instalar para todos os usuários**. Instalar a fonte apenas no perfil do administrador pode não corrigir a tela para o usuário que realmente executa o UniNFe.

**Como confirmar a correção:** abra o menu principal e algumas telas de configuração. Botões, textos e ícones devem aparecer sem áreas vazias. Se apenas um usuário continuar com a tela branca, compare as fontes e permissões desse perfil.

</details>

<details id="erro-canal-seguro-ssl-tls">
<summary><strong>“A solicitação foi anulada: Não foi possível criar um canal seguro para SSL/TLS” ao enviar um DF-e</strong></summary>

**O que significa:** o Windows e o webservice não concluíram a criação do canal HTTPS. O erro ocorre antes de o XML receber uma resposta fiscal. Ele pode envolver protocolo TLS, cadeia de confiança, certificado da empresa, proxy com inspeção HTTPS ou combinação desses fatores.

**Causa provável:** o Windows e o servidor não conseguiram negociar uma conexão TLS. As causas mais comuns são Windows desatualizado, TLS 1.2 desabilitado, cadeia de certificados incompleta, certificado digital inacessível ou interferência de proxy/antivírus.

**Como separar as causas:**

| Resultado do teste | Indicação mais provável |
|---|---|
| Nenhuma empresa ou serviço conecta | Windows, TLS, proxy, firewall, cadeia raiz ou rede da máquina |
| Apenas uma empresa falha | Certificado ou configuração daquela empresa |
| A1 funciona e A3 falha | Token, driver, middleware, PIN ou acesso ao dispositivo A3 |
| A3 funciona e A1 falha | PFX, senha, chave privada, caminho ou permissão do arquivo A1 |
| Apenas uma SEFAZ falha | Disponibilidade, endpoint ou negociação de TLS/suíte específica daquela SEFAZ |
| A porta 443 responde, mas o canal seguro falha | Rede básica disponível; investigar TLS e certificados |

**Solução passo a passo:**

1. Instale todas as atualizações do Windows e reinicie o computador.
2. Confirme que o TLS 1.2 está habilitado.
3. Atualize as cadeias de certificados pelo Windows Update e reinstale a cadeia oficial do certificado digital, quando necessário.
4. Para o certificado A1, valide o PFX, a senha e o acesso da conta do serviço. Para o A3, valide driver, middleware, token e PIN.
5. Revise proxy, firewall, inspeção HTTPS e antivírus.
6. Teste a consulta de status de outro serviço para separar um problema local de uma indisponibilidade do órgão.

**Reinstalação assistida das cadeias:**

1. Baixe o pacote [cadeiascertificado.zip disponibilizado pela Unimake](https://www.unimake.com.br/downloads/cadeiascertificado.zip) e descompacte-o em uma pasta local.
2. Faça backup do Registro do Windows e confirme a procedência dos arquivos.
3. Na subpasta `cadeias`, execute `install.bat` como administrador para instalar as cadeias fornecidas.
4. Se o problema persistir, peça à infraestrutura para avaliar o arquivo `FixCrypto_TLS1_2_Windows10.reg` antes de aplicá-lo. Alterações no Schannel afetam todas as aplicações da máquina.
5. Reinicie o computador e repita a consulta de status antes do envio do documento.

O pacote histórico também pode conter um script que habilita SSL 3.0 junto com TLS 1.2. Não execute essa opção sem revisão: mantenha SSL 3.0 desabilitado e habilite somente os protocolos necessários, preferencialmente TLS 1.2.

**Como confirmar a correção:** reinicie o Windows após alterações de cadeia, Registro ou Schannel. Execute primeiro uma consulta de status. Receber uma resposta do webservice confirma que o canal seguro foi criado; uma eventual rejeição fiscal posterior deve ser analisada como outro problema.

</details>

<details id="erro-assembly-cryptography-xml-8">
<summary><strong>“Não foi possível carregar arquivo ou assembly ‘System.Security.Cryptography.Xml, Version=8.0.0.0...’”</strong></summary>

**O que significa:** o UniNFe tentou carregar a biblioteca responsável por operações criptográficas em XML, como assinatura, e não encontrou exatamente a versão esperada ou alguma dependência dela. O nome `Version=8.0.0.0` faz parte da identidade do assembly; não significa, sozinho, que o UniNFe deva ser migrado para “.NET 8”.

**Causa provável:** dependências de versões diferentes foram misturadas, algum arquivo foi bloqueado pelo Windows, a instalação está incompleta ou o ambiente não atende aos requisitos atuais.

**Como diagnosticar:**

1. Confirme se o erro começou depois de atualização parcial, cópia manual de DLLs ou restauração de backup da pasta do aplicativo.
2. Verifique nas propriedades dos arquivos se aparece a opção **Desbloquear**.
3. Confira a quarentena do antivírus.
4. Compare a data de modificação das DLLs. Muitas datas ou versões diferentes podem indicar que arquivos de distribuições distintas foram misturados.

**Solução passo a passo:**

1. Atualize o Windows e instale o .NET Framework 4.8.1.
2. Atualize ou reinstale o UniNFe pelo pacote oficial completo.
3. Não substitua somente a DLL citada: mantenha todas as dependências da mesma distribuição.
4. Se os arquivos vieram de outro computador ou de um ZIP, abra **Propriedades** e use **Desbloquear** antes de extrair ou instalar.
5. Para desbloquear manualmente, clique com o botão direito no arquivo, selecione **Propriedades > Geral**, marque **Desbloquear**, clique em **Aplicar** e repita somente nos arquivos da distribuição oficial.
6. Para desbloqueio controlado de uma pasta já validada, um administrador pode executar:

```powershell
Get-ChildItem "C:\Caminho\Do\UniNFe" -Recurse -File | Unblock-File
```

Use o comando somente sobre a pasta exata do UniNFe e depois de confirmar a procedência dos arquivos.

Não baixe `System.Security.Cryptography.Xml.dll` isoladamente. Atualize o conjunto completo de DLLs para manter todas as dependências na mesma versão.

**Como confirmar a correção:** reinicie o UniNFe e repita uma operação que valide ou assine XML. Se outra dependência passar a ser citada, a instalação ainda pode estar incompleta; reinstale o pacote oficial inteiro.

</details>

<details id="erro-elemento-raiz-inexistente">
<summary><strong>“Elemento raiz inexistente”</strong></summary>

**O que significa:** todo XML válido precisa ter um primeiro elemento, chamado elemento raiz. Por exemplo, uma NF-e possui uma estrutura XML iniciada por um elemento próprio do documento. Um arquivo de zero bytes, cortado no meio da gravação ou contendo somente espaços não possui raiz e não pode ser interpretado.

**Causa provável:** existe um XML vazio ou incompleto na pasta `Enviados\EmProcessamento`, geralmente porque a gravação foi interrompida.

**Como localizar sem excluir arquivos:**

1. Descubra na configuração da empresa qual é a pasta de XMLs enviados. Não suponha que todas as empresas usam o mesmo caminho.
2. Pare temporariamente o envio do ERP para que nenhum arquivo seja analisado enquanto ainda está sendo gravado.
3. Liste somente os XMLs de tamanho zero, trocando o caminho pelo caminho real:

```powershell
Get-ChildItem "C:\Pasta\Enviados\EmProcessamento" -Filter *.xml -File |
    Where-Object Length -eq 0 |
    Select-Object FullName, Length, LastWriteTime
```

Esse comando apenas lista arquivos; ele não exclui nem altera nada.

**Solução passo a passo:**

1. Pare temporariamente o envio do ERP e feche o UniNFe ou interrompa o serviço.
2. Localize a pasta `Enviados\EmProcessamento` configurada para a empresa.
3. Faça uma cópia de segurança e identifique arquivos XML com tamanho zero ou conteúdo incompleto.
4. Remova somente o arquivo comprovadamente vazio ou corrompido.
5. Inicie o UniNFe e repita o processamento necessário.

Não exclua indiscriminadamente os demais XMLs de `EmProcessamento`: eles podem representar documentos enviados cuja situação ainda precisa ser consultada.

Se o arquivo possui tamanho maior que zero, abra uma cópia em editor de texto e confirme se o conteúdo começa como XML e contém um elemento de abertura e seu fechamento. Não corrija manualmente um XML fiscal já assinado, pois qualquer alteração invalida a assinatura.

**Como confirmar a correção:** inicie o UniNFe e observe se o erro deixa de ocorrer. Depois, investigue por que o ERP criou o arquivo vazio: gravação interrompida, falta de espaço, permissão, cópia pela rede ou processamento do arquivo antes de sua conclusão.

</details>

<details id="erro-handshake-tls-uma-sefaz">
<summary><strong>Windows Server: somente uma SEFAZ falha com “The SSL connection could not be established” ou `HandshakeFailure`</strong></summary>

**O que significa:** a conexão chegou ao servidor, mas a negociação TLS foi recusada antes do envio do XML. Quando apenas uma SEFAZ falha, é provável que aquela SEFAZ aceite um conjunto de protocolos ou suítes diferente dos demais endpoints usados pela máquina.

**Causa provável:** o Windows Server e aquela SEFAZ não encontraram uma combinação comum de versão TLS e suíte de cifras. O fato de outros órgãos funcionarem indica uma incompatibilidade específica de negociação, não necessariamente um defeito no certificado.

**Como diagnosticar:**

1. Guarde a exceção completa, inclusive as mensagens internas. Procure termos como `AuthenticationException`, `HandshakeFailure` e “message received was unexpected or badly formatted”.
2. Confirme que o mesmo certificado e a mesma máquina conectam a outra SEFAZ. Isso ajuda a separar certificado inválido de incompatibilidade específica do endpoint.
3. Registre a configuração atual antes de mudar Schannel, protocolos ou suítes.

**Solução passo a passo:**

1. Atualize o Windows Server e reinicie após a instalação das correções.
2. Confirme que o TLS 1.2 está habilitado no Schannel.
3. Nas versões do Windows que disponibilizam o cmdlet, execute `Get-TlsCipherSuite` no PowerShell para inventariar as suítes disponíveis e compare-as com as aceitas pela SEFAZ.
4. Se houver política corporativa personalizada, peça à infraestrutura para revisar **Configuração do Computador > Modelos Administrativos > Rede > Configurações de SSL > Ordem de Suítes de Cifras SSL**.
5. Reinicie o servidor após qualquer alteração e repita o teste.

Não copie uma lista de cifras sem validar sua compatibilidade com a versão do Windows e com os demais sistemas do servidor. A Microsoft recomenda políticas ou cmdlets próprios para essa configuração; alterações diretas na ordem pelo Registro não são suportadas e podem ser substituídas por atualizações. Consulte [Gerenciar TLS no Windows Server](https://learn.microsoft.com/windows-server/security/tls/manage-tls).

Como alternativa de diagnóstico, a infraestrutura pode usar o [IIS Crypto](https://www.nartac.com/Products/IISCrypto) para visualizar e ajustar protocolos e suítes do Schannel. Antes de aplicar mudanças, exporte ou registre a configuração atual. Mantenha o TLS 1.2 habilitado e SSL 3.0, TLS 1.0 e TLS 1.1 desabilitados. Em servidores recentes, desabilitar temporariamente o TLS 1.3 pode ser usado apenas como teste de compatibilidade com a SEFAZ afetada; restaure a configuração se não houver resultado e avalie o impacto sobre os demais serviços.

**Como confirmar a correção:** reinicie o servidor, teste primeiro a SEFAZ que falhava e depois repita uma consulta em outros órgãos usados pela empresa. A alteração só é aceitável se corrigir o endpoint afetado sem interromper os demais serviços do servidor.

</details>

Se o erro não estiver nesta relação ou persistir depois das verificações, reúna a mensagem completa, versão do UniNFe, documento/serviço, ambiente, horário da ocorrência e os logs antes de procurar [Apoio e Suporte](../referencias/apoio-e-suporte.md).
