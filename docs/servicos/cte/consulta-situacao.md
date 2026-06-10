# Consulta de situação do CTe

A consulta de situação do CTe permite que o ERP consulte na SEFAZ a situação atual de um Conhecimento de Transporte Eletrônico pela chave de acesso. Ela é usada para confirmar se o CTe foi autorizado, denegado, cancelado, rejeitado ou se existe alguma ocorrência que precise ser tratada pelo ERP.

Este serviço também ajuda a finalizar documentos que ficaram em processamento. Quando o UniNFe encontra o XML original do CTe em `Enviados\EmProcessamento` e a SEFAZ retorna autorização, o UniNFe pode gerar o XML de distribuição `-procCTe.xml` e mover os arquivos para as pastas corretas.

## Quando usar

Use a consulta de situação quando:

- O ERP precisa confirmar a situação atual de um CTe na SEFAZ.
- Houve dúvida sobre o resultado do envio.
- O CTe ficou em processamento e é necessário tentar finalizar o fluxo.
- O XML de distribuição autorizado ainda não foi localizado pelo ERP.
- É necessário confirmar se o documento foi autorizado, denegado ou cancelado.

## Pré-requisitos

Antes de executar a consulta, confira na configuração da empresa:

- A empresa emissora está cadastrada no UniNFe.
- A pasta de envio, a pasta de retorno e a pasta de XMLs enviados estão configuradas.
- O certificado digital da empresa está configurado e válido.
- O ambiente da consulta é o mesmo ambiente usado na emissão do CTe.
- O tipo de emissão informado no XML corresponde ao tipo de emissão usado no documento.
- As configurações de proxy estão preenchidas, se a rede exigir proxy para acesso à internet.

## Arquivo de envio

O ERP deve gerar o XML de consulta na pasta de envio da empresa com o final fixo:

```text
<identificador>-ped-sit.xml
```

O `<identificador>` deve ser único para a consulta. Normalmente é usada a própria chave de acesso do CTe.

Exemplo:

```text
50200106117473000150550010000606641403753210-ped-sit.xml
```

O conteúdo do XML deve usar a estrutura de consulta de situação do CTe:

```xml
<?xml version="1.0" encoding="utf-8"?>
<consSitCTe versao="4.00" xmlns="http://www.portalfiscal.inf.br/cte">
  <tpAmb>2</tpAmb>
  <xServ>CONSULTAR</xServ>
  <chCTe>50200106117473000150550010000606641403753210</chCTe>
</consSitCTe>
```

Campos principais:

| Campo | Como preencher |
|---|---|
| `versao` | Versão do leiaute da consulta. |
| `tpAmb` | Ambiente da consulta. Use o mesmo ambiente em que o CTe foi emitido. |
| `xServ` | Informe `CONSULTAR`. |
| `chCTe` | Chave de acesso completa do CTe que será consultado. |

O UniNFe identifica pela chave se a consulta deve ser feita como CTe ou CTe OS.

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-ped-sit.xml` na pasta de envio.
2. O UniNFe lê o XML de consulta e identifica a chave do CTe.
3. O UniNFe aplica as configurações da empresa, certificado, tipo de emissão, proxy e conexão TLS quando configurado.
4. A consulta é enviada para a SEFAZ.
5. O retorno do webservice é gravado na pasta de retorno como `<identificador>-sit.xml`.
6. Se o retorno indicar autorização e o XML original estiver em `Enviados\EmProcessamento`, o UniNFe gera ou localiza o XML `<identificador-do-cte>-procCTe.xml`.
7. O XML de distribuição autorizado é movido para `Enviados\Autorizados`.
8. O XML original assinado é movido para `Enviados\Autorizados` ou `Enviados\Originais`, conforme a configuração da empresa.
9. Se a consulta indicar denegação, o XML original é movido para `Enviados\Denegados`.
10. Se a consulta indicar cancelamento, rejeição ou situação que retire o documento do fluxo, o XML original é movido para a pasta de erros.
11. Se ocorrer falha local, o UniNFe grava `<identificador>-sit.err` na pasta de retorno.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera <identificador>-ped-sit.xml"] --> B["Pasta de envio da empresa"]
    B --> C["UniNFe lê consSitCTe"]
    C --> D["Aplica certificado, tipo de emissão, proxy e TLS"]
    D --> E["Consulta situação do CTe na SEFAZ"]
    E --> F["Grava <identificador>-sit.xml na pasta de retorno"]
    F --> G{"Retorno permite finalizar o CTe?"}
    G -->|Autorizado| H{"XML original está em Enviados\\EmProcessamento?"}
    H -->|Sim| I["Gera ou localiza <identificador-do-cte>-procCTe.xml"]
    I --> J["Move XML de distribuição para Enviados\\Autorizados"]
    J --> K["Move XML original para Autorizados ou Originais"]
    H -->|Não| L["Mantém apenas o retorno da consulta para o ERP"]
    G -->|Denegado| M["Move XML original para Enviados\\Denegados"]
    G -->|Cancelado, rejeitado ou inválido| N["Move XML original para a pasta de erros"]
    C -->|Erro local| O["Grava <identificador>-sit.err na pasta de retorno"]
    D -->|Erro local| O
    E -->|Erro local| O
```

