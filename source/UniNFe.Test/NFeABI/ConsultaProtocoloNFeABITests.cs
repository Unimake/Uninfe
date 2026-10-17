using NFe.Components;
using NFe.Service;
using NFe.Service.NFeABI;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using UniNFe.Test.Abstractions;
using Unimake.Business.DFe.Servicos;
using Xunit;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class ConsultaProtocoloNFeABITests : TaskTestFixtureBase, IDisposable
    {
        private const string Chave = "41260912345678000123770010000000011012345670";

        [Fact]
        public void ConsultaPreservaXmlBrutoEConfiguraTransporte()
        {
            using(var contexto = new Contexto(Pedido()))
            {
                var retorno = Retorno();
                string recebido = null;
                Configuracao configuracao = null;
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = (xml, config) =>
                {
                    recebido = xml;
                    configuracao = config;
                    return retorno;
                };

                ExecutarEmThread("0", () => new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio).Execute());

                Assert.Contains("<chNFeABI>" + Chave + "</chNFeABI>", recebido);
                Assert.Equal(TipoDFe.NFeABI, configuracao.TipoDFe);
                Assert.Equal(41, configuracao.CodigoUF);
                Assert.Equal(TipoAmbiente.Homologacao, configuracao.TipoAmbiente);
                Assert.Equal(retorno, File.ReadAllText(Path.Combine(contexto.PastaRetorno, "consulta-sit.xml")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, "consulta-sit.err")));
            }
        }

        [Fact]
        public void AdaptadorRealSelecionaServicoEModeloSemExecutarTransporte()
        {
            using(var consulta = new Unimake.Business.DFe.Servicos.NFeABI.ConsultaProtocolo(
                Pedido(), new Configuracao()))
            {
                Assert.Equal(Servico.NFeABIConsultaProtocolo, consulta.Configuracoes.Servico);
                Assert.Equal(ModeloDFe.NFeABI, consulta.Configuracoes.Modelo);
                Assert.Equal(TipoAmbiente.Homologacao, consulta.Configuracoes.TipoAmbiente);
            }
        }

        [Fact]
        public void RoteamentoExecutaConsultaSemRede()
        {
            using(var contexto = new Contexto(Pedido()))
            {
                var checarConexaoAnterior = ConfiguracaoApp.ChecarConexaoInternet;
                var executou = false;
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = (xml, config) =>
                {
                    executou = true;
                    return Retorno();
                };

                try
                {
                    ConfiguracaoApp.ChecarConexaoInternet = false;
                    ExecutarEmThread("0", () => new Processar().ProcessaArquivo(0, contexto.ArquivoEnvio));
                }
                finally
                {
                    ConfiguracaoApp.ChecarConexaoInternet = checarConexaoAnterior;
                }

                var erro = Path.Combine(contexto.PastaRetorno, "consulta-sit.err");
                Assert.True(executou, File.Exists(erro) ? File.ReadAllText(erro) :
                    "Roteamento não executou a consulta e não gerou .ERR; pedido existe: " + File.Exists(contexto.ArquivoEnvio));
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "consulta-sit.xml")));
            }
        }

        [Theory]
        [InlineData("STATUS", Chave, 2)]
        [InlineData("CONSULTAR", "123", 2)]
        [InlineData("CONSULTAR", "41260912345678000123550010000000011012345670", 2)]
        [InlineData("CONSULTAR", Chave, 1)]
        public void PedidoInvalidoNaoIniciaTransporte(string servico, string chave, int ambiente)
        {
            using(var contexto = new Contexto(Pedido(servico, chave, ambiente)))
            {
                var executou = false;
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = (xml, config) =>
                {
                    executou = true;
                    return Retorno();
                };

                ExecutarEmThread("0", () => new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio).Execute());

                Assert.False(executou);
                Assert.True(File.Exists(contexto.ArquivoEnvio));
                Assert.Contains("Message|Falha de Configuracao", File.ReadAllText(
                    Path.Combine(contexto.PastaRetorno, "consulta-sit.err")));
            }
        }

        [Fact]
        public void ProducaoSemEndpointNaoIniciaTransporte()
        {
            using(var contexto = new Contexto(Pedido(ambiente: 1), 1))
            {
                var executou = false;
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = (xml, config) =>
                {
                    executou = true;
                    return Retorno();
                };

                ExecutarEmThread("0", () => new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio).Execute());

                Assert.False(executou);
                Assert.Contains("Message|Falha de Configuracao", File.ReadAllText(
                    Path.Combine(contexto.PastaRetorno, "consulta-sit.err")));
            }
        }

        [Fact]
        public void FalhaDeTransporteGeraErroPadraoSemVazarSegredo()
        {
            using(var contexto = new Contexto(Pedido()))
            {
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = (xml, config) =>
                {
                    throw new Exception("Falha TLS senha=NAO_VAZAR");
                };

                ExecutarEmThread("0", () => new TaskConsultaSituacaoNFeABI(contexto.ArquivoEnvio).Execute());

                var erro = File.ReadAllText(Path.Combine(contexto.PastaRetorno, "consulta-sit.err"));
                Assert.Contains("Versão UniNFe|", erro);
                Assert.Contains("ErrorCode|", erro);
                Assert.Contains("Message|Falha de TLS na consulta de protocolo da NF-e ABI.", erro);
                Assert.DoesNotContain("NAO_VAZAR", erro);
                Assert.True(File.Exists(contexto.ArquivoEnvio));
            }
        }

        private static string Pedido(string servico = "CONSULTAR", string chave = Chave, int ambiente = 2)
        {
            return "<consSitNFeABI xmlns=\"http://www.portalfiscal.inf.br/nfeabi\" versao=\"1.00\"><tpAmb>" + ambiente +
                "</tpAmb><xServ>" + servico + "</xServ><chNFeABI>" + chave + "</chNFeABI></consSitNFeABI>";
        }

        private static string Retorno()
        {
            return "<retConsSitNFeABI versao=\"1.00\" xmlns=\"http://www.portalfiscal.inf.br/nfeabi\"><tpAmb>2</tpAmb><verAplic>TESTE</verAplic><cStat>217</cStat><xMotivo>NFeABI nao consta na base de dados</xMotivo><cUF>41</cUF><dhRecbto>2026-10-13T09:00:00-03:00</dhRecbto><chNFeABI>" + Chave + "</chNFeABI></retConsSitNFeABI>";
        }

        public void Dispose()
        {
            TaskConsultaSituacaoNFeABI.ExecutarConsulta = null;
        }

        private sealed class Contexto : TaskTestContextBase
        {
            internal Contexto(string xml, int ambiente = 2) : base("consulta-ped-sit.xml", xml)
            {
                var pastaErro = Path.Combine(PastaTemporaria, "Erro");
                var pastaValidar = Path.Combine(PastaTemporaria, "Validar");
                Directory.CreateDirectory(pastaErro);
                Directory.CreateDirectory(pastaValidar);
                DefinirConfiguracoesEmpresas(new List<Empresa>
                {
                    new Empresa
                    {
                        PastaXmlEnvio = PastaTemporaria,
                        PastaXmlEnviado = PastaTemporaria,
                        PastaXmlRetorno = PastaRetorno,
                        PastaXmlErro = pastaErro,
                        PastaValidar = pastaValidar,
                        AmbienteCodigo = ambiente,
                        Servico = TipoAplicativo.NFeABI,
                        UsaCertificado = false
                    }
                });
            }

        }
    }
}
