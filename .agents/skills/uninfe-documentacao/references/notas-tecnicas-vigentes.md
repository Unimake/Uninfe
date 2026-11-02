# Notas técnicas vigentes e prazos

Referência exclusiva do `MODO_ATUALIZAR_NOTAS_TECNICAS_VIGENTES`. As regras dos demais documentos e modos permanecem inalteradas.

## Acionamento e destino

- Comando: **`$uninfe-documentacao atualizar notas técnicas vigentes`**. Aceitar também pedidos equivalentes para atualizar a documentação com o conteúdo da planilha.
- Fonte padrão: [Notas técnicas e prazos](https://docs.google.com/spreadsheets/d/1vW6Gu_joO2mvua4rGjnBQ5LccVUVljvEpUtYin30BFs/edit?gid=0#gid=0), aba `Unimake - NTs e Prazos` (gid 0).
- Se o usuário fornecer uma nova planilha ou exportação como fonte desta atualização, ler essa fonte atual e registrar sua origem no catálogo. Não mesclar com dados antigos por padrão.
- Destino estável: `docs/referencias/notas-tecnicas-vigentes.md`, H1 **Notas técnicas vigentes e prazos**, categoria **Referências**.
- A planilha é fonte autorizada para este acompanhamento editorial. Não inferir suporte por versão do UniNFe, implementação concluída ou vigência jurídica a partir de SIM ou de prazo vencido.
- Ler a planilha sem alterá-la. Esta rotina não publica o site nem cria atualização agendada.

## Reconstrução integral em toda execução

1. Consultar metadados e cabeçalhos atuais com o conector Google Drive/Sheets. Usar nomes de abas e intervalos delimitados confirmados; não fixar o número de linhas da primeira edição. O cabeçalho estava na linha 3, nas colunas A–K, mas pode mudar.
2. Ler todas as linhas relevantes da fonte atual, em blocos se necessário. Localizar as colunas pelo cabeçalho: descrição, documento fiscal, UF, observações, links, NT Vigente?, data da análise da Unimake, prazos e qualificadores de homologação/produção. As colunas auxiliares antes das datas contêm EM/ATÉ e fazem parte do prazo.
3. Filtrar exclusivamente `trim(valor).toUpperCase() === 'SIM'` em NT Vigente?. Não usar data, tachado, cor ou versão mais alta como substitutos desse filtro.
4. Construir do zero o conjunto de fichas, a consulta rápida, os cronogramas e os links com os registros selecionados. A página anterior serve apenas como referência de formato e de identificadores estáveis, nunca como fonte de dados atuais.
5. Remover da página as notas que deixaram de ser SIM, inclusive etapas e links antigos de fichas que ainda tenham outras linhas SIM. NÃO apenas acrescentar novidades. Se não houver SIM após leitura completa confirmada, publicar uma indicação curta de que não há registros selecionados na fonte na data da consulta.
6. Se a leitura estiver incompleta ou a fonte indisponível, preservar a página existente e informar o bloqueio. Não interpretar falha de acesso como ausência de registros nem gerar com dados antigos como se fossem atuais.

## Agrupamento, cronogramas e conflitos

- Reunir linhas apenas quando documento, identificação da NT/IT, versão e abrangência forem compatíveis. Mesma numeração em documentos fiscais distintos não é a mesma publicação. Não fundir uma versão desconhecida com outra explícita sem confirmação.
- Duplicatas equivalentes viram uma ficha; aproveitar links preenchidos e reunir informações complementares sem perder condições. Várias etapas da mesma nota viram linhas da tabela de prazos. Usar o nome da etapa somente quando a relação estiver explícita na fonte.
- Preservar versões diferentes marcadas SIM, mesmo havendo uma versão mais nova. Não decidir por conta própria que a antiga deixou de vigorar.
- Normalizar datas para DD/MM/AAAA e preservar os qualificadores **Em** e **Até**. Manter **Imediato** e **Sem prazo informado** sem convertê-los em datas ou estados de implantação.
- Incluir etapas adicionais descritas explicitamente nas observações, mesmo que não exista uma linha própria na planilha. Não inventar data de homologação a partir da data de produção.
- Comparar cronogramas das colunas entre si e com as observações. Não escolher silenciosamente uma data ou um qualificador. Tentar confirmar divergências na publicação oficial vinculada; registrar no catálogo o que foi confirmado e a fonte.
- Se não for possível resolver um conflito, manter a ficha e os campos não conflitantes. No campo conflitante escrever **Consultar o cronograma oficial**, com o documento oficial acessível na ficha. Registrar valores conflitantes e limitações apenas no catálogo; não publicar rascunhos ou pendências internas. Reavaliar cada conflito com a fonte atual, sem perpetuar omissões já resolvidas.

## Formato fixo da página

- Introdução com link da fonte, data real de atualização e contagem de registros SIM e fichas após consolidação. Explicar que é uma fotografia do acompanhamento da planilha e que não confirma implantação nem atendimento por versão do UniNFe.
- Tabela **Consulta rápida**: documento fiscal, nota/versão com link para sua ficha, abrangência e produção (várias etapas quando necessário). Agrupar as linhas por documento e manter ordem consistente entre resumo e fichas.
- Grupos por documento fiscal com fichas expansíveis fechadas por padrão. Usar HTML somente para `<details>`, `<summary>` e o identificador; manter o conteúdo em Markdown.
- Cada ficha: título com identificação/versão e assunto, documento fiscal e UF da fonte, publicação quando informada, data de análise da Unimake, tabela **Prazos** (etapa, homologação e produção), **Principais alterações e orientações** quando houver conteúdo e **Documentos e links**.
- Modelo:

```markdown
<details id="nt-nfcom-2026-004-v1-00">
<summary><strong>NT 2026.004 v1.00 — Adequações da Reforma Tributária</strong></summary>

**Documento fiscal na planilha:** NFCom.  
**Abrangência:** Todas as UFs.  
**Publicação:** 01/10/2026.  
**Análise pela Unimake:** 08/10/2026.

### Prazos

| Etapa | Homologação | Produção |
|---|---|---|
| Regras gerais | Em 13/10/2026 | Em 16/11/2026 |

### Principais alterações e orientações

- Resumo fiel às observações da fonte atual.

### Documentos e links

- [Nota técnica](URL-informada-na-fonte)

</details>
```

- IDs começam com `nt-` ou `it-`, incluem documento, número e versão, usam minúsculas sem acentos, números e hífens. Acrescentar abrangência quando necessário para distinguir fichas. Nunca usar o número da linha como ID. Preservar o ID das fichas sobreviventes mesmo ao revisar o título.
- Preservar linha em branco depois de `</summary>` e antes de `</details>`. Links da consulta rápida usam `#id`; links públicos usam `viewer/?doc=referencias/notas-tecnicas-vigentes.md#id`.

## Tratamento das observações

- Reescrever em português claro, com parágrafos curtos, listas, subtítulos e destaques. Converter negritos de WhatsApp e preservar código inline, XML e identificadores técnicos.
- Preservar condições, exceções, ambientes, versões, campos, rejeições e distinções entre disponibilização e obrigatoriedade. Não criar explicações funcionais quando Observações estiver vazia nem herdá-las de outra versão.
- Retirar humor, chamadas promocionais, redundância e marcadores residuais de citação. Trocar “hoje” por data absoluta somente quando a fonte informar a data.
- Não perpetuar anúncios antigos como “BETA será disponibilizada” como notícia atual. Não afirmar disponibilidade de produto sem confirmação.
- Organizar URLs das colunas e das observações em links nomeados; distinguir NT/IT, anexos oficiais e comentário complementar. Texto como “veja observações” não é URL.
- Preservar a abrangência das colunas. Se houver aparente incompatibilidade com o resumo, registrar no catálogo e confirmar antes de corrigir por inferência.

## Integração e conferência

- Manter links em `docs/referencias/README.md`, `docs/index.md` e na página estável `docs/referencias/notas-tecnicas-e-noticias-para-desenvolvedores.md`, sem duplicá-los a cada execução.
- Atualizar `docs/_catalogo-documentacao.md` com data, origem, intervalo lido, contagem selecionada, mapeamento das consolidações e conflitos atuais. Separar registros históricos da seleção corrente.
- Executar `node viewer/build-docs-index.js` a partir de `docs` em toda atualização; incluir manifesto e busca regenerados. Não é necessário alterar o viewer para este formato.
- Conferir cobertura de todas as linhas SIM, ausência de registros excluídos, IDs únicos, âncoras válidas, cronogramas e links. Verificar que uma alteração SIM → NÃO remove também resumo/etapas/links correspondentes.
- Quando a prévia local estiver disponível, conferir renderização, abertura da ficha por link e busca. Informar limitações reais de verificação sem alegar sucesso visual não observado.
- Na entrega, indicar contagem de registros/fichas, documento criado ou reconstruído, conflitos que afetaram os prazos e o comando de atualização. Não alterar outros assuntos da documentação nessa rotina.
