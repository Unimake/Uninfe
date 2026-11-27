# NFSe Nacional 1.01 — mudanças no XML para orientação do suporte

**NT SE/CGNFS-e nº 009 — atualização dos schemas de 24/11/2026**

Este material detalha as mudanças desta atualização em relação ao pacote anterior da mesma NT. A versão da DPS e da NFS-e continua sendo 1.01.

Modelo de referência — [GerarNfseNovasTagsDPSIBSCBS-Nota-Tecnica-SECGNFS-e-009-2026-11-24-env-loterps.xml](GerarNfseNovasTagsDPSIBSCBS-Nota-Tecnica-SECGNFS-e-009-2026-11-24-env-loterps.xml). A DPS completa usa dados fictícios e reúne grupos de cenários diferentes para demonstrar a estrutura. Na emissão, mantenha apenas os grupos aplicáveis à operação. As regras de negócio do ambiente nacional também se aplicam.

## Visão geral

| Alteração no schema | O que muda no XML |
|---|---|
| Mudança de posição de `indDest` e `dest` | Os dados do destinatário passam para `DPS/infDPS`, logo após `toma`. |
| Novo leiaute de `comExt` | Entram dados de fatura, DUIMP, DU-E e contrato de câmbio. Mudam os códigos de `mdPrestacao`. |
| Novo `cMun` em `atvEvento` | Identifica o município do evento nos casos previstos no schema. |
| Novo `vAjusteBCIBSCBSComExt` | Permite ajuste com sinal na base de IBS/CBS em comércio exterior. |
| Novo grupo `condominios` | Detalha vencimento, cobranças e descontos. Os valores da DPS refletem as somas correspondentes. |
| NFS-e de retorno | Entram `verCalcIBSCBS` e o ajuste de comércio exterior na fórmula de `vBC`. |

Os trechos deste documento são fragmentos de XML. O arquivo de referência contém a DPS completa.

## 1. Destinatário — mudança de posição de indDest e dest

**Antes**, os dados ficavam em `DPS/infDPS/IBSCBS`.

```xml
<IBSCBS>
  <!-- indFinal, cIndOp e demais campos anteriores -->
  <indDest>1</indDest>
  <dest>
    <CNPJ>11111111000191</CNPJ>
    <xNome>DESTINATARIO TESTE</xNome>
  </dest>
  <!-- imovel, bensMoveis e demais campos posteriores -->
</IBSCBS>
```

**Agora**, ficam diretamente em `DPS/infDPS`, após `toma` e antes de `interm` ou `serv`.

```xml
<infDPS>
  <!-- Dados da DPS até toma -->
  <indDest>1</indDest>
  <dest>
    <CNPJ>11111111000191</CNPJ>
    <xNome>DESTINATARIO TESTE</xNome>
  </dest>
  <!-- interm, quando informado, e serv -->
</infDPS>
```

- `indDest=0` identifica o próprio tomador ou adquirente. `indDest=1` identifica outro destinatário ou estabelecimento, como no exemplo.
- O schema nacional atualizado exige `indDest` na nova posição. `dest` é opcional no XSD. Ao informar `dest`, preencha o nome e uma identificação entre `CNPJ`, `CPF`, `NIF` e `cNaoNIF`.
- Durante a transição, a Unimake.DFe aceita que as duas tags sejam omitidas em ambas as posições. Para gerar no leiaute novo, preencha `InfDPS.IndDest` e, quando aplicável, `InfDPS.Dest`. Para gerar na posição anterior, use `IBSCBS.IndDest` e `IBSCBS.Dest`, propriedades obsoletas que geram aviso de compilação. Escolha uma posição para o preenchimento.

## 2. Comércio exterior — novo leiaute de comExt

Caminho `DPS/infDPS/serv/comExt`. O grupo continua opcional. Este fragmento mostra a sequência completa do novo leiaute.

