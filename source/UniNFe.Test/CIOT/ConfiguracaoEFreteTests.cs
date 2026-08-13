using NFe.Components;
using NFe.Service.CIOT;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace UniNFe.Test.CIOT
{
    public class ConfiguracaoEFreteTests
    {
        [Fact]
        public void PersisteCamposCriptografadosEFazRoundTrip()
        {
            var arquivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                var empresa = CriarEmpresaEFrete();
                empresa.CriptografarConfiguracaoEFrete();
                new ObjectXMLSerializer().Save(empresa, arquivo);

                var conteudo = File.ReadAllText(arquivo);
                Assert.Contains("<EFreteIntegrador>", conteudo);
                Assert.Contains("<EFreteToken>", conteudo);
                Assert.Contains("<EFreteUsuario>", conteudo);
                Assert.Contains("<EFreteSenha>", conteudo);
                Assert.DoesNotContain("INTEGRADOR-SINTETICO", conteudo);
                Assert.DoesNotContain("TOKEN-SINTETICO", conteudo);
                Assert.DoesNotContain("USUARIO-SINTETICO", conteudo);
                Assert.DoesNotContain("SENHA-SINTETICA", conteudo);

                var carregada = (Empresa)new ObjectXMLSerializer().Load(typeof(Empresa), arquivo);
                carregada.DescriptografarConfiguracaoEFrete();

                Assert.Equal("INTEGRADOR-SINTETICO", carregada.EFreteIntegrador);
                Assert.Equal("TOKEN-SINTETICO", carregada.EFreteToken);
                Assert.Equal("USUARIO-SINTETICO", carregada.EFreteUsuario);
                Assert.Equal("SENHA-SINTETICA", carregada.EFreteSenha);
            }
            finally
            {
                if (File.Exists(arquivo))
                {
                    File.Delete(arquivo);
                }
            }
        }

        [Fact]
        public void ConfiguracaoLegadaSemCamposEFreteAssumeValoresVazios()
        {
            var arquivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                File.WriteAllText(arquivo, "<?xml version=\"1.0\" encoding=\"utf-8\"?><Empresa><Servico>CIOT</Servico></Empresa>");
                var empresa = (Empresa)new ObjectXMLSerializer().Load(typeof(Empresa), arquivo);
                empresa.DescriptografarConfiguracaoEFrete();

                Assert.Equal(string.Empty, empresa.EFreteIntegrador);
                Assert.Equal(string.Empty, empresa.EFreteToken);
                Assert.Equal(string.Empty, empresa.EFreteUsuario);
                Assert.Equal(string.Empty, empresa.EFreteSenha);
            }
            finally
            {
                if (File.Exists(arquivo))
                {
                    File.Delete(arquivo);
                }
            }
        }

        [Fact]
        public void CamposVaziosPermanecemVaziosAposCriptografiaEDescriptografia()
        {
            var empresa = new Empresa { Servico = TipoAplicativo.CIOT };

            empresa.CriptografarConfiguracaoEFrete();
            empresa.DescriptografarConfiguracaoEFrete();
            empresa.ValidarConfiguracaoEFrete();

            Assert.Equal(string.Empty, empresa.EFreteIntegrador);
            Assert.Equal(string.Empty, empresa.EFreteToken);
            Assert.Equal(string.Empty, empresa.EFreteUsuario);
            Assert.Equal(string.Empty, empresa.EFreteSenha);
        }

        [Fact]
        public void AceitaFormasValidasDeAutenticacao()
        {
            new Empresa { Servico = TipoAplicativo.CIOT }.ValidarConfiguracaoEFrete();
            new Empresa { Servico = TipoAplicativo.CIOT, EFreteIntegrador = "INTEGRADOR" }.ValidarConfiguracaoEFrete();
            new Empresa { Servico = TipoAplicativo.CIOT, EFreteIntegrador = "INTEGRADOR", EFreteToken = "TOKEN" }.ValidarConfiguracaoEFrete();
            new Empresa { Servico = TipoAplicativo.CIOT, EFreteIntegrador = "INTEGRADOR", EFreteUsuario = "USUARIO", EFreteSenha = "SENHA" }.ValidarConfiguracaoEFrete();
            new Empresa { Servico = TipoAplicativo.Todos, EFreteIntegrador = "INTEGRADOR", EFreteToken = "TOKEN", EFreteUsuario = "USUARIO", EFreteSenha = "SENHA" }.ValidarConfiguracaoEFrete();
        }

        [Theory]
        [InlineData("", "TOKEN", "", "")]
        [InlineData("INTEGRADOR", "", "USUARIO", "")]
        [InlineData("INTEGRADOR", "", "", "SENHA")]
        [InlineData("", "", "USUARIO", "SENHA")]
        public void RejeitaFormasIncompletasDeAutenticacao(string integrador, string token, string usuario, string senha)
        {
            var empresa = new Empresa
            {
                Servico = TipoAplicativo.CIOT,
                EFreteIntegrador = integrador,
                EFreteToken = token,
                EFreteUsuario = usuario,
                EFreteSenha = senha
            };

            Assert.Throws<Exception>(() => empresa.ValidarConfiguracaoEFrete());
        }

        [Fact]
        public void TaskCIOTCopiaConfiguracaoDaEmpresaParaDLL()
        {
            var arquivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");
            var configuracoesOriginais = Empresas.Configuracoes;
            using (var certificado = new X509Certificate2())
            {
                try
                {
                    File.WriteAllText(arquivo, "<ConsultarCIOTGerado xmlns=\"http://www.antt.gov.br/ciot\" />");
                    var empresa = CriarEmpresaEFrete();
                    empresa.AmbienteCodigo = 2;
                    empresa.X509Certificado = certificado;
                    Empresas.Configuracoes = new List<Empresa> { empresa };

                    var task = new TaskCIOTTeste(arquivo);
                    var configuracao = task.ObterConfiguracao(0);

                    Assert.Equal("INTEGRADOR-SINTETICO", configuracao.EFreteIntegrador);
                    Assert.Equal("TOKEN-SINTETICO", configuracao.EFreteToken);
                    Assert.Equal("USUARIO-SINTETICO", configuracao.EFreteUsuario);
                    Assert.Equal("SENHA-SINTETICA", configuracao.EFreteSenha);
                    Assert.Same(certificado, configuracao.CertificadoDigital);
                    Assert.Equal(Unimake.Business.DFe.Servicos.TipoAmbiente.Homologacao, configuracao.TipoAmbiente);
                }
                finally
                {
                    Empresas.Configuracoes = configuracoesOriginais;
                    if (File.Exists(arquivo))
                    {
                        File.Delete(arquivo);
                    }
                }
            }
        }

        private static Empresa CriarEmpresaEFrete()
        {
            return new Empresa
            {
                Servico = TipoAplicativo.CIOT,
                EFreteIntegrador = "INTEGRADOR-SINTETICO",
                EFreteToken = "TOKEN-SINTETICO",
                EFreteUsuario = "USUARIO-SINTETICO",
                EFreteSenha = "SENHA-SINTETICA"
            };
        }

        private sealed class TaskCIOTTeste : TaskCIOTBase
        {
            internal TaskCIOTTeste(string arquivo) : base(arquivo)
            {
            }

            protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTConsultar;

            internal Unimake.Business.DFe.Servicos.Configuracao ObterConfiguracao(int empresa)
            {
                return CriarConfiguracao(empresa);
            }

            public override void Execute()
            {
            }
        }
    }
}
