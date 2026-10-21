using NFe.Components;
using NFe.Service.NFeABI;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;
using Xunit;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class ConsultaStatusNFeABITests : NFeABITestFixture
    {
        [Theory]
        [InlineData(107)]
        [InlineData(108)]
        [InlineData(109)]
        public void StatusUsaConfiguracaoDaDllEPreservaRetornoBruto(int status)
        {
            using(var contexto = new Contexto("status-ped-sta.xml", PedidoStatus()))
            {
                var retorno = Retorno("retConsStatServNFeABI", status);
                var executou = false;
                TaskConsultaStatusNFeABI.ExecutarConsulta = consulta =>
                {
                    executou = true;
                    Assert.Equal(TipoDFe.NFeABI, consulta.Configuracoes.TipoDFe);
                    Assert.Equal(41, consulta.Configuracoes.CodigoUF);
                    Assert.Equal(TipoAmbiente.Homologacao, consulta.Configuracoes.TipoAmbiente);
                    Assert.True(consulta.Configuracoes.PrepararConexaoTLSAntesDoEnvio);
                    Assert.True(consulta.Configuracoes.ColetarTelemetriaDisponibilidade);
                    consulta.RetornoWSString = retorno;
                };

                Executar(new TaskConsultaStatusNFeABI(contexto.ArquivoEnvio));

                Assert.True(executou);
                Assert.Equal(retorno, File.ReadAllText(Path.Combine(contexto.PastaRetorno, "status-sta.xml")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, "status-sta.err")));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StatusRespeitaConfiguracaoDeProxy(bool usarProxy)
        {
            using(var contexto = new Contexto("status-ped-sta.xml", PedidoStatus()))
            {
                ConfiguracaoApp.Proxy = usarProxy;
                ConfiguracaoApp.ProxyServidor = "proxy.example.com";
                ConfiguracaoApp.ProxyPorta = 3128;
                ConfiguracaoApp.ProxyUsuario = "usuario-teste";
                ConfiguracaoApp.ProxySenha = "senha-teste";
                Configuracao capturada = null;
                TaskConsultaStatusNFeABI.ExecutarConsulta = consulta =>
                {
                    capturada = consulta.Configuracoes;
                    consulta.RetornoWSString = Retorno("retConsStatServNFeABI", 107);
                };

                Executar(new TaskConsultaStatusNFeABI(contexto.ArquivoEnvio));

                Assert.NotNull(capturada);
                Assert.Equal(usarProxy, capturada.HasProxy);
                if(usarProxy)
                {
                    Assert.Equal("proxy.example.com", capturada.ProxyServer);
                    Assert.Equal(3128, capturada.ProxyPort);
                    Assert.Equal("usuario-teste", capturada.ProxyUser);
                    Assert.Equal("senha-teste", capturada.ProxyPassword);
                }
            }
        }

        [Fact]
        public void ErroOriginalDaDllEhGravadoEPedidoEhRemovido()
        {
            using(var contexto = new Contexto("status-ped-sta.xml", PedidoStatus()))
            {
                TaskConsultaStatusNFeABI.ExecutarConsulta = consulta => { throw new InvalidOperationException("Falha TLS de teste da DLL."); };
                Executar(new TaskConsultaStatusNFeABI(contexto.ArquivoEnvio));

                var erro = File.ReadAllText(Path.Combine(contexto.PastaRetorno, "status-sta.err"));
                Assert.Contains("Message|Falha TLS de teste da DLL.", erro);
                Assert.Contains("Type|System.InvalidOperationException", erro);
                Assert.False(File.Exists(contexto.ArquivoEnvio));
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, "status-ped-sta.xml")));
            }
        }

        [Fact]
        public void AmbienteDoXmlEhResolvidoPelaDllSemComparacaoLocal()
        {
            using(var contexto = new Contexto("status-ped-sta.xml", PedidoStatus()))
            {
                contexto.Empresa.AmbienteCodigo = 1;
                var executou = false;
                TaskConsultaStatusNFeABI.ExecutarConsulta = consulta =>
                {
                    executou = true;
                    Assert.Equal(TipoAmbiente.Homologacao, consulta.Configuracoes.TipoAmbiente);
                    consulta.RetornoWSString = Retorno("retConsStatServNFeABI", 107);
                };
                Executar(new TaskConsultaStatusNFeABI(contexto.ArquivoEnvio));
                Assert.True(executou);
            }
        }

        [Fact]
        public void ProducaoSemEndpointEhRejeitadaPelaDll()
        {
            using(var contexto = new Contexto("status-ped-sta.xml", PedidoStatus(1)))
            {
                var executou = false;
                TaskConsultaStatusNFeABI.ExecutarConsulta = consulta => { executou = true; };
                Executar(new TaskConsultaStatusNFeABI(contexto.ArquivoEnvio));
                Assert.False(executou);
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "status-sta.err")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
            }
        }

        [Fact]
        public void RoteamentoRealExecutaStatusSemRede()
        {
            using(var contexto = new Contexto("status-ped-sta.xml", PedidoStatus()))
            {
                TaskConsultaStatusNFeABI.ExecutarConsulta = consulta => consulta.RetornoWSString = Retorno("retConsStatServNFeABI", 107);
                Processar(contexto);
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "status-sta.xml")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
            }
        }
    }
}
