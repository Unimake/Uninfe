# Perguntas frequentes

Esta página reúne dúvidas comuns sobre instalação, configuração, operação e integração do UniNFe.

<details id="faq-dfe-parado-em-processamento">
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

<details id="faq-recuperar-xml-distribuicao-nao-gerado">
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

<details id="faq-baixar-xml-nfe-destinado">
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

<details id="faq-nfe-sem-xml-completo-na-distribuicao">
<summary><strong>Por que a consulta de documentos destinados não baixou o XML completo da NF-e?</strong></summary>

Quando a distribuição DFe não retorna o XML completo de uma NF-e emitida contra o CNPJ da empresa, isso normalmente ocorre por uma destas causas:

- a NF-e ainda não recebeu uma manifestação que libere o documento completo para o destinatário;
- o interessado começou a usar o serviço recentemente ou ficou mais de 60 dias sem consultar por `distNSU`;
- o documento já está fora do período de disponibilidade da distribuição;
- a NF-e ainda não foi sincronizada pela SEFAZ de origem com o Ambiente Nacional;
- o ERP leu apenas o retorno principal e não importou os XMLs extraídos em `Retorno\dfe`.

### Primeiro, identifique o que foi retornado

Abra o retorno `<identificador>-dist-dfe.xml` e verifique `cStat`, `xMotivo`, `ultNSU`, `maxNSU` e o schema de cada `docZip`:

| Informação | O que significa |
|---|---|
| `resNFe` | Resumo da NF-e. O XML completo ainda não foi distribuído nessa consulta. |
| `procNFe` | NF-e completa com o protocolo de autorização. O UniNFe extrai esse documento em `Retorno\dfe`. |
| `cStat=137` | Nenhum documento foi localizado nessa consulta. Quando não houver mais documentos, aguarde pelo menos uma hora antes de consultar novamente. |
| `cStat=138` | Há documentos localizados no lote de distribuição. Importe os XMLs extraídos em `Retorno\dfe`. |
| `cStat=656` | Consumo indevido. Interrompa as consultas pelo período indicado pela SEFAZ; novas tentativas antes do prazo podem reiniciar a contagem do bloqueio. |

O retorno principal informa o resultado da consulta, mas os documentos descompactados ficam na subpasta `Retorno\dfe`. Não conclua que o XML está ausente antes de verificar essa pasta.

### Se foi retornado somente o resumo

Antes da manifestação, o Ambiente Nacional pode disponibilizar ao destinatário apenas o resumo da NF-e. Para liberar o XML completo, envie uma manifestação compatível com a situação real da operação, como:

| Código | Manifestação | Quando usar |
|---|---|---|
| `210210` | Ciência da operação | Quando o destinatário tomou conhecimento da NF-e, mas ainda precisa avaliar a operação. |
| `210200` | Confirmação da operação | Quando a operação ocorreu e deve ser confirmada. |
| `210240` | Operação não realizada | Quando a operação era conhecida, mas não foi realizada; exige a justificativa prevista no leiaute. |

Não manifeste automaticamente todas as NF-e apenas para obter o XML. O ERP deve escolher o evento de acordo com a situação fiscal e operacional do documento.

Depois que o evento for homologado:

1. Aguarde a sincronização do evento com o Ambiente Nacional.
2. Consulte novamente usando a sequência correta de `ultNSU`, respeitando o intervalo informado pela SEFAZ.
3. Leia o novo retorno principal.
4. Importe o `procNFe` extraído pelo UniNFe em `Retorno\dfe`.

### Limites de histórico e continuidade

Os documentos e resumos ficam disponíveis para distribuição por até **90 dias após a recepção pelo Ambiente Nacional da NF-e**. Esse prazo não deve ser contado somente pela data de emissão impressa no documento.

A geração de NSU também considera a continuidade de uso de `distNSU`:

