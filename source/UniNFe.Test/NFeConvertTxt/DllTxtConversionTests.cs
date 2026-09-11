using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using NFe.ConvertTxt;
using NFe.Service;
using NFe.Settings;
using Unimake.Business.DFe.Servicos;
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

        [Fact]
        public void NFeWDeveUsarLeiauteMonofasicoLegadoEmTodosOsAmbientes()
        {
            var arquivo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "RTC", "NFe_Reforma_Tributaria_Monofasica-nfe.txt");

            using (var resultado = fixture.Converter(arquivo))
            {
                Assert.True(resultado.Sucesso, resultado.MensagemErro);
                var conversor = new NFeW();

                resultado.Nota.ide.tpAmb = TipoAmbiente.Producao;
                conversor.GerarXml(resultado.Nota, null, arquivo);
                var producao = new XmlDocument();
                producao.LoadXml(conversor.XMLString);
                Assert.NotNull(producao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gMonoPadrao']"));
                Assert.Null(producao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gIBSMonoAdRem']"));

                resultado.Nota.ide.tpAmb = TipoAmbiente.Homologacao;
                conversor.GerarXml(resultado.Nota, null, arquivo);
                var homologacao = new XmlDocument();
                homologacao.LoadXml(conversor.XMLString);
                Assert.NotNull(homologacao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gMonoPadrao']"));
                Assert.Null(homologacao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gIBSMonoAdRem']"));
            }
        }

        [Fact]
        public void ServicoDeveUsarLeiauteMonofasicoLegadoEmTodosOsAmbientes()
        {
            var producao = ConverterPeloServico((int)TipoAmbiente.Producao);
            var homologacao = ConverterPeloServico((int)TipoAmbiente.Homologacao);

            Assert.NotNull(producao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gMonoPadrao']"));
            Assert.Null(producao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gIBSMonoAdRem']"));
            Assert.NotNull(homologacao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gMonoPadrao']"));
            Assert.Null(homologacao.SelectSingleNode("//*[local-name()='gIBSCBSMono']/*[local-name()='gIBSMonoAdRem']"));
        }

        private static XmlDocument ConverterPeloServico(int ambienteCodigo)
        {
            var raiz = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
            var pastaTemp = Path.Combine(raiz, "Temp");
            var pastaRetorno = Path.Combine(raiz, "Retorno");
            var pastaErro = Path.Combine(raiz, "Erro");
            var pastaEnvio = Path.Combine(raiz, "Envio");
            var pastaValidar = Path.Combine(raiz, "Validar");
            Directory.CreateDirectory(pastaTemp);
            Directory.CreateDirectory(pastaRetorno);
            Directory.CreateDirectory(pastaErro);
            Directory.CreateDirectory(pastaEnvio);
            Directory.CreateDirectory(pastaValidar);

            var origem = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "RTC", "NFe_Reforma_Tributaria_Monofasica-nfe.txt");
            var arquivo = Path.Combine(pastaTemp, Path.GetFileName(origem));
            File.Copy(origem, arquivo);
            var configuracoesOriginais = Empresas.Configuracoes;

            try
            {
                Empresas.Configuracoes = new List<Empresa>
                {
                    new Empresa
                    {
                        AmbienteCodigo = ambienteCodigo,
                        PastaXmlRetorno = pastaRetorno,
                        PastaXmlErro = pastaErro,
                        PastaXmlEnvio = pastaEnvio,
                        PastaValidar = pastaValidar
                    }
                };

                Exception erro = null;
                var thread = new Thread(() =>
                {
                    try
                    {
                        new ConverterTXT(arquivo);
                    }
                    catch (Exception ex)
                    {
                        erro = ex;
                    }
                })
                {
                    Name = "0"
                };
                thread.Start();
                thread.Join();
                Assert.Null(erro);

                var xml = new XmlDocument();
                xml.Load(Assert.Single(Directory.GetFiles(raiz, "*-nfe.xml", SearchOption.TopDirectoryOnly)));
                return xml;
            }
            finally
            {
                Empresas.Configuracoes = configuracoesOriginais;
                if (Directory.Exists(raiz))
                {
                    Directory.Delete(raiz, true);
                }
            }
        }
    }
}