## Arquivos gerados e movimentados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Pedido de consulta | Pasta de envio | `<identificador>-ped-sit.xml` | Arquivo criado pelo ERP para consultar a situação do CTe. |
| Retorno ao ERP | Pasta de retorno | `<identificador>-sit.xml` | Retorno XML recebido da SEFAZ com a situação do CTe. |
| Erro ao ERP | Pasta de retorno | `<identificador>-sit.err` | Erro local antes ou durante a consulta, como falha de leitura, certificado, comunicação ou gravação. |
| XML original em processamento | `Enviados\EmProcessamento` | `<identificador-do-cte>-cte.xml` | XML do CTe que ainda está aguardando finalização no fluxo do UniNFe. |
| XML de distribuição | `Enviados\Autorizados\<subpasta por data>` | `<identificador-do-cte>-procCTe.xml` | Gerado ou movido quando a SEFAZ retorna autorização e o XML original é localizado. |
| XML original assinado | `Enviados\Autorizados\<subpasta por data>` ou `Enviados\Originais\<subpasta por data>` | `<identificador-do-cte>-cte.xml` | Movido quando o CTe é finalizado como autorizado. O destino depende da configuração para salvar somente o XML de distribuição. |
| XML denegado | `Enviados\Denegados\<subpasta por data>` | `<identificador-do-cte>-cte.xml` ou arquivo de distribuição de denegação | Movido quando a SEFAZ retorna denegação para o CTe. |
| XML com problema | Pasta de erros configurada | `<identificador-do-cte>-cte.xml` | Movido quando a consulta indica cancelamento, rejeição, invalidação do fluxo ou divergência que impede finalizar o documento. |

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar:

```text
<identificador>-sit.xml
```

Esse arquivo contém a resposta da SEFAZ para a chave consultada. O ERP deve analisar o status e o motivo retornados para atualizar a situação do CTe em sua base.

Quando a situação indicar autorização, procure também o XML de distribuição:

```text
<identificador-do-cte>-procCTe.xml
```

Esse arquivo fica em `Enviados\Autorizados`, dentro da subpasta de data configurada no UniNFe, e deve ser armazenado pelo ERP como XML fiscal autorizado.

Quando a situação indicar denegação, trate o documento como denegado e consulte os arquivos em `Enviados\Denegados`, quando gerados. Quando a situação indicar rejeição, cancelamento ou inexistência na base da SEFAZ, apresente o motivo ao usuário e corrija o fluxo no ERP conforme o caso.

Se o XML original do CTe não estiver em `Enviados\EmProcessamento`, o UniNFe ainda grava o retorno da consulta para o ERP, mas não tem como gerar o XML de distribuição a partir do documento original. Nesse caso, use o retorno para atualizar a situação no ERP e avalie se será necessário recuperar o XML autorizado por outro procedimento.

Se o retorno da consulta trouxer eventos processados vinculados ao CTe, o UniNFe também pode gravar os XMLs de evento processado correspondentes nas pastas de XMLs enviados.

## Erros locais

Se a consulta não puder ser concluída por falha local, será gerado:

```text
<identificador>-sit.err
```

As causas mais comuns são:

- XML de consulta fora da estrutura esperada.
- Chave do CTe ausente ou inválida.
- Certificado digital ausente, inválido ou vencido.
- Ambiente da consulta diferente do ambiente de emissão.
- Tipo de emissão ausente ou incompatível com o documento.
- Proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com o webservice.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-ped-sit.xml` na pasta de envio.

## Cuidados para o integrador

- Use sempre o final `-ped-sit.xml` para consulta de situação.
- Informe a chave completa do CTe em `chCTe`.
- Consulte no mesmo ambiente em que o CTe foi emitido.
- Aguarde o arquivo `-sit.xml` para interpretar o retorno da SEFAZ.
- Não altere manualmente arquivos em `Enviados\EmProcessamento`.
- Quando a consulta finalizar um CTe autorizado, armazene o XML `-procCTe.xml`.
- Quando a consulta retornar denegação, trate o documento como denegado no ERP.
- Em erros `.err`, corrija a causa local antes de reenviar a consulta.
