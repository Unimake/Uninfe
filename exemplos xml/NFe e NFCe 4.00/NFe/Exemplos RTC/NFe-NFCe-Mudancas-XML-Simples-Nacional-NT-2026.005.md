# NF-e/NFC-e: Simples Nacional, evento 412140 e pagamentos vinculados

Este guia explica as mudanças dos schemas `DFeTiposBasicos_v1.00.xsd` e `leiauteNFe_v4.00.xsd`, a inclusão de `e412140_v1.00.xsd` e o tratamento correspondente na Unimake.DFe. O foco é orientar o desenvolvedor do ERP na formação do XML, na seleção dos grupos de tributação, na totalização e na leitura do novo evento.

Os modelos desta pasta usam o leiaute **4.00**, o namespace `http://www.portalfiscal.inf.br/nfe` e o ambiente de homologação (`tpAmb=2`). As mudanças de leiaute também abrangem a NFC-e, modelo 65; os sete documentos fornecidos são exemplos de **NF-e, modelo 55**. A adaptação para NFC-e exige respeitar suas regras próprias, inclusive impressão, identificação do consumidor e suplemento/QR Code.

**O evento 412140 é produzido pela Administração Tributária, com `tpAutor=5`.** Seu modelo serve para estudar a estrutura e preparar a leitura. O nome `Evento_412140-ped-eve.xml` segue o padrão da pasta, mas não representa autorização para o ERP transmitir esse evento em nome da empresa. A NT 2026.005 v1.00 prevê implementação futura do evento.

## 1. Documentação oficial e downloads

