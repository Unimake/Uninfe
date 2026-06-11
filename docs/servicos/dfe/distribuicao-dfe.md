# Distribuição DFe

A distribuição DFe permite que o ERP consulte documentos fiscais eletrônicos disponibilizados pela SEFAZ para uma empresa ou pessoa física, como documentos emitidos por terceiros, resumos, eventos e XMLs processados retornados no lote de distribuição.

O UniNFe lê o pedido de distribuição gravado na pasta de envio da empresa, envia a consulta para a SEFAZ, grava o retorno principal para o ERP e extrai os documentos compactados para uma subpasta própria dentro da pasta de retorno.

## Quando usar

Use este serviço quando o ERP precisa:

- Consultar documentos disponíveis a partir do último NSU conhecido.
- Consultar um NSU específico.
- Consultar uma NF-e específica pela chave de acesso.
- Obter os XMLs de documentos e eventos distribuídos pela SEFAZ.
- Manter o controle de NSU no ERP e buscar apenas novos documentos.

## Pré-requisitos

Antes de executar a consulta, confira na configuração da empresa:

- A empresa está cadastrada no UniNFe.
- A pasta de envio e a pasta de retorno estão configuradas.
- O certificado digital está configurado e válido.
- O ambiente informado no XML ou TXT está correto.
- A UF autorizadora está correta.
- As configurações de proxy e conexão TLS estão corretas, se a rede exigir proxy ou preparação TLS.

## Arquivo de envio em XML

Para consultar por XML, o ERP deve gerar o arquivo na pasta de envio da empresa com o final fixo:

```text
<identificador>-con-dist-dfe.xml
```

Exemplo:

```text
20120701184701-1-01-comCNPJ-con-dist-dfe.xml
```

O XML deve usar a raiz `distDFeInt` no leiaute de distribuição DFe da NF-e:

```xml
<?xml version="1.0" encoding="utf-8"?>
<distDFeInt versao="1.01" xmlns="http://www.portalfiscal.inf.br/nfe">
  <tpAmb>1</tpAmb>
  <cUFAutor>35</cUFAutor>
  <CNPJ>06117723000112</CNPJ>
  <distNSU>
    <ultNSU>123456789012345</ultNSU>
  </distNSU>
</distDFeInt>
```

Campos principais:

| Campo | Como preencher |
|---|---|
| `versao` | Versão do leiaute da distribuição DFe. |
| `tpAmb` | Ambiente da consulta. Use `1` para produção ou `2` para homologação. |
| `cUFAutor` | Código da UF autorizadora da consulta. |
| `CNPJ` | CNPJ do interessado na distribuição. |
| `CPF` | CPF do interessado, quando a consulta for feita por pessoa física. Use `CNPJ` ou `CPF`, não ambos. |
| `distNSU/ultNSU` | Último NSU conhecido pelo ERP. Use para buscar documentos posteriores a esse NSU. |
| `consNSU/NSU` | NSU específico que o ERP deseja consultar. |
| `consChNFe/chNFe` | Chave de acesso da NF-e que o ERP deseja consultar. |

## Arquivo de envio em TXT

Para consultar por TXT, o ERP deve gerar o arquivo na pasta de envio da empresa com o final fixo:

```text
<identificador>-con-dist-dfe.txt
```

Exemplo:

```text
20120701184701-con-dist-dfe.txt
```

Estrutura do TXT:

```text
versao|1.01
tpAmb|1
cUFAutor|41
CNPJ|06117473000150
ultNSU|123456789012345
```

Também é possível usar `CPF` no lugar de `CNPJ`, `NSU` no lugar de `ultNSU`, ou `chNFe` para consultar por chave de acesso.

Campos principais:

| Linha | Como preencher |
|---|---|
| `versao` | Versão do leiaute da distribuição DFe. |
| `tpAmb` | Ambiente da consulta. |
| `cUFAutor` | Código da UF autorizadora. |
| `CNPJ` | CNPJ do interessado. |
| `CPF` | CPF do interessado, quando aplicável. |
| `ultNSU` | Último NSU conhecido pelo ERP. |
| `NSU` | NSU específico que será consultado. |
| `chNFe` | Chave de acesso da NF-e que será consultada. |

Ao receber um pedido em TXT, o UniNFe gera o XML correspondente e segue o processamento da consulta pelo leiaute XML.

## Fluxo de processamento

