# Tela Log

## Finalidade

A tela **Log** permite consultar os arquivos de log gerados pelo UniNFe. Ela é usada principalmente para suporte, diagnóstico de falhas e conferência de operações executadas pela aplicação.

Os logs ajudam a identificar mensagens de erro, falhas de configuração, problemas de atualização, eventos de processamento e outras ocorrências registradas durante a execução do UniNFe.

## Onde acessar

No menu principal, acesse a opção **Logs**.

## Campos

| Campo | O que faz | Como usar |
|---|---|---|
| Arquivos | Lista os arquivos `.log` existentes na pasta de logs do UniNFe. | Selecione o arquivo que deseja visualizar. |
| Área de visualização | Mostra o conteúdo do arquivo de log selecionado. | Leia as mensagens registradas. Use as barras de rolagem para navegar pelo conteúdo. |

## Como consultar um log

1. Abra a tela **Log**.
2. No campo **Arquivos**, selecione o arquivo de log desejado.
3. Leia o conteúdo exibido na área de visualização.
4. Procure mensagens próximas ao horário do problema investigado.

Ao abrir a tela, o UniNFe tenta selecionar automaticamente o log do dia atual. Se não existir log do dia, a tela seleciona outro arquivo disponível.

## Onde os logs ficam gravados

Os arquivos são lidos da pasta `log`, localizada dentro da pasta de instalação/execução do UniNFe.

O nome dos arquivos de log segue o padrão diário usado pelo UniNFe, como `uninfe_ano-mes-dia.log`.

## Botões e ações

| Ação | O que acontece | Quando usar |
|---|---|---|
| Excluir | Solicita confirmação e remove o arquivo de log selecionado. Depois disso, a lista de arquivos é atualizada. | Use somente quando o arquivo não for mais necessário para suporte, auditoria ou investigação. |

## Mensagens e erros

Se o UniNFe não conseguir ler o arquivo selecionado, a área de visualização mostra a mensagem de erro retornada pelo Windows ou pelo aplicativo. Isso pode ocorrer, por exemplo, se o arquivo tiver sido removido, estiver inacessível ou houver restrição de permissão.

## Cuidados

- Excluir um log remove o histórico daquele arquivo da pasta de logs.
- Antes de excluir, confirme se o arquivo não será necessário para suporte ou análise posterior.
- Se estiver investigando um problema, preserve logs do dia do erro e dos dias próximos.
- A tela exibe o conteúdo do arquivo selecionado em modo somente leitura; alterações no texto exibido não são usadas para alterar o arquivo original.
