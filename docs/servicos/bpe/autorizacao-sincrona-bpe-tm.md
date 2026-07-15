# Autorização síncrona de BPe TM

A autorização síncrona de BPe TM permite que o ERP envie um Bilhete de Passagem Eletrônico de Transporte Metropolitano ao UniNFe por troca de arquivos. O ERP grava o XML do BPe TM na pasta de envio configurada para a empresa, o UniNFe assina o documento, transmite para a SEFAZ e grava o retorno na pasta de retorno.

Use este serviço quando a empresa emite BPe TM e precisa que o UniNFe faça o envio direto do XML para autorização.

## Pré-requisitos

Antes de enviar um BPe TM, confira na configuração da empresa:

- A empresa emissora está cadastrada no UniNFe.
- A pasta de envio, a pasta de retorno e a pasta de XMLs enviados estão configuradas.
- O certificado digital da empresa está configurado e válido.
- O ambiente de emissão está configurado conforme a operação desejada.
- As configurações de proxy estão preenchidas, se a rede exigir proxy para acesso à internet.
- A preparação TLS está habilitada quando o ambiente exigir essa configuração.
- Os dados do responsável técnico estão preenchidos, quando a emissão da empresa exigir essa informação.

Se o XML do BPe TM não possuir o grupo do responsável técnico e os dados estiverem configurados na empresa, o UniNFe utiliza os dados da configuração para compor o XML antes do envio.

## Arquivo de envio

O ERP deve gerar o XML do BPe TM na pasta de envio da empresa com o final fixo:

```text
<identificador>-bpe-tm.xml
```

O `<identificador>` deve ser único para evitar conflito entre documentos. Normalmente ele é a chave do BPe TM.

Exemplo:

```text
35260712345678000195630010000000011234567890-bpe-tm.xml
```

O conteúdo do arquivo deve ser o XML do BPe TM, com a estrutura esperada para o documento fiscal:

```xml
<?xml version="1.0" encoding="utf-8"?>
<BPeTM xmlns="http://www.portalfiscal.inf.br/bpe">
  <infBPe versao="1.00" Id="BPe35260712345678000195630010000000011234567890">
    <ide>
      <cUF>35</cUF>
      <tpAmb>2</tpAmb>
      <mod>63</mod>
      <serie>1</serie>
      <nBP>1</nBP>
      <cBP>12345678</cBP>
      <dhEmi>2026-07-06T08:00:00-03:00</dhEmi>
      <dCompet>2026-07-06</dCompet>
      <tpEmis>1</tpEmis>
      <tpBPe>4</tpBPe>
      <CFOP>5353</CFOP>
    </ide>
    <emit>
      <CNPJ>12345678000195</CNPJ>
      <IE>123456789012</IE>
      <xNome>EMPRESA BP-E TM</xNome>
    </emit>
    <detBPeTM idEqpCont="1">
      <UFIniViagem>SP</UFIniViagem>
      <det nViagem="1">
        <cMunIni>3550308</cMunIni>
        <qPass>1</qPass>
        <vBP>10.00</vBP>
      </det>
    </detBPeTM>
  </infBPe>
</BPeTM>
```

O exemplo acima mostra apenas os principais grupos. O XML real deve conter todos os campos exigidos pelo leiaute do BPe TM para a operação fiscal.

Campos e grupos principais:

| Campo ou grupo | Como preencher |
|---|---|
| `infBPe/@Id` | Identificador do BPe TM. Deve ser compatível com a chave de acesso do documento. |
| `ide` | Dados de identificação do BPe TM, como UF, ambiente, modelo, série, número, data de emissão, competência, tipo de emissão, tipo de BPe e CFOP. |
| `emit` | Dados do emitente do BPe TM. |
| `detBPeTM` | Dados do equipamento de controle, UF de início da viagem e detalhamento das viagens. |
| `det` | Detalhamento da viagem, município de início, quantidade de passageiros, valor do bilhete, impostos e componentes. |
| `total` | Totais do documento, incluindo valores do BPe TM e totais de tributos quando exigidos. |
| `pgtoVinc` | Informações de pagamento vinculado, quando aplicáveis ao documento. |
| `infRespTec` | Dados do responsável técnico. Quando ausente no XML, pode ser preenchido com os dados configurados no UniNFe. |

Não inclua XML de consulta, evento ou status de serviço neste arquivo. Este serviço é síncrono: o envio e o retorno do webservice acontecem no mesmo processamento.

## Fluxo de processamento

1. O ERP grava o arquivo `<identificador>-bpe-tm.xml` na pasta de envio.
2. O UniNFe identifica o documento como BPe TM pelo XML e pelo final do arquivo.
3. O UniNFe lê o XML, aplica as configurações da empresa, prepara certificado, proxy e conexão TLS quando configurado.
4. Quando necessário, o UniNFe complementa o grupo do responsável técnico com os dados configurados na empresa.
5. O XML é assinado e gravado em `Enviados\EmProcessamento` com o mesmo nome do arquivo de envio.
6. O UniNFe envia o BPe TM para autorização síncrona na SEFAZ.
7. O retorno do webservice é gravado na pasta de retorno como `<identificador>-ret-bpe-tm.xml`.
8. Se o BPe TM for autorizado, o UniNFe cria o XML de distribuição `<identificador>-procBPeTM.xml` e move os arquivos para a pasta de autorizados.
9. Se a configuração da empresa estiver marcada para salvar somente o XML de distribuição, o XML original assinado é movido para `Enviados\Originais`.
10. Se o BPe TM for rejeitado, o XML assinado é movido para a pasta de erros e o ERP deve tratar a rejeição informada no retorno.
11. Se ocorrer erro local durante o envio, o UniNFe grava um arquivo `<identificador>-pro-rec.err` na pasta de retorno com os detalhes do erro.

