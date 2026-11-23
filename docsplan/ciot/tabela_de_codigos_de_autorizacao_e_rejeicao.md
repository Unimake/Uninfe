# CIOT/ANTT - Tabela de Códigos de Status e Rejeição dos Serviços PEF

> Documento técnico para desenvolvedores que integram com os serviços de CIOT/PEF da ANTT.
>
> Base principal: **Documento de Contrato de Serviço - Pagamento Eletrônico de Frete - Serviços PEF**, [PEF v1.2](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/dcs_pef_v1-2.pdf), versão pública referenciada pela ANTT na revisão de 23/11/2026.
>
> Objetivo: organizar os códigos retornados no campo `Codigo` e as respectivas mensagens de sucesso ou rejeição para cada serviço disponível.

---

## 1. Como interpretar o campo `Codigo`

Nos retornos dos serviços PEF/CIOT, o campo `Codigo` indica o resultado do processamento da solicitação.

O DCS da ANTT estabelece dois tipos principais de resultado:

| Tipo de resultado | Interpretação técnica |
|---|---|
| Sucesso | Solicitação atendida. O retorno traz código de sucesso, mensagem e, quando aplicável, protocolo, CIOT ou código verificador. |
| Rejeição | Solicitação não atendida. O retorno traz o código do motivo da rejeição e a mensagem correspondente. |

A tabela oficial de sucesso consolidada no DCS é:

| Código | Mensagem oficial | Interpretação para o sistema |
|---:|---|---|
| `110` | Dados inseridos com sucesso! | A operação de manutenção foi aceita/inserida/processada com sucesso. Usado em serviços como declaração, cancelamento, retificação e encerramento. |
| `111` | Consulta realizada com sucesso! | A consulta foi executada com sucesso. Usado em serviços de consulta, como transportador, frota, exceção e CIOT gerado. |

---

## 2. Exemplo de retorno JSON de declaração autorizada

Exemplo de retorno de uma declaração de operação de transporte autorizada/inserida com sucesso:

```json
{
  "IdOperacaoTransporte": "110000011111",
  "CodigoVerificador": "1736",
  "Protocolo": "1100000111111111",
  "Codigo": 110,
  "Mensagem": "Dados inseridos com sucesso!",
  "AvisoTransportador": ""
}
```

Neste serviço, a tabela de saída do DCS usa `CodigoIdentificacaoOperacao`, mas o exemplo JSON do próprio DCS usa `IdOperacaoTransporte`. O exemplo acima segue este último nome. Confirme o contrato do provedor no ambiente usado e não confunda a resposta da ANTT com o XML de retorno do UniNFe.

Interpretação:

| Campo | Valor no exemplo | Significado |
|---|---|---|
| `IdOperacaoTransporte` | `110000011111` | Identificador de 12 caracteres, sem o código verificador. |
| `CodigoVerificador` | `1736` | Código verificador gerado pela ANTT. |
| `Protocolo` | `1100000111111111` | Protocolo retornado pela ANTT. Guarde-o como campo separado; não derive o CIOT ou o código verificador a partir dele. |
| `Codigo` | `110` | Sucesso: dados inseridos com sucesso. |
| `Mensagem` | `Dados inseridos com sucesso!` | Mensagem oficial de sucesso. |
| `AvisoTransportador` | vazio | Quando houver aviso, deve ser apresentado ao transportador e impresso no documento correspondente, conforme orientação do DCS. |

---

## 3. Serviços disponíveis no DCS PEF/CIOT

O DCS PEF v1.2 relaciona sete serviços. `ConsultarExcecao` foi removido da especificação; integrações novas devem verificar as exceções pela regulamentação, sem depender desse serviço legado.

| Nº | Serviço | Finalidade |
|---:|---|---|
| 1 | `ConsultarSituacaoTransportador` | Validar dados do transportador junto ao RNTRC e retornar categoria, situação e se é equiparado a TAC. |
| 2 | `ConsultarFrotaTransportador` | Verificar a situação de veículos sob posse ou propriedade do transportador. |
| 3 | `DeclaracaoOperacaoTransporte` | Recepcionar e armazenar dados da operação de transporte e retornar protocolo de autorização. |
| 4 | `CancelamentoOperacaoTransporte` | Cancelar uma operação de transporte, desde que ainda não tenha sido realizada. |
| 5 | `RetificacaoOperacaoTransporte` | Retificar uma operação declarada com erro ou com alteração permitida dentro do prazo. |
| 6 | `EncerramentoOperacaoTransporte` | Encerrar uma operação de transporte executada com sucesso. |
| 7 | `ConsultarCIOTGerado` | Retornar o código de identificação da operação de transporte com o dígito verificador gerado pela ANTT. |

