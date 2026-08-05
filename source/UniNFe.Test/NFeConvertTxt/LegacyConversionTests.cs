using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using NFe.ConvertTxt;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class LegacyConversionTests
    {
        private readonly NFeConvertTxtFixture fixture = new NFeConvertTxtFixture();

        [Theory]
        [MemberData(nameof(NFeConvertTxtFixture.ArquivosNFe400), MemberType = typeof(NFeConvertTxtFixture))]
        public void ConverterArquivo400DeveManterResultadoAtual(string arquivoOrigem, bool sucessoEsperado)
        {
            using (var resultado = fixture.Converter(arquivoOrigem))
            {
                Assert.True(sucessoEsperado == resultado.Sucesso, Path.GetFileName(arquivoOrigem));

                if (!sucessoEsperado)
                {
                    Assert.False(string.IsNullOrEmpty(resultado.MensagemErro), Path.GetFileName(arquivoOrigem));
                    Assert.Empty(resultado.Arquivos);
                    return;
                }

                Assert.True(string.IsNullOrEmpty(resultado.MensagemErro), resultado.MensagemErro);
                Assert.NotEmpty(resultado.Arquivos);

                foreach (var arquivo in resultado.Arquivos)
                {
                    Assert.True(File.Exists(arquivo.Caminho), arquivo.Caminho);
                    Assert.EndsWith("-nfe.xml", arquivo.Caminho.ToLowerInvariant());
                    Assert.Equal(44, arquivo.Chave.Length);
                    Assert.True(arquivo.Numero > 0);
                    Assert.True(arquivo.Serie >= 0);

                    var xml = new XmlDocument();
                    xml.Load(arquivo.Caminho);
                    Assert.Equal("NFe", xml.DocumentElement.LocalName);
                    Assert.Equal("http://www.portalfiscal.inf.br/nfe", xml.DocumentElement.NamespaceURI);

                    var namespaceManager = new XmlNamespaceManager(xml.NameTable);
                    namespaceManager.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");
                    var infNFe = (XmlElement)xml.SelectSingleNode("/nfe:NFe/nfe:infNFe", namespaceManager);
                    Assert.NotNull(infNFe);
                    Assert.Equal("4.00", infNFe.GetAttribute("versao"));
                }
            }
        }

        [Fact]
        public void PagamentoAntecipadoDeveUsarRefNFeNoLayoutENoXml()
        {
            var arquivo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "RTC", "NFe_Reforma_Tributaria-nfe.txt");

            using (var resultado = fixture.Converter(arquivo))
            {
                Assert.True(resultado.Sucesso, resultado.MensagemErro);
                var xml = new XmlDocument();
                xml.Load(Assert.Single(resultado.Arquivos).Caminho);

                Assert.Equal(3, xml.SelectNodes("//*[local-name()='gPagAntecipado']/*[local-name()='refNFe']").Count);
                Assert.Null(xml.SelectSingleNode("//*[local-name()='gPagAntecipado']/*[local-name()='refDFe']"));
            }

            var tipoCatalogo = typeof(ConversaoTXT).Assembly.GetType("NFe.ConvertTxt.NFeTxtLayoutCatalog", true);
            var criar = tipoCatalogo.GetMethod("Criar", BindingFlags.Static | BindingFlags.NonPublic);
            var layouts = (IDictionary)criar.Invoke(null, null);
            Assert.EndsWith("BC01|refNFe|", (string)layouts["BC01"]);
        }

        [Fact]
        public void ComparadorDeveIgnorarDeclaracaoEspacosEOrdemDosAtributos()
        {
            const string esperado = "<?xml version=\"1.0\"?><NFe xmlns=\"urn:nfe\"><infNFe versao=\"4.00\" Id=\"NFe1\"><ide /></infNFe></NFe>";
            const string atual = "<NFe xmlns=\"urn:nfe\">\n  <infNFe Id=\"NFe1\" versao=\"4.00\">\n    <ide/>\n  </infNFe>\n</NFe>";

            Assert.Null(NFeConvertTxtXmlComparer.Comparar(esperado, atual));
        }

        [Fact]
        public void ComparadorDeveInformarCaminhoDoValorDiferente()
        {
            const string esperado = "<NFe xmlns=\"urn:nfe\"><infNFe><ide><mod>55</mod></ide></infNFe></NFe>";
            const string atual = "<NFe xmlns=\"urn:nfe\"><infNFe><ide><mod>65</mod></ide></infNFe></NFe>";

            var diferenca = NFeConvertTxtXmlComparer.Comparar(esperado, atual);

            Assert.Contains("/mod", diferenca);
            Assert.Contains("55", diferenca);
            Assert.Contains("65", diferenca);
        }
    }
}
