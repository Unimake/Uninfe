# Tela Configurações

A tela **Configurações** centraliza as opções gerais do UniNFe e o cadastro das empresas monitoradas. Nela são definidos proxy, senha de acesso, atualização automática, empresa, serviço fiscal, ambiente, pastas de trabalho, certificado digital, FTP, impressão, responsável técnico, integrações e outras opções operacionais.

## Acesso

No menu principal, clique em **Configurações**.

Quando há senha de configuração cadastrada e o tempo de acesso anterior expirou, o UniNFe solicita a senha antes de abrir a tela.

## Organização da tela

A tela possui duas áreas principais:

| Aba | Para que serve |
|---|---|
| Geral | Configura opções globais do aplicativo, usadas por todas as empresas. |
| Empresas | Permite incluir, selecionar, alterar, salvar, cancelar e excluir empresas monitoradas pelo UniNFe. A inclusão começa pela tela [Cadastro de Nova Empresa](cadastro-nova-empresa.md). |

Na aba **Empresas**, as configurações da empresa selecionada são separadas em abas internas:

| Aba da empresa | Para que serve |
|---|---|
| Principal | Define identificação da empresa, serviço fiscal, ambiente, UF ou município, tipo de emissão e opções principais de processamento. |
| Pastas | Define as pastas usadas para envio, retorno, erro, validação, enviados, backup, envio em lote e download. |
| Certificado digital | Configura o certificado usado na comunicação com os serviços fiscais. |
| FTP | Configura envio e consulta de arquivos por FTP. |
| DANFE | Configura integração com UniDANFE e DANFE Mon. |
| Responsável Técnico | Configura os dados do responsável técnico e CSRT. |
| Integrações | Configura credenciais e alertas de integrações como e-bank e u-Messenger. |
| Outras Configurações | Configura opções adicionais conforme o serviço da empresa. |

Algumas abas e campos aparecem ou ficam ocultos conforme o serviço selecionado para a empresa.

## Botões principais

| Botão | Para que serve |
|---|---|
| Nova | Abre a tela [Cadastro de Nova Empresa](cadastro-nova-empresa.md), onde são informados documento, nome e serviço inicial da empresa. |
| Salvar | Grava as alterações feitas na empresa ou nas configurações gerais. O botão **Nova** muda para **Salvar** quando há alterações pendentes. |
| Excluir | Remove a empresa selecionada após confirmação. |
| Cancelar | Cancela as alterações pendentes. O botão **Excluir** muda para **Cancelar** quando há alterações pendentes. |
| Voltar | Retorna ao menu anterior. Se houver alterações não salvas, o UniNFe pede confirmação antes de abandonar a tela. |

## Aba Geral

A aba **Geral** reúne opções que valem para o aplicativo como um todo.

| Campo | Para que serve |
|---|---|
| Usar um servidor proxy | Ativa o uso de proxy para comunicações externas do UniNFe, como serviços fiscais e atualização do aplicativo. |
| Servidor | Endereço do servidor proxy. Fica habilitado quando o uso de proxy está ativo e a detecção automática não está marcada. |
| Porta | Porta do servidor proxy. Fica habilitada quando o uso de proxy está ativo e a detecção automática não está marcada. |
| Usuário | Usuário para autenticação no proxy, quando exigido. |
| Senha | Senha do usuário do proxy, quando exigida. |
| Detectar configuração de proxy automaticamente | Faz o UniNFe usar a detecção automática de proxy, desabilitando o preenchimento manual de servidor e porta. |
| Senha de acesso a tela de configurações | Define uma senha para proteger o acesso à tela de configurações. |
| Repetir a senha | Confirma a senha de acesso à tela de configurações. As duas senhas devem ser idênticas. |
| Gravar log das operações realizadas | Ativa o registro das operações executadas pelo UniNFe nos logs. |
| Checar a conexão com a internet ao enviar o XML | Faz o UniNFe verificar a conexão antes de enviar XML aos serviços fiscais. |
| Exibir tela de confirmação ao fechar manualmente o UniNFe | Exibe confirmação quando o usuário tenta fechar o aplicativo manualmente. |
| Manter o UniNFe atualizado | Ativa a verificação automática de atualização do UniNFe. |
| Salvar | Grava as configurações gerais. |

## Aba Empresas

Na aba **Empresas**, o campo **Empresas** lista as empresas cadastradas. Ao selecionar uma empresa, o UniNFe carrega suas configurações nas abas internas.

