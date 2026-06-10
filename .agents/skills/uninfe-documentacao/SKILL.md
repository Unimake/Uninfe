---
name: uninfe-documentacao
description: Gerar, revisar, organizar e manter documentacao Markdown do UniNFe de forma incremental. Use quando Codex precisar documentar o UniNFe, documentar um servico, tela/formulario, configuracao, integracao por troca de arquivos, modelos XML, retornos, erros, pastas, atualizar indice/catalogo de documentacao, revisar documentacao Markdown existente, ou criar/organizar paginas .md do UniNFe.
---

# UniNFe Documentacao

## Principio central

Documente o UniNFe de forma incremental, rastreavel e baseada em evidencia do repositorio. Nao gere a documentacao completa de uma vez. Cada tarefa deve cobrir um modulo, servico, tela, configuracao ou assunto especifico.

Toda pagina publica em `/docs` deve sair como documentacao final para publicacao, pronta para usuario, suporte ou desenvolvedor integrador usar. Nao inclua na pagina publica secoes de controle do processo, como `Status`, `Evidencias analisadas`, `Pendencias`, notas do Codex, comentarios sobre estar "em elaboracao", ou listas de arquivos consultados.

Use portugues do Brasil, Markdown simples e links relativos. Quando algo nao puder ser confirmado em codigo, formularios, configuracoes, exemplos, schemas ou documentacao existente, nao publique como comportamento. Se a informacao for essencial e ainda incerta, registre a pendencia somente no catalogo interno (`docs/_catalogo-documentacao.md`) ou em inventario interno, nao na pagina final.

Escreva para o leitor final. Para telas, explique o caminho de menu, o nome visivel do botao/campo e o efeito pratico. Nao cite nomes de eventos, metodos, classes, controles, arquivos `.cs` ou detalhes internos como `cmConsultaCadastro_Click`, salvo em documentacao explicitamente voltada a manutencao tecnica do codigo.

## Quando usar

Use esta skill para documentacao do UniNFe, incluindo servicos fiscais, telas WinForms, configuracoes, integracao por troca de arquivos, XML/TXT, retornos, erros, pastas, logs, indice, catalogo e revisao de paginas Markdown.

## Quando nao usar

Nao use esta skill para alterar codigo de producao, criar testes, implementar servicos, revisar ticket de codigo, modernizar projetos, ou gerar documentacao de outro produto.

## Modos

Escolha um modo antes de editar:

- `MODO_MAPEAR_PROJETO`: mapear menus, formularios, configuracoes, servicos, modelos XML, pastas, classes e fluxos relevantes. Gerar ou atualizar inventario documental.
- `MODO_CRIAR_ESTRUTURA_DOCS`: criar `/docs`, `docs/index.md` e `docs/_catalogo-documentacao.md` se nao existirem.
- `MODO_DOCUMENTAR_TELA`: documentar uma tela/formulario especifico.
- `MODO_DOCUMENTAR_CONFIGURACAO`: documentar uma configuracao especifica.
- `MODO_DOCUMENTAR_INTEGRACAO_ARQUIVOS`: documentar integracao por troca de arquivos.
- `MODO_DOCUMENTAR_SERVICO`: documentar um servico especifico, como NFe, NFCe, CTe, MDFe, NFCom, NF3e, BPe, CIOT, GNRE ou outro encontrado no codigo.
- `MODO_ATUALIZAR_INDICE`: ler os `.md` existentes em `/docs` e atualizar indice/catalogo, sem criar conteudo funcional novo.
- `MODO_REVISAR_DOCUMENTACAO`: verificar links quebrados, duplicidades, documentos orfaos, inconsistencias, lacunas e trechos sem evidencia; corrigir apenas problemas seguros.

## Fluxo incremental

1. Identificar o modo e o escopo exato pedido.
2. Verificar se `/docs`, `docs/index.md` e `docs/_catalogo-documentacao.md` existem.
3. Localizar evidencias no repositorio antes de escrever explicacoes funcionais.
4. Ler arquivos vizinhos e exemplos equivalentes antes de definir nomes, fluxos e estrutura.
5. Criar ou editar somente os `.md` necessarios ao escopo.
6. Registrar evidencias analisadas no catalogo interno, nunca como secao da pagina publica.
7. Atualizar `docs/index.md` e `docs/_catalogo-documentacao.md`.
8. Revisar links relativos e remover da pagina publica qualquer observacao de processo, pendencia interna ou referencia desnecessaria a codigo.

## Evidencias aceitas

Use como fonte: codigo C#, formularios `.cs` e `.Designer.cs`, arquivos de configuracao, schemas, modelos XML/TXT, `exemplos xml`, scripts, README, documentos existentes e nomes reais de pastas/arquivos.

As evidencias orientam a geracao, mas nao devem aparecer na pagina publica como lista de arquivos, colunas `Evidencia`, nome de metodo/evento ou justificativa tecnica. Registre caminhos e lacunas no catalogo interno quando for util para manutencao da documentacao.

Se uma afirmacao nao tiver fonte, remova. Se a lacuna impedir documentar uma funcionalidade importante, crie uma pendencia no catalogo interno e deixe a pagina publica apenas com o comportamento confirmado.

## Estrutura sugerida

Mantenha a documentacao em `/docs`:

- `index.md`
- `_catalogo-documentacao.md`
- `introducao/`
- `instalacao/`
- `configuracao/`
- `integracao/`
- `servicos/<servico>/`
- `referencias/`

Prefira documentos pequenos e linkados. Nao mova documentacao existente sem necessidade.

## Regras por tipo

