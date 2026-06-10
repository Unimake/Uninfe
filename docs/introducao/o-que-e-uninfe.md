# O que é o UniNFe

O **UniNFe** é um monitor de documentos fiscais eletrônicos desenvolvido pela Unimake Software para simplificar a integração entre sistemas ERP e os serviços fiscais brasileiros.

Ele atua como uma camada intermediária entre o ERP e os ambientes da SEFAZ, prefeituras, Receita Federal e outros órgãos integrados. O ERP gera arquivos de envio, grava esses arquivos nas pastas configuradas e o UniNFe se encarrega de monitorar, validar, assinar, transmitir, receber retornos e gravar os arquivos de resposta.

O projeto é gratuito, open source e mantido pela Unimake Software. A página oficial do projeto está disponível em:

[https://inside.unimake.com.br/uninfe](https://inside.unimake.com.br/uninfe)

## Para que serve

O UniNFe foi criado para reduzir a complexidade fiscal dentro do ERP. Em vez de cada sistema implementar diretamente a comunicação com todos os webservices fiscais, o UniNFe centraliza essa integração por meio de arquivos XML e TXT.

Com ele, o ERP pode:

- enviar documentos fiscais eletrônicos;
- consultar status de serviço;
- consultar situação de documentos;
- enviar eventos fiscais;
- receber retornos dos serviços fiscais;
- validar XML;
- assinar XML com certificado digital;
- organizar arquivos de envio, retorno, erro, enviados, autorizados e backup;
- trabalhar com múltiplas empresas e serviços fiscais na mesma instalação.

## Como funciona a integração

A integração principal é feita por troca de arquivos.

1. O ERP grava o XML ou TXT na pasta de envio configurada.
2. O UniNFe monitora a pasta.
3. O UniNFe identifica o serviço solicitado pelo arquivo.
4. O XML é validado, assinado quando necessário e transmitido ao serviço fiscal.
5. O retorno é gravado na pasta de retorno configurada.
6. Em caso de erro, o UniNFe grava o retorno de erro conforme o fluxo do serviço.

Esse modelo permite que o ERP continue independente da comunicação direta com cada webservice fiscal, usando as pastas configuradas como contrato de integração.

## Quem utiliza

O UniNFe é voltado principalmente para:

- desenvolvedores de ERP;
- software houses;
- equipes de suporte técnico;
- empresas que precisam integrar emissão, consulta e retorno de documentos fiscais eletrônicos;
- ambientes que precisam executar o monitor na área de notificação do Windows ou como serviço do Windows.

## Documentos e serviços atendidos

O UniNFe atende os seguintes documentos e serviços fiscais configuráveis no monitor:

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
| GNRE | Guia Nacional de Recolhimento de Tributos Estaduais |
| DARE | Documento de Arrecadação de Receitas Estaduais |
| EFD-Reinf | Escrituração Fiscal Digital de Retenções e Outras Informações Fiscais |
| eSocial | Sistema de Escrituração Digital das Obrigações Fiscais, Previdenciárias e Trabalhistas |
| CIOT | Código Identificador da Operação de Transporte |

O monitor também possui uma opção de configuração para **Todos**, usada para processar vários documentos fiscais eletrônicos em uma mesma empresa, exceto NFS-e.

## Relação com outros produtos Unimake

O UniNFe é o monitor por troca de arquivos. Ele é indicado quando o ERP integra com o ambiente fiscal gravando arquivos em pastas monitoradas.

Quando a integração desejada é direta por código, sem troca de arquivos com o monitor, a alternativa é usar a biblioteca **Unimake.DFe**, também mantida pela Unimake.

## Próximos passos

Para começar a usar o UniNFe:

1. Instale o aplicativo.
2. Cadastre a empresa.
3. Configure certificado, ambiente, serviço fiscal e pastas.
4. Teste a comunicação com a consulta de status de serviço.
5. Integre o ERP gravando arquivos nas pastas configuradas.

Consulte também:

- [Instalação do UniNFe](../instalacao/instalacao.md)
- [Tela Configurações](../configuracao/telas-cadastros-configuracoes.md)
- [Consulta status de serviço](../servicos/consulta-status-servico.md)
- [Executar o UniNFe como serviço no Windows](../instalacao/executar-como-servico-windows.md)
