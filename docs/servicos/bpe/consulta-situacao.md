# Consulta de situação do BPe

A consulta de situação do BPe permite que o ERP consulte na SEFAZ a situação atual de um Bilhete de Passagem Eletrônico pela chave de acesso. Ela é usada para confirmar se o BPe foi autorizado, rejeitado, cancelado ou se existe alguma ocorrência que precise ser tratada pelo ERP.

Este serviço também ajuda a finalizar documentos que ficaram em processamento. Quando o UniNFe encontra o XML original do BPe em `Enviados\EmProcessamento` e a SEFAZ retorna autorização, o UniNFe pode gerar o XML de distribuição `-procBPe.xml` e mover os arquivos para as pastas corretas.

## Quando usar

Use a consulta de situação quando:

- O ERP precisa confirmar a situação atual de um BPe na SEFAZ.
- Houve dúvida sobre o resultado do envio.
- O BPe ficou em processamento e é necessário tentar finalizar o fluxo.
- O XML de distribuição autorizado ainda não foi localizado pelo ERP.
- É necessário recuperar eventos processados que venham vinculados ao retorno da consulta.

## Pré-requisitos

Antes de executar a consulta, confira na configuração da empresa:

- A empresa emissora está cadastrada no UniNFe.
- A pasta de envio, a pasta de retorno e a pasta de XMLs enviados estão configuradas.
- O certificado digital da empresa está configurado e válido.
- O ambiente da consulta é o mesmo ambiente usado na emissão do BPe.
- As configurações de proxy estão preenchidas, se a rede exigir proxy para acesso à internet.
- A preparação TLS está habilitada quando o ambiente exigir essa configuração.

## Arquivo de envio

O ERP deve gerar o XML de consulta na pasta de envio da empresa com o final fixo:

```text
<identificador>-ped-sit.xml
```

O `<identificador>` deve ser único para a consulta. Normalmente é usada a própria chave de acesso do BPe.

Exemplo:

```text
35260712345678000123630010000000011000000010-ped-sit.xml
```

O conteúdo do XML deve usar a estrutura de consulta de situação do BPe:

```xml
<?xml version="1.0" encoding="utf-8"?>
<consSitBPe versao="1.00" xmlns="http://www.portalfiscal.inf.br/bpe">
  <tpAmb>2</tpAmb>
  <xServ>CONSULTAR</xServ>
  <chBPe>35260712345678000123630010000000011000000010</chBPe>
</consSitBPe>
```

Campos principais:

| Campo | Como preencher |
|---|---|
| `versao` | Versão do leiaute da consulta. |
| `tpAmb` | Ambiente da consulta. Use o mesmo ambiente em que o BPe foi emitido. |
| `xServ` | Informe `CONSULTAR`. |
| `chBPe` | Chave de acesso completa do BPe que será consultado. |

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-ped-sit.xml` na pasta de envio.
2. O UniNFe lê o XML de consulta e identifica a chave do BPe.
3. O UniNFe aplica as configurações da empresa, certificado digital, proxy e preparação TLS quando configurados.
4. A consulta é enviada para a SEFAZ.
5. O retorno do webservice é gravado na pasta de retorno como `<identificador>-sit.xml`.
6. Se o retorno trouxer eventos processados vinculados ao BPe, o UniNFe grava os XMLs processados dos eventos.
7. Se o retorno indicar autorização e o XML original estiver em `Enviados\EmProcessamento`, o UniNFe gera ou localiza o XML `<identificador-do-bpe>-procBPe.xml`.
8. O XML de distribuição autorizado é movido para `Enviados\Autorizados`.
9. O XML original assinado é movido para `Enviados\Autorizados` ou `Enviados\Originais`, conforme a configuração da empresa.
10. Se a consulta indicar rejeição ou situação que retire o documento do fluxo, o XML original é movido para a pasta de erros.
11. Se ocorrer falha local, o UniNFe grava `<identificador>-sit.err` na pasta de retorno.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera <identificador>-ped-sit.xml"] --> B["Pasta de envio da empresa"]
    B --> C["UniNFe lê consSitBPe"]
    C --> D["Aplica certificado, proxy e TLS"]
    D --> E["Consulta situação do BPe na SEFAZ"]
    E --> F["Grava <identificador>-sit.xml na pasta de retorno"]
    F --> G["Grava eventos processados quando existirem no retorno"]
    F --> H{"Retorno permite finalizar o BPe?"}
    H -->|Autorizado| I{"XML original está em Enviados\\EmProcessamento?"}
    I -->|Sim| J["Gera ou localiza <identificador-do-bpe>-procBPe.xml"]
    J --> K["Move XML de distribuição para Enviados\\Autorizados"]
    K --> L["Move XML original para Autorizados ou Originais"]
    I -->|Não| M["Mantém apenas o retorno da consulta para o ERP"]
    H -->|Rejeitado ou inválido| N["Move XML original para a pasta de erros"]
    C -->|Erro local| O["Grava <identificador>-sit.err na pasta de retorno"]
    D -->|Erro local| O
    E -->|Erro local| O
```