```xml
<comExt>
  <mdPrestacao>5</mdPrestacao>
  <vincPrest>0</vincPrest>
  <tpMoeda>220</tpMoeda>
  <nFatura>000123</nFatura>
  <vFatura>300.50</vFatura>
  <mecAFComexP>01</mecAFComexP>
  <mecAFComexT>01</mecAFComexT>
  <movTempBens>2</movTempBens>
  <nDUIMP>26BR00000000001</nDUIMP>
  <!-- nDUE é mostrada junto da DUIMP apenas para localizar as duas tags no XML. -->
  <nDUE>26BR0000000001</nDUE>
  <mdic>0</mdic>
  <nContCambio>000456</nContCambio>
</comExt>
<!-- cMun se aplica aos subitens 12.13 e 17.10 quando o evento ocorre em outro município. -->
<atvEvento>
  <xNome>EVENTO TESTE</xNome>
  <dtIni>2026-11-26</dtIni>
  <dtFim>2026-11-27</dtFim>
  <cMun>4106902</cMun>
  <idAtvEvt>EVENTO-001</idAtvEvt>
</atvEvento>
```

| Tag nova | Formato e preenchimento |
|---|---|
| `nFatura` | Opcional. Número da fatura ou do documento que comprova a transação, com 1 a 20 dígitos. O schema orienta o preenchimento quando houver exigência de documento fiscal para a importação do serviço. |
| `vFatura` | Opcional. Valor total da fatura ou do documento na moeda indicada em `tpMoeda`. |
| `nDUIMP` | Opcional no XSD. Número da Declaração Única de Importação, com 15 caracteres, aplicável à cobrança de serviço de importação de bem material. |
| `nDUE` | Opcional no XSD. Número da Declaração Única de Exportação, com 14 caracteres, aplicável à cobrança de serviço de exportação de bem material. |
| `nContCambio` | Opcional. Número do contrato de câmbio ou do documento que comprova o pagamento, com 1 a 20 dígitos. |

Os campos `vServMoeda`, `nDI` e `nRE` saíram do novo leiaute. O exemplo mostra `nDUIMP` e `nDUE` no mesmo fragmento para localizar as duas tags. Preencha a declaração correspondente à operação.

### Códigos de mdPrestacao

| Código no XML | Leiaute anterior | Leiaute de 24/11/2026 |
|---|---|---|
| `0` | Desconhecido | Desconhecido. Uso previsto no compartilhamento municipal com o ADN NFS-e. |
| `1` | Transfronteiriço | Transfronteiriço |
| `2` | Consumo no Brasil | Consumo no Brasil |
| `3` | Presença comercial no exterior | Movimento temporário de pessoas físicas |
| `4` | Movimento temporário de pessoas físicas | Consumo no exterior |
| `5` | Código indisponível | Consumo concomitante no território nacional e no exterior |

- Com `mdPrestacao=5`, informe em `vServ` apenas a parcela consumida no Brasil na importação. Na exportação, informe apenas a parcela exportada e emita uma NFS-e regular para a parcela consumida no Brasil.
- Em `tpMoeda`, use o código de três dígitos da moeda vigente na data de competência, conforme a tabela do Banco Central do Brasil.
- `mecAFComexP` e `mecAFComexT` referem-se aos mecanismos de apoio do prestador e do tomador, respectivamente. O código `00` é reservado ao compartilhamento municipal com o ADN NFS-e. A mesma restrição se aplica ao código `0` de `movTempBens`.
- Em `movTempBens`, os códigos `2` e `3` passam a identificar vínculo com DUIMP e DU-E, respectivamente. `mdic` indica se a NFS-e deve ser disponibilizada ao MDIC (`0` para não e `1` para sim).

### Compatibilidade com o leiaute anterior na Unimake.DFe

