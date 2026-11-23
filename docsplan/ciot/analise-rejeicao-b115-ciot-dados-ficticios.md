# Análise da rejeição B115 na emissão direta do CIOT

> **Importante:** todos os nomes empresariais, CNPJs, RNTRC, placas, identificadores, protocolos e caminhos de arquivos apresentados neste documento são **fictícios** e foram utilizados exclusivamente para fins de exemplo técnico.

## Identificação dos envolvidos

| Papel na operação | Empresa fictícia | CNPJ fictício |
|---|---|---:|
| Emissor da API / contratante / pagador | **Transportes Horizonte Azul Ltda.** | **12.345.678/0001-95** |
| Transportador contratado / recebedor do frete | **Logística Serra Dourada Ltda.** | **98.765.432/0001-98** |

A simulação foi feita exatamente com a relação comercial informada pelo cliente, mas a declaração não foi autorizada.

Existe um impeditivo da ANTT para que a **Transportes Horizonte Azul Ltda.** faça a emissão direta utilizando seu próprio certificado digital quando outra empresa figura como transportador contratado.

## Cenário informado

```text
Certificado/emissor da API:
12.345.678/0001-95 — Transportes Horizonte Azul Ltda.

CpfCnpjContratante:
12.345.678/0001-95 — Transportes Horizonte Azul Ltda.

CpfCnpjContratado:
98.765.432/0001-98 — Logística Serra Dourada Ltda.,
empresa que efetivamente realizará o transporte

RNTRCContratado:
012345678

Recebedor do frete:
98.765.432/0001-98 — Logística Serra Dourada Ltda.
```

A relação entre contratante e contratado está conceitualmente correta:

- a **Transportes Horizonte Azul Ltda.** é a contratante;
- a **Logística Serra Dourada Ltda.** é a empresa contratada para executar o transporte.

O problema não está nessa relação comercial, mas em **quem está tentando registrar diretamente a operação pela API da ANTT**.

---

## Retorno da ANTT

Foi gerado um novo identificador de operação e o payload foi enviado ao ambiente de homologação.

```text
IdOperacaoTransporte: 560000987654
Protocolo da rejeição: E98000000123456
```

A ANTT retornou:

```text
B115: Não foi encontrado transportador contratado com
CPF/CNPJ 98765432000198 e RNTRC 012345678.

O veículo do tipo automotor informado não possui vínculo
com o transportador contratado.

A empresa transportadora só pode emitir CIOT diretamente
em nome próprio quando for o transportador contratado.
```

Não houve rejeição relacionada ao identificador da operação ou às datas informadas.

Portanto:

- o ID foi gerado corretamente;
- as datas estavam dentro da tolerância;
- o JSON estava estruturalmente correto;
- o retorno foi desserializado corretamente;
- a rejeição possui natureza **cadastral e regulatória**.

---

## O impeditivo da regra B115

