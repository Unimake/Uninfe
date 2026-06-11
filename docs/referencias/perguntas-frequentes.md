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
