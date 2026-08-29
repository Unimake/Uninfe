# Reforma Tributária - NFe/NFCe

Este manual orienta a classificação IBS/CBS de itens de NFe e NFCe com as tabelas disponibilizadas pela Unimake.

**Versão de referência:** arquivos disponibilizados pela Unimake e consultados em 06/08/2026.

**Objetivo:** selecionar, para cada item do documento fiscal, o par correto `CST` + `cClassTrib` de IBS/CBS e usar as tabelas como fonte de validação e rastreabilidade legal.

## 1. Visão geral

As oito tabelas não são alternativas: elas se complementam. Algumas ajudam a decidir a tributação; outras validam o resultado e indicam os grupos técnicos do XML.

| Tabela | Papel na solução |
| --- | --- |
| [Tabela de Operações](https://www.unimake.com.br/downloads/Tabela_Operacao.json) | Ponto de partida. Classifica a natureza da operação e fornece o enquadramento padrão ou prioritário. |
| [Tabela CFOP](https://www.unimake.com.br/downloads/tabela_cfop.json) | Catálogo dos CFOPs com descrição, tipo e destino da operação, vigência e indicadores de aplicação, inclusive para NF-e e exclusão de IBS/CBS. |
| [Tabela NCM](https://www.unimake.com.br/downloads/tabela_ncm.json) | Identifica o produto e aponta enquadramento direto, anexos ordinários e exceções legais prioritárias. |
| [Tipos de aplicação da exceção prioritária](https://www.unimake.com.br/downloads/tabela_ncm_tipo_aplicacao_anexo_excecao_prioritaria.json) | Traduz o `Tipo_Aplicacao` de uma exceção do NCM em condição de negócio e pergunta orientadora. Ajuda a decidir quando a aplicação pode automatizar e quando deve solicitar confirmação. |
| [Tabela de Anexos da LC 214](https://www.unimake.com.br/downloads/tabela_anexos_lc214.json) | Catálogo descritivo dos Anexos I a XV. É usado para interpretar a finalidade de cada anexo apontado pelo NCM; não contém a regra de seleção por si só. |
| [Tabela CST IBS/CBS](https://www.unimake.com.br/downloads/tabela_cst_ibscbs.json) | Define a situação tributária e quais grupos técnicos do leiaute podem/devem ser preenchidos para cada CST. |
| [Tabela CST e cClassTrib IBS/CBS](https://www.unimake.com.br/downloads/tabela_cst_classtrib_ibscbs.json) | Fonte final de validação da combinação `CST` + `cClassTrib`, vigência, fundamento legal, percentuais de redução e compatibilidade com cada DF-e. |
| [Tabela de Crédito Presumido IBS/CBS](https://www.unimake.com.br/downloads/tabela_ccredpres.json) | Valida o `cCredPres`, a forma de apropriação, os grupos de IBS/CBS, alíquotas e vigências quando a operação e a classificação permitirem crédito presumido. |

O `cClassTrib` detalha a hipótese legal e o seu prefixo de três posições deve corresponder ao `CST`. Exemplo: `200035` pertence ao CST `200`.

> Importante: NCM sozinho não prova o benefício. Quando o mesmo NCM possui mais de um anexo ou exceção, a aplicação deve usar fatos conhecidos da operação e do cadastro. Se ainda restar mais de uma resposta possível, deve pedir a decisão ao usuário e guardar a origem dessa decisão; não deve escolher o primeiro registro nem cair silenciosamente na regra genérica.

## 2. Relação entre as tabelas

```mermaid
%%{init: {"flowchart": {"useMaxWidth": false}} }%%
flowchart TD
    A["Selecionar a operação"] --> B{"Prioridade<br/>= S?"}
    B -- "Sim" --> Z["Usar CST e cClassTrib<br/>da operação"]
    B -- "Não" --> C{"Há regra específica<br/>já validada?"}
    C -- "Sim" --> P["Revalidar regra do item<br/>na data do documento"]
    C -- "Não" --> D["Localizar NCM<br/>vigente do item"]
    D --> E{"NCM tem classificação<br/>direta?"}
    E -- "Sim" --> N["Usar CST e cClassTrib<br/>diretos do NCM"]
    E -- "Não" --> F["Avaliar exceções com<br/>Tipo_Aplicacao"]
    F --> G{"Uma hipótese foi<br/>comprovada?"}
    G -- "Sim" --> X["Usar CST e cClassTrib<br/>da exceção"]
    G -- "Não há exceção" --> H["Avaliar anexos,<br/>finalidade e vigência"]
    G -- "Ambíguo" --> U["Solicitar decisão<br/>ao usuário"]
    H --> I{"Resultado dos<br/>anexos"}
    I -- "Um aplicável" --> J["Usar CST e cClassTrib<br/>do anexo"]
    I -- "Nenhum aplicável" --> K["Usar padrão<br/>da operação"]
    I -- "Mais de um possível" --> U
    U --> L["Guardar escolha, fatos<br/>e origem da decisão"]
    Z --> V["Validar na tabela<br/>CST + cClassTrib"]
    P --> V
    N --> V
    X --> V
    J --> V
    K --> V
    L --> V
    V --> Q{"Há crédito<br/>presumido?"}
    Q -- "Sim" --> R["Validar cCredPres,<br/>vigência e grupos"]
    Q -- "Não" --> S["Montar grupos IBS/CBS<br/>permitidos no XML"]
    R --> S
```

### Precedência de seleção

1. **Operação com `Prioridade = "S"`**: use o `CST` e o `cClassTrib` da própria operação. Não permita que uma regra genérica de NCM substitua uma operação que encerra a decisão.
2. **Regra específica já confirmada**: se o ERP guarda uma escolha fiscal para a combinação exata de operação e item, ela pode ser reutilizada, mas deve ser revalidada pela data do documento e pelas tabelas atuais.
3. **Operação com `Prioridade = "N"`**: localize o NCM completo e vigente do item.
4. Se o NCM possuir `CST` e `cClassTrib` diretamente, use a dupla indicada.
5. Sem classificação direta, avalie `Anexos_excecao_prioritaria`. Relacione `Tipo_Aplicacao` com a tabela de tipos e responda à pergunta orientadora usando dados conhecidos, como perfil do adquirente, destinação, registro do produto ou outra condição legal.
6. Se nenhuma exceção for comprovada, avalie os `Anexos` ordinários, inclusive `Aplicabilidade.Condicao`, `Aplicabilidade.Excecao` e vigência.
7. Quando o NCM não produzir candidato aplicável, use o `CST` e o `cClassTrib` padrão da operação.
8. Quando houver candidatos, mas os fatos disponíveis não permitirem escolher um único resultado, solicite a decisão ao usuário. Não selecione o primeiro candidato e não esconda a ambiguidade usando a operação genérica.
9. Valide o resultado na tabela CST/cClassTrib: prefixo, vigência, `indNFe` ou `indNFCe`, reduções e grupos condicionais.
10. Se houver crédito presumido, valide também o código na tabela `cCredPres`, suas vigências e a forma de apropriação.

Finalidade, destinação, perfil do adquirente e demais condições materiais são fatos de negócio. Parte deles aparece em textos como `Aplicabilidade.Condicao` e `Pergunta_Orientadora`, mas o JSON não sabe se o item concreto satisfaz a condição. A aplicação deve obter esses fatos do cadastro, da operação, do destinatário ou do usuário.

## 3. Algoritmo de referência

```text
operacao = localizar Tabela_Operacao pelo Codigo

se operacao.Prioridade == "S":
    resultado = (operacao.CST, operacao.cClassTrib)
senão:
    se existe regra específica do item/operação já confirmada e vigente:
        resultado = regra específica
    senão:
        ncm = localizar Tabela_NCM.Nomenclaturas pelo Codigo e data do documento

        se ncm possui CST e cClassTrib diretos:
            resultado = (ncm.CST, ncm.cClassTrib)
        senão:
            para cada excecao em ncm.Anexos_excecao_prioritaria:
                tipo = localizar Tipos pelo excecao.Tipo_Aplicacao
                avaliar Pergunta_Orientadora e Aplicabilidade com fatos conhecidos

            excecoes_comprovadas = filtrar exceções atendidas pelos fatos da operação
            se exatamente uma exceção for comprovada:
                resultado = (excecao.CST, excecao.cClassTrib)
            senão se houver mais de uma exceção comprovada ou possível:
                solicitar decisão ao usuário e registrar fatos e escolha
            senão:
                anexos = filtrar ncm.Anexos por finalidade, condição e vigência
                se exatamente um anexo for aplicável:
                    resultado = (anexo.CST, anexo.cClassTrib)
                senão se não houver candidato aplicável:
                    resultado = (operacao.CST, operacao.cClassTrib)
                senão:
                    solicitar decisão ao usuário e registrar fatos e escolha

validar que (resultado.CST, resultado.cClassTrib) exista na tabela CST/cClassTrib;
validar vigência, DF-e e indicadores de grupos do leiaute;
se operacao.CodCredPresumido não estiver vazio, validar também na tabela específica cCredPres.
guardar no documento a fotografia do resultado e a versão/hash das tabelas utilizadas.
```

## 4. Tabela de Operações — `Tabela_Operacao.json`

É a tabela inicial. O usuário ou a regra de negócio seleciona uma linha por seu `Codigo`; a tabela não substitui a análise da situação concreta quando a operação não é prioritária.

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `Codigo` | Identificador da operação na tabela. | Chave de pesquisa. Deve ser guardado na parametrização da operação do ERP. Ex.: `00100` = venda de produção própria ou de terceiros. |
| `Descricao` | Nome legível da natureza da operação. | Apresentar ao usuário e usar como apoio à configuração; não deve ser usada como chave técnica. |
| `CST` | CST IBS/CBS padrão ou prioritário da operação. | Use junto com o `cClassTrib` da mesma linha. Nunca use isoladamente para definir toda a tributação. |
| `cClassTrib` | Classificação tributária padrão ou prioritária. | É o resultado quando `Prioridade = S` ou o fallback quando o NCM não produzir regra aplicável. |
| `Prioridade` | Define se a classificação da operação encerra a decisão. | `S`: a dupla `CST`/`cClassTrib` da operação prevalece e a pesquisa de NCM/anexos é ignorada. `N`: permite a especialização pelo NCM. |
| `CodCredPresumido` | Código de crédito presumido associado à operação, quando houver. | Se preenchido, valide na [Tabela de Crédito Presumido IBS/CBS](https://www.unimake.com.br/downloads/tabela_ccredpres.json). Não presuma que todo item terá crédito. |
| `Atualizar` | Indicador operacional de atualização/manutenção da regra da operação. | Na versão analisada, todas as linhas trazem `True`. Trate-o como metadado de controle da tabela; ele não altera a precedência do `cClassTrib`. |
| `EmergenciaNacional` | Marca uma operação relacionada a hipótese de emergência nacional. | Campo de apoio à regra de negócio. Está `False` em todas as linhas atuais; se vier `True` em atualização futura, a regra correspondente deve ser revisada à luz da legislação e do leiaute vigente. |
| `Doacao` | Sinaliza operação de doação/amostra gratuita. | Permite distinguir a operação que demanda o tratamento fiscal próprio de doação. Ex.: operação `00920` possui `True`. |
| `modalidadeOperacao` | Caracteriza a onerosidade e a tributação da operação. | Informação para a regra de negócio e conferência: `1-Onerosa e Tributada`, `2-Onerosa e Não Tributada` ou `3-Não onerosa e Não Tributada`. Não substitui `CST`/`cClassTrib`. |

## 5. Tabela NCM — `tabela_ncm.json`

O arquivo possui um cabeçalho e o vetor `Nomenclaturas`. Há registros hierárquicos de capítulo, posição, subposição e NCM completo; para a tributação do item, a pesquisa deve usar o código NCM efetivamente informado, respeitando pontuação e vigência.

### 5.1 Campos de cabeçalho

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `Data_Ultima_Atualizacao_NCM` | Informação de vigência/atualização global da NCM usada no arquivo. | Registrar na auditoria da carga e comparar com versões posteriores. |
| `Ato` | Ato normativo de referência da nomenclatura. | Dado documental; não participa da escolha de `cClassTrib`. |
| `Nomenclaturas` | Lista de registros NCM e seus vínculos fiscais. | Coleção a ser indexada por `Codigo`. |
| `Data_Auditoria_Tributaria` | Data da auditoria dos vínculos tributários incorporados ao arquivo. | Registrar com a carga; não substitui a vigência de cada regra. |
| `Fontes_Auditoria_Tributaria` | Fontes usadas na auditoria dos vínculos. | Preservar para rastreabilidade da versão importada. |

### 5.2 Campos do registro em `Nomenclaturas`

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `Codigo` | Código NCM, inclusive registros de níveis hierárquicos. | Chave de busca do produto. Para a regra fiscal, prefira o NCM completo/cadastrado no item. |
| `Descricao` | Descrição oficial da classificação NCM. | Apoia a conferência da classificação do produto; não dispensa a análise da mercadoria real. Pode conter marcação HTML. |
| `Data_Inicio` | Data inicial de vigência da linha NCM. | Use para validar o NCM na data de emissão. |
| `Data_Fim` | Data final de vigência da linha NCM. | Use para encerrar a validade; `31/12/9999` representa vigência aberta no arquivo. |
| `Tipo_Ato` | Tipo do ato que instituiu/alterou a nomenclatura. | Metadado de rastreabilidade da NCM. |
| `Numero_Ato` | Número do ato normativo da NCM. | Usado com `Tipo_Ato` e `Ano_Ato` para auditoria. |
| `Ano_Ato` | Ano do ato normativo da NCM. | Complementa a identificação do ato. |
| `CST` | CST IBS/CBS atribuído diretamente ao NCM, quando existente. | Se a operação não for prioritária e o NCM também possuir `cClassTrib`, esta é a classificação direta do NCM. |
| `cClassTrib` | cClassTrib atribuído diretamente ao NCM, quando existente. | Deve ser usado sempre com o `CST` direto. Exemplo atual: determinados combustíveis têm classificação monofásica direta. |
| `Anexos` | Lista de anexos ordinários da LC 214 associados ao NCM. | Use somente se não houver regra direta nem exceção aplicável. Pode conter mais de um anexo; escolha pela finalidade legal. |
| `Anexos_excecao_prioritaria` | Lista de hipóteses legais específicas que prevalecem sobre anexos ordinários quando suas condições forem atendidas. | Não usar apenas porque existe no NCM: confirmar a hipótese material do artigo. Havendo aderência, usar o `CST`/`cClassTrib` da exceção. |

### 5.3 Objeto de anexo ordinário — `Anexos[]`

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `Legislacao` | Identifica a legislação da regra. | Atualmente aponta `LC 214/2025`; valida a origem do vínculo. |
| `Anexo` | Número romano do Anexo da LC 214. | Chave de ligação com `tabela_anexos_lc214.json.Anexo`. É também a referência para verificar a finalidade descrita. |
| `CST` | CST aplicável àquele NCM quando enquadrado no anexo. | Use com o `cClassTrib` da mesma entrada do vetor. |
| `cClassTrib` | Classificação tributária aplicável ao anexo. | É o resultado da seleção do anexo compatível com a finalidade. |
| `Aplicabilidade` | Condições, exceções, fundamento, vigência e origem da regra. | Avaliar os campos internos antes de considerar o anexo aplicável ao item concreto. |

### 5.4 Objeto de exceção — `Anexos_excecao_prioritaria[]`

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `Legislacao` | Legislação da hipótese excepcional. | Confirma a origem legal, atualmente `LC 214/2025`. |
| `Artigo` | Artigo da LC 214 que descreve a situação específica. | Chave jurídica da exceção; deve ser confrontado com as condições reais da operação. |
| `CST` | CST da exceção. | Use somente se a hipótese do artigo for satisfeita. |
| `cClassTrib` | cClassTrib da exceção. | Substitui o resultado de anexos ordinários quando a exceção for aplicável. |
| `Tipo_Aplicacao` | Código da condição de negócio usada pela exceção. | Relacionar com `Tipos[].Codigo` no JSON de tipos de aplicação. |
| `Numero_Anexo` | Anexo relacionado à hipótese excepcional, quando informado. | Usar como apoio de rastreabilidade e conferência da regra. |
| `Aplicabilidade` | Fundamentação, condição, exceção, observação, vigência, fontes e status da regra. | Exibir a condição ao usuário e validar a data antes de aplicar a exceção. |

### 5.5 Por que a exceção existe

O NCM `9619.00.00` demonstra bem a diferença: ele aparece no Anexo VIII com `200035` (produtos de higiene e limpeza com redução de 60%), mas também possui exceção do art. 147 com `200013` para tampões, absorventes internos/externos, calcinhas absorventes e coletores menstruais, com redução de 100%. Portanto, não se deve usar automaticamente `200035` para todo produto daquela NCM.

Outro exemplo é o NCM `3004.20.59`, que pode apontar Anexo XIV, Anexo IX e exceção do art. 133. A classificação depende de a mercadoria atender, ou não, às condições do medicamento registrado/manipulado e da hipótese de alíquota zero. A presença de múltiplos vínculos é justamente o sinal de que a regra precisa da finalidade e das condições legais, além do NCM.

## 6. Tipos de aplicação da exceção prioritária — `tabela_ncm_tipo_aplicacao_anexo_excecao_prioritaria.json`

Este arquivo complementa `NCM.Anexos_excecao_prioritaria[].Tipo_Aplicacao`. Ele não escolhe sozinho o tratamento fiscal: descreve a condição que precisa ser comprovada.

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `Versao` | Versão do catálogo de tipos. | Registrar na importação e na auditoria da decisão. |
| `Data_Corte` | Data de corte das informações analisadas. | Comparar durante a atualização; não usar como vigência do documento fiscal. |
| `Tipos` | Lista das condições conhecidas. | Indexar por `Codigo`. |
| `Codigo` | Chave relacionada a `Tipo_Aplicacao`. | Localizar a condição correspondente à exceção do NCM. |
| `Nome` e `Descricao` | Identificação e explicação da condição. | Exibir em telas de revisão e logs de decisão. |
| `Pergunta_Orientadora` | Pergunta que traduz a condição legal para o negócio. | Responder automaticamente apenas quando o ERP possuir os fatos necessários; caso contrário, apresentar ao usuário. |
| `cClassTrib_Relacionados` | Classificações que podem usar o tipo. | Validar coerência; a presença do código na lista não comprova que a condição foi atendida. |
| `Em_Uso` | Indica se o tipo está ativo no catálogo. | Não aplicar automaticamente um tipo inativo. |
| `Observacao` | Limites ou condições adicionais. | Mostrar junto da pergunta quando ajudar a decisão. |

## 7. Tabela de anexos da LC 214 — `tabela_anexos_lc214.json`

Esta tabela é descritiva. Ela não traz NCM, CST ou `cClassTrib`; a ligação é feita por `NCM.Anexos[].Anexo` = `Anexo` desta tabela.

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `ID` | Identificador interno sequencial do anexo. | Pode ser usado como chave interna de armazenamento; a ligação fiscal principal é pelo campo `Anexo`. |
| `Ativo` | Situação do anexo no catálogo. | Considere `S` como ativo. Preserve a checagem para futuras versões que tragam anexos desativados. |
| `Legislacao` | Norma à qual o anexo pertence. | Atualmente `LC 214/2025`. |
| `Anexo` | Número romano do Anexo. | Chave para relacionar com `NCM.Anexos[].Anexo` e com a coluna `ANEXO` da tabela de cClassTrib quando esta trouxer o número do anexo. |
| `Descricao` | Finalidade e tratamento geral previstos no anexo. | É a referência para decidir se o uso concreto se enquadra no anexo: cesta básica, educação, saúde, dispositivos médicos, insumos agropecuários, medicamentos, entre outros. |

## 8. Tabela CST IBS/CBS — `tabela_cst_ibscbs.json`

Esta tabela tem 18 CSTs e é técnica: seus indicadores controlam grupos do XML/DF-e. Em regra, `1` indica que o grupo é aplicável/exigível conforme o leiaute e `0` que não é aplicável. A Nota Técnica e o schema do documento continuam sendo a referência final da obrigatoriedade.

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `CST` | Código de Situação Tributária do IBS/CBS. | Chave da tabela. Validar que corresponde aos três primeiros dígitos do `cClassTrib` selecionado. |
| `Descricao` | Significado do CST. | Exibir para conferência fiscal, como tributação integral, alíquota reduzida, isenção, monofásica, diferimento etc. |
| `ind_gIBSCBS` | Indicador do grupo regular de IBS/CBS. | Use para controlar o grupo `gIBSCBS` do leiaute. |
| `ind_gIBSCBSMono` | Indicador do grupo de tributação monofásica. | Use para controlar `gIBSCBSMono` quando o CST for monofásico. |
| `ind_gRed` | Indicador do grupo de redução de alíquota. | Use para controlar o grupo de redução correspondente. |
| `ind_gDif` | Indicador do grupo de diferimento. | Use para controlar o grupo de diferimento. |
| `ind_gTransfCred` | Indicador do grupo de transferência de crédito. | Use para controlar o grupo de transferência de crédito. |
| `ind_gCredPresIBSZFM` | Indicador do grupo de crédito presumido de IBS na Zona Franca de Manaus. | Use apenas nas hipóteses compatíveis com ZFM. |
| `ind_gAjusteCompet` | Indicador do grupo de ajuste de competência. | Use para controlar o grupo de ajuste de competência, quando previsto pelo leiaute. |
| `ind_RedutorBC` | Indicador do redutor de base de cálculo. | Use para controlar o grupo/campo de redução da base de cálculo. |
| `DataAtualizacao` | Data de atualização daquela linha CST. | Armazenar para auditoria e recarga de tabelas. |

## 9. Tabela CST e cClassTrib — `tabela_cst_classtrib_ibscbs.json`

É a tabela de validação final. Depois da seleção, procure a linha cujo `CST` e `cClassTrib` sejam exatamente os escolhidos. A linha informa fundamento, vigência, redução e capacidade técnica do DF-e.

### 9.1 Identificação, descrição e base legal

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `CST` | CST IBS/CBS da classificação. | Deve ser igual ao CST escolhido e ao prefixo de `cClassTrib`. |
| `Descricao_CST` | Descrição do CST da linha. | Conferência humana do CST. |
| `cClassTrib` | Código de Classificação Tributária do IBS/CBS. | Chave de resultado do processo e valor a informar no DF-e. |
| `Nome_cClassTrib` | Nome curto da classificação. | Exibição em telas, logs e cadastros. |
| `Descricao_cClassTrib` | Descrição completa da hipótese tributária. | Principal apoio para conferir o enquadramento material da operação. |
| `LC_214_25` | Referência resumida ao dispositivo da LC 214/2025. | Usar para rastreabilidade jurídica e consulta fiscal. |
| `LC_Redacao` | Redação legal armazenada na tabela. | Apoio documental; não dispense a consulta à versão vigente da lei. |
| `ANEXO` | Identificação de anexo ligado à classificação. | Pode ser o número romano do anexo ou um identificador técnico no formato `9XXXY`; não confundir o identificador técnico com o Anexo I a XV. |
| `Link` | URL para o dispositivo legal de referência. | Use em telas de consulta e auditoria. |
| `Regulamento_CBS` | Referência/redação do regulamento da CBS, quando existente. | Complementa a base legal da CBS. |
| `Regulamento_IBS` | Referência/redação do regulamento do IBS, quando existente. | Complementa a base legal do IBS. |

### 9.2 Alíquota, redução e vigência

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `TipoDeAliquota` | Tipo de alíquota aplicável à classificação. | Usar como informação de cálculo e conferência, juntamente com o leiaute e a legislação. |
| `pRedIBS` | Percentual de redução da alíquota do IBS. | Aplicar somente quando a classificação e o leiaute exigirem a redução; exemplo: `60` ou `100`. |
| `pRedCBS` | Percentual de redução da alíquota da CBS. | Mesma regra de `pRedIBS`, para CBS. |
| `dIniVig` | Início de vigência do `cClassTrib`. | A classificação só pode ser usada em documentos emitidos a partir desta data. |
| `dFimVig` | Fim de vigência do `cClassTrib`. | Vazio significa sem encerramento indicado; se preenchido, não usar depois da data. |
| `DataAtualizacao` | Data da última atualização daquela linha. | Controle de sincronização e auditoria; não substitui `dIniVig`/`dFimVig`. |
| `tpRBSN` | Tipo de receita bruta aplicável ao Simples Nacional. | Use somente nas regras específicas do Simples Nacional, conforme o leiaute e a parametrização do contribuinte. Os valores são códigos técnicos (`0`, `1`, `2`, `3`, `4`, `5`, `9`), não percentuais. |

### 9.3 Indicadores de grupos tributários do leiaute

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `ind_gTribRegular` | Indicador do grupo de tributação regular. | Controla a aplicação desse grupo no DF-e. |
| `ind_gCredPresOper` | Indicador do grupo de crédito presumido da operação. | Controla o grupo de crédito presumido; depende também de código válido de `cCredPres`, quando aplicável. |
| `ind_gMonoPadrao` | Indicador do grupo de tributação monofásica padrão. | Controla o preenchimento do grupo monofásico padrão. |
| `indMonoReten` | Indicador de monofásica com retenção. | Controla o subgrupo/campo de retenção monofásica. |
| `indMonoRet` | Indicador de monofásica com retenção já ocorrida. | Controla o subgrupo/campo de tributação monofásica retida. |
| `indMonoDif` | Indicador de monofásica com diferimento. | Controla a informação monofásica diferida nas classificações que ainda o possuam. |
| `ind_gEstornoCred` | Indicador do grupo de estorno de crédito. | Controla o grupo de estorno de créditos. |
| `ind_gpBioDiferenca` | Indicador do grupo de diferença de biocombustível. | Quando ativo, controla `gpBioDiferenca` no contexto de IBS ad rem/combustíveis. |

### 9.4 Indicadores de documentos fiscais

Cada campo abaixo indica se a classificação pode ser informada no respectivo modelo de documento. Use `1` como permitido/aplicável e `0` como não permitido/não aplicável, sempre em conjunto com a Nota Técnica do DF-e.

| Campo | Documento ou finalidade |
| --- | --- |
| `indNFeABI` | NF-e de Alienação de Bens Imóveis. |
| `indNFe` | Nota Fiscal Eletrônica — NF-e. |
| `indNFCe` | Nota Fiscal de Consumidor Eletrônica — NFC-e. |
| `indCTe` | Conhecimento de Transporte Eletrônico — CT-e. |
| `indCTeOS` | CT-e Outros Serviços. |
| `indBPe` | Bilhete de Passagem Eletrônico — BP-e. |
| `indBPeTA` | BP-e Transporte Aquaviário. |
| `indBPeTM` | BP-e Transporte Metropolitano. |
| `indNF3e` | Nota Fiscal de Energia Elétrica Eletrônica — NF3e. |
| `indNFSe` | Nota Fiscal de Serviço eletrônica — NFS-e. |
| `indNFSe_Via` | NFS-e de exploração de via. |
| `indNFCom` | Nota Fiscal de Comunicação eletrônica — NFCom. |
| `indNFAg` | Nota Fiscal da Água e Saneamento eletrônica — NFAg. |
| `indNFGas` | Nota Fiscal de Gás Canalizado eletrônica — NFGas. |
| `indDERE` | Declarações de Regimes Específicos. |
| `indDIR` | Declaração de Incentivos, Renúncias, Benefícios e Imunidades de Natureza Tributária — DIR. |
| `indDUIMP` | Declaração Única de Importação — DUIMP. |

## 10. Tabela de crédito presumido — `tabela_ccredpres.json`

Consulte esta tabela quando a operação indicar `CodCredPresumido` ou quando a classificação final tiver `ind_gCredPresOper = 1`. O código não deve ser deduzido apenas pelo CST ou pelo NCM.

| Campo | Para que serve | Como usar |
| --- | --- | --- |
| `cCredPres` | Código do crédito presumido. | Chave técnica a informar no grupo aplicável do DF-e. |
| `Descricao` | Descrição da hipótese. | Exibir para conferência e auditoria; confirmar a aderência à operação concreta. |
| `LC_214_2025` | Fundamento na LC 214/2025. | Rastreabilidade legal da escolha. |
| `ApropriaViaNF` e `ApropriaViaEvento` | Formas admitidas de apropriação. | Não gerar o grupo da nota quando a hipótese exigir apropriação exclusivamente por evento. |
| `ind_DeduzCredPres` | Indica se o crédito presumido é deduzido nos termos do leiaute. | Controlar o cálculo e o grupo correspondente conforme a Nota Técnica vigente. |
| `ind_gCBSCredPres` e `ind_gIBSCredPres` | Indicam os grupos de CBS e IBS aplicáveis. | Gerar somente os grupos marcados e permitidos pelo documento. |
| `AliquotaCBS` e `AliquotaIBS` | Tipo ou referência da alíquota aplicável. | Interpretar em conjunto com os percentuais e a legislação; não tratar como percentual sem conferir o conteúdo. |
| `pAliqCredPresCBS` e `pRedTransicaoIBS` | Percentuais previstos na tabela, quando preenchidos. | Aplicar somente no período e na hipótese correspondente. |
| `dIniVig`, `dFimVig`, `dIniVigCBS`, `dFimVigCBS`, `dIniVigIBS` e `dFimVigIBS` | Vigências geral e específicas. | Validar pela data do documento e pelo tributo; campo final vazio representa ausência de encerramento informado. |
| `cClassNotaReferenciada` | Classificação exigida para nota referenciada, quando aplicável. | Validar o documento de origem antes de aceitar o crédito. |

## 11. Automação segura e intervenção do usuário

Automatize quando os fatos cadastrados levarem a uma única resposta vigente. Exemplos: operação prioritária explicitamente configurada, NCM com classificação direta ou uma única regra de anexo cuja condição esteja comprovada por dados objetivos.

Quando duas ou mais hipóteses continuarem possíveis, apresente ao usuário as opções com descrição, fundamento, condição e vigência. A escolha deve ser feita sobre a **condição de negócio** — por exemplo, finalidade do produto, natureza da doação ou atendimento de requisito legal — e não por uma lista isolada de códigos tributários.

Depois da decisão, grave no item do documento a fotografia tributária usada na emissão:

- `CST`, `cClassTrib` e `cCredPres`, quando houver;
- origem da decisão e regra aplicada;
- operação, NCM, anexo, artigo ou tipo de aplicação determinante;
- percentuais, bases e demais valores calculados;
- versão, datas de referência ou hash dos catálogos consultados;
- resposta manual e usuário responsável, quando necessária.

Uma alteração futura no cadastro ou nos JSONs não deve modificar silenciosamente um documento já calculado ou emitido.

## 12. Sincronização segura dos JSONs

As tabelas mudam ao longo do tempo. Atualize-as como um conjunto de referências versionadas, sem apagar os dados válidos antes de confirmar o novo conteúdo:

1. Baixe cada arquivo para um nome temporário.
2. Confirme resposta HTTP bem-sucedida, conteúdo não vazio, codificação e JSON válido; rejeite HTML ou mensagens de erro salvas como JSON.
3. Desserialize tolerando propriedades novas e campos opcionais, mas rejeite mudança estrutural incompatível.
4. Valide chaves, tipos mínimos, duplicidades, vigências e uma quantidade plausível de registros.
5. Importe em área temporária ou transação única e confira o resultado.
6. Substitua os dados anteriores somente depois da validação completa; em qualquer falha, faça rollback e preserve a versão anterior.
7. Registre URL de origem, data/hora do download, versão ou data declarada, hash, quantidade e resultado da importação.

A reimportação do mesmo arquivo deve ser idempotente. Propriedades desconhecidas podem ser ignoradas, mas registros inválidos não devem ser parcialmente gravados.

## 13. Validações obrigatórias antes de gerar o XML

- A operação selecionada deve existir na Tabela de Operações.
- O NCM deve ser válido na data do documento, quando a operação não for prioritária.
- Uma exceção ou anexo só pode ser aplicado quando suas condições e sua vigência forem atendidas; múltiplos candidatos não resolvidos devem bloquear a emissão e solicitar decisão.
- O par `CST` + `cClassTrib` deve existir na tabela de classificação; o CST deve corresponder ao prefixo do `cClassTrib`.
- A classificação deve estar vigente entre `dIniVig` e `dFimVig`.
- `indNFe` deve permitir o uso na NF-e e `indNFCe` na NFC-e; não reutilize automaticamente a permissão de um modelo no outro.
- Os grupos tributários devem obedecer aos indicadores da tabela CST e da tabela `cClassTrib`.
- Se a operação trouxer `CodCredPresumido`, valide-o na tabela `cCredPres` vigente e preencha seus grupos somente quando permitidos.
- Registre em auditoria a origem da decisão, a regra usada e a versão das tabelas; não recalcule documentos emitidos com cadastros atualizados posteriormente.

## 14. Exemplo resumido

**Venda normal** (`Codigo` da operação `00100`, `Prioridade = N`) de item NCM `9619.00.00`:

1. A operação não é prioritária; consultar NCM.
2. O NCM possui Anexo VIII (`200035`) e exceção do art. 147 (`200013`).
3. Se o item é um dos produtos do art. 147 — tampão, absorvente interno/externo, calcinha absorvente ou coletor menstrual — usar `CST 200` + `cClassTrib 200013`.
4. Se não atende à hipótese específica do art. 147, avaliar o Anexo VIII e a sua finalidade, usando `CST 200` + `cClassTrib 200035` quando aplicável.
5. Validar `200013` ou `200035` na tabela de classificação, a vigência e a permissão para o DF-e emitido.

## 15. Referências oficiais e fontes das tabelas

- [Lei Complementar nº 214/2025 — Planalto](https://www.planalto.gov.br/ccivil_03/leis/lcp/lcp214.htm)
- [Projeto Reforma Tributária do Consumo / Portal Nacional da NF-e](https://www.nfe.fazenda.gov.br/portal/principal.aspx)
- [Tabela de NCM Unimake](https://www.unimake.com.br/downloads/tabela_ncm.json)
- [Tipos de aplicação das exceções prioritárias de NCM Unimake](https://www.unimake.com.br/downloads/tabela_ncm_tipo_aplicacao_anexo_excecao_prioritaria.json)
- [Tabela CST IBS/CBS Unimake](https://www.unimake.com.br/downloads/tabela_cst_ibscbs.json)
- [Tabela CST e cClassTrib Unimake](https://www.unimake.com.br/downloads/tabela_cst_classtrib_ibscbs.json)
- [Tabela de Crédito Presumido IBS/CBS Unimake](https://www.unimake.com.br/downloads/tabela_ccredpres.json)
- [Tabela de Operações Unimake](https://www.unimake.com.br/downloads/Tabela_Operacao.json)
- [Tabela CFOP Unimake](https://www.unimake.com.br/downloads/tabela_cfop.json)
- [Tabela de Anexos LC 214 Unimake](https://www.unimake.com.br/downloads/tabela_anexos_lc214.json)

> Este manual descreve a lógica de integração das tabelas e a sua aplicação técnica. A definição fiscal da finalidade, a classificação correta do produto e a confirmação dos requisitos legais de cada benefício devem permanecer sujeitas à validação tributária da empresa.
