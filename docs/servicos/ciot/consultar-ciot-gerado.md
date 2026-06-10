# Consultar CIOT gerado

O serviço de consulta de CIOT gerado permite que o ERP consulte uma operação de transporte já registrada no serviço CIOT. O ERP grava o XML de consulta na pasta de envio, o UniNFe transmite a solicitação e grava o retorno na pasta configurada para retornos.

Use este serviço quando for necessário recuperar ou confirmar informações de um CIOT já gerado a partir do código de identificação da operação e do ano da declaração.

## Pré-requisitos

Antes de executar a consulta, confira:

- A empresa está cadastrada no UniNFe.
- A pasta de envio e a pasta de retorno estão configuradas.
- O certificado digital está configurado e válido.
- O ambiente está configurado conforme a operação consultada.
- As configurações de proxy estão preenchidas, se a rede exigir proxy para acesso à internet.
- O código de identificação da operação e o ano da declaração estão corretos.

## Arquivo de envio

O ERP deve gerar o XML de consulta na pasta de envio da empresa com o final fixo:

```text
<identificador>-consultar.xml
```

O `<identificador>` deve ser único para evitar conflito entre consultas. Ele pode ser uma composição com o código da operação, o ano ou outro controle interno do ERP.

Exemplo:

```text
consultarCIOTGerado-consultar.xml
```

O conteúdo do XML deve usar a estrutura de consulta de CIOT gerado:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ConsultarCIOTGerado xmlns="http://www.antt.gov.br/ciot">
    <CodigoIdentificacaoOperacao>123456789012</CodigoIdentificacaoOperacao>
    <AnoDeclaracao>2026</AnoDeclaracao>
</ConsultarCIOTGerado>
```

Campos principais:

| Campo | Como preencher |
|---|---|
| `CodigoIdentificacaoOperacao` | Código de identificação da operação de transporte que será consultada. |
| `AnoDeclaracao` | Ano da declaração da operação de transporte. |

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-consultar.xml` na pasta de envio.
2. O UniNFe lê o XML `ConsultarCIOTGerado`.
3. O UniNFe aplica as configurações da empresa, certificado, ambiente, proxy e conexão TLS quando configurado.
4. O UniNFe envia a consulta ao serviço CIOT.
5. O retorno do serviço é gravado na pasta de retorno como `<identificador>-ret-consultar.xml`.
6. Se ocorrer falha local, o UniNFe grava `<identificador>-ret-consultar.err` na pasta de retorno.
7. O arquivo original da pasta de envio é removido após o processamento.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera <identificador>-consultar.xml"] --> B["Pasta de envio da empresa"]
    B --> C["UniNFe lê ConsultarCIOTGerado"]
    C --> D["Aplica certificado, ambiente, proxy e TLS"]
    D --> E["Envia consulta ao serviço CIOT"]
    E --> F["Grava <identificador>-ret-consultar.xml na pasta de retorno"]
    F --> G["ERP interpreta o retorno da consulta"]
    C -->|Erro local| H["Grava <identificador>-ret-consultar.err na pasta de retorno"]
    D -->|Erro local| H
    E -->|Erro local| H
```

## Arquivos gerados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Envio pelo ERP | Pasta de envio | `<identificador>-consultar.xml` | Arquivo criado pelo ERP para consultar um CIOT já gerado. |
| Retorno ao ERP | Pasta de retorno | `<identificador>-ret-consultar.xml` | Retorno XML do serviço CIOT com o resultado da consulta. |
| Erro ao ERP | Pasta de retorno | `<identificador>-ret-consultar.err` | Erro local antes ou durante o processamento, como falha de leitura, certificado, comunicação ou gravação. |

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar:

```text
<identificador>-ret-consultar.xml
```

Esse arquivo contém a resposta do serviço CIOT para o código de identificação e ano informados. O ERP deve analisar o conteúdo retornado para atualizar sua base com a situação ou os dados do CIOT consultado.

Este serviço não grava XML processado em `Enviados\Autorizados`. O resultado operacional para o ERP é o arquivo `-ret-consultar.xml` gerado na pasta de retorno.

## Erros locais

Se o UniNFe não conseguir concluir a consulta por falha local, será gerado:

```text
<identificador>-ret-consultar.err
```

As causas mais comuns são:

- XML fora da estrutura esperada para `ConsultarCIOTGerado`.
- Código de identificação da operação ausente ou inválido.
- Ano da declaração ausente ou inválido.
- Certificado digital ausente, inválido ou vencido.
- Ambiente, proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com o serviço CIOT.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-consultar.xml` na pasta de envio.

## Cuidados para o integrador

- Use sempre o final `-consultar.xml` para consultar CIOT gerado.
- Use o namespace `http://www.antt.gov.br/ciot` no XML.
- Informe corretamente o código de identificação da operação.
- Informe o ano da declaração correspondente à operação.
- Aguarde o arquivo `-ret-consultar.xml` para interpretar o retorno do serviço.
- Não espere geração de `-procCIOT.xml` ou outro XML processado neste serviço.
- Em erros `.err`, corrija a causa local antes de reenviar.
