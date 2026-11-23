# Opções Temporárias para Geração do CIOT

> Documento de orientação para empresas que precisam gerar o **CIOT — Código Identificador da Operação de Transporte** enquanto o ERP, TMS ou sistema emissor ainda não possui integração automática.
>
> **Foco deste material:** deixar claro quando a empresa pode usar a integração direta com a ANTT e quando deve procurar uma **Instituição de Pagamento autorizada/apta à geração de CIOT** — aqui tratada, em linguagem operacional, como empresa terceirizada autorizada.
>
> **Última revisão:** 23/11/2026.

---

## 1. Objetivo deste documento

Este documento orienta, de forma prática, qual caminho a empresa deve seguir para gerar o CIOT enquanto seu sistema próprio ainda não está adaptado.

A decisão principal é esta:

```text
A empresa pode gerar CIOT diretamente no ambiente da ANTT?
        ou
A empresa deve gerar CIOT por uma Instituição de Pagamento autorizada?
```

A resposta depende principalmente de:

- quem executa o transporte;
- se existe contratação de TAC ou TAC equiparado;
- se a empresa é ETC com RNTRC ativo;
- quantidade de veículos da ETC no RNTRC;
- se o transporte é remunerado e de carga de terceiros;
- se existe integração técnica pronta, certificado digital e controle operacional.

---

## 2. Resumo executivo

| Situação da operação | Pode usar ambiente direto da ANTT? | Caminho recomendado |
|---|---:|---|
| **ETC com mais de 3 veículos no RNTRC, executando diretamente a operação, sem subcontratar TAC/TAC equiparado** | **Sim, em regra pode.** | Integração direta com Web Service da ANTT ou Instituição de Pagamento autorizada. |
| **ETC com mais de 3 veículos, mas sem ERP/TMS preparado para integração** | **Tecnicamente pode, mas não é o melhor caminho temporário.** | Usar temporariamente uma Instituição de Pagamento autorizada, especialmente portal gratuito, até o sistema ficar pronto. |
| **Contratação de TAC** | **Não.** | Gerar CIOT por Instituição de Pagamento autorizada/apta. |
| **Contratação de TAC equiparado** | **Não.** | Gerar CIOT por Instituição de Pagamento autorizada/apta. |
| **CTC — Cooperativa de Transporte Rodoviário de Cargas** | **Em regra, não como emissão direta ANTT do contratante.** | Tratar como fluxo de TAC equiparado, usando Instituição de Pagamento autorizada, conforme enquadramento. |
| **ETC com até 3 veículos no RNTRC** | **Não para geração direta como ETC não equiparada.** | É considerada TAC equiparada para fins de CIOT/PEF; usar Instituição de Pagamento autorizada. |
| **Embarcador contratando ETC com mais de 3 veículos que efetivamente executará o transporte** | **A responsabilidade tende a ser da ETC executora.** | A ETC gera via ANTT ou via Instituição de Pagamento autorizada. |
| **Embarcador contratando TAC/TAC equiparado** | **Não.** | O embarcador/contratante gera por Instituição de Pagamento autorizada. |
| **Carga própria, sem transporte remunerado de carga de terceiros** | **Não é cenário típico de CIOT.** | Em regra, não gerar CIOT; validar se não há prestação de serviço de transporte. |
| **Empresa sem RNTRC ativo, sem certificado, sem frota regularizada ou sem controle operacional** | **Não deve tentar direto.** | Regularizar RNTRC e frota antes da operação. A instituição autorizada é alternativa ao canal direto, não à regularização cadastral. |

---

## 3. Regra prática principal

### 3.1. Quem pode usar o ambiente da ANTT diretamente

O ambiente direto da ANTT é indicado para a **ETC — Empresa de Transporte Rodoviário de Cargas** que:

