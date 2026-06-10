# Atualização do UniNFe

A tela de atualização permite baixar e iniciar a instalação de uma versão mais recente do UniNFe a partir do próprio aplicativo.

## Acesso

No menu principal, clique em **Atualizar o UniNFe**.

Quando há senha de configuração cadastrada e o tempo de acesso anterior expirou, o UniNFe solicita a senha antes de abrir a tela de atualização.

## Como atualizar

1. Clique em **Iniciar a atualização**.
2. Confirme a mensagem informando que, após o download, a aplicação será encerrada para executar o instalador.
3. Aguarde o download do instalador.
4. Ao concluir o download, o UniNFe inicia o instalador e encerra o aplicativo.

Durante o download, a tela exibe uma barra de progresso. O título da janela também indica que a atualização está sendo baixada.

## Botões

| Botão | Para que serve |
|---|---|
| Iniciar a atualização | Inicia o download do instalador da versão mais recente do UniNFe. Antes de continuar, o aplicativo pede confirmação ao usuário. |
| Fechar | Fecha a tela de atualização sem iniciar o download. |

## O que acontece durante a atualização

Antes de executar o instalador, o UniNFe interrompe os serviços do monitor. Em seguida, baixa o instalador da atualização para a pasta de execução do aplicativo e inicia a instalação de forma automática.

Se o aplicativo estiver configurado para usar proxy, a atualização usa as mesmas configurações de proxy cadastradas no UniNFe.

## Mensagens e erros

Ao iniciar a atualização, o UniNFe exibe uma confirmação com o aviso de que a aplicação será encerrada após o download.

Se ocorrer falha no download do instalador, a tela mostra uma mensagem informando que não foi possível baixar o instalador e apresenta o erro retornado. A falha também é registrada no log do UniNFe.

## Cuidados

Antes de iniciar a atualização, conclua operações em andamento no monitor e confirme que a máquina tem acesso à internet. Em ambientes que usam proxy, revise as configurações de proxy do UniNFe antes de atualizar.
