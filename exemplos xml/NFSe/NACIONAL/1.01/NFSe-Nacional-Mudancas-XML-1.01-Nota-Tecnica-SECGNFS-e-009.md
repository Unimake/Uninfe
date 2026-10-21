# NFSe Nacional 1.01 — mudanças no XML para orientação do suporte

Este material detalha as alterações na estrutura XML da DPS e da NFS-e de retorno, com exemplos para orientar o suporte e os integradores.

Modelo de referência: **GerarNfseNovasTagsDPSIBSCBS-env-loterps.xml**, disponível em `exemplos xml/NFSe/NACIONAL/1.01` do UniNFe. O arquivo foi validado contra o `DPS_v1.01.xsd` atualizado.

O modelo usa dados fictícios e reúne grupos para demonstração. Na utilização, devem ser mantidos somente os grupos aplicáveis à operação, com os códigos, valores e documentos correspondentes.

## Visão geral

| Alteração no schema | O que muda no XML |
|---|---|
| `finNFSe` saiu de `IBSCBS` e passou para `infDPS` | Informar a finalidade diretamente em `DPS/infDPS`, na posição correta. |
| Mudou a sequência de `cServ` | Informar `xDescServ` depois de `cNBS` e `cIntContrib`, quando estes existirem. |
| Novo grupo `vAjusteBC` | Utilizar a estrutura de ajustes da base de cálculo, com valores e/ou documentos, em lugar dos grupos antigos removidos. |
| CST e classificação tributária mudaram de nível | Informar `CST` e `cClassTrib` diretamente em `IBSCBS/valores/trib`, fora de `gIBSCBS`. |
| Nova estrutura de imóveis | Informar município, locação e unidades imobiliárias; os dados de identificação e endereço ficam por unidade. |
| Novo grupo `bensMoveis` | Informar NCM, descrição e quantidade por ocorrência. |
| Novos valores calculados e campos opcionais | Observar as novas tags da NFS-e de retorno e omitir os elementos opcionais não informados. |

Os trechos deste documento são **fragmentos de XML**, para destacar os caminhos e a ordem. O modelo de referência contém a DPS completa.

## 1. Finalidade da NFS-e: mudança de posição de finNFSe

**Antes:**

```xml
<infDPS>
  <!-- Demais grupos da DPS -->
  <IBSCBS>
    <finNFSe>0</finNFSe>
    <!-- Demais informações de IBS/CBS -->
  </IBSCBS>
</infDPS>
```

**Agora:**

```xml
<infDPS>
  <!-- tpAmb, dhEmi, verAplic, serie, nDPS e dCompet -->
  <tpEmit>1</tpEmit>
  <!-- cMotivoEmisTI, quando informado, fica aqui -->
  <finNFSe>0</finNFSe>
  <!-- chNFSeRej, quando informado, fica aqui -->
  <cLocEmi>4105508</cLocEmi>
  <!-- Demais grupos da DPS -->
</infDPS>
```

Orientações:

- Caminho novo: `DPS/infDPS/finNFSe`.
- A tag é obrigatória no schema atualizado, inclusive quando o grupo `IBSCBS` não for informado.
- O domínio atual é `0` — NFS-e regular.
- A sequência é `tpEmit → cMotivoEmisTI? → finNFSe → chNFSeRej? → cLocEmi`. O símbolo `?` indica elemento opcional.
- Não manter outra ocorrência de `finNFSe` dentro de `IBSCBS`.

## 2. Código do serviço: nova ordem em cServ

O grupo continua no caminho `DPS/infDPS/serv/cServ`. Os nomes das tags permanecem iguais; mudou sua sequência.

| Ordem anterior | Ordem atual |
|---|---|
| cTribNac | cTribNac |
| cTribMun, opcional | cTribMun, opcional |
| xDescServ | cNBS, opcional |
| cNBS, opcional | cIntContrib, opcional |
| cIntContrib, opcional | xDescServ |

**Exemplo na ordem atual:**

```xml
<cServ>
  <cTribNac>010101</cTribNac>
  <cNBS>115022000</cNBS>
  <cIntContrib>SERVICOTESTE</cIntContrib>
  <xDescServ>DESCRICAO DO SERVICO TESTE</xDescServ>
</cServ>
```

Se `cTribMun` for utilizado, ele deve ficar imediatamente depois de `cTribNac`. Omitir uma tag opcional não altera a ordem das restantes.

Uma rejeição indicando que `cNBS` ou `cIntContrib` apareceu em posição inválida pode decorrer de `xDescServ` ainda estar na posição antiga.

