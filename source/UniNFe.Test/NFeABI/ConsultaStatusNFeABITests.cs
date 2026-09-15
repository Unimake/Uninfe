using NFe.Components;
using NFe.Service.NFeABI;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UniNFe.Test.Abstractions;
using Unimake.Business.DFe.Servicos;
using Xunit;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class ConsultaStatusNFeABITests : TaskTestFixtureBase, IDisposable
    {
        private readonly List<Empresa> configuracoesAnteriores;
        private readonly bool proxyAnterior;
        private readonly bool proxyAutoAnterior;
        private readonly bool checarConexaoAnterior;
        private readonly string proxyUsuarioAnterior;
        private readonly string proxySenhaAnterior;
        private readonly string pasta;
        private readonly string pastaErro;
        private X509Certificate2 certificadoTemporario;

        public ConsultaStatusNFeABITests()
        {
            configuracoesAnteriores = Empresas.Configuracoes;
            proxyAnterior = ConfiguracaoApp.Proxy;
            proxyAutoAnterior = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
            checarConexaoAnterior = ConfiguracaoApp.ChecarConexaoInternet;
            proxyUsuarioAnterior = ConfiguracaoApp.ProxyUsuario;
            proxySenhaAnterior = ConfiguracaoApp.ProxySenha;
            pasta = Path.Combine(Path.GetTempPath(), "UniNFe.Test.NFeABI", Guid.NewGuid().ToString("N"));
            pastaErro = Path.Combine(pasta, "Erro");
            Directory.CreateDirectory(pasta);
            Directory.CreateDirectory(pastaErro);
            Empresas.Configuracoes = new List<Empresa>
            {
                new Empresa
                {
                    PastaXmlEnvio = pasta,
                    PastaXmlRetorno = pasta,
                    PastaXmlErro = pastaErro,
                    PastaValidar = Path.Combine(pasta, "Validar"),
                    AmbienteCodigo = 2,
                    Servico = TipoAplicativo.NFeABI,
                    UsaCertificado = false,
                    AtivarPreparacaoTLSAntesEnvioXML = true
                }
            };
        }

        [Theory]
        [InlineData(107)]
        [InlineData(108)]
        [InlineData(109)]
        public void StatusPreservaXmlBrutoConfiguraTransporteEGeraDiagnosticoPassivo(int cStat)
        {
            ConfiguracaoApp.Proxy = true;
            ConfiguracaoApp.DetectarConfiguracaoProxyAuto = true;
            ConfiguracaoApp.ProxyUsuario = "usuario-proxy";
            ConfiguracaoApp.ProxySenha = "segredo-proxy";
            var arquivo = CriarPedido();
            var retorno = "<retConsStatServNFeABI xmlns=\"http://www.portalfiscal.inf.br/nfeabi\" versao=\"1.00\"><tpAmb>2</tpAmb><verAplic>TESTE</verAplic><cStat>" + cStat + "</cStat><xMotivo>OK</xMotivo><cUF>41</cUF><dhRecbto>2026-09-15T00:00:00-03:00</dhRecbto></retConsStatServNFeABI>";
            Configuracao capturada = null;
            string xmlRecebido = null;
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) =>
            {
                xmlRecebido = xml;
                capturada = configuracao;
                return retorno;
            };

            ExecutarEmThread("0", () => new TaskConsultaStatusNFeABI(arquivo).Execute());

            Assert.False(File.Exists(arquivo));
            Assert.Equal(retorno, File.ReadAllText(Path.Combine(pasta, "status-sta.xml")));
            Assert.Contains("<consStatServNFeABI", xmlRecebido);
            Assert.Equal(TipoDFe.NFeABI, capturada.TipoDFe);
            Assert.Equal(41, capturada.CodigoUF);
            Assert.Equal(TipoAmbiente.Homologacao, capturada.TipoAmbiente);
            Assert.True(capturada.PrepararConexaoTLSAntesDoEnvio);
            Assert.True(capturada.HasProxy);
            Assert.True(capturada.ProxyAutoDetect);
            Assert.Equal("usuario-proxy", capturada.ProxyUser);
            Assert.Equal("segredo-proxy", capturada.ProxyPassword);

            var diagnostico = File.ReadAllText(Path.Combine(pasta, "status-diagdispdfe.xml"));
            Assert.Contains("<ResultadoOperacao>RetornoFiscalRecebido</ResultadoOperacao>", diagnostico);
            Assert.Contains("<CategoriaFalha>Nenhuma</CategoriaFalha>", diagnostico);
            Assert.Contains("<CStat>" + cStat + "</CStat>", diagnostico);
            Assert.DoesNotContain("segredo-proxy", diagnostico);
            Assert.DoesNotContain("consStatServNFeABI", diagnostico);
        }

        [Theory]
        [InlineData("DNS", "Falha DNS no host; segredo=<xml>NAO_VAZAR</xml>")]
        [InlineData("TLS", "Falha TLS; senha=NAO_VAZAR")]
        [InlineData("Proxy", "Falha no proxy; token=NAO_VAZAR")]
        public void FalhaDeTransporteGeraErroClassificadoSemSegredoEPreservaPedido(string categoria, string mensagem)
        {
            var arquivo = CriarPedido();
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) => { throw new Exception(mensagem); };

            ExecutarEmThread("0", () => new TaskConsultaStatusNFeABI(arquivo).Execute());

            Assert.True(File.Exists(arquivo) || File.Exists(Path.Combine(pastaErro, "status-ped-sta.xml")));
            var erro = File.ReadAllText(Path.Combine(pasta, "status-sta.err"));
            Assert.Contains("Falha de " + categoria, erro);
            Assert.DoesNotContain("NAO_VAZAR", erro);
            var diagnostico = File.ReadAllText(Path.Combine(pasta, "status-diagdispdfe.xml"));
            Assert.Contains("<CategoriaFalha>" + categoria + "</CategoriaFalha>", diagnostico);
            Assert.DoesNotContain("NAO_VAZAR", diagnostico);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RetornoVazioGeraErroSemXmlVazioEPreservaPedido(string retorno)
        {
            var arquivo = CriarPedido();
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) => retorno;

            ExecutarEmThread("0", () => new TaskConsultaStatusNFeABI(arquivo).Execute());

            Assert.False(File.Exists(Path.Combine(pasta, "status-sta.xml")));
            Assert.True(File.Exists(arquivo) || File.Exists(Path.Combine(pastaErro, "status-ped-sta.xml")));
            Assert.Contains("Falha de Retorno", File.ReadAllText(Path.Combine(pasta, "status-sta.err")));
        }

        [Fact]
        public void FalhaAoArquivarPreservaPedidoOriginalEGravaErroFallback()
        {
            var arquivo = CriarPedido();
            Directory.Delete(pastaErro);
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) => { throw new Exception("Falha DNS"); };

            ExecutarEmThread("0", () => new TaskConsultaStatusNFeABI(arquivo).Execute());

            Assert.True(File.Exists(arquivo));
            Assert.Contains("Falha de DNS", File.ReadAllText(Path.Combine(pasta, "status-sta.err")));
        }

        [Fact]
        public void CertificadoAusenteNaoExecutaTransporte()
        {
            var arquivo = CriarPedido();
            Empresas.Configuracoes[0].UsaCertificado = true;
            var executou = false;
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) => { executou = true; return string.Empty; };

            ExecutarEmThread("0", () => new TaskConsultaStatusNFeABI(arquivo).Execute());

            Assert.False(executou);
            Assert.Contains("Falha de Certificado", File.ReadAllText(Path.Combine(pasta, "status-sta.err")));
        }

        [Fact]
        public void RoteamentoComCertificadoAusenteDelegaErroParaTask()
        {
            var arquivo = CriarPedido();
            Empresas.Configuracoes[0].UsaCertificado = true;
            ConfiguracaoApp.ChecarConexaoInternet = false;
            var executou = false;
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) => { executou = true; return string.Empty; };

            ExecutarEmThread("0", () => new NFe.Service.Processar().ProcessaArquivo(0, arquivo));

            Assert.False(executou);
            Assert.True(File.Exists(Path.Combine(pasta, "status-sta.err")));
            Assert.Contains("Falha de Certificado", File.ReadAllText(Path.Combine(pasta, "status-sta.err")));
        }

        [Fact]
        public void CertificadoVencidoNaoExecutaTransporte()
        {
            var arquivo = CriarPedido();
            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest("CN=UniNFe Teste", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                certificadoTemporario = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddDays(-2));
            }
            Empresas.Configuracoes[0].UsaCertificado = true;
            Empresas.Configuracoes[0].X509Certificado = certificadoTemporario;
            var executou = false;
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) => { executou = true; return string.Empty; };

            ExecutarEmThread("0", () => new TaskConsultaStatusNFeABI(arquivo).Execute());

            Assert.False(executou);
            Assert.Contains("Falha de Certificado", File.ReadAllText(Path.Combine(pasta, "status-sta.err")));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        public void ProducaoOuAmbienteDivergenteNaoExecutamTransporte(int ambienteEmpresa, int ambientePedido)
        {
            Empresas.Configuracoes[0].AmbienteCodigo = ambienteEmpresa;
            var arquivo = CriarPedido(ambientePedido);
            var executou = false;
            TaskConsultaStatusNFeABI.ExecutarConsulta = (xml, configuracao) => { executou = true; return string.Empty; };

            ExecutarEmThread("0", () => new TaskConsultaStatusNFeABI(arquivo).Execute());

            Assert.False(executou);
            Assert.Contains("Falha de Configuracao", File.ReadAllText(Path.Combine(pasta, "status-sta.err")));
        }

        public void Dispose()
        {
            TaskConsultaStatusNFeABI.ExecutarConsulta = null;
            ConfiguracaoApp.Proxy = proxyAnterior;
            ConfiguracaoApp.DetectarConfiguracaoProxyAuto = proxyAutoAnterior;
            ConfiguracaoApp.ChecarConexaoInternet = checarConexaoAnterior;
            ConfiguracaoApp.ProxyUsuario = proxyUsuarioAnterior;
            ConfiguracaoApp.ProxySenha = proxySenhaAnterior;
            Empresas.Configuracoes = configuracoesAnteriores;
            certificadoTemporario?.Dispose();
            if (Directory.Exists(pasta))
            {
                Directory.Delete(pasta, true);
            }
        }

        private string CriarPedido(int ambiente = 2)
        {
            var arquivo = Path.Combine(pasta, "status-ped-sta.xml");
            File.WriteAllText(arquivo, "<consStatServNFeABI xmlns=\"http://www.portalfiscal.inf.br/nfeabi\" versao=\"1.00\"><tpAmb>" + ambiente + "</tpAmb><cUF>41</cUF><xServ>STATUS</xServ></consStatServNFeABI>");
            return arquivo;
        }
    }
}