- Para um novo usuário do serviço, a geração de NSU começa a partir do primeiro acesso; não são criados NSU retroativos.
- Se o interessado ficar mais de 60 dias sem consultar por `distNSU`, a geração de NSU é interrompida e retomada a partir da próxima consulta; o período sem geração não é recuperado retroativamente.
- Nessas duas situações, o primeiro acesso pode retornar `cStat=137`. Aguarde pelo menos uma hora antes da consulta seguinte para não provocar consumo indevido.

Por isso, o ERP deve executar a distribuição continuamente e armazenar o `ultNSU` retornado. Não reinicie todas as consultas com `ultNSU` igual a zero e não mantenha aplicações independentes avançando a sequência do mesmo CNPJ sem coordenação.

### Se a NF-e ainda não aparecer

1. Confirme que o CNPJ ou CPF consultado é um ator autorizado a receber o documento.
2. Verifique se o certificado digital permite consultar o interessado informado. Para pessoa jurídica, o CNPJ-base consultado deve ser o mesmo do certificado.
3. Confirme a homologação da manifestação e a chave de acesso utilizada no evento.
4. Continue a consulta a partir do último `ultNSU` efetivamente processado até que `ultNSU` seja igual a `maxNSU`.
5. Se conhecer a chave de acesso e o documento ainda estiver no período de disponibilidade, faça uma consulta pontual por `consChNFe`. Se conhecer um NSU faltante, use `consNSU`.
6. Considere que a ordem de distribuição segue a recepção no Ambiente Nacional, não necessariamente a ordem de emissão das NF-e.
7. Se uma NF-e recente continuar ausente, aguarde a sincronização e consulte novamente após o intervalo permitido.
8. Persistindo a ausência, contate a SEFAZ de origem para confirmar a sincronização com o Ambiente Nacional.

O primeiro uso e a interrupção superior a 60 dias impedem a geração retroativa de NSU para a consulta sequencial, mas uma consulta pontual pode recuperar um documento conhecido que ainda esteja disponível. Se o XML não for retornado nem pela consulta pontual ou já estiver fora do período de disponibilidade, solicite-o ao emitente ou obtenha-o por outro canal fiscal autorizado. A distribuição DFe não deve ser usada como única forma de guarda dos XMLs da empresa.

Consulte também:

- [DFe - Distribuição](../servicos/dfe/distribuicao-dfe.md)
- [NFe - Eventos por arquivo](../servicos/nfe/eventos.md)

</details>

<details id="faq-baixar-xml-cte-distribuicao">
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

<details id="faq-gerar-idcsrt-hashcsrt-nfe-nfce">
<summary><strong>Como gerar as tags idCSRT e hashCSRT da NF-e ou NFC-e com o UniNFe?</strong></summary>

O `idCSRT` e o `hashCSRT` podem ser informados pelo ERP ou gerados pelo UniNFe. Para evitar XML incompleto, escolha uma das duas formas e envie ou configure **todos os dados do responsável técnico**.

| Cenário | Comportamento do UniNFe |
|---|---|
| O ERP envia `CNPJ`, `xContato`, `email`, `fone`, `idCSRT` e `hashCSRT` | O UniNFe utiliza o grupo enviado pelo ERP. Não é necessário repetir esses dados na configuração da empresa. |
| O ERP envia o grupo completo, mas o conteúdo de `hashCSRT` ainda não está no formato Base64 esperado | O UniNFe converte o conteúdo automaticamente. Para o resultado ser correto, o conteúdo informado deve corresponder ao **CSRT concatenado com a chave de acesso da NF-e ou NFC-e**. |
| O ERP envia o grupo do responsável técnico sem `idCSRT`, sem `hashCSRT` ou com outros campos faltando | O UniNFe não combina esse grupo parcial com a configuração da empresa. O XML pode ser rejeitado na validação. |
| O ERP não envia o grupo do responsável técnico e todos os campos estão preenchidos no UniNFe | O UniNFe cria o grupo completo e calcula o `hashCSRT` com o CSRT configurado e a chave de acesso do documento. |
| O ERP não envia o grupo e a configuração do UniNFe está vazia ou incompleta | Não há dados suficientes para gerar corretamente o responsável técnico. Preencha a configuração completa ou faça o ERP enviar o grupo completo. |

