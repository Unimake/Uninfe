# UniNFe Codex Instructions

## Project Context

UniNFe is a C#/.NET Framework 4.8.1 solution for fiscal XML/TXT integration. It includes a WinForms/MetroFramework UI, a Windows Service host, service-processing projects, validation, settings, conversion, and shared libraries. The main solution is `source/uninfe.sln`.

Main projects:

- `NFe.Components`: constants, enums, file extensions, helpers, schemas, common functions, and shared infrastructure.
- `NFe.Settings`: global and per-company configuration (`Empresas.Configuracoes`, `Empresa`, `ConfiguracaoApp`).
- `NFe.Service`: file processing and fiscal-service integration. Most new operations should be implemented as `Task* : TaskAbst`.
- `NFe.Validate`: XML/schema validation and signing.
- `NFe.ConvertTxt`: TXT to XML conversion.
- `NFe.Threadings`: folder monitoring, queues, and asynchronous processing.
- `NFe.UI` and `uninfe`: WinForms UI based on MetroFramework.
- `UniNFe.Service`: Windows Service host.
- `MetroFramework`: embedded UI library; change it only for explicit visual infrastructure requests.

## Implementation Rules

- Preserve the current style: classic C#, project-based namespaces (`NFe.Components`, `NFe.Service`, `NFe.Settings`, `NFe.UI`, etc.), existing domain names, and `#region` usage where files already use it.
- Do not migrate projects to SDK-style, modern .NET, `PackageReference`, or newer frameworks unless explicitly requested.
- Avoid new dependencies when an existing helper or library already covers the need, especially `Unimake.Business.DFe`, `Unimake.*`, `Functions`, `Auxiliar`, `TFunctions`, `GerarXML`, `LerXML`, and `ValidarXMLNew`.
- Use `Path.Combine` in new code when practical, but preserve existing comparisons, suffixes, and extensions when they are part of the ERP/file contract.
- User-facing and ERP-facing errors/logs should be in Portuguese and follow the objective tone already used in the codebase.
- Avoid broad refactors. Change the smallest set of files needed to preserve compatibility with existing integrations.

## Service And File Flow

- Central processing is in `NFe.Service.Processar`.
- A new service type usually requires coordinated updates to:
  - `NFe.Components.Enums.Servicos`;
  - `NFe.Components.Propriedade.TipoEnvio` and `Propriedade.Extensao(...)`;
  - detection/validation in `Processar.DefinirTipoServico` and `Processar.ValidarExtensao`, when applicable;
  - the `Processar.ProcessaArquivo` switch;
  - `Processar.GravaErroERP` for the correct `.err` return extension;
  - examples under `exemplos xml`, when relevant.
- New send/query operations should prefer a `Task... : TaskAbst` class with `Execute()` and `Servico` set in the constructor or before processing, following neighboring classes for the same DFe.
- ERP returns should use `XmlRetorno(...)`, `oGerarXML.XmlRetorno(...)`, or `TFunctions.GravarArqErroServico(...)` according to the equivalent local pattern.
- Preserve file suffixes such as `-ped-...xml`, `-ret-...xml`, `.err`, and `-proc...xml`; they are public contracts with ERPs.

## XML, Validation, And Certificates

- Use `XmlDocument` and existing routines when surrounding code already works with DOM XML.
- For typed DFe classes, prefer `Unimake.Business.DFe` models and services.
- Always respect `Empresas.Configuracoes[emp]` for environment, UF, certificate, proxy, CSC, technical responsible data, and folders.
- Before sending anything that requires a certificate, follow existing patterns for `CertVencido(emp)`, `CarregarPINA3(emp)`, and `CertificadoDigital`.
- Do not bypass validations for `tpAmb`, `tpEmis`, DFe key, schemas, or signatures.

## UI And Configuration

- The UI is WinForms with MetroFramework. New screens should follow `NFe.UI.Formularios`, `MetroUserControl`, `UserControl1`, and display through `menu`, `FormDummy`, or `MetroTaskWindow`, according to the existing flow.
- Do not manually edit `.Designer.cs` or `.resx` unless necessary and consistent with WinForms.
- Persisted settings should go through `ConfiguracaoApp`, `Empresas`, `Empresa`, and existing configuration XMLs. Do not create parallel formats.

## Concurrency, Files, And Logs

- The project processes monitored folders. Avoid breaking queues, temporary files, locks, `EmProcessamento`, `Retorno`, `Erro`, `Enviados`, or per-company subfolders.
- Use `Auxiliar.WriteLog(...)` or `Functions.WriteLog(...)` according to the current file. In Windows Service hosts, use `Program.WriteLog(...)` when that is the local pattern.
- In `catch` blocks, preserve ERP error-return generation when it already exists. Do not replace expected response files with only internal logs.
- Do not swallow new exceptions without logging or without returning an error file when the flow expects one.

## Build And Validation

- Main solution: `source/uninfe.sln`.
- Projects use .NET Framework 4.8.1 and `packages.config`.
- Before finishing relevant changes, validate with a build of the affected project or solution when the environment allows it, for example:

```powershell
dotnet build source/uninfe.sln --no-restore
```

- There is no broad automated test suite evident in the repository. For service changes, validate by build and with equivalent XML/TXT examples in `exemplos xml` when practical.

## Special Care

- ERP contracts are based on file names, extensions, input/return XMLs, and configured folders. Treat these names as public APIs.
- Backward compatibility is more important than style modernization.
- Do not change `MetroFramework`, setup files, or XML examples in bulk without a direct need.
- When changing NFSe, observe provider/city patterns in `NFe.Components.Schemas` and separated examples under `exemplos xml`.

## Documentation Guidance

- The UniNFe documentation must help users, support, and ERP integrators install, configure, operate, and integrate with the application.
- Document behavior only from evidence found in code, WinForms forms, configuration files, XML/TXT models, examples, existing docs, or repository scripts.
- Do not invent screens, service flows, folder behavior, file suffixes, validations, or operational steps. Mark uncertain items as `PENDENTE DE VALIDAÇÃO`.
- Prefer simple Markdown in Portuguese do Brasil, with small linked pages instead of one large document.
- Keep `docs/index.md` and `docs/_catalogo-documentacao.md` updated whenever documentation pages are created, moved, reviewed, or changed.
- Use the `uninfe-documentacao` skill for tasks involving UniNFe documentation, service documentation, screen/form documentation, configuration documentation, file-exchange integration documentation, Markdown page organization, index updates, or documentation review.
