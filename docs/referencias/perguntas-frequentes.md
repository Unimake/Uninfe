# Perguntas frequentes

Esta página reúne dúvidas comuns sobre instalação, configuração, operação e integração do UniNFe.

<details>
<summary><strong>O que fazer quando um DF-e fica parado na pasta Enviados\EmProcessamento?</strong></summary>

Quando um DF-e fica parado em `Enviados\EmProcessamento`, o ERP deve gerar uma consulta de situação para a chave do documento. Essa consulta permite que o UniNFe confirme a situação atual na SEFAZ e finalize o fluxo quando o documento estiver autorizado.

Procedimento recomendado:

1. Gere o arquivo de consulta de situação com o final `-ped-sit.xml`, ou `-ped-sit.txt` quando o serviço aceitar TXT.
2. Grave o arquivo na pasta de envio da empresa.
3. Aguarde o retorno da consulta na pasta de retorno.
4. Se a SEFAZ retornar que o documento está autorizado e o XML original estiver em `Enviados\EmProcessamento`, o UniNFe gera o XML de distribuição/processado e move os arquivos para as pastas corretas.
5. Atualize o ERP com base no retorno da consulta de situação.

Não finalize o documento no ERP apenas com base em consulta de recibo ou protocolo quando houver dúvida sobre a conclusão do processamento. A consulta de situação é a forma indicada para recuperar a situação fiscal da chave e concluir o fluxo com segurança.

Essa orientação também vale para eventos quando houver XML de evento aguardando conclusão em `Enviados\EmProcessamento`, como cancelamento, carta de correção e outros eventos compatíveis com o documento fiscal.

Para evitar vincular um protocolo ao XML errado, mantenha habilitada a validação que compara o `DigestValue` retornado pela SEFAZ com o `DigestValue` da assinatura do XML. Se houver divergência, o UniNFe retorna erro na pasta de retorno e não conclui o processamento com um protocolo incompatível.

Consulte também:

- [NFe e NFCe - Consulta de situação por arquivo](../servicos/nfe/consulta-situacao-arquivo.md)
- [CTe - Consulta de situação](../servicos/cte/consulta-situacao.md)
- [MDFe - Consulta de situação](../servicos/mdfe/consulta-situacao.md)
- [NFCom - Consulta de situação](../servicos/nfcom/consulta-situacao.md)
- [NFGas - Consulta de situação](../servicos/nfgas/consulta-situacao.md)
- [NF3e - Consulta de situação](../servicos/nf3e/consulta-situacao.md)
- [DCe - Consulta de situação](../servicos/dce/consulta-situacao.md)

</details>

<details>
<summary><strong>O DF-e foi autorizado, mas o XML de distribuição não foi gerado. Como recuperar?</strong></summary>

Quando o DF-e foi autorizado, mas o XML de distribuição/processado não foi localizado, o UniNFe pode reconstruir esse XML a partir do XML original e de uma consulta de situação.

Procedimento recomendado:

1. Localize o XML original gerado pelo ERP.
2. Copie ou mova esse XML para `Enviados\EmProcessamento`, dentro da pasta de documentos enviados da empresa.
3. Gere uma consulta de situação para a chave do documento, usando o final `-ped-sit.xml`, ou `-ped-sit.txt` quando o serviço aceitar TXT.
4. Grave a consulta na pasta de envio da empresa.
5. Aguarde o retorno da consulta.
6. Se a SEFAZ retornar que o documento está autorizado, o UniNFe usa o XML original em `Enviados\EmProcessamento` para gerar o XML de distribuição/processado e mover os arquivos para as pastas corretas.

Se o XML original estiver na pasta de XMLs com erro, mova-o para `Enviados\EmProcessamento` antes de gerar a consulta de situação. Se ele já estiver em `Enviados\EmProcessamento`, não é necessário movê-lo novamente.

Se o XML original não for localizado, o ERP deve gerar novamente o mesmo XML, sem alterar nenhuma informação. Qualquer alteração no conteúdo muda a assinatura e impede que o protocolo seja vinculado com segurança ao documento original.

Nesse caso:

1. Gere novamente o XML com o mesmo conteúdo do envio original.
2. Grave o XML na pasta de validação da empresa, não na pasta de envio.
3. Aguarde o UniNFe validar o XML.
4. Após a validação, use o XML validado e mova-o para `Enviados\EmProcessamento`.
5. Gere a consulta de situação e siga o procedimento de recuperação.

O ERP deve evitar gerar novamente o mesmo DF-e enquanto o primeiro envio ainda está sendo processado. Antes de permitir novo envio ou nova geração do XML, aguarde o retorno do processamento, consulte a situação da chave ou execute a recuperação descrita acima.

