using NFe.ConvertTxt;
using NFe.ConvertTxt.Mapping;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Xunit;
using System;

namespace UniNFe.Test.NFeConvertTxt
{
    public sealed class NFeDFeProductMapperTests
    {
        [Fact]
        public void MapearDevePreservarCamposBasicosDoProduto()
        {
            var nota = CriarNotaComProduto();

            var detalhe = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0];

            Assert.Equal(1, detalhe.NItem);
            Assert.Equal("PROD-1", detalhe.Prod.CProd);
            Assert.Equal("SEM GTIN", detalhe.Prod.CEAN);
            Assert.Equal("PRODUTO DE TESTE", detalhe.Prod.XProd);
            Assert.Equal("12345678", detalhe.Prod.NCM);
            Assert.Equal("0123456", detalhe.Prod.CEST);
            Assert.Equal(2.5m, detalhe.Prod.QCom);
            Assert.Equal(10.1234567890m, detalhe.Prod.VUnCom);
            Assert.Equal(25.31, detalhe.Prod.VProd);
            Assert.Equal(SimNao.Sim, detalhe.Prod.IndTot);
        }

        [Fact]
        public void MapearDeveManterCamposOpcionaisAusentes()
        {
            var produto = new NFeDFeMapper().Mapear(CriarNotaComProduto()).InfNFeField.Det[0].Prod;

            Assert.Null(produto.CBarra);
            Assert.Null(produto.NVE);
            Assert.Null(produto.CNPJFab);
            Assert.Null(produto.CBenef);
            Assert.Null(produto.GCred);
            Assert.Null(produto.TpCredPresIBSZFM);
            Assert.Null(produto.XPed);
            Assert.Null(produto.NItemPed);
            Assert.Null(produto.NFCI);
        }

        [Fact]
        public void MapearDeveAplicarDescricaoDeHomologacaoNoPrimeiroItemDaNFCe()
        {
            var nota = CriarNotaComProduto();
            nota.ide.mod = TpcnMod.modNFCe;
            nota.ide.tpAmb = TipoAmbiente.Homologacao;

            var produto = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Prod;

            Assert.Equal("NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL", produto.XProd);
        }

        [Fact]
        public void ProdutoBasicoDeveSerSerializadoPelaDll()
        {
            var modelo = new NFeDFeMapper().Mapear(CriarNotaComProduto());

            var xml = XMLUtility.Serializar(modelo);

            Assert.Equal("1", xml.GetElementsByTagName("det")[0].Attributes["nItem"].Value);
            Assert.Equal("PROD-1", xml.GetElementsByTagName("cProd")[0].InnerText);
            Assert.Equal("2.5000", xml.GetElementsByTagName("qCom")[0].InnerText);
            Assert.Equal("10.1234567890", xml.GetElementsByTagName("vUnCom")[0].InnerText);
        }

        [Fact]
        public void MapearDevePreservarImportacaoExportacaoRastroEDFeReferenciado()
        {
            var nota = CriarNotaComProduto();
            var produto = nota.det[0].Prod;
            var di = new DI
            {
                nDI = "DI123",
                dDI = new DateTime(2026, 7, 1),
                xLocDesemb = "PARANAGUA",
                UFDesemb = "PR",
                dDesemb = new DateTime(2026, 7, 2),
                tpViaTransp = TpcnTipoViaTransp.tvMaritima,
                vAFRMM = 12.34,
                tpIntermedio = TpcnTipoIntermedio.tiContaPropria,
                cExportador = "EXP1"
            };
            di.adi.Add(new Adi { nAdicao = 1, nSeqAdi = 1, cFabricante = "FAB", vDescDI = 2.50 });
            produto.DI.Add(di);
            produto.detExport.Add(new detExport
            {
                nDraw = "DRAW1",
                exportInd = new exportInd { nRE = "RE1", chNFe = "41123456789012345678901234567890123456789012", qExport = 1.2500 }
            });
            produto.rastro.Add(new Rastro
            {
                nLote = "LOTE1",
                qLote = 2.500,
                dFab = new DateTime(2026, 6, 1),
                dVal = new DateTime(2027, 6, 1),
                cAgreg = "AG1"
            });
            nota.det[0].DfeReferenciado.chaveAcesso = "41123456789012345678901234567890123456789013";
            nota.det[0].DfeReferenciado.nItem = 2;

            var detalhe = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0];

