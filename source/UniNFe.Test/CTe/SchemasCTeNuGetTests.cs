using System.Xml;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Unimake.Business.DFe.Xml.CTe;
using Xunit;

namespace UniNFe.Test.CTe
{
    public class SchemasCTeNuGetTests
    {
        [Fact]
        public void PacoteSerializaNovoEventoEContemSeuSchema()
        {
            var evento = new EventoCTe
            {
                Versao = "4.00",
                InfEvento = new InfEvento
                {
                    COrgao = UFBrasil.PR,
                    TpAmb = TipoAmbiente.Homologacao,
                    CNPJ = "00000000000000",
                    ChCTe = "41261000000000000000570010000000011000000000",
                    DhEvento = new System.DateTimeOffset(2026, 10, 10, 10, 0, 0, System.TimeSpan.FromHours(-3)),
                    TpEvento = TipoEventoCTe.ApropriacaoCreditoPresumido,
                    DetEvento = new DetEventoApropriacaoCreditoPresumido
                    {
                        VersaoEvento = "4.00",
                        VBCCredPres = 1500,
                        CCredPres = "01",
                        GIBSCredPres = new GCredPresEvento { PCredPres = 1.2345, VCredPres = 18.52 },
                        GCBSCredPres = new GCredPresEvento { PCredPres = 2.5, VCredPres = 37.5 },
                        XDecPag = DeclaracaoPagamentoCreditoPresumidoCTe.ComAcentuacao
                    }
                }
            };
            var xml = evento.GerarXML();
            var lido = XMLUtility.Deserializar<EventoCTe>(xml);
            var detalhe = Assert.IsType<DetEventoApropriacaoCreditoPresumido>(lido.InfEvento.DetEvento);
            Assert.Equal(1.2345d, detalhe.GIBSCredPres.PCredPres);
            Assert.Equal("01", detalhe.CCredPres);
            Assert.Equal(xml.InnerText, lido.GerarXML().InnerText);
            var especifico = new XmlDocument();
            especifico.LoadXml(xml.GetElementsByTagName("evApropriaCredPres")[0].OuterXml);
            var validador = new ValidarSchema();
            validador.Validar(especifico, "CTe.evApropriaCredPres_v4.00.xsd", "http://www.portalfiscal.inf.br/cte");
            Assert.True(validador.Success, validador.ErrorMessage);
        }

        [Fact]
        public void PacoteAceitaContingenciaOfflineNosTresDocumentos()
        {
            Assert.Equal(2, (int)new Ide { TpEmis = TipoEmissao.ContingenciaOfflineCTe }.TpEmis);
            Assert.Equal(2, (int)new Unimake.Business.DFe.Xml.CTeOS.Ide { TpEmis = TipoEmissao.ContingenciaOfflineCTe }.TpEmis);
            Assert.Equal(2, (int)new Unimake.Business.DFe.Xml.CTeSimp.Ide { TpEmis = TipoEmissao.ContingenciaOfflineCTe }.TpEmis);
        }
    }
}