Também é recomendado manter habilitada a validação que compara o `DigestValue` retornado pela SEFAZ com o `DigestValue` da assinatura do XML. Essa validação evita que um protocolo autorizado seja associado a um XML diferente do documento original.

</details>

<details>
<summary><strong>Falhas de conexão com SEFAZ, Receita Federal ou prefeitura podem ser causadas por certificados raiz desatualizados?</strong></summary>

Sim. Em alguns ambientes, falhas de conexão com webservices da SEFAZ, Receita Federal, prefeitura, bancos ou outros serviços HTTPS podem ocorrer porque o Windows está com a cadeia de certificados raiz desatualizada ou incompleta.

Quando isso acontece, o UniNFe pode não conseguir validar corretamente o certificado do servidor acessado, mesmo que o certificado digital da empresa esteja correto. O problema costuma aparecer em mensagens relacionadas a SSL/TLS, canal seguro, cadeia de certificados, certificado não confiável ou falha de validação do certificado remoto.

Uma forma de atualizar as Autoridades Certificadoras Raiz confiáveis do Windows é gerar um pacote de certificados a partir do Windows Update e importá-lo no repositório de certificados raiz confiáveis.

Procedimento recomendado:

1. Abra o **Prompt de Comando** ou o **PowerShell** como administrador.
2. Execute o comando abaixo para gerar o arquivo `roots.sst`:

```bat
certutil -generateSSTFromWU roots.sst
```

3. Aguarde o Windows baixar a lista de certificados raiz confiáveis pela Microsoft.
4. Importe o arquivo gerado para o repositório de Autoridades de Certificação Raiz Confiáveis. Este comando só executa corretamente se o **Prompt de Comando** ou o **PowerShell** estiver aberto como administrador:

```bat
certutil -addstore -f root roots.sst
```

5. Reinicie o UniNFe ou o serviço do UniNFe, quando ele estiver executando como serviço do Windows.
6. Tente novamente a operação que estava falhando.

O arquivo `roots.sst` é um pacote de certificados raiz confiáveis. Ele não substitui o certificado digital A1 ou A3 da empresa; ele atualiza as raízes usadas pelo Windows para validar cadeias de certificados em conexões seguras.

Cuidados importantes:

- O comando precisa de acesso à internet, pois consulta a Windows Update.
- Execute em uma conta com permissão administrativa.
- Em servidores controlados por domínio, política de grupo ou equipe de infraestrutura, confirme antes se a atualização manual de raízes é permitida.
- Em ambientes críticos, faça o procedimento primeiro em uma máquina de homologação ou janela controlada.
- Se o servidor não tiver acesso à Windows Update, gere o `roots.sst` em uma máquina com acesso e avalie com a infraestrutura a forma correta de importar no servidor.
- Depois da atualização, mantenha o Windows Update e as políticas de certificados do ambiente em dia para evitar recorrência.

Se a falha continuar após atualizar as raízes, revise também proxy, firewall, data e hora do Windows, versão do TLS habilitada no sistema operacional, validade do certificado da empresa e permissões de acesso ao certificado quando o UniNFe estiver rodando como serviço.

</details>

<details>
<summary><strong>Como baixar os XMLs de NF-e emitidas contra o meu CNPJ?</strong></summary>

Para baixar XMLs de NF-e emitidas por fornecedores contra o seu CNPJ, use o serviço de distribuição DFe. O ERP deve gerar uma consulta de distribuição na pasta de envio da empresa, controlar o NSU retornado pela SEFAZ e importar os XMLs extraídos pelo UniNFe na pasta de retorno.

Fluxo recomendado:

1. Gere o arquivo de distribuição DFe com o final `-con-dist-dfe.xml` ou `-con-dist-dfe.txt`.
2. Na primeira consulta, use `ultNSU` igual a `000000000000000`.
3. Leia o retorno principal `<identificador>-dist-dfe.xml`.
4. Importe os XMLs extraídos pelo UniNFe na subpasta `Retorno\dfe`.
5. Grave no ERP o último NSU retornado pela SEFAZ.
6. Faça novas consultas usando o último NSU gravado.
7. Continue consultando até que `ultNSU` seja igual a `maxNSU`.

Quando `ultNSU` for igual a `maxNSU`, não há novos documentos disponíveis naquele momento. Aguarde o prazo recomendado pela SEFAZ antes de consultar novamente, evitando consumo indevido do serviço.

Se houver furo na sequência de NSU, por exemplo a consulta retornar `100`, `101`, `103` e `104`, consulte o NSU ausente individualmente usando `consNSU`. Isso ajuda a evitar perda de documento.

