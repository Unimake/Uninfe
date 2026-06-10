# Tela Consulta ao Cadastro de Contribuinte

A tela **Consulta ao cadastro de contribuinte** permite consultar dados cadastrais de um contribuinte nos serviços fiscais, usando o certificado digital de uma empresa cadastrada no UniNFe.

Use esta tela quando precisar verificar dados como CNPJ ou CPF, inscrição estadual, razão social, endereço, situação cadastral e informações fiscais retornadas pela consulta.

## Acesso

No menu principal, clique em **Cadastro de contribuinte**.

O botão fica disponível quando há empresa de NF-e cadastrada. Empresas configuradas somente para NFS-e não usam este serviço.

## Como consultar

1. Selecione a empresa que será usada para a consulta.
2. Confira a UF para onde a consulta será enviada.
3. Selecione o tipo de conteúdo da pesquisa: **CNPJ**, **CPF** ou **IE**.
4. Informe o conteúdo correspondente.
5. Clique em **Consultar**.

Enquanto a consulta está em andamento, o campo **Status** mostra que o UniNFe está consultando o servidor. Quando há retorno com cadastro encontrado, a tela de resultado é exibida em abas.

## Campos da consulta

| Campo | Para que serve |
|---|---|
| Utilizar para consulta o certificado da empresa | Define qual empresa cadastrada será usada na consulta. O certificado digital e as configurações dessa empresa são usados para enviar a solicitação. |
| UF para o envio da consulta | Define a unidade federativa para onde a consulta será enviada. Ao selecionar a empresa, o UniNFe preenche a UF conforme a configuração da empresa. |
| Versão | Mostra a versão do serviço usada na consulta. Na tela, este campo é exibido desabilitado. |
| Conteúdo para pesquisa | Informa o dado que será consultado. O conteúdo deve corresponder ao tipo selecionado: CNPJ, CPF ou IE. |
| CNPJ | Indica que o conteúdo informado é um CNPJ. O campo aceita até 14 dígitos. |
| CPF | Indica que o conteúdo informado é um CPF. O campo aceita até 11 dígitos. |
| IE | Indica que o conteúdo informado é uma inscrição estadual. O campo aceita até 20 caracteres. |
| Status | Exibe mensagens da consulta, como andamento, retorno sem cadastro ou erro recebido durante o processamento. |

## Botões

| Botão | Para que serve |
|---|---|
| Consultar | Envia a consulta usando a empresa, UF e conteúdo informados. |
| Voltar | Na tela de resultado, retorna para a tela de consulta. |

## Resultado da consulta

Quando a consulta retorna um ou mais cadastros, o UniNFe abre a tela de resultado. Cada cadastro retornado aparece em uma aba, identificada como **Consulta-1**, **Consulta-2** e assim por diante.

Os campos do resultado são somente para leitura.

| Campo | O que apresenta |
|---|---|
| CNPJ ou CPF | Documento do contribuinte retornado pela consulta. |
| I.E. | Inscrição estadual retornada. |
| Nome | Nome ou razão social do contribuinte. |
| Fantasia | Nome fantasia, quando retornado. |
| Endereço | Logradouro do endereço cadastral. |
| Número | Número do endereço. |
| Complemento | Complemento do endereço. |
| Bairro | Bairro do endereço. |
| Cidade | Município do endereço. |
| Estado | UF do endereço. |
| CEP | CEP do endereço. |
| Data de ocorrência da baixa | Data de baixa cadastral, quando retornada. |
| CNAE | Código da atividade econômica retornado. |
| Data da última modificação da situação cadastral | Data da última alteração da situação cadastral. |
| Data de Início da atividade | Data de início da atividade do contribuinte. |
| Regime de Apuração do ICMS | Regime de apuração do ICMS retornado pela consulta. |
| I.E. Única | Indicação ou informação de inscrição estadual única, quando retornada. |
| I.E. Atual | Inscrição estadual atual, quando retornada. |
| Observações | Situação ou observação cadastral retornada pelo serviço. |

## Mensagens e validações

Se o conteúdo para pesquisa não for informado, o UniNFe exibe a mensagem **Conteúdo para pesquisa deve ser informado**.

Quando o serviço não retorna cadastro, o campo **Status** mostra o motivo informado no retorno da consulta.

Se ocorrer erro durante a consulta, a mensagem é apresentada no campo **Status** para orientar a correção.
