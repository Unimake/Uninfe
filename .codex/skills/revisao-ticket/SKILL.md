---
name: revisao-ticket
description: "Review UniNFe commits linked to a ticket using the exact commit-message pattern ID #000000. Use when the user asks Codex to review, audit, or justify approval/rejection of all commits for a ticket, consolidating the final state and producing Textile only for remaining technical problems."
---

# Revisao tecnica de ticket por commits

## Objetivo

Revisar os commits relacionados a um ticket informado pelo usuario e avaliar o resultado final do conjunto de alteracoes.

Localizar o ticket nas mensagens de commit pelo padrao exato:

```text
ID #123456
```

Use o restante do repositorio apenas como contexto para entender arquitetura, padroes, compatibilidade e impacto. Nao critique codigo antigo nao alterado pelos commits do ticket, exceto quando ele for indispensavel para explicar o impacto direto da alteracao revisada.

## Entrada

O usuario deve informar o numero do ticket, por exemplo:

```text
123456
```

Se o numero do ticket nao for informado, peca somente essa informacao.

## Fluxo obrigatorio

1. Localizar todos os commits cuja mensagem contenha exatamente `ID #NUMERO`.
2. Considerar todos os branches disponiveis quando possivel.
3. Analisar os commits em ordem cronologica.
4. Levantar arquivos, metodos, classes, formularios, XMLs, TXT, schemas, setup e exemplos alterados pelo ticket.
5. Comparar achados intermediarios com o estado final apos todos os commits do ticket.
6. Reportar somente problemas que ainda permanecem no resultado final.

Comandos de referencia:

```powershell
git log --all --grep="ID #123456" --format="%H %an %ad %s" --date=iso --reverse
git log --all --grep="ID #123456" --name-only --format="" | Sort-Object -Unique
git show --stat --patch HASH_DO_COMMIT
git show HEAD:CAMINHO_DO_ARQUIVO
```

Quando houver sequencia linear clara, use diff acumulado:

```powershell
git diff COMMIT_BASE..COMMIT_FINAL -- CAMINHO_DO_ARQUIVO
```

Se o branch atual nao contiver o estado final do ticket, use o ultimo commit do ticket como referencia para os arquivos alterados.

## Regra de consolidacao

A revisao e do ticket, nao de cada commit isolado.

Nao reportar:

- erro introduzido em commit intermediario e corrigido depois;
- codigo removido posteriormente no mesmo ticket;
- implementacao parcial completada por commit posterior;
- inconsistencia temporaria entre commits do mesmo ticket;
- problema que nao existe no estado final do codigo.

Antes de reprovar, confirme se algum commit posterior do mesmo ticket alterou o mesmo arquivo, classe, metodo, formulario, XML, schema, exemplo, setup ou comportamento relacionado.

## Escopo da revisao

Analise somente alteracoes feitas pelo ticket, incluindo efeitos diretos sobre:

- codigo C# dos projetos em `source`;
- fluxo de processamento em `NFe.Service`;
- enums, extensoes de arquivo, retornos e constantes em `NFe.Components`;
- configuracoes em `NFe.Settings`;
- validacao, assinatura, certificados, schemas e XML fiscal;
- conversao TXT/XML em `NFe.ConvertTxt`;
- monitoramento de pastas, filas, locks e threads em `NFe.Threadings`;
- UI WinForms/MetroFramework em `NFe.UI` e `uninfe`;
- servico Windows em `UniNFe.Service`;
- exemplos XML/TXT em `exemplos xml`;
- instaladores/scripts em `setup`, quando alterados.

Nao implemente nem corrija durante a revisao, a menos que o usuario peca explicitamente.

## Padroes criticos do UniNFe

- A solucao principal fica em `source/uninfe.sln`.
- Os projetos usam `.csproj` classico, `packages.config` e .NET Framework 4.8.1.
- Nao aceitar mudancas que exijam SDK-style project, `PackageReference`, .NET moderno, C# moderno ou APIs indisponiveis no .NET Framework 4.8.1 sem justificativa explicita.
- Compatibilidade com integracoes existentes e mais importante que modernizacao estetica.
- O processamento central passa por `NFe.Service.Processar.ProcessaArquivo`.
- Novos servicos geralmente precisam atualizar `Servicos`, `TipoEnvio`, `Propriedade.Extensao(...)`, deteccao em `DefinirTipoServico`, validacao em `ValidarExtensao`, roteamento em `ProcessaArquivo`, erro ERP em `GravaErroERP` e exemplos correspondentes.
- Novas operacoes de envio/consulta devem seguir classes equivalentes `Task... : TaskAbst`, com `Execute()` e `Servico` configurado corretamente.
- Sufixos de arquivos (`-ped-...xml`, `-ret-...xml`, `.err`, `-proc...xml`) sao contrato externo com ERPs. Reprove mudancas que quebrem nomes, casing, retorno esperado ou pasta de destino sem compatibilidade.
- Sempre respeite `Empresas.Configuracoes[emp]` para ambiente, UF, certificado, proxy, CSC, responsavel tecnico, pastas, emissao e configuracoes especificas.
- Nao aceitar remocao ou enfraquecimento de validacoes de `tpAmb`, `tpEmis`, chave do DFe, CNPJ/CPF, schema ou assinatura.
- Configuracoes devem passar por `ConfiguracaoApp`, `Empresas`, `Empresa` e XMLs de configuracao existentes. Nao aceitar formato paralelo sem necessidade.
- Fluxos de pasta sao parte do contrato: `PastaXmlEnvio`, `PastaXmlRetorno`, `PastaXmlErro`, `PastaXmlEnviado`, `PastaValidar`, `temp`, `EmProcessamento`, `Autorizados`, `Denegados`, `Originais`.
- Reprove alteracoes que possam perder retorno ao ERP, mover arquivo para pasta errada, quebrar locks/fila ou processar duas vezes o mesmo XML.
- Logs devem usar `Auxiliar.WriteLog(...)`, `Functions.WriteLog(...)` ou `Program.WriteLog(...)` conforme o padrao do arquivo.
- Em `catch`, preservar gravacao de erro para ERP quando o fluxo espera arquivo de resposta; nao substituir por apenas log interno.
- Nao alterar `MetroFramework` salvo quando o ticket tratar explicitamente de infraestrutura visual.
- Exemplos XML/TXT devem refletir extensoes e schemas reais; nao aceitar exemplos que ensinem sufixo ou estrutura divergente do processamento.

