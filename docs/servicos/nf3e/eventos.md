# Eventos da NF3e

O serviço de eventos da NF3e permite que o ERP envie eventos vinculados a uma Nota Fiscal de Energia Elétrica Eletrônica já emitida. O ERP grava o XML do evento na pasta de envio, o UniNFe assina o XML, transmite o evento para a SEFAZ e grava o retorno na pasta de retorno.

Use este serviço quando for necessário registrar uma ocorrência fiscal relacionada à NF3e, como cancelamento, vinculação de pagamento ou cancelamento da vinculação de pagamento.

## Eventos atendidos nos exemplos

Os exemplos disponíveis para NF3e cobrem estes tipos de evento:

| Tipo de evento | Descrição no XML |
|---|---|
| `110111` | Cancelamento |
| `110300` | Vinculação do Pagamento |
| `110301` | Cancelamento da vinculacao do pagamento |

Use o tipo de evento, o detalhamento e as regras fiscais conforme o manual da NF3e e conforme a situação real do documento.

## Pré-requisitos

Antes de enviar um evento, confira:

- A empresa emissora está cadastrada no UniNFe.
- A pasta de envio e a pasta de retorno estão configuradas.
- A pasta de XMLs enviados e a pasta de backup estão configuradas quando usadas pela empresa.
- O certificado digital da empresa está configurado e válido.
- A NF3e referenciada no evento existe e a chave informada é a chave correta.
- O ambiente do evento é o mesmo ambiente em que a NF3e foi emitida.

## Arquivo de envio

O ERP deve gerar o XML do evento na pasta de envio da empresa com o final fixo:

```text
<identificador>-ped-eve.xml
```

O `<identificador>` deve ser único para o evento. Uma forma prática é usar uma composição com o documento, o tipo de evento e a sequência.

Exemplos:

```text
eventoNF3e_110111-ped-eve.xml
eventoNF3e_110300-ped-eve.xml
eventoNF3e_110301-ped-eve.xml
```

O conteúdo do XML deve usar a estrutura de evento da NF3e:

```xml
<?xml version="1.0" encoding="utf-8"?>
<eventoNF3e versao="1.00" xmlns="http://www.portalfiscal.inf.br/nf3e">
  <infEvento Id="ID11011112345678901234567890123456789012345678901234001">
    <cOrgao>41</cOrgao>
    <tpAmb>2</tpAmb>
    <CNPJ>06117473000150</CNPJ>
    <chNF3e>12345678901234567890123456789012345678901234</chNF3e>
    <dhEvento>2025-02-09T00:00:00-03:00</dhEvento>
    <tpEvento>110111</tpEvento>
    <nSeqEvento>1</nSeqEvento>
    <detEvento versaoEvento="1.00">
      <evCancNF3e>
        <descEvento>Cancelamento</descEvento>
        <nProt>123456789012345</nProt>
        <xJust>Justificativa do cancelamento</xJust>
      </evCancNF3e>
    </detEvento>
  </infEvento>
</eventoNF3e>
```

Campos principais:

| Campo | Como preencher |
|---|---|
| `infEvento/@Id` | Identificador do evento. Deve ser compatível com o tipo de evento, chave da NF3e e sequência. |
| `cOrgao` | Código da UF ou órgão responsável pelo evento. |
| `tpAmb` | Ambiente do evento. Use o mesmo ambiente da NF3e. |
| `CNPJ` | CNPJ do emissor do evento. |
| `chNF3e` | Chave de acesso da NF3e vinculada ao evento. |
| `dhEvento` | Data e hora do evento. |
| `tpEvento` | Tipo do evento, como `110111`, `110300` ou `110301`. |
| `nSeqEvento` | Número sequencial do evento para a mesma chave e tipo de evento. |
| `detEvento` | Grupo de detalhes do evento. O conteúdo muda conforme o tipo de evento. |
| `nProt` | Número do protocolo da NF3e ou do evento relacionado, quando exigido pelo tipo de evento. |
| `xJust` | Justificativa do evento, quando exigida pelo tipo de evento. |

