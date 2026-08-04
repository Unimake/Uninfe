# UniNFe

Monitor open source para integração de sistemas ERP com documentos fiscais eletrônicos brasileiros por troca de arquivos XML e TXT.

[![Licença MIT](https://img.shields.io/badge/licen%C3%A7a-MIT-blue.svg)](LICENSE)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8.1-512BD4.svg)](source/uninfe.sln)
[![Documentação](https://img.shields.io/badge/documenta%C3%A7%C3%A3o-online-0B6E4F.svg)](https://www.unimake.com.br/uninfe/docs)

## Sobre o projeto

O **UniNFe** é um monitor de documentos fiscais eletrônicos desenvolvido pela **Unimake Software** para simplificar a comunicação entre sistemas ERP e os serviços fiscais brasileiros.

O ERP grava arquivos em pastas configuradas, e o UniNFe se encarrega de monitorar, validar, assinar, transmitir, receber retornos e organizar os arquivos gerados pelos webservices fiscais.

Esse modelo reduz a necessidade de cada ERP implementar diretamente a comunicação com todos os serviços fiscais, mantendo um contrato simples baseado em nomes de arquivos, pastas de integração e XML/TXT.

Página oficial do projeto:

[https://inside.unimake.com.br/uninfe](https://inside.unimake.com.br/uninfe)

## Documentação

A documentação completa está disponível em:

[https://www.unimake.com.br/uninfe/docs](https://www.unimake.com.br/uninfe/docs)

Links diretos para os principais assuntos:

| Assunto | Link |
|---|---|
| O que é o UniNFe | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=introducao/o-que-e-uninfe.md) |
| Instalação | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=instalacao/instalacao.md) |
| Atualização do UniNFe | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=instalacao/atualizacao.md) |
| Executar como serviço do Windows | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=instalacao/executar-como-servico-windows.md) |
| Configurações e empresas | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=configuracao/telas-cadastros-configuracoes.md) |
| Cadastro de nova empresa | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=configuracao/cadastro-nova-empresa.md) |
| Configuração automática por arquivo | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=configuracao/configuracao-automatica-por-arquivo.md) |
| Consulta status de serviço | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=servicos/consulta-status-servico.md) |
| Consulta cadastro de contribuinte | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=servicos/consulta-cadastro-contribuinte.md) |
| Perguntas frequentes | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=referencias/perguntas-frequentes.md) |
| Apoio e suporte | [Abrir documentação](https://www.unimake.com.br/uninfe/docs/viewer/?doc=referencias/apoio-e-suporte.md) |

Também é possível consultar a documentação diretamente no repositório em [docs](docs/).

## Como funciona

O fluxo principal do UniNFe é baseado em pastas monitoradas:

1. O ERP gera um XML ou TXT conforme o serviço fiscal desejado.
2. O arquivo é gravado na pasta de envio da empresa.
3. O UniNFe identifica o serviço pelo conteúdo e pelo sufixo do arquivo.
4. O XML é validado e assinado quando necessário.
5. O documento ou consulta é transmitido ao webservice fiscal.
6. O retorno é gravado na pasta de retorno configurada.
7. Arquivos autorizados, rejeitados, em erro ou em processamento são organizados nas pastas correspondentes.

Os sufixos de arquivos, nomes de retornos e pastas configuradas fazem parte do contrato de integração com o ERP.

## Documentos e serviços atendidos

O UniNFe atende diversos documentos e serviços fiscais, incluindo:

| Sigla | Significado |
|---|---|
| NF-e | Nota Fiscal Eletrônica |
| NFC-e | Nota Fiscal de Consumidor Eletrônica |
| CT-e | Conhecimento de Transporte Eletrônico |
| MDF-e | Manifesto Eletrônico de Documentos Fiscais |
| NFS-e | Nota Fiscal de Serviço Eletrônica |
| NF3-e | Nota Fiscal de Energia Elétrica Eletrônica |
| NFCom | Nota Fiscal Fatura de Serviço de Comunicação Eletrônica |
| NFGas | Nota Fiscal Eletrônica do Gás |
| DC-e | Declaração de Conteúdo Eletrônica |
| BP-e | Bilhete de Passagem Eletrônico |
| GNRE | Guia Nacional de Recolhimento de Tributos Estaduais |
| DARE | Documento de Arrecadação de Receitas Estaduais |
| EFD-Reinf | Escrituração Fiscal Digital de Retenções e Outras Informações Fiscais |
| eSocial | Sistema de Escrituração Digital das Obrigações Fiscais, Previdenciárias e Trabalhistas |
| CIOT | Código Identificador da Operação de Transporte |
| PIX | Serviços de consulta e criação de cobranças PIX |
| eBoleto | Registro, consulta e manutenção de boletos |
| uMessenger | Envio de mensagens por integração |

## Documentação de serviços por arquivo

Alguns pontos de partida para integradores:

| Serviço | Documentação |
|---|---|
| NF-e e NFC-e - autorização | [docs/servicos/nfe/autorizacao.md](docs/servicos/nfe/autorizacao.md) |
| NF-e e NFC-e - eventos | [docs/servicos/nfe/eventos.md](docs/servicos/nfe/eventos.md) |
| CT-e - autorização | [docs/servicos/cte/autorizacao-sincrona.md](docs/servicos/cte/autorizacao-sincrona.md) |
| MDF-e - autorização | [docs/servicos/mdfe/autorizacao-sincrona.md](docs/servicos/mdfe/autorizacao-sincrona.md) |
| NFCom - autorização | [docs/servicos/nfcom/autorizacao-sincrona.md](docs/servicos/nfcom/autorizacao-sincrona.md) |
| NFGas - autorização | [docs/servicos/nfgas/autorizacao-sincrona.md](docs/servicos/nfgas/autorizacao-sincrona.md) |
| NF3-e - autorização | [docs/servicos/nf3e/autorizacao-sincrona.md](docs/servicos/nf3e/autorizacao-sincrona.md) |
| BP-e - autorização | [docs/servicos/bpe/autorizacao-sincrona.md](docs/servicos/bpe/autorizacao-sincrona.md) |
| DC-e - autorização | [docs/servicos/dce/autorizacao-sincrona.md](docs/servicos/dce/autorizacao-sincrona.md) |
| Distribuição DF-e | [docs/servicos/dfe/distribuicao-dfe.md](docs/servicos/dfe/distribuicao-dfe.md) |
| GNRE - recepção de lote | [docs/servicos/gnre/recepcao-lote.md](docs/servicos/gnre/recepcao-lote.md) |
| eSocial - recepção de lote | [docs/servicos/esocial/recepcao-lote-eventos.md](docs/servicos/esocial/recepcao-lote-eventos.md) |
| EFD-Reinf - recepção de lote | [docs/servicos/efdreinf/recepcao-lote-eventos.md](docs/servicos/efdreinf/recepcao-lote-eventos.md) |
| PIX - criar cobrança | [docs/servicos/pix/criar-cobranca.md](docs/servicos/pix/criar-cobranca.md) |
| eBoleto - registrar boleto | [docs/servicos/eboleto/registrar-boleto.md](docs/servicos/eboleto/registrar-boleto.md) |

O índice completo está em [docs/index.md](docs/index.md).

## Requisitos para uso

Para executar o UniNFe, consulte a documentação de instalação. Em linhas gerais, o ambiente precisa de:

- Windows;
- .NET Framework 4.8 ou superior, conforme o instalador utilizado;
- certificado digital A1 ou certificado instalado no Windows, de acordo com a empresa emissora;
- permissões de leitura e gravação nas pastas configuradas;
- acesso aos webservices fiscais usados pela empresa;
- configuração de proxy, quando exigida pela rede.

Guia de instalação:

[docs/instalacao/instalacao.md](docs/instalacao/instalacao.md)

## Estrutura do repositório

| Pasta | Conteúdo |
|---|---|
| [source](source/) | Solução principal, projetos C#/.NET Framework, UI, serviços, validação, configurações e bibliotecas internas. |
| [docs](docs/) | Documentação Markdown e visualizador estático da documentação. |
| [exemplos xml](exemplos%20xml/) | Exemplos de XML/TXT usados na integração por arquivos. |
| [setup](setup/) | Arquivos relacionados ao instalador. |
| [planos](planos/) | Materiais de planejamento e apoio do projeto. |

Solução principal:

[source/uninfe.sln](source/uninfe.sln)

## Desenvolvimento

O projeto usa **.NET Framework 4.8.1** e projetos no formato clássico do Visual Studio.

Para validar alterações quando o ambiente permitir:

```powershell
dotnet build source/uninfe.sln --no-restore
```

Ao alterar ou criar páginas em `docs`, atualize o índice do visualizador:

```powershell
cd docs
node viewer/build-docs-index.js
```

Esse comando recria:

- `docs/viewer/docs-manifest.json`
- `docs/viewer/search-index.json`

## Suporte e comunidade

Antes de pedir ajuda, consulte a documentação e identifique em qual etapa ocorre o problema: instalação, configuração, certificado, envio, validação, retorno do webservice, leitura do retorno pelo ERP ou execução como serviço.

Canais de apoio:

- [Discord - Unimake Groups](https://discord.gg/UwFPRxJp3N)
- [Google Groups - UniNFe](https://groups.google.com/g/uninfe)
- [Telegram - UniNFe](https://t.me/joinchat/Lly8_xQkn2NNi4yHN5aPqw)
- [Página oficial do UniNFe](https://inside.unimake.com.br/uninfe)

Veja também:

[docs/referencias/apoio-e-suporte.md](docs/referencias/apoio-e-suporte.md)

## Licença

Este projeto é distribuído sob a licença MIT.

Consulte [LICENSE](LICENSE).
