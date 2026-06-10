# Consulta Status De Serviço

## O que é

A tela **Pedido de situação de serviços** consulta a disponibilidade dos webservices fiscais usados pelo UniNFe. Ela permite verificar se o serviço da SEFAZ ou do autorizador está respondendo para uma empresa, UF, ambiente, tipo de emissão, versão e modelo de documento.

Use esta tela quando houver dúvida se uma falha de envio está relacionada ao UniNFe, à configuração da empresa, à internet local ou à indisponibilidade do serviço autorizador.

## Onde acessar

No menu principal, acesse a opção de **Situação dos serviços**.

## Serviços consultados

A consulta está disponível para:

| Serviço | Versão usada na consulta |
|---|---|
| NF-e | 4.00 |
| NFC-e | 4.00 |
| CT-e | 4.00 |
| MDF-e | 3.00 |
| NF3-e | 1.00 |
| NFCom | 1.00 |
| NFGas | 1.00 |

Quando a empresa estiver configurada como **Todos**, a tela consulta NF-e, NFC-e, MDF-e e CT-e para a empresa selecionada.

## Campos

| Campo | O que faz | Como preencher |
|---|---|---|
| Utilizar para consulta o certificado da empresa | Seleciona a empresa usada na consulta. A opção **Todos** executa a consulta para todas as empresas cadastradas compatíveis. | Escolha a empresa que deseja testar ou selecione **Todos** para uma visão geral. |
| UF para o envio da consulta | Define a UF usada na consulta do status do serviço. | Normalmente é preenchida conforme a empresa selecionada. Altere somente se precisar consultar outra UF. |
| Versão | Define a versão do leiaute usada na consulta. | A tela ajusta a versão conforme o serviço selecionado. Use a versão sugerida pela tela. |
| Ambiente | Define se a consulta será feita no ambiente de produção ou homologação. | Confirme se o ambiente corresponde ao ambiente configurado para a empresa ou ao cenário que deseja testar. |
| CNPJ | Exibe o documento da empresa selecionada. | Campo informativo; não é editado nesta tela. |
| Tipo de emissão | Define o tipo de emissão usado na consulta. | Use **Normal** na operação comum. Use contingência somente quando o modelo do documento permitir. |
| Serviço | Define o modelo fiscal consultado. | Selecione o serviço desejado, como NF-e, NFC-e, CT-e, MDF-e, NF3-e, NFCom ou NFGas. |

## Como consultar

1. Selecione a empresa ou a opção **Todos**.
2. Confira a UF, versão, ambiente, tipo de emissão e serviço.
3. Clique em **Consultar**.
4. Aguarde a conclusão da consulta.
5. Leia o resultado na grade.
6. Clique em uma linha da grade para ver a mensagem completa no campo inferior.

## Resultado

A grade exibe uma linha para cada consulta realizada.

| Coluna | O que mostra |
|---|---|
| Empresa | Empresa consultada. |
| UF | UF usada na consulta. |
| Tipo de Serviço | Modelo fiscal consultado. |
| Situação do Serviço | Mensagem retornada pelo webservice ou mensagem de erro capturada durante a consulta. |

O campo de mensagem abaixo da grade mostra o texto completo da situação selecionada.

## Validações e mensagens

A tela valida combinações de serviço e tipo de emissão antes de consultar:

| Situação | Mensagem exibida |
|---|---|
| CT-e com contingência SVCAN | `CT-e não dispõe do tipo de contingência SVCAN.` |
| NF-e com contingência SVCSP | `NF-e não dispõe do tipo de contingência SVCSP.` |
| MDF-e com tipo de emissão diferente de Normal | `MDF-e só dispõe do tipo de emissão Normal.` |
| NFC-e com tipo de emissão diferente de Normal | `NFC-e só dispõe do tipo de emissão Normal.` |
| Serviço sem consulta disponível nesta tela | `Serviço de consulta não disponível para este modelo de documento.` |

Quando ocorre falha de comunicação, certificado, proxy, configuração ou retorno do autorizador, a tela exibe a mensagem retornada pela tentativa de consulta na coluna **Situação do Serviço** ou no campo de mensagem.

## Cuidados

- A consulta usa as configurações da empresa selecionada, incluindo certificado digital, ambiente, UF e preparação de TLS quando configurada.
- Se houver proxy configurado no UniNFe, a consulta usa as credenciais de proxy da configuração geral.
- Resultado positivo na consulta indica que o webservice respondeu, mas não garante que um XML específico esteja válido para autorização.
- Resultado de indisponibilidade ou erro ajuda no diagnóstico, mas deve ser analisado junto com logs, configuração da empresa e retorno do envio que apresentou problema.