Na emissão direta, a [Portaria SUROC nº 6/2026, art. 3º](https://anttlegis.antt.gov.br/action/TematicaAction.php?acao=abrirVinculos&cod_menu=7144&cod_modulo=421&cotematica=24965313) atribui o registro à ETC não equiparada que efetivamente executa a operação. O retorno reproduzido acima indicou B115; ele deve ser lido com o CNPJ autenticado, o contratado efetivo e a categoria RNTRC, sem presumir que toda ocorrência de 314 signifique apenas divergência de CNPJ.

O DCS v1.2 revisou a B115: a comparação com `CpfCnpjContratado` identifica emissão em nome próprio e, quando há correspondência, a regra também impede a emissão para si própria por CTC ou ETC equiparada a TAC. A regra é do serviço ANTT, não do schema XML do UniNFe.

[Documento de Contrato de Serviço da ANTT — regra B115](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/dcs_pef_v1-2.pdf#page=62)

Neste envio:

```text
CNPJ identificado pelo certificado: 12345678000195
CpfCnpjContratado:                  98765432000198
```

Como os CNPJs são diferentes, a emissão direta pela **Transportes Horizonte Azul Ltda.** não é permitida nesse cenário.

A ANTT esclarece que uma ETC pode emitir diretamente pela API somente nas operações em que ela própria realizará o transporte, utilizando veículo vinculado à sua frota no RNTRC.

Quando uma ETC subcontrata outra ETC, a responsabilidade pelo registro passa para a empresa subcontratada que efetivamente executará o transporte.

[Orientações oficiais do CIOT Para Todos](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/codigo-identificador-da-operacao-de-transporte-ciot/)

---

## Como a operação deve ser emitida

A forma correta de emissão depende da categoria RNTRC da **Logística Serra Dourada Ltda.**, CNPJ `98.765.432/0001-98`.

### Situação 1 — Contratada classificada como ETC não equiparada a TAC

Se a **Logística Serra Dourada Ltda.** for uma ETC não equiparada a TAC, ela deverá:

1. emitir o CIOT diretamente, utilizando o próprio certificado digital; ou
2. realizar a emissão por meio de uma Instituição de Pagamento autorizada.

Nesse caso, os participantes da operação podem permanecer assim:

```xml
<CpfCnpjContratado>98765432000198</CpfCnpjContratado>
<CpfCnpjContratante>12345678000195</CpfCnpjContratante>
```

Entretanto, o certificado utilizado no acesso direto à API deve pertencer à **Logística Serra Dourada Ltda.**, CNPJ `98.765.432/0001-98`.

### Situação 2 — Contratada classificada como TAC ou TAC equiparado

Se a **Logística Serra Dourada Ltda.** for TAC ou TAC equiparado:

- a **Transportes Horizonte Azul Ltda.** continua sendo responsável pela contratação;
- o CIOT deve ser gerado por uma **Instituição de Pagamento autorizada**;
- a emissão não deve ser feita diretamente por essa integração com a API da ANTT.

A FAQ oficial da ANTT diferencia expressamente essas situações.

[Perguntas frequentes oficiais da ANTT — itens 6.4 a 6.8](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/perguntas-frequentes/perguntas-frequentes-ciot)

---

## Outras inconsistências cadastrais

Mesmo utilizando o certificado correto, ainda existem duas inconsistências que precisam ser verificadas.

### 1. Correspondência entre CNPJ e RNTRC

```text
CNPJ contratado: 98.765.432/0001-98
RNTRC informado: 012345678
```

O ambiente de homologação não encontrou correspondência válida entre esse CNPJ e o RNTRC informado.

### 2. Vínculo dos veículos com o transportador contratado

As placas informadas foram:

```text
ABC1D23
EFG4H56
```

Esses veículos não foram reconhecidos como vinculados ao transportador contratado.

Antes de uma nova tentativa, o cliente deve fornecer:

- RNTRC vigente e correspondente ao CNPJ `98.765.432/0001-98`;
- placas efetivamente vinculadas a esse RNTRC;
- categoria do transportador no RNTRC: ETC, TAC, ETC equiparada a TAC ou outra;
- confirmação de que a ETC executora, se elegível, usará seu próprio certificado no ambiente dela. Não envie arquivo de certificado, senha, PIN nem chave privada ao suporte.

---

## Informações de pagamento

Considerando que a **Transportes Horizonte Azul Ltda.** está pagando a empresa que realizará o transporte, o preenchimento coerente seria:

```xml
<ChavePix>98765432000198</ChavePix>
<CpfCnpjCreditado>98765432000198</CpfCnpjCreditado>
```

A relação financeira ficaria assim:

```text
Pagador/contratante:
Transportes Horizonte Azul Ltda. — 12.345.678/0001-95

Recebedor/contratado:
Logística Serra Dourada Ltda. — 98.765.432/0001-98
```

Caso o contratante informe que o pagamento será creditado no próprio CNPJ `12.345.678/0001-95`, a operação deverá ser esclarecida, pois isso representaria o contratante pagando a si mesmo.

Corrigir o recebedor do pagamento não resolve uma rejeição ligada ao papel do emissor. Para investigar a B115, compare:

```text
CNPJ identificado pelo certificado
x
CNPJ do transportador contratado
```

A validação de pagamento é separada. Confirme também a categoria RNTRC do contratado; na v1.2, ter os CNPJs iguais não basta se o contratado for CTC ou ETC equiparada a TAC.

---

## Modelo XML de referência

O repositório mantém um modelo de entrada em `exemplos xml/CIOT/ANTT/declaracaoOperacaoTransporte-ciot.xml`. Ele ilustra o leiaute aceito pelo UniNFe; os identificadores fictícios deste diagnóstico não são credenciais para testes de autorização.

---

## Conclusão

A composição abaixo está correta do ponto de vista da relação comercial:

```text
Contratante:
Transportes Horizonte Azul Ltda. — 12.345.678/0001-95

Transportador contratado:
Logística Serra Dourada Ltda. — 98.765.432/0001-98
```

O que não é permitido é a **Transportes Horizonte Azul Ltda.**, sendo uma empresa transportadora, utilizar seu próprio certificado digital para emitir diretamente pela API da ANTT uma operação na qual a **Logística Serra Dourada Ltda.** aparece como transportador contratado.

Para que a emissão direta seja aceita, o certificado utilizado na API deve pertencer ao mesmo transportador informado em `CpfCnpjContratado`, além de o RNTRC e os veículos estarem corretamente vinculados a essa empresa.
