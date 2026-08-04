using System;
using System.IO;
using Unimake.Business.DFe.Xml.NFe;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class DllTxtConversionTests
    {
        private readonly NFeConvertTxtFixture fixture = new NFeConvertTxtFixture();

        [Theory]
        [MemberData(nameof(NFeConvertTxtFixture.ArquivosNFe400), MemberType = typeof(NFeConvertTxtFixture))]
        public void ConversorDaDllDevePreservarXmlDoConversorAtual(string arquivo, bool sucessoEsperado)
        {
            if (!sucessoEsperado) return;

            using (var legado = fixture.Converter(arquivo))
            {
                Assert.True(legado.Sucesso, legado.MensagemErro);

                var novo = new NFeTxtConverter().Converter(arquivo);
                Assert.True(novo.Sucesso, novo.MensagemErro);
                Assert.Equal(legado.Arquivos.Count, novo.Documentos.Count);

                for (var indice = 0; indice < legado.Arquivos.Count; indice++)
                {
                    var esperado = legado.Arquivos[indice];
                    var atual = novo.Documentos[indice];
                    Assert.Equal(esperado.Chave, atual.Chave);
                    Assert.Equal(esperado.Numero, atual.Numero);
                    Assert.Equal(esperado.Serie, atual.Serie);

                    var diferenca = NFeConvertTxtXmlComparer.Comparar(File.ReadAllText(esperado.Caminho), atual.Xml);
                    Assert.True(diferenca == null, Path.GetFileName(arquivo) + ": " + diferenca);
                }
            }
        }
    }
}
