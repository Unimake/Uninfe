using NFe.Service;
using NFe.Service.NFeABI;
using System;
using System.IO;
using System.Xml;
using Xunit;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class ConsultaProtocoloNFeABITests : NFeABITestFixture
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ConsultaRecuperaNotaPorChaveOuFluxoEArquivaProcessado(bool fluxo, bool somenteDistribuicao)
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                contexto.Empresa.SalvarSomenteXMLDistribuicao = somenteDistribuicao;
                var arquivo = contexto.GuardarDocumento("nome-erp-nfeabi.xml", fluxo);
                var original = File.ReadAllText(arquivo);
                var retorno = Retorno("retConsSitNFeABI", 100, Protocolo());
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString = retorno;

                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));

                var proc = contexto.Destino("nome-erp-procNFeABI.xml");
                Assert.True(File.Exists(proc));
                Assert.Equal(original, File.ReadAllText(contexto.Destino("nome-erp-nfeabi.xml", somenteDistribuicao ? "Originais" : "Autorizados")));
                Assert.Equal(retorno, File.ReadAllText(Path.Combine(contexto.PastaRetorno, "consulta-sit.xml")));
                Assert.False(File.Exists(arquivo));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
                var processado = CarregarXml(proc);
                Assert.Equal("nfeabiProc", processado.DocumentElement.LocalName);
                Assert.Equal(Documento().DocumentElement.OuterXml, processado.GetElementsByTagName("NFeABI")[0].OuterXml);
                Assert.Equal(Protocolo(), processado.GetElementsByTagName("protNFeABI")[0].OuterXml);
                Assert.Equal(string.Empty, new FluxoNfe(0).LerTag("NFeABI" + Chave, FluxoNfe.ElementoFixo.ArqNFe));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void DocumentoAusenteOuDeOutraEmpresaPreservaRetornoSemArquivar(bool outraEmpresa)
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                string arquivo = null;
                if(outraEmpresa)
                {
                    arquivo = contexto.GuardarDocumento();
                    contexto.Empresa.CNPJ = "99999999000191";
                }
                var retorno = Retorno("retConsSitNFeABI", 100, Protocolo());
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString = retorno;
                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));

                Assert.Equal(retorno, File.ReadAllText(Path.Combine(contexto.PastaRetorno, "consulta-sit.xml")));
                Assert.False(File.Exists(contexto.Destino("pedido-procNFeABI.xml")));
                if(arquivo != null) Assert.True(File.Exists(arquivo));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ComparacaoDeDigestRespeitaConfiguracao(bool comparar)
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                contexto.Empresa.CompararDigestValueDFeRetornadoSEFAZ = comparar;
                var arquivo = contexto.GuardarDocumento();
                var documento = CarregarXml(arquivo);
                var assinatura = documento.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
                var digest = documento.CreateElement("DigestValue", assinatura.NamespaceURI);
                digest.InnerText = "AA==";
                assinatura.AppendChild(digest);
                documento.DocumentElement.AppendChild(assinatura);
                documento.Save(arquivo);
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta =>
                    consulta.RetornoWSString = Retorno("retConsSitNFeABI", 100, Protocolo(digest: "AQ=="));

                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));

                Assert.Equal(!comparar, File.Exists(contexto.Destino("pedido-procNFeABI.xml")));
                Assert.Equal(comparar, File.Exists(Path.Combine(contexto.PastaRetorno, "consulta-sit.err")));
                if(comparar) Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, "pedido-nfeabi.xml")));
            }
        }

        [Theory]
        [InlineData(100, "")]
        [InlineData(101, "")]
        [InlineData(102, "")]
        [InlineData(100, "<protNFeABI versao=\"1.00\" />")]
        public void RetornoSemProtocoloUtilizavelMantemNotaEmProcessamento(int status, string protocolo)
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                var arquivo = contexto.GuardarDocumento();
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString = Retorno("retConsSitNFeABI", status, protocolo);
                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));
                Assert.True(File.Exists(arquivo));
                Assert.False(File.Exists(contexto.Destino("pedido-procNFeABI.xml")));
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, "consulta-sit.err")));
            }
        }

        [Theory]
        [InlineData(101, 100)]
        [InlineData(150, 150)]
        public void ConsultaUsaProtocoloDeAutorizacaoMesmoAposCancelamento(int status, int statusProtocolo)
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                contexto.GuardarDocumento();
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString =
                    Retorno("retConsSitNFeABI", status, Protocolo(status: statusProtocolo));
                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));
                Assert.True(File.Exists(contexto.Destino("pedido-procNFeABI.xml")));
            }
        }

        [Theory]
        [InlineData(216)]
        [InlineData(217)]
        public void RejeicaoRetiraNotaDoFluxo(int status)
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                var arquivo = contexto.GuardarDocumento(fluxo: true);
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString = Retorno("retConsSitNFeABI", status);
                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));
                Assert.False(File.Exists(arquivo));
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, "pedido-nfeabi.xml")));
                Assert.Equal(string.Empty, new FluxoNfe(0).LerTag("NFeABI" + Chave, FluxoNfe.ElementoFixo.ArqNFe));
            }
        }

        [Fact]
        public void FalhaDeMovimentacaoDoProcessadoMantemOriginalEmProcessamento()
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                var arquivo = contexto.GuardarDocumento();
                Directory.CreateDirectory(contexto.Destino("pedido-procNFeABI.xml"));
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString = Retorno("retConsSitNFeABI", 100, Protocolo());
                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));
                Assert.True(File.Exists(arquivo));
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "consulta-sit.err")));
            }
        }

        [Fact]
        public void ErroDeTransporteMantemExcecaoOriginalEExcluiPedido()
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => { throw new Exception("Falha DNS de teste da DLL."); };
                Executar(new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio));
                Assert.Contains("Message|Falha DNS de teste da DLL.", File.ReadAllText(Path.Combine(contexto.PastaRetorno, "consulta-sit.err")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
            }
        }

        [Fact]
        public void RoteamentoRealExecutaConsultaSemRede()
        {
            using(var contexto = new Contexto("consulta-ped-sit.xml", PedidoConsulta()))
            {
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString = Retorno("retConsSitNFeABI", 217);
                Processar(contexto);
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "consulta-sit.xml")));
            }
        }
    }
}
