# Cadastro de Nova Empresa

A tela **Nova empresa** é aberta a partir da tela **Configurações** e serve para informar os dados iniciais de uma empresa antes de completar as demais configurações.

Depois de confirmar esta tela, o UniNFe retorna para **Configurações > Empresas**, onde as abas da empresa ficam disponíveis para ajustar serviço, ambiente, pastas, certificado digital, FTP, DANFE, responsável técnico, integrações e outras opções.

## Acesso

1. No menu principal, clique em **Configurações**.
2. Acesse a aba **Empresas**.
3. Clique em **Nova**.

## Campos

| Campo | Para que serve |
|---|---|
| Documento | Define o tipo de documento da empresa. As opções disponíveis são **CPF**, **CEI**, **CAEPF** e **CNPJ**. |
| Número | Número do documento selecionado. Ao sair do campo, o UniNFe formata o número conforme o tipo de documento escolhido. |
| Inscrição Estadual | Inscrição estadual vinculada ao CPF. Este campo aparece somente quando o tipo de documento selecionado é **CPF**. |
| Nome | Nome da empresa que será exibido na lista de empresas cadastradas. O preenchimento é obrigatório. |
| Serviço | Serviço fiscal que será monitorado para a empresa. Após confirmar o cadastro, este serviço define quais abas e campos ficam disponíveis na tela de configurações da empresa. |

## Botões

| Botão | Para que serve |
|---|---|
| Ok | Valida os dados iniciais e cria a empresa para continuidade da configuração. |
| Cancelar | Fecha a tela sem criar a empresa. |

## Validações

Antes de criar a empresa, o UniNFe valida o documento informado conforme o tipo selecionado:

- **CPF** deve ser válido;
- **CEI** deve ser válido;
- **CNPJ** deve ser válido;
- **Nome** deve estar preenchido.

O UniNFe também verifica se já existe uma empresa com o mesmo documento configurada para o mesmo serviço. Para os serviços **Todos** e **NF-e**, a verificação considera que ambos compartilham a mesma configuração, impedindo duplicidade entre eles.

## Próximos passos

Após clicar em **Ok**, conclua o cadastro na tela **Configurações**, principalmente nas abas:

- **Principal**, para revisar serviço, ambiente, UF ou município e opções de processamento;
- **Pastas**, para definir os diretórios usados na integração;
- **Certificado digital**, para configurar o certificado da empresa;
- demais abas aplicáveis ao serviço selecionado.