| Documento | Relação com esta atualização | Download oficial |
|---|---|---|
| **NT 2026.005 v1.00 — Simples Nacional** | Fonte principal de `tpRegimeIBSCBS`, `cAtivSN`, grupos `gTribSN`, totalizadores e evento 412140. Consultar especialmente as páginas 5–7, 8–10 e 11–12. | [PDF no Portal Nacional da NF-e](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=HInG7WD5Sj8=) · [Download pela SVRS](https://dfe-portal.svrs.rs.gov.br/NFE/DownloadArquivoEstatico/?sistema=NFE&tipoArquivo=3&nomeArquivo=NT_2026.005_v1.00_RTC_SimplesNacional.pdf) |
| **NT 2025.002 v1.53 — RTC** | Leiaute-base de IBS/CBS/IS, grupos de tributação regular, monofasia, finalidades e regras da Reforma Tributária. Deve ser lida em conjunto com a NT do Simples. | [PDF no Portal Nacional da NF-e](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=BMcHSMEoepU=) |
| **NT 2026.006 v1.01 — Vinculação de pagamento** | Define o grupo `gPgtoVinc/gPgto`, seus identificadores e a relação com as transações financeiras. A correção de unicidade do XSD aplica-se a esse contrato. | [PDF no Portal Nacional da NF-e](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=7xf05PpO4Z0=) |
| **IT 2026.001 v1.01 — Meios de pagamento para vinculação** | Tabela de códigos aceitos em `tpMeioPgto`. O modelo utiliza `17`, Pix QR Code, presente nessa tabela. | [PDF na SVRS](https://dfe-portal.svrs.rs.gov.br/DFE/DownloadArquivoEstatico/?sistema=DFE&tipoArquivo=16&nomeArquivo=IT%202026.001%20v.1.01%20-%20Tabelas%20de%20Meios%20de%20Pagamento%20para%20Vincula%C3%A7%C3%A3o%20com%20o%20Split.pdf) |
| **NT 2026.008 v1.01 — Valor líquido** | Referência complementar para formação dos valores de produto, item e documento. Ao integrar os novos grupos, compatibilize o cálculo com a versão aplicável dessa NT. | [PDF no Portal Nacional da NF-e](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=PVjdIcA/H2s=) |

O [Portal de Documentos da SVRS](https://dfe-portal.svrs.rs.gov.br/Nfe/Documentos) reúne as publicações e os pacotes de schemas. Antes de implantar, confira se existe versão posterior das NTs, das tabelas e do pacote utilizado pelo autorizador.

### Cronogramas separados

Na NT 2026.005 v1.00, o quadro de cronograma prevê os campos do Simples em homologação até **26/10/2026** e em produção em **16/11/2026**. Para o evento 412140, o mesmo quadro informa **implementação futura** nos dois ambientes. A disponibilidade dos campos no XSD e na DLL não antecipa a implantação do evento.

A NT 2026.006 v1.01 esclarece que os campos de split payment são preparatórios e não têm exigência de preenchimento em produção durante 2026. Sua ativação efetiva depende do cronograma próprio. Não transforme a presença de `gPgtoVinc` no modelo em obrigatoriedade geral de emissão.

## 2. Modelos disponíveis e como escolher

Os nomes preservam o padrão `CST_000_CLASSTRIB_000001_<cenário>-nfe.xml`. Todos usam o mesmo CST e a mesma classificação para facilitar a comparação da estrutura. Esses códigos são ilustrativos: o enquadramento de cada operação real deve ser parametrizado com base na tabela vigente e na orientação fiscal da empresa.

| Modelo | CRT | `tpRegimeIBSCBS` | Grupo regular `gIBSCBS` | Composição `gTribSN` |
|---|---:|---|---|---|
| [CRT 1 sem regime regular](CST_000_CLASSTRIB_000001_SimplesNacional_CRT1_SemRegimeRegular-nfe.xml) | 1 | Ausente | Ausente | IBS e CBS |
| [CRT 2 sem regime regular](CST_000_CLASSTRIB_000001_SimplesNacional_CRT2_SemRegimeRegular-nfe.xml) | 2 | Ausente | Presente; CBS regular com alíquota zero | Somente CBS |
| [CRT 1 com regime regular](CST_000_CLASSTRIB_000001_SimplesNacional_CRT1_ComRegimeRegular-nfe.xml) | 1 | 1 | Presente | Ausente |
| [CRT 2 com regime regular](CST_000_CLASSTRIB_000001_SimplesNacional_CRT2_ComRegimeRegular-nfe.xml) | 2 | 1 | Presente | Ausente |
| [CRT 3 com opção pendente pelo CRT 1](CST_000_CLASSTRIB_000001_SimplesNacional_CRT3_OpcaoPendenteCRT1-nfe.xml) | 3 | 2 | Presente | IBS e CBS |
| [CRT 3 com opção pendente pelo CRT 2](CST_000_CLASSTRIB_000001_SimplesNacional_CRT3_OpcaoPendenteCRT2-nfe.xml) | 3 | 3 | Presente | Somente CBS |
| [CRT 3 com opção pendente e regime regular](CST_000_CLASSTRIB_000001_SimplesNacional_CRT3_OpcaoPendenteComRegimeRegular-nfe.xml) | 3 | 4 | Presente | Ausente |

Essa matriz corresponde às orientações da seção 2.3 da NT 2026.005. A informação de `gIBSCBS` continua condicionada ao CST da operação. Nos exemplos foi escolhido um CST de tributação regular para tornar visível a diferença entre os regimes.

O primeiro arquivo também contém **dois pagamentos vinculados**, com `nPag` e `idTransacao` distintos. O [modelo do evento 412140](../../Eventos/Evento_412140-ped-eve.xml) referencia a chave desse primeiro documento e seus itens 1 e 2.

### Dados didáticos

- Cada NF-e possui dois produtos, de R$ 100,00 e R$ 50,00, para permitir conferir os somatórios.
- Identificadores, inscrições, endereços, transações e nomes são sintéticos. As chaves estão consistentes com os campos do próprio modelo, inclusive seu dígito verificador; isso não confere validade cadastral aos identificadores fictícios.
- A data ilustrativa é **15/01/2027**. Ela permite estudar os campos em um cenário de transição; não comprova disponibilidade de serviços nem autoriza usar a mesma data em uma emissão real.
- Alíquotas de IBS, CBS e Simples foram escolhidas para demonstrar casas decimais e arredondamento. Não são uma tabela legal de alíquotas.
- O tratamento dos demais tributos é simplificado e não deve ser copiado como regra fiscal para qualquer produto. A classificação real de ICMS, PIS, COFINS e demais tributos precisa ser determinada pela operação.
- Os arquivos são modelos de entrada sem assinatura, no padrão dos exemplos da pasta. Não contêm protocolo de autorização.

## 3. Resumo das mudanças no XML

| Local completo | Mudança | Tratamento pelo ERP |
|---|---|---|
| `NFe/infNFe/emit/tpRegimeIBSCBS` | Novo campo opcional de um dígito, domínio 1–4. | Manter o regime de IBS/CBS separado do `CRT`; emitir a tag somente nas hipóteses previstas. |
| `NFe/infNFe/det/prod/cAtivSN` | Novo código de atividade por item, com dois caracteres. | Classificar cada operação/item; preservar zeros à esquerda. |
| `NFe/infNFe/det/imposto/IBSCBS/gTribSN` | Novo grupo opcional de composição de IBS/CBS do Simples. | Selecionar conforme o enquadramento, preservando os grupos regulares quando a matriz os exigir. |
| `.../gTribSN/gIBSSN` | Grupo opcional com alíquota e valor de IBS. | Distinguir ausência de IBS do Simples de um valor efetivamente informado como zero. |
| `.../gTribSN/gCBSSN` | Grupo obrigatório quando `gTribSN` estiver presente. | Informar alíquota e valor de CBS; não gerar um `gTribSN` vazio. |
| `NFe/infNFe/total/IBSCBSTot/gTribSN` | Novo totalizador, com `vIBSSN` e `vCBSSN`. | Somar os valores já calculados dos itens, por tributo, e controlar presença do grupo. |
| `NFe/infNFe/gPgtoVinc/gPgto` | Correção dos seletores de unicidade do XSD. | Impedir repetição de `nPag` e de `idTransacao` dentro do mesmo grupo. |
| `envEvento/evento/infEvento/detEvento` para `tpEvento=412140` | Novo schema e novo detalhamento tipado. | Preparar leitura e conciliação das informações recebidas da Administração Tributária. |
| Anotações de ICMS no XSD | Correções ortográficas nas descrições e limpeza de espaço no texto do grupo de pagamento. | Sem mudança de tag, domínio ou tipo nesses trechos; não exigem alteração no XML do ERP. |

Os schemas alterados acrescentam os campos do Simples; não houve remoção ou mudança de tipo das propriedades públicas preexistentes da serialização. Os seletores atualizados dos pagamentos mantêm a exigência de identificadores únicos: XMLs com duplicidades precisam ser corrigidos pelo ERP.

## 4. Regime do emitente: `tpRegimeIBSCBS`

`CRT` identifica o regime tributário do emitente. O novo `tpRegimeIBSCBS` identifica situações específicas do regime de **IBS/CBS**, sem substituir o CRT.

| Valor | Quando informar |
|---:|---|
| 1 | CRT 1 ou 2 com opção pelo regime regular de IBS/CBS. |
| 2 | CRT 3 com opção pendente pelo CRT 1, sem opção pelo regime regular de IBS/CBS. |
| 3 | CRT 3 com opção pendente pelo CRT 2, sem opção pelo regime regular de IBS/CBS. |
| 4 | CRT 3 com opção pendente pelo Simples, mas com regime regular de IBS/CBS. |

Na definição da NT, a opção pendente corresponde ao pedido indeferido cujo indeferimento está em discussão administrativa ou judicial. Não classifique automaticamente qualquer solicitação em análise como opção pendente. Os contribuintes nessa situação são tratados como do regime regular para os fins descritos na NT.

Para CRT 1 ou 2 sem opção pelo regime regular, **omita a tag**. Também não preencha `0`, string vazia ou o próprio valor de CRT como substituto. CRT 3 sem essa situação especial e MEI não devem receber um código inventado para o campo.

No XSD, a sequência coloca a nova tag **depois de `CRT` e antes de `ISUFEmit`**, quando a inscrição na Suframa existir. A ordenação é relevante para validadores baseados em `xs:sequence`.

O cadastro do ERP deve permitir identificar separadamente o CRT, a opção pelo regime regular e a situação que fundamenta o código 2, 3 ou 4. A emissão deve usar a situação aplicável ao documento, e não somente uma marcação genérica de que a empresa é do Simples.

## 5. Atividade por item: `cAtivSN`

O código fica em **`det/prod`**, não no emitente nem dentro do grupo de impostos. No XSD recebido, vem depois de `indBemMovelUsado`, quando existente, e antes das declarações de importação `DI`. Os exemplos não possuem esses opcionais e mostram `cAtivSN` depois de `indTot`.

| Código XML | Classificação descrita no leiaute |
|---|---|
| `01` | Revenda e venda de mercadorias industrializadas pelo contribuinte, conforme o Anexo I indicado pela NT. |
| `02` | Demais bens materiais sem incidência de ICMS, conforme o Anexo I. |
| `03` | Mercadorias com industrialização incentivada na Zona Franca de Manaus, conforme o Anexo II. |
| `04` | Demais bens materiais com industrialização incentivada na Zona Franca de Manaus, sem ICMS, conforme o Anexo II. |
| `90` | Operações não tributáveis. |

Os códigos devem ter **dois caracteres**: `01` não pode virar `1`. Essa classificação não é o CNAE da empresa, o CFOP ou o código de classificação tributária de IBS/CBS. Ela deve ser definida para a atividade da operação de cada item, em conjunto com sua classificação fiscal.

A NT orienta o preenchimento para emitentes CRT 1 ou 2 e para CRT 3 com `tpRegimeIBSCBS` 2, 3 ou 4. A ocorrência `0–1` no XSD define a estrutura permitida; não transforma em facultativa uma informação exigida pela orientação de preenchimento aplicável.

## 6. IBS/CBS do Simples no item

O caminho novo é `det/imposto/IBSCBS/gTribSN`. Ele é irmão dos grupos de tributação regular e dos demais grupos de IBS/CBS, e não filho de `gIBSCBS`.

```xml
<IBSCBS>
  <CST>000</CST>
  <cClassTrib>000001</cClassTrib>
  <gTribSN>
    <gIBSSN>
      <pIBSSN>1.2345</pIBSSN>
      <vIBSSN>1.23</vIBSSN>
    </gIBSSN>
    <gCBSSN>
      <pCBSSN>2.3456</pCBSSN>
      <vCBSSN>2.35</vCBSSN>
    </gCBSSN>
  </gTribSN>
</IBSCBS>
```

Esse recorte representa o **primeiro item de CRT 1 sem regime regular**. Para os cenários com opção pendente, a matriz pode exigir também `gIBSCBS`, que deve aparecer antes de `gTribSN`.

| Campo/grupo | Ocorrência dentro do pai | Formato e significado |
|---|---|---|
| `gTribSN` | 0–1 em `IBSCBS` | Composição do IBS/CBS do Simples para o item. |
| `gIBSSN` | 0–1 em `gTribSN` | Informar somente quando houver composição de IBS do Simples nesse cenário. |
| `pIBSSN` | 1–1 em `gIBSSN` | Percentual com duas a quatro casas decimais. `1.2345` significa 1,2345%, não 123,45%. |
| `vIBSSN` | 1–1 em `gIBSSN` | Valor monetário com duas casas decimais. |
| `gCBSSN` | 1–1 em `gTribSN` | Composição da CBS do Simples; obrigatório se o grupo pai for informado. |
| `pCBSSN` | 1–1 em `gCBSSN` | Percentual com duas a quatro casas decimais. |
| `vCBSSN` | 1–1 em `gCBSSN` | Valor monetário com duas casas decimais. |

Os valores são não negativos nos tipos do XSD. Use ponto como separador decimal e não aplique separador de milhar.

### Seleção dos grupos

- **CRT 1 sem regime regular:** informar `gTribSN` com IBS e CBS; não formar o grupo regular desse cenário por cópia de um modelo CRT 3.
- **CRT 2 sem regime regular:** informar `gIBSCBS` quando exigido pelo CST, com CBS regular de alíquota zero, e `gTribSN` com CBS do Simples. O IBS regular permanece no grupo regular; não zere todo o `gIBSCBS` por causa da orientação específica da CBS.
- **CRT 1 ou 2 com `tpRegimeIBSCBS=1`:** usar a tributação regular; não informar `gTribSN`.
- **CRT 3 com `tpRegimeIBSCBS=2` ou `3`:** preservar os grupos regulares e acrescentar a composição do Simples indicada na matriz. São informações distintas e precisam ser armazenadas separadamente.
- **CRT 3 com `tpRegimeIBSCBS=4`:** usar o regime regular; não informar `gTribSN`.

No `xs:choice` preexistente, `gIBSCBS`, `gIBSCBSMono`, `gTransfCred` e `gAjusteCompet` são alternativas. O novo `gTribSN` está **depois desse choice**, antes de `gEstornoCred` e dos grupos de crédito presumido. Não elimine essa separação nem gere alternativas incompatíveis no mesmo item.

### Situações com orientação específica

A NT 2026.005 orienta que, para **CST 620, monofasia de combustíveis**, seja utilizado `gIBSCBSMono` e não `gTribSN`. Para **MEI, CRT 4**, orienta não informar `IBSCBS`.

Nas compras governamentais de 2027 e 2028, quando `tpEnteGov=1` — União — a NT determina `pIBSSN` zero e `pCBSSN` com a soma das alíquotas efetivas de IBS e CBS do Simples. Para os demais entes, os percentuais seguem o tratamento indicado para cada tributo. Os modelos fornecidos não simulam compra governamental.

Para devolução (`finNFe=4`) e os retornos por recusa previstos na seção 2.4 — crédito tipo 03 ou 06 e débito tipo 09 — o grupo `IBSCBS` do item deve espelhar o da nota de fornecimento referenciada. Evite recalcular essa estrutura usando apenas o cadastro atual do emitente, ignorando o documento original.

## 7. Totalizadores e arredondamento

O novo totalizador fica em **`total/IBSCBSTot/gTribSN`**. Ele tem o mesmo nome do grupo do item, mas sua estrutura é diferente: contém diretamente dois valores, sem os subgrupos `gIBSSN` e `gCBSSN`.

```xml
<gTribSN>
  <vIBSSN>1.85</vIBSSN>
  <vCBSSN>3.52</vCBSSN>
</gTribSN>
```

Esses são os totais dos dois itens do primeiro modelo:

| Item | Valor ilustrativo | `pIBSSN` | `vIBSSN` | `pCBSSN` | `vCBSSN` |
|---|---:|---:|---:|---:|---:|
| 1 | 100,00 | 1,2345% | 1,23 | 2,3456% | 2,35 |
| 2 | 50,00 | 1,2345% | 0,62 | 2,3456% | 1,17 |
| Soma dos valores dos itens | 150,00 | — | **1,85** | — | **3,52** |

Os valores didáticos acima resultam da aplicação dos percentuais ao valor indicado e arredondamento monetário de cada item. O XSD de `gTribSN` não contém uma tag de base de cálculo: não deduza dessa demonstração que a base legal de toda operação será sempre `vProd`. O motor fiscal deve apurar a base e os valores conforme a operação e a legislação aplicável.

Para totalizar, some os **valores monetários informados nos itens**, já arredondados. Neste exemplo, aplicar 1,2345% diretamente sobre 150,00 produziria 1,85175, mas o que deve ser conciliado no total é 1,23 + 0,62. Essa disciplina evita divergências em notas com muitos itens e percentuais variáveis.

Na ausência de IBS do Simples nos itens, como no modelo CRT 2 sem regime regular, o total `vIBSSN` é `0.00`; `vCBSSN` mantém a soma da CBS do Simples. Os dois campos são obrigatórios quando o totalizador `gTribSN` existe.

`vBCIBSCBS`, os totais regulares `gIBS/gCBS` e o totalizador do Simples possuem funções diferentes. Não acumule o mesmo valor em dois totalizadores por equivalência de nomes. No primeiro modelo não há `gIBSCBS` nos itens e `vBCIBSCBS` foi mantido em `0.00`; nos modelos regulares a base demonstrativa é 150,00.

### Regras de validação da NT 2026.005

| Regra | Código de rejeição | Verificação para o DEV |
|---|---:|---|
| W59h-10 | 1275 | Havendo `gTribSN` em algum item, o totalizador correspondente precisa existir. |
| W59h-20 | 1276 | Sem `gTribSN` nos itens, não gerar o totalizador do Simples. |
| W59i-10 | 1277 | Total de IBS do Simples deve coincidir com a soma dos valores de IBS do Simples dos itens. |
| W59j-10 | 1284 | Total de CBS do Simples deve coincidir com a soma dos valores de CBS do Simples dos itens. |

As regras de somatório não são expressas apenas pela estrutura do XSD. Inclua conferências no ERP antes do envio; a aprovação no schema não comprova que todos os cálculos fiscais estão corretos.

## 8. Evento 412140 — Tributação do Simples Nacional

Arquivo: [Evento_412140-ped-eve.xml](../../Eventos/Evento_412140-ped-eve.xml).

O modelo contém dois itens, todas as tags novas de seu schema e os totais correspondentes. Referencia a primeira NF-e desta coleção para permitir cruzar `chNFe` e `nItem`.

A função do evento é registrar a tributação apurada pela Administração Tributária para composição da receita bruta, confissão de dívida e eventual crédito de IBS/CBS nos termos da NT. O ERP deve preservar a NF-e original e tratar o evento como informação vinculada, com identificação própria, e não como uma edição do XML originalmente autorizado.

### Envelope e cabeçalho específico

| Campo | Tratamento |
|---|---|
| `envEvento/@versao`, `evento/@versao`, `verEvento`, `detEvento/@versao` | O modelo utiliza `1.00`. A NF-e referenciada continua no leiaute 4.00. |
| `tpEvento` | `412140`. |
| `infEvento/@Id` | `ID` + código do evento + chave de 44 posições + sequencial com duas posições. |
| `chNFe` | Chave do documento ao qual os itens e totais se referem. |
| `CNPJ` do envelope | Identificador do autor; no modelo é sintético. Não presuma que todo evento seja assinado pelo emitente da NF-e. |
| `descEvento` | Descrição do evento do Simples. O XSD aceita as grafias com e sem acentuação previstas no próprio schema. |
| `cOrgaoAutor` | Código do órgão autor conforme o domínio do XSD; o modelo usa a UF 41. |
| `tpAutor` | **5 — Fisco**. O schema específico só aceita esse valor. |
| `verAplic` | Versão do aplicativo que recebeu o evento. |

### Detalhamento `gTribItemSN`

O grupo pode ocorrer de **1 a 990 vezes**. O atributo `nItem` deve preservar a associação com o número do item da NF-e; o domínio do XSD é de 1 a 990. Não associe os itens apenas pela posição em que foram recebidos na lista.

| Campo | Ocorrência | Significado |
|---|---|---|
| `@nItem` | Obrigatório | Número do item referenciado. |
| `vRBSNitem` | Obrigatório | Receita bruta do Simples atribuída ao item. O nome XML utiliza `item` com **i minúsculo**. |
| `tpRBSN` | Obrigatório | Classificação da receita no domínio 0, 1, 2, 3, 4, 5 ou 9. |
| `pIBSSN` | Opcional | Percentual de IBS aplicável ao item. |
| `vIBSSN` | Opcional | Valor de IBS devido pelo Simples. |
| `pCBSSN` | Opcional | Percentual de CBS aplicável ao item. |
| `vCBSSN` | Opcional | Valor de CBS devido pelo Simples. |
| `vIBSPendSusp` | Opcional | Valor de IBS pendente ou suspenso do item. |
| `vCBSPendSusp` | Opcional | Valor de CBS pendente ou suspenso do item. |

Os percentuais têm duas a quatro casas decimais; os valores monetários, duas. Preserve a diferença entre **tag ausente** e **tag informada com zero**. O segundo item do evento contém `vCBSPendSusp=0.00` justamente para demonstrar zero explícito.

| `tpRBSN` | Classificação |
|---:|---|
| 0 | Não integra receita bruta. |
| 1 | Receita bruta interna. |
| 2 | Receita bruta interna sem cálculo de IBS/CBS. |
| 3 | Receita bruta de exportação direta. |
| 4 | Receita bruta de exportação indireta. |
| 5 | Receita bruta de mercado interno e exportação. |
| 9 | Fornecimento incompatível com o Simples Nacional. |

`tpRBSN=0` é um valor válido e obrigatório quando essa classificação se aplicar; não é um indicador de ausência.

### Totais `gTotalSN`

O grupo ocorre uma vez e contém:

| Campo | Ocorrência | Valor no modelo |
|---|---|---:|
| `vRBSNtot` | Obrigatório | 150.00 |
| `vIBSSNtot` | Opcional | 1.85 |
| `vIBSSNtotPendSusp` | Opcional | 0.15 |
| `vCBSSNtot` | Opcional | 3.52 |
| `vCBSSNtotPendSusp` | Opcional | 0.20 |

O trecho `tot` desses nomes XML começa com **t minúsculo**. Não substitua essas tags por `vRBSNTot`, `vIBSSNTot` ou nomes de grupos utilizados por outros documentos fiscais.

A base de dados deve distinguir valores devidos, pendentes e suspensos conforme os campos recebidos. Não some ou subtraia esses componentes automaticamente para gerar crédito disponível sem aplicar as regras da apuração e da escrituração correspondentes.

### Leitura e conciliação

1. Identifique o código 412140 e mantenha sua versão, chave, autor e sequencial.
2. Leia cada item pelo atributo `nItem` e vincule-o ao item original.
3. Preserve a classificação de receita e a presença ou ausência de cada valor opcional.
4. Mantenha os totais recebidos e as diferenças encontradas em relação à informação original, para conciliação.
5. Ao receber novamente a mesma identificação de evento, evite duplicar efeitos na base de dados.
6. Trate o eventual cancelamento conforme o vínculo do evento original, preservando o histórico.

A NT prevê cancelamento pelo **evento genérico 110001**. Não foi criado um novo evento de cancelamento 412141 para NF-e nesta atualização. O [modelo genérico já existente](../../Eventos/Evento_110001-ped-eve.xml) deve ser estudado em conjunto com a NT e as regras de autoria; a existência desse modelo não habilita a empresa a cancelar um evento gerado pelo Fisco.

## 9. Pagamentos vinculados: validação de unicidade

No `leiauteNFe_v4.00.xsd`, os seletores dos constraints `pk_gPgto_nPag` e `pk_gPgto_idTransacao` passaram de `editix:gPgto` para `./*`. O efeito prático esperado é selecionar os filhos do grupo e impedir duplicidades dos dois atributos. A estrutura dos campos de pagamento não foi renomeada por essa correção.

O primeiro modelo contém:

```xml
<gPgtoVinc>
  <gPgto nPag="1" idTransacao="TRANSACAO-TESTE-001">
    <tpMeioPgto>17</tpMeioPgto>
    <CNPJReceb>99999999000191</CNPJReceb>
    <CNPJBasePSP>00000000</CNPJBasePSP>
  </gPgto>
  <gPgto nPag="2" idTransacao="TRANSACAO-TESTE-002">
    <tpMeioPgto>17</tpMeioPgto>
    <CNPJReceb>99999999000191</CNPJReceb>
    <CNPJBasePSP>00000000</CNPJBasePSP>
  </gPgto>
</gPgtoVinc>
```

Ao gerar esse grupo:

- aceite de 1 a 99 ocorrências de `gPgto`; não gere um grupo vazio;
- mantenha `nPag` único, sem zeros à esquerda, respeitando o intervalo de 1 a 990;
- mantenha `idTransacao` único, com 2 a 35 caracteres; preserve o identificador recebido da transação financeira em uso real;
- preserve os tamanhos de `CNPJReceb` — 14 posições — e `CNPJBasePSP` — 8 posições — inclusive zeros à esquerda e o formato de identificador aceito pelo schema;
- consulte a tabela própria para `tpMeioPgto`; o pattern de dois dígitos no XSD não garante, sozinho, que o código exista na tabela oficial;
- mantenha também o grupo normal `pag/detPag`. A vinculação não substitui as informações de pagamento já exigidas.

No exemplo, `17` representa Pix QR Code na tabela do IT consultado. Os identificadores `TRANSACAO-TESTE-*` e a base de PSP com zeros são fictícios e devem ser substituídos na integração real. `gPgto` não possui uma tag `vPag` nesse contrato; não acrescente campos por semelhança com `detPag` ou com outro evento.

Para testar a rejeição, faça uma cópia do modelo e repita primeiro `nPag`; depois restaure o número e repita somente `idTransacao`. Os dois casos devem falhar no schema, cada um pelo seu constraint. Mantenha os modelos originais válidos.

## 10. Uso das classes da Unimake.DFe

As propriedades públicas preservam o padrão de XML tipado da biblioteca. A aplicação informa os dados já apurados; acrescentar propriedades de serialização não cria um motor automático de cálculo fiscal.

| XML | Propriedade/classe |
|---|---|
| `emit/tpRegimeIBSCBS` | `Emit.TpRegimeIBSCBS`, enum `TipoRegimeIBSCBS`. |
| `det/prod/cAtivSN` | `Prod.CAtivSN`, enum `CodigoAtividadeSimplesNacional`. |
| `IBSCBS/gTribSN` | `IBSCBS.GTribSN`, classe `GTribSN`. |
| `gTribSN/gIBSSN` | `GTribSN.GIBSSN`, classe `GIBSSN`. |
| `gTribSN/gCBSSN` | `GTribSN.GCBSSN`, classe `GCBSSN`. |
| `IBSCBSTot/gTribSN` | `IBSCBSTot.GTribSN`, classe `GTribSNTot`. |
| `detEvento` do código 412140 | `DetEventoTributacaoSN`, identificado por `TipoEventoNFe.Evento412140`. |
| `gTribItemSN` e `gTotalSN` | `GTribItemSN` e `GTotalSN`. |

### Recorte C#: um item de CRT 1 sem regime regular

O trecho abaixo usa valores didáticos já calculados. Ele pressupõe um objeto `inf` do tipo `InfNFe` com emitente, item, imposto e total inicializados. Para a nota com dois itens desta pasta, os totais devem ser 1,85 e 3,52; no recorte de um item, são 1,23 e 2,35.

```csharp
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFe;

inf.Emit.CRT = CRT.SimplesNacional;
#if INTEROP
inf.Emit.TpRegimeIBSCBS = (TipoRegimeIBSCBS)(-1);
#else
inf.Emit.TpRegimeIBSCBS = null;
#endif
inf.Det[0].Prod.CAtivSN = CodigoAtividadeSimplesNacional.RevendaOuVendaMercadorias;
inf.Det[0].Imposto.IBSCBS = new IBSCBS
{
    CST = "000",
    CClassTrib = "000001",
    GTribSN = new GTribSN
    {
        GIBSSN = new GIBSSN { PIBSSN = 1.2345, VIBSSN = 1.23 },
        GCBSSN = new GCBSSN { PCBSSN = 2.3456, VCBSSN = 2.35 }
    }
};
inf.Total.IBSCBSTot = new IBSCBSTot
{
    VBCIBSCBS = 0,
    GTribSN = new GTribSNTot { VIBSSN = 1.23, VCBSSN = 2.35 }
};
```

Para um cenário com regime regular, o enum de valor 1 é `TipoRegimeIBSCBS.SimplesNacionalRegimeRegular`. A mudança desse campo precisa acompanhar a seleção dos grupos indicada na matriz; alterar apenas o emitente e manter os grupos do cenário anterior produz uma combinação incorreta.

### Opcionais, formatação e INTEROP

Em .NET, `TpRegimeIBSCBS` e `CAtivSN` são enums anuláveis. Na compilação INTEROP, usam a sentinela `-1` para ausência, com os métodos `ShouldSerialize...` controlando a omissão. A sentinela é um detalhe da API e não deve aparecer no XML.

No evento, os valores numéricos opcionais também distinguem ausência de zero: em .NET usam nullable; em INTEROP a ausência é representada por `-1`. O zero explícito deve ser preservado. Os valores monetários obrigatórios dos novos grupos da NF-e não usam essa convenção de ausência: se o grupo existir, suas tags obrigatórias precisam ser serializadas.

Os membros `...Field` tratam a representação textual XML. Para atribuir e ler valores na aplicação, use as propriedades principais. A serialização usa duas casas para dinheiro e duas a quatro para os percentuais novos, sem depender da cultura regional do Windows.

Para linguagens COM, `DetEventoTributacaoSN` disponibiliza `AddGTribItemSN`, `GetGTribItemSN` e `GetGTribItemSNCount`. O índice de acesso da lista começa em zero; ele não substitui o número fiscal `NItem` de cada objeto.

### Leitura do modelo do evento

```csharp
using Unimake.Business.DFe.Utility;
using Unimake.Business.DFe.Xml.NFe;

var lote = new EnvEvento().LoadFromFile(caminhoDoArquivo);
var infEvento = lote.Evento[0].InfEvento;
var detalhe = infEvento.DetEvento as DetEventoTributacaoSN;
if (detalhe != null)
{
    foreach (var item in detalhe.GTribItemSN)
    {
        var numeroDoItem = item.NItem;
        var receita = item.VRBSNItem; // XML: vRBSNitem
        var classificacao = item.TpRBSN;
        // Persistir os valores recebidos e sua presença/ausência para conciliação.
    }
    var receitaTotal = detalhe.GTotalSN.VRBSNTot; // XML: vRBSNtot
}
```

O código não executa um serviço fiscal. Para XML recebido como string ou `XmlDocument`, também existe `XMLUtility.Deserializar<EnvEvento>(...)`.

## 11. XML, TXT e integração com o UniNFe

Os arquivos desta entrega são **XMLs**. O suporte das novas propriedades de serialização não implica que o catálogo TXT ganhou segmentos ou colunas correspondentes. Não acrescente campos nos segmentos `C`, `I` ou `UB` por conta própria. Para uso em TXT, confirme o leiaute publicado e a implementação do conversor da versão efetivamente instalada.

Para usar a NF-e como base de uma emissão, substitua os dados fictícios, escolha o cenário correto, recalcule os valores e a chave e siga o [fluxo de autorização por arquivo do UniNFe](../../../../docs/servicos/nfe/autorizacao.md). Os nomes desta pasta são descritivos; no fluxo de integração utilize o padrão `<chave>-nfe.xml` documentado para a empresa configurada.

O UniNFe utiliza a Unimake.DFe por pacote NuGet. Confirme a versão efetivamente restaurada e instalada; ter o código-fonte atualizado em outro checkout não atualiza os schemas nem as classes usados pelo aplicativo. Para desenvolvimento conjunto, siga o fluxo oficial de pacote offline e mantenha as referências de versão apenas locais enquanto o pacote existir somente no feed offline.

O [modelo do evento do Fisco](../../Eventos/Evento_412140-ped-eve.xml) não deve ser colocado na pasta monitorada de envio da empresa. A extensão foi mantida para acompanhar a convenção da coleção de exemplos, mas a autoria e o fluxo de disponibilização fiscal são os definidos pela documentação oficial.

## 12. Conferências antes de integrar

1. Identifique CRT e situação de IBS/CBS; selecione o modelo correspondente, inclusive presença ou ausência de `tpRegimeIBSCBS`.
2. Classifique `cAtivSN` por item, mantendo dois caracteres e compatibilidade com a operação.
3. Aplique a matriz de grupos e as exceções de monofasia e MEI; respeite as alternativas do schema.
4. Calcule os tributos no motor fiscal e totalize os valores dos itens, mantendo separado o que é regular e o que é do Simples.
5. Confira as condições de presença de `gTribSN` nos itens e nos totais, além dos somatórios.
6. Valide o grupo de pagamentos quanto à quantidade, domínios e unicidade; preserve a informação normal de pagamento.
7. Faça ida e volta do XML com a versão da DLL utilizada pelo consumidor, verificando zero, ausência, casas decimais, namespace e ordem.
8. Para o evento, valide envelope e detalhe específico e preserve autoria, chave, sequencial, itens e totais para conciliação.
9. Use certificados e dados próprios somente no ambiente apropriado e acompanhe o cronograma oficial de cada recurso.

Os oito modelos desta coleção foram conferidos contra os schemas correspondentes. Como os schemas completos exigem assinatura, a verificação do documento/envelope foi feita em cópias assinadas em memória com certificado temporário; as assinaturas não foram gravadas nos exemplos. Foram conferidos os totais, os cenários de regime, a referência do evento e a rejeição de duplicidades dos dois atributos de pagamento. Essa validação estrutural não equivale a autorização pela SEFAZ nem a uma auditoria fiscal completa da operação real.