## Validacao esperada

Nao ha suite de testes automatizados evidente no repositorio. Ao revisar, valorize build e exemplos XML/TXT realistas.

Para alteracoes relevantes, cobre pelo menos build do projeto/solucao afetada quando viavel:

```powershell
dotnet build source/uninfe.sln --no-restore
```

Para servicos novos ou alterados, cobre exemplo XML/TXT de entrada e retorno esperado quando o ticket introduz novo contrato de arquivo.

## O que reprovar

Reporte somente problema concreto e remanescente, como:

- bug ou comportamento incorreto;
- quebra de contrato com ERP por extensao, nome de retorno, pasta ou XML/TXT;
- servico novo incompleto em enum, extensao, deteccao, validacao, roteamento ou erro ERP;
- uso de recurso incompativel com .NET Framework 4.8.1 ou estilo de projeto classico;
- XML gerado fora do schema/manual fiscal ou sem validacao/assinatura necessaria;
- uso incorreto de certificado, ambiente, UF, emissao, CSC, responsavel tecnico ou proxy;
- perda de retorno, arquivo movido para local errado, concorrencia insegura, lock/fila quebrados;
- excecao engolida sem log/retorno quando o ERP espera resposta;
- UI fora do padrao WinForms/MetroFramework ou que quebra tray/servico/threads;
- setup inconsistente com os binarios/dependencias;
- exemplo XML/TXT incorreto para o novo comportamento;
- duplicacao tecnica desnecessaria frente a helper/padrao ja existente;
- organizacao em diretorio/namespace errado;
- teste, exemplo ou validacao ausente para comportamento novo relevante.

Nao reprovar por preferencia pessoal, modernizacao opcional, estilo antigo que o proprio projeto usa amplamente ou problema que nao foi introduzido pelo ticket.

## Como escrever achados

Cada achado deve explicar:

- o que foi encontrado;
- por que e problema neste projeto;
- impacto pratico;
- como corrigir;
- confianca da analise.

Se depender de contexto externo ou regra fiscal nao confirmada no codigo, marque como risco potencial e use confianca media ou baixa.

Nao invente problemas. Nao use comentarios genericos como "melhorar codigo", "refatorar" ou "avaliar impacto" sem apontar defeito concreto.

## Formato da resposta

Nao escreva preambulo, resumo de investigacao, lista de commits analisados, conclusao extra ou arquivos sem problema.

### Sem problemas

Se nenhum problema concreto permanecer apos a analise consolidada, responda somente:

```text
Nao encontramos falhas.
```

### Com problemas

Se houver problemas remanescentes, comece exatamente com:

```text
h2. Justificativa da reprova na revisao do ticket:
```

Para cada problema, use Textile simples:

```text
h3. Problema N - titulo curto

Explique em poucas linhas o problema identificado.

h4. Trecho de codigo relevante

<pre>
mostre somente o trecho final necessario para justificar a analise
</pre>

h4. Por que isso e um problema

Explique objetivamente por que pode falhar, gerar comportamento incorreto, dificultar manutencao ou quebrar padrao do projeto.

h4. Impacto pratico

Explique em linguagem simples o impacto para producao, manutencao, compatibilidade, XML fiscal, integracao, processamento de arquivos, setup ou validacao.

h4. Sugestao de correcao

Explique o que mudar, como mudar e por que a alternativa respeita melhor o padrao do projeto.

h4. Confianca da analise

Alto: problema claro e evidente.
Medio: forte indicio, mas depende de contexto.
Baixo: risco potencial.

h4. Observacao didatica

Inclua uma orientacao curta para evitar esse tipo de problema no futuro.
```

## Restricoes finais

- Entregue somente o relatorio final.
- Seja direto, didatico e respeitoso.
- Nao repreenda, nao ironize e nao escreva teoria longa.
- Nao repita trechos grandes de codigo.
- Mostre o trecho de codigo no estado final, nao em commit intermediario.
- Nao liste commits aprovados.
- Nao inclua problemas corrigidos no proprio ticket.
