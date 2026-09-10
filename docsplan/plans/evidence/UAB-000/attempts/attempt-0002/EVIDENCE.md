# UniNFe - NF-e ABI - Evidência UAB-000

- Etapa: UAB-000
- Status: DELIVERED_FOR_REVIEW
- AttemptId: attempt-0002
- Estado-base: attempt-0001 entregue; correção de localização solicitada pelo DEV em 2026-09-10
- Próxima etapa iniciada: NÃO

## Escopo e checkpoint

Os sete caminhos exclusivos do pacote foram movidos de `docs/` para `docsplan/`. Referências operacionais, manifests, orquestradores, dossiê e linter agora apontam para `docsplan/`. O conteúdo preexistente de `docs/` permaneceu no lugar.

## Ambiente

Windows win-x64; PowerShell 7.6.5; .NET SDK 10.0.401. Credenciais e certificado não foram acessados.

## Rollback e cleanup

Restaurar os sete caminhos exclusivos para `docs/` e reverter somente as referências de planejamento desta tentativa.
