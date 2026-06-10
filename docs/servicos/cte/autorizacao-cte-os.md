# Autorização de CTe OS

A autorização de CTe OS permite que o ERP envie um Conhecimento de Transporte Eletrônico para Outros Serviços ao UniNFe por troca de arquivos. O ERP grava o XML do CTe OS na pasta de envio configurada para a empresa, o UniNFe assina o documento, transmite para a SEFAZ e grava o retorno na pasta de retorno.

Use este serviço quando a empresa emite CTe OS, modelo `67`, e precisa que o UniNFe faça o envio do XML para autorização.

## Pré-requisitos

Antes de enviar um CTe OS, confira na configuração da empresa:

- A empresa emissora está cadastrada no UniNFe.
- A pasta de envio, a pasta de retorno e a pasta de XMLs enviados estão configuradas.
- O certificado digital da empresa está configurado e válido.
- O ambiente de emissão está configurado conforme a operação desejada.
- As configurações de proxy estão preenchidas, se a rede exigir proxy para acesso à internet.
- O XML contém a estrutura de CTe OS e todos os grupos exigidos pelo leiaute para a operação fiscal.

## Arquivo de envio

O ERP deve gerar o XML do CTe OS na pasta de envio da empresa com o final fixo:

```text
<identificador>-cte.xml
```

O `<identificador>` deve ser único para evitar conflito entre documentos. Normalmente ele é a chave de acesso do CTe OS.

Exemplo:

```text
35170799999999999999670000000000261309301440-cte.xml
```

O conteúdo do arquivo deve ser o XML do CTe OS, com a estrutura esperada para o documento fiscal:

```xml
<?xml version="1.0" encoding="utf-8"?>
<CTeOS versao="4.00" xmlns="http://www.portalfiscal.inf.br/cte">
  <infCte versao="4.00" Id="CTe35170799999999999999670000000000261309301440">
    <ide>
      <cUF>35</cUF>
      <mod>67</mod>
      <serie>0</serie>
      <nCT>26</nCT>
      <dhEmi>2017-07-26T11:47:36-03:00</dhEmi>
      <tpEmis>1</tpEmis>
      <tpAmb>2</tpAmb>
    </ide>
    <emit>
      <CNPJ>99999999999999</CNPJ>
      <IE>999999999999</IE>
      <xNome>EMITENTE DO CTE OS</xNome>
    </emit>
  </infCte>
</CTeOS>
```

O exemplo acima mostra apenas os principais grupos. O XML real deve conter todos os campos exigidos pelo leiaute do CTe OS para a operação fiscal.

Campos e grupos principais:

| Campo ou grupo | Como preencher |
|---|---|
| `CTeOS` | Elemento raiz do documento. |
| `infCte/@Id` | Identificador do CTe OS. Deve ser compatível com a chave de acesso do documento. |
| `ide` | Dados de identificação, como UF, modelo `67`, série, número, data de emissão, ambiente, tipo de emissão, município de envio e tipo de serviço. |
| `emit` | Dados do emitente do CTe OS. |
| `toma` | Dados do tomador do serviço. |
| `vPrest` | Valores da prestação do serviço. |
| `imp` | Informações tributárias do documento. |
| `infCTeNorm` | Informações normais do CTe OS, incluindo dados específicos do serviço prestado. |

Não inclua XML de consulta, evento ou status de serviço neste arquivo. O CTe OS é reconhecido pelo conteúdo `CTeOS`, mesmo usando o mesmo final `-cte.xml` utilizado em outros fluxos de CTe.

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-cte.xml` na pasta de envio.
2. O UniNFe identifica o documento como CTe OS pelo elemento `CTeOS`.
3. O UniNFe lê o XML, aplica as configurações da empresa, prepara certificado, proxy e conexão TLS quando configurado.
4. O XML é assinado e gravado em `Enviados\EmProcessamento` com o mesmo nome do arquivo de envio.
5. O UniNFe envia o CTe OS para autorização na SEFAZ.
6. O retorno do webservice é gravado na pasta de retorno como `<identificador>-pro-rec.xml`.
7. Se o CTe OS for autorizado, o UniNFe cria o XML de distribuição `<identificador>-procCTe.xml` e move os arquivos para `Enviados\Autorizados`.
8. Se o CTe OS for denegado, o UniNFe cria o XML de denegação `<identificador>-den.xml` e move o arquivo para `Enviados\Denegados`.
9. Se o CTe OS for rejeitado, o XML assinado é movido para a pasta de erros e o ERP deve tratar a rejeição informada no retorno.
10. Se ocorrer erro local durante o envio, o UniNFe grava `<identificador>-pro-rec.err` na pasta de retorno.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera <identificador>-cte.xml com raiz CTeOS"] --> B["Pasta de envio da empresa"]
    B --> C["UniNFe identifica CTe OS"]
    C --> D["Aplica configurações, certificado, proxy e TLS"]
    D --> E["Assina o XML"]
    E --> F["Grava XML assinado em Enviados\\EmProcessamento"]
    F --> G["Envia CTe OS para a SEFAZ"]
    G --> H["Grava <identificador>-pro-rec.xml na pasta de retorno"]
    H --> I{"Resultado do CTe OS"}
    I -->|Autorizado| J["Gera <identificador>-procCTe.xml"]
    J --> K["Move XMLs para Enviados\\Autorizados"]
    I -->|Denegado| L["Gera <identificador>-den.xml"]
    L --> M["Move arquivo para Enviados\\Denegados"]
    I -->|Rejeitado| N["Move XML assinado para a pasta de erros"]
    C -->|Erro local| O["Grava <identificador>-pro-rec.err na pasta de retorno"]
    D -->|Erro local| O
    E -->|Erro local| O
    G -->|Erro local| O
```