Ao criar uma empresa nova, o UniNFe abre a tela [Cadastro de Nova Empresa](cadastro-nova-empresa.md) para coletar documento, nome e serviço. Depois da confirmação, as abas da empresa ficam disponíveis para completar a configuração.

## Aba Principal

A aba **Principal** identifica a empresa e define as opções básicas do serviço fiscal.

| Campo | Para que serve |
|---|---|
| Nome | Nome da empresa exibido no UniNFe. É obrigatório. |
| CNPJ/CPF/CEI/CAEPF | Documento da empresa. Em empresas já cadastradas, o campo é exibido como informação da empresa selecionada. |
| Serviço | Serviço fiscal monitorado para a empresa, como NF-e, NFC-e, CT-e, MDF-e, NFS-e, eSocial, EFD-Reinf, NFCom, NF3e, NFGas, CIOT ou outros disponíveis na instalação. |
| Unidade Federativa (UF) | UF usada para o serviço fiscal da empresa. Para NFS-e, o campo é apresentado como **Município**. |
| Município | Município usado para configuração de NFS-e. Ao selecionar o município, o UniNFe preenche o código do município e o padrão identificado. |
| Código do município | Código do município selecionado para NFS-e. |
| Padrão | Padrão de NFS-e identificado para o município selecionado. |
| Ambiente | Define se a empresa opera em ambiente de produção ou homologação, conforme as opções disponíveis no aplicativo. |
| Tipo de emissão | Define o tipo de emissão usado para os documentos fiscais quando o serviço selecionado utiliza essa opção. |
| Formatação pastas | Define o formato de organização das pastas de armazenamento, quando aplicável ao serviço. |
| Quantidade em segundos p/efetuar a consultar da autorização da NFe/NFCe/MDFe | Intervalo usado para consultar a autorização após o envio. O valor deve ficar entre 2 e 15 segundos. |
| Quantos dias devem ser mantidos os arquivos na pasta temporário e retorno? | Define por quantos dias os arquivos permanecem nas pastas temporárias e de retorno antes da limpeza. Use `0` para não aplicar limpeza automática por quantidade de dias. |
| Gravar os retornos dos webservices também no formato texto (TXT) | Gera retorno em TXT além do XML para os retornos dos webservices. |
| Gravar os eventos na mesma pasta dos arquivos de NFe/NFCe/MDFe/CTe autorizados/denegados | Grava eventos junto aos documentos autorizados ou denegados. |
| Gravar os eventos de cancelamento na mesma pasta dos arquivos da NFe/NFCe/MDFe/CTe autorizados/denegados | Grava eventos de cancelamento junto aos documentos correspondentes. |
| Gravar os eventos na consulta de NFe/NFCe/MDFe/CTe de terceiros? | Grava eventos retornados em consultas de documentos de terceiros. |
| Gravar o nome dos XML da NFe/CTe retornados na manifestação no formato com o número do NSU | Usa o NSU no nome dos XML retornados na manifestação, quando a opção está disponível para o serviço. |
| Comparar Digest Value do DFe em processamento com o retornado pela SEFAZ? | Compara o Digest Value do documento em processamento com o valor retornado pela SEFAZ. |
| Gravar avisos na pasta retorno (não impeditivo) | Grava avisos não impeditivos na pasta de retorno. |
| Ativar envio RPS síncrono (THEMA) | Ativa o envio síncrono de RPS para o padrão THEMA, quando disponível. |
| WS-Usuário | Usuário de webservice usado em alguns padrões de NFS-e. |
| WS-Senha | Senha de webservice usada em alguns padrões de NFS-e. |
| ClientID | Identificador de cliente exigido por algumas integrações de NFS-e. |
| Client Secret | Segredo do cliente exigido por algumas integrações de NFS-e. |
| ID Token | Identificador do CSC usado na NFC-e. Quando informado, o CSC também deve ser informado. |
| CSC | Código de Segurança do Contribuinte usado na NFC-e. Quando informado, o ID Token também deve ser informado. |

## Aba Pastas

A aba **Pastas** define onde o UniNFe lê arquivos de entrada e grava retornos, erros e arquivos processados.

