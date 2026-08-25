# CIOT

O UniNFe integra os serviços de CIOT com dois provedores: **ANTT** e **eFrete**. O provedor é escolhido em cada XML de envio pela tag `ProvedorCIOT`, que deve ser o primeiro elemento dentro da tag raiz:

```xml
<ProvedorCIOT>ANTT</ProvedorCIOT>
```

ou:

```xml
<ProvedorCIOT>EFrete</ProvedorCIOT>
```

Use exatamente os valores `ANTT` ou `EFrete`. O UniNFe lê essa tag e encaminha a solicitação ao provedor correspondente. O namespace, a tag raiz e o sufixo do arquivo continuam sendo os mesmos para os dois provedores, permitindo reaproveitar a estrutura já usada na integração com a ANTT. Os XMLs do eFrete acrescentam campos próprios quando a operação exige.

## Serviços por provedor

| Serviço | ANTT | eFrete |
|---|:---:|:---:|
| [Cadastro de motorista](cadastro-motorista-efrete.md) | Não | Sim |
| [Cadastro de proprietário](cadastro-proprietario-efrete.md) | Não | Sim |
| [Cadastro de veículo](cadastro-veiculo-efrete.md) | Não | Sim |
| [Cancelamento da operação de transporte](cancelamento-operacao-transporte.md) | Sim | Sim |
| [Consultar CIOT gerado](consultar-ciot-gerado.md) | Sim | Sim |
| [Consultar exceção](consultar-excecao.md) | Sim | Não há modelo eFrete |
| [Consultar frota do transportador](consultar-frota-transportador.md) | Sim | Sim |
| [Consultar situação do transportador](consultar-situacao-transportador.md) | Sim | Sim |
| [Declaração de operação de transporte](declaracao-operacao-transporte.md) | Sim | Sim |
| [Encerramento da operação de transporte](encerramento-operacao-transporte.md) | Sim | Sim |
| [Gerar identificador da operação de transporte](gerar-id-operacao-transporte.md) | Sim | Não há modelo eFrete |
| [Retificação da operação de transporte](retificacao-operacao-transporte.md) | Sim | Não há modelo eFrete |

## Configuração do eFrete

Para enviar XMLs com `ProvedorCIOT` igual a `EFrete`, abra **Configurações > Empresas**, selecione a empresa e acesse **Outras Configurações > Configuração eFrete (CIOT)**. Preencha os dados fornecidos pelo eFrete:

| Campo | Uso |
|---|---|
| **Integrador** | Identifica o integrador. É obrigatório nas operações eFrete. |
| **Token** | Autenticação preferencial quando informado. |
| **Usuário** e **Senha** | Forma alternativa de autenticação; os dois campos devem ser informados em conjunto. |

A prioridade de autenticação é: token, usuário e senha e, quando essas credenciais não forem informadas, certificado digital. Se houver token ou usuário e senha, o integrador também deve estar preenchido. Consulte a [documentação da tela Configurações](../../configuracao/telas-cadastros-configuracoes.md#aba-outras-configurações).

## Modelos XML

Os modelos ficam separados por provedor:

- `exemplos xml/CIOT/ANTT`: modelos da integração já existente com a ANTT;
- `exemplos xml/CIOT/eFrete`: modelos eFrete, inclusive três variações de declaração de operação de transporte.

Parta sempre do modelo do provedor e do serviço que será consumido. Não envie um XML da ANTT apenas trocando a tag para `EFrete` quando a página do serviço indicar campos próprios do eFrete.

## Serviços documentados

- [Cadastro de motorista no eFrete](cadastro-motorista-efrete.md)
- [Cadastro de proprietário no eFrete](cadastro-proprietario-efrete.md)
- [Cadastro de veículo no eFrete](cadastro-veiculo-efrete.md)
- [Cancelamento da operação de transporte do CIOT](cancelamento-operacao-transporte.md)
- [Consultar CIOT gerado](consultar-ciot-gerado.md)
- [Consultar exceção do CIOT](consultar-excecao.md)
- [Consultar frota do transportador no CIOT](consultar-frota-transportador.md)
- [Consultar situação do transportador no CIOT](consultar-situacao-transportador.md)
- [Declaração de operação de transporte do CIOT](declaracao-operacao-transporte.md)
- [Encerramento da operação de transporte do CIOT](encerramento-operacao-transporte.md)
- [Gerar identificador da operação de transporte do CIOT](gerar-id-operacao-transporte.md)
- [Retificação da operação de transporte do CIOT](retificacao-operacao-transporte.md)
