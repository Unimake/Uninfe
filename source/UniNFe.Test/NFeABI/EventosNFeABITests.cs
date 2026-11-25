using NFe.Service.NFeABI;
using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFeABI;
using Xunit;
using RecepcaoEvento = Unimake.Business.DFe.Servicos.NFeABI.RecepcaoEvento;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class EventosNFeABITests : NFeABITestFixture
    {
        private const string NomePedido = "Evento_110111-ped-eve.xml";
        private const string NomeRetorno = "Evento_110111-eve.xml";
        private const string NomeErro = "Evento_110111-eve.err";
        private const string NomeProcessado = Chave + "_110111_1-procEventoNFeABI.xml";
        private const string ProtocoloEvento = "1412600000000001";

        [Theory]
        [InlineData(135)]
        [InlineData(136)]
        public void ProcessaPedidoEArquivaEventoAssinado(int status)
        {
            using(var contexto = new Contexto(NomePedido, Pedido()))
            {
                var certificado = contexto.UsarCertificado();
                contexto.Empresa.PastaBackup = Path.Combine(contexto.PastaTemporaria, "Backup");
                TaskNFeABIEventos.ExecutarRecepcao = recepcao =>
                {
                    Assert.Equal(Servico.NFeABIRecepcaoEvento, recepcao.Configuracoes.Servico);
                    Assert.Equal(TipoAmbiente.Homologacao, recepcao.Configuracoes.TipoAmbiente);
                    Assert.Same(certificado, recepcao.Configuracoes.CertificadoDigital);
                    Responder(recepcao, status);
                };

                Processar(contexto);

                Assert.False(File.Exists(contexto.ArquivoEnvio));
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, NomeErro)));
                Assert.Equal(status.ToString(), Ler(Path.Combine(contexto.PastaRetorno, NomeRetorno))["retEventoNFeABI"]["infEvento"]["cStat"].InnerText);

                var caminho = contexto.Destino(NomeProcessado);
                var processado = Ler(caminho);
                Assert.Equal("procEventoNFeABI", processado.DocumentElement.LocalName);
                Assert.Equal(Namespace, processado.DocumentElement.NamespaceURI);
                Assert.Equal("1.00", processado.DocumentElement.GetAttribute("versao"));
                Assert.Equal(ProtocoloEvento, processado.DocumentElement["retEventoNFeABI", Namespace]["infEvento", Namespace]["nProt", Namespace].InnerText);
                var registro = processado.DocumentElement["eventoNFeABI", Namespace];
                var assinatura = new SignedXml(registro);
                assinatura.LoadXml(registro["Signature", SignedXml.XmlDsigNamespaceUrl]);
                Assert.True(assinatura.CheckSignature(certificado, true));
                var backup = Directory.GetFiles(contexto.Empresa.PastaBackup, NomeProcessado, SearchOption.AllDirectories);
                Assert.Equal(File.ReadAllText(caminho), File.ReadAllText(Assert.Single(backup)));
            }
        }

        [Fact]
        public void DevolveRejeicaoAoErpEArquivaPedidoNoErro()
        {
            using(var contexto = new Contexto(NomePedido, Pedido()))
            {
                contexto.UsarCertificado();
                TaskNFeABIEventos.ExecutarRecepcao = recepcao => Responder(recepcao, 573);

                Processar(contexto);

                Assert.False(File.Exists(contexto.ArquivoEnvio));
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, NomePedido)));
                Assert.False(File.Exists(contexto.Destino(NomeProcessado)));
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, NomeErro)));
                Assert.Equal("573", Ler(Path.Combine(contexto.PastaRetorno, NomeRetorno))["retEventoNFeABI"]["infEvento"]["cStat"].InnerText);
            }
        }

        [Theory]
        [InlineData(135, 2, ProtocoloEvento, "não corresponde ao evento")]
        [InlineData(135, 1, null, "está incompleto")]
        [InlineData(0, 1, ProtocoloEvento, "não contém um status válido")]
        public void NaoPublicaRespostaInvalidaAoErp(int status, int sequencia, string protocolo, string mensagem)
        {
            using(var contexto = new Contexto(NomePedido, Pedido()))
            {
                contexto.UsarCertificado();
                TaskNFeABIEventos.ExecutarRecepcao = recepcao => Responder(recepcao, status, sequencia, protocolo);

                Processar(contexto);

                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, NomeRetorno)));
                Assert.Contains(mensagem, File.ReadAllText(Path.Combine(contexto.PastaRetorno, NomeErro)));
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, NomePedido)));
                Assert.False(File.Exists(contexto.Destino(NomeProcessado)));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RegistraErroDeAmbienteOuTransporte(bool ambienteDivergente)
        {
            using(var contexto = new Contexto(NomePedido, Pedido(ambienteDivergente ? 1 : 2)))
            {
                contexto.UsarCertificado();
                var executou = false;
                TaskNFeABIEventos.ExecutarRecepcao = recepcao =>
                {
                    executou = true;
                    throw new InvalidOperationException("Falha simulada no transporte");
                };

                Processar(contexto);

                Assert.Equal(!ambienteDivergente, executou);
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, NomeRetorno)));
                Assert.Contains(ambienteDivergente ? "tpAmb" : "Falha simulada no transporte",
                    File.ReadAllText(Path.Combine(contexto.PastaRetorno, NomeErro)));
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, NomePedido)));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
            }
        }

        private static string Pedido(int ambiente = 2)
        {
            var arquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NFeABI", "Resources", NomePedido);
            var xml = Ler(arquivo);
            xml.DocumentElement["infEvento", Namespace]["tpAmb", Namespace].InnerText = ambiente.ToString();
            return xml.OuterXml;
        }

        private static XmlDocument Ler(string arquivo)
        {
            var xml = new XmlDocument { PreserveWhitespace = true };
            xml.Load(arquivo);
            return xml;
        }

        private static void Responder(RecepcaoEvento recepcao, int status, int sequencia = 1, string protocolo = ProtocoloEvento)
        {
            var enviado = recepcao.EventoNFeABI.InfEvento;
            var retorno = new RetEventoNFeABI
            {
                Versao = "1.00",
                InfEvento = new InfRetEventoNFeABI
                {
                    TpAmb = enviado.TpAmb,
                    COrgao = enviado.COrgao,
                    VerAplic = "TESTE",
                    CStat = status,
                    XMotivo = "RETORNO TESTE",
                    ChNFeABI = enviado.ChNFeABI,
                    TpEvento = enviado.TpEvento,
                    NSeqEvento = sequencia,
                    DhRegEvento = new DateTimeOffset(2026, 9, 14, 12, 0, 0, TimeSpan.FromHours(-3)),
                    NProt = protocolo
                }
            };

            recepcao.RetornoWSXML = retorno.GerarXML();
            recepcao.RetornoWSString = recepcao.RetornoWSXML.OuterXml;
        }
    }
}
