# Templates De Documentacao UniNFe

Use estes modelos como ponto de partida para paginas publicas finais. Remova secoes vazias quando nao fizerem sentido para o escopo. Nao preencha com suposicoes.

Nao inclua em paginas publicas:

- `Status`
- `Evidencias analisadas`
- `Pendencias`
- colunas `Evidencia`
- nomes de eventos, metodos, classes ou arquivos fonte quando o leitor so precisa operar/configurar o UniNFe
- comentarios sobre a documentacao estar incompleta, em elaboracao ou pendente

Registre evidencias, lacunas e status editorial apenas em `docs/_catalogo-documentacao.md` ou em inventario interno.

## Servico

```markdown
# Nome Do Servico

## O que e

Explique a finalidade do servico em linguagem de usuario/integrador.

## Quando usar

Explique os cenarios confirmados de uso.

## Como configurar

Liste configuracoes relacionadas, tela/campo quando confirmado e cuidados operacionais.

## Como operar

Explique o fluxo operacional confirmado.

## Como integrar por arquivos

### Arquivos de envio

| Arquivo/sufixo | Finalidade | Como usar |
|---|---|---|

### Arquivos de retorno

| Arquivo/sufixo | Quando e gerado | Como usar |
|---|---|---|

### Erros

Explique retornos de erro confirmados.

## Exemplos

Linke exemplos existentes no repositorio quando forem uteis ao leitor.

## Observacoes importantes

Registre cuidados, limites e dependencias.
```

## Tela Ou Formulario

```markdown
# Nome Da Tela

## Finalidade

Explique para que a tela existe.

## Onde acessar

Descreva o caminho de menu, botao ou atalho usando os nomes visiveis ao usuario.

## Campos

| Campo | O que faz | Como preencher |
|---|---|---|

## Botoes e acoes

| Acao | O que acontece | Quando usar |
|---|---|---|

## Validacoes e mensagens

Liste validacoes confirmadas e mensagens relevantes.

## Efeitos em arquivos ou configuracoes

Explique o que e salvo, alterado ou acionado.
```

## Configuracao

```markdown
# Nome Da Configuracao

## O que e

Explique a configuracao.

## Onde configurar

Informe tela, arquivo ou fluxo quando confirmado.

## Campos e valores

| Campo | Valores conhecidos | Impacto |
|---|---|---|

## Impacto operacional

Explique efeitos no funcionamento do UniNFe.

## Cuidados

Liste riscos, dependencias e recomendacoes confirmadas.
```

## Integracao Por Arquivos

```markdown
# Assunto Da Integracao Por Arquivos

## O que e

Explique o papel da troca de arquivos.

## Pastas envolvidas

| Pasta | Uso | Cuidados |
|---|---|---|

## Fluxo operacional

1. Passo confirmado.
2. Passo confirmado.

## Arquivos de entrada

| Nome/sufixo | Servico | Conteudo esperado |
|---|---|---|

## Arquivos de retorno

| Nome/sufixo | Quando e gerado | Como ler |
|---|---|---|

## Erros e logs

Explique arquivos `.err`, logs e mensagens confirmadas.
```
