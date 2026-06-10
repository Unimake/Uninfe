# Tela Validar XML

## Finalidade

A tela **Validar XML** permite conferir se um arquivo XML fiscal é reconhecido pelo UniNFe e se está consistente com o schema e as regras de validação aplicáveis ao tipo de documento.

Ela é útil para suporte, implantação e integradores que precisam testar um XML antes de colocá-lo na pasta de envio ou antes de investigar rejeições e erros de processamento.

## Onde acessar

No menu principal, acesse a opção **Validar XML**.

A opção fica disponível quando há empresas cadastradas no UniNFe.

## Campos

| Campo | O que faz | Como preencher |
|---|---|---|
| Selecione a empresa do certificado a ser utilizado para validação | Define a empresa usada como referência para localizar configurações, UF, serviço, schema e certificado digital. | Selecione a empresa relacionada ao XML que será validado. |
| Arquivo XML a ser validado | Caminho do arquivo XML que será analisado. | Digite ou cole o caminho do XML, ou clique no ícone de pasta para escolher o arquivo. |
| Tipo de arquivo | Mostra o tipo de XML identificado durante a validação. | Campo informativo; é preenchido pelo UniNFe após a validação. |
| Resultado da validação | Mostra mensagens de sucesso, inconsistências, erro de schema, erro de assinatura ou falha de leitura do arquivo. | Leia o resultado após clicar em **Validar**. |
| XML | Exibe uma visualização do XML validado. | Use para conferir o conteúdo do arquivo selecionado depois da validação. |

## Como validar um XML

1. Selecione a empresa relacionada ao XML.
2. No campo **Arquivo XML a ser validado**, informe o caminho do arquivo ou clique no ícone de pasta.
3. Se escolher o arquivo pelo ícone de pasta, a validação é iniciada automaticamente após a seleção.
4. Se digitar ou colar o caminho manualmente, clique em **Validar**.
5. Confira o campo **Tipo de arquivo**.
6. Leia a aba **Resultado da validação**.
7. Use a aba **XML** para visualizar o conteúdo do arquivo analisado.

## Arquivos aceitos na seleção

Para empresas de **NFS-e**, a seleção de arquivo prioriza XMLs relacionados a envio de lote RPS, cancelamento de NFS-e, consulta de lote, consulta de NFS-e, consulta por RPS, inutilização e pedidos de impressão ou retorno em PNG, PDF e XML.

Para os demais serviços, a seleção oferece XMLs fiscais como:

- XML de NF-e/NFC-e.
- XML de CT-e.
- XML de DF-e.
- XML de MDF-e.
- XML de eventos, como CC-e, pedido de evento, cancelamento e manifestação.
- XML do eSocial.
- XML do EFD-Reinf.
- Outros arquivos `.xml`.

## Resultado da validação

| Resultado | Significado |
|---|---|
| `Arquivo validado com sucesso!` | O XML foi reconhecido e validado sem inconsistências bloqueantes. |
| `Arquivo não encontrado.` | O caminho informado está vazio ou o arquivo não existe. |
| `XML não possui schema de validação, sendo assim não é possível validar XML` | O tipo de XML foi identificado, mas não há schema disponível para validar esse arquivo. |
| `XML INCONSISTENTE!` | O XML foi validado e apresentou inconsistências. Leia o detalhe exibido na tela. |
| `Ocorreu um erro na validação do XML` | Houve falha ao carregar, identificar ou validar o XML. |
| `Ocorreu um erro ao tentar assinar o XML` | O UniNFe tentou assinar a cópia usada na validação e encontrou problema com certificado, tag de assinatura ou estrutura do XML. |

## Uso do certificado

A empresa selecionada define o certificado usado quando a validação precisa assinar uma cópia do XML para concluir a checagem. Por isso, selecione a empresa correta antes de validar.

Se houver erro de certificado, revise a configuração da empresa em **Configurações > Empresas > Certificado digital**.

## Cuidados

- A tela usa uma cópia temporária do XML durante a validação; o arquivo original selecionado não é sobrescrito pela validação.
- Escolha uma empresa compatível com o tipo de XML. Uma empresa de serviço diferente pode levar a identificação incorreta, falha de schema ou erro de assinatura.
- Se o XML estiver em uma pasta protegida ou com permissão restrita, copie-o para uma pasta acessível antes de validar.
- Uma validação com sucesso indica que o XML passou pela validação local disponível no UniNFe, mas não garante autorização pela SEFAZ, prefeitura ou ambiente nacional.
- Se houver inconsistência, corrija o XML no ERP ou sistema emissor e valide novamente antes de enviar.