| Campo | Para que serve |
|---|---|
| Criar as pastas automaticamente | Permite ao UniNFe criar automaticamente as pastas configuradas quando elas ainda não existem. |
| Pasta onde serão gravados os arquivos XML´s a serem enviados individualmente para os WebServices | Pasta de envio individual. O ERP grava nesta pasta os XML que o UniNFe deve processar individualmente. |
| Pasta onde serão gravados os arquivos XML´s de retorno dos WebServices | Pasta de retorno. O UniNFe grava nela os XML de retorno dos serviços fiscais. |
| Pasta para arquivamento temporário dos XML´s que apresentaram erro na tentativa de envio | Pasta usada para armazenar XML que apresentaram erro durante o processamento. |
| Pasta onde serão gravados os arquivos XML´s a serem somente validados | Pasta usada pela rotina de validação de XML. |
| Pasta onde serão gravados os arquivos XML´s enviados | Pasta onde o UniNFe arquiva arquivos enviados/processados conforme a configuração da empresa. |
| Pasta para Backup dos XML´s enviados | Pasta de backup dos XML enviados/autorizados. Quando não informada para serviços que usam backup, o UniNFe alerta antes de salvar. |
| Pasta onde serão gravados os arquivos XML´s de NF-e a serem enviadas em lote para os WebServices | Pasta de envio em lote, usada nos serviços que trabalham com esse fluxo. |
| Pasta onde serão gravados os arquivos XML´s de NFe baixados da Sefaz e retornos de distribuição | Pasta usada para XML baixados da SEFAZ e retornos de distribuição, quando o serviço possui esse recurso. |

Ao alterar a pasta de envio, o UniNFe pode perguntar se deve redefinir os demais diretórios para manter a mesma estrutura. A tela também impede o uso da mesma pasta em empresas diferentes quando isso poderia causar conflito.

## Aba Certificado Digital

A aba **Certificado digital** configura o certificado usado pela empresa.

| Campo | Para que serve |
|---|---|
| Utilizar certificado | Indica se a empresa usa certificado digital. Para NFS-e, essa opção pode ser alterada na tela; para outros serviços, o uso do certificado segue a necessidade do serviço. |
| Utilizar certificado instalado no Windows? | Usa um certificado instalado no repositório de certificados do Windows. |
| Informações do certificado digital selecionado | Mostra sujeito, validade, dias restantes para vencimento e thumbprint do certificado selecionado. Se o certificado instalado não for detectado na estação, a tela mostra um alerta. |
| Local de armazenamento do Certificado Digital | Caminho do arquivo do certificado quando a opção de certificado instalado no Windows não está marcada. |
| Senha do Certificado | Senha do arquivo de certificado. |
| PIN Certificado A3 (opcional) | PIN do certificado A3, exibido quando o certificado selecionado é A3. |
| Testar PIN | Testa o PIN informado para certificado A3. |

Ao salvar, o UniNFe valida o certificado configurado. Se o CNPJ ou CPF do certificado for diferente do documento da empresa, a tela exibe um aviso para o usuário.

## Aba FTP

A aba **FTP** configura a conexão FTP usada pela empresa.

| Campo | Para que serve |
|---|---|
| Ativo? | Ativa o uso de FTP para a empresa. |
| Nome do servidor | Endereço do servidor FTP. |
| Porta | Porta de conexão do FTP. |
| Nome do usuário | Usuário para autenticação no FTP. |
| Senha | Senha do usuário FTP. |
| Modo passivo? | Ativa o modo passivo na conexão FTP. |
| Pasta onde serão gravados os arquivos XML´s enviados | Pasta de destino dos XML autorizados/enviados no FTP. |
| Pasta onde serão gravados os arquivos XML´s de retorno dos WebServices | Pasta de retornos no FTP. |
| Gravar os XML's autorizados em uma única pasta | Grava os XML autorizados em uma pasta única. Quando desmarcado, o UniNFe organiza os XML em pastas conforme a estrutura do serviço. |
| Testar | Testa a conexão FTP e verifica as pastas configuradas. Se uma pasta não existir, o UniNFe pode perguntar se deve criá-la. |

Se o teste de FTP acusar erro e o FTP estiver ativo, o UniNFe impede salvar enquanto o problema não for corrigido.

## Aba DANFE

A aba **DANFE** configura integração com impressão e ferramentas relacionadas ao UniDANFE e DANFE Mon.