### Como configurar a geração pelo UniNFe

1. Acesse **Configurações > Empresas**.
2. Selecione a empresa e abra a aba **Responsável Técnico**.
3. Preencha **CNPJ**, **Contato**, **E-mail**, **Telefone**, **ID CSRT** e **CSRT**.
4. Salve a configuração.
5. Faça o ERP omitir todo o grupo `infRespTec` do XML. Durante a preparação do documento, o UniNFe incluirá o grupo e calculará o `hashCSRT`.

Preencher somente **ID CSRT** e **CSRT** não é suficiente. Os dados cadastrais do responsável técnico também precisam estar completos.

### Como gerar pelo ERP

O ERP deve enviar o grupo `infRespTec` completo. O `hashCSRT` é o resultado do SHA-1 aplicado à concatenação do CSRT com a chave de acesso do documento, convertido para Base64:

```text
hashCSRT = Base64(SHA-1(CSRT + chave de acesso))
```

Como a chave de acesso participa do cálculo, o hash deve ser gerado novamente para cada NF-e ou NFC-e. Não reutilize o `hashCSRT` de outro documento.

### O que conferir quando ocorrer erro

1. Confirme se o ERP enviou o grupo `infRespTec`. Se enviou, verifique se todos os campos necessários estão no próprio XML.
2. Se o ERP não enviou o grupo, confira se todos os campos da aba **Responsável Técnico** estão preenchidos para a empresa correta.
3. Confirme se o **ID CSRT** e o **CSRT** são os valores fornecidos pela SEFAZ para o responsável técnico.
4. Se o ERP calculou o hash, confira se foi usada a chave de acesso do mesmo documento.
5. Valide novamente o XML antes do envio.

O CSRT é um código de segurança. Não o publique em capturas de tela, logs ou chamados de suporte. Quando precisar demonstrar o problema, envie o XML com esse dado protegido e informe apenas se a geração ficou sob responsabilidade do ERP ou do UniNFe.

Consulte também:

- [Tela Configurações — aba Responsável Técnico](../configuracao/telas-cadastros-configuracoes.md)
- [Tela Validar XML](tela-validar-xml.md)

</details>

<details id="faq-impressao-ibs-cbs-danfe">
<summary><strong>Como os novos tributos IBS e CBS devem aparecer no DANFE?</strong></summary>

O XML da NF-e e o DANFE obedecem a especificações diferentes. O fato de o XML possuir os grupos de IBS e CBS não significa que esses campos possam ser acrescentados livremente ao documento impresso: o DANFE deve seguir o leiaute oficial aplicável ao tipo de impressão utilizado.

Para o **DANFE convencional da NF-e**, a [Nota Técnica 2025.002 da NF-e/NFC-e](https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=04BIflQt1aY%3D) informa, na versão vigente consultada, que as alterações destinadas a exibir os novos tributos ainda estão em estudo e serão publicadas em uma versão futura. Por isso, o UniDANFE deve incorporar essas informações somente depois que o Portal Nacional da NF-e publicar a definição oficial correspondente.

Isso não dispensa o preenchimento correto do XML. O ERP deve continuar gerando os grupos de IBS, CBS e Imposto Seletivo conforme a legislação, o schema e as regras de validação vigentes, mesmo quando a representação impressa ainda não mostrar todos esses dados.

### Atenção ao DANFE Simplificado Tipo 2

O **DANFE Simplificado Tipo 2** possui uma especificação própria. A [Nota Técnica 2026.003](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=2YdaBTIql%2Bk%3D) prevê uma divisão para os valores de CBS, IBS e Imposto Seletivo quando existirem no documento.

Essa regra não deve ser aplicada automaticamente ao DANFE convencional. Para identificar corretamente o formato:

| Situação | O que observar |
|---|---|
| DANFE convencional de NF-e | Aguarde e siga o leiaute oficial específico. Não crie campos ou posições por conta própria. |
| DANFE Simplificado Tipo 2 | Na NF-e, o XML usa `tpImp=6`. A impressão deve seguir a especificação própria desse formato. |
| Contingência Off-line com NF-e e DANFE Simplificado Tipo 2 | Além de `tpImp=6`, o XML usa `tpEmis=9` e deve ser transmitido posteriormente para autorização. |

### O que conferir quando IBS ou CBS não aparecerem na impressão

1. Abra o XML autorizado, normalmente o arquivo `-procNFe.xml`, e confirme se os grupos e valores de IBS/CBS estão presentes. A impressão não cria informações tributárias ausentes no XML.
2. Confira o modelo do documento e o valor de `tpImp` para saber se a impressão é convencional ou DANFE Simplificado Tipo 2.
3. Consulte a versão mais recente da nota técnica no Portal Nacional da NF-e. Não use somente uma cópia antiga salva localmente.
4. Mantenha o UniDANFE atualizado para uma versão compatível com a especificação oficial aplicável.
5. Gere novamente a impressão após a atualização, usando o mesmo XML autorizado, e compare o resultado com o leiaute publicado.

Não inclua manualmente valores de IBS ou CBS em posições improvisadas do DANFE para tentar antecipar uma regra futura. Se houver uma exigência fiscal específica para a operação, confirme-a com o responsável tributário e confronte-a com a documentação oficial vigente.

Consulte também:

- [Reforma Tributária - NFe/NFCe](reforma-tributaria-nfe-nfce.md)
- [Contingência Off-line](../contingencia/off-line.md)
- [Notas técnicas e notícias para desenvolvedores](notas-tecnicas-e-noticias-para-desenvolvedores.md)

</details>

<details id="faq-documentacao-modelos-xml-nfse-nacional">
<summary><strong>Onde encontro a documentação da NFS-e do Ambiente Nacional e os modelos de XML para o UniNFe?</strong></summary>

Use duas fontes em conjunto:

- [Documentação Atual da NFS-e Nacional](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual/documentacao-atual): contém os manuais, anexos de leiaute, regras de negócio e esquemas XSD vigentes em produção.
- [Modelos XML da NFS-e Nacional 1.01 para o UniNFe](https://www.unimake.com.br/uninfe/modelos.php?p=NFSe%2FNACIONAL%2F1.01): contém exemplos de arquivos com os sufixos reconhecidos pelo UniNFe para emissão, consulta, cancelamento, distribuição e eventos.

Consulte sempre a página principal da documentação oficial antes de baixar os arquivos. O nome e a data do anexo podem mudar quando o Ambiente Nacional publica uma revisão, enquanto a página **Documentação Atual** permanece como ponto de entrada para a versão vigente.

### Qual arquivo oficial devo consultar?

Na página oficial, procure estes materiais:

| Material | Para que serve |
|---|---|
| **ANEXO I — SEFIN/ADN — DPS/NFS-e** | Define os campos, grupos, tipos, tamanhos, ocorrências e regras de negócio da DPS e da NFS-e. |
| **Esquemas XSD da NFS-e** | Validam a estrutura XML, namespace, ordem e formato dos elementos. |
| **ANEXO II — Pedido de Registro de Evento/Eventos** | Define estruturas e regras dos eventos da NFS-e Nacional. |
| **Anexos de domínio** | Fornecem códigos oficiais usados em campos como município, serviço, NBS e indicador da operação. |

A versão 1.01 contempla os grupos relacionados à Reforma Tributária, mas os anexos e as regras podem receber revisões sem mudança do número principal da versão. Confira a data do arquivo antes de implementar ou corrigir o XML.

Para testes, use a [Documentação Técnica de homologação/produção restrita](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/producao-restrita). Não misture planilha ou XSD de homologação com envio para produção, nem arquivos de produção com um ambiente de testes que esteja em outra revisão.

### Como interpretar a planilha do Anexo I?

As abas mais úteis são:

- **LEIAUTE DPS_NFS-e**: apresenta a árvore de campos da DPS e da NFS-e, com caminho, descrição, tipo, tamanho e quantidade permitida. Para localizar a parte da DPS dentro da estrutura documentada, pesquise por `NFSe/infNFSe/DPS` em vez de depender de um número fixo de linha, pois as linhas mudam entre revisões.
- **RN DPS_NFS-e**: reúne as regras de negócio e os códigos de rejeição. Pesquise pelo código retornado e leia a condição completa, os campos envolvidos e o ambiente em que a regra se aplica.

O XSD e a aba de leiaute confirmam a estrutura, mas não substituem as regras de negócio. Um XML pode estar válido no XSD e ainda ser rejeitado por município, cadastro, tributação, data, ambiente ou combinação de campos.

### Qual modelo XML devo usar?

Escolha o arquivo correspondente à operação que o ERP realmente executará:

| Necessidade | Modelo de referência |
|---|---|
| Emissão com os campos essenciais | `GerarNfseMinima-env-loterps.xml` |
| Emissão com mais grupos preenchidos | `GerarNFSeEnvio-env-loterps.xml` |
| Referência dos grupos disponíveis na DPS | `dps_com_todas_as_tags-env-loterps.xml` |
| Cancelamento | `CancelarNFSe-ped-cannfse.xml` |
| Consulta da NFS-e | `ConsultarNFSeEnvio-ped-sitnfse.xml` |
| Consulta por DPS/RPS | `ConsultarNFSeRPS-ped-sitnfserps.xml` |
| Consulta da distribuição por NSU | `ConsultarDistribuicaoNFSeNSU-cons-nsunfse.xml` |
| Registro de evento | arquivos com final `-ped-regev.xml` |

O arquivo `dps_com_todas_as_tags-env-loterps.xml` é uma referência para localizar grupos e campos. Não envie esse modelo inteiro sem avaliar as ocorrências, escolhas e regras aplicáveis à operação; alguns grupos são condicionais ou incompatíveis entre si.

### Como adaptar um modelo sem gerar novas rejeições

1. Confirme que a empresa está configurada no padrão **NACIONAL** e no ambiente correto.
2. Baixe a planilha, os XSD e o modelo da mesma versão e ambiente.
3. Escolha o modelo da operação e preserve o sufixo de arquivo esperado pelo UniNFe.
4. Substitua todos os dados demonstrativos: identificadores, documentos, municípios, datas, séries, números, códigos de serviço, valores e informações tributárias.
5. Recalcule identificadores compostos, como o `Id` da DPS, em vez de copiar o valor do exemplo.
6. Remova grupos opcionais que não se aplicam e respeite a ordem dos elementos definida no XSD.
7. Valide o XML e, se houver rejeição, procure o código na aba **RN DPS_NFS-e**.
8. Compare a regra encontrada com os dados reais da operação e com as parametrizações do município antes de reenviar.

Não corrija uma rejeição apenas copiando uma tag de outro exemplo. Primeiro confirme se o campo é permitido, obrigatório ou incompatível no cenário fiscal da DPS.

Consulte também:

- [Reforma Tributária - NFS-e](reforma-tributaria-nfse.md)
- [Tela Validar XML](tela-validar-xml.md)

</details>

<details id="faq-retencao-pis-cofins-csll-nfse-sp-layout-2">
<summary><strong>Como informar a retenção de PIS, COFINS e CSLL na NFS-e de São Paulo no layout 2 sem reduzir o valor do serviço?</strong></summary>

O **valor total do serviço permanece sendo o valor contratado**. As retenções de PIS, COFINS e CSLL não reduzem esse valor no XML: elas reduzem somente o valor financeiro pago diretamente pelo tomador ao prestador.

No exemplo de um serviço contratado por **R$ 435,00**, o cálculo fica assim:

| Informação | Alíquota | Valor |
|---|---:|---:|
| Valor contratado da NFS-e | — | **R$ 435,00** |
| PIS retido | 0,65% | R$ 2,83 |
| COFINS retida | 3,00% | R$ 13,05 |
| CSLL retida | 1,00% | R$ 4,35 |
| **Total das retenções** | **4,65%** | **R$ 20,23** |
| **Valor líquido a pagar** | — | **R$ 414,77** |

> **Atenção:** a soma das alíquotas é **4,65%**, e não 4,35%.

### Por que a NFS-e continua com o valor de R$ 435,00?

A retenção funciona como uma divisão do pagamento:

```text
R$ 414,77  pagos diretamente ao prestador
R$  20,23  retidos e recolhidos pelo tomador
----------
R$ 435,00  valor total do serviço contratado
```

Portanto, o prestador continua faturando **R$ 435,00**. A diferença é que ele recebe **R$ 414,77** diretamente do cliente, enquanto **R$ 20,23** são retidos pelo tomador para o recolhimento tributário correspondente.

O valor líquido não deve substituir o valor bruto do serviço no XML. Primeiro existe o valor contratado; depois, as retenções são calculadas sobre ele e descontadas apenas do pagamento.

### Qual tag representa o valor do serviço no layout 2?

No layout 2 da Prefeitura de São Paulo não existe mais a tag `ValorServicos`. O manual orienta utilizar `ValorInicialCobrado` **ou** `ValorFinalCobrado`:

> “O campo `<ValorServicos>` não existe na versão 2. Utilizar o valor do elemento `<ValorInicialCobrado>` ou `<ValorFinalCobrado>`.”

Essas tags formam uma escolha no leiaute: deve ser informada **uma ou outra**, e não as duas.

- `ValorInicialCobrado`: use quando o valor informado representa o preço antes dos tributos que serão acrescentados.
- `ValorFinalCobrado`: use quando o valor informado já representa o total final contratado, incluindo os tributos que compõem o preço.

Se **R$ 435,00** é o preço final definido no contrato e exibido na nota, o preenchimento coerente é:

```xml
<ValorFinalCobrado>435.00</ValorFinalCobrado>
```

Não informe o valor líquido como se fosse o valor do serviço:

```xml
<!-- Incorreto para este cenário -->
<ValorFinalCobrado>414.77</ValorFinalCobrado>
```

Isso confundiria o **valor do serviço** com o **valor líquido do pagamento**.

Mesmo quando a formação contratual do preço exigir `ValorInicialCobrado`, a retenção não transforma R$ 435,00 em R$ 414,77. A escolha entre valor inicial e valor final está ligada à formação do preço e ao cálculo dos tributos, não ao desconto financeiro das retenções.

### Como informar PIS, COFINS e CSLL no XML?

Desde **14/05/2026**, a Prefeitura de São Paulo determina a seguinte sistemática para os leiautes 1 e 2:

- `ValorPIS`: valor total do PIS sobre a operação;
- `ValorCOFINS`: valor total da COFINS sobre a operação;
- `ValorCSLL`: apesar do nome da tag, recebe a **soma dos valores retidos de PIS, COFINS e CSLL** na emissão via Web Service;
- `RetencaoPisCofins`: identifica quais contribuições foram retidas.

Como as três contribuições foram retidas no exemplo, utilize:

```xml
<RetencaoPisCofins>3</RetencaoPisCofins>
```

O código `3` significa *PIS, COFINS e CSLL retidos*.

Consulte a orientação oficial da Prefeitura: [Alteração na emissão de NFS-e: nova sistemática de indicação de tributos federais nos leiautes 1 e 2](https://notadomilhao.sf.prefeitura.sp.gov.br/noticias/alteracao-na-emissao-de-nfs-e-nova-sistematica-de-indicacao-de-tributos-federais-nos-leiautes-1-e-2/). Para conferir a versão mais recente do leiaute, consulte também os [manuais da Nota Fiscal Paulistana](https://notadomilhao.sf.prefeitura.sp.gov.br/manuais/).

### Exemplo de fragmento XML

O trecho abaixo considera que:

- R$ 435,00 é o preço final contratado;
- PIS, COFINS e CSLL estão sujeitos à retenção;
- não há INSS, IR, multa, juros nem deduções legais;
- as alíquotas são 0,65% de PIS, 3,00% de COFINS e 1,00% de CSLL.

```xml
<RPS>
    <Assinatura>ASSINATURA_DO_RPS</Assinatura>

    <ChaveRPS>
        <InscricaoPrestador>INSCRICAO_MUNICIPAL</InscricaoPrestador>
        <SerieRPS>SERIE</SerieRPS>
        <NumeroRPS>NUMERO</NumeroRPS>
    </ChaveRPS>

    <TipoRPS>RPS</TipoRPS>
    <DataEmissao>2026-08-05</DataEmissao>
    <StatusRPS>N</StatusRPS>
    <TributacaoRPS>T</TributacaoRPS>

    <!-- A retenção não é uma dedução do valor do serviço. -->
    <ValorDeducoes>0.00</ValorDeducoes>

    <!-- Valores totais sobre a operação. -->
    <ValorPIS>2.83</ValorPIS>
    <ValorCOFINS>13.05</ValorCOFINS>
    <ValorINSS>0.00</ValorINSS>
    <ValorIR>0.00</ValorIR>

    <!-- PIS 2,83 + COFINS 13,05 + CSLL 4,35 = 20,23. -->
    <ValorCSLL>20.23</ValorCSLL>

    <CodigoServico>CODIGO_SERVICO_SP</CodigoServico>
    <AliquotaServicos>ALIQUOTA_ISS</AliquotaServicos>
    <ISSRetido>false</ISSRetido>

    <!-- Demais dados do tomador e do serviço. -->

    <!-- Valor total contratado, antes das retenções. -->
    <ValorFinalCobrado>435.00</ValorFinalCobrado>

    <!-- Código 3: PIS, COFINS e CSLL retidos. -->
    <RetencaoPisCofins>3</RetencaoPisCofins>

    <!-- Demais campos obrigatórios do layout 2:
         NBS, local da prestação, IBS/CBS etc. -->
</RPS>
```

Esse é um **fragmento demonstrativo**. Para validá-lo contra o XSD, complete o documento conforme a versão vigente do leiaute, respeite a ordem dos elementos e informe os dados reais do prestador, do tomador, do serviço, da NBS, do local da prestação, do IBS/CBS, da discriminação e da assinatura.

### O que deve aparecer na impressão?

O resultado conceitualmente correto é:

| Informação | Valor |
|---|---:|
| Valor total do serviço/NFS-e | **R$ 435,00** |
| Contribuições sociais retidas | **R$ 20,23** |
| Valor líquido a receber | **R$ 414,77** |

A Prefeitura também informou que o espelho da NFS-e foi alterado para apresentar os campos **Contribuições Sociais – Retidas** e **Descrição Contribuições Sociais – Retidas**.

### Resumo do preenchimento

```text
Valor do serviço/valor final cobrado: R$ 435,00
Valor das deduções:                  R$   0,00
Contribuições sociais retidas:       R$  20,23
Valor líquido do pagamento:          R$ 414,77
```

Portanto, **não envie R$ 414,77 como valor total do serviço**. Esse é apenas o valor líquido restante depois das retenções.

> **Ressalva fiscal:** confirme com o contador ou com o setor fiscal se PIS, COFINS e CSLL realmente devem ser retidos para o prestador, o tomador e o serviço envolvidos. O exemplo demonstra o preenchimento quando a retenção total de 4,65% já foi corretamente determinada.

</details>