---

## 4. Tabelas por serviço

### 4.1. `ConsultarSituacaoTransportador`

#### Códigos de sucesso

| Código | Mensagem oficial | Interpretação |
|---:|---|---|
| `111` | Consulta realizada com sucesso! | Consulta do transportador processada com sucesso. |

#### Códigos de rejeição aplicáveis

| Código | Regra | Mensagem/descrição |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `207` | `B3` | Rejeição: Não foi encontrado transportador contratado com CPF/CNPJ `{0}` e RNTRC `{1}`. |
| `205` | `B4` | Rejeição: O campo `<nome do campo>` é inválido. |
| `205` | `B60` | Rejeição: O campo `<nome do campo>` é inválido. Aplicável à normalização/quantidade de dígitos do RNTRC. |
| `229` | `B1` | Rejeição: O campo `<nome do campo>` é obrigatório. |
| `311` | `B112` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador informado e, por isso, não possui permissão para realizar a operação. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |

---

### 4.2. `ConsultarFrotaTransportador`

#### Códigos de sucesso

| Código | Mensagem oficial | Interpretação |
|---:|---|---|
| `111` | Consulta realizada com sucesso! | Consulta de frota processada com sucesso. |

#### Códigos de rejeição aplicáveis

| Código | Regra | Mensagem/descrição |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `207` | `B3` | Rejeição: Não foi encontrado transportador contratado com CPF/CNPJ `{0}` e RNTRC `{1}`. |
| `205` | `B4` | Rejeição: O campo `<nome do campo>` é inválido. |
| `205` | `B60` | Rejeição: O campo `<nome do campo>` é inválido. Aplicável à normalização/quantidade de dígitos do RNTRC. |
| `229` | `B1` | Rejeição: O campo `<nome do campo>` é obrigatório. |
| `311` | `B112` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador informado e, por isso, não possui permissão para realizar a operação. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |

---

### 4.3. `DeclaracaoOperacaoTransporte`

#### Códigos de sucesso

| Código | Mensagem oficial | Interpretação |
|---:|---|---|
| `110` | Dados inseridos com sucesso! | Declaração da operação de transporte autorizada/inserida com sucesso. |

#### Códigos de rejeição aplicáveis

