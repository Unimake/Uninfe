# Instrucoes para GitHub Copilot

## Contexto do projeto

UniNFe e uma solucao C#/.NET Framework 4.8.1 para integracao fiscal por arquivos XML/TXT, com aplicacao WinForms/MetroFramework, servico Windows e bibliotecas compartilhadas. A solucao principal fica em `source/uninfe.sln`.

Principais projetos:

- `NFe.Components`: constantes, enums, extensoes de arquivos, helpers, schemas, funcoes comuns e infraestrutura compartilhada.
- `NFe.Settings`: configuracao global e por empresa (`Empresas.Configuracoes`, `Empresa`, `ConfiguracaoApp`).
- `NFe.Service`: processamento dos arquivos e integracao com servicos fiscais. A maioria das novas operacoes deve entrar como `Task* : TaskAbst`.
- `NFe.Validate`: assinatura e validacao de XML/schema.
- `NFe.ConvertTxt`: conversao de TXT para XML.
- `NFe.Threadings`: monitoramento de pastas, filas e processamento assíncrono.
- `NFe.UI` e `uninfe`: interface WinForms baseada em MetroFramework.
- `UniNFe.Service`: host do servico Windows.
- `MetroFramework`: biblioteca de UI embarcada no repositório; altere apenas quando a demanda for especificamente de infraestrutura visual.

## Regras gerais de implementacao

- Preserve o estilo atual do codigo: C# classico, namespaces por projeto (`NFe.Components`, `NFe.Service`, `NFe.Settings`, `NFe.UI`, etc.), regioes quando ja existirem no arquivo e nomes ja usados no dominio fiscal.
- Nao migrar projetos para SDK-style, .NET moderno, `PackageReference` ou novos frameworks sem solicitacao explicita.
- Nao introduzir dependencias novas se ja existir helper ou biblioteca no projeto, especialmente `Unimake.Business.DFe`, `Unimake.*`, `Functions`, `Auxiliar`, `TFunctions`, `GerarXML`, `LerXML` e `ValidarXMLNew`.
- Use `Path.Combine` em codigo novo sempre que possivel, mas preserve comparacoes e extensoes existentes quando elas forem parte do contrato com ERP/arquivos.
- Mensagens de erro e logs voltados ao usuario/ERP devem ser em portugues e seguir o tom objetivo ja existente.
- Evite refatoracoes amplas. Altere o menor conjunto de arquivos necessario para manter compatibilidade com integracoes existentes.

## Fluxo de servicos e arquivos

- O processamento central fica em `NFe.Service.Processar`.
- Para novo tipo de servico, normalmente atualizar:
  - `NFe.Components.Enums.Servicos`;
  - `NFe.Components.Propriedade.TipoEnvio` e `Propriedade.Extensao(...)`;
  - deteccao/validacao em `Processar.DefinirTipoServico` e `Processar.ValidarExtensao`, quando aplicavel;
  - `switch` de `Processar.ProcessaArquivo`;
  - `Processar.GravaErroERP` para extensao correta de retorno `.err`;
  - exemplos em `exemplos xml`, quando fizer sentido.
- Novas operacoes de envio/consulta devem preferir uma classe `Task... : TaskAbst`, com `Execute()` e `Servico` configurado no construtor ou antes do processamento, seguindo classes vizinhas do mesmo DFe.
- Retornos para o ERP devem usar `XmlRetorno(...)`, `oGerarXML.XmlRetorno(...)` ou `TFunctions.GravarArqErroServico(...)` conforme o padrao da tarefa equivalente.
- Preserve os sufixos de arquivo (`-ped-...xml`, `-ret-...xml`, `.err`, `-proc...xml`) porque eles sao contrato externo com os ERPs.

## XML, validacao e certificado

- Use `XmlDocument` e as rotinas existentes quando o codigo ao redor ja trabalha com DOM XML.
- Para classes tipadas de DFe, prefira os modelos e servicos de `Unimake.Business.DFe`.
- Sempre respeite `Empresas.Configuracoes[emp]` para ambiente, UF, certificado, proxy, CSC, responsavel tecnico e pastas.
- Antes de envio que exige certificado, siga os padroes existentes de `CertVencido(emp)`, `CarregarPINA3(emp)` e configuracao `CertificadoDigital`.
- Nao ignore validacoes de ambiente (`tpAmb`), tipo de emissao (`tpEmis`) e chave do DFe.

## UI e configuracoes

- A UI e WinForms com MetroFramework. Telas novas devem seguir o padrao de `NFe.UI.Formularios`, `MetroUserControl`, `UserControl1` e exibicao via `menu`, `FormDummy` ou `MetroTaskWindow`, conforme o fluxo existente.
- Nao editar manualmente arquivos `.Designer.cs` ou `.resx` salvo quando necessario e consistente com WinForms.
- Configuracoes persistidas devem passar por `ConfiguracaoApp`, `Empresas`, `Empresa` e os XMLs de configuracao existentes; nao criar formatos paralelos.

## Concorrencia, arquivos e logs

- O projeto processa arquivos monitorados em pastas. Evite mudancas que quebrem fila, temporarios, locks, `EmProcessamento`, `Retorno`, `Erro`, `Enviados` e subpastas por empresa.
- Use `Auxiliar.WriteLog(...)` ou `Functions.WriteLog(...)` conforme o arquivo atual. Em hosts de servico Windows, use `Program.WriteLog(...)` quando ja for o padrao local.
- Em blocos `catch`, preserve a gravacao de erro para ERP quando existir. Nao substituir erros de retorno por apenas log interno.
- Nao engolir excecoes novas sem log ou sem retorno de erro quando o fluxo espera arquivo de resposta.

## Build e validacao

- A solucao principal e `source/uninfe.sln`; projetos usam .NET Framework 4.8.1 e `packages.config`.
- Antes de finalizar mudancas relevantes, validar com build do projeto ou da solucao afetada, por exemplo `dotnet build source/uninfe.sln --no-restore` quando o ambiente permitir.
- Nao ha suite de testes automatizados evidente no repositorio; para alteracoes de servico, validar por build e por exemplos XML/TXT equivalentes em `exemplos xml` quando possivel.

## Cuidados especiais

- Contratos com ERPs sao baseados em nomes de arquivos, extensoes, XMLs de entrada/retorno e pastas configuradas. Trate esses nomes como API publica.
- Compatibilidade retroativa e mais importante que modernizacao de estilo.
- Nao alterar `MetroFramework`, arquivos de setup ou exemplos XML em massa sem necessidade direta.
- Ao mexer em NFSe, observar padroes por provedor/municipio em `NFe.Components.Schemas` e exemplos separados por padrao.