## 3. Ajustes da base de cálculo: novo grupo vAjusteBC

O grupo novo fica em:

```text
DPS/infDPS/valores/vAjusteBC
```

O schema removeu a estrutura `valores/vDedRed` e o grupo `IBSCBS/valores/gReeRepRes`. A nova estrutura documental de ajustes fica em:

```text
DPS/infDPS/valores/vAjusteBC/documentos/docAjusteBC
```

Não basta trocar o nome das tags antigas: o conteúdo, a ordem e os códigos de classificação dos documentos mudaram.

### Percentual, valor e documentos

```xml
<vAjusteBC>
  <pAjusteBCISSQN>10.00</pAjusteBCISSQN>
  <documentos>
    <docAjusteBC>
      <tpAjusteBC>2</tpAjusteBC>
      <vTotDoc>100.00</vTotDoc>
      <vAjusteAplic>50.00</vAjusteAplic>
      <docOutro>
        <nDoc>1</nDoc>
        <xDoc>DOCUMENTO TESTE</xDoc>
      </docOutro>
    </docAjusteBC>
  </documentos>
</vAjusteBC>
```

Regras de estrutura:

- `vAjusteBC` é opcional.
- Informar **no máximo uma** das tags `pAjusteBCISSQN` e `vAjusteBCISSQN`. O schema também permite omitir ambas.
- Para usar valor, substituir o percentual por `<vAjusteBCISSQN>150.00</vAjusteBCISSQN>`.
- `documentos` é opcional e pode coexistir com o percentual ou o valor escolhido.
- Quando `documentos` existir, deve conter de **1 a 1000** ocorrências de `docAjusteBC`.

### Ordem dentro de docAjusteBC

| Posição | Tag ou grupo | Ocorrência |
|---|---|---|
| 1 | tpAjusteBC | Obrigatório |
| 2 | xTpAjusteBC | Opcional |
| 3 | vTotDoc | Obrigatório |
| 4 | vAjusteAplic | Obrigatório |
| 5 | dtEmiDoc | Opcional |
| 6 | dtCompDoc | Opcional |
| 7 | dFeNacional **ou** docFiscalOutro **ou** docOutro | Exatamente uma alternativa |
| 8 | fornec | Opcional |

As datas usam o formato `AAAA-MM-DD`.

**Exemplo com datas e fornecedor:**

```xml
<docAjusteBC>
  <tpAjusteBC>199</tpAjusteBC>
  <xTpAjusteBC>OUTRO REEMBOLSO TESTE</xTpAjusteBC>
  <vTotDoc>100.00</vTotDoc>
  <vAjusteAplic>50.00</vAjusteAplic>
  <dtEmiDoc>2026-10-20</dtEmiDoc>
  <dtCompDoc>2026-10-01</dtCompDoc>
  <docOutro>
    <nDoc>1</nDoc>
    <xDoc>DOCUMENTO TESTE</xDoc>
  </docOutro>
  <fornec>
    <CNPJ>12345678000195</CNPJ>
  </fornec>
</docAjusteBC>
```

### Alternativas de identificação do documento

| Alternativa | Conteúdo |
|---|---|
| dFeNacional | tipoChaveDFe, xTipoChaveDFe opcional e chaveDFe |
| docFiscalOutro | cMunDocFiscal, nDocFiscal e xDocFiscal |
| docOutro | nDoc e xDoc |

Cada ocorrência de `docAjusteBC` deve usar somente uma alternativa. O modelo completo demonstra as três alternativas em documentos separados.

### Códigos de tpAjusteBC

| Código XML | Descrição |
|---|---|
| 2 | Materiais |
| 5 | Repasse consorciado |
| 6 | Repasse de plano de saúde |
| 7 | Serviços |
| 8 | Subempreitada de mão de obra |
| 9 | Profissional parceiro |
| 99 | Outras deduções exclusivas do ISSQN |
| 101 | Repasse de remuneração por intermediação de imóveis a demais corretores envolvidos |
| 102 | Repasse a fornecedor por fornecimento intermediado por agência de turismo |
| 103 | Reembolso ou ressarcimento de produção externa por agência de propaganda e publicidade |
| 104 | Reembolso ou ressarcimento de mídia por agência de propaganda e publicidade |
| 105 | Glosa de serviços de saúde |
| 199 | Outros reembolsos ou ressarcimentos por operações por conta e ordem de terceiro |