| Código | Regra | Mensagem/descrição |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `204` | `B100` | Rejeição: Não é permitido o tipo de pagamento informado para transportador TAC ou equiparado. |
| `205` | `B4` | Rejeição: O campo `<nome do campo>` é inválido. |
| `205` | `B60` | Rejeição: O campo `<nome do campo>` é inválido. Aplicável à normalização/quantidade de dígitos do RNTRC. |
| `207` | `B3` | Rejeição: Não foi encontrado transportador contratado com CPF/CNPJ `{0}` e RNTRC `{1}`. |
| `208` | `B6` | Rejeição: A data de início da viagem não pode ser inferior à data atual. |
| `210` | `B101` | Rejeição: Quantidade de eixos inválida para o tipo de veículo informado. |
| `211` | `B9` | Rejeição: O código da natureza da carga informado não existe. |
| `214` | `B96` | Rejeição: A data da declaração não pode ser anterior a 168 horas em relação à data atual. |
| `215` | `B103` | Rejeição: O campo `<campo dependente>` é obrigatório quando `<campo-condição>` = true. |
| `215` | `B106` | Rejeição: Os campos de parcelamento não devem ser informados quando o pagamento for à vista. |
| `216` | `B14` | Rejeição: CPF/CNPJ e RNTRC informados não correspondem a um transportador com RNTRC ativo. |
| `217` | `B15` | Rejeição: A placa `{0}` não pertence ao transportador de RNTRC `{1}`, ou o mesmo não está ativo. |
| `219` | `B17` | Rejeição: Código de identificação da operação já cadastrado. |
| `224` | `B99` | Rejeição: Foram informados campos não aplicáveis ao tipo de pagamento selecionado. |
| `225` | `B12` | Rejeição: A data de início da viagem não pode ser inferior à data de declaração. |
| `226` | `B18` | Rejeição: Peso da carga deve ser maior que 0 e menor 9999999.99. |
| `227` | `B19` | Rejeição: A data da declaração deve ser menor ou igual à data atual e não pode ser anterior a 30 dias em relação à data atual. |
| `228` | `B20` | Rejeição: O veículo do tipo automotor informado não possui vínculo com o transportador contratado. |
| `229` | `B1` | Rejeição: O campo `<nome do campo>` é obrigatório. |
| `229` | `B104` | Rejeição: Não é permitido informar o campo `<campo dependente>` quando `<campo-condição>` = false. |
| `230` | `B66` | Rejeição: CPF/CNPJ do destinatário obrigatório para o tipo de operação informado. |
| `231` | `B67` | Rejeição: O campo `<nome do campo>` é obrigatório no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga fracionada. |
| `233` | `B92` | Rejeição: Os campos obrigatórios para o tipo de pagamento informado não foram preenchidos. |
| `234` | `B21` | Rejeição: O intervalo entre a data de início e a data de fim da viagem não pode ser superior a 90 dias. |
| `239` | `B65` | Rejeição: O campo `<nome do campo>` é obrigatório no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga lotação. |
| `241` | `B69` | Rejeição: Natureza da carga obrigatória para o tipo de operação informado. |
| `244` | `B24` | Rejeição: Já existem 2 CIOTs abertos entre o contratante e o contratado informados, com um ou mais veículos já relacionados em operação de transporte do tipo TAC-Agregado. |
| `245` | `B25` | Rejeição: Não é possível declarar uma nova operação de transporte de TAC-Agregado, pois existe um CIOT pendente de encerramento. |
| `247` | `B27` | Rejeição: O contratado informado deve ser do tipo TAC para operações do tipo TAC-Agregado. |
| `250` | `B30` | Rejeição: O intervalo entre a data de início e a data de fim da viagem, para operação de transporte do tipo TAC-Agregado, não pode ser superior a 30 dias. |
| `252` | `B105` | Rejeição: Os campos de parcelamento devem ser informados quando o pagamento for parcelado. |
| `257` | `B56` | Rejeição: A data de início da viagem não deve ser informada na declaração de operação de transporte de TAC-Agregado. |
| `258` | `B71` | Rejeição: O RNTRC informado não pertence ao CPF/CNPJ do contratante informado. |
| `261` | `B64` | Rejeição: Não é permitido informar o campo `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga lotação. |
| `264` | `B31` | Rejeição: Já existem 2 CIOTs vigentes entre o contratante e o contratado informados, relacionando os mesmos veículos. |
| `265` | `B32` | Rejeição: Já existem 2 CIOTs, um aberto e um vigente, entre o contratante e o contratado informados, relacionando os mesmos veículos. |
| `266` | `B33` | Rejeição: Existem CIOTs pendentes de encerramento pelo contratante há mais de 60 dias: `<CIOT + DV>`. |
| `268` | `B5` | Rejeição: Existe duplicidade de placa na lista informada. |
| `269` | `B11` | Rejeição: A data e hora da declaração está fora do intervalo de tolerância permitido para esta operação. |
| `270` | `B13` | Rejeição: A data prevista para término da viagem deve ser maior ou igual a data de início da viagem. |
| `271` | `B22` | Rejeição: A data de início da viagem excede o limite máximo de 30 dias em relação à data atual. |
| `274` | `B57` | Rejeição: Não é permitido informar o campo `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga fracionada. |
| `276` | `B49` | Rejeição: Somente é permitido informar até 5 veículos para uma declaração de operação de transporte. |
| `280` | `B58` | Rejeição: O campo `<nome do campo>` é obrigatório no serviço de `<declaração/retificação/encerramento>` para operações do tipo TAC-Agregado. |
| `281` | `B62` | Rejeição: Não é permitido informar o `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` da operação de transporte do tipo TAC-Agregado. |
| `285` | `B73` | Rejeição: O(s) veículo(s) de placas `{0}` está(ão) contratado(s) para outro contratante. |
| `291` | `B80` | Frete inferior ao piso aplicável. O DCS v1.2 descreve a validação fora da contingência, para carga lotação com um único par origem/destino, ressalvada exceção judicial ativa cadastrada na ANTT. |
| `292` | `B82` | Rejeição: Distância não compatível com a Origem/Destino informados. Revise a distância declarada ou seja mais específico nas informações de origem e destino. |
| `293` | `B83` | Rejeição: É necessário informar ao menos um veículo do tipo automotor. |
| `294` | `B84` | Rejeição: Somente um veículo deve ser do tipo automotor. |
| `307` | `B107` | Rejeição por veículo bloqueado judicialmente no RNTRC. O DCS informa a regra e o código, mas deixa a coluna de mensagem textual sem preenchimento. |
| `308` | `B109` | Rejeição: Deve ser informada ao menos uma forma válida de localização para origem e destino: município, CEP ou coordenadas. |
| `309` | `B110` | Rejeição: Latitude e longitude devem ser informadas em conjunto. |
| `310` | `B111` | Rejeição: O tipo de localização informado para origem deve ser o mesmo informado para destino. |
| `314` | `B115` | Na v1.2, compara contratado e CNPJ autenticado para identificar emissão em nome próprio e rejeitá-la para CTC ou ETC equiparada a TAC. Confira também o contexto e a mensagem efetivamente retornada. |
| `316` | `B117` | Fora da contingência, em carga lotação ou fracionada com cavalo-trator, informe pelo menos um implemento. |
| `317` | `B118` | Rejeição: Não é permitido informar o tipo de pagamento por cartão pré-pago quando o certificado digital não pertence a uma IP. |
| `318` | `B119` | Rejeição: O(s) contratante(s) da carga fracionada não pode(m) ser igual(is) ao contratante da operação de transporte. |
| `319` | `B120` | Rejeição: O valor do frete deve ser informado. Regra associada: valor do frete deve ser maior que zero. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |
| Não indicado no DCS | `B123` | No TAC-agregado, o fim informado deve ser pelo menos 10 dias após o início. |
| `322` | `B124` | Origem e destino com o mesmo código IBGE: use CEPs distintos ou coordenadas. |
| `323` | `B125` | Origem e destino com o mesmo CEP: informe coordenadas. |
| `324` | `B126` | Coordenadas de origem e destino iguais. |
| `325` | `B127` | CEP não reconhecido: informe as coordenadas no par de origem e destino. |

