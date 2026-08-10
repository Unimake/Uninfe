using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using NFe.ConvertTxt;
using NFe.Settings;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class SecondRegressionTests
    {
        [Theory]
        [InlineData("0000042301054300027600113072026-NFE.txt")]
        [InlineData("versaoprouducao-nfe-orig.txt")]
        [InlineData("000580_08606985000105_001-nfe.txt")]
        [InlineData("0000072301054300027600116072026-NFE-orig.txt")]
        [InlineData("0000092301054300027600116072026-NFE-orig.txt")]
        [InlineData("0000112301054300027600116072026-NFE-orig.txt")]
        [InlineData("35260747498059000115550010004029951909226874-nfe-orig.txt")]
        [InlineData("002310_01_01_31_07_2026-nfe-orig.txt")]
        [InlineData("000479_09531276000170_003_31_07_2026-nfe-orig.txt")]
        [InlineData("nfe-nfe-orig.txt")]
        [InlineData("14222_43343052000335_1_31_7_2026-nfe-orig.txt")]
        [InlineData("046481_01391063000189_0_03_08_2026-nfe-orig.txt")]
        [InlineData("Nota_Fiscal_20265.txt")]
        [InlineData("20819_22716895000289_1_382026-nfe.txt")]
        [InlineData("2140_01955703000136_4_8_2026-nfe-orig.txt")]
        [InlineData("000071619_37870375000112_001_03_08_2026-nfe-orig.txt")]
        [InlineData("58_78789542000182_4_8_2026-nfe-orig.txt")]
        [InlineData("000001_01_01_05_08_2026-nfe-orig.txt")]
        [InlineData("08785-NFe.TXT")]
        [InlineData("31260803742159000170550020000003051000234068-NFE-orig.txt")]
        [InlineData("31260803742159000170550020000003051000234068-NFE-orig-v5.txt")]
        [InlineData("060218_32336224000165_001_06_08_2026-nfe-orig.txt")]
        [InlineData("NT60860218.TXT")]
        [InlineData("27260821287558000170650010001143821778530846-nfe-orig.txt")]
        [InlineData("41260801182867000178550010001800011567804549-nfe-orig.txt")]
        [InlineData("41260806225442000112550010002455051903698959-nfe-orig.txt")]
        [InlineData("nfe000077-NFE.txt")]
        public void NovoXmlDeveSerIgualAoLegado(string nomeArquivo)
        {
            var arquivo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "Regressions", nomeArquivo);
            var fixture = new NFeConvertTxtFixture();
            using (var resultado = fixture.Converter(arquivo))
            {
                Assert.True(resultado.Sucesso, resultado.MensagemErro);
                var pasta = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(pasta);
                try
                {
                    var legadoGerador = new NFeW { cMensagemErro = string.Empty };
                    var configuracoesOriginais = Empresas.Configuracoes;
                    if (configuracoesOriginais == null || configuracoesOriginais.Count == 0)
                    {
                        Empresas.Configuracoes = new List<Empresa> { new Empresa() };
                    }
                    try
                    {
                        typeof(NFeW).GetMethod("GerarXmlLegado", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(legadoGerador, new object[] { resultado.Nota, pasta, arquivo });
                    }
                    finally
                    {
                        Empresas.Configuracoes = configuracoesOriginais;
                    }
                    var legado = File.ReadAllText(legadoGerador.cFileName);
                    var conversaoNova = new Unimake.Business.DFe.Xml.NFe.NFeTxtConverter().Converter(arquivo);
                    Assert.True(conversaoNova.Sucesso, conversaoNova.MensagemErro);
                    var novo = Assert.Single(conversaoNova.Documentos).Xml;
                    if (string.Equals(nomeArquivo, "35260747498059000115550010004029951909226874-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Equal("60.0000", ObterReducaoIbsMunicipalDoDecimoSegundoItem(legado));
                        Assert.Equal("60.0000", ObterReducaoIbsMunicipalDoDecimoSegundoItem(novo));
                    }
                    if (string.Equals(nomeArquivo, "14222_43343052000335_1_31_7_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        const string csrt = "CSRTTESTE0123456789012345678";
                        var esperado = Unimake.Business.DFe.Utility.Converter.CalculateSHA1Hash(csrt + Assert.Single(conversaoNova.Documentos).Chave);
                        Assert.Equal(esperado, ObterHashCsrt(legado));
                        Assert.Equal(esperado, ObterHashCsrt(novo));
                    }
                    if (string.Equals(nomeArquivo, "046481_01391063000189_0_03_08_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Equal(20, ContarIpiTribCst99(legado));
                        Assert.Equal(20, ContarIpiTribCst99(novo));
                        Assert.Equal(0, ContarIpiNaoTributado(legado));
                        Assert.Equal(0, ContarIpiNaoTributado(novo));
                    }
                    if (string.Equals(nomeArquivo, "Nota_Fiscal_20265.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarRegressaoNotaFiscal20265(legado);
                        ValidarRegressaoNotaFiscal20265(novo);
                    }
                    if (string.Equals(nomeArquivo, "20819_22716895000289_1_382026-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarFaturaSemDesconto(legado);
                        ValidarFaturaSemDesconto(novo);
                    }
                    if (string.Equals(nomeArquivo, "2140_01955703000136_4_8_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIcmsSn900EDescricoesComAspas(legado);
                        ValidarIcmsSn900EDescricoesComAspas(novo);
                    }
                    if (string.Equals(nomeArquivo, "000071619_37870375000112_001_03_08_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIcms90PisECofinsOutros(legado);
                        ValidarIcms90PisECofinsOutros(novo);
                    }
                    if (string.Equals(nomeArquivo, "58_78789542000182_4_8_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarReducaoZeradaDoIcms51(legado);
                        ValidarReducaoZeradaDoIcms51(novo);
                    }
                    if (string.Equals(nomeArquivo, "000001_01_01_05_08_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarNfceEmContingencia(legado);
                        ValidarNfceEmContingencia(novo);
                    }
                    if (string.Equals(nomeArquivo, "08785-NFe.TXT", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarCamposVaziosDoIcmsSn900(legado);
                        ValidarCamposVaziosDoIcmsSn900(novo);
                    }
                    if (string.Equals(nomeArquivo, "31260803742159000170550020000003051000234068-NFE-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarImpostoImportacaoZeradoEIcms51(legado);
                        ValidarImpostoImportacaoZeradoEIcms51(novo);
                        ValidarPrecisaoDosValoresDoProduto(legado, true);
                        ValidarPrecisaoDosValoresDoProduto(novo, false);
                    }
                    if (string.Equals(nomeArquivo, "31260803742159000170550020000003051000234068-NFE-orig-v5.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIcms51ComBasePositiva(legado);
                        ValidarIcms51ComBasePositiva(novo);
                    }
                    if (string.Equals(nomeArquivo, "060218_32336224000165_001_06_08_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(nomeArquivo, "NT60860218.TXT", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarNfceEmContingenciaComImpostos(legado);
                        ValidarNfceEmContingenciaComImpostos(novo);
                    }
                    if (string.Equals(nomeArquivo, "27260821287558000170650010001143821778530846-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIcmsSn500EReformaComCamposVazios(legado);
                        ValidarIcmsSn500EReformaComCamposVazios(novo);
                    }
                    if (string.Equals(nomeArquivo, "41260801182867000178550010001800011567804549-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarRastroValorItemEResponsavelTecnico(legado);
                        ValidarRastroValorItemEResponsavelTecnico(novo);
                    }
                    if (string.Equals(nomeArquivo, "41260806225442000112550010002455051903698959-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        const string csrt = "CSRTTESTE0123456789012345678";
                        var esperado = Unimake.Business.DFe.Utility.Converter.CalculateSHA1Hash(csrt + Assert.Single(conversaoNova.Documentos).Chave);
                        Assert.Equal(esperado, ObterHashCsrt(legado));
                        Assert.Equal(esperado, ObterHashCsrt(novo));
                    }
                    if (string.Equals(nomeArquivo, "nfe000077-NFE.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarDfeReferenciadoPorItem(legado);
                        ValidarDfeReferenciadoPorItem(novo);
                    }
                    var diferenca = NFeConvertTxtXmlComparer.Comparar(legado, novo);
                    Assert.True(diferenca == null, diferenca);
                }
                finally { if (Directory.Exists(pasta)) Directory.Delete(pasta, true); }
            }
        }

        [Fact]
        public void MassasTxtNaoDevemConterDadosIdentificaveisConhecidos()
        {
            var dadosIdentificaveis = new[]
            {
                "EMERSON SILVA GUEDES",
                "contato@roguelimp.com.br",
                "05976103804",
                "mepagodi@gmail.com",
                "WONENFE@GMAIL.COM",
                "JULIANO KOCH",
                "51999626374",
                "suporte@microprisma.com.br",
                "WYLBER NASSA",
                "DEBORA PJ",
                "RUA SANTO ANDRE|134",
                "R. PEDRO VITORATO",
                "RUA GENERAL MARIANTE",
                "48577324915",
                "04690036934",
                "92991289953",
                "SOC.COM.MAT.P/CONSTR.LUIZ LOPES LTDA",
                "00454749000109",
                "108680702113",
                "956224310481",
                "RUA MAJOR OTAVIANO",
                "R. OLIVEIRA CATRAMBI",
                "1122911633",
                "11997556655",
                "luizlopes.nfe@uol.com.br",
                "VENDEDOR: 0110 WAGNER",
                "AUTO VIDROS PRUDENTE",
                "562319803111",
                "592009166115",
                "45523719000811",
                "RUA ANTONIO RUIZ",
                "AVENIDA XV DE NOVEMBRO",
                "Marco Thomaz",
                "marco@duesoft.com.br",
                "1839167600",
                "PLACA: FRT 6828",
                "J. R. DE OLIVEIRA AUTO ELETRICA",
                "38136977000103",
                "03640467000194",
                "401300590118",
                "401035229111",
                "1436215947",
                "36025222",
                "RUA OTAVIO CONEGUNDES DE SOUZA",
                "SUPERM. JAU SERVE LTDA",
                "nfe@jauserve.com.br",
                "carlos.tagiarolli@jauserve.com.br",
                "AVENIDA JOAO SANZOVO",
                "Florestal Alvorada Florestamento e Reflorestamento Ltda",
                "Industria de Compensados Sudati Ltda",
                "João Henrique Buckta",
                "joao.henrique@valorflorestal.com.br",
                "8X77VU0XB39URUYTGYSU7IU14UQB",
                "NET LIGHT LTDA.",
                "notafiscal@zummo.com.br",
                "RUA MATOS COSTA",
                "278064462111",
                "1146128926",
                "ARVENSIS COSMETICOS LTDA",
                "ARVENSIS COSMETICOS",
                "GMN EMBALAGENS LTDA",
                "RUA DOMICIANO MARTINS DE ANDRADE",
                "RUA DR MILTON LADEIRA",
                "3232258011",
                "J.A. HARD NUTRITION",
                "PEDRO HENRIQUE MELLO CASAGRANDE",
                "materiaprimasuplementosfw@gmail.com",
                "Rua Jambeiro",
                "R 21 DE ABRIL",
                "ATIVA DISTRIBUICAO E LOGISTICA LTDA",
                "sac@underlabznutrition.com",
                "M-126863",
                "134004",
                "0042882644996",
                "0618231258819",
                "Conquista Industria de Artigos Para Selaria",
                "CONQUISTA IND. DE ART. P/SELARIA",
                "Rua Ezidio Balladelli",
                "RUA EZIDIO BALADELLI",
                "DEOCLECIO ALVES DE ARAUJO",
                "RUA MEN DE SA",
                "59623500904",
                "9013566450",
                "0443351392",
                "CENTERKASA COMERCIAL LTDA",
                "NOVA ROCHA IND TINTAS LTDA",
                "CIARIN COMERCIO E INDUSTRIA DE ARTIGOS P/ SELARIA LTDA",
                "CIARIN METAIS",
                "AGROPECUARIA GALPAO DO BOIADEIRO LTDA EPP",
                "SUDOESTE TRANSPORTES LTDA",
                "RUA EZIDIO BALLADELLI",
                "RUA SALDANHA MARINHO",
                "RUA ALMERINDA SILVEIRA COELHO",
                "8330316005",
                "9012364374",
                "01468972000178",
                "02343801000851",
                "4433513934",
                "236235023",
                "devolucoes@leinertex.com.br",
                "AV ANAPOLIS",
                "AV JATAI",
                "VILA CONCORDIA",
                "PQ IND AP VICE P JOSE ALENCAR",
                "102575584",
                "103120939",
                "6232081448",
                "6232750800",
                "420396)"
            };

            var pasta = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures");
            foreach (var arquivo in Directory.GetFiles(pasta, "*.txt", SearchOption.AllDirectories))
            {
                var conteudo = File.ReadAllText(arquivo);
                foreach (var dadoIdentificavel in dadosIdentificaveis)
                {
                    Assert.True(
                        conteudo.IndexOf(dadoIdentificavel, StringComparison.OrdinalIgnoreCase) < 0,
                        $"O arquivo '{Path.GetFileName(arquivo)}' contém o dado identificável '{dadoIdentificavel}'.");
                }
            }
        }

        private static string ObterReducaoIbsMunicipalDoDecimoSegundoItem(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            return xml.SelectSingleNode("//*[local-name()='det'][12]/*[local-name()='imposto']/*[local-name()='IBSCBS']/*[local-name()='gIBSCBS']/*[local-name()='gIBSMun']/*[local-name()='gRed']/*[local-name()='pRedAliq']")?.InnerText;
        }

        private static string ObterHashCsrt(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            return xml.SelectSingleNode("//*[local-name()='infRespTec']/*[local-name()='hashCSRT']")?.InnerText;
        }

        private static int ContarIpiTribCst99(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            return xml.SelectNodes("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='IPI']/*[local-name()='IPITrib' and *[local-name()='CST']='99']").Count;
        }

        private static int ContarIpiNaoTributado(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            return xml.SelectNodes("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='IPI']/*[local-name()='IPINT']").Count;
        }

        private static void ValidarRegressaoNotaFiscal20265(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var icms51 = xml.SelectSingleNode("//*[local-name()='ICMS51']");

            Assert.NotNull(icms51);
            Assert.Equal(3, icms51.ChildNodes.Count);
            Assert.Equal("49", xml.SelectSingleNode("//*[local-name()='PISOutr']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("49", xml.SelectSingleNode("//*[local-name()='COFINSOutr']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("150.48", xml.SelectSingleNode("//*[local-name()='impostoDevol']/*[local-name()='IPI']/*[local-name()='vIPIDevol']")?.InnerText);
        }

        private static void ValidarFaturaSemDesconto(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var fatura = xml.SelectSingleNode("//*[local-name()='cobr']/*[local-name()='fat']");

            Assert.NotNull(fatura);
            Assert.Null(fatura.SelectSingleNode("*[local-name()='vDesc']"));
            Assert.Equal("13961.12", fatura.SelectSingleNode("*[local-name()='vOrig']")?.InnerText);
            Assert.Equal("13961.12", fatura.SelectSingleNode("*[local-name()='vLiq']")?.InnerText);
        }

        private static void ValidarIcmsSn900EDescricoesComAspas(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);

            Assert.Equal(149, xml.SelectNodes("//*[local-name()='ICMSSN900']").Count);
            Assert.Equal(0, xml.SelectNodes("//*[local-name()='ICMSSN900']/*[local-name()='pMVAST']").Count);

            var descricoesComAspas = xml.SelectNodes("//*[local-name()='xProd'][contains(text(), '&quot;')]");
            Assert.Equal(4, descricoesComAspas.Count);
            Assert.Equal("BRIDAO DE FERRO MODELO &quot;D&quot; SIMPLES", descricoesComAspas[0].InnerText);
        }

        private static void ValidarIcms90PisECofinsOutros(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var icms90 = xml.SelectSingleNode("//*[local-name()='ICMS90']");

            Assert.NotNull(icms90);
            Assert.Equal(6, icms90.ChildNodes.Count);
            Assert.Null(icms90.SelectSingleNode("*[local-name()='modBCST']"));
            Assert.Null(icms90.SelectSingleNode("*[local-name()='vICMSDeson']"));
            Assert.Equal("49", xml.SelectSingleNode("//*[local-name()='PISOutr']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("1.6500", xml.SelectSingleNode("//*[local-name()='PISOutr']/*[local-name()='pPIS']")?.InnerText);
            Assert.Equal("49", xml.SelectSingleNode("//*[local-name()='COFINSOutr']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("7.6000", xml.SelectSingleNode("//*[local-name()='COFINSOutr']/*[local-name()='pCOFINS']")?.InnerText);
            Assert.Equal("11.15", xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='prod']/*[local-name()='vOutro']")?.InnerText);
        }

        private static void ValidarReducaoZeradaDoIcms51(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var grupos = xml.SelectNodes("//*[local-name()='ICMS51']");

            Assert.Equal(30, grupos.Count);
            foreach (XmlNode grupo in grupos)
            {
                Assert.Equal("0.0000", grupo.SelectSingleNode("*[local-name()='pRedBC']")?.InnerText);
            }
            Assert.Equal("5801.49", xml.SelectSingleNode("//*[local-name()='IBSCBSTot']/*[local-name()='vBCIBSCBS']")?.InnerText);
            Assert.Equal("5.80", xml.SelectSingleNode("//*[local-name()='IBSCBSTot']/*[local-name()='gIBS']/*[local-name()='vIBS']")?.InnerText);
            Assert.Equal("52.21", xml.SelectSingleNode("//*[local-name()='IBSCBSTot']/*[local-name()='gCBS']/*[local-name()='vCBS']")?.InnerText);
        }

        private static void ValidarNfceEmContingencia(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);

            Assert.Equal("65", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='mod']")?.InnerText);
            Assert.Equal("6", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='tpEmis']")?.InnerText);
            Assert.Equal("2026-08-05T13:00:00-03:00", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='dhCont']")?.InnerText);
            Assert.Equal("SEFAZ SP FORA DO AR", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='xJust']")?.InnerText);
            Assert.Equal("102", xml.SelectSingleNode("//*[local-name()='ICMSSN102']/*[local-name()='CSOSN']")?.InnerText);
            Assert.Equal("99", xml.SelectSingleNode("//*[local-name()='PISOutr']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("99", xml.SelectSingleNode("//*[local-name()='COFINSOutr']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("232.30", xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='vItem']")?.InnerText);
            Assert.Equal("232.30", xml.SelectSingleNode("//*[local-name()='total']/*[local-name()='vNFTot']")?.InnerText);
        }

        private static void ValidarCamposVaziosDoIcmsSn900(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var icms = xml.SelectSingleNode("//*[local-name()='ICMSSN900']");

            Assert.NotNull(icms);
            Assert.Equal(3, icms.ChildNodes.Count);
            Assert.Equal("0", icms.SelectSingleNode("*[local-name()='orig']")?.InnerText);
            Assert.Equal("900", icms.SelectSingleNode("*[local-name()='CSOSN']")?.InnerText);
            Assert.Equal("0", icms.SelectSingleNode("*[local-name()='modBC']")?.InnerText);
        }

        private static void ValidarImpostoImportacaoZeradoEIcms51(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var impostoImportacao = xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='II']");
            var icms51 = xml.SelectSingleNode("//*[local-name()='ICMS51']");

            Assert.NotNull(xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='prod']/*[local-name()='DI']"));
            Assert.NotNull(impostoImportacao);
            Assert.Equal(4, impostoImportacao.ChildNodes.Count);
            Assert.Equal("0.00", impostoImportacao.SelectSingleNode("*[local-name()='vBC']")?.InnerText);
            Assert.Equal("0.00", impostoImportacao.SelectSingleNode("*[local-name()='vDespAdu']")?.InnerText);
            Assert.Equal("0.00", impostoImportacao.SelectSingleNode("*[local-name()='vII']")?.InnerText);
            Assert.Equal("0.00", impostoImportacao.SelectSingleNode("*[local-name()='vIOF']")?.InnerText);
            Assert.NotNull(icms51);
            Assert.Equal(2, icms51.ChildNodes.Count);
            Assert.Null(icms51.SelectSingleNode("*[local-name()='modBC']"));
        }

        private static void ValidarPrecisaoDosValoresDoProduto(string conteudoXml, bool legado)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var produto = xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='prod']");

            Assert.NotNull(produto);
            Assert.Equal("22919.6000", produto.SelectSingleNode("*[local-name()='qCom']")?.InnerText);
            Assert.Equal("22919.6000", produto.SelectSingleNode("*[local-name()='qTrib']")?.InnerText);
            Assert.Equal(legado ? "14.0463140000" : "14.046314", produto.SelectSingleNode("*[local-name()='vUnCom']")?.InnerText);
            Assert.Equal(legado ? "14.0463140000" : "14.046314", produto.SelectSingleNode("*[local-name()='vUnTrib']")?.InnerText);
            Assert.Equal("321935.90", produto.SelectSingleNode("*[local-name()='vProd']")?.InnerText);
        }

        private static void ValidarNfceEmContingenciaComImpostos(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);

            Assert.Equal(47, xml.DocumentElement.SelectSingleNode("*[local-name()='infNFe']")?.Attributes["Id"]?.Value.Length);
            Assert.Equal("65", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='mod']")?.InnerText);
            Assert.Equal("9", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='tpEmis']")?.InnerText);
            Assert.Equal("2026-08-06T10:08:59-03:00", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='dhCont']")?.InnerText);
            Assert.Equal("PROBLEMA DE CONECTIVIDADE PARA TESTE", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='xJust']")?.InnerText);
            Assert.Equal("25.41", xml.SelectSingleNode("//*[local-name()='ICMS00']/*[local-name()='vICMS']")?.InnerText);
            Assert.Equal("2.0000", xml.SelectSingleNode("//*[local-name()='ICMS00']/*[local-name()='pFCP']")?.InnerText);
            Assert.Equal("2.54", xml.SelectSingleNode("//*[local-name()='ICMS00']/*[local-name()='vFCP']")?.InnerText);
            Assert.Equal("2.10", xml.SelectSingleNode("//*[local-name()='PISAliq']/*[local-name()='vPIS']")?.InnerText);
            Assert.Equal("9.65", xml.SelectSingleNode("//*[local-name()='COFINSAliq']/*[local-name()='vCOFINS']")?.InnerText);
            Assert.Equal("127.03", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
        }

        private static void ValidarIcmsSn500EReformaComCamposVazios(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var icms = xml.SelectSingleNode("//*[local-name()='ICMSSN500']");

            Assert.Equal("NFe27260821287558000170650010001143821800677760", xml.SelectSingleNode("//*[local-name()='infNFe']")?.Attributes["Id"]?.Value);
            Assert.NotNull(icms);
            Assert.Equal(2, icms.ChildNodes.Count);
            Assert.Equal("500", icms.SelectSingleNode("*[local-name()='CSOSN']")?.InnerText);
            Assert.Equal("06", xml.SelectSingleNode("//*[local-name()='PISNT']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("06", xml.SelectSingleNode("//*[local-name()='COFINSNT']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("000", xml.SelectSingleNode("//*[local-name()='IBSCBS']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("000001", xml.SelectSingleNode("//*[local-name()='IBSCBS']/*[local-name()='cClassTrib']")?.InnerText);
            Assert.Equal("9.00", xml.SelectSingleNode("//*[local-name()='gIBSCBS']/*[local-name()='vBC']")?.InnerText);
            Assert.Equal("0.1000", xml.SelectSingleNode("//*[local-name()='gIBSUF']/*[local-name()='pIBSUF']")?.InnerText);
            Assert.Equal("0.01", xml.SelectSingleNode("//*[local-name()='gIBSUF']/*[local-name()='vIBSUF']")?.InnerText);
            Assert.Equal("0.9000", xml.SelectSingleNode("//*[local-name()='gCBS']/*[local-name()='pCBS']")?.InnerText);
            Assert.Equal("0.08", xml.SelectSingleNode("//*[local-name()='gCBS']/*[local-name()='vCBS']")?.InnerText);
            Assert.Equal("9.00", xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='vItem']")?.InnerText);
        }

        private static void ValidarDfeReferenciadoPorItem(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var referencias = xml.SelectNodes("//*[local-name()='det']/*[local-name()='DFeReferenciado']");

            Assert.Equal(3, referencias.Count);
            Assert.Equal("35260796597620000129550010001408741335108850", referencias[0].SelectSingleNode("*[local-name()='chaveAcesso']")?.InnerText);
            Assert.Equal("991", referencias[0].SelectSingleNode("*[local-name()='nItem']")?.InnerText);
            Assert.Equal("25", referencias[1].SelectSingleNode("*[local-name()='nItem']")?.InnerText);
            Assert.Equal("15", referencias[2].SelectSingleNode("*[local-name()='nItem']")?.InnerText);
        }

        private static void ValidarIcms51ComBasePositiva(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var icms51 = xml.SelectSingleNode("//*[local-name()='ICMS51']");

            Assert.NotNull(icms51);
            Assert.Equal(9, icms51.ChildNodes.Count);
            Assert.Null(icms51.SelectSingleNode("*[local-name()='modBC']"));
            Assert.Equal("0.0000", icms51.SelectSingleNode("*[local-name()='pRedBC']")?.InnerText);
            Assert.Equal("398422.66", icms51.SelectSingleNode("*[local-name()='vBC']")?.InnerText);
            Assert.Equal("0.0000", icms51.SelectSingleNode("*[local-name()='pICMS']")?.InnerText);
            Assert.Equal("0.00", icms51.SelectSingleNode("*[local-name()='vICMSOp']")?.InnerText);
            Assert.Equal("0.0000", icms51.SelectSingleNode("*[local-name()='pDif']")?.InnerText);
            Assert.Equal("0.00", icms51.SelectSingleNode("*[local-name()='vICMSDif']")?.InnerText);
            Assert.Equal("0.00", icms51.SelectSingleNode("*[local-name()='vICMS']")?.InnerText);
        }

        private static void ValidarRastroValorItemEResponsavelTecnico(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var itens = xml.SelectNodes("//*[local-name()='det']");

            Assert.Equal(3, itens.Count);
            Assert.Equal("LOTE001", itens[0].SelectSingleNode("*[local-name()='prod']/*[local-name()='rastro']/*[local-name()='nLote']")?.InnerText);
            Assert.Equal("25.000", itens[0].SelectSingleNode("*[local-name()='prod']/*[local-name()='rastro']/*[local-name()='qLote']")?.InnerText);
            Assert.Equal("2026-08-05", itens[0].SelectSingleNode("*[local-name()='prod']/*[local-name()='rastro']/*[local-name()='dFab']")?.InnerText);
            Assert.Equal("2028-08-05", itens[0].SelectSingleNode("*[local-name()='prod']/*[local-name()='rastro']/*[local-name()='dVal']")?.InnerText);
            Assert.Equal("1351.25", itens[0].SelectSingleNode("*[local-name()='vItem']")?.InnerText);
            Assert.Equal("2064.00", itens[1].SelectSingleNode("*[local-name()='vItem']")?.InnerText);
            Assert.Equal("1074.75", itens[2].SelectSingleNode("*[local-name()='vItem']")?.InnerText);
            Assert.Equal("02", xml.SelectSingleNode("//*[local-name()='infRespTec']/*[local-name()='idCSRT']")?.InnerText);
            Assert.Equal("AAAAAAAAAAAAAAAAAAAAAAAAAAA=", xml.SelectSingleNode("//*[local-name()='infRespTec']/*[local-name()='hashCSRT']")?.InnerText);
        }
    }
}
