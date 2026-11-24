using NFe.Components;
using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using UniNFe.Test.Abstractions;
using Unimake.Business.DFe.Servicos;
using Xunit;

namespace UniNFe.Test.Autorizacao
{
    [CollectionDefinition("NFe Eventos Serial", DisableParallelization = true)]
    public class NFeEventosCollection : ICollectionFixture<NFeEventosProxyTestFixture>
    {
    }

    [Collection("NFe Eventos Serial")]
    public class NFeEventosProxyTests
    {
        private readonly NFeEventosProxyTestFixture fixture;

        public NFeEventosProxyTests(NFeEventosProxyTestFixture fixture) => this.fixture = fixture;

        [Theory]
        [InlineData("110111")]
        [InlineData("110110")]
        public void EventoNFeRepassaProxyManualAoTransporte(string tipoEvento)
        {
            using(var contexto = new NFeEventosProxyTestContext(tipoEvento))
            {
                ConfiguracaoApp.Proxy = true;
                ConfiguracaoApp.ProxyServidor = "proxy.example.com";
                ConfiguracaoApp.ProxyPorta = 3128;
                ConfiguracaoApp.ProxyUsuario = "usuario-teste";
                ConfiguracaoApp.ProxySenha = "senha-teste";

                var configuracao = fixture.Executar(contexto);

                Assert.True(configuracao.HasProxy);
                Assert.Equal("proxy.example.com", configuracao.ProxyServer);
                Assert.Equal(3128, configuracao.ProxyPort);
                Assert.Equal("usuario-teste", configuracao.ProxyUser);
                Assert.Equal("senha-teste", configuracao.ProxyPassword);
            }
        }

        [Theory]
        [InlineData("110111")]
        [InlineData("110110")]
        public void EventoNFeNaoHabilitaProxyQuandoDesativado(string tipoEvento)
        {
            using(var contexto = new NFeEventosProxyTestContext(tipoEvento))
            {
                var configuracao = fixture.Executar(contexto);

                Assert.False(configuracao.HasProxy);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void RecusaEpecNFCeSpSemCriarServico(bool misto, bool cancelamentoPrimeiro)
        {
            var epec = Evento("110140", "65", "35", "35");
            var cancelamento = Evento("110111", "65", "35", "35");
            var eventos = !misto ? epec : cancelamentoPrimeiro ? cancelamento + epec : epec + cancelamento;
            using (var contexto = new NFeEventosProxyTestContext(Lote(eventos), 35))
            {
                Assert.Null(fixture.Executar(contexto));
                var erro = File.ReadAllText(Path.Combine(contexto.PastaRetorno, "evento-eve.err"));
                Assert.Contains("Utilize a contingência offline da NFC-e", erro);
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, "evento-eve.xml")));
            }
        }

        [Theory]
        [InlineData("110140", "55", "91", "41", TipoDFe.NFe)]
        [InlineData("110111", "65", "35", "35", TipoDFe.NFCe)]
        public void MantemRotaDosEventosSuportados(string tipo, string modelo, string orgao, string uf, TipoDFe tipoDFe)
        {
            using (var contexto = new NFeEventosProxyTestContext(Lote(Evento(tipo, modelo, orgao, uf)), int.Parse(uf)))
            {
                contexto.Empresa.tpEmis = tipo == "110140" ? 4 : 1;
                var configuracao = fixture.Executar(contexto);
                Assert.NotNull(configuracao);
                Assert.Equal(tipoDFe, configuracao.TipoDFe);
            }
        }

        private static string Lote(string eventos) =>
            "<envEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\"><idLote>1</idLote>" + eventos + "</envEvento>";