- Ao gerar um novo objeto `ComExt`, use `UsarLeiauteLegado = true` para o leiaute anterior ou `false` para o atualizado. No anterior, preencha `vServMoeda` explicitamente, inclusive quando o valor for `0.00`. `nDI` e `nRE` continuam opcionais nesse leiaute.
- Na leitura, a DLL reconhece o formato anterior pela presença de `vServMoeda` no XML de origem. O preenchimento posterior de `vServMoedaField` preserva a interpretação do código lido. `UsarLeiauteLegadoNaLeitura` permite selecionar explicitamente a interpretação quando necessário.
- O serviço rejeita XML com mistura de campos dos dois leiautes de comércio exterior, `mdPrestacao=5` com o formato anterior ou `vAjusteBCIBSCBSComExt` com `comExt` anterior.

## 3. Atividade de evento — município do evento

Caminho `DPS/infDPS/serv/atvEvento/cMun`. O campo é opcional no XSD e recebe o código IBGE de sete dígitos. Informe-o para serviços dos subitens 12.13 e 17.10 quando o município do evento for diferente do município da prestação.

```xml
<atvEvento>
  <xNome>EVENTO TESTE</xNome>
  <dtIni>2026-11-26</dtIni>
  <dtFim>2026-11-27</dtFim>
  <cMun>4106902</cMun>
  <idAtvEvt>EVENTO-001</idAtvEvt>
</atvEvento>
```

A sequência é `dtFim`, `cMun` e, em seguida, a opção escolhida entre `idAtvEvt` e `end`.

## 4. Ajuste da base do IBS/CBS no comércio exterior

Caminho `DPS/infDPS/valores/vAjusteBC/vAjusteBCIBSCBSComExt`. Tag opcional, após `documentos` quando esse grupo estiver presente. O XSD admite valor positivo, negativo ou zero, com até 15 dígitos na parte inteira e duas casas quando houver parte decimal.

```xml
<vAjusteBC>
  <!-- Ajuste de ISSQN e documentos, quando informados -->
  <vAjusteBCIBSCBSComExt>-50.00</vAjusteBCIBSCBSComExt>
</vAjusteBC>
```

O valor entra com o próprio sinal no cálculo de `NFSe/infNFSe/IBSCBS/valores/vBC`. O schema indica a soma do ajuste tanto na fórmula até 2026, que desconta PIS e COFINS, quanto na fórmula de 2027 a 2032.

## 5. Condomínios — vencimento, cobranças e descontos

Caminho `DPS/infDPS/IBSCBS/condominios`. O grupo é opcional e se aplica às atividades de condomínio do subitem 99.05 de `cTribNac`. Na sequência de `IBSCBS`, vem após `imovel` e `bensMoveis`, quando presentes, e antes de `valores`.

```xml
<condominios>
  <dVencOrig>2026-11-30</dVencOrig>
  <gCobranca>
    <tpCobranca>99</tpCobranca>
    <xTpCobranca>OUTRA COBRANCA TESTE</xTpCobranca>
    <vCobranca>1500.00</vCobranca>
    <gDetCobranca>
      <tpDetCobranca>999</tpDetCobranca>
      <xTpDetCobranca>OUTRO DETALHE TESTE</xTpDetCobranca>
      <vDetCobranca>1500.00</vDetCobranca>
    </gDetCobranca>
  </gCobranca>
  <gDesconto>
    <tpDesconto>1</tpDesconto>
    <vDesconto>10.00</vDesconto>
    <xTpDesconto>DESCONTO INCONDICIONADO TESTE</xTpDesconto>
  </gDesconto>
  <gDesconto>
    <tpDesconto>2</tpDesconto>
    <vDesconto>5.00</vDesconto>
    <xTpDesconto>DESCONTO CONDICIONADO TESTE</xTpDesconto>
  </gDesconto>
</condominios>
```

