# Executar o UniNFe como Serviço no Windows

O UniNFe pode ser executado como serviço do Windows para processar arquivos mesmo sem um usuário logado na máquina. Nesse modo, o monitor trabalha em segundo plano e não aparece na bandeja do Windows.

Use esta configuração em servidores ou estações dedicadas em que o UniNFe deve iniciar junto com o Windows e continuar processando os arquivos das empresas configuradas.

## Antes de instalar o serviço

1. Faça logon no Windows com o mesmo usuário que será usado para executar o serviço.
2. Instale o certificado digital nesse usuário, quando for utilizado certificado instalado no Windows.
3. Instale o UniNFe.
4. Abra o UniNFe normalmente pela interface.
5. Configure as empresas, pastas, certificados, ambiente e demais opções necessárias.
6. Teste o funcionamento pela interface antes de instalar o serviço.

O serviço usa as configurações já gravadas no UniNFe. Por isso, a instalação como serviço deve ser feita somente depois que o aplicativo estiver funcionando corretamente pela interface.

## Instalar o serviço

1. Feche o UniNFe, se ele estiver aberto.
2. Abra o **Prompt de Comando** como administrador.
3. Acesse a pasta onde o UniNFe está instalado.
4. Execute o comando:

```cmd
UniNFe.Service install
```

Após a instalação, o serviço fica registrado no Windows com o nome **UniNFeServico**.

## Configurar o usuário do serviço

Depois de instalar o serviço, configure o usuário usado para executá-lo.

1. Abra o gerenciador de serviços do Windows.
2. Localize o serviço **UniNFeServico**.
3. Clique com o botão direito sobre o serviço e escolha **Propriedades**.
4. Acesse a aba **Logon**.
5. Marque **Esta conta**.
6. Informe o usuário e a senha que devem executar o serviço.
7. Confirme em **OK**.

Use o mesmo usuário em que o certificado digital foi instalado. Se o serviço for executado com outro usuário, o UniNFe pode não conseguir acessar o certificado instalado no Windows.

## Iniciar ou reiniciar o computador

Depois de configurar o usuário do serviço, reinicie o computador ou inicie o serviço manualmente pelo gerenciador de serviços do Windows.

Ao iniciar, o serviço aguarda as dependências do sistema antes de começar o processamento. Se as dependências demorarem, o serviço tenta iniciar por alguns minutos e registra o andamento no log do UniNFe.

Quando o serviço é iniciado, ele:

- carrega as configurações gerais do UniNFe;
- carrega as empresas cadastradas;
- verifica certificados configurados;
- executa conversões necessárias de versão;
- inicia o processamento das pastas monitoradas.

Se o serviço não conseguir ler o certificado de alguma empresa configurada, o UniNFe grava uma mensagem de erro na pasta de retorno da empresa e também registra o problema no log.

## Testar o serviço

Para confirmar que o serviço está funcionando:

1. Reinicie o computador ou inicie o serviço **UniNFeServico**.
2. Não abra o UniNFe pela interface.
3. Gere um XML de consulta de status de serviço pelo ERP ou pelo processo de integração usado pela empresa.
4. Grave o XML na pasta de envio configurada.
5. Aguarde o processamento.
6. Verifique se o UniNFe gerou o arquivo de retorno na pasta de retorno configurada.

Se o retorno for gerado, o serviço está processando os arquivos corretamente.

Se o retorno não for gerado, revise:

- se o serviço **UniNFeServico** está iniciado;
- se as pastas configuradas existem e o usuário do serviço tem permissão de leitura e gravação;
- se o certificado está acessível para o usuário configurado na aba **Logon** do serviço;
- se as configurações da empresa foram salvas antes da instalação do serviço;
- os logs do UniNFe.

## Usar a interface quando o serviço está instalado

Quando o UniNFe está rodando como serviço, a interface do aplicativo não deve ser aberta ao mesmo tempo para processar arquivos.

Para abrir a interface e alterar configurações:

1. Pare o serviço **UniNFeServico** no gerenciador de serviços do Windows.
2. Abra o UniNFe pela interface.
3. Faça as alterações necessárias.
4. Feche o UniNFe.
5. Inicie novamente o serviço **UniNFeServico**.

## Remover o UniNFe da inicialização do Windows

Depois de instalar o UniNFe como serviço, remova o aplicativo **UniNFe** da inicialização do Windows. Isso evita que a interface seja aberta automaticamente junto com o serviço.

No Windows:

1. Abra o **Gerenciador de Tarefas**.
2. Acesse a aba **Inicializar**.
3. Localize **UniNFe**.
4. Clique com o botão direito.
5. Selecione **Desabilitar**.

## Parar o serviço

Para parar o processamento em segundo plano:

1. Abra o gerenciador de serviços do Windows.
2. Localize **UniNFeServico**.
3. Clique com o botão direito.
4. Selecione **Parar**.

Ao parar, o serviço interrompe o processamento das pastas monitoradas e limpa arquivos de controle de bloqueio usados pelo UniNFe.

## Remover o serviço

Para desinstalar o serviço do Windows:

1. Pare o serviço **UniNFeServico**, se ele estiver em execução.
2. Abra o **Prompt de Comando** como administrador.
3. Acesse a pasta onde o UniNFe está instalado.
4. Execute o comando:

```cmd
UniNFe.Service uninstall
```

Depois disso, reinicie o computador. O UniNFe não será mais executado como serviço.

## Cuidados importantes

- Configure e teste o UniNFe pela interface antes de instalar o serviço.
- Use no serviço o mesmo usuário do Windows que tem acesso ao certificado digital.
- Garanta permissão de leitura e gravação nas pastas de envio, retorno, erro, enviados, backup e demais pastas configuradas.
- Não deixe a interface do UniNFe e o serviço processando arquivos ao mesmo tempo.
- Após alterar configurações pela interface, feche o aplicativo e reinicie o serviço.
- Consulte os logs do UniNFe sempre que o serviço iniciar, parar ou não processar arquivos como esperado.