| Campo | Para que serve |
|---|---|
| Pasta do executável do UniDANFE | Caminho onde está o executável do UniDANFE. |
| Pasta do arquivo de configurações do UniDANFE | Caminho onde ficam os arquivos de configuração do UniDANFE. |
| Pasta onde deve ser gravado o XML para a impressão do documento fiscal a partir do DANFE Mon | Pasta monitorada pelo DANFE Mon para impressão. |
| Nomes das configurações - NFe/MDFe/CTe/NFCe | Nome da configuração de impressão usada para documentos fiscais. |
| Nomes das configurações - CCe | Nome da configuração de impressão usada para Carta de Correção eletrônica. |
| XML´s a serem copiados na pasta para impressão do DANFE a partir do DANFE Mon - XML do documento fiscal eletrônico | Copia o XML do documento fiscal para a pasta usada pelo DANFE Mon. |
| XML´s a serem copiados na pasta para impressão do DANFE a partir do DANFE Mon - XML de distribuição do documento fiscal eletrônico | Copia o XML de distribuição para a pasta usada pelo DANFE Mon. |
| E-mail | E-mail adicional a ser usado nas informações do DANFE. |
| Adiciona este e-mail ao que constar no XML | Quando marcado, acrescenta o e-mail configurado ao e-mail já existente no XML. |

## Aba Responsável Técnico

A aba **Responsável Técnico** registra os dados do responsável técnico da empresa.

| Campo | Para que serve |
|---|---|
| CNPJ | CNPJ do responsável técnico. |
| Contato | Nome do contato do responsável técnico. |
| E-mail | E-mail do responsável técnico. A tela valida o formato do e-mail ao sair do campo. |
| Telefone | Telefone do responsável técnico. |
| ID CSRT | Identificador do CSRT. Quando informado, deve ser maior que zero e é gravado com dois dígitos. |
| CSRT | Código de Segurança do Responsável Técnico. |

Se qualquer um dos campos obrigatórios do responsável técnico for preenchido, os campos **CNPJ**, **Contato**, **E-mail** e **Telefone** devem ser preenchidos em conjunto.

## Aba Integrações

A aba **Integrações** configura credenciais e alertas externos.

| Campo | Para que serve |
|---|---|
| e-bank - AppID | AppID da integração e-bank. O valor deve ter no máximo 200 caracteres. |
| e-bank - Secret | Secret da integração e-bank. O valor deve ter no máximo 200 caracteres. |
| u-Messenger - AppID | AppID da integração u-Messenger. |
| u-Messenger - Secret | Secret da integração u-Messenger. |
| Utilizar oAppID e Secret do Ebank para o uMessenger | Usa as mesmas credenciais do e-bank para o u-Messenger e desabilita o preenchimento separado de AppID e Secret do u-Messenger. |
| Enviar alerta de documentos rejeitados para WhatsApp? | Envia alerta via WhatsApp quando houver documentos rejeitados. |
| Enviar alerta de documentos denegados para WhatsApp? | Envia alerta via WhatsApp quando houver documentos denegados. |
| Enviar alerta de erros ocorridos no UniNFe para WhatsApp? | Envia alerta via WhatsApp quando ocorrerem erros no UniNFe. |
| Numero para envio | Número de WhatsApp para envio dos alertas. O campo aparece quando algum alerta do u-Messenger está marcado. |

## Aba Outras Configurações

A aba **Outras Configurações** exibe opções adicionais conforme o serviço da empresa.

| Campo | Para que serve |
|---|---|
| Salvar na pasta autorizados somente o XML de distribuição? | Define se, na pasta de autorizados, será salvo somente o XML de distribuição. |
| Intervalo de tempo (em segundos) entre o envio de cada arquivo de NFSe | Intervalo entre envios de arquivos de NFS-e. Aparece para empresas configuradas com NFS-e. |
| Versão do QrCode da NFCe (Deixe 0 para o padrão) | Versão do QR Code da NFC-e. Use `0` para manter o padrão do UniNFe. |
| Ativar preparação de TLS antes do envio do XML? | Ativa uma preparação de TLS antes do envio do XML. Use somente quando houver problemas de conexão TLS. |

## Salvamento e validações

Ao salvar, o UniNFe valida as configurações da empresa e das abas aplicáveis ao serviço selecionado. Entre as validações executadas estão:

- nome da empresa obrigatório;
- UF ou município obrigatório quando o serviço exige;
- tempo de consulta de autorização entre 2 e 15 segundos;
- par **ID Token** e **CSC** preenchido em conjunto para NFC-e;
- senha de acesso à tela de configurações confirmada corretamente;
- dados de proxy completos quando o proxy manual está ativo;
- certificado digital válido quando configurado;
- dados parciais do responsável técnico completados antes de salvar;
- correção de erros encontrados no teste de FTP quando o FTP está ativo.

Quando a configuração de limpeza automática é alterada, o UniNFe alerta que arquivos das pastas temporárias e de retorno poderão ser apagados conforme os dias configurados. Revise as pastas antes de confirmar essa opção.
