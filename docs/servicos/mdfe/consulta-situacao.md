# Consulta de situação do MDFe

A consulta de situação do MDFe permite que o ERP consulte na SEFAZ a situação atual de um Manifesto Eletrônico de Documentos Fiscais pela chave de acesso. Ela é usada para confirmar se o MDFe foi autorizado, cancelado, encerrado, rejeitado ou se existe alguma ocorrência que precise ser tratada pelo ERP.

Este serviço também ajuda a finalizar manifestos que ficaram em processamento. Quando o UniNFe encontra o XML original do MDFe em `Enviados\EmProcessamento` e a SEFAZ retorna autorização, o UniNFe pode gerar o XML de distribuição `-procMDFe.xml` e mover os arquivos para as pastas corretas.

## Quando usar

Use a consulta de situação quando:

- O ERP precisa confirmar a situação atual de um MDFe na SEFAZ.
- Houve dúvida sobre o resultado do envio.
- O MDFe ficou em processamento e é necessário tentar finalizar o fluxo.
- O XML de distribuição autorizado ainda não foi localizado pelo ERP.
- É necessário confirmar se o manifesto foi encerrado, cancelado ou autorizado.

## Pré-requisitos

Antes de executar a consulta, confira na configuração da empresa:

- A empresa emissora está cadastrada no UniNFe.
- A pasta de envio, a pasta de retorno e a pasta de XMLs enviados estão configuradas.
- O certificado digital da empresa está configurado e válido.
- O ambiente da consulta é o mesmo ambiente usado na emissão do MDFe.
- O tipo de emissão informado no XML corresponde ao tipo de emissão usado no documento.
- As configurações de proxy estão preenchidas, se a rede exigir proxy para acesso à internet.

## Arquivo de envio

O ERP deve gerar o XML de consulta na pasta de envio da empresa com o final fixo:

```text
<identificador>-ped-sit.xml
```

O `<identificador>` deve ser único para a consulta. Normalmente é usada a própria chave de acesso do MDFe.

Exemplo:

```text
41110479189676000125550010000025721613066220-ped-sit.xml
```

O conteúdo do XML deve usar a estrutura de consulta de situação do MDFe:

```xml
<?xml version="1.0" encoding="utf-8"?>
<consSitMDFe xmlns="http://www.portalfiscal.inf.br/mdfe" versao="3.00">
  <tpAmb>2</tpAmb>
  <xServ>CONSULTAR</xServ>
  <tpEmis>1</tpEmis>
  <chMDFe>41110479189676000125550010000025721613066220</chMDFe>
</consSitMDFe>
```

Campos principais:

| Campo | Como preencher |
|---|---|
| `versao` | Versão do leiaute da consulta. |
| `tpAmb` | Ambiente da consulta. Use o mesmo ambiente em que o MDFe foi emitido. |
| `xServ` | Informe `CONSULTAR`. |
| `tpEmis` | Tipo de emissão do MDFe consultado. Para emissão normal, use `1`. |
| `chMDFe` | Chave de acesso completa do MDFe que será consultado. |

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-ped-sit.xml` na pasta de envio.
2. O UniNFe lê o XML de consulta e identifica a chave do MDFe.
3. O UniNFe aplica as configurações da empresa, certificado, tipo de emissão, proxy e conexão TLS quando configurado.
4. A consulta é enviada para a SEFAZ.
5. O retorno do webservice é gravado na pasta de retorno como `<identificador>-sit.xml`.
6. Se o retorno indicar autorização e o XML original estiver em `Enviados\EmProcessamento`, o UniNFe gera ou localiza o XML `<identificador-do-mdfe>-procMDFe.xml`.
7. O XML de distribuição autorizado é movido para `Enviados\Autorizados`.
8. O XML original assinado é movido para `Enviados\Autorizados` ou `Enviados\Originais`, conforme a configuração da empresa.
9. Se a consulta indicar rejeição ou situação que retire o documento do fluxo, o XML original é movido para a pasta de erros.
10. Se ocorrer falha local, o UniNFe grava `<identificador>-sit.err` na pasta de retorno.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera <identificador>-ped-sit.xml"] --> B["Pasta de envio da empresa"]
    B --> C["UniNFe lê consSitMDFe"]
    C --> D["Aplica certificado, tipo de emissão, proxy e TLS"]
    D --> E["Consulta situação do MDFe na SEFAZ"]
    E --> F["Grava <identificador>-sit.xml na pasta de retorno"]
    F --> G{"Retorno permite finalizar o MDFe?"}
    G -->|Autorizado| H{"XML original está em Enviados\\EmProcessamento?"}
    H -->|Sim| I["Gera ou localiza <identificador-do-mdfe>-procMDFe.xml"]
    I --> J["Move XML de distribuição para Enviados\\Autorizados"]
    J --> K["Move XML original para Autorizados ou Originais"]
    H -->|Não| L["Mantém apenas o retorno da consulta para o ERP"]
    G -->|Rejeitado ou inválido| M["Move XML original para a pasta de erros"]
    C -->|Erro local| N["Grava <identificador>-sit.err na pasta de retorno"]
    D -->|Erro local| N
    E -->|Erro local| N
```