            Assert.Single(detalhe.Prod.DI);
            Assert.Single(detalhe.Prod.DI[0].Adi);
            Assert.Single(detalhe.Prod.DetExport);
            Assert.Single(detalhe.Prod.Rastro);
            Assert.Equal("41123456789012345678901234567890123456789013", detalhe.DFeReferenciado.ChaveAcesso);
            Assert.Equal("2", detalhe.DFeReferenciado.NItem);
        }

        [Fact]
        public void MapearDevePreservarMedicamentoEArmas()
        {
            var nota = CriarNotaComProduto();
            nota.det[0].Prod.med.Add(new Med
            {
                cProdANVISA = "ISENTO",
                xMotivoIsencao = "RDC 123",
                vPMC = 10.50
            });
            nota.det[0].Prod.arma.Add(new Arma
            {
                tpArma = TpcnTipoArma.taUsoPermitido,
                nSerie = "SERIE1",
                nCano = "CANO1",
                descr = "ARMA DE TESTE"
            });

            var produto = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Prod;

            Assert.Equal("ISENTO", produto.Med.CProdANVISA);
            Assert.Equal("RDC 123", produto.Med.XMotivoIsencao);
            Assert.Single(produto.Arma);
            Assert.Equal("SERIE1", produto.Arma[0].NSerie);
        }

        [Fact]
        public void MapearDeveRejeitarMaisDeUmMedicamentoPoisDllSuportaGrupoUnico()
        {
            var nota = CriarNotaComProduto();
            nota.det[0].Prod.med.Add(new Med { cProdANVISA = "1" });
            nota.det[0].Prod.med.Add(new Med { cProdANVISA = "2" });

            var exception = Assert.Throws<NotSupportedException>(() => new NFeDFeMapper().Mapear(nota));

            Assert.Contains("somente um grupo med", exception.Message);
        }

        [Fact]
        public void MapearDevePreservarCombustivelESeusSubgrupos()
        {
            var nota = CriarNotaComProduto();
            nota.det[0].Prod.comb = new Comb
            {
                cProdANP = 210203001,
                descANP = "GLP",
                pGLP = 50,
                pGNn = 25,
                pGNi = 25,
                vPart = 5.50,
                qTemp = 10.2500,
                UFCons = "PR",
                pBio = 14,
                CIDE = new CIDE { qBCprod = 10, vAliqProd = 0.10, vCIDE = 1 },
                encerrante = new Encerrante { nBico = 1, nTanque = 2, vEncIni = "100.000", vEncFin = "110.000" }
            };
            nota.det[0].Prod.comb.origComb.Add(new OrigComb { indImport = 0, cUFOrig = 41, pOrig = 100 });

            var combustivel = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Prod.Comb[0];

            Assert.Equal("210203001", combustivel.CProdANP);
            Assert.NotNull(combustivel.CIDE);
            Assert.NotNull(combustivel.Encerrante);
            Assert.Single(combustivel.OrigComb);
            Assert.Equal(110, combustivel.Encerrante.VEncFin);
        }

        internal static NFe.ConvertTxt.NFe CriarNotaComProduto()
        {
            var nota = NFeDFeMapperTests.CriarNotaBase();
            var detalhe = new Det();
            detalhe.Prod.nItem = 1;
            detalhe.Prod.cProd = "PROD-1";
            detalhe.Prod.cEAN = "SEM GTIN";
            detalhe.Prod.xProd = "PRODUTO DE TESTE";
            detalhe.Prod.NCM = "12345678";
            detalhe.Prod.CEST = 123456;
            detalhe.Prod.CFOP = "5102";
            detalhe.Prod.uCom = "UN";
            detalhe.Prod.qCom = 2.5000m;
            detalhe.Prod.vUnCom = 10.1234567890m;
            detalhe.Prod.vProd = 25.31;
            detalhe.Prod.cEANTrib = "SEM GTIN";
            detalhe.Prod.uTrib = "UN";
            detalhe.Prod.qTrib = 2.5000m;
            detalhe.Prod.vUnTrib = 10.1234567890m;
            detalhe.Prod.indTot = TpcnIndicadorTotal.itSomaTotalNFe;
            nota.det.Add(detalhe);
            return nota;
        }
    }
}