Para eventos de vinculação de pagamento, o grupo de detalhes contém as informações do pagamento, como meio de pagamento, recebedor, PSP e identificação da transação. Para cancelamento da vinculação de pagamento, o grupo de detalhes informa o protocolo da vinculação que será cancelada.

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-ped-eve.xml` na pasta de envio.
2. O UniNFe lê o XML `eventoNF3e`.
3. O UniNFe aplica as configurações da empresa, certificado e conexão TLS quando configurada.
4. O UniNFe assina o XML do evento.
5. O evento é enviado para a SEFAZ.
6. O retorno do webservice é gravado na pasta de retorno como `<identificador>-eve.xml`.
7. Se o evento for aceito, o UniNFe gera o XML processado do evento em `Enviados\Autorizados`.
8. Quando houver pasta de backup configurada, o XML processado do evento também é gravado no backup.
9. Se o evento for rejeitado ou não puder ser confirmado como aceito, o XML original do pedido é movido para a pasta de erros.
10. Se ocorrer erro local, o UniNFe grava `<identificador>-eve.err` na pasta de retorno.
11. O arquivo de solicitação é removido da pasta de envio após o processamento.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera <identificador>-ped-eve.xml"] --> B["Pasta de envio da empresa"]
    B --> C["UniNFe lê eventoNF3e"]
    C --> D["Aplica certificado e TLS"]
    D --> E["Assina o XML do evento"]
    E --> F["Envia evento para a SEFAZ"]
    F --> G["Grava <identificador>-eve.xml na pasta de retorno"]
    G --> H{"Evento aceito?"}
    H -->|Sim| I["Gera XML processado do evento"]
    I --> J["Grava em Enviados\\Autorizados"]
    J --> K["Grava no backup quando configurado"]
    H -->|Não| L["Move pedido do evento para a pasta de erros"]
    C -->|Erro local| M["Grava <identificador>-eve.err na pasta de retorno"]
    D -->|Erro local| M
    E -->|Erro local| M
    F -->|Erro local| M
```

## Arquivos gerados e movimentados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Pedido do evento | Pasta de envio | `<identificador>-ped-eve.xml` | Arquivo criado pelo ERP para enviar o evento da NF3e. |
| Retorno ao ERP | Pasta de retorno | `<identificador>-eve.xml` | Retorno XML recebido da SEFAZ com o resultado do evento. |
| Erro ao ERP | Pasta de retorno | `<identificador>-eve.err` | Erro local antes ou durante o processamento do evento. |
| Evento processado | `Enviados\Autorizados\<subpasta por data>` | `<chaveNF3e>_<tipoEvento>_<sequencia>-procEventoNF3e.xml` | Evento aceito pela SEFAZ. O conteúdo do arquivo é um XML `procEventoNF3e`. |
| Backup do evento processado | Pasta de backup, quando configurada | `<chaveNF3e>_<tipoEvento>_<sequencia>-procEventoNF3e.xml` | Cópia de segurança do evento aceito. |
| XML rejeitado ou não aceito | Pasta de erros configurada | `<identificador>-ped-eve.xml` | Evento rejeitado ou não confirmado como aceito pela SEFAZ. |

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar:

```text
<identificador>-eve.xml
```

Esse arquivo contém a resposta da SEFAZ para o evento enviado. O ERP deve analisar o status e o motivo retornados.

Quando o evento for aceito, o UniNFe gera um XML processado do evento com o conteúdo `procEventoNF3e`. O arquivo é gravado em `Enviados\Autorizados`, dentro da subpasta de data configurada, usando o padrão:

```text
<chaveNF3e>_<tipoEvento>_<sequencia>-procEventoNF3e.xml
```

O ERP deve armazenar esse XML como comprovante do evento aceito.

Quando o evento for rejeitado, o ERP deve apresentar o motivo ao usuário, corrigir os dados e gerar um novo arquivo `-ped-eve.xml` na pasta de envio.

## Erros locais

Se o UniNFe não conseguir concluir o processamento por falha local, será gerado:

```text
<identificador>-eve.err
```

As causas mais comuns são:

- XML do evento fora da estrutura esperada.
- Identificador do evento incompatível com tipo, chave ou sequência.
- Chave da NF3e ausente ou inválida.
- Certificado digital ausente, inválido ou vencido.
- Ambiente do evento diferente do ambiente da NF3e.
- Falha de assinatura.
- Falha de comunicação com o webservice.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-ped-eve.xml` na pasta de envio.

## Cuidados para o integrador

- Use sempre o final `-ped-eve.xml` para envio de evento da NF3e.
- Informe `tpEvento` e `nSeqEvento` de acordo com a operação fiscal.
- Mantenha o identificador `infEvento/@Id` compatível com o evento enviado.
- Use o mesmo ambiente da NF3e original.
- Aguarde o arquivo `-eve.xml` para interpretar o retorno da SEFAZ.
- Armazene o XML processado do evento quando o evento for aceito.
- Em rejeições, corrija o XML e envie um novo pedido de evento.
- Em erros `.err`, corrija a causa local antes de reenviar.