Os códigos antigos de `tpDedRed` e `tpReeRepRes` não devem ser copiados automaticamente: é necessário selecionar o código correspondente ao significado no domínio novo.

## 4. CST e classificação tributária: novo nível

**Antes:**

```xml
<valores>
  <trib>
    <gIBSCBS>
      <CST>000</CST>
      <cClassTrib>000001</cClassTrib>
    </gIBSCBS>
  </trib>
</valores>
```

**Agora, dentro de IBSCBS:**

```xml
<valores>
  <trib>
    <CST>000</CST>
    <cClassTrib>000001</cClassTrib>
  </trib>
</valores>
```

Caminhos novos:

- `DPS/infDPS/IBSCBS/valores/trib/CST`
- `DPS/infDPS/IBSCBS/valores/trib/cClassTrib`

O grupo `gIBSCBS` passou a ser opcional. Quando utilizado, fica **depois de cClassTrib** e contém os complementos `cCredPres`, `gTribRegular` e `gDif`, conforme aplicáveis.

```xml
<trib>
  <CST>000</CST>
  <cClassTrib>000001</cClassTrib>
  <gIBSCBS>
    <cCredPres>01</cCredPres>
  </gIBSCBS>
</trib>
```

Não manter `CST` ou `cClassTrib` dentro de `gIBSCBS`. Preservar os zeros à esquerda dos códigos.

## 5. Imóveis: município, locação e unidades imobiliárias

O grupo continua em `DPS/infDPS/IBSCBS/imovel`, mas seu conteúdo mudou.

**Estrutura anterior:**

```text
imovel
  inscImobFisc?
  cCIB OU end
```

**Estrutura atual:**

```text
imovel
  cMun
  gLocacao?
  gUnidImob* — até 99 unidades
    inscImobFisc?
    cCIB OU end
    gAjusteBCLocImoveis* — até 1000 ajustes por unidade
```

O símbolo `*` indica repetição opcional.

**Exemplo:**

```xml
<imovel>
  <cMun>4106902</cMun>
  <gLocacao>
    <pCopropriedade>50.00</pCopropriedade>
    <vTotOper>1500.00</vTotOper>
    <vDescIncondTot>10.00</vDescIncondTot>
    <vDescCondTot>5.00</vDescCondTot>
    <dVencOrig>2026-11-01</dVencOrig>
  </gLocacao>
  <gUnidImob>
    <inscImobFisc>000123</inscImobFisc>
    <cCIB>12345678</cCIB>
    <gAjusteBCLocImoveis>
      <tpAjusteBCLocImoveis>99</tpAjusteBCLocImoveis>
      <xTpAjusteBCLocImoveis>OUTRA PARCELA TESTE</xTpAjusteBCLocImoveis>
      <vAjusteBCLocImoveis>5.00</vAjusteBCLocImoveis>
    </gAjusteBCLocImoveis>
  </gUnidImob>
</imovel>
```

Orientações:

- `cMun` é obrigatório quando `imovel` existir.
- `gLocacao` é opcional. Quando existir, `pCopropriedade` e `vTotOper` são obrigatórios; descontos e vencimento são opcionais.
- Cada `gUnidImob` deve conter **cCIB ou end**, nunca ambos.
- A inscrição imobiliária pertence à unidade e deve preservar zeros à esquerda.
- O novo `end` contém `CEP`, `xLgr`, `nro`, `xCpl?` e `xBairro?`, nessa ordem. Esse endereço não possui a alternativa `endExt`.

**Unidade identificada pelo endereço:**

```xml
<gUnidImob>
  <end>
    <CEP>80000000</CEP>
    <xLgr>RUA EXEMPLO</xLgr>
    <nro>1</nro>
    <xCpl>SALA TESTE</xCpl>
    <xBairro>BAIRRO TESTE</xBairro>
  </end>
</gUnidImob>
```

### Códigos de tpAjusteBCLocImoveis

| Código XML | Parcela não integrante da base de cálculo |
|---|---|
| 01 | Tributos inclusos no aluguel ou equivalente |
| 02 | Emolumento incluso no aluguel ou equivalente |
| 03 | Condomínio incluso no aluguel ou equivalente |
| 04 | Redutor social |
| 99 | Outras parcelas inclusas no aluguel ou equivalente |

Usar dois dígitos: `01`, `02`, `03` e `04`, preservando o zero inicial. A aplicação de cada tipo deve observar o enquadramento da operação; no XSD, as descrições dos códigos 01 a 03 referenciam as NBS 1.1002.10.00 e 1.1002.20.00, e o código 04 referencia a NBS 1.1002.10.00.