---

### 4.4. `CancelamentoOperacaoTransporte`

#### Códigos de sucesso

| Código | Mensagem oficial | Interpretação |
|---:|---|---|
| `110` | Dados inseridos com sucesso! | Cancelamento processado com sucesso. |

#### Códigos de rejeição aplicáveis

| Código | Regra | Mensagem/descrição |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `205` | `B4` | Rejeição: O campo `<nome do campo>` é inválido. |
| `220` | `B34` | Rejeição: Não foi encontrada nenhuma Operação de Transporte com os dados informados. |
| `221` | `B35` | Rejeição: O prazo para cancelamento da operação de transporte foi excedido. |
| `222` | `B37` | Rejeição: Operação de Transporte já está cancelada. |
| `229` | `B1` | Rejeição: O campo `<nome do campo>` é obrigatório. |
| `235` | `B36` | Rejeição: Não é possível cancelar uma operação de transporte já consultada pela fiscalização da ANTT. |
| `236` | `B39` | Rejeição: O prazo para retificação da operação de transporte foi expirado para o tipo de operação informado. |
| `251` | `B38` | Rejeição: Operação de Transporte já está encerrada. |
| `312` | `B113` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador responsável pelo CIOT informado e, por isso, não possui permissão para realizar a operação. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |

> Observação: a regra `B39` aparece vinculada ao campo `MotivoCancelamento` no leiaute do serviço, embora a descrição da regra trate de prazo de retificação. Mantenha esse vínculo como consta no DCS, mas valide em testes reais.

---

### 4.5. `RetificacaoOperacaoTransporte`

#### Códigos de sucesso

| Código | Mensagem oficial | Interpretação |
|---:|---|---|
| `110` | Dados inseridos com sucesso! | Retificação processada com sucesso. |

#### Códigos de rejeição aplicáveis

