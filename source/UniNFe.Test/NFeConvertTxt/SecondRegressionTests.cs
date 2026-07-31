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
                "AVENIDA JOAO SANZOVO"
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
    }
}