1. possui **RNTRC ativo**;
2. possui **mais de 3 veículos automotores de carga** vinculados à sua frota no RNTRC;
3. efetivamente executará a operação de transporte;
4. não está contratando ou subcontratando **TAC** ou **TAC equiparado** para executar aquela operação;
5. possui **certificado digital ICP-Brasil válido** do CNPJ responsável;
6. possui software preparado para consumir os Web Services da ANTT;
7. consegue tratar consulta, geração, cancelamento, retificação, encerramento, retornos, rejeições, logs e rastreabilidade.

Em termos simples:

> **A integração direta com a ANTT é para ETC não equiparada a TAC, que executa a própria operação de transporte e tem estrutura técnica para integrar.**

A ANTT informa que as ETCs já possuem cadastro para utilização dos serviços do CIOT, sendo necessário certificado digital válido conforme o DCS para realizar a integração. Mesmo assim, isso não significa que exista um portal manual simples da ANTT para digitação de CIOT.

---

### 3.2. Quem deve procurar uma Instituição de Pagamento autorizada/apta

A empresa deve procurar uma **Instituição de Pagamento autorizada/apta à geração de CIOT** quando ocorrer qualquer um destes cenários:

1. a operação envolve **TAC**;
2. a operação envolve **TAC equiparado**;
3. a operação envolve **ETC com até 3 veículos automotores de carga no RNTRC**;
4. a operação envolve **CTC**, quando tratada como TAC equiparado para fins de PEF/CIOT;
5. a ETC subcontratou TAC ou TAC equiparado;
6. o contratante delegou a geração à ETC equiparada ou CTC, hipótese em que o fluxo deve ocorrer por Instituição de Pagamento;
7. o cliente precisa emitir CIOT **manual ou temporariamente**, enquanto o ERP/TMS ainda não está integrado;
8. a empresa não possui software pronto para integração direta;
9. a empresa não possui certificado digital válido ou estrutura técnica para Web Service;
10. a empresa precisa de portal, suporte, pagamento eletrônico de frete, vale-pedágio, conciliação ou serviços operacionais agregados.

Em termos simples:

> **Se houver TAC, TAC equiparado ou necessidade de emissão manual imediata, o caminho prático é procurar uma Instituição de Pagamento autorizada/apta à geração de CIOT.**

---

### 3.3. Quem não deve gerar CIOT

A empresa normalmente **não deve gerar CIOT** quando a operação for apenas **carga própria**, sem prestação de serviço de transporte a terceiros e sem pagamento de frete.

Exemplo:

```text
Empresa transporta mercadoria própria, com veículo próprio, sem cobrar frete de terceiro.
```

Nesse cenário, a operação não se enquadra, em regra, como transporte rodoviário remunerado de cargas de terceiros. Antes de gerar CIOT indevidamente, a empresa deve validar o enquadramento jurídico e operacional.

---


## 4. Ambiente da ANTT não é portal manual de emissão

A ANTT disponibiliza documentação técnica, DCS, serviços de integração, biblioteca DLL, executável e ambientes de homologação/produção.

Entretanto, isso não deve ser confundido com um portal simples, no estilo:

```text
entrar no site → digitar dados → clicar em gerar CIOT
```

A integração direta com a ANTT envolve um processo técnico:

- Web Services;
- comunicação HTTPS/TLS;
- autenticação por certificado digital ICP-Brasil;
- envio de dados estruturados da operação;
- tratamento de protocolos e retornos;
- consulta de situação;
- consulta de frota;
- declaração da operação;
- consulta do CIOT gerado;
- cancelamento;
- retificação, quando permitida;
- encerramento;
- armazenamento de logs, requests, responses e comprovantes.

Por isso, para um cliente que precisa apenas **emitir CIOT agora**, antes do ERP/TMS estar pronto, o caminho mais seguro costuma ser usar uma Instituição de Pagamento autorizada que ofereça portal ou solução operacional.

---

## 5. Quando usar cada caminho

## 5.1. Usar integração direta com a ANTT

Use a integração direta com a ANTT quando todas as condições abaixo forem verdadeiras:

