using NFe.ConvertTxt;
using NFe.ConvertTxt.Mapping;
using Unimake.Business.DFe.Utility;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    public sealed class NFeDFeTaxMapperTests
    {
        [Fact]
        public void MapearDeveSelecionarICMS00EPreservarValores()
        {
            var nota = NFeDFeProductMapperTests.CriarNotaComProduto();
            nota.det[0].Imposto.ICMS.CST = "00";
            nota.det[0].Imposto.ICMS.orig = TpcnOrigemMercadoria.oeNacional;
            nota.det[0].Imposto.ICMS.modBC = TpcnDeterminacaoBaseIcms.dbiValorOperacao;
            nota.det[0].Imposto.ICMS.vBC = 25.31;
            nota.det[0].Imposto.ICMS.pICMS = 18;
            nota.det[0].Imposto.ICMS.vICMS = 4.56;

            var imposto = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Imposto;

            Assert.NotNull(imposto.ICMS.ICMS00);
            Assert.Null(imposto.ICMS.ICMS20);
            Assert.Equal(25.31, imposto.ICMS.ICMS00.VBC);
            Assert.Equal(18, imposto.ICMS.ICMS00.PICMS);
            Assert.Equal(4.56, imposto.ICMS.ICMS00.VICMS);
        }

        [Fact]
        public void MapearDeveSelecionarGrupoCorretoParaCST40ECSOSN102()
        {
            var notaCst = NFeDFeProductMapperTests.CriarNotaComProduto();
            notaCst.det[0].Imposto.ICMS.CST = "41";
            var icmsCst = new NFeDFeMapper().Mapear(notaCst).InfNFeField.Det[0].Imposto.ICMS;
            Assert.NotNull(icmsCst.ICMS40);

            var notaSn = NFeDFeProductMapperTests.CriarNotaComProduto();
            notaSn.det[0].Imposto.ICMS.CSOSN = 102;
            var icmsSn = new NFeDFeMapper().Mapear(notaSn).InfNFeField.Det[0].Imposto.ICMS;
            Assert.NotNull(icmsSn.ICMSSN102);
            Assert.Equal("102", icmsSn.ICMSSN102.CSOSN);
        }

        [Fact]
        public void ICMS00MapeadoDeveSerSerializadoPelaDll()
        {
            var nota = NFeDFeProductMapperTests.CriarNotaComProduto();
            nota.det[0].Imposto.ICMS.CST = "00";
            nota.det[0].Imposto.ICMS.orig = TpcnOrigemMercadoria.oeNacional;
            nota.det[0].Imposto.ICMS.modBC = TpcnDeterminacaoBaseIcms.dbiValorOperacao;
            nota.det[0].Imposto.ICMS.vBC = 25.31;
            nota.det[0].Imposto.ICMS.pICMS = 18;
            nota.det[0].Imposto.ICMS.vICMS = 4.56;

            var xml = XMLUtility.Serializar(new NFeDFeMapper().Mapear(nota));

            Assert.Equal(1, xml.GetElementsByTagName("ICMS00").Count);
            Assert.Equal("00", xml.GetElementsByTagName("CST")[0].InnerText);
            Assert.Equal("25.31", xml.GetElementsByTagName("vBC")[0].InnerText);
        }

        [Fact]
        public void MapearDeveSelecionarVariantesDeIPIPISECOFINS()
        {
            var nota = NFeDFeProductMapperTests.CriarNotaComProduto();
            nota.det[0].Imposto.IPI.CST = "50";
            nota.det[0].Imposto.IPI.cEnq = "999";
            nota.det[0].Imposto.IPI.vBC = 25.31;
            nota.det[0].Imposto.IPI.pIPI = 5;
            nota.det[0].Imposto.IPI.vIPI = 1.27;
            nota.det[0].Imposto.PIS.CST = "01";
            nota.det[0].Imposto.PIS.vBC = 25.31;
            nota.det[0].Imposto.PIS.pPIS = 1.65;
            nota.det[0].Imposto.PIS.vPIS = 0.42;
            nota.det[0].Imposto.COFINS.CST = "03";
            nota.det[0].Imposto.COFINS.qBCProd = 2.5000;
            nota.det[0].Imposto.COFINS.vAliqProd = 0.50;
            nota.det[0].Imposto.COFINS.vCOFINS = 1.25;

            var imposto = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Imposto;

            Assert.NotNull(imposto.IPI.IPITrib);
            Assert.NotNull(imposto.PIS.PISAliq);
            Assert.NotNull(imposto.COFINS.COFINSQtde);
            Assert.Equal(1.27, imposto.IPI.IPITrib.VIPI);
            Assert.Equal(0.42, imposto.PIS.PISAliq.VPIS);
            Assert.Equal(1.25, imposto.COFINS.COFINSQtde.VCOFINS);
        }

        [Fact]
        public void MapearOutrosEStDeveEmitirSomenteFormaPercentualOuQuantidade()
        {
            var nota = NFeDFeProductMapperTests.CriarNotaComProduto();
            nota.det[0].Imposto.PIS.CST = "99";
            nota.det[0].Imposto.PIS.qBCProd = 2;
            nota.det[0].Imposto.PIS.vAliqProd = 0.10;
            nota.det[0].Imposto.PIS.vPIS = 0.20;
            nota.det[0].Imposto.COFINSST.vBC = 25.31;
            nota.det[0].Imposto.COFINSST.pCOFINS = 7.6;
            nota.det[0].Imposto.COFINSST.vCOFINS = 1.92;

            var imposto = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Imposto;

            Assert.Null(imposto.PIS.PISOutr.VBC);
            Assert.Equal(2, imposto.PIS.PISOutr.QBCProd);
            Assert.Equal(25.31, imposto.COFINSST.VBC);
            Assert.Null(imposto.COFINSST.QBCProd);
        }

        [Fact]
        public void MapearDevePreservarISSQNICMSUFDestEImpostoDevol()
        {
            var nota = NFeDFeProductMapperTests.CriarNotaComProduto();
            nota.det[0].Imposto.ISSQN.vBC = 100;
            nota.det[0].Imposto.ISSQN.vAliq = 2.5;
            nota.det[0].Imposto.ISSQN.vISSQN = 2.5;
            nota.det[0].Imposto.ISSQN.cMunFG = 4118402;
            nota.det[0].Imposto.ISSQN.cListServ = "01.01";
            nota.det[0].Imposto.ISSQN.indISS = TpcnindISS.iiExigivel;
            nota.det[0].Imposto.ICMS.ICMSUFDest.vBCUFDest = 100;
            nota.det[0].Imposto.ICMS.ICMSUFDest.pICMSInter = 12;
            nota.det[0].Imposto.ICMS.ICMSUFDest.vICMSUFDest = 6;
            nota.det[0].impostoDevol.pDevol = 50;
            nota.det[0].impostoDevol.vIPIDevol = 1.25;

            var detalhe = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0];

            Assert.Equal(100, detalhe.Imposto.ISSQN.VBC);
            Assert.Equal("Servico0101", detalhe.Imposto.ISSQN.CListServ.ToString());
            Assert.Equal(12, detalhe.Imposto.ICMSUFDest.PICMSInter);
            Assert.Equal(50, detalhe.ImpostoDevol.PDevol);
            Assert.Equal(1.25, detalhe.ImpostoDevol.IPI.VIPIDevol);
        }

        [Fact]
        public void GruposOpcionaisDeISSPartilhaEDevolucaoNaoDevemNascerVazios()
        {
            var detalhe = new NFeDFeMapper().Mapear(NFeDFeProductMapperTests.CriarNotaComProduto()).InfNFeField.Det[0];

            Assert.Null(detalhe.Imposto);
            Assert.Null(detalhe.ImpostoDevol);
        }

        [Fact]
        public void MapearDevePreservarISENaoCriarIBSCBSVazio()
        {
            var nota = NFeDFeProductMapperTests.CriarNotaComProduto();
            nota.det[0].Imposto.IS.CSTIS = "000";
            nota.det[0].Imposto.IS.cClassTribIS = "000001";
            nota.det[0].Imposto.IS.vBCIS = 100;
            nota.det[0].Imposto.IS.pIS = 5;
            nota.det[0].Imposto.IS.pISEspec = 0.25;
            nota.det[0].Imposto.IS.vIS = 5;

            var imposto = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Imposto;

            Assert.Equal("000", imposto.IS.CSTIS);
            Assert.Equal(0.25, imposto.IS.AdRemIS);
            Assert.Null(imposto.IBSCBS);
        }

        [Fact]
        public void MapearDeveCriarGIBSCBSQuandoSomenteSubgrupoPossuiDados()
        {
            var nota = NFeDFeProductMapperTests.CriarNotaComProduto();
            nota.det[0].Imposto.IBSCBS.CST = "510";
            nota.det[0].Imposto.IBSCBS.cClassTrib = "510001";
            nota.det[0].Imposto.IBSCBS.gIBSCBS.gIBSUF.gDif.pDif = 10;
            nota.det[0].Imposto.IBSCBS.gIBSCBS.gIBSUF.gDif.vDif = 1;

            var ibsCbs = new NFeDFeMapper().Mapear(nota).InfNFeField.Det[0].Imposto.IBSCBS;

            Assert.NotNull(ibsCbs.GIBSCBS);
            Assert.NotNull(ibsCbs.GIBSCBS.GIBSUF);
            Assert.Equal(1, ibsCbs.GIBSCBS.GIBSUF.GDif.VDif);
            Assert.Null(ibsCbs.GIBSCBS.GIBSMun.GDevTrib);
        }
    }
}