| Código | Regra | Mensagem/descrição |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `205` | `B4` | Rejeição: O campo `<nome do campo>` é inválido. |
| `220` | `B34` | Rejeição: Não foi encontrada nenhuma Operação de Transporte com os dados informados. |
| `222` | `B37` | Rejeição: Operação de Transporte já está cancelada. |
| `229` | `B1` | Rejeição: O campo `<nome do campo>` é obrigatório. |
| `234` | `B21` | Rejeição: O intervalo entre a data de início e a data de fim da viagem não pode ser superior a 90 dias. |
| `236` | `B39` | Rejeição: O prazo para retificação da operação de transporte foi expirado para o tipo de operação informado. |
| `237` | `B40` | Rejeição: Não é possível retificar uma operação de transporte já consultada pela fiscalização da ANTT. |
| `250` | `B30` | Rejeição: O intervalo entre a data de início e a data de fim da viagem, para operação de transporte do tipo TAC-Agregado, não pode ser superior a 30 dias. |
| `251` | `B38` | Rejeição: Operação de Transporte já está encerrada. |
| `261` | `B64` | Rejeição: Não é permitido informar o campo `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga lotação. |
| `270` | `B13` | Rejeição: A data prevista para término da viagem deve ser maior ou igual a data de início da viagem. |
| `274` | `B57` | Rejeição: Não é permitido informar o campo `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga fracionada. |
| `281` | `B62` | Rejeição: Não é permitido informar o `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` da operação de transporte do tipo TAC-Agregado. |
| `291` | `B80` | Frete inferior ao piso aplicável. O DCS v1.2 descreve a validação fora da contingência, para carga lotação com um único par origem/destino, ressalvada exceção judicial ativa cadastrada na ANTT. |
| `308` | `B109` | Rejeição: Deve ser informada ao menos uma forma válida de localização para origem e destino: município, CEP ou coordenadas. |
| `309` | `B110` | Rejeição: Latitude e longitude devem ser informadas em conjunto. |
| `310` | `B111` | Rejeição: O tipo de localização informado para origem deve ser o mesmo informado para destino. |
| `312` | `B113` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador responsável pelo CIOT informado e, por isso, não possui permissão para realizar a operação. |
| `319` | `B120` | Rejeição: O valor do frete deve ser informado. Regra associada: valor do frete deve ser maior que zero. |
| `320` | `B121` | Rejeição: Não é permitido realizar retificação para operações do tipo carga lotação. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |
| `322` | `B124` | Origem e destino com o mesmo código IBGE: use CEPs distintos ou coordenadas. |
| `323` | `B125` | Origem e destino com o mesmo CEP: informe coordenadas. |
| `324` | `B126` | Coordenadas de origem e destino iguais. |
| `325` | `B127` | CEP não reconhecido: informe as coordenadas no par de origem e destino. |

---

### 4.6. `EncerramentoOperacaoTransporte`

#### Códigos de sucesso

| Código | Mensagem oficial | Interpretação |
|---:|---|---|
| `110` | Dados inseridos com sucesso! | Encerramento processado com sucesso. |

#### Códigos de rejeição aplicáveis

| Código | Regra | Mensagem/descrição |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `205` | `B4` | Rejeição: O campo `<nome do campo>` é inválido. |
| `220` | `B34` | Rejeição: Não foi encontrada nenhuma Operação de Transporte com os dados informados. |
| `222` | `B37` | Rejeição: Operação de Transporte já está cancelada. |
| `226` | `B18` | Rejeição: Peso da carga deve ser maior que 0 e menor 9999999.99. |
| `229` | `B1` | Rejeição: O campo `<nome do campo>` é obrigatório. |
| `239` | `B65` | Rejeição: O campo `<nome do campo>` é obrigatório no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga lotação. |
| `251` | `B38` | Rejeição: Operação de Transporte já está encerrada. |
| `263` | `B46` | Rejeição: Não é possível encerrar uma operação de transporte antes da data de início cadastrada. |
| `274` | `B57` | Rejeição: Não é permitido informar o campo `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` para operações do tipo carga fracionada. |
| `280` | `B58` | Rejeição: O campo `<nome do campo>` é obrigatório no serviço de `<declaração/retificação/encerramento>` para operações do tipo TAC-Agregado. |
| `281` | `B62` | Rejeição: Não é permitido informar o `<nome do campo>` no serviço de `<declaração/retificação/encerramento>` da operação de transporte do tipo TAC-Agregado. |
| `308` | `B109` | Rejeição: Deve ser informada ao menos uma forma válida de localização para origem e destino: município, CEP ou coordenadas. |
| `309` | `B110` | Rejeição: Latitude e longitude devem ser informadas em conjunto. |
| `310` | `B111` | Rejeição: O tipo de localização informado para origem deve ser o mesmo informado para destino. |
| `312` | `B113` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador responsável pelo CIOT informado e, por isso, não possui permissão para realizar a operação. |
| `315` | `B116` | No TAC-agregado, o encerramento não pode ocorrer antes de 10 dias do início da viagem. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |
| `322` | `B124` | Origem e destino com o mesmo código IBGE: use CEPs distintos ou coordenadas. |
| `323` | `B125` | Origem e destino com o mesmo CEP: informe coordenadas. |
| `324` | `B126` | Coordenadas de origem e destino iguais. |
| `325` | `B127` | CEP não reconhecido: informe as coordenadas no par de origem e destino. |

---

### 4.7. `ConsultarCIOTGerado`

#### Códigos de sucesso