- a empresa é ETC;
- possui mais de 3 veículos automotores de carga no RNTRC;
- o RNTRC está ativo e regular;
- os veículos usados estão vinculados corretamente à frota no RNTRC;
- a operação é transporte rodoviário remunerado de carga de terceiros;
- a própria ETC executará a operação;
- não haverá contratação ou subcontratação de TAC/TAC equiparado;
- o certificado digital está válido;
- o sistema está preparado tecnicamente;
- a empresa conseguirá encerrar/cancelar/tratar pendências do CIOT.

### Exemplo prático

```text
Transportadora ETC com 20 caminhões no RNTRC transporta carga de um embarcador usando sua própria frota e seus motoristas.

Caminho possível:
- gerar CIOT diretamente pela integração ANTT; ou
- gerar CIOT por Instituição de Pagamento autorizada.
```

---

## 5.2. Usar Instituição de Pagamento autorizada/apta

Use uma Instituição de Pagamento autorizada/apta quando qualquer condição abaixo for verdadeira:

- o contratado é TAC;
- o contratado é TAC equiparado;
- a transportadora contratada é ETC com até 3 veículos automotores de carga no RNTRC;
- há CTC envolvida em cenário equiparado;
- há subcontratação de TAC/TAC equiparado;
- a empresa não quer ou não consegue integrar tecnicamente com a ANTT;
- o ERP/TMS ainda não está pronto;
- a necessidade é emitir CIOT manualmente por portal;
- a empresa quer solução com pagamento, vale-pedágio, conta, conciliação, suporte ou integração comercial.

### Exemplo prático 1 — Embarcador contrata TAC

```text
Embarcador contrata motorista autônomo TAC para transportar carga.

Caminho correto:
- gerar CIOT por Instituição de Pagamento autorizada/apta.
```

### Exemplo prático 2 — Transportadora subcontrata TAC

```text
ETC recebe a carga do embarcador, mas subcontrata TAC para executar a viagem.

Caminho correto:
- a ETC/subcontratante deve gerar o CIOT por Instituição de Pagamento autorizada/apta.
```

### Exemplo prático 3 — ETC pequena

```text
ETC possui até 3 veículos automotores de carga no RNTRC.

Caminho correto:
- tratar como TAC equiparada para fins de CIOT/PEF;
- gerar CIOT por Instituição de Pagamento autorizada/apta.
```

---

## 6. A geração do CIOT pode ser gratuita?

Sim. A geração/recebimento do CIOT deve ser gratuita.

Entretanto, é necessário separar:

| Item | Tratamento prático |
|---|---|
| **Geração do CIOT** | Deve ser gratuita enquanto cadastramento/recebimento do CIOT. |
| **Serviços adicionais** | Podem ser cobrados, conforme a instituição: integração API, plataforma, suporte, pagamento eletrônico de frete, vale-pedágio, conta, cartão, conciliação, relatórios e automação. |
| **Licença de integração** | Pode ser cobrada, dependendo da empresa e do modelo contratado. |
| **Portal gratuito** | Pode existir, mas exige cadastro, aprovação, documentação e validação das condições atuais. |

Portanto:

> O cliente pode buscar uma opção gratuita para emissão do CIOT, mas deve confirmar se a gratuidade cobre apenas a geração do CIOT ou também outros serviços necessários ao seu fluxo.

---

### Prazo para quitar o frete

