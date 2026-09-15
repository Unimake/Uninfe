# Auditoria de integração

O contrato foi integrado aos enums, extensões, detecção e configuração existentes. Guards exigem raiz e sufixo compatíveis; o seletor de configuração inclui NF-e ABI, a consulta manual a exclui até a UAB-004 e `Todos` detecta as duas raízes. O helper consumido por `GravaErroERP` foi exercitado para autorização e Status.

O diff foi auditado sem ocorrência de NFeABI em `Task*.cs` ou `userPedidoSituacao.cs`. Nenhum dispatch, transporte, geração de `procNFeABI` ou endpoint foi implementado; `-procNFeABI.xml` é somente uma constante pública nesta etapa.

O checkout irmão foi revalidado após avançar externamente para `308864d94`. Ele é descendente de `3f8811253`; o diff adicional trata configuração NFSe de Duque de Caxias e remoção do evento NFe 211120, sem mudanças em `Servicos/NFeABI`, `Xml/NFeABI` ou nos membros NFeABI dos enums.