## Arquivos gerados e movimentados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Envio pelo ERP | Pasta de envio | `<identificador>-cte.xml` | Arquivo criado pelo ERP para solicitar a autorização do CTe OS. |
| Em processamento | `Enviados\EmProcessamento` | `<identificador>-cte.xml` | XML já assinado pelo UniNFe enquanto o serviço está processando a autorização. |
| Retorno ao ERP | Pasta de retorno | `<identificador>-pro-rec.xml` | Retorno XML recebido do webservice, tanto para autorização, denegação ou rejeição retornada pela SEFAZ. |
| Erro local do envio | Pasta de retorno | `<identificador>-pro-rec.err` | Erro local durante o processamento, como falha de leitura, certificado, assinatura, comunicação ou gravação. |
| Erro de validação do arquivo | Pasta de retorno | `<identificador>-cte.err` | Erro identificado antes da conclusão do serviço de autorização do CTe. |
| XML de distribuição | `Enviados\Autorizados\<subpasta por data>` | `<identificador>-procCTe.xml` | CTe OS autorizado. É o XML principal para armazenamento fiscal e uso pelo ERP. |
| XML original assinado | `Enviados\Autorizados\<subpasta por data>` | `<identificador>-cte.xml` | CTe OS autorizado. |
| XML de denegação | `Enviados\Denegados\<subpasta por data>` | `<identificador>-den.xml` | CTe OS denegado pela SEFAZ. |
| XML rejeitado | Pasta de erros configurada | `<identificador>-cte.xml` | CTe OS rejeitado pela SEFAZ ou com falha que exige correção e novo envio. |

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar o arquivo:

```text
<identificador>-pro-rec.xml
```

Esse arquivo contém a resposta do webservice da SEFAZ. O ERP deve ler as informações de status, motivo e protocolo quando existirem. Quando o status indicar autorização, o ERP também deve localizar e armazenar o XML de distribuição:

```text
<identificador>-procCTe.xml
```

O XML de distribuição é gravado em `Enviados\Autorizados`, dentro da subpasta criada conforme a configuração de organização por data. Ele contém o CTe OS autorizado com o protocolo anexado.

Quando o status indicar denegação, o ERP deve tratar o documento como denegado e consultar o arquivo:

```text
<identificador>-den.xml
```

Quando o status indicar rejeição, o ERP deve apresentar o motivo ao usuário, corrigir os dados do CTe OS e gerar um novo arquivo `-cte.xml` na pasta de envio.

## Erros locais

Se o UniNFe não conseguir concluir o processamento por falha local, será gerado:

```text
<identificador>-pro-rec.err
```

Também pode haver retorno de erro do próprio tipo de arquivo CTe:

```text
<identificador>-cte.err
```

Esses arquivos devem ser tratados pelo ERP ou pelo suporte antes de reenviar o CTe OS. As causas mais comuns são:

- XML fora da estrutura esperada para CTe OS.
- Elemento raiz diferente de `CTeOS`.
- Modelo do documento diferente de `67`.
- Certificado digital ausente, inválido ou vencido.
- Falha de assinatura.
- Ambiente, proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com o webservice.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-cte.xml` na pasta de envio.

## Cuidados para o integrador

- Use o final `-cte.xml`, mas envie o XML com raiz `CTeOS`.
- Garanta que o modelo do documento seja `67`.
- Não reutilize o mesmo identificador enquanto houver processamento pendente para o documento.
- Aguarde o arquivo `-pro-rec.xml` para saber o resultado retornado pela SEFAZ.
- Armazene o XML `-procCTe.xml` quando o CTe OS for autorizado.
- Trate `-den.xml` como documento denegado, não como autorizado.
- Em rejeições, corrija o XML e envie novamente; não altere manualmente arquivos em `EmProcessamento`.
- Em erros `.err`, corrija a causa local antes de reenviar o documento.
