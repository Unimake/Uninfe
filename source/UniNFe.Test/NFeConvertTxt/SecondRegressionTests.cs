using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using NFe.ConvertTxt;
using NFe.Settings;
using Unimake.Business.DFe.Utility;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class SecondRegressionTests
    {
        [Fact]
        public void ConversorLegadoDeveRejeitarTxtSemCabecalhoNotaFiscal()
        {
            var arquivo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "Regressions", "000000411-nfe.txt");
            var arquivoTemporario = Path.GetTempFileName();
            try
            {
                var linhas = File.ReadAllLines(arquivo);
                var linhasSemCabecalho = new string[linhas.Length - 1];
                Array.Copy(linhas, 1, linhasSemCabecalho, 0, linhasSemCabecalho.Length);
                File.WriteAllLines(arquivoTemporario, linhasSemCabecalho);

                var fixture = new NFeConvertTxtFixture();
                using (var resultado = fixture.Converter(arquivoTemporario))
                {
                    Assert.False(resultado.Sucesso);
                    Assert.Contains("segmento A", resultado.MensagemErro, StringComparison.OrdinalIgnoreCase);
                }
            }
            finally
            {
                File.Delete(arquivoTemporario);
            }
        }

        [Theory]
        [InlineData("35260847498059000115550010004030011909226990-nfe.txt", "6", "07", null, "1")]
        [InlineData("35260847498059000115550010004030021004029993-nfe.txt", "5", null, "03", "0")]
        public void DllDeveConverterNotaDeCreditoEDebitoSemMunicipioFatoGeradorIbs(
            string nomeArquivo,
            string finalidadeEsperada,
            string tipoDebitoEsperado,
            string tipoCreditoEsperado,
            string tipoOperacaoEsperado)
        {
            var arquivo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "Regressions", nomeArquivo);
            var resultado = new Unimake.Business.DFe.Xml.NFe.NFeTxtConverter().Converter(arquivo);

            Assert.True(resultado.Sucesso, resultado.MensagemErro);
            var xml = new XmlDocument();
            xml.LoadXml(Assert.Single(resultado.Documentos).Xml);

            Assert.Equal(finalidadeEsperada, xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='finNFe']")?.InnerText);
            Assert.Equal(tipoOperacaoEsperado, xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='tpNF']")?.InnerText);
            Assert.Equal(tipoDebitoEsperado, xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='tpNFDebito']")?.InnerText);
            Assert.Equal(tipoCreditoEsperado, xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='tpNFCredito']")?.InnerText);
            Assert.Null(xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='cMunFGIBS']"));
            Assert.NotNull(xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='IBSCBS']"));
        }

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
        [InlineData("41260801182867000178550010001800811409310317-nfe.txt")]
        [InlineData("NFe_2998-nfe-orig-v2.txt")]
        [InlineData("NFe_2999-nfe-orig-v2.txt")]
        [InlineData("000023655_11092080000179_001_11_08_2026-nfe-orig.txt")]
        [InlineData("398_15528301000160_1_11_08_2026-NFE-orig.txt")]
        [InlineData("399_15528301000160_1_11_08_2026-NFE-orig.txt")]
        [InlineData("35260847498059000115550010004030011909226990-nfe.txt")]
        [InlineData("35260847498059000115550010004030021004029993-nfe.txt")]
        [InlineData("0000056689-nfe-orig.txt")]
        [InlineData("NFe_000049184_08_27_14-nfe.txt")]
        [InlineData("002320_01_01_17_08_2026-nfe.txt")]
        [InlineData("000017136_19041494000180_001_19_08_2026-nfe-orig.txt")]
        [InlineData("035814-nfe-orig.txt")]
        [InlineData("161540-nfe-orig.txt")]
        [InlineData("000015493-nfe.txt")]
        [InlineData("000000892-nfe.txt")]
        [InlineData("000002191-nfe-orig.txt")]
        [InlineData("000000200-nfe.txt")]
        [InlineData("000062981-nfe-orig.txt")]
        [InlineData("000000411-nfe.txt")]
        [InlineData("000027937-nfe.txt")]
        [InlineData("RTC2026-NFe621-nfe.txt")]
        [InlineData("RTC2026-NFe622-nfe.txt")]
        [InlineData("RTC2026-NFe623-nfe.txt")]
        [InlineData("RTC2026-NFe624-nfe.txt")]
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
                    if (string.Equals(nomeArquivo, "41260801182867000178550010001800811409310317-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarInfAdProdAposNormalizacao(novo);
                    }
                    if (string.Equals(nomeArquivo, "NFe_2998-nfe-orig-v2.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarDivergenciaVOutroInformadaPeloErp(legado);
                        ValidarDivergenciaVOutroInformadaPeloErp(novo);
                    }
                    if (string.Equals(nomeArquivo, "NFe_2999-nfe-orig-v2.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarDivergenciaVtotTribInformadaPeloErp(legado);
                        ValidarDivergenciaVtotTribInformadaPeloErp(novo);
                    }
                    if (string.Equals(nomeArquivo, "000023655_11092080000179_001_11_08_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarTotaisDaNfceDevolucao23655(legado);
                        ValidarTotaisDaNfceDevolucao23655(novo);
                    }
                    if (string.Equals(nomeArquivo, "398_15528301000160_1_11_08_2026-NFE-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarCobrancaPagamentosEReformaDaNfe398(legado);
                        ValidarCobrancaPagamentosEReformaDaNfe398(novo);
                    }
                    if (string.Equals(nomeArquivo, "399_15528301000160_1_11_08_2026-NFE-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIpiEItemForaDoTotalDaNfe399(legado);
                        ValidarIpiEItemForaDoTotalDaNfe399(novo);
                    }
                    if (nomeArquivo.StartsWith("352608474980590001155500100040300", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(nomeArquivo, "NFe_000049184_08_27_14-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        legado = OmitirPaisBrasilOpcionalDeRetiradaEEntrega(legado);
                        novo = OmitirPaisBrasilOpcionalDeRetiradaEEntrega(novo);
                    }
                    if (string.Equals(nomeArquivo, "002320_01_01_17_08_2026-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarProdutoIpiEReformaDaNFe2320(legado);
                        ValidarProdutoIpiEReformaDaNFe2320(novo);
                    }
                    if (string.Equals(nomeArquivo, "000017136_19041494000180_001_19_08_2026-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarPagamentosDaNfce17136(legado);
                        ValidarPagamentosDaNfce17136(novo);
                    }
                    if (string.Equals(nomeArquivo, "035814-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarPisECofinsDaNfe35814(legado);
                        ValidarPisECofinsDaNfe35814(novo);
                    }
                    if (string.Equals(nomeArquivo, "161540-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarImpostosDaNfce161540(legado);
                        ValidarImpostosDaNfce161540(novo);
                    }
                    if (string.Equals(nomeArquivo, "000015493-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIcms10DaNfe15493(legado);
                        ValidarIcms10DaNfe15493(novo);
                    }
                    if (string.Equals(nomeArquivo, "000000892-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarReferenciaProdutorItensEPagamentosDaNfe892(legado);
                        ValidarReferenciaProdutorItensEPagamentosDaNfe892(novo);
                    }
                    if (string.Equals(nomeArquivo, "000002191-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIcmsSn500DaNfe2191(legado);
                        ValidarIcmsSn500DaNfe2191(novo);
                    }
                    if (string.Equals(nomeArquivo, "000000200-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarCofinsAliquotaDaNfe200(legado);
                        ValidarCofinsAliquotaDaNfe200(novo);
                    }
                    if (string.Equals(nomeArquivo, "000062981-nfe-orig.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarItensImpostosETotaisDaNfce62981(legado);
                        ValidarItensImpostosETotaisDaNfce62981(novo);
                    }
                    if (string.Equals(nomeArquivo, "000000411-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarChaveEInformacoesAdicionaisDaNfce411(legado);
                        ValidarChaveEInformacoesAdicionaisDaNfce411(novo);
                    }
                    if (string.Equals(nomeArquivo, "000027937-nfe.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarIcmsComplementarSemModalidadeSt(legado);
                        ValidarIcmsComplementarSemModalidadeSt(novo);
                    }
                    if (nomeArquivo.StartsWith("RTC2026-", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidarModeloRtc2026(legado, nomeArquivo);
                        ValidarModeloRtc2026(novo, nomeArquivo);
                    }
                    var diferenca = NFeConvertTxtXmlComparer.Comparar(legado, novo);
                    Assert.True(diferenca == null, diferenca);
                }
                finally { if (Directory.Exists(pasta)) Directory.Delete(pasta, true); }
            }
        }

        private static void ValidarPisECofinsDaNfe35814(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            var pis = documento.SelectSingleNode("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='PIS']/*");
            var cofins = documento.SelectSingleNode("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='COFINS']/*");

            Assert.Equal("PISAliq", pis?.LocalName);
            Assert.Equal("01", pis?.SelectSingleNode("*[local-name()='CST']")?.InnerText);
            Assert.Equal("0.00", pis?.SelectSingleNode("*[local-name()='vBC']")?.InnerText);
            Assert.Equal("COFINSAliq", cofins?.LocalName);
            Assert.Equal("01", cofins?.SelectSingleNode("*[local-name()='CST']")?.InnerText);
            Assert.Equal("0.00", cofins?.SelectSingleNode("*[local-name()='vBC']")?.InnerText);
        }

        private static void ValidarChaveEInformacoesAdicionaisDaNfce411(string xml)
        {
            const string chaveEsperada = "35260899999999000191650000000004111000007768";
            var documento = new XmlDocument();
            documento.LoadXml(xml);

            Assert.Equal("NFe" + chaveEsperada, documento.SelectSingleNode("//*[local-name()='infNFe']")?.Attributes?["Id"]?.Value);
            Assert.Equal("00000776", documento.SelectSingleNode("//*[local-name()='ide']/*[local-name()='cNF']")?.InnerText);
            Assert.Equal("8", documento.SelectSingleNode("//*[local-name()='ide']/*[local-name()='cDV']")?.InnerText);
            Assert.Null(documento.SelectSingleNode("//*[local-name()='infAdic']"));
            Assert.Equal("60", documento.SelectSingleNode("//*[local-name()='ICMS60']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("07", documento.SelectSingleNode("//*[local-name()='PISNT']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("07", documento.SelectSingleNode("//*[local-name()='COFINSNT']/*[local-name()='CST']")?.InnerText);
            Assert.Equal("30.00", documento.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
        }

        private static void ValidarIcmsComplementarSemModalidadeSt(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            var icms = documento.SelectSingleNode("//*[local-name()='ICMSSN900']");

            Assert.NotNull(icms);
            Assert.Equal("2", documento.SelectSingleNode("//*[local-name()='ide']/*[local-name()='finNFe']")?.InnerText);
            Assert.Equal("3", icms.SelectSingleNode("*[local-name()='modBC']")?.InnerText);
            Assert.Equal("269.00", icms.SelectSingleNode("*[local-name()='vBC']")?.InnerText);
            Assert.Equal("18.0000", icms.SelectSingleNode("*[local-name()='pICMS']")?.InnerText);
            Assert.Equal("9.98", icms.SelectSingleNode("*[local-name()='vICMS']")?.InnerText);
            Assert.Null(icms.SelectSingleNode("*[local-name()='modBCST']"));
        }

        private static void ValidarModeloRtc2026(string conteudoXml, string nomeArquivo)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var numero = nomeArquivo.Substring("RTC2026-NFe".Length, 3);

            Assert.Equal(numero, xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='nNF']")?.InnerText);
            Assert.Equal("PRODUTO-TESTE", xml.SelectSingleNode("//*[local-name()='prod']/*[local-name()='cProd']")?.InnerText);
            Assert.Equal("SEM GTIN", xml.SelectSingleNode("//*[local-name()='prod']/*[local-name()='cEAN']")?.InnerText);
            Assert.NotNull(xml.SelectSingleNode("//*[local-name()='IBSCBS']"));
            Assert.NotNull(xml.SelectSingleNode("//*[local-name()='IBSCBSTot']"));
            Assert.NotNull(xml.SelectSingleNode("//*[local-name()='vNFTot']"));
            Assert.Equal("RESPONSAVEL TECNICO TESTE", xml.SelectSingleNode("//*[local-name()='infRespTec']/*[local-name()='xContato']")?.InnerText);
            Assert.Equal("AAAAAAAAAAAAAAAAAAAAAAAAAAA=", xml.SelectSingleNode("//*[local-name()='infRespTec']/*[local-name()='hashCSRT']")?.InnerText);

            if (numero == "622")
            {
                Assert.Equal("1", xml.SelectSingleNode("//*[local-name()='prod']/*[local-name()='indBemMovelUsado']")?.InnerText);
                Assert.Equal("95.0000", xml.SelectSingleNode("//*[local-name()='ICMS20']/*[local-name()='pRedBC']")?.InnerText);
                Assert.Equal("20.00", xml.SelectSingleNode("//*[local-name()='IBSCBSTot']/*[local-name()='gIBS']/*[local-name()='vIBS']")?.InnerText);
                Assert.Equal("180.00", xml.SelectSingleNode("//*[local-name()='IBSCBSTot']/*[local-name()='gCBS']/*[local-name()='vCBS']")?.InnerText);
            }

            if (numero == "623")
            {
                Assert.Equal("41260799999999000191550010000006211152363383", xml.SelectSingleNode("//*[local-name()='NFref']/*[local-name()='refNFe']")?.InnerText);
            }
        }

        private static void ValidarImpostosDaNfce161540(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);

            Assert.Equal("65", documento.SelectSingleNode("//*[local-name()='ide']/*[local-name()='mod']")?.InnerText);
            Assert.Null(documento.SelectSingleNode("//*[local-name()='dest']"));
            Assert.Equal("32.00", documento.SelectSingleNode("//*[local-name()='ICMS00']/*[local-name()='vBC']")?.InnerText);
            Assert.Equal("5.44", documento.SelectSingleNode("//*[local-name()='ICMS00']/*[local-name()='vICMS']")?.InnerText);
            Assert.Equal("26.56", documento.SelectSingleNode("//*[local-name()='PISAliq']/*[local-name()='vBC']")?.InnerText);
            Assert.Equal("0.44", documento.SelectSingleNode("//*[local-name()='PISAliq']/*[local-name()='vPIS']")?.InnerText);
            Assert.Equal("26.56", documento.SelectSingleNode("//*[local-name()='COFINSAliq']/*[local-name()='vBC']")?.InnerText);
            Assert.Equal("2.02", documento.SelectSingleNode("//*[local-name()='COFINSAliq']/*[local-name()='vCOFINS']")?.InnerText);
            Assert.Equal("000001", documento.SelectSingleNode("//*[local-name()='IBSCBS']/*[local-name()='cClassTrib']")?.InnerText);
            Assert.Equal("24.10", documento.SelectSingleNode("//*[local-name()='IBSCBS']/*[local-name()='gIBSCBS']/*[local-name()='vBC']")?.InnerText);
        }

        private static void ValidarIcms10DaNfe15493(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            var icms = documento.SelectSingleNode("//*[local-name()='ICMS10']");

            Assert.NotNull(icms);
            Assert.Null(icms.SelectSingleNode("*[local-name()='pMVAST']"));
            Assert.Null(icms.SelectSingleNode("*[local-name()='pRedBCST']"));
            Assert.Equal("5585.21", icms.SelectSingleNode("*[local-name()='vBCST']")?.InnerText);
            Assert.Equal("18.0000", icms.SelectSingleNode("*[local-name()='pICMSST']")?.InnerText);
            Assert.Equal("335.12", icms.SelectSingleNode("*[local-name()='vICMSST']")?.InnerText);
        }

        private static void ValidarReferenciaProdutorItensEPagamentosDaNfe892(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            var referencia = documento.SelectSingleNode("//*[local-name()='ide']/*[local-name()='NFref']/*[local-name()='refNFP']");

            Assert.NotNull(referencia);
            Assert.Null(referencia.SelectSingleNode("*[local-name()='CNPJ']"));
            Assert.Equal("11144477735", referencia.SelectSingleNode("*[local-name()='CPF']")?.InnerText);
            Assert.Equal("1234567890", referencia.SelectSingleNode("*[local-name()='IE']")?.InnerText);
            Assert.Equal("04", referencia.SelectSingleNode("*[local-name()='mod']")?.InnerText);
            Assert.Equal("890", referencia.SelectSingleNode("*[local-name()='serie']")?.InnerText);
            Assert.Equal("1", referencia.SelectSingleNode("*[local-name()='nNF']")?.InnerText);
            Assert.Equal(6, documento.SelectNodes("//*[local-name()='det']").Count);
            Assert.Equal(6, documento.SelectNodes("//*[local-name()='ICMSSN102']").Count);
            Assert.Equal(6, documento.SelectNodes("//*[local-name()='ICMSSN102']/*[local-name()='orig' and text()='0']").Count);
            Assert.Equal(6, documento.SelectNodes("//*[local-name()='ICMSSN102']/*[local-name()='CSOSN' and text()='102']").Count);
            Assert.Equal(0, documento.SelectNodes("//*[local-name()='IPI']/*[local-name()='CNPJProd']").Count);
            Assert.Equal(0, documento.SelectNodes("//*[local-name()='prod']/*[local-name()='indEscala']").Count);
            Assert.Equal("4700.00", documento.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
            Assert.Equal(0, documento.SelectNodes("//*[local-name()='cobr']/*[local-name()='fat']").Count);
            Assert.Equal(1, documento.SelectNodes("//*[local-name()='pag']/*[local-name()='detPag']").Count);
            Assert.Equal(1, documento.SelectNodes("//*[local-name()='detPag']/*[local-name()='tPag' and text()='90']").Count);
            Assert.Equal(1, documento.SelectNodes("//*[local-name()='detPag']/*[local-name()='vPag' and text()='0.00']").Count);
            Assert.Equal(0, documento.SelectNodes("//*[local-name()='detPag']/*[local-name()='indPag']").Count);
            Assert.Equal(0, documento.SelectNodes("//*[local-name()='detPag']/*[local-name()='xPag']").Count);
        }

        private static void ValidarIcmsSn500DaNfe2191(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);

            Assert.Equal(2, documento.SelectNodes("//*[local-name()='ICMSSN500']").Count);
            Assert.Equal(2, documento.SelectNodes("//*[local-name()='ICMSSN500']/*[local-name()='orig' and text()='0']").Count);
            Assert.Equal(2, documento.SelectNodes("//*[local-name()='ICMSSN500']/*[local-name()='CSOSN' and text()='500']").Count);
            Assert.Equal(0, documento.SelectNodes("//*[local-name()='ICMSSN102']").Count);
            Assert.Equal("OUTROS MEIOS", documento.SelectSingleNode("//*[local-name()='detPag']/*[local-name()='xPag']")?.InnerText);
        }

        private static void ValidarCofinsAliquotaDaNfe200(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            var grupos = documento.SelectNodes("//*[local-name()='COFINSAliq']");

            Assert.Equal(3, grupos.Count);
            Assert.Equal(3, documento.SelectNodes("//*[local-name()='COFINSAliq']/*[local-name()='CST' and text()='01']").Count);
            Assert.Equal(0, documento.SelectNodes("//*[local-name()='COFINSOutr']").Count);
            Assert.Equal("45007.60", grupos[0].SelectSingleNode("*[local-name()='vBC']")?.InnerText);
            Assert.Equal("3.0000", grupos[0].SelectSingleNode("*[local-name()='pCOFINS']")?.InnerText);
            Assert.Equal("1350.23", grupos[0].SelectSingleNode("*[local-name()='vCOFINS']")?.InnerText);
        }

        private static void ValidarItensImpostosETotaisDaNfce62981(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);

            Assert.Equal("65", documento.SelectSingleNode("//*[local-name()='ide']/*[local-name()='mod']")?.InnerText);
            Assert.Equal(3, documento.SelectNodes("//*[local-name()='det']").Count);
            Assert.Equal(3, documento.SelectNodes("//*[local-name()='prod']/*[local-name()='CEST']").Count);
            Assert.Equal(1, documento.SelectNodes("//*[local-name()='ICMS60']").Count);
            Assert.Equal(2, documento.SelectNodes("//*[local-name()='ICMS00']").Count);
            Assert.Equal(3, documento.SelectNodes("//*[local-name()='PISNT']/*[local-name()='CST' and text()='07']").Count);
            Assert.Equal(3, documento.SelectNodes("//*[local-name()='COFINSNT']/*[local-name()='CST' and text()='07']").Count);
            Assert.Equal(3, documento.SelectNodes("//*[local-name()='IBSCBS']").Count);
            Assert.Equal("18.00", documento.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vBC']")?.InnerText);
            Assert.Equal("3.24", documento.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vICMS']")?.InnerText);
            Assert.Equal("107.00", documento.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vProd']")?.InnerText);
            Assert.Equal("107.00", documento.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
            Assert.Equal("27.28", documento.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vTotTrib']")?.InnerText);
            Assert.Equal("103.76", documento.SelectSingleNode("//*[local-name()='IBSCBSTot']/*[local-name()='vBCIBSCBS']")?.InnerText);
            Assert.Equal("107.00", documento.SelectSingleNode("//*[local-name()='vNFTot']")?.InnerText);
            Assert.Equal("04", documento.SelectSingleNode("//*[local-name()='detPag']/*[local-name()='tPag']")?.InnerText);
            Assert.Equal("107.00", documento.SelectSingleNode("//*[local-name()='detPag']/*[local-name()='vPag']")?.InnerText);
            Assert.Equal("2", documento.SelectSingleNode("//*[local-name()='detPag']/*[local-name()='card']/*[local-name()='tpIntegra']")?.InnerText);
        }

        private static string OmitirPaisBrasilOpcionalDeRetiradaEEntrega(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            foreach (XmlElement local in documento.SelectNodes("//*[local-name()='retirada' or local-name()='entrega']"))
            {
                var codigoPais = local.SelectSingleNode("*[local-name()='cPais' and text()='1058']");
                var nomePais = local.SelectSingleNode("*[local-name()='xPais' and translate(text(), 'brasil', 'BRASIL')='BRASIL']");
                if (codigoPais != null)
                {
                    local.RemoveChild(codigoPais);
                }
                if (nomePais != null)
                {
                    local.RemoveChild(nomePais);
                }
            }
            return documento.OuterXml;
        }

        private static void ValidarProdutoIpiEReformaDaNFe2320(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            var produto = documento.SelectSingleNode("//*[local-name()='det']/*[local-name()='prod']");

            Assert.Equal("ABRACADEIRA ROSCA SEM FIM 51X64(200X212) INCA", produto.SelectSingleNode("*[local-name()='xProd']")?.InnerText);
            Assert.Equal("73269090", produto.SelectSingleNode("*[local-name()='NCM']")?.InnerText);
            Assert.Equal("1006200", produto.SelectSingleNode("*[local-name()='CEST']")?.InnerText);
            Assert.Equal("SP010830", produto.SelectSingleNode("*[local-name()='cBenef']")?.InnerText);
            Assert.Null(produto.SelectSingleNode("*[local-name()='EXTIPI']"));
            Assert.Equal("5124", produto.SelectSingleNode("*[local-name()='CFOP']")?.InnerText);
            Assert.Equal("500.0000", produto.SelectSingleNode("*[local-name()='qCom']")?.InnerText);
            Assert.Equal("53", documento.SelectSingleNode("//*[local-name()='IPI']//*[local-name()='CST']")?.InnerText);
            Assert.Equal("4219.50", documento.SelectSingleNode("//*[local-name()='IBSCBS']/*[local-name()='gIBSCBS']/*[local-name()='vBC']")?.InnerText);
            Assert.Equal("4392.18", documento.SelectSingleNode("//*[local-name()='det']/*[local-name()='vItem']")?.InnerText);
            Assert.Equal("4261.68", documento.SelectSingleNode("//*[local-name()='total']/*[local-name()='vNFTot']")?.InnerText);
        }

        private static void ValidarPagamentosDaNfce17136(string xml)
        {
            var documento = new XmlDocument();
            documento.LoadXml(xml);
            var pagamentos = documento.SelectNodes("//*[local-name()='detPag']");
            var cartoes = documento.SelectNodes("//*[local-name()='detPag']/*[local-name()='card']");

            Assert.Equal(5, pagamentos.Count);
            Assert.Equal(4, cartoes.Count);
            Assert.Equal("196.00", pagamentos[0].SelectSingleNode("*[local-name()='vPag']")?.InnerText);
            Assert.Equal("295.00", pagamentos[1].SelectSingleNode("*[local-name()='vPag']")?.InnerText);
            Assert.Equal("300.00", pagamentos[2].SelectSingleNode("*[local-name()='vPag']")?.InnerText);
            Assert.Equal("65.00", pagamentos[3].SelectSingleNode("*[local-name()='vPag']")?.InnerText);
            Assert.Equal("24.00", pagamentos[4].SelectSingleNode("*[local-name()='vPag']")?.InnerText);
            Assert.Equal("AUT001", cartoes[0].SelectSingleNode("*[local-name()='cAut']")?.InnerText);
            Assert.Equal("AUT004", cartoes[3].SelectSingleNode("*[local-name()='cAut']")?.InnerText);
        }

        [Fact]
        public void MassasTxtNaoDevemConterDadosIdentificaveisConhecidos()
        {
            var dadosIdentificaveis = new[]
            {
                "AGILLE COMERCIO DE MEDICAMENTOS LTDA",
                "OON ONCOLOGIA, ORTOPEDIA E NEUROLOGIA VET LTDA",
                "cmanhaesvet@gmail.com",
                "AV DAS AGUIAS",
                "RUA FELIPE NEVES",
                "AVENIDA ATLANTICA N 720",
                "nfe@agillemed.com.br",
                "OXI GENESES COM.GASES EQUIPAMENTOS LTDA EPP",
                "METACAULIM BRASIL INDUSTRIA COMERCIO LTDA",
                "RUA  AGOSTINHO BALESTRIN",
                "AV.HUMBERTO CERESER",
                "vendas@metacaulim.com.br",
                "LOTUS CENTRAL DE DIST DE HIGIENICOS LTDA",
                "TEXTIL BICOLOR INDUSTRIA E COM DE CONFEC",
                "R DR JOAO ALTES DE LIMA",
                "VENDEDOR: VIVIANE",
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
                "SOC.COM.MAT.P/CONSTR.LUIZ LOPES LTDA",
                "108680702113",
                "RUA MAJOR OTAVIANO",
                "ROD. RAPOSO TAVARES KM-18 5",
                "100441666118",
                "60561719000557",
                "COMERCIO DE PRODUTOS AGROVETERINARIOS LTDA",
                "CASA DO FAZENDEIRO",
                "00131341545",
                "36912368000173",
                "AV MATO GROSSO 201",
                "6634381569",
                "JOSE ADELMO DE JESUS",
                "45184984100",
                "RUA JOSE ANDRE VAJAO",
                "VOL IMPORTS - MG",
                "0032376020050",
                "30999720000173",
                "AV DOUTOR ROFLES CECILIO",
                "3432124039",
                "MILLS PESADOS LOCACAO SERVICOS E LOGISTICA SA",
                "671666958115",
                "gestaonotas.pesados@mills.com.br",
                "01633840003099",
                "R FIORAVANTE MANCINO",
                "1154306482",
                "CLIENTE RETIRA",
                "DHIEFFERSON FELIPE RENDE SANTOS",
                "5500021454",
                "SN/013244",
                "FROTA 1417",
                "M. L. SCHWERTNER PLANTAS",
                "MLS PLANTAS",
                "1280058657",
                "28508340000147",
                "PRIMEIRO DE MAIO",
                "51997012925",
                "PAULO JAIR HOLDEFER",
                "05579449020",
                "2131010863",
                "R BOA VISTA",
                "5135624755",
                "JASMIM PLANTAS ORNAMENTAIS",
                "00001280019163",
                "R RS 122 KM 09",
                "87215802000105",
                "68711275",
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
                "420396)",
                "ALTO DA BOA VISTA MATERIAS DE CONSTRUCAO LTDA",
                "CENTERKASA",
                "RUA JACINTO RAMOS",
                "6235133655",
                "58033)",
                "TINTA LEINERTEX ACR FOSCA 18L AREIA",
                "7898360090686",
                "08561701000101",
                "082853",
                "739532",
                "430893",
                "526811",
                "FS HOME SIGNS COMERCIO DE PLASTICOS LTDA",
                "451219138113",
                "65624784000174",
                "RUA MARIO SAURIN",
                "PARQUE DOS BURITIS",
                "17991926177",
                "ORION HOME DESIGN LTDA",
                "191052997118",
                "61182144000109",
                "ESTRADA ARTUR FORNAZARI",
                "LIMOEIRO",
                "Referente Pedido Nr.: 828",
                "REDE CAFE TANTA LTDA",
                "CAFE TANTA",
                "224291586114",
                "30985309000149",
                "AVENIDA PROFESSOR JOSE PEDRETTI NETO",
                "CONJ HAB FREI FIDELI",
                "01438136127",
                "ACES CABO 1.2MT IPHONE",
                "PAO DE QUEIJO UN",
                "NESCAFE CHOCOLATE ALPINO",
                "Op: LUCELIA",
                "B.B. DE OLIVEIRA CENTRO AUTOMOTIVO - EPP",
                "QUITANDA DOS PNEUS",
                "379260061119",
                "24531255000149",
                "ESTRADA DE SAO BENTO",
                "JD ODETE",
                "08598100",
                "1146458785",
                "35260824531255000149650000000004111000007760",
                "ELEB-MATERIAIS ELETRICOS LTDA",
                "0010379490048",
                "08746947000158",
                "RUA MARTINS BARBOSA",
                "36090300",
                "3232221948",
                "POSTO ECO LTDA",
                "3670838040021",
                "03845434000180",
                "AV BRASIL",
                "36081500",
                "31260608746947000158550000000274421121715254"
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

        private static void ValidarInfAdProdAposNormalizacao(string conteudoXml)
        {
            var nfe = XMLUtility.Deserializar<Unimake.Business.DFe.Xml.NFe.NFe>(conteudoXml);
            var xml = XMLUtility.Serializar(nfe);
            var detalhe = xml.SelectSingleNode("//*[local-name()='det']");
            var produto = detalhe.SelectSingleNode("*[local-name()='prod']");
            var imposto = detalhe.SelectSingleNode("*[local-name()='imposto']");
            var informacaoAdicional = detalhe.SelectSingleNode("*[local-name()='infAdProd']");

            Assert.NotNull(produto);
            Assert.NotNull(imposto);
            Assert.NotNull(informacaoAdicional);
            Assert.Same(produto, detalhe.FirstChild);
            Assert.Same(imposto, informacaoAdicional.PreviousSibling);
            Assert.Equal("INFORMACAO ADICIONAL DO ITEM PARA TESTE DE ORDENACAO", informacaoAdicional.InnerText);
        }

        private static void ValidarDivergenciaVOutroInformadaPeloErp(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var itens = xml.SelectNodes("//*[local-name()='det']/*[local-name()='prod']/*[local-name()='vOutro']");

            Assert.Equal(2, itens.Count);
            Assert.Equal("0.65", itens[0].InnerText);
            Assert.Equal("0.64", itens[1].InnerText);
            Assert.Equal("0.00", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vOutro']")?.InnerText);
            Assert.Equal("654.40", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
            Assert.Equal("1.29", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vTotTrib']")?.InnerText);
        }

        private static void ValidarDivergenciaVtotTribInformadaPeloErp(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);

            Assert.Equal(0, xml.SelectNodes("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='vTotTrib']").Count);
            Assert.Equal("2.80", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vTotTrib']")?.InnerText);
        }

        private static void ValidarTotaisDaNfceDevolucao23655(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);

            Assert.Equal("65", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='mod']")?.InnerText);
            Assert.Equal("35260899999999000191550010000000011000000017", xml.SelectSingleNode("//*[local-name()='ide']/*[local-name()='NFref']/*[local-name()='refNFe']")?.InnerText);
            Assert.Equal("27.58", xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='vTotTrib']")?.InnerText);
            Assert.Equal("85.00", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vProd']")?.InnerText);
            Assert.Equal("85.00", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
            Assert.Equal("27.58", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vTotTrib']")?.InnerText);
            Assert.Equal("20", xml.SelectSingleNode("//*[local-name()='detPag']/*[local-name()='tPag']")?.InnerText);
            Assert.Equal("85.00", xml.SelectSingleNode("//*[local-name()='detPag']/*[local-name()='vPag']")?.InnerText);
        }

        private static void ValidarCobrancaPagamentosEReformaDaNfe398(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var pagamentos = xml.SelectNodes("//*[local-name()='detPag']");

            Assert.Equal(2, xml.SelectNodes("//*[local-name()='dup']").Count);
            Assert.Equal(2, pagamentos.Count);
            Assert.Equal("01", pagamentos[0].SelectSingleNode("*[local-name()='tPag']")?.InnerText);
            Assert.Equal("50.00", pagamentos[0].SelectSingleNode("*[local-name()='vPag']")?.InnerText);
            Assert.Equal("99", pagamentos[1].SelectSingleNode("*[local-name()='tPag']")?.InnerText);
            Assert.Equal("NAO INFORMADO", pagamentos[1].SelectSingleNode("*[local-name()='xPag']")?.InnerText);
            Assert.Equal("50.00", pagamentos[1].SelectSingleNode("*[local-name()='vPag']")?.InnerText);
            Assert.Equal("30.96", xml.SelectSingleNode("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='vTotTrib']")?.InnerText);
            Assert.Equal("0.10", xml.SelectSingleNode("//*[local-name()='gIBSUF']/*[local-name()='vIBSUF']")?.InnerText);
            Assert.Equal("0.90", xml.SelectSingleNode("//*[local-name()='gCBS']/*[local-name()='vCBS']")?.InnerText);
            Assert.Equal("100.00", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
        }

        private static void ValidarIpiEItemForaDoTotalDaNfe399(string conteudoXml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXml);
            var ipiTrib = xml.SelectSingleNode("//*[local-name()='IPITrib']");

            Assert.Equal("0", xml.SelectSingleNode("//*[local-name()='prod']/*[local-name()='indTot']")?.InnerText);
            Assert.Equal("50", ipiTrib.SelectSingleNode("*[local-name()='CST']")?.InnerText);
            Assert.Equal("0.00", ipiTrib.SelectSingleNode("*[local-name()='vBC']")?.InnerText);
            Assert.Equal("0.0000", ipiTrib.SelectSingleNode("*[local-name()='pIPI']")?.InnerText);
            Assert.Equal("5.00", ipiTrib.SelectSingleNode("*[local-name()='vIPI']")?.InnerText);
            Assert.Equal("0.01", xml.SelectSingleNode("//*[local-name()='gIBSUF']/*[local-name()='vIBSUF']")?.InnerText);
            Assert.Equal("0.05", xml.SelectSingleNode("//*[local-name()='gCBS']/*[local-name()='vCBS']")?.InnerText);
            Assert.Equal("5.00", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vIPI']")?.InnerText);
            Assert.Equal("5.00", xml.SelectSingleNode("//*[local-name()='ICMSTot']/*[local-name()='vNF']")?.InnerText);
            Assert.Equal("90", xml.SelectSingleNode("//*[local-name()='detPag']/*[local-name()='tPag']")?.InnerText);
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