A [Lei nº 15.485/2026](https://www.planalto.gov.br/ccivil_03/_ato2023-2026/2026/lei/l15485.htm) estabelece **até 30 dias úteis para quitar o frete**. O [Comunicado Geral SUROC nº 1/2026](https://anttlegis.antt.gov.br/action/ActionDatalegis.php?acao=abrirTextoAto&cod_menu=9230&cod_modulo=623&numeroAto=00000001&orgao=SUROC%2FANTT%2FMT&seqAto=000&tipo=COM&valorAno=2026) esclarece: contratações novas desde **06/08/2026**; operações sob contratos anteriores a partir de **05/11/2026**, ressalvadas operações já concluídas e direitos adquiridos. A regra alcança TAC, ETC e CTC e vale também na subcontratação, para cada contratante em relação à sua obrigação. A ANTT informou que as funcionalidades específicas do novo modelo de CIOT para forma e prazo de quitação ainda não estavam disponíveis. Até ato próprio, mantenha o fluxo vigente, controle o prazo no ERP/TMS e guarde contratos, documentos fiscais e comprovantes de liquidação. Não confunda os campos de pagamento já existentes no DCS com a implantação do novo modelo.

---

## 7. Lista oficial da ANTT deve ser a primeira validação

Antes de indicar qualquer fornecedor, o cliente deve validar se a instituição consta na lista oficial da ANTT como apta/credenciada para geração de CIOT.

### Link oficial

- [Instituições de Pagamento Eletrônico de Frete habilitadas — ANTT](https://www.gov.br/antt/pt-br/assuntos/cargas/pagamento-eletronico-de-fretes-pef-ciot/instituicoes-de-pagamento-eletronico-de-frete)

A lista muda com o tempo. Nunca use apenas material comercial, indicação de terceiro ou documentação antiga como fonte definitiva.

---

## 8. Opções de empresas para emissão por Instituição de Pagamento

As opções abaixo foram mantidas e reorganizadas a partir do documento original, com reforço de que a validação final deve ser feita na lista oficial da ANTT e diretamente com cada empresa.

> Importante: esta lista não é recomendação comercial. É apenas um conjunto de opções públicas para o cliente avaliar enquanto o sistema próprio ainda não está integrado.

---

**Habilitação conferida em 23/11/2026**, com base na [lista oficial da ANTT](https://www.gov.br/antt/pt-br/assuntos/cargas/pagamento-eletronico-de-fretes-pef-ciot/instituicoes-de-pagamento-eletronico-de-frete) atualizada em 03/11/2026. A **PagBem** (CNPJ 23.149.892/0001-92) não é uma opção para novas emissões: a [Deliberação ANTT nº 158/2026](https://anttlegis.antt.gov.br/action/ActionDatalegis.php?acao=abrirTextoAto&cod_menu=9230&cod_modulo=623&numeroAto=00000158&orgao=DG%2FANTT%2FMT&seqAto=000&tipo=DLB&valorAno=2026) revogou sua habilitação, com efeitos desde a publicação em 09/06/2026. Não confunda com a PAGME. Para operações antigas ainda abertas, confirme o procedimento com a ANTT; não presuma que CIOTs anteriores tenham sido invalidados pela revogação.

### 8.1. Roadcard / Pamcard

**Tipo de uso:** portal/plataforma para geração de CIOT e serviços relacionados ao pagamento de frete.

**Acesso ao sistema Pamcard; confirme com a Roadcard o cadastro para emissão sem serviços adicionais:**  
[https://www.roadcard.com.br/sistemapamcard/](https://www.roadcard.com.br/sistemapamcard/)

**Site oficial na lista ANTT:**  
[https://www.roadcard.com.br](https://www.roadcard.com.br)

**Quando avaliar:**

- cliente precisa de geração operacional enquanto o ERP/TMS ainda não está pronto;
- cliente aceita usar sistema/portal externo;
- cliente também pode precisar de pagamento eletrônico de frete, vale-pedágio ou serviços agregados.

**Pontos de atenção:**

- confirmar se a modalidade gratuita está ativa;
- confirmar prazo de cadastro;
- confirmar se atende geração, consulta, cancelamento, retificação e encerramento;
- confirmar custos de serviços adicionais.

---

### 8.2. Target Instituição de Pagamento

**Tipo de uso:** cadastro para geração via web. Confirme a disponibilidade e o procedimento atual com a instituição.

**Site oficial na lista ANTT:**  
[https://www.targetmp.com.br/](https://www.targetmp.com.br/)

**Quando avaliar:**

- cliente precisa gerar CIOT via cadastro e acesso fornecido pela instituição;
- cliente quer alternativa temporária de emissão manual;
- cliente quer confirmar com a instituição a modalidade de geração sem serviços adicionais.

**Pontos de atenção:**

- validar documentação exigida;
- confirmar prazo de liberação de acesso;
- confirmar se há integração/API ou apenas portal;
- confirmar custos para recursos adicionais.

---

### 8.3. APROMS NET

**Tipo de uso:** cadastro para emissão gratuita de CIOT.

**Link indicado no documento original:**  
[https://www.apromsnet.com.br/pages/ciot.html](https://www.apromsnet.com.br/pages/ciot.html)

**Site oficial na lista ANTT:**  
[https://www.apromsnet.com.br](https://www.apromsnet.com.br)

**Quando avaliar:**

- cliente precisa de uma opção de cadastro direto para emissão de CIOT;
- cliente não precisa, inicialmente, de API REST pública;
- cliente aceita fluxo manual/portal.

**Pontos de atenção:**

- confirmar se o cadastro está operacional;
- confirmar se há suporte para todas as operações posteriores;
- confirmar se existe integração por arquivo ou outro formato;
- confirmar condições atuais diretamente com a instituição.

---

### 8.4. FFCred / FF Digital

**Tipo de uso:** página pública de cadastro para emissão de CIOT.

**Link indicado no documento original:**  
[https://ffcredscd.com.br/ciot-gratuito](https://ffcredscd.com.br/ciot-gratuito)

**Site oficial na lista ANTT:**  
[https://ffcredscd.com.br](https://ffcredscd.com.br)

**Quando avaliar:**

- cliente precisa de alternativa de cadastro para emissão de CIOT;
- cliente quer validar opção gratuita ou operacional simples;
- cliente aceita confirmar condições comerciais e operacionais diretamente com a empresa.

**Pontos de atenção:**

- confirmar se a emissão gratuita está disponível para o cenário do cliente;
- confirmar prazo de cadastro;
- confirmar funcionalidades disponíveis;
- validar se atende o tipo de operação.

---

### 8.5. eFrete / NSTech

**Tipo de uso:** emissão de CIOT por sistema, chave de acesso, integração e licença de integração.

**Link indicado no documento original:**  
[https://www.efrete.com.br/emissao-de-ciot-integrado/](https://www.efrete.com.br/emissao-de-ciot-integrado/)

**Site oficial na lista ANTT:**  
[https://www.efrete.com.br](https://www.efrete.com.br)

**Quando avaliar:**

- cliente quer emitir CIOT integrado ao TMS/ERP;
- cliente aceita contratar licença ou solução de integração, se necessário;
- cliente quer sair do portal manual e automatizar o processo.

**Pontos de atenção:**

- diferenciar emissão gratuita por chave/acesso de licença de integração;
- confirmar custo de integração;
- confirmar modelo técnico de onboarding;
- confirmar suporte para consulta, cancelamento, retificação e encerramento.

---

### 8.6. Repom / Edenred

**Tipo de uso:** plataforma comercial de gestão de frete, geração de CIOT e serviços relacionados.

**Link indicado no documento original:**  
[https://www.edenredmobilidade.com.br/geracao-ciot/](https://www.edenredmobilidade.com.br/geracao-ciot/)

**Site oficial na lista ANTT:**  
[https://www.repom.com.br](https://www.repom.com.br)

**Quando avaliar:**

- cliente quer uma solução mais completa, não apenas gerar CIOT;
- cliente precisa de gestão de frete, pagamento, atendimento, relatórios e operação estruturada;
- cliente aceita contato comercial e eventual contratação de plataforma.

**Pontos de atenção:**

- confirmar se há modalidade gratuita para o cenário do cliente;
- confirmar custos de plataforma, suporte e integração;
- confirmar se atende operação manual imediata;
- confirmar documentação técnica caso haja integração futura.

---

### 8.7. Banco BBC Digital

**Tipo de uso:** alternativa de instituição apta, conforme lista oficial da ANTT e validação direta.

**Link indicado no documento original:**  
[https://bancobbcdigital.com.br/ciot](https://bancobbcdigital.com.br/ciot)

**Site oficial na lista ANTT:**  
[https://bancobbcdigital.com.br](https://bancobbcdigital.com.br)

**Quando avaliar:**

- cliente quer comparar opções de instituição apta;
- cliente precisa confirmar se existe portal de geração manual;
- cliente quer validar eventual gratuidade, cadastro e condições de uso.

**Pontos de atenção:**

- confirmar a situação atual na lista ANTT;
- confirmar se há portal operacional de geração;
- confirmar se há custos para emissão, integração ou serviços adicionais;
- confirmar se atende o tipo de operação do cliente.

---

## 9. Outras instituições que também aparecem na lista oficial da ANTT

Além das opções detalhadas acima, a lista oficial da ANTT consultada também apresenta outras instituições aptas/credenciadas para geração de CIOT, como:

- Sem Parar Instituição de Pagamento Ltda.;
- NDD Tech Ltda.;
- Banco Bradesco S.A.;
- Strada Pay Instituição de Pagamento Ltda.;
- Logcard Meios de Pagamento Ltda.;
- FitBank Pagamentos Eletrônicos S.A.;
- AuthPay Instituição de Pagamento Ltda.;
- VBS Sociedade de Crédito Direto S.A.;
- CDC Sociedade de Crédito Direto S.A.;
- PAGME Instituição de Pagamento Ltda. (CNPJ 34.471.744/0001-06, distinta da PagBem).

Essas instituições podem ser avaliadas pelo cliente, mas nem todas possuem, necessariamente, portal público simples, página clara de CIOT gratuito ou documentação pública de API. A validação deve ser feita caso a caso.

---

## 10. Comparativo prático das opções destacadas

| Opção | Melhor uso temporário | Indício público de portal/cadastro | Atenção principal |
|---|---|---:|---|
| **Roadcard / Pamcard** | Portal/plataforma de geração e gestão | Sim | Confirmar modalidade gratuita e custos agregados. |
| **Target** | Geração manual por cadastro | Sim | Confirmar liberação de acesso e funcionalidades. |
| **APROMS NET** | Cadastro para emissão gratuita | Sim | Confirmar suporte operacional e operações posteriores. |
| **FFCred / FF Digital** | Cadastro para emissão de CIOT | Sim | Confirmar condições atuais da modalidade gratuita. |
| **eFrete / NSTech** | Integração com TMS/ERP ou sistema próprio | Sim | Diferenciar chave gratuita de licença de integração. |
| **Repom / Edenred** | Plataforma completa de gestão de frete | Sim | Validar custo, contrato e eventual modalidade gratuita. |
| **Banco BBC Digital** | Avaliação conforme disponibilidade do portal | Sim/validar | Confirmar portal, gratuidade e condições atuais. |

---

## 11. Cuidados antes de indicar uma empresa ao cliente

Antes de orientar o cliente a usar qualquer empresa, valide:

1. A instituição está na lista oficial atualizada da ANTT?
2. A instituição está apta/autorizada a gerar CIOT no momento da contratação?
3. A geração do CIOT é gratuita na modalidade pretendida?
4. Há cobrança para plataforma, API, integração, suporte ou serviços adicionais?
5. O portal permite apenas gerar ou também consultar, cancelar, retificar e encerrar?
6. A operação do cliente envolve TAC, TAC equiparado, ETC, CTC ou carga própria?
7. O portal atende carga lotação, carga fracionada e TAC-Agregado, quando aplicável?
8. Há prazo de aprovação cadastral?
9. Quais documentos são exigidos?
10. O cliente receberá número do CIOT, código verificador, protocolo e comprovante?
11. O cliente conseguirá informar o CIOT no MDF-e quando houver MDF-e aplicável?
12. Há canal de suporte para rejeição, cancelamento, encerramento e contingência?

---

## 12. Informação que o cliente deve guardar após gerar o CIOT fora do ERP

Mesmo que o CIOT seja gerado fora do ERP/TMS, a empresa deve guardar e posteriormente lançar no sistema:

- número do CIOT;
- código verificador, quando retornado;
- protocolo ou comprovante;
- instituição responsável pela geração;
- CPF/CNPJ do responsável pelo CIOT;
- data e hora da geração;
- dados do contratante;
- dados do contratado/subcontratado;
- RNTRC;
- placa(s) do(s) veículo(s);
- origem e destino;
- tipo da operação;
- dados de carga;
- valor do frete;
- forma de pagamento;
- status da operação;
- arquivos ou comprovantes retornados pelo portal;
- vínculo com MDF-e, CT-e, contrato, ordem de coleta ou documento equivalente, conforme o caso.

Esses dados são importantes para fiscalização, suporte, auditoria e futura conciliação com o ERP/TMS.

---

## 13. Relação com o MDF-e

O CIOT não substitui MDF-e, CT-e, NF-e, contrato ou documento fiscal municipal.

Quando houver MDF-e aplicável à operação, o CIOT deve ser vinculado/informado no MDF-e correspondente.

O [Ajuste SINIEF 03/26](https://www.gov.br/fazenda/pt-br/composicao/orgaos/orgaos-colegiados/crsnsp/servicos/busca-de-atas-pautas-e-decisoes/pautas-das-sessoes-de-julgamentos/pauta-336-dou.pdf) produz efeitos desde **01/06/2026**. A [NT MDF-e 2026.001 v1.00](https://www.cte.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=F9WDPRCi0Bs%3D) prevê homologação em **21/09/2026** e produção em **23/11/2026** para a rejeição **684** (ausência de `infCIOT`). No modal rodoviário (`modal=1`), a regra verifica `tpEmit=1`, `tpEmit=3` ou `tpEmit=2` com `tpTransp` informado. Em carga própria legítima, não invente CIOT nem altere as tags para contornar a regra: confira se o enquadramento e os dados do XML estão corretos. A data técnica da rejeição não é a data de início da obrigação da ANTT.

No [leiaute do MDF-e](https://moc.sped.fazenda.pr.gov.br/MDFe/Leiaute.html), informe o **CIOT de 12 caracteres** em `infANTT/infCIOT/CIOT` e o CPF ou CNPJ de quem foi responsável pela geração. Não use o `Protocolo` nem concatene o código verificador nesse campo. Guarde o verificador: no DCS PEF ele integra o identificador de 16 caracteres exigido por operações como cancelamento, retificação e encerramento.

Estrutura conceitual no MDF-e rodoviário:

```text
MDFe
└── infMDFe
    └── infModal
        └── rodo
            └── infANTT
                └── infCIOT
```

Para o emissor de MDF-e, o ponto prático é:

> Mesmo que o CIOT seja gerado em portal externo, o número deverá ser informado no sistema emissor para constar no MDF-e quando aplicável.

---

## 14. Fluxo recomendado enquanto o ERP/TMS não está pronto

```text
1. Verificar se a operação exige CIOT
        ↓
2. Identificar o tipo de contratado
   - ETC com mais de 3 veículos?
   - TAC?
   - TAC equiparado?
   - CTC?
        ↓
3. Decidir o caminho
   - ANTT direta, se for ETC apta e com integração pronta
   - Instituição de Pagamento, se houver TAC/TAC equiparado ou emissão manual
        ↓
4. Escolher instituição na lista oficial da ANTT
        ↓
5. Fazer cadastro no portal ou contratar solução
        ↓
6. Gerar CIOT antes do início da operação
        ↓
7. Salvar CIOT, código verificador, protocolo e comprovantes
        ↓
8. Informar CIOT no MDF-e, quando houver MDF-e aplicável
        ↓
9. Acompanhar status da operação
        ↓
10. Cancelar, retificar ou encerrar conforme a regra e o tipo de operação
```

---

## 15. Orientação pronta para o cliente

Use este texto quando o cliente perguntar se pode usar direto a ANTT:

```text
A integração direta com a ANTT não é um portal manual de emissão de CIOT.

Ela é indicada para ETC com RNTRC ativo, mais de 3 veículos automotores de carga no RNTRC, certificado digital válido e operação executada diretamente pela própria ETC, sem contratação ou subcontratação de TAC/TAC equiparado.

Se a operação envolver TAC, TAC equiparado, CTC/equiparado, ETC com até 3 veículos ou se você precisa emitir CIOT manualmente enquanto o sistema ainda não está integrado, o caminho recomendado é procurar uma Instituição de Pagamento autorizada/apta à geração de CIOT.

Antes de usar qualquer instituição, confirme se ela consta na lista oficial da ANTT e valide se existe portal gratuito, prazo de cadastro, documentos exigidos e custos de serviços adicionais.
```

---

## 16. Conclusão

A regra prática para decisão é simples:

```text
ETC não equiparada, com mais de 3 veículos, RNTRC ativo, executando diretamente a operação e com integração técnica pronta
→ pode usar ambiente direto da ANTT ou Instituição de Pagamento.

TAC, TAC equiparado, CTC/equiparado, ETC com até 3 veículos, subcontratação de TAC/TAC equiparado ou emissão manual temporária
→ usar Instituição de Pagamento autorizada/apta.

Carga própria sem transporte remunerado de terceiros
→ em regra, não gerar CIOT.
```

Para uma solução temporária, antes da integração do ERP/TMS, o caminho operacional mais seguro costuma ser:

1. consultar a lista oficial da ANTT;
2. escolher uma Instituição de Pagamento apta;
3. validar se há portal gratuito;
4. gerar o CIOT antes do início da operação;
5. guardar comprovantes e protocolo;
6. informar o CIOT no MDF-e quando aplicável;
7. migrar para integração automática quando o sistema próprio estiver pronto.

---

## 17. Links úteis

### ANTT

- [CIOT Para Todos — ANTT](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1)
- [Perguntas Frequentes CIOT — ANTT](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/perguntas-frequentes)
- [Documentos Técnicos CIOT — ANTT](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/documentos-tecnicos)
- [Instituições de Pagamento Eletrônico de Frete habilitadas — ANTT](https://www.gov.br/antt/pt-br/assuntos/cargas/pagamento-eletronico-de-fretes-pef-ciot/instituicoes-de-pagamento-eletronico-de-frete)

### Opções destacadas neste documento

- [Roadcard / Pamcard](https://www.roadcard.com.br/sistemapamcard/)
- [Target — site oficial](https://www.targetmp.com.br/)
- [APROMS NET — CIOT](https://www.apromsnet.com.br/pages/ciot.html)
- [FFCred / FF Digital — CIOT Gratuito](https://ffcredscd.com.br/ciot-gratuito)
- [eFrete / NSTech — Emissão de CIOT Integrado](https://www.efrete.com.br/emissao-de-ciot-integrado/)
- [Repom / Edenred — Geração de CIOT](https://www.edenredmobilidade.com.br/geracao-ciot/)
- [Banco BBC Digital — CIOT](https://bancobbcdigital.com.br/ciot)

---

## 18. Observação final

As regras, links, instituições aptas, modalidades gratuitas, exigências de cadastro e preços de serviços adicionais podem mudar.

Antes de orientar um cliente ou iniciar uma operação real, valide sempre:

1. a FAQ oficial da ANTT;
2. a lista oficial de instituições aptas;
3. a documentação técnica vigente;
4. as condições atuais da instituição escolhida;
5. o enquadramento real da operação de transporte.
