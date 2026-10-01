using NFe.Service;
using System;
using System.Collections.Generic;
using Unimake.Business.DFe.Utility;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFe;
using Xunit;

namespace UniNFe.Test.Monofasia
{
    public sealed class MonofasiaLeiauteTests
    {
        [Theory]
        [InlineData(2025, VersaoLeiauteMonofasia.Legado)]
        [InlineData(2026, VersaoLeiauteMonofasia.Atual2026)]
        [InlineData(2027, VersaoLeiauteMonofasia.Atual2027A2028)]
        [InlineData(2028, VersaoLeiauteMonofasia.Atual2027A2028)]
        [InlineData(2029, VersaoLeiauteMonofasia.Atual2029EmDiante)]
        public void HomologacaoDeveSelecionarLeiautePeloAnoDaEmissao(int ano, VersaoLeiauteMonofasia esperado)
        {
            var lote = CriarLote(TipoAmbiente.Homologacao, ano);

            TaskNFeRecepcao.AjustarLeiauteMonofasiaHomologacao(lote);

            Assert.Equal(esperado, lote.NFe[0].InfNFeField.Det[0].Imposto.IBSCBS.GIBSCBSMono.VersaoLeiaute);
        }

        [Fact]
        public void ProducaoDevePermanecerNoLeiauteLegado()
        {
            var lote = CriarLote(TipoAmbiente.Producao, 2026);

            TaskNFeRecepcao.AjustarLeiauteMonofasiaHomologacao(lote);

            Assert.Equal(VersaoLeiauteMonofasia.Legado, lote.NFe[0].InfNFeField.Det[0].Imposto.IBSCBS.GIBSCBSMono.VersaoLeiaute);
        }

        [Fact]
        public void Homologacao2026DeveSerializarRetidoLegadoComoAdValorem()
        {
            var lote = CriarLote(TipoAmbiente.Homologacao, 2026);

            TaskNFeRecepcao.AjustarLeiauteMonofasiaHomologacao(lote);
            var xml = XMLUtility.Serializar(lote.NFe[0].InfNFeField.Det[0].Imposto.IBSCBS.GIBSCBSMono);

            Assert.Equal("0.00", xml.SelectSingleNode("//*[local-name()='gIBSMonoAdValorem']/*[local-name()='gMonoRet']/*[local-name()='vIBSMonoRet']")?.InnerText);
            Assert.Equal("0.00", xml.SelectSingleNode("//*[local-name()='gCBSMonoAdValorem']/*[local-name()='gMonoRet']/*[local-name()='vCBSMonoRet']")?.InnerText);
            Assert.Null(xml.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gMonoRet']"));
        }

        private static EnviNFe CriarLote(TipoAmbiente ambiente, int ano)
        {
            return new EnviNFe
            {
                NFe = new List<Unimake.Business.DFe.Xml.NFe.NFe>
                {
                    new Unimake.Business.DFe.Xml.NFe.NFe
                    {
                        InfNFeField = new InfNFe
                        {
                            Ide = new Ide
                            {
                                TpAmb = ambiente,
                                DhEmi = new DateTimeOffset(ano, 1, 1, 0, 0, 0, TimeSpan.FromHours(-3))
                            },
                            Det = new List<Det>
                            {
                                new Det
                                {
                                    Imposto = new Imposto
                                    {
                                        IBSCBS = new IBSCBS
                                        {
                                            GIBSCBSMono = new GIBSCBSMono
                                            {
                                                GMonoRet = new GMonoRet()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