1. O ERP grava `<identificador>-con-dist-dfe.xml` ou `<identificador>-con-dist-dfe.txt` na pasta de envio da empresa.
2. Quando o pedido está em TXT, o UniNFe converte o conteúdo para XML de distribuição DFe.
3. O UniNFe lê o XML `distDFeInt`.
4. O UniNFe aplica certificado digital, ambiente informado no pedido, proxy e preparação TLS quando configurados.
5. A consulta é enviada para a SEFAZ.
6. O retorno principal é gravado como `<identificador>-dist-dfe.xml` na pasta de retorno.
7. Os documentos compactados do retorno são descompactados na subpasta `dfe` dentro da pasta de retorno.
8. Se houver configuração de envio de retornos por FTP, os XMLs extraídos também podem ser enviados para a pasta de FTP configurada.
9. Se ocorrer falha local antes ou durante a consulta, o UniNFe grava `<identificador>-con-dist-dfe.err` na pasta de retorno.
10. O arquivo de solicitação é removido da pasta de envio após o processamento.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera -con-dist-dfe.xml ou -con-dist-dfe.txt"] --> B["Pasta de envio da empresa"]
    B --> C{"Formato do pedido"}
    C -->|TXT| D["UniNFe gera XML distDFeInt"]
    C -->|XML| E["UniNFe lê distDFeInt"]
    D --> E
    E --> F["Aplica certificado, ambiente, proxy e TLS"]
    F --> G["Consulta distribuição DFe na SEFAZ"]
    G --> H["Grava <identificador>-dist-dfe.xml"]
    H --> I{"Retorno possui documentos compactados?"}
    I -->|Sim| J["Extrai XMLs em Retorno\\dfe"]
    I -->|Não| K["ERP interpreta apenas o retorno principal"]
    J --> L["ERP importa documentos e eventos extraídos"]
    E -->|Erro local| M["Grava <identificador>-con-dist-dfe.err"]
    F -->|Erro local| M
    G -->|Erro local| M
```

## Arquivos gerados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Pedido XML | Pasta de envio | `<identificador>-con-dist-dfe.xml` | Arquivo criado pelo ERP para consultar a distribuição DFe em XML. |
| Pedido TXT | Pasta de envio | `<identificador>-con-dist-dfe.txt` | Arquivo criado pelo ERP para consultar a distribuição DFe em TXT. |
| XML convertido | Pasta de processamento configurada pelo UniNFe | `<identificador>-con-dist-dfe.xml` | Gerado a partir do TXT antes da consulta. |
| Retorno principal | Pasta de retorno | `<identificador>-dist-dfe.xml` | Retorno XML recebido da SEFAZ. |
| Erro ao ERP | Pasta de retorno | `<identificador>-con-dist-dfe.err` | Erro local antes ou durante a consulta, como falha de leitura, certificado, comunicação ou gravação. |
| XMLs extraídos | `Retorno\dfe` | `<identificador>-<NSU>-nfe.xml` | Resumo de NF-e retornado com NSU. |
| XMLs extraídos | `Retorno\dfe` | `<chave>-procNFe.xml` ou `<identificador>-<NSU>-procNFe.xml` | NF-e processada retornada pela distribuição. O nome pode usar a chave ou o NSU, conforme a configuração da empresa. |
| XMLs extraídos | `Retorno\dfe` | `<identificador>-<NSU>-eve.xml` | Resumo de evento retornado com NSU. |
| XMLs extraídos | `Retorno\dfe` | `<chave>_<tipoEvento>_<sequencia>-procEventoNFe.xml` ou `<identificador>-<NSU>-procEventoNFe.xml` | Evento processado de NF-e. O nome pode usar chave/tipo/sequência ou o NSU, conforme a configuração da empresa. |

O UniNFe também reconhece documentos de CT-e retornados no lote de distribuição e os extrai na mesma subpasta `dfe`, quando a SEFAZ retornar esse tipo de conteúdo.

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar:

```text
<identificador>-dist-dfe.xml
```

Esse arquivo contém o retorno principal da SEFAZ, incluindo status, motivo, último NSU, maior NSU e os documentos compactados quando houver documentos disponíveis.

Depois de ler o retorno principal, o ERP deve verificar a subpasta:

```text
Retorno\dfe
```

Nessa subpasta ficam os XMLs descompactados pelo UniNFe. O ERP deve importar esses arquivos conforme o tipo retornado: resumo de NF-e, NF-e processada, resumo de evento ou evento processado.

O controle de NSU deve ficar no ERP. Após processar um retorno bem-sucedido, armazene o último NSU retornado e utilize-o na próxima consulta por `ultNSU`.

## Erros locais

Se a consulta não puder ser concluída por falha local, será gerado:

```text
<identificador>-con-dist-dfe.err
```

As causas mais comuns são:

- XML ou TXT fora da estrutura esperada.
- `CNPJ` e `CPF` ausentes, ou preenchimento simultâneo indevido.
- Falta de `ultNSU`, `NSU` ou `chNFe` para definir o tipo de consulta.
- Certificado digital ausente, inválido ou vencido.
- Ambiente ou UF autorizadora preenchidos incorretamente.
- Proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com a SEFAZ.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo de consulta na pasta de envio.

## Cuidados para o integrador

- Use `-con-dist-dfe.xml` para pedido XML.
- Use `-con-dist-dfe.txt` para pedido TXT.
- Use um identificador único para relacionar pedido, retorno principal e XMLs extraídos.
- Controle o último NSU processado no ERP.
- Consulte por `ultNSU` para buscar novos documentos de forma contínua.
- Use `NSU` quando precisar reconsultar um NSU específico.
- Use `chNFe` quando precisar consultar uma chave de NF-e específica.
- Importe os XMLs da subpasta `Retorno\dfe` somente depois de receber o retorno principal da consulta.
- Em erros `.err`, corrija a causa local antes de reenviar a consulta.
