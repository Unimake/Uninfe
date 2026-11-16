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
        public void DeveSelecionarLeiautePeloAnoDaEmissao(int ano, VersaoLeiauteMonofasia esperado)
        {
            var lote = CriarLote(TipoAmbiente.Homologacao, ano);

            TaskNFeRecepcao.AjustarLeiauteMonofasia(lote);

            Assert.Equal(esperado, lote.NFe[0].InfNFeField.Det[0].Imposto.IBSCBS.GIBSCBSMono.VersaoLeiaute);
        }

        [Fact]
        public void Producao2026DeveSelecionarLeiauteAtual()
        {
            var lote = CriarLote(TipoAmbiente.Producao, 2026);

            TaskNFeRecepcao.AjustarLeiauteMonofasia(lote);

            Assert.Equal(VersaoLeiauteMonofasia.Atual2026, lote.NFe[0].InfNFeField.Det[0].Imposto.IBSCBS.GIBSCBSMono.VersaoLeiaute);
        }

        [Theory]
        [InlineData(TipoAmbiente.Homologacao)]
        [InlineData(TipoAmbiente.Producao)]
        public void Ambientes2026DevemSerializarRetidoLegadoComoAdValorem(TipoAmbiente ambiente)
        {
            var lote = CriarLote(ambiente, 2026);

            TaskNFeRecepcao.AjustarLeiauteMonofasia(lote);
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
