# Passo a passo para preparar o CNPJ para uso das integrações CIOT/ANTT

> **Objetivo deste material**  
> Orientar, de forma simples e prática, o que o cliente usuário do software deve verificar, providenciar e configurar para conseguir utilizar corretamente os serviços relacionados ao CIOT — Código Identificador da Operação de Transporte.
>
> Este material diferencia dois caminhos possíveis:
>
> 1. **integração direta com o Web Service da ANTT**, quando a regulamentação permitir;  
> 2. **geração por Instituição de Pagamento autorizada**, quando a operação envolver TAC, TAC equiparado ou outro cenário em que a regra exigir esse caminho.
>
> **Público-alvo**  
> Empresas transportadoras, usuários de ERP/TMS e equipes administrativas que precisam deixar o CNPJ, o RNTRC, a frota, o certificado digital e o software aptos para geração, consulta, cancelamento ou encerramento de CIOT por integração sistêmica.
>
> **Data de elaboração:** 26/05/2026  
> **Última revisão:** 23/11/2026

---

## 1. Antes de começar: entenda qual é o seu caso

Nem toda empresa deve seguir o mesmo caminho para gerar CIOT.

Antes de falar em “habilitar CNPJ”, o cliente precisa identificar em qual cenário se encaixa.

| Cenário | Pode usar integração direta com a ANTT? | Caminho recomendado |
|---|---:|---|
| **ETC — Empresa de Transporte Rodoviário de Cargas, não equiparada a TAC, com mais de 3 veículos no RNTRC, realizando a operação diretamente e sem subcontratar TAC/TAC equiparado** | Sim, em regra pode usar integração com Web Service da ANTT ou Instituição de Pagamento autorizada. | Validar RNTRC, frota vinculada, certificado digital e integração no software. |
| **Operação com contratação ou subcontratação de TAC ou TAC equiparado** | Não para geração direta dessa relação pela transportadora. | Gerar CIOT por Instituição de Pagamento autorizada pela ANTT. |
| **ETC que subcontrata outra ETC não equiparada a TAC, e essa ETC subcontratada efetivamente realiza o transporte** | A responsabilidade pelo registro pode recair sobre a ETC que efetivamente realizará a operação, observadas as regras de cadastramento. | Validar a ETC executora, RNTRC, frota, certificado digital e provedor CIOT adequado. |
| **CTC ou ETC equiparada a TAC, quando contratada ou quando houver delegação operacional permitida** | Não como fluxo direto comum pela ANTT. | Geração por Instituição de Pagamento autorizada. |
| **Empresa com frota própria, mas transportando carga de terceiros mediante remuneração** | Pode, se for ETC apta e a operação for realizada diretamente sem TAC/TAC equiparado. | Validar se é transporte rodoviário remunerado de cargas, RNTRC, frota e regra operacional. |
| **Carga própria, sem prestação de serviço a terceiros e sem pagamento de frete** | Não é o cenário típico de CIOT. | Não ativar CIOT sem necessidade regulatória. |
| **Operações expressamente excepcionadas pela Portaria SUROC nº 6/2026** | Não, se a operação estiver dentro de hipótese de não aplicação. | Validar a exceção antes de gerar CIOT. |
| **Instituição de Pagamento que quer se tornar geradora oficial de CIOT** | Sim, mas é outro processo. | Solicitar autorização formal à ANTT pelo fluxo aplicável. |

---

## 2. Ponto mais importante para o cliente final

Para uma **ETC** já cadastrada corretamente na ANTT, a regra prática é:

> As ETCs já possuem cadastro na ANTT para utilização dos serviços do CIOT.  
> Para usar a integração direta, é necessário possuir **certificado digital válido**, conforme o DCS da ANTT, manter o cadastro/RNTRC/frota regularizados e operar dentro de um cenário em que a geração direta seja permitida.

Ou seja: para a transportadora comum, normalmente não existe uma “tela de habilitação de CNPJ” igual ocorre em alguns ambientes fiscais.

O que precisa existir é:

1. CNPJ ativo;
2. RNTRC ativo e regular;
3. frota vinculada corretamente ao RNTRC do transportador que efetivamente realizará a operação;
4. certificado digital válido do CNPJ correto;
5. software configurado para usar esse certificado na comunicação com a ANTT ou com a Instituição de Pagamento autorizada;
6. operação de transporte dentro das regras permitidas para o caminho escolhido;
7. controle de geração, consulta, cancelamento, retificação, contingência e encerramento conforme o tipo de operação.

---


## 2.1 Informação operacional importante sobre geração do identificador da operação

Na integração direta com a ANTT existe o conceito de `IdOperacaoTransporte`.

A documentação técnica da ANTT prevê que o identificador da operação seja gerado conforme padrão definido pela própria ANTT, utilizando a biblioteca ou o executável disponibilizado oficialmente.

As opções disponibilizadas são:

1. `GeradorCIOTShared.dll`
2. `GeradorCIOT.exe`

Em termos práticos:

