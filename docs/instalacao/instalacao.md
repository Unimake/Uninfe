# Instalação do UniNFe

Esta página orienta a instalação do UniNFe em uma estação ou servidor Windows.

## Antes de começar

Antes de executar o instalador, confirme os pontos abaixo:

1. Defina em qual computador o UniNFe será executado.
2. Verifique se esse computador possui acesso ao certificado digital usado pelas empresas.
3. Se o certificado for **A1**, confirme que o computador tem acesso ao arquivo do certificado e à senha.
4. Se o certificado for instalado no Windows, instale-o no mesmo usuário do Windows que executará o UniNFe.
5. Garanta que o usuário do Windows tenha permissão para criar e gravar nas pastas que serão usadas pelo UniNFe.

O usuário usado para instalar ou acessar o certificado deve ser o mesmo usado para executar o UniNFe. Isso evita falhas de acesso ao certificado digital.

## Requisito do .NET Framework

O UniNFe depende do **.NET Framework 4.8** ou superior, conforme o instalador utilizado.

Durante a instalação, o setup verifica se o .NET Framework necessário está instalado. Se não estiver, o instalador pode solicitar o download e a instalação do framework antes de continuar.

Quando o Windows não atender ao requisito do framework, instale ou atualize o .NET Framework e execute novamente o instalador do UniNFe.

## Baixar o instalador

Baixe o instalador do UniNFe no site do projeto:

[www.uninfe.com.br](http://www.uninfe.com.br)

Escolha o instalador conforme a necessidade do ambiente:

| Instalador | Quando usar |
|---|---|
| Monitor para DF-e | Para integração com documentos fiscais eletrônicos como NF-e, NFC-e, CT-e, MDF-e, NFS-e e demais serviços suportados pelo UniNFe. |
| Versão BETA | Para testes de versão em desenvolvimento. Não use em produção sem orientação da Unimake. |

## Instalar o UniNFe

1. Localize o arquivo do instalador baixado.
2. Execute o instalador.
3. Siga as orientações do assistente de instalação.
4. Mantenha ou altere a pasta de instalação conforme o padrão da empresa.
5. Conclua a instalação.

Por padrão, o instalador usa a pasta:

```text
\Unimake\UniNFe
```

Ao final da instalação, o UniNFe é executado automaticamente.

## Primeira execução

Depois de instalado, o UniNFe é aberto e fica disponível na área de notificação do Windows, perto do relógio.

Para abrir a tela principal:

1. Localize o ícone do UniNFe na área de notificação do Windows.
2. Dê duplo clique no ícone.

O instalador também cria um atalho no menu iniciar:

```text
Unimake Softwares > UniNFe - Monitor DF-e
```

Use esse atalho quando precisar abrir o UniNFe manualmente.

## Configurar o UniNFe após instalar

Após abrir o UniNFe pela primeira vez:

1. Acesse **Configurações**.
2. Cadastre a empresa em **Empresas > Nova**.
3. Configure serviço fiscal, ambiente e UF ou município.
4. Configure as pastas de envio, retorno, erro, enviados, backup e demais pastas usadas pela integração.
5. Configure o certificado digital.
6. Salve as configurações.
7. Faça um teste de comunicação, como a consulta de status de serviço.

Consulte também:

- [Tela Configurações](../configuracao/telas-cadastros-configuracoes.md)
- [Cadastro de Nova Empresa](../configuracao/cadastro-nova-empresa.md)
- [Consulta status de serviço](../servicos/consulta-status-servico.md)

## Instalar o UniNFe para executar como serviço

A instalação normal abre o UniNFe como aplicativo de monitoramento na área de notificação do Windows.

Se o ambiente precisar executar o UniNFe como serviço do Windows, primeiro instale e configure o UniNFe normalmente. Depois siga o procedimento específico:

[Executar o UniNFe como serviço no Windows](executar-como-servico-windows.md)

## UniDANFE

O UniNFe não substitui a configuração do aplicativo de impressão. Se o ambiente usa UniDANFE ou DANFE Mon para impressão, instale e configure essas ferramentas separadamente e depois informe os caminhos necessários na aba **DANFE** das configurações da empresa.

## Checklist de instalação

Antes de liberar o UniNFe para uso, confirme:

- o UniNFe foi instalado no computador correto;
- o .NET Framework necessário está instalado;
- o certificado digital está acessível para o usuário que executa o UniNFe;
- a empresa foi cadastrada;
- as pastas de integração foram configuradas;
- o ambiente fiscal está correto;
- a consulta de status de serviço foi testada;
- o ERP consegue gravar arquivos na pasta de envio e ler retornos na pasta de retorno.