## Arquivos gerados e movimentados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Pedido de consulta | Pasta de envio | `<identificador>-ped-sit.xml` | Arquivo criado pelo ERP para consultar a situação do BPe. |
| Retorno ao ERP | Pasta de retorno | `<identificador>-sit.xml` | Retorno XML recebido da SEFAZ com a situação do BPe. |
| Erro ao ERP | Pasta de retorno | `<identificador>-sit.err` | Erro local antes ou durante a consulta, como falha de leitura, certificado, comunicação ou gravação. |
| XML original em processamento | `Enviados\EmProcessamento` | `<identificador-do-bpe>-bpe.xml` | XML do BPe que ainda está aguardando finalização no fluxo do UniNFe. |
| XML de distribuição | `Enviados\Autorizados\<subpasta por data>` | `<identificador-do-bpe>-procBPe.xml` | Gerado ou movido quando a SEFAZ retorna autorização e o XML original é localizado. |
| XML original assinado | `Enviados\Autorizados\<subpasta por data>` ou `Enviados\Originais\<subpasta por data>` | `<identificador-do-bpe>-bpe.xml` | Movido quando o BPe é finalizado como autorizado. O destino depende da configuração para salvar somente o XML de distribuição. |
| Evento processado | `Enviados\Autorizados\<subpasta por data>` | `<chaveBPe>_<tipoEvento>_<sequencia>-procEventoBPe.xml` | Gerado quando o retorno da consulta contém evento processado vinculado ao BPe. |
| XML com problema | Pasta de erros configurada | `<identificador-do-bpe>-bpe.xml` | Movido quando a consulta indica rejeição, invalidação do fluxo, divergência de assinatura ou outra situação que impede finalizar o documento. |

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar:

```text
<identificador>-sit.xml
```

Esse arquivo contém a resposta da SEFAZ para a chave consultada. O ERP deve analisar o status e o motivo retornados para atualizar a situação do BPe em sua base.

Quando a situação indicar autorização, procure também o XML de distribuição:

```text
<identificador-do-bpe>-procBPe.xml
```

Esse arquivo fica em `Enviados\Autorizados`, dentro da subpasta de data configurada no UniNFe, e deve ser armazenado pelo ERP como XML fiscal autorizado.

Se o XML original do BPe não estiver em `Enviados\EmProcessamento`, o UniNFe ainda grava o retorno da consulta para o ERP, mas não tem como gerar o XML de distribuição a partir do documento original. Nesse caso, use o retorno para atualizar a situação no ERP e avalie se será necessário recuperar o XML autorizado por outro procedimento.

Se o retorno da consulta trouxer eventos processados vinculados ao BPe, o UniNFe também pode gravar os XMLs de evento processado correspondentes nas pastas de XMLs enviados.

## Validação do documento recuperado

Quando a consulta é usada para finalizar um BPe que estava em processamento, o UniNFe compara a assinatura do XML original com a informação retornada pela SEFAZ, quando essa validação estiver habilitada na configuração da empresa. Se houver divergência, o XML original é movido para a pasta de erros e o fluxo não é concluído com aquele arquivo.

Essa validação evita que um protocolo autorizado seja associado a um XML diferente do documento que foi realmente autorizado.

## Erros locais

Se a consulta não puder ser concluída por falha local, será gerado:

```text
<identificador>-sit.err
```

As causas mais comuns são:

- XML de consulta fora da estrutura esperada.
- Chave do BPe ausente ou inválida.
- Certificado digital ausente, inválido ou vencido.
- Ambiente da consulta diferente do ambiente de emissão.
- Proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com o webservice.
- Falha de permissão ou acesso às pastas configuradas.
- XML original em `Enviados\EmProcessamento` vazio, inválido ou incompatível com a chave consultada.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-ped-sit.xml` na pasta de envio.

## Cuidados para o integrador

- Use sempre o final `-ped-sit.xml` para consulta de situação.
- Informe a chave completa do BPe em `chBPe`.
- Consulte no mesmo ambiente em que o BPe foi emitido.
- Aguarde o arquivo `-sit.xml` para interpretar o retorno da SEFAZ.
- Não altere manualmente arquivos em `Enviados\EmProcessamento`.
- Quando a consulta finalizar um BPe autorizado, armazene o XML `-procBPe.xml`.
- Quando a consulta gerar eventos processados, armazene também os arquivos `-procEventoBPe.xml`.
- Em erros `.err`, corrija a causa local antes de reenviar a consulta.