| Código | Mensagem oficial | Interpretação |
|---:|---|---|
| `111` | Consulta realizada com sucesso! | Consulta do CIOT gerado processada com sucesso. |

#### Códigos de rejeição aplicáveis

| Código | Regra | Mensagem/descrição |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `205` | `B4` | Rejeição: O campo `<nome do campo>` é inválido. |
| `220` | `B34` | Rejeição: Não foi encontrada nenhuma Operação de Transporte com os dados informados. |
| `229` | `B1` | Rejeição: O campo `<nome do campo>` é obrigatório. |
| `312` | `B113` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador responsável pelo CIOT informado e, por isso, não possui permissão para realizar a operação. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |

---

## 5. Catálogo geral de códigos de rejeição identificados no DCS

Esta tabela reúne os códigos de rejeição encontrados nas regras de validação do DCS. Alguns códigos podem aparecer em mais de uma regra.

| Código | Regra | Mensagem/descrição oficial ou interpretação direta do DCS |
|---:|---|---|
| `203` | `B2` | Serviço inativo. O DCS informa a regra "Verifica se o serviço está ativo", mas não traz mensagem textual completa. |
| `204` | `B100` | Rejeição: Não é permitido o tipo de pagamento informado para transportador TAC ou equiparado. |
| `205` | `B4`, `B16`, `B60` | Rejeição: O campo `<nome do campo>` é inválido. |
| `207` | `B3` | Rejeição: Não foi encontrado transportador contratado com CPF/CNPJ `{0}` e RNTRC `{1}`. |
| `208` | `B6` | Rejeição: A data de início da viagem não pode ser inferior à data atual. |
| `210` | `B101` | Rejeição: Quantidade de eixos inválida para o tipo de veículo informado. |
| `211` | `B9` | Rejeição: O código da natureza da carga informado não existe. |
| `214` | `B96` | Rejeição: A data da declaração não pode ser anterior a 168 horas em relação à data atual. |
| `215` | `B103`, `B106` | Rejeição: campo dependente obrigatório quando condição verdadeira; ou campos de parcelamento não devem ser informados quando pagamento for à vista. |
| `216` | `B14` | Rejeição: CPF/CNPJ e RNTRC informados não correspondem a um transportador com RNTRC ativo. |
| `217` | `B15` | Rejeição: A placa `{0}` não pertence ao transportador de RNTRC `{1}`, ou o mesmo não está ativo. |
| `218` | `B42` | Rejeição: Esta administradora não tem permissão para declarar operações de transporte para o contratante de CNPJ `{0}`. |
| `219` | `B17` | Rejeição: Código de identificação da operação já cadastrado. |
| `220` | `B34` | Rejeição: Não foi encontrada nenhuma Operação de Transporte com os dados informados. |
| `221` | `B35` | Rejeição: O prazo para cancelamento da operação de transporte foi excedido. |
| `222` | `B37`, `B86` | Rejeição: Operação de Transporte já está cancelada. |
| `224` | `B99` | Rejeição: Foram informados campos não aplicáveis ao tipo de pagamento selecionado. |
| `225` | `B12` | Rejeição: A data de início da viagem não pode ser inferior à data de declaração. |
| `226` | `B18` | Rejeição: Peso da carga deve ser maior que 0 e menor 9999999.99. |
| `227` | `B19` | Rejeição: A data da declaração deve ser menor ou igual à data atual e não pode ser anterior a 30 dias em relação à data atual. |
| `228` | `B20` | Rejeição: O veículo do tipo automotor informado não possui vínculo com o transportador contratado. |
| `229` | `B1`, `B104` | Rejeição: campo obrigatório; ou campo dependente informado indevidamente quando a condição é falsa. |
| `230` | `B66` | Rejeição: CPF/CNPJ do destinatário obrigatório para o tipo de operação informado. |
| `231` | `B67` | Rejeição: O campo `<nome do campo>` é obrigatório para operações do tipo carga fracionada. |
| `233` | `B92` | Rejeição: Os campos obrigatórios para o tipo de pagamento informado não foram preenchidos. |
| `234` | `B21` | Rejeição: O intervalo entre a data de início e a data de fim da viagem não pode ser superior a 90 dias. |
| `235` | `B36` | Rejeição: Não é possível cancelar uma operação de transporte já consultada pela fiscalização da ANTT. |
| `236` | `B39` | Rejeição: O prazo para retificação da operação de transporte foi expirado para o tipo de operação informado. |
| `237` | `B40` | Rejeição: Não é possível retificar uma operação de transporte já consultada pela fiscalização da ANTT. |
| `239` | `B65` | Rejeição: O campo `<nome do campo>` é obrigatório para operações do tipo carga lotação. |
| `241` | `B69` | Rejeição: Natureza da carga obrigatória para o tipo de operação informado. |
| `244` | `B24` | Rejeição: Já existem 2 CIOTs abertos entre o contratante e o contratado informados, com veículo já relacionado em operação TAC-Agregado. |
| `245` | `B25` | Rejeição: Não é possível declarar nova operação TAC-Agregado, pois existe CIOT pendente de encerramento. |
| `247` | `B27` | Rejeição: O contratado informado deve ser do tipo TAC para operações do tipo TAC-Agregado. |
| `250` | `B30` | Rejeição: O intervalo entre início e fim da viagem para TAC-Agregado não pode ser superior a 30 dias. |
| `251` | `B38` | Rejeição: Operação de Transporte já está encerrada. |
| `252` | `B105` | Rejeição: Os campos de parcelamento devem ser informados quando o pagamento for parcelado. |
| `257` | `B56` | Rejeição: A data de início da viagem não deve ser informada na declaração TAC-Agregado. |
| `258` | `B71` | Rejeição: O RNTRC informado não pertence ao CPF/CNPJ do contratante informado. |
| `260` | `B43` | Rejeição: Os dados da viagem são obrigatórios no encerramento de operação de transporte de TAC-Agregado. |
| `261` | `B44`, `B64` | Rejeição: dados da viagem não devem ser informados em encerramento de operação padrão; ou campo não permitido para carga lotação. |
| `263` | `B46` | Rejeição: Não é possível encerrar uma operação antes da data de início cadastrada. |
| `264` | `B31` | Rejeição: Já existem 2 CIOTs vigentes entre o contratante e o contratado, relacionando os mesmos veículos. |
| `265` | `B32` | Rejeição: Já existem 2 CIOTs, um aberto e um vigente, entre o contratante e o contratado, relacionando os mesmos veículos. |
| `266` | `B33` | Rejeição: Existem CIOTs pendentes de encerramento pelo contratante há mais de 60 dias: `<CIOT + DV>`. |
| `268` | `B5` | Rejeição: Existe duplicidade de placa na lista informada. |
| `269` | `B11` | Rejeição: A data e hora da declaração está fora do intervalo de tolerância permitido. |
| `270` | `B13` | Rejeição: A data prevista para término da viagem deve ser maior ou igual à data de início. |
| `271` | `B22` | Rejeição: A data de início da viagem excede o limite máximo de 30 dias em relação à data atual. |
| `274` | `B57` | Rejeição: Campo não permitido para operações do tipo carga fracionada. |
| `276` | `B49` | Rejeição: Somente é permitido informar até 5 veículos para uma declaração de operação de transporte. |
| `280` | `B58` | Rejeição: Campo obrigatório para operações do tipo TAC-Agregado. |
| `281` | `B62` | Rejeição: Campo não permitido para operação de transporte do tipo TAC-Agregado. |
| `285` | `B73` | Rejeição: O(s) veículo(s) de placas `{0}` está(ão) contratado(s) para outro contratante. |
| `291` | `B80` | Frete inferior ao piso aplicável. O DCS v1.2 descreve a validação fora da contingência, para carga lotação com um único par origem/destino, ressalvada exceção judicial ativa cadastrada na ANTT. |
| `292` | `B82` | Rejeição: Distância não compatível com a Origem/Destino informados. |
| `293` | `B83` | Rejeição: É necessário informar ao menos um veículo do tipo automotor. |
| `294` | `B84` | Rejeição: Somente um veículo deve ser do tipo automotor. |
| `296` | `B87` | Rejeição: O campo `<nome do campo>` não deve ser informado na retificação de operação de transporte de TAC-Agregado. |
| `307` | `B107` | Rejeição por veículo bloqueado judicialmente no RNTRC. O DCS informa regra e código, mas não traz mensagem textual preenchida. |
| `308` | `B109` | Rejeição: Deve ser informada ao menos uma forma válida de localização para origem e destino. |
| `309` | `B110` | Rejeição: Latitude e longitude devem ser informadas em conjunto. |
| `310` | `B111` | Rejeição: O tipo de localização informado para origem deve ser o mesmo informado para destino. |
| `311` | `B112` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador informado. |
| `312` | `B113` | Rejeição: O CPF/CNPJ do certificado digital não corresponde ao transportador responsável pelo CIOT informado. |
| `313` | `B114` | Rejeição: O contratante informado encontra-se bloqueado por penalidade vigente. |
| `314` | `B115` | Na v1.2, compara contratado e CNPJ autenticado para identificar emissão em nome próprio e rejeitá-la para CTC ou ETC equiparada a TAC. Confira também o contexto e a mensagem efetivamente retornada. |
| `315` | `B116` | No TAC-agregado, o encerramento não pode ocorrer antes de 10 dias do início da viagem. |
| `316` | `B117` | Fora da contingência, em carga lotação ou fracionada com cavalo-trator, informe pelo menos um implemento. |
| `317` | `B118` | Rejeição: Não é permitido informar pagamento por cartão pré-pago quando o certificado digital não pertence a uma IP. |
| `318` | `B119` | Rejeição: O(s) contratante(s) da carga fracionada não pode(m) ser igual(is) ao contratante da operação de transporte. |
| `319` | `B120` | Rejeição: O valor do frete deve ser informado. |
| `320` | `B121` | Rejeição: Não é permitido realizar retificação para operações do tipo carga lotação. |
| `321` | `B122` | Rejeição: Não é possível consultar operações de transporte vinculadas a CPF/CNPJ diferente do transportador autenticado. |
| Não indicado no DCS | `B123` | No TAC-agregado, o fim informado deve ser pelo menos 10 dias após o início. |
| `322` | `B124` | Origem e destino com o mesmo código IBGE: use CEPs distintos ou coordenadas. |
| `323` | `B125` | Origem e destino com o mesmo CEP: informe coordenadas. |
| `324` | `B126` | Coordenadas de origem e destino iguais. |
| `325` | `B127` | CEP não reconhecido: informe as coordenadas no par de origem e destino. |
| `-` | `C1` | Rejeição: O CPF/CNPJ do certificado digital não corresponde a nenhum transportador cadastrado no RNTRC. |