- informa-se o CNPJ do contratado/transportador aplicável ao cenário;
- o componente gera o identificador da operação conforme o padrão da ANTT;
- esse identificador é usado no fluxo sistêmico de declaração/consulta da operação;
- o CIOT efetivo, o código verificador, o protocolo e as mensagens de retorno devem ser obtidos e gravados a partir dos retornos oficiais do Web Service ou da Instituição de Pagamento autorizada.

> **Atenção:** a DLL/EXE não substitui a transmissão da operação, as validações, os retornos oficiais, o protocolo, o código verificador nem o armazenamento da resposta da ANTT ou da fonte autorizada.

### Importante para empresas com sistemas legados

Sistemas antigos podem não conseguir consumir DLL .NET diretamente.

Exemplos comuns:

- Visual FoxPro;
- xHarbour/Harbour;
- Delphi legado;
- integrações nativas Win32.

Nestes casos, o `GeradorCIOT.exe` costuma ser a alternativa operacional mais viável, desde que o software continue realizando corretamente a declaração, consulta, persistência e tratamento dos retornos oficiais.

---

## 3. Passo a passo resumido

```text
1. Confirmar se existe transporte rodoviário remunerado de cargas
        ↓
2. Identificar quem efetivamente realizará a operação: ETC, CTC, TAC ou TAC equiparado
        ↓
3. Verificar se o caminho correto é ANTT direta ou Instituição de Pagamento autorizada
        ↓
4. Verificar se o CNPJ possui RNTRC ativo
        ↓
5. Conferir se os veículos estão vinculados ao RNTRC correto
        ↓
6. Providenciar certificado digital ICP-Brasil válido do CNPJ correto
        ↓
7. Entregar/configurar o certificado em ambiente seguro
        ↓
8. Configurar o módulo CIOT no software
        ↓
9. Fazer teste em homologação
        ↓
10. Conferir retornos da ANTT ou da Instituição de Pagamento autorizada
        ↓
11. Liberar produção
        ↓
12. Gerar o CIOT antes do início da operação de transporte
        ↓
13. Vincular o CIOT ao MDF-e, quando houver MDF-e aplicável
        ↓
14. Encerrar, cancelar, retificar ou tratar contingência conforme o tipo de operação
```

---

# 4. Passo 1 — Verificar se a empresa realmente precisa gerar CIOT

O cliente deve responder às perguntas abaixo.

## 4.1 A empresa realiza transporte rodoviário remunerado de cargas?

Pergunta simples:

> A empresa recebe para transportar carga de terceiros?

Se **sim**, existe necessidade de avaliar a geração do CIOT, observadas as exceções previstas na regulamentação.

Se **não**, e a empresa apenas transporta carga própria, sem prestação de serviço de transporte a terceiros e sem pagamento de frete, em regra não é o cenário típico de geração de CIOT.

## 4.2 A empresa é transportadora registrada no RNTRC?

Verificar se a empresa está enquadrada em uma destas categorias:

| Sigla | Descrição |
|---|---|
| **ETC** | Empresa de Transporte Rodoviário de Cargas |
| **CTC** | Cooperativa de Transporte Rodoviário de Cargas |
| **TAC** | Transportador Autônomo de Cargas |

Para uso direto pela empresa jurídica, o cenário mais comum será **ETC não equiparada a TAC**, realizando a operação com frota vinculada ao seu RNTRC.

## 4.3 A operação envolve TAC ou TAC equiparado?

Se a operação envolver contratação ou subcontratação de:

- TAC;
- TAC equiparado;
- ETC equiparada a TAC;
- CTC em cenário equiparado ao TAC conforme regra vigente;

então a geração do CIOT deve seguir o fluxo por **Instituição de Pagamento autorizada pela ANTT**, e não simplesmente pela integração direta da transportadora com a ANTT.

## 4.4 A operação está em alguma exceção de não obrigatoriedade?

Antes de ativar o fluxo, confirme se a operação não está em uma das hipóteses de não aplicação previstas na Portaria SUROC nº 6/2026, como:

- prestação de serviço de transporte de veículo novo não emplacado quando o próprio veículo for o meio de transporte, no autotransporte;
- prestação de serviço de transporte de cargas especiais realizada por composição veicular não homologada pela Senatran;
- prestação de serviço de transporte rodoviário internacional de cargas nos termos da regulamentação específica.

Se houver dúvida, valide a operação com base na norma vigente e na orientação oficial da ANTT antes de gerar ou dispensar o CIOT.

---

# 5. Passo 2 — Consultar a situação do RNTRC

O cliente deve consultar se o CNPJ está com RNTRC ativo.

## 5.1 Onde consultar

A consulta pública da ANTT permite consultar dados do RNTRC por transportador, localidade ou veículo.

O cliente deve acessar a consulta pública da ANTT e verificar:

- se o CNPJ aparece como transportador;
- se a categoria está correta;
- se a situação está ativa;
- se o número do RNTRC está correto;
- se não há pendência cadastral.

## 5.2 O que validar

