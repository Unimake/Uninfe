# Mapa do handoff da DLL — UAB-002

## Estado aprovado

- Checkout: `C:\projetos\github\Unimake.DFe`.
- Árvore Git: limpa na conferência.
- Commit: `3f88112533c3890e4d071879bac4ae551cd650b0` (`docs(nfeabi): registra aprovação final e encerra o plano ABI`).
- Governança: ABI-006 e REQ-010 estão `APPROVED`; o dossiê ABI-006 registra 71/71 testes determinísticos e 64 tipos públicos.

## Referência Debug

- `source/UniNFe.Test/UniNFe.Test.csproj` usa `ProjectReference` para `..\..\..\Unimake.DFe\source\.NET Standard\Unimake.Business.DFe\Unimake.Business.DFe.csproj` quando `Configuration != Release`.
- A avaliação MSBuild com `Configuration=Debug` resolveu `Configuration=Debug`, `TargetFramework=netstandard2.0` e o caminho absoluto do checkout irmão.
- Artefato consumidor: `source/UniNFe.Test/bin/Debug/net481/Unimake.Business.DFe.dll`.
- Artefato irmão: `C:\projetos\github\Unimake.DFe\source\Unimake.DFe\Compilacao\netstandard2.0\Unimake.Business.DFe.dll`.
- SHA-256 de ambos: `626AC2A0EB712E97BA48474F90F6522768C09AB0B5324F68A2E8752545E4F138` — cópia byte a byte idêntica.

Os projetos clássicos do UniNFe mantêm `OutputPath` legado em `bin\Release` inclusive na PropertyGroup `Debug|AnyCPU`; isso não representa seleção Release. O comando usou `Configuration=Debug`, preservou `DEBUG` e a referência efetiva da DLL foi o ProjectReference Debug comprovado acima.

## API mínima consumida

- Modelo `ModeloDFe.NFeABI = 77`.
- Tipos XML públicos `NFeABI`, `ConsStatServNFeABI`, `RetConsStatServNFeABI` e `RetNFeABI`, com namespace oficial.
- Construtor tipado de `Servicos.NFeABI.StatusServico(ConsStatServNFeABI, Configuracao)`.
- Construtor tipado de `Servicos.NFeABI.AutorizacaoSinc(NFeABI, Configuracao)`.

O smoke test apenas compila e inspeciona esses contratos; não executa rede, certificado, serviço fiscal ou fluxo por arquivo.

## Estado posterior ao gate

O checkout irmão estava limpo durante todas as validações acima. Após o CHECK/ACT, apareceram mudanças locais externas relacionadas à remoção do evento NFe 211120. A inspeção complementar confirmou que elas não modificam arquivos NFeABI, `ModeloDFe.NFeABI` nem os construtores tipados testados. Os resultados coletados enquanto a árvore estava limpa continuam válidos; nenhuma validação desta etapa cobre a composição posterior. Revalidar o checkout antes de qualquer nova compilação.