| Elemento | Preenchimento |
|---|---|
| `dVencOrig` | Obrigatório no grupo. Vencimento original no formato AAAA-MM-DD. |
| `gCobranca` | De 1 a 20 ocorrências. Cada cobrança exige `tpCobranca` e `vCobranca`. Informe `xTpCobranca` somente para o tipo `99`. |
| `gDetCobranca` | De 0 a 20 ocorrências por cobrança. Cada detalhe exige `tpDetCobranca` e `vDetCobranca`. Informe `xTpDetCobranca` somente para o tipo `999`. Quando houver detalhamento, `vCobranca` corresponde à soma de `vDetCobranca`. |
| `gDesconto` | De 0 a 10 ocorrências. Cada desconto exige `tpDesconto`, `vDesconto` e `xTpDesconto`. Use `1` para incondicionado e `2` para condicionado. |

### Códigos de cobrança e de detalhamento

| `tpCobranca` | Descrição | `tpDetCobranca` admitidos quando houver detalhamento |
|---|---|---|
| `01` | Condomínio ordinário | `001` a `006`, `018`, `035`, `036`, `999` |
| `02` | Condomínio privativo | `007` a `014`, `017` a `019`, `023`, `025` a `027`, `029`, `031` a `034`, `999` |
| `03` | Fundo de Promoção e Propaganda (FPP) | `018`, `028`, `999` |
| `04` | Impostos e taxas | `015`, `016`, `020` a `022`, `024`, `999` |
| `05` | Fundo de reserva | `018`, `030`, `999` |
| `99` | Outra cobrança | Qualquer código do domínio `TSRTCTpDetCobranca` (`001` a `036` ou `999`) |

Preserve os zeros à esquerda nos códigos de cobrança e de detalhamento.

### Totais correspondentes em valores

Para o subitem 99.05, `vServPrest/vServ` é a soma de `vCobranca`. Em `vDescCondIncond`, `vDescIncond` soma os `vDesconto` do tipo `1` e `vDescCond` soma os do tipo `2`.

```xml
<valores>
  <vServPrest>
    <vServ>1500.00</vServ>
  </vServPrest>
  <vDescCondIncond>
    <vDescIncond>10.00</vDescIncond>
    <vDescCond>5.00</vDescCond>
  </vDescCondIncond>
  <!-- vAjusteBC, quando informado, e trib -->
</valores>
```

## 6. NFS-e de retorno — versão da calculadora de IBS/CBS

Caminho `NFSe/infNFSe/verCalcIBSCBS`. A tag é opcional e fica entre `ambGer` e `tpEmis`. Ela identifica a versão da calculadora usada pela SEFIN Nacional nas notas autorizadas ou geradas pelos emissores públicos nacionais (`ambGer=2)). Nas notas compartilhadas pelos municípios (`ambGer=1`), a tag deve ser omitida.

```xml
<infNFSe>
  <!-- Dados anteriores do retorno -->
  <ambGer>2</ambGer>
  <verCalcIBSCBS>1.00</verCalcIBSCBS>
  <tpEmis>1</tpEmis>
  <!-- Demais dados do retorno -->
</infNFSe>
```

## 7. Outras orientações acrescentadas aos schemas

- O schema da NFS-e passou a indicar que os grupos de IBS/CBS são obrigatórios para optantes do Simples Nacional a partir de 2027.
- Os códigos de `CST` e `cClassTrib` do IBS/CBS seguem os Informes Técnicos. A atualização registra a referência dessas tabelas nas anotações.
- A anotação do atributo `NFSe/infNFSe/@Id` esclarece que o dígito verificador continua usando módulo 11 para o CNPJ alfanumérico, tratando os caracteres pela sua representação numérica. O formato do identificador permanece com 53 posições.

## Conferência rápida para o suporte

- A versão do XML continua `1.01`. Escolha o leiaute de comércio exterior conforme o ambiente de destino.
- Para o leiaute atualizado, coloque `indDest` e `dest` diretamente em `infDPS` e use o domínio atualizado de `mdPrestacao`.
- Verifique a condição de uso de `atvEvento/cMun` e os totais de `condominios` antes do envio.
- Procure `verCalcIBSCBS` na NFS-e de retorno, no ambiente gerador nacional.
