using NFe.Components;
using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace UniNFe.Test.Certificados
{
    [Collection("Certificados Serial")]
    public class ProcessamentoPinTests : IDisposable
    {
        private readonly List<Empresa> configuracoesAnteriores;
        private readonly MethodInfo deveCarregarPin;

        public ProcessamentoPinTests()
        {
            configuracoesAnteriores = Empresas.Configuracoes;
            Empresas.Configuracoes = new List<Empresa>
            {
                new Empresa { UsaCertificado = true, CertificadoPIN = "1234" }
            };
            deveCarregarPin = typeof(Processar).GetMethod("DeveCarregarPin", BindingFlags.NonPublic | BindingFlags.Static);
        }

        [Fact]
        public void EmpresaMenosUmNaoIndexaConfiguracoes()
        {
            var resultado = Executar(-1, @"C:\Geral\Temp\arquivo.xml", Servicos.NFeEnviarLote);

            Assert.False(resultado);
        }

        [Fact]
        public void PastaGeralIgnoraDiferencasDeCaixaESeparadorFinal()
        {
            var arquivo = Propriedade.PastaGeralTemporaria
                .TrimEnd('\\', '/')
                .ToUpperInvariant() + "\\arquivo.xml";

            var resultado = Executar(0, arquivo, Servicos.NFeEnviarLote);

            Assert.False(resultado);
        }

        [Theory]
        [InlineData(Servicos.UniNFeAlterarConfiguracoes)]
        [InlineData(Servicos.UniNFeConsultaInformacoes)]
        [InlineData(Servicos.NFeConverterTXTparaXML)]
        public void ArquivoAdministrativoNaoTentaCarregarPin(Servicos servico)
        {
            var resultado = Executar(0, @"C:\Empresa\Envio\arquivo.xml", servico);

            Assert.False(resultado);
        }

        public void Dispose()
        {
            Empresas.Configuracoes = configuracoesAnteriores;
        }

        private bool Executar(int empresa, string arquivo, Servicos servico)
        {
            Assert.NotNull(deveCarregarPin);
            return (bool)deveCarregarPin.Invoke(null, new object[] { empresa, arquivo, servico });
        }
    }
}
