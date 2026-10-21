using NFe.Service;
using NFe.Service.NFeABI;
using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;
using Xunit;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class AutorizacaoNFeABITests : NFeABITestFixture
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AutorizacaoUsaProcessadoDaDllEArquivaOriginalNoDestinoConfigurado(bool somenteDistribuicao)
        {
            using(var contexto = new Contexto("pedido-nfeabi.xml", Documento().OuterXml))
            {
                contexto.UsarCertificado();
                contexto.Empresa.SalvarSomenteXMLDistribuicao = somenteDistribuicao;
                var executou = false;
                string retorno = null;
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao =>
                {
                    executou = true;
                    Assert.Equal(41, autorizacao.Configuracoes.CodigoUF);
                    Assert.True(autorizacao.Configuracoes.ColetarTelemetriaDisponibilidade);
                    Assert.Same(contexto.Empresa.X509Certificado, autorizacao.Configuracoes.CertificadoDigital);
                    retorno = Retorno("retNFeABI", 100, Protocolo(autorizacao.NFeABI.InfNFeABI.Chave));
                    autorizacao.RetornoWSString = retorno;
                };

                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));

                Assert.True(executou);
                Assert.True(File.Exists(contexto.Destino("pedido-procNFeABI.xml")));
                Assert.True(File.Exists(contexto.Destino("pedido-nfeabi.xml", somenteDistribuicao ? "Originais" : "Autorizados")));
                Assert.Equal(retorno, File.ReadAllText(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.xml")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
                Assert.False(File.Exists(Path.Combine(contexto.EmProcessamento, "pedido-nfeabi.xml")));
                Assert.False(File.Exists(Path.Combine(contexto.EmProcessamento, "pedido-ret-nfeabi.xml")));
                Assert.False(File.Exists(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.err")));
                var proc = CarregarXml(contexto.Destino("pedido-procNFeABI.xml"));
                var assinatura = proc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);
                Assert.Equal(1, assinatura.Count);
                var validador = new SignedXml(proc);
                validador.LoadXml((XmlElement)assinatura[0]);
                Assert.True(validador.CheckSignature(contexto.Empresa.X509Certificado, true));
            }
        }

        [Fact]
        public void RejeicaoGravaRetornoBrutoEMoveAssinadoParaErro()
        {
            using(var contexto = new Contexto("pedido-nfeabi.xml", Documento().OuterXml))
            {
                contexto.UsarCertificado();
                var retorno = Retorno("retNFeABI", 999);
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao => autorizacao.RetornoWSString = retorno;
                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));
                Assert.Equal(retorno, File.ReadAllText(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.xml")));
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, "pedido-nfeabi.xml")));
                Assert.False(File.Exists(contexto.Destino("pedido-procNFeABI.xml")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ResponsavelTecnicoAusenteDelegaValidacaoDosCamposDisponiveisParaDll(bool configurar)
        {
            using(var contexto = new Contexto("pedido-nfeabi.xml", Documento(false).OuterXml))
            {
                contexto.UsarCertificado();
                if(configurar) contexto.Empresa.RespTecXContato = "CONTATO TESTE";
                var executou = false;
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao =>
                {
                    executou = true;
                    autorizacao.RetornoWSString = Retorno("retNFeABI", 999);
                };
                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));
                Assert.False(executou);
                var erro = File.ReadAllText(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.err"));
                Assert.Contains("Erro ao validar schema", erro);
                Assert.Contains(configurar ? "CNPJ" : "infRespTec", erro);
            }
        }

        [Fact]
        public void ResponsavelTecnicoExistenteEhPreservado()
        {
            var xml = Documento(false);
            var grupo = xml.CreateElement("infRespTec", Namespace);
            grupo.InnerXml = "<CNPJ xmlns=\"" + Namespace + "\">12345678000123</CNPJ><xContato xmlns=\"" + Namespace +
                "\">CONTATO ORIGINAL</xContato><email xmlns=\"" + Namespace + "\">original@example.com</email><fone xmlns=\"" + Namespace + "\">41999999999</fone>";
            xml.GetElementsByTagName("infNFeABI")[0].AppendChild(grupo);
            using(var contexto = new Contexto("pedido-nfeabi.xml", xml.OuterXml))
            {
                contexto.UsarCertificado();
                contexto.Empresa.RespTecXContato = "NAO SUBSTITUIR";
                var executou = false;
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao =>
                {
                    executou = true;
                    var responsavel = autorizacao.NFeABI.InfNFeABI.InfRespTec;
                    Assert.Equal("CONTATO ORIGINAL", responsavel.XContato);
                    Assert.Equal("12345678000123", responsavel.CNPJ);
                    Assert.Equal("original@example.com", responsavel.Email);
                    Assert.Equal("41999999999", responsavel.Fone);
                    autorizacao.RetornoWSString = Retorno("retNFeABI", 999);
                };
                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));
                Assert.True(executou);
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.xml")));
            }
        }

        [Fact]
        public void InclusaoDoResponsavelTecnicoGeraNovaAssinaturaValidaPelaDll()
        {
            using(var contexto = new Contexto("pedido-nfeabi.xml", Documento().OuterXml))
            {
                var certificado = contexto.UsarCertificado();
                var xml = Documento();
                using(var anterior = new Unimake.Business.DFe.Servicos.NFeABI.AutorizacaoSinc(xml.OuterXml,
                    new Unimake.Business.DFe.Servicos.Configuracao { CertificadoDigital = certificado }))
                {
                    anterior.ConteudoXMLAssinado.Save(contexto.ArquivoEnvio);
                }
                var pedido = CarregarXml(contexto.ArquivoEnvio);
                var responsavel = pedido.GetElementsByTagName("infRespTec")[0];
                responsavel.ParentNode.RemoveChild(responsavel);
                pedido.Save(contexto.ArquivoEnvio);
                contexto.Empresa.RespTecCNPJ = "12345678000123";
                contexto.Empresa.RespTecEmail = "novo@example.com";
                contexto.Empresa.RespTecTelefone = "41999999999";
                contexto.Empresa.RespTecXContato = "CONTATO TESTE";
                var executou = false;
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao =>
                {
                    executou = true;
                    var assinado = autorizacao.ConteudoXMLAssinado;
                    Assert.Contains("CONTATO TESTE", assinado.OuterXml);
                    var validador = new SignedXml(assinado);
                    validador.LoadXml((XmlElement)assinado.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0]);
                    Assert.True(validador.CheckSignature(certificado, true));
                    autorizacao.RetornoWSString = Retorno("retNFeABI", 999);
                };
                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));
                Assert.True(executou);
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.xml")));
            }
        }

        [Theory]
        [InlineData("Falha DNS da DLL.")]
        [InlineData("Falha TLS da DLL.")]
        [InlineData("Falha no proxy da DLL.")]
        public void FalhaDeTransportePreservaMensagemOriginalNoErroPadrao(string mensagem)
        {
            using(var contexto = new Contexto("pedido-nfeabi.xml", Documento().OuterXml))
            {
                contexto.UsarCertificado();
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao => { throw new InvalidOperationException(mensagem); };
                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));
                var erro = File.ReadAllText(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.err"));
                Assert.Contains("Message|" + mensagem, erro);
                Assert.Contains("Type|System.InvalidOperationException", erro);
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, "pedido-nfeabi.xml")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
            }
        }

        [Fact]
        public void NomesAcimaDoLimiteSaoValidadosPelaDllComErroOriginal()
        {
            var xml = Documento();
            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("abi", Namespace);
            var fantasia = xml.CreateElement("xFant", Namespace);
            var emitente = xml.SelectSingleNode("//abi:emit", ns);
            emitente.InsertAfter(fantasia, emitente.SelectSingleNode("abi:xNome", ns));
            foreach(XmlNode nome in xml.SelectNodes("//abi:emit/abi:xNome|//abi:emit/abi:xFant|//abi:adquirente/abi:xNome", ns))
            {
                nome.InnerText = new string('X', 62);
            }
            using(var contexto = new Contexto("pedido-nfeabi.xml", xml.OuterXml))
            {
                contexto.UsarCertificado();
                var executou = false;
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao => { executou = true; };
                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));
                var erro = File.ReadAllText(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.err"));
                Assert.False(executou);
                Assert.Contains("xNome", erro);
                Assert.Contains("xFant", erro);
                Assert.DoesNotContain("Falha de XML na autorização", erro);
            }
        }

        [Fact]
        public void FalhaAoMoverProcessadoPreservaOriginalEmProcessamento()
        {
            using(var contexto = new Contexto("pedido-nfeabi.xml", Documento().OuterXml))
            {
                contexto.UsarCertificado();
                Directory.CreateDirectory(contexto.Destino("pedido-procNFeABI.xml"));
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao =>
                    autorizacao.RetornoWSString = Retorno("retNFeABI", 100, Protocolo(autorizacao.NFeABI.InfNFeABI.Chave));
                Executar(new TaskNFeABIRecepcaoSinc(contexto.ArquivoEnvio));
                Assert.True(File.Exists(Path.Combine(contexto.EmProcessamento, "pedido-nfeabi.xml")));
                Assert.False(File.Exists(contexto.Destino("pedido-nfeabi.xml")));
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.err")));
            }
        }

        [Fact]
        public void RoteamentoRealExecutaAutorizacaoSemRede()
        {
            using(var contexto = new Contexto("pedido-nfeabi.xml", Documento().OuterXml))
            {
                contexto.UsarCertificado();
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = autorizacao => autorizacao.RetornoWSString = Retorno("retNFeABI", 999);
                Processar(contexto);
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "pedido-ret-nfeabi.xml")));
                Assert.False(File.Exists(contexto.ArquivoEnvio));
            }
        }
    }
}