| Item | O que conferir |
|---|---|
| CNPJ | Deve ser o mesmo CNPJ que será identificado na operação e, quando aplicável, no certificado digital. |
| RNTRC | Deve estar ativo. |
| Categoria | ETC, CTC ou TAC, conforme o caso. |
| Situação cadastral | Não pode estar suspensa, pendente, cancelada ou irregular. |
| Responsável técnico | Deve estar regular, quando aplicável. |
| CNAE | Deve estar compatível com transporte rodoviário de cargas, quando aplicável. |
| Equiparação a TAC | Deve ser observada para definir se a operação pode seguir pela ANTT direta ou se deve passar por Instituição de Pagamento autorizada. |

## 5.3 Resultado esperado

O cliente deve conseguir responder:

```text
Meu CNPJ está ativo no RNTRC?
Minha categoria está correta?
Meu RNTRC está regular?
Sou ETC não equiparada a TAC ou estou em cenário que exige Instituição de Pagamento?
```

Se a resposta for **não** ou **não sei** para qualquer uma dessas perguntas, regularize ou confirme o RNTRC antes de tentar usar a integração CIOT.

---

# 6. Passo 3 — Conferir a frota vinculada ao RNTRC

A ANTT valida se os veículos informados na operação pertencem ou estão vinculados ao transportador/RNTRC informado.

Por isso, antes de testar a API, o cliente deve conferir a frota.

## 6.1 O que verificar

Para cada veículo usado nas operações:

- placa;
- RENAVAM, quando aplicável ao cadastro;
- situação do veículo;
- se o veículo automotor está vinculado ao RNTRC correto;
- se os implementos necessários estão corretamente vinculados, quando houver composição veicular;
- se o veículo está autorizado para transporte remunerado de cargas;
- se a quantidade de veículos no RNTRC está compatível com o cenário da empresa;
- se o veículo que efetivamente realizará a operação está vinculado ao transportador declarado.

## 6.2 Erro comum

Um erro comum é o cliente tentar gerar CIOT usando um veículo que:

- está cadastrado em outro CNPJ;
- não está vinculado ao RNTRC da empresa que realizará a operação;
- está com cadastro irregular;
- foi vendido, mas ainda consta na base de outro transportador;
- está com dado divergente no cadastro;
- pertence ao CPF do sócio/motorista, mas não está regularmente vinculado à ETC que realizará a operação.

## 6.3 Resultado esperado

O cliente deve conseguir responder:

```text
As placas que uso no transporte estão vinculadas ao RNTRC do transportador que efetivamente realizará a operação?
```

Se a resposta for **não**, regularize a frota no RNTRC antes dos testes.

---

# 7. Passo 4 — Providenciar certificado digital válido

A integração com os serviços da ANTT usa comunicação segura com certificado digital.

## 7.1 Tipo de certificado

O DCS da ANTT prevê certificado digital ICP-Brasil do tipo **A1 ou A3**.

Para integração automática em servidor, o certificado mais prático costuma ser:

```text
e-CNPJ A1
```

O A3 também pode ser aceito, mas costuma exigir cuidados operacionais adicionais por depender de dispositivo físico, senha e disponibilidade local.

## 7.2 Requisitos importantes do certificado

O certificado deve:

- ser emitido por Autoridade Certificadora credenciada na ICP-Brasil;
- conter o **CNPJ correto** para o cenário de integração;
- estar dentro do prazo de validade;
- permitir autenticação de cliente;
- estar disponível no ambiente onde o software fará a chamada;
- ser protegido por senha e armazenado com segurança;
- atender aos requisitos técnicos exigidos no DCS vigente.

## 7.3 Atenção ao CNPJ do certificado

O certificado usado na comunicação deve corresponder ao CNPJ autorizado/identificado para a operação, conforme validação da ANTT ou da Instituição de Pagamento autorizada.

Regra prática:

> Não use certificado da contabilidade, da software house, de outra filial ou de outro CNPJ se a operação será declarada em nome da transportadora.

Se o certificado não corresponder ao CNPJ esperado, a requisição poderá ser rejeitada por falta de permissão ou inconsistência cadastral.

---

# 8. Passo 5 — Definir onde o certificado será instalado

O cliente deve alinhar com a software house ou equipe de TI onde o certificado será usado.

## 8.1 Possíveis locais

| Local | Quando usar |
|---|---|
| Servidor local do cliente | Quando o software roda dentro da empresa. |
| Servidor em nuvem da empresa | Quando o ERP/TMS roda em ambiente cloud próprio. |
| Ambiente seguro da software house | Apenas se houver contrato, autorização formal e política de segurança clara. |

## 8.2 Cuidados de segurança

Nunca envie certificado digital e senha por canais inseguros.

Evite:

- WhatsApp;
- e-mail comum;
- chat sem criptografia corporativa;
- armazenamento em pasta pública;
- compartilhamento com usuários sem necessidade.

Recomendado:

- usar cofre de senhas;
- limitar acesso;
- registrar quem instalou;
- registrar onde o certificado está instalado;
- controlar vencimento;
- planejar renovação antes da expiração.

---

# 9. Passo 6 — Configurar o módulo CIOT no software