- Telas: analisar classe do formulario, designer, eventos, validacoes, propriedades persistidas e chamadas de servico. Na pagina publica, explicar finalidade, caminho de acesso, campos, botoes visiveis, mensagens, efeitos praticos e relacao com arquivos/configuracoes, sem expor nomes internos de codigo.
- Configuracoes: analisar `NFe.Settings`, `ConfiguracaoApp`, `Empresas`, `Empresa`, telas relacionadas e arquivos de configuracao. Na pagina publica, explicar onde configurar, como preencher, impacto operacional e cuidados, usando nomes que aparecem na tela ou nos arquivos de integracao.
- Integracao por arquivos: analisar `Processar`, `Propriedade`, enums, tasks, validadores, geradores de retorno, exemplos XML/TXT e pastas configuradas. Preservar sufixos e nomes reais.
- Servicos: analisar enum, tipo de envio, extensoes, `Task*`, validacao, schemas, exemplos, retornos e erros. Na pagina publica, explicar visao geral, configuracao, envio, retorno, erros comuns, arquivos envolvidos e exemplos existentes sem transformar a arquitetura interna em instrucao de uso.
- Indice: refletir somente os arquivos reais em `/docs`, organizar por publico/assunto e corrigir links relativos.
- Revisao: apontar lacunas, duplicidades, links quebrados, documentos orfaos e trechos sem evidencia; corrigir apenas quando a evidencia for clara.

## Padrao para consumo de servico por arquivos

Use este padrao quando o usuario pedir documentacao de um servico processado pelo UniNFe a partir de XML/TXT gravado em pasta monitorada. Este complemento nao altera o padrao de telas, configuracoes ou paginas gerais.

Antes de escrever, confirme no codigo e em exemplos equivalentes:

- Sufixo exato do arquivo de entrada, extensao de retorno, extensao de erro e extensao do XML processado/distribuicao, preservando maiusculas e minusculas.
- Pasta onde o ERP deve gravar o arquivo, pasta onde o UniNFe grava o retorno, e pastas internas que afetam a operacao do usuario, como `EmProcessamento`, `Autorizados`, `Originais` e pasta de erros.
- Elemento XML raiz esperado, tipo de documento fiscal, envio sincrono ou assincrono, certificado, ambiente, proxy e configuracoes da empresa usadas no processamento.
- Possiveis retornos para o ERP: retorno normal do webservice, arquivo de erro local, XML de distribuicao/processado, XML original assinado e movimentacao em caso de rejeicao.

Na pagina publica, inclua quando aplicavel:

- Finalidade do servico e quando usar.
- Pre-requisitos de configuracao no UniNFe.
- Contrato de nomes de arquivos, com exemplos reais no formato `<identificador>-sufixo.xml`.
- Fluxo operacional em passos curtos, escrito para o ERP/integrador.
- Um diagrama em Markdown/Mermaid com bloco ```` ```mermaid ```` e `flowchart TD` para mostrar o caminho do arquivo da pasta de envio ate retorno/autorizacao/erro.
- Tabela de arquivos gerados e movimentados, informando pasta, nome esperado e quando cada arquivo aparece.
- Orientacao de tratamento de sucesso, rejeicao e erro local, sem transformar nomes de classes, metodos ou eventos internos em instrucao de uso.

Quando o servico for sincrono, nao copie a explicacao de lote/recibo de servicos assincronos. Deixe claro que o ERP deve aguardar o retorno direto do webservice na pasta de retorno e o XML de distribuicao na pasta de autorizados quando houver autorizacao.

## Separacao entre pagina publica e controle interno

Pagina publica:

- Deve ser final, limpa e publicavel.
- Deve usar nomes visiveis ao usuario, nomes de arquivos/pastas do contrato, menus, botoes e campos.
- Pode conter secoes como `O que e`, `Quando usar`, `Como configurar`, `Como operar`, `Campos`, `Botoes`, `Arquivos envolvidos`, `Retornos`, `Erros comuns`, `Cuidados`.
- Nao deve conter `Status`, `Evidencias analisadas`, `Pendencias`, `PENDENTE DE VALIDAÇÃO`, nomes de eventos, nomes de metodos, nomes de classes ou comentarios sobre como a documentacao foi gerada.

Controle interno:

- Use `docs/_catalogo-documentacao.md` para registrar status editorial, evidencias consultadas, lacunas e proximas melhorias.
- Use inventarios internos somente quando o usuario pedir mapeamento ou quando isso ajudar a organizar uma entrega grande.
- O catalogo pode mencionar arquivos fonte e pendencias, pois ele nao e a pagina de uso final.

## Recursos

Leia referencias conforme o modo:

- `references/templates.md`: modelos de paginas para servico, tela, configuracao e integracao por arquivos.
- `references/checklist-revisao.md`: checklist para revisar consistencia, links, evidencias e catalogo.
- `references/padrao-editorial.md`: padrao de escrita Markdown para a documentacao.

## Criterios de aceite

- O escopo foi documentado sem inventar comportamento.
- Cada explicacao funcional tem evidencia verificada, com registros internos no catalogo quando necessario.
- A pagina publica esta em portugues do Brasil, clara, navegavel e pronta para publicacao.
- A pagina publica nao contem secoes de status, evidencias analisadas, pendencias ou referencias internas de codigo desnecessarias ao leitor.
- Links relativos foram conferidos.
- `docs/index.md` e `docs/_catalogo-documentacao.md` foram atualizados quando aplicavel.
- A documentacao atende usuario configurador, suporte e desenvolvedor integrador quando o assunto exigir.