---

## 6. Recomendações para implementação

A v1.2 dispensa parte das validações em declarações em contingência. Consulte o histórico do DCS; a dispensa não se aplica automaticamente aos demais serviços nem autoriza usar contingência como forma de evitar rejeição.

### 6.1. Não tratar apenas HTTP Status

A integração pode retornar HTTP de sucesso e, ainda assim, conter rejeição de negócio dentro do payload.

O sistema deve sempre analisar:

- `Codigo`;
- `Mensagem`;
- `Protocolo`;
- dados específicos do retorno;
- conteúdo bruto da resposta.

### 6.2. Regra prática para autorização

Para a declaração da operação de transporte:

```text
Codigo = 110
Mensagem = Dados inseridos com sucesso!
```

Esse é o cenário em que a operação foi inserida/autorizada com sucesso.

### 6.3. Campos a armazenar sempre

| Campo | Motivo |
|---|---|
| `Codigo` | Define sucesso ou rejeição. |
| `Mensagem` | Texto oficial retornado pela ANTT. |
| `IdOperacaoTransporte` ou `CodigoIdentificacaoOperacao` | Identificador da operação de transporte/CIOT. |
| `CodigoVerificador` | Usado para compor o CIOT com DV. |
| `Protocolo` | Comprovação do processamento pela ANTT. |
| `AvisoTransportador` | Deve ser apresentado/impresso quando preenchido. |
| `RequestOriginal` | Auditoria e suporte. |
| `ResponseOriginal` | Auditoria e suporte. |
| `DataHoraEnvio` | Rastreabilidade. |
| `Ambiente` | Homologação ou produção. |
| `Certificado/CNPJ usado` | Auditoria técnica. |

---

## 7. Fontes consultadas

- ANTT/Gov.br - Documento de Contrato de Serviço - Pagamento Eletrônico de Frete - Serviços PEF: `https://www.gov.br/antt/pt-br/assuntos/cargas/pagamento-eletronico-de-fretes-pef-ciot/documento-de-contrato-de-servico.pdf`
- ANTT/Gov.br - Serviços PEF / DCS PEF v1.2: `https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1/dcs_pef_v1-2.pdf`

---

## 8. Nota de cautela

Este documento se baseia no DCS público v1.2 e em um retorno observado de integração. A rejeição 684 do MDF-e é fiscal, não um código destes serviços PEF; a rejeição 314/B115 pertence à emissão direta na ANTT.

Como a ANTT pode alterar regras, mensagens, serviços e validações, recomenda-se validar este material sempre que houver nova versão do DCS, alteração regulatória ou mudança operacional no ambiente de homologação/produção.