Depois que o RNTRC, a frota e o certificado estiverem prontos, configure o software.

## 9.1 Dados mínimos da empresa

O cliente deve informar/conferir no cadastro da empresa:

| Campo | Observação |
|---|---|
| CNPJ | Deve bater com o certificado e com o RNTRC, quando aplicável ao cenário. |
| Razão social | Deve estar coerente com cadastro oficial. |
| RNTRC | Informar corretamente, sem pontuação indevida. |
| Categoria | ETC, CTC, TAC, conforme cadastro. |
| Equiparado a TAC | Informação crítica para definir o caminho de geração. |
| Ambiente | Homologação ou produção. |
| Provedor CIOT | ANTT direta ou Instituição de Pagamento autorizada. |
| Certificado digital | Selecionar o certificado correto. |
| Forma de geração do identificador | Biblioteca ou executável oficial da ANTT, quando aplicável à integração direta. |

## 9.2 Dados operacionais necessários

O cliente também deve ter cadastros confiáveis de:

- contratante;
- contratado;
- subcontratante/subcontratado, quando houver;
- transportador;
- RNTRC;
- motorista, quando exigido pelo fluxo/provedor;
- veículos;
- implementos;
- origem;
- destino;
- distância;
- tipo de operação: carga lotação, carga fracionada ou TAC-Agregado;
- tipo/natureza da carga;
- quantidade e peso da carga;
- valor do frete;
- forma de pagamento;
- datas de início e fim;
- dados bancários, Pix ou forma de pagamento, quando exigidos;
- dados fiscais relacionados ao MDF-e/CT-e/NF-e/NFS-e, quando aplicável.

---

# 10. Passo 7 — Testar a comunicação em homologação

Antes de usar em produção, execute testes no ambiente de homologação.

## 10.1 O que testar primeiro

A ordem segura é:

1. testar se o certificado é aceito;
2. consultar situação do transportador;
3. consultar frota do transportador;
4. gerar o identificador da operação pelo componente oficial, quando aplicável;
5. declarar uma operação de transporte de teste;
6. consultar o CIOT gerado;
7. testar cancelamento, retificação ou encerramento, conforme o tipo de operação permitir;
8. validar logs, protocolo, código verificador, mensagens de retorno e eventuais avisos ao transportador.

## 10.2 Resultado esperado

O sistema deve conseguir:

- conectar na ANTT ou na Instituição de Pagamento autorizada;
- autenticar com certificado ou credencial exigida pelo provedor;
- enviar JSON válido no caso da integração direta ANTT;
- receber resposta;
- gravar protocolo;
- gravar código do CIOT;
- gravar código verificador;
- gravar mensagens de retorno;
- tratar avisos obrigatórios ao transportador, quando retornados;
- manter trilha de auditoria.

> **Atenção:** dados e protocolos retornados em homologação não devem ser usados como operação oficial.

---

# 11. Passo 8 — Corrigir inconsistências encontradas na homologação

Durante os testes, podem aparecer rejeições.

## 11.1 Rejeições comuns

| Rejeição ou problema | Causa provável | Como resolver |
|---|---|---|
| Certificado não aceito | Certificado vencido, inválido, sem autenticação cliente ou CNPJ divergente. | Conferir certificado e instalar o correto. |
| CPF/CNPJ do certificado não corresponde ao transportador ou interessado | O certificado usado não pertence ao CNPJ esperado para a operação. | Usar certificado do CNPJ correto e conferir RNTRC/cadastro. |
| RNTRC não encontrado ou inativo | Cadastro RNTRC irregular. | Regularizar no RNTRC Digital/ANTT. |
| CPF/CNPJ e RNTRC não correspondem ao mesmo transportador | Dados cadastrais divergentes. | Corrigir cadastro do transportador e RNTRC. |
| Placa não vinculada ao RNTRC | Veículo não pertence ou não está vinculado ao transportador informado. | Corrigir cadastro de frota. |
| Placa duplicada na composição | A mesma placa foi enviada mais de uma vez. | Corrigir lista de veículos/implementos. |
| Operação bloqueada por pendência | Existe CIOT pendente, penalidade, bloqueio ou inconsistência. | Consultar pendências e regularizar. |
| Valor do frete inválido | Valor zerado ou abaixo do piso mínimo quando a operação estiver sujeita ao piso. | Corrigir cálculo do frete/piso e validar tipo de operação. |
| Data de início inválida | Data de início anterior à declaração ou fora das tolerâncias previstas. | Corrigir datas e horário oficial de Brasília. |
| Retificação não permitida | O tipo de operação não admite retificação naquele momento. | Verificar regras por tipo de operação. |
| Dados obrigatórios ausentes | Payload incompleto para o tipo de operação. | Completar cadastro e regras do software. |
| AvisoTransportador ignorado | Retorno com mensagem obrigatória não foi apresentado ou impresso. | Ajustar o software para exibir/registrar o aviso conforme DCS. |

---

# 12. Passo 9 — Liberar ambiente de produção

Depois que os testes forem aprovados, configure produção.

