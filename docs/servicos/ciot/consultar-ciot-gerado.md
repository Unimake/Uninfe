# Consultar CIOT gerado

O serviço de consulta de CIOT gerado permite que o ERP consulte uma operação de transporte já registrada no serviço CIOT. O ERP grava o XML de consulta na pasta de envio, o UniNFe transmite a solicitação e grava o retorno na pasta configurada para retornos.

Use este serviço quando for necessário recuperar ou confirmar informações de um CIOT já gerado. A identificação da consulta depende do provedor.

## Provedores

O serviço está disponível para **ANTT** e **eFrete**. Na ANTT, a consulta usa o código de identificação da operação e o ano da declaração. No eFrete, usa o CNPJ da matriz e o identificador da operação no sistema do cliente.

## Pré-requisitos

Antes de executar a consulta, confira:

- A empresa está cadastrada no UniNFe.
- A pasta de envio e a pasta de retorno estão configuradas.
- O certificado digital está configurado e válido.
- O ambiente está configurado conforme a operação consultada.
- As configurações de proxy estão preenchidas, se a rede exigir proxy para acesso à internet.
- Os campos de identificação exigidos pelo provedor estão corretos.
- Para eFrete, o integrador e a forma de autenticação estão configurados conforme a [visão geral do CIOT](README.md#configuração-do-efrete).

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
    <ProvedorCIOT>ANTT</ProvedorCIOT>
    <CodigoIdentificacaoOperacao>123456789012</CodigoIdentificacaoOperacao>
    <AnoDeclaracao>2026</AnoDeclaracao>
</ConsultarCIOTGerado>
```

Campos principais:

Para consultar no eFrete, preserve a mesma tag raiz e o mesmo sufixo, mas use os campos próprios do provedor:

```xml
<ConsultarCIOTGerado xmlns="http://www.antt.gov.br/ciot">
    <ProvedorCIOT>EFrete</ProvedorCIOT>
    <MatrizCNPJ>12345678000199</MatrizCNPJ>
    <IdOperacaoCliente>CIOT-CLIENTE-0001</IdOperacaoCliente>
</ConsultarCIOTGerado>
```

| Campo | Provedor | Como preencher |
|---|---|---|
| `ProvedorCIOT` | Ambos | Use `ANTT` ou `EFrete`. Deve ser o primeiro elemento dentro de `ConsultarCIOTGerado`. |
| `CodigoIdentificacaoOperacao` | ANTT | Código de identificação da operação de transporte que será consultada. |
| `AnoDeclaracao` | ANTT | Ano da declaração da operação de transporte. |
| `MatrizCNPJ` | eFrete | CNPJ da matriz vinculada à operação. |
| `IdOperacaoCliente` | eFrete | Identificador atribuído à operação pelo sistema do cliente. |

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-consultar.xml` na pasta de envio.
2. O UniNFe lê o XML `ConsultarCIOTGerado`.
3. O UniNFe aplica as configurações da empresa, certificado, ambiente, proxy e conexão TLS quando configurado.
4. O UniNFe envia a consulta ao provedor indicado em `ProvedorCIOT`.
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
- Campos de identificação exigidos pelo provedor ausentes ou inválidos.
- Certificado digital ausente, inválido ou vencido.
- Ambiente, proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com o serviço CIOT.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-consultar.xml` na pasta de envio.

## Cuidados para o integrador

- Use sempre o final `-consultar.xml` para consultar CIOT gerado.
- Use o namespace `http://www.antt.gov.br/ciot` no XML.
- Informe `ProvedorCIOT` como primeira tag, com `ANTT` ou `EFrete`.
- Para ANTT, informe o código da operação e o ano da declaração.
- Para eFrete, informe o CNPJ da matriz e o identificador da operação no sistema do cliente.
- Aguarde o arquivo `-ret-consultar.xml` para interpretar o retorno do serviço.
- Não espere geração de `-procCIOT.xml` ou outro XML processado neste serviço.
- Em erros `.err`, corrija a causa local antes de reenviar.