## 6. Bens móveis: novo grupo bensMoveis

Caminho:

```text
DPS/infDPS/IBSCBS/bensMoveis
```

O grupo é opcional, pode repetir até **1000 vezes** e aparece **depois de imovel e antes de valores**, quando esses grupos estiverem presentes.

```xml
<bensMoveis>
  <cNCMBemMovel>01012100</cNCMBemMovel>
  <xNCMBemMovel>BEM TESTE</xNCMBemMovel>
  <qtdNCMBemMovel>1</qtdNCMBemMovel>
</bensMoveis>
```

As três tags são obrigatórias por ocorrência:

| Tag | Conteúdo |
|---|---|
| cNCMBemMovel | Código NCM com oito dígitos; preservar zeros à esquerda |
| xNCMBemMovel | Descrição do bem |
| qtdNCMBemMovel | Quantidade, conforme o tipo numérico de até três dígitos do schema |

Para vários itens, repetir `bensMoveis`. Não criar um grupo intermediário de lista.

## 7. Valores calculados e campos que passaram a opcionais

### Novos valores calculados da NFS-e de retorno

**Estas tags pertencem à raiz NFSe, não à DPS de envio.** Por isso, elas não aparecem no modelo `-env-loterps.xml`.

| Caminho na NFS-e de retorno | Mudança |
|---|---|
| NFSe/infNFSe/valores/vCalcAjusteBCISSQN | Novo valor calculado de ajuste da base do ISSQN; a tag antiga vCalcDR foi removida |
| NFSe/infNFSe/IBSCBS/valores/vCalcAjusteBCLocImoveis | Novo valor calculado de ajuste da base relativo à locação de imóveis |
| NFSe/infNFSe/IBSCBS/valores/vCalcAjusteBCIBSCBS | Novo valor calculado de ajuste da base do IBS/CBS; a tag antiga vCalcReeRepRes foi removida |

Em `NFSe/infNFSe/IBSCBS/valores`, a ordem é:

```xml
<valores>
  <vBC>1000.00</vBC>
  <vCalcAjusteBCLocImoveis>30.00</vCalcAjusteBCLocImoveis>
  <vCalcAjusteBCIBSCBS>70.00</vCalcAjusteBCIBSCBS>
  <!-- uf, mun e fed, nessa ordem -->
</valores>
```

As três tags numéricas exibidas acima são opcionais nesse grupo. Os valores são ilustrativos; não representam uma fórmula de cálculo ou uma autorização de reutilização dos valores antigos.

### Opcionais alterados

| Contexto | Elementos que passaram a opcionais |
|---|---|
| DPS/infDPS/IBSCBS | cIndOp |
| DPS/infDPS/IBSCBS/valores/trib | gIBSCBS |
| Grupos do tipo TCInfoPessoa | xNome |
| NFSe/infNFSe/IBSCBS | cLocalidadeIncid, xLocalidadeIncid e valores |
| NFSe/infNFSe/IBSCBS/valores | vBC |
| NFSe/infNFSe/IBSCBS/totCIBS | gIBS e gCBS |

A mudança de `xNome` se refere aos grupos que usam `TCInfoPessoa`; não significa que o nome se tornou opcional em todos os grupos de pessoas do documento.

Quando um elemento opcional não tiver sido informado, ele pode ser omitido. **Ausência, elemento vazio e valor zero são situações diferentes.** Não preencher automaticamente uma tag opcional com zero ou emitir `xsi:nil` apenas porque ela passou a ser opcional. Quando houver um valor zero efetivamente informado, respeitar o formato e as restrições da tag correspondente.

## Conferência rápida para o suporte

1. Conferir `finNFSe` diretamente em `infDPS`, na posição correta.
2. Conferir a ordem `cTribNac → cTribMun? → cNBS? → cIntContrib? → xDescServ`.
3. Substituir a estrutura antiga de dedução/reembolso pelos grupos e códigos novos de `vAjusteBC`.
4. Conferir `CST` e `cClassTrib` diretamente em `trib`.
5. Conferir `cMun` e os dados de identificação/endereço dentro de cada `gUnidImob`.
6. Conferir as alternativas exclusivas de documento e de identificação do imóvel.
7. Preservar zeros à esquerda de códigos e inscrições.
8. Não inserir na DPS as tags de valores calculados que pertencem à NFS-e de retorno.
9. Validar o XML completo contra o schema atualizado da raiz correspondente.