## 12.1 Antes de ativar produção

Verifique:

- [ ] RNTRC ativo;
- [ ] frota regular;
- [ ] cenário de geração direta ou via Instituição de Pagamento definido;
- [ ] certificado válido;
- [ ] certificado instalado no ambiente correto;
- [ ] senha do certificado protegida;
- [ ] ambiente de produção configurado;
- [ ] componente oficial de geração do identificador configurado, quando aplicável;
- [ ] logs habilitados;
- [ ] usuários treinados;
- [ ] fluxo de cancelamento, retificação, contingência e encerramento definido;
- [ ] rotina para renovar certificado antes do vencimento.

## 12.2 Não misture ambientes

Nunca use dados reais em homologação como se fossem operação oficial.

Nunca use configuração de homologação em operação real.

Os endereços, certificados, URLs e credenciais devem estar separados por ambiente.

---

# 13. Passo 10 — Gerar o CIOT antes do início da operação

O CIOT deve ser gerado previamente ao início da operação de transporte.

Fluxo recomendado:

```text
1. Cadastrar viagem/operação no ERP/TMS
2. Conferir dados do contratante/contratado/subcontratado
3. Conferir RNTRC e veículos
4. Classificar o tipo de operação: carga lotação, carga fracionada ou TAC-Agregado
5. Calcular/validar valor do frete e piso mínimo, quando aplicável
6. Gerar o identificador da operação, quando aplicável à integração direta
7. Declarar a operação e obter retorno oficial
8. Gravar CIOT, código verificador, protocolo e mensagens
9. Gerar MDF-e com o CIOT vinculado, quando houver MDF-e aplicável
10. Iniciar a operação
```

O sistema não deve “inventar” o CIOT. O código deve ser retorno oficial da ANTT ou da fonte autorizada.

---

# 14. Passo 11 — Vincular o CIOT ao MDF-e, quando aplicável

Quando houver MDF-e aplicável à operação, o CIOT deve ser vinculado ao MDF-e correspondente.