Algumas NF-e podem retornar inicialmente apenas como resumo. Quando o CNPJ consultado é destinatário da NF-e, a SEFAZ pode exigir manifestação do destinatário para liberar o XML completo. Nesse caso, envie o evento de manifestação apropriado, como `210210` para ciência da operação, e depois faça nova consulta de distribuição DFe.

Cuidados importantes:

- O controle de `ultNSU` deve ficar no ERP.
- Use `CNPJ` ou `CPF` do interessado que tem direito de consultar os documentos.
- Notas muito recentes podem demorar alguns minutos para aparecer na distribuição.
- Se a empresa ficar muito tempo sem consultar, a SEFAZ pode limitar o alcance da consulta conforme as regras vigentes do ambiente nacional.
- Guarde o retorno principal e os XMLs extraídos em `Retorno\dfe` para auditoria e reprocessamento.

Consulte também:

- [DFe - Distribuição](../servicos/dfe/distribuicao-dfe.md)
- [NFe - Eventos por arquivo](../servicos/nfe/eventos.md)

</details>

<details>
<summary><strong>Por que a consulta de documentos destinados não baixou o XML completo da NF-e?</strong></summary>

Quando a distribuição DFe não retorna o XML completo de uma NF-e emitida contra o CNPJ da empresa, verifique primeiro se a nota foi manifestada. Para muitas situações, a SEFAZ libera apenas o resumo da NF-e até que o destinatário registre uma manifestação.

Pontos a conferir:

1. Confirme se a NF-e foi manifestada pelo destinatário.
2. Se ainda não houve manifestação, envie um evento de manifestação, como `210210` para ciência da operação, quando esse for o evento adequado ao fluxo do ERP.
3. Depois da manifestação homologada, execute novamente a distribuição DFe.
4. Importe os XMLs extraídos na subpasta `Retorno\dfe`.
5. Mantenha o controle de `ultNSU` no ERP para continuar a sequência correta das consultas.

Também existem limitações do ambiente nacional que podem impedir o download de documentos antigos ou anteriores ao início do uso do serviço:

- Se o CNPJ nunca usou a distribuição DFe, a SEFAZ pode disponibilizar apenas documentos emitidos a partir da primeira consulta.
- Se o CNPJ ficar muito tempo sem usar o serviço, a SEFAZ pode limitar novamente o histórico disponível.
- Documentos muito antigos podem não estar mais disponíveis para download pela distribuição.
- NF-e muito recente pode demorar alguns minutos para aparecer, dependendo da sincronização entre a SEFAZ de origem e o ambiente nacional.

Quando a NF-e não aparecer mesmo após manifestação e nova consulta, aguarde a sincronização e repita a distribuição DFe. Se a nota continuar ausente, avalie contato com a SEFAZ de origem para confirmar se o documento foi sincronizado com o ambiente nacional.

Consulte também:

- [DFe - Distribuição](../servicos/dfe/distribuicao-dfe.md)
- [NFe - Eventos por arquivo](../servicos/nfe/eventos.md)

</details>

<details>
<summary><strong>Como baixar os XMLs de CT-e de meu interesse?</strong></summary>

Para baixar XMLs de CT-e disponibilizados para o seu CNPJ ou CPF, use o serviço de distribuição DFe de CT-e. O ERP deve gerar uma consulta na pasta de envio da empresa, controlar o NSU retornado pela SEFAZ e importar os XMLs extraídos pelo UniNFe na pasta de retorno.

Fluxo recomendado:

1. Gere o arquivo de distribuição DFe de CT-e com o final `-con-dist-dfecte.xml` ou `-con-dist-dfecte.txt`.
2. Na primeira consulta, use `ultNSU` igual a `000000000000000`.
3. Leia o retorno principal `<identificador>-dist-dfecte.xml`.
4. Importe os XMLs extraídos pelo UniNFe na subpasta `Retorno\dfe`.
5. Grave no ERP o último NSU retornado pela SEFAZ.
6. Faça novas consultas usando o último NSU gravado.
7. Continue consultando até que `ultNSU` seja igual a `maxNSU`.

Quando `ultNSU` for igual a `maxNSU`, não há novos CT-e disponíveis naquele momento. Aguarde o prazo recomendado pela SEFAZ antes de consultar novamente, evitando consumo indevido do serviço.

Se houver furo na sequência de NSU, consulte o NSU ausente individualmente usando `consNSU`.

Cuidados importantes:

- O controle de `ultNSU` deve ficar no ERP.
- Use `CNPJ` ou `CPF` do interessado que tem direito de consultar os CT-e.
- Notas muito recentes podem demorar alguns minutos para aparecer na distribuição.
- Guarde o retorno principal e os XMLs extraídos em `Retorno\dfe` para auditoria e reprocessamento.

Consulte também:

- [DFe - Distribuição de CT-e](../servicos/dfe/distribuicao-dfe-cte.md)

</details>
