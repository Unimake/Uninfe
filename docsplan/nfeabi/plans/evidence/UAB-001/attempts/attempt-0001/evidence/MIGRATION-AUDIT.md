# Auditoria da migração do plano — UAB-001

- Origem solicitada: `docsplan/`
- Destino solicitado: `docsplan/nfeabi/`
- Resultado: `docsplan/` contém somente o diretório `nfeabi`; todos os arquivos do plano foram movidos para esse diretório.
- Referências: README, AGENTS, instruções, skills, scripts do linter, manifests, planos e dossiês foram adaptados para `docsplan/nfeabi/`.
- Duplicação: nenhuma ocorrência de `docsplan/nfeabi/nfeabi` foi encontrada.
- Caminho normativo: ocorrências escapadas incorretamente de `PL_NFeABI_1.00` foram normalizadas.
- Histórico: os dossiês UAB-000 permanecem disponíveis na nova raiz. O linter conserva seus snapshots e deixa de comparar arquivos vivos depois que a etapa está `APPROVED`, alinhado ao contrato já usado no plano da DLL.
- Produto: nenhum arquivo sob `source/`, `exemplos xml/` ou outro subtree de produto foi alterado.