## Fluxograma

```mermaid
flowchart TD
    A["ERP gera <identificador>-bpe-tm.xml"] --> B["Pasta de envio da empresa"]
    B --> C["UniNFe lê e identifica o BPe TM"]
    C --> D["Aplica configurações, certificado, proxy e TLS"]
    D --> E["Complementa responsável técnico quando configurado"]
    E --> F["Assina o XML"]
    F --> G["Grava XML assinado em Enviados\\EmProcessamento"]
    G --> H["Envia para autorização síncrona na SEFAZ"]
    H --> I["Grava <identificador>-ret-bpe-tm.xml na pasta de retorno"]
    I --> J{"BPe TM autorizado?"}
    J -->|Sim| K["Gera <identificador>-procBPeTM.xml"]
    K --> L["Move XML de distribuição para Enviados\\Autorizados"]
    L --> M["Move XML original para Autorizados ou Originais"]
    J -->|Não| N["Move XML assinado para a pasta de erros"]
    C -->|Erro local| O["Grava <identificador>-pro-rec.err na pasta de retorno"]
    D -->|Erro local| O
    F -->|Erro local| O
    H -->|Erro local| O
```

## Arquivos gerados e movimentados

| Momento | Pasta | Nome do arquivo | Quando aparece |
|---|---|---|---|
| Envio pelo ERP | Pasta de envio | `<identificador>-bpe-tm.xml` | Arquivo criado pelo ERP para solicitar a autorização do BPe TM. |
| Em processamento | `Enviados\EmProcessamento` | `<identificador>-bpe-tm.xml` | XML já assinado pelo UniNFe enquanto o serviço está processando a autorização. |
| Retorno ao ERP | Pasta de retorno | `<identificador>-ret-bpe-tm.xml` | Retorno XML recebido do webservice, tanto para autorização quanto para rejeição retornada pela SEFAZ. |
| Erro local do envio | Pasta de retorno | `<identificador>-pro-rec.err` | Erro local durante o processamento, como falha de leitura, certificado, assinatura, comunicação ou gravação. |
| Erro de validação do arquivo | Pasta de retorno | `<identificador>-ret-bpe-tm.err` | Erro identificado antes da conclusão do serviço de autorização, conforme o retorno de erro do tipo de arquivo BPe TM. |
| XML de distribuição | `Enviados\Autorizados\<subpasta por data>` | `<identificador>-procBPeTM.xml` | BPe TM autorizado. É o XML principal para armazenamento fiscal e uso pelo ERP. |
| XML original assinado | `Enviados\Autorizados\<subpasta por data>` ou `Enviados\Originais\<subpasta por data>` | `<identificador>-bpe-tm.xml` | BPe TM autorizado. O destino depende da configuração para salvar somente o XML de distribuição. |
| XML rejeitado | Pasta de erros configurada | `<identificador>-bpe-tm.xml` | BPe TM rejeitado pela SEFAZ ou com falha que exige correção e novo envio. |

## Como tratar o retorno

O ERP deve monitorar a pasta de retorno e aguardar o arquivo:

```text
<identificador>-ret-bpe-tm.xml
```

Esse arquivo contém a resposta do webservice da SEFAZ. O ERP deve ler as informações de status, motivo e protocolo quando existirem. Quando o status indicar autorização, o ERP também deve localizar e armazenar o XML de distribuição:

```text
<identificador>-procBPeTM.xml
```

O XML de distribuição é gravado na pasta `Enviados\Autorizados`, dentro da subpasta criada conforme a configuração de organização por data. Ele contém o BPe TM autorizado com o protocolo anexado.

Quando o status indicar rejeição, o ERP deve apresentar o motivo ao usuário, corrigir os dados do BPe TM e gerar um novo arquivo `-bpe-tm.xml` na pasta de envio. A rejeição não deve ser tratada como autorização.

## Erros locais

Se o UniNFe não conseguir concluir o processamento por falha local, será gerado um arquivo de erro na pasta de retorno. Durante o envio síncrono, o retorno esperado é:

```text
<identificador>-pro-rec.err
```

Também pode haver retorno de erro do próprio tipo de arquivo BPe TM:

```text
<identificador>-ret-bpe-tm.err
```

Esses arquivos devem ser tratados pelo ERP ou pelo suporte antes de reenviar o BPe TM. As causas mais comuns são:

- XML fora da estrutura esperada.
- Certificado digital ausente, inválido ou vencido.
- Falha de assinatura.
- Ambiente, proxy ou conexão TLS configurados incorretamente.
- Falha de comunicação com o webservice.
- Falha de permissão ou acesso às pastas configuradas.

Depois de corrigir o problema, gere novamente o arquivo `<identificador>-bpe-tm.xml` na pasta de envio.

## Cuidados para o integrador

- Use sempre o final `-bpe-tm.xml` para o arquivo de envio do BPe TM.
- Não reutilize o mesmo identificador enquanto houver processamento pendente para o documento.
- Aguarde o arquivo `-ret-bpe-tm.xml` para saber o resultado retornado pela SEFAZ.
- Armazene o XML `-procBPeTM.xml` quando o BPe TM for autorizado.
- Em rejeições, corrija o XML e envie novamente; não altere manualmente arquivos em `EmProcessamento`.
- Em erros `.err`, corrija a causa local antes de reenviar o documento.