        private static string Evento(string tipo, string modelo, string orgao, string uf)
        {
            var emissao = tipo == "110140" ? "4" : "1";
            var chave = uf + "261112345678000123" + modelo + "001000000001" + emissao + "000000010";
            var detalhe = tipo == "110140"
                ? $"<descEvento>EPEC</descEvento><cOrgaoAutor>{uf}</cOrgaoAutor><tpAutor>1</tpAutor><verAplic>TESTE</verAplic>" +
                  "<dhEmi>2026-11-24T12:00:00-03:00</dhEmi><tpNF>1</tpNF><IE>0000000000</IE>" +
                  "<dest><UF>SP</UF><CPF>12345678909</CPF><vNF>10.00</vNF><vICMS>0.00</vICMS><vST>0.00</vST></dest>"
                : "<descEvento>Cancelamento</descEvento><nProt>135260000000001</nProt><xJust>Cancelamento sintetico de teste offline</xJust>";
            return $"<evento versao=\"1.00\"><infEvento Id=\"ID{tipo}{chave}01\"><cOrgao>{orgao}</cOrgao><tpAmb>2</tpAmb>" +
                $"<CNPJ>12345678000123</CNPJ><chNFe>{chave}</chNFe><dhEvento>2026-11-24T12:00:01-03:00</dhEvento>" +
                $"<tpEvento>{tipo}</tpEvento><nSeqEvento>1</nSeqEvento><verEvento>1.00</verEvento>" +
                $"<detEvento versao=\"1.00\">{detalhe}</detEvento></infEvento></evento>";
        }
    }

    public class NFeEventosProxyTestFixture : TaskTestFixtureBase
    {
        internal Configuracao Executar(NFeEventosProxyTestContext contexto)
        {
            Configuracao configuracaoCapturada = null;
            ExecutarEmThread("0", () =>
            {
                var task = new TaskNFeEventos(contexto.ArquivoEnvio);
                task.CriarRecepcaoEvento = (xml, configuracao) =>
                    new RecepcaoEventoNFeFake(() => configuracaoCapturada = configuracao);
                task.Execute();
            });
            return configuracaoCapturada;
        }
    }

    public class NFeEventosProxyTestContext : TaskTestContextBase, IDisposable
    {
        private readonly bool proxyAnterior = ConfiguracaoApp.Proxy;
        private readonly bool proxyAutoAnterior = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
        private readonly string servidorAnterior = ConfiguracaoApp.ProxyServidor;
        private readonly int portaAnterior = ConfiguracaoApp.ProxyPorta;
        private readonly string usuarioAnterior = ConfiguracaoApp.ProxyUsuario;
        private readonly string senhaAnterior = ConfiguracaoApp.ProxySenha;

        internal NFeEventosProxyTestContext(string tipoEvento) : this(PedidoEvento(tipoEvento), 41) { }

        internal NFeEventosProxyTestContext(string pedido, int uf) : base("evento-ped-eve.xml", pedido)
        {
            Empresa = new Empresa
            {
                CNPJ = "12345678000123",
                UnidadeFederativaCodigo = uf,
                PastaXmlEnvio = PastaTemporaria,
                PastaXmlEnviado = Path.Combine(PastaTemporaria, "Enviados"),
                PastaXmlRetorno = PastaRetorno,
                PastaXmlErro = Path.Combine(PastaTemporaria, "Erro"),
                PastaXmlEmLote = Path.Combine(PastaTemporaria, "Lote"),
                PastaValidar = Path.Combine(PastaTemporaria, "Validar"),
                PastaBackup = string.Empty,
                AmbienteCodigo = 2,
                Servico = TipoAplicativo.Nfe,
                UsaCertificado = false
            };
            Directory.CreateDirectory(Empresa.PastaXmlErro);
            Directory.CreateDirectory(Empresa.PastaXmlEmLote);
            Directory.CreateDirectory(Empresa.PastaValidar);
            Empresa.CriarSubPastaEnviado();
            DefinirConfiguracoesEmpresas(new List<Empresa> { Empresa });
            ConfiguracaoApp.Proxy = false;
            ConfiguracaoApp.DetectarConfiguracaoProxyAuto = false;
        }

        internal Empresa Empresa { get; }

        public new void Dispose()
        {
            ConfiguracaoApp.Proxy = proxyAnterior;
            ConfiguracaoApp.DetectarConfiguracaoProxyAuto = proxyAutoAnterior;
            ConfiguracaoApp.ProxyServidor = servidorAnterior;
            ConfiguracaoApp.ProxyPorta = portaAnterior;
            ConfiguracaoApp.ProxyUsuario = usuarioAnterior;
            ConfiguracaoApp.ProxySenha = senhaAnterior;
            base.Dispose();
        }

        private static string PedidoEvento(string tipoEvento)
        {
            const string chave = "41261112345678000123550010000000011000000010";
            var detalhe = tipoEvento == "110111"
                ? "<descEvento>Cancelamento</descEvento><nProt>141260000000001</nProt><xJust>Justificativa de teste</xJust>"
                : "<descEvento>Carta de Correcao</descEvento><xCorrecao>Correcao de teste para a nota fiscal.</xCorrecao>";
            return "<envEvento versao=\"1.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\"><idLote>000000000000001</idLote>" +
                "<evento versao=\"1.00\"><infEvento Id=\"ID" + tipoEvento + chave + "01\"><cOrgao>41</cOrgao><tpAmb>2</tpAmb>" +
                "<CNPJ>12345678000123</CNPJ><chNFe>" + chave + "</chNFe><dhEvento>2026-11-24T10:00:00-03:00</dhEvento>" +
                "<tpEvento>" + tipoEvento + "</tpEvento><nSeqEvento>1</nSeqEvento><verEvento>1.00</verEvento><detEvento versao=\"1.00\">" +
                detalhe + "</detEvento></infEvento></evento></envEvento>";
        }
    }

    internal sealed class RecepcaoEventoNFeFake : Unimake.Business.DFe.Servicos.NFe.RecepcaoEvento
    {
        private readonly Action aoExecutar;

        internal RecepcaoEventoNFeFake(Action aoExecutar)
        {
            this.aoExecutar = aoExecutar;
        }

        public override void Executar()
        {
            aoExecutar();
            throw new InvalidOperationException("Transporte de evento substituído para teste.");
        }
    }
}