O [Ajuste SINIEF 03/26](https://www.gov.br/fazenda/pt-br/composicao/orgaos/orgaos-colegiados/crsnsp/servicos/busca-de-atas-pautas-e-decisoes/pautas-das-sessoes-de-julgamentos/pauta-336-dou.pdf) produz efeitos desde **01/06/2026**. A [NT MDF-e 2026.001 v1.00](https://www.cte.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=F9WDPRCi0Bs%3D) prevê homologação em **21/09/2026** e produção em **23/11/2026** para a rejeição **684** (ausência de `infCIOT`). No modal rodoviário (`modal=1`), a regra verifica `tpEmit=1`, `tpEmit=3` ou `tpEmit=2` com `tpTransp` informado. Em carga própria legítima, não invente CIOT nem altere as tags para contornar a regra: confira se o enquadramento e os dados do XML estão corretos. A data técnica da rejeição não é a data de início da obrigação da ANTT.

No [leiaute do MDF-e](https://moc.sped.fazenda.pr.gov.br/MDFe/Leiaute.html), informe o **CIOT de 12 caracteres** em `infANTT/infCIOT/CIOT` e o CPF ou CNPJ de quem foi responsável pela geração. Não use o `Protocolo` nem concatene o código verificador nesse campo. Guarde o verificador: no DCS PEF ele integra o identificador de 16 caracteres exigido por operações como cancelamento, retificação e encerramento.

No XML do MDF-e rodoviário, o CIOT fica no grupo:

```text
infModal/rodo/infANTT/infCIOT
```

Exemplo simplificado:

```xml
<infCIOT>
  <CIOT>123456789012</CIOT>
  <CNPJ>12345678000195</CNPJ>
</infCIOT>
```

Onde:

| Campo | Significado |
|---|---|
| `CIOT` | Código gerado/retornado pela ANTT ou fonte autorizada. |
| `CNPJ` ou `CPF` | Responsável pela geração do CIOT, conforme o leiaute e o cenário da operação. |

O CIOT não substitui MDF-e, CT-e, nota fiscal, NFS-e, contrato ou comprovantes de pagamento. Ele é um registro regulatório da operação perante a ANTT.

---

# 15. Passo 12 — Encerrar, cancelar, retificar ou tratar o CIOT após a operação

Gerar o CIOT não encerra o fluxo.

Dependendo do tipo de operação, o cliente pode precisar:

- cancelar o CIOT, se a operação não acontecer;
- encerrar o CIOT, quando a operação for concluída;
- consultar o CIOT;
- tratar pendências;
- emitir novo CIOT se a operação mudar e a regra exigir;
- retificar, quando o tipo de operação e o prazo permitirem;
- transmitir informações geradas em contingência dentro do prazo regulamentar.

## 15.1 Quando cancelar

Use cancelamento quando a operação declarada não será realizada.

Como regra operacional, o cancelamento deve respeitar o prazo e as restrições aplicáveis ao tipo de operação. Para carga lotação e carga fracionada, a regra operacional vigente prevê cancelamento até 24 horas antes da data de início da operação.

## 15.2 Quando encerrar

Use encerramento quando a operação foi concluída.

Para carga lotação e carga fracionada, o encerramento deve ocorrer até 5 dias contados da data prevista para o término da operação, podendo haver encerramento automático caso essa providência não seja adotada no prazo.

Para TAC-Agregado, observar as regras específicas de prazo mínimo, prazo máximo, pendência e encerramento previstas na Portaria e no DCS.

## 15.3 Quando retificar

A retificação depende do tipo de operação:

| Tipo de operação | Orientação operacional |
|---|---|
| **Carga lotação** | Em regra, não admite retificação após a geração do CIOT. Alterações relevantes podem exigir cancelamento, se ainda possível, ou novo cadastro conforme a regra aplicável. |
| **Carga fracionada** | Pode admitir retificação enquanto a operação não estiver encerrada, observadas as validações do sistema. |
| **TAC-Agregado** | Pode admitir retificação dentro dos prazos específicos previstos para esse tipo de operação. |

## 15.4 Quando emitir novo CIOT

A eventual mudança de informação do veículo enseja o cadastramento de uma nova operação de transporte.

Também pode ser necessário novo CIOT quando houver mudança relevante que descaracterize a operação originalmente declarada.

## 15.5 Contingência

A geração de CIOT em contingência deve ser tratada como exceção, apenas em situações de problema técnico de comunicação ou processamento que impeçam a autorização em tempo real.

A transmissão das informações de contingência deve observar o prazo máximo regulamentar e não afasta a responsabilidade pela veracidade, integridade e autenticidade das informações prestadas.

## 15.6 Por que isso importa

CIOT pendente, divergente ou mal encerrado pode causar:

- bloqueio de novas operações;
- inconsistência em fiscalização;
- divergência com MDF-e;
- problemas com transportador;
- necessidade de suporte manual;
- risco de autuação.

---

## 15.7 Prazo de quitação do frete

A [Lei nº 15.485/2026](https://www.planalto.gov.br/ccivil_03/_ato2023-2026/2026/lei/l15485.htm) estabelece **até 30 dias úteis para quitar o frete**. O [Comunicado Geral SUROC nº 1/2026](https://anttlegis.antt.gov.br/action/ActionDatalegis.php?acao=abrirTextoAto&cod_menu=9230&cod_modulo=623&numeroAto=00000001&orgao=SUROC%2FANTT%2FMT&seqAto=000&tipo=COM&valorAno=2026) esclarece: contratações novas desde **06/08/2026**; operações sob contratos anteriores a partir de **05/11/2026**, ressalvadas operações já concluídas e direitos adquiridos. A regra alcança TAC, ETC e CTC e vale também na subcontratação, para cada contratante em relação à sua obrigação. A ANTT informou que as funcionalidades específicas do novo modelo de CIOT para forma e prazo de quitação ainda não estavam disponíveis. Até ato próprio, mantenha o fluxo vigente, controle o prazo no ERP/TMS e guarde contratos, documentos fiscais e comprovantes de liquidação. Não confunda os campos de pagamento já existentes no DCS com a implantação do novo modelo.

---

# 16. Checklist final para o cliente

Use este checklist antes de chamar a software house dizendo que está pronto para usar CIOT.

## 16.1 Cadastro ANTT/RNTRC

- [ ] CNPJ ativo.
- [ ] RNTRC ativo.
- [ ] Categoria correta: ETC, CTC ou TAC.
- [ ] Verificação se a empresa é ou não equiparada a TAC.
- [ ] Frota vinculada corretamente.
- [ ] Placas conferidas.
- [ ] Sem pendências cadastrais conhecidas.
- [ ] Responsável técnico regular, quando aplicável.

## 16.2 Certificado digital

- [ ] Certificado ICP-Brasil válido.
- [ ] Tipo A1 ou A3, conforme aceito pelo DCS/provedor.
- [ ] Certificado pertence ao CNPJ correto.
- [ ] Certificado não está vencido.
- [ ] Senha disponível para instalação/configuração.
- [ ] Certificado instalado em local seguro.
- [ ] TI sabe onde o certificado está instalado.
- [ ] Existe controle de renovação.

## 16.3 Software

- [ ] Módulo CIOT habilitado no sistema.
- [ ] Provedor CIOT definido: ANTT direta ou Instituição de Pagamento autorizada.
- [ ] Ambiente de homologação testado.
- [ ] Ambiente de produção configurado.
- [ ] Biblioteca/EXE oficial configurado, quando aplicável.
- [ ] Logs de integração habilitados.
- [ ] Usuários treinados para gerar, consultar, cancelar, retificar e encerrar CIOT.
- [ ] Fluxo de suporte definido.
- [ ] Tratamento de retorno, código verificador, protocolo e AvisoTransportador implementado, quando aplicável.

## 16.4 Operação

- [ ] Contratante cadastrado.
- [ ] Contratado cadastrado.
- [ ] Subcontratante/subcontratado cadastrado, quando houver.
- [ ] Transportador/RNTRC cadastrado.
- [ ] Veículos cadastrados e vinculados.
- [ ] Origem/destino definidos.
- [ ] Distância definida.
- [ ] Tipo de operação definido: carga lotação, carga fracionada ou TAC-Agregado.
- [ ] Tipo/natureza de carga definido.
- [ ] Valor do frete calculado.
- [ ] Piso mínimo validado, quando aplicável.
- [ ] Forma de pagamento definida.
- [ ] CIOT gerado antes do início da operação.
- [ ] CIOT vinculado ao MDF-e, quando houver MDF-e aplicável.
- [ ] Fluxo pós-operação definido: cancelamento, retificação, encerramento ou contingência.

---

# 17. Roteiro simples para enviar ao suporte da software house

Quando o cliente quiser solicitar ativação da integração, pode enviar estas informações:

```text
Solicito a ativação da integração CIOT/ANTT para o CNPJ abaixo.

CNPJ:
Razão social:
RNTRC:
Categoria no RNTRC: ETC / CTC / TAC
Equiparado a TAC? Sim / Não / Não sei
Quantidade de veículos no RNTRC:
Veículo(s) principal(is) vinculado(s) ao RNTRC? Sim / Não
Ambiente desejado: Homologação / Produção
Provedor desejado: ANTT direta / Instituição de Pagamento autorizada / Não sei
Tipo de operação: Carga lotação / Carga fracionada / TAC-Agregado / Outra
A operação será realizada pela própria ETC? Sim / Não
Haverá subcontratação de outra ETC? Sim / Não
Usa TAC ou TAC equiparado? Sim / Não
Certificado digital disponível? Sim / Não
Tipo do certificado: A1 / A3
CNPJ do certificado:
Data de vencimento do certificado:
Responsável interno pelo certificado:
Contato técnico:
Contato administrativo:
```

---

# 18. Quando o cliente NÃO deve pedir integração direta com a ANTT

A integração direta com a ANTT pode não ser o caminho correto quando:

- a empresa não é transportadora registrada no RNTRC;
- o RNTRC está inativo;
- a empresa é TAC, CTC ou ETC equiparada a TAC em cenário que exige Instituição de Pagamento;
- a operação envolve TAC ou TAC equiparado e a regra exige Instituição de Pagamento;
- a empresa subcontratada não será quem efetivamente realizará o transporte;
- o veículo não está vinculado ao RNTRC do transportador que realizará a operação;
- a operação é carga própria sem prestação remunerada a terceiros;
- a operação está em hipótese de não aplicação do CIOT;
- a empresa quer apenas emitir CIOT gratuitamente via portal de uma Instituição de Pagamento;
- a empresa não possui certificado digital válido;
- a empresa não possui controle operacional para cancelar, retificar, transmitir contingência ou encerrar CIOT;
- o software ainda não está preparado para tratar retornos, rejeições, protocolos, código verificador, AvisoTransportador e logs.

Nesses casos, o cliente deve primeiro regularizar o cadastro, ajustar a operação ou utilizar uma Instituição de Pagamento autorizada.

---

# 19. Apêndice — Caso especial: Instituição de Pagamento que quer autorização da ANTT

Este item **não é o caminho normal do cliente transportador**.

Ele se aplica quando a empresa quer atuar como **Instituição de Pagamento autorizada** para emitir CIOT.

## 19.1 Quem pode solicitar

O processo deve ser verificado na página oficial vigente da ANTT/Gov.br.

Em regra, aplica-se a Instituições de Pagamento autorizadas a funcionar pelo Banco Central do Brasil e que atendam aos requisitos técnicos, operacionais e regulatórios exigidos pela ANTT.

## 19.2 Como solicitar

O pedido deve ser apresentado à ANTT pelo canal oficial indicado no serviço vigente.

Fluxo resumido:

```text
1. Acessar o serviço Gov.br ou canal oficial de solicitação de autorização para emissão de CIOT
2. Entrar com a conta exigida no serviço
3. Protocolar o pedido no sistema indicado
4. Enviar documentação exigida
5. Aguardar análise da ANTT
6. Responder eventuais exigências
7. Receber resposta com os encaminhamentos
8. Cumprir requisitos técnicos e operacionais
9. Integrar ao sistema de geração de CIOT
```

## 19.3 Observação

Esse processo é para empresas que querem ser **geradoras autorizadas de CIOT**, não para uma transportadora comum que apenas deseja gerar CIOT das próprias operações.

---

# 20. Perguntas rápidas

## 20.1 Preciso de usuário e senha da ANTT para a API?

Pelo modelo técnico do DCS, a comunicação usa HTTPS/TLS com autenticação mútua por certificado digital. A identificação ocorre pelo certificado, não por login e senha tradicional.

## 20.2 Qualquer certificado A1 serve?

Não. O certificado deve ser válido, ICP-Brasil, conter o CNPJ correto e permitir autenticação cliente. Na prática, use o e-CNPJ da empresa que será identificada na operação. O DCS prevê certificados A1 ou A3.

## 20.3 Posso usar certificado da matriz para operação da filial?

Na **emissão direta**, use o certificado do estabelecimento que efetivamente executa a operação e confira o RNTRC e a frota. Não presuma que matriz e filial sejam intercambiáveis pela raiz comum. O [DCS v1.2, B115](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/dcs_pef_v1-2.pdf#page=62) usa o CNPJ identificado na API e o `CpfCnpjContratado` para identificar emissão em nome próprio e impedir essa modalidade para CTC ou ETC equiparada a TAC.

## 20.4 Posso gerar CIOT depois do MDF-e?

Não é o fluxo recomendado. O CIOT deve ser gerado antes do início da operação e deve ser vinculado ao MDF-e correspondente quando houver MDF-e aplicável.

## 20.5 A geração do CIOT é paga?

A geração/recebimento do CIOT deve ser gratuita. Instituições de Pagamento podem oferecer serviços adicionais, mas a geração do CIOT em si não deve gerar custo ao transportador.

## 20.6 O software pode inventar o número do CIOT?

Não. O CIOT é retorno oficial da ANTT ou de fonte autorizada. O software deve declarar a operação, receber o código, o código verificador, o protocolo e as mensagens de retorno, e armazenar essas informações.

## 20.7 ETC com até 3 veículos pode usar integração direta com a ANTT?

Em regra, a ETC com até 3 veículos é tratada como TAC equiparado para determinadas regras de pagamento e CIOT. Quando a operação envolver TAC ou TAC equiparado, a geração deve ocorrer por Instituição de Pagamento autorizada.

## 20.8 Se não houver MDF-e ou CT-e, deixa de existir obrigação de CIOT?

Não necessariamente. A obrigação do CIOT decorre da operação de transporte rodoviário remunerado de cargas, observadas as exceções da regulamentação. Em operações municipais, por exemplo, pode não haver MDF-e ou CT-e, mas ainda pode haver obrigação regulatória de CIOT se houver transporte de carga de terceiros mediante remuneração.

## 20.9 O recebimento do CIOT significa que a operação está definitivamente regular?

Não. O recebimento do CIOT indica que a operação foi registrada e passou pelas validações sistêmicas aplicáveis naquele momento. A ANTT pode fiscalizar posteriormente os dados declarados, documentos fiscais, contrato, pagamento, RNTRC, veículos e demais elementos da operação.

## 20.10 Mudou o veículo da operação. Posso apenas retificar?

A eventual mudança de informação do veículo enseja o cadastramento de uma nova operação de transporte. O software deve tratar esse cenário com cuidado e não assumir retificação automática.

---

# 21. Resumo para o cliente

Para usar CIOT corretamente, o cliente deve garantir:

1. que existe operação de transporte rodoviário remunerado de cargas;
2. que a operação não está em hipótese de exceção;
3. que foi definido o caminho correto: ANTT direta ou Instituição de Pagamento autorizada;
4. que o CNPJ está regular;
5. que o RNTRC está ativo;
6. que a categoria RNTRC e a equiparação a TAC foram conferidas;
7. que a frota está correta e vinculada ao transportador que realizará a operação;
8. que possui certificado digital válido, quando aplicável;
9. que o software está configurado para usar esse certificado ou o provedor correto;
10. que os testes foram feitos em homologação;
11. que o CIOT será gerado antes do início da operação;
12. que o CIOT será vinculado ao MDF-e, quando houver MDF-e aplicável;
13. que haverá controle de consulta, cancelamento, retificação, contingência e encerramento.

---

# 22. Fontes oficiais usadas como base

- ANTT — CIOT Para Todos: página oficial com regras gerais, obrigatoriedade, responsáveis pela geração, tipos de operação, contingência e exceções.
- [ANTT — Documentos Técnicos do CIOT](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/documentos-tecnicos): página com DCS, biblioteca/executável de produção e homologação e informação de que as ETCs já possuem cadastro para uso dos serviços do CIOT.
- [ANTT — DCS PEF v1.2](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/dcs_pef_v1-2.pdf): Documento de Contrato de Serviço do PEF/CIOT, contendo serviços, padrão REST/JSON, TLS/HTTPS, autenticação mútua, requisitos do certificado digital, regras de validação e ambientes.
- ANTT — Perguntas Frequentes CIOT: orientações sobre obrigatoriedade, geração antes da operação, uso de Web Services, Instituição de Pagamento, vínculo com MDF-e, custo, dados necessários, retornos, prazos e fiscalização.
- ANTT — RNTRC: informações sobre cadastro obrigatório no Registro Nacional de Transportadores Rodoviários de Cargas.
- Consulta Pública ANTT: consulta de RNTRC, PEF/CIOT, Vale-Pedágio e dados relacionados.

---

# 23. Observação final

Este material deve ser usado como orientação operacional para o cliente.  

Antes de liberar produção, valide sempre:

- documentação técnica vigente da ANTT;
- versão atual do DCS;
- portarias operacionais vigentes;
- regras de homologação;
- condições reais do CNPJ/RNTRC do cliente;
- vínculo da frota;
- cenário de contratação;
- caminho correto de geração do CIOT.