## Arquivos gerados e movimentados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Pedido de consulta | Pasta de envio | `<identificador>-ped-sit.xml` | Arquivo criado pelo ERP para consultar a situação do MDFe. |
| Retorno ao ERP | Pasta de retorno | `<identificador>-sit.xml` | Retorno XML recebido da SEFAZ com a situação do MDFe. |
| Erro ao ERP | Pasta de retorno | `<identificador>-sit.err` | Erro local antes ou durante a consulta, como falha de leitura, certificado, comunicação ou gravação. |
| XML original em processamento | `Enviados\EmProcessamento` | `<identificador-do-mdfe>-mdfe.xml` | XML do MDFe que ainda está aguardando finalização no fluxo do UniNFe. |
| XML de distribuição | `Enviados\Autorizados\<subpasta por data>` | `<identificador-do-mdfe>-procMDFe.xml` | Gerado ou movido quando a SEFAZ retorna autorização e o XML original é localizado. |
| XML original assinado | `Enviados\Autorizados\<subpasta por data>` ou `Enviados\Originais\<subpasta por data>` | `<identificador-do-mdfe>-mdfe.xml` | Movido quando o MDFe é finalizado como autorizado. O destino depende da configuração para salvar somente o XML de distribuição. |
| XML com problema | Pasta de erros configurada | `<identificador-do-mdfe>-mdfe.xml` | Movido quando a consulta indica rejeição, invalidação do fluxo ou divergência que impede finalizar o documento. |

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar:

```text
<identificador>-sit.xml
```

Esse arquivo contém a resposta da SEFAZ para a chave consultada. O ERP deve analisar o status e o motivo retornados para atualizar a situação do MDFe em sua base.

Quando a situação indicar autorização, procure também o XML de distribuição:

```text
<identificador-do-mdfe>-procMDFe.xml
```

Esse arquivo fica em `Enviados\Autorizados`, dentro da subpasta de data configurada no UniNFe, e deve ser armazenado pelo ERP como XML fiscal autorizado.

Se o XML original do MDFe não estiver em `Enviados\EmProcessamento`, o UniNFe ainda grava o retorno da consulta para o ERP, mas não tem como gerar o XML de distribuição a partir do documento original. Nesse caso, use o retorno para atualizar a situação no ERP e avalie se será necessário recuperar o XML autorizado por outro procedimento.

Se o retorno da consulta trouxer eventos processados vinculados ao MDFe, o UniNFe também pode gravar os XMLs de evento processado correspondentes nas pastas de XMLs enviados.

## Erros locais

Se a consulta não puder ser concluída por falha local, será gerado:

```text
<identificador>-sit.err
```

As causas mais comuns são:

- XML de consulta fora da estrutura esperada.
- Chave do MDFe ausente ou inválida.
- Certificado digital ausente, inválido ou vencido.
- Ambiente da consulta diferente do ambiente de emissão.
- Tipo de emissão ausente ou incompatível com o documento.
- Proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com o webservice.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-ped-sit.xml` na pasta de envio.

## Cuidados para o integrador

- Use sempre o final `-ped-sit.xml` para consulta de situação.
- Informe a chave completa do MDFe em `chMDFe`.
- Consulte no mesmo ambiente em que o MDFe foi emitido.
- Informe em `tpEmis` o tipo de emissão usado pelo documento.
- Aguarde o arquivo `-sit.xml` para interpretar o retorno da SEFAZ.
- Não altere manualmente arquivos em `Enviados\EmProcessamento`.
- Quando a consulta finalizar um MDFe autorizado, armazene o XML `-procMDFe.xml`.
- Em erros `.err`, corrija a causa local antes de reenviar a consulta.
