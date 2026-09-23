using NFe.Components;
using NFe.Service.NFeABI;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using UniNFe.Test.Abstractions;
using Unimake.Business.DFe.Servicos;
using Xunit;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class AutorizacaoNFeABITests : TaskTestFixtureBase, IDisposable
    {
        private readonly List<Empresa> anterior;
        private readonly string pasta;
        private readonly string erro;
        private X509Certificate2 certificadoTemporario;

        public AutorizacaoNFeABITests()
        {
            anterior = Empresas.Configuracoes;
            pasta = Path.Combine(Path.GetTempPath(), "UniNFe.Test.NFeABI", Guid.NewGuid().ToString("N")); erro = Path.Combine(pasta, "Erro");
            Directory.CreateDirectory(pasta); Directory.CreateDirectory(erro);
            Empresas.Configuracoes = new List<Empresa> { new Empresa { PastaXmlEnvio = pasta, PastaXmlEnviado = pasta, PastaXmlRetorno = pasta, PastaXmlErro = erro, PastaValidar = pasta, AmbienteCodigo = 2, Servico = TipoAplicativo.NFeABI, UsaCertificado = false } };
        }

        [Fact]
        public void AssinadoEhPersistidoAntesDoTransporteEPrechecksNaoChamamFactory()
        {
            var arquivo = Pedido(); var executou = false;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { executou = File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml")); return Retorno("100", "CHAVE"); });
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            Assert.True(executou);

            Empresas.Configuracoes[0].AmbienteCodigo = 1; executou = false;
            arquivo = Pedido(); ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            Assert.False(executou);
        }

        [Fact]
        public void RetomadaERejeicaoNaoReexecutamTransporte()
        {
            var arquivo = Pedido(); var chamadas = 0;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { chamadas++; return Retorno("999", "CHAVE"); });
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            Assert.Equal(1, chamadas);
            Assert.False(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-procNFeABI.xml")));

            // retorno previamente persistido só permite finalizar localmente; o fake não pode ser chamado.
            arquivo = Pedido();
            File.WriteAllText(Path.Combine(pasta, "pedido-ret-nfeabi.xml"), Retorno("100", "CHAVE"));
            Directory.CreateDirectory(Path.Combine(pasta, "EmProcessamento"));
            File.WriteAllText(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml"), Assinado());
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { throw new Exception("não pode retransmitir"); };
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            Assert.Equal(1, chamadas);
            Assert.False(File.Exists(arquivo));
            Assert.False(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml")));
        }

        [Fact]
        public void FalhaDeMovimentoAposRetornoPreservaEvidenciasEGeraErroSeguro()
        {
            var arquivo = Pedido(); var chamadas = 0;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { chamadas++; return Retorno("100", "CHAVE"); });
            TaskNFeABIRecepcaoSinc.MoverArquivo = (arquivoMover, destino, data) => { throw new IOException("falha simulada"); };
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            Assert.Equal(1, chamadas);
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.err")));
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { throw new Exception("retransmissão indevida"); };
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            Assert.Equal(1, chamadas);
        }

        [Fact]
        public void ColisaoDeProcIdenticoFinalizaLocalmenteSemRetransmissao()
        {
            var arquivo = Pedido();
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => Retorno("100", "CHAVE"));
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            arquivo = Pedido();
            Directory.CreateDirectory(Path.Combine(pasta, "EmProcessamento"));
            File.WriteAllText(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml"), Assinado());
            var factoryFoiChamada = false;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { factoryFoiChamada = true; throw new Exception("retransmissão indevida"); };
            TaskNFeABIRecepcaoSinc.MoverArquivo = (origem, destino, data) => { throw new Exception("movimentação indevida"); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            Assert.False(factoryFoiChamada);
            Assert.False(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-procNFeABI.xml")));
            Assert.False(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml")));
            Assert.False(File.Exists(arquivo));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));
        }

        [Fact]
        public void ColisaoDeProcIncompativelPreservaRetomadaEOriginalESinalizaErro()
        {
            var arquivo = Pedido();
            Directory.CreateDirectory(Path.Combine(pasta, "EmProcessamento"));
            File.WriteAllText(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml"), Assinado());
            File.WriteAllText(Path.Combine(pasta, "pedido-ret-nfeabi.xml"), Retorno("100", "CHAVE"));
            var destino = Path.Combine(pasta, "Autorizados", Empresas.Configuracoes[0].DiretorioSalvarComo.ToString(new DateTime(2026, 9, 15)), "pedido-procNFeABI.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(destino));
            File.WriteAllText(destino, "evidência de outra autorização");
            var factoryFoiChamada = false;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { factoryFoiChamada = true; throw new Exception("retransmissão indevida"); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            Assert.False(factoryFoiChamada);
            Assert.True(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-procNFeABI.xml")));
            Assert.True(File.Exists(arquivo));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.err")));
        }

        [Fact]
        public void InfRespTecExistenteEhPreservadoIntegralmente()
        {
            var recebido = ExecutarCapturandoPedido(Assinado().Replace("</infNFeABI>", "<infRespTec><CNPJ>ORIGINAL</CNPJ><xContato>CONTATO ORIGINAL</xContato><email>original@example.com</email><fone>41999999999</fone></infRespTec></infNFeABI>"));

            Assert.Contains("<CNPJ>ORIGINAL</CNPJ>", recebido);
            Assert.Contains("<xContato>CONTATO ORIGINAL</xContato>", recebido);
            Assert.Contains("<email>original@example.com</email>", recebido);
            Assert.Contains("<fone>41999999999</fone>", recebido);
        }

        [Fact]
        public void InfRespTecAusenteComConfiguracoesVaziasPermaneceAusente()
        {
            var recebido = ExecutarCapturandoPedido(Assinado());

            Assert.DoesNotContain("infRespTec", recebido);
        }

        [Fact]
        public void InfRespTecAusenteIncluiSomenteCamposConfigurados()
        {
            Empresas.Configuracoes[0].RespTecXContato = "CONTATO TESTE";
            Empresas.Configuracoes[0].RespTecTelefone = "41999999999";
            var recebido = ExecutarCapturandoPedido(Assinado());

            Assert.Contains("<xContato>CONTATO TESTE</xContato>", recebido);
            Assert.Contains("<fone>41999999999</fone>", recebido);
            Assert.DoesNotContain("<CNPJ>", recebido);
            Assert.DoesNotContain("<email>", recebido);
        }

        [Fact]
        public void CertificadoAusenteNaoCriaFactoryNemTransporte()
        {
            Empresas.Configuracoes[0].UsaCertificado = true;
            var factoryFoiChamada = false;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { factoryFoiChamada = true; throw new Exception("factory indevida"); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(Pedido()).Execute());

            Assert.False(factoryFoiChamada);
            Assert.Contains("Certificado", File.ReadAllText(Path.Combine(pasta, "pedido-ret-nfeabi.err")));
        }

        [Fact]
        public void CertificadoVencidoNaoCriaFactoryNemTransporte()
        {
            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest("CN=UniNFe Teste", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                certificadoTemporario = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddDays(-2));
            }
            Empresas.Configuracoes[0].UsaCertificado = true;
            Empresas.Configuracoes[0].X509Certificado = certificadoTemporario;
            var factoryFoiChamada = false;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { factoryFoiChamada = true; throw new Exception("factory indevida"); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(Pedido()).Execute());

            Assert.False(factoryFoiChamada);
            Assert.Contains("Certificado", File.ReadAllText(Path.Combine(pasta, "pedido-ret-nfeabi.err")));
        }

        [Fact]
        public void DiagnosticoPassivoEhLocalSanitizadoENaoChamaFactory()
        {
            Empresas.Configuracoes[0].AmbienteCodigo = 1;
            var factoryFoiChamada = false;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { factoryFoiChamada = true; throw new Exception("segredo=NAO_VAZAR"); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(Pedido()).Execute());

            Assert.False(factoryFoiChamada);
#if DEBUG || _BETA
            var diagnostico = File.ReadAllText(Path.Combine(pasta, "pedido-diagdispdfe.xml"));
            Assert.Contains("<TipoDFe>NFeABI</TipoDFe>", diagnostico);
            Assert.Contains("<CategoriaFalha>Configuracao</CategoriaFalha>", diagnostico);
            Assert.Contains("<RespostaFiscalRecebida>Nao</RespostaFiscalRecebida>", diagnostico);
            Assert.DoesNotContain("NAO_VAZAR", diagnostico);
#else
            Assert.False(File.Exists(Path.Combine(pasta, "pedido-diagdispdfe.xml")));
#endif
        }

        [Theory]
        [InlineData("Falha DNS segredo=NAO_VAZAR", "DNS")]
        [InlineData("Falha TLS segredo=NAO_VAZAR", "TLS")]
        [InlineData("Falha no proxy segredo=NAO_VAZAR", "Proxy")]
        [InlineData("Falha desconhecida segredo=NAO_VAZAR", "Transporte")]
        public void DiagnosticoClassificaFalhaDeTransporteSemVazarSegredo(string mensagem, string categoria)
        {
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { throw new Exception(mensagem); });

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(Pedido()).Execute());

            Assert.Contains("Falha de " + categoria, File.ReadAllText(Path.Combine(pasta, "pedido-ret-nfeabi.err")));
#if DEBUG || _BETA
            var diagnostico = File.ReadAllText(Path.Combine(pasta, "pedido-diagdispdfe.xml"));
            Assert.Contains("<CategoriaFalha>" + categoria + "</CategoriaFalha>", diagnostico);
            Assert.Contains("<Preparado>Sim</Preparado>", diagnostico);
            Assert.Contains("<TransporteIniciado>Sim</TransporteIniciado>", diagnostico);
            Assert.DoesNotContain("NAO_VAZAR", diagnostico);
#else
            Assert.False(File.Exists(Path.Combine(pasta, "pedido-diagdispdfe.xml")));
#endif
        }

        [Fact]
        public void InclusaoDoResponsavelTecnicoRemoveAssinaturaAntigaEAFactoryPersisteNovaAssinatura()
        {
            Empresas.Configuracoes[0].RespTecXContato = "CONTATO TESTE";
            string recebido = null;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (xml, configuracao) => { recebido = xml.OuterXml; return new Fake(() => string.Empty); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(Pedido()).Execute());

            Assert.Contains("infRespTec", recebido);
            Assert.DoesNotContain("<Signature", recebido);
            var persistido = File.ReadAllText(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml"));
            Assert.Contains("<Signature", persistido);
        }

        [Fact]
        public void FactoryRealReassinaDocumentoModificadoSemExecutarTransporte()
        {
            using (var rsa = RSA.Create(2048))
            {
                certificadoTemporario = new CertificateRequest("CN=UniNFe Teste", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                    .CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
            }
            var xml = new XmlDocument();
            xml.Load(Path.Combine("C:\\projetos\\github\\Unimake.DFe\\source\\Unimake.DFe.Test\\NFeABI\\Resources", "NFeABI-minima.xml"));
            var assinaturaAntiga = xml.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];
            assinaturaAntiga.ParentNode.RemoveChild(assinaturaAntiga);
            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao { TipoDFe = TipoDFe.NFeABI, CodigoUF = 41, TipoAmbiente = TipoAmbiente.Homologacao, CertificadoDigital = certificadoTemporario };

            using (var autorizacao = new Unimake.Business.DFe.Servicos.NFeABI.AutorizacaoSinc(xml.OuterXml, configuracao))
            {
                var assinado = autorizacao.ConteudoXMLAssinado;
                var assinaturaNova = (XmlElement)assinado.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];
                var validador = new SignedXml(assinado);
                validador.LoadXml(assinaturaNova);
                Assert.True(validador.CheckSignature(certificadoTemporario, true));
            }
        }

        [Theory]
        [InlineData("<retNFeABI xmlns=\"http://www.portalfiscal.inf.br/nfeabi\" versao=\"1.00\"><tpAmb>2</tpAmb></retNFeABI>")]
        [InlineData("<erro xmlns=\"http://www.portalfiscal.inf.br/nfeabi\"><cStat>999</cStat></erro>")]
        public void RetornoTecnicoInvalidoPreservaEvidenciasENaoLimpaComoRejeicao(string retorno)
        {
            var chamadas = 0;
            var arquivo = Pedido();
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { chamadas++; return retorno; });

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            Assert.Equal(1, chamadas);
            Assert.True(File.Exists(arquivo));
            Assert.True(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.err")));
        }

        [Fact]
        public void FalhaAoPersistirRespostaMantemArtefatoRecuperavelERetomaSemRetransmissao()
        {
            var chamadas = 0;
            var arquivo = Pedido();
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { chamadas++; return Retorno("100", "CHAVE"); });
            TaskNFeABIRecepcaoSinc.PersistirRetorno = retorno => { throw new IOException("falha de persistência simulada"); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            Assert.Equal(1, chamadas);
            Assert.True(File.Exists(arquivo));
            Assert.True(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-nfeabi.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-ret-nfeabi.xml")));
            Assert.Contains("Persistencia", File.ReadAllText(Path.Combine(pasta, "pedido-ret-nfeabi.err")));

            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { throw new Exception("retransmissão indevida"); };
            TaskNFeABIRecepcaoSinc.PersistirRetorno = retorno => File.WriteAllText(Path.Combine(pasta, "pedido-ret-nfeabi.xml"), retorno);
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            Assert.Equal(1, chamadas);
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));
        }

        [Fact]
        public void CleanupDeAutorizacaoRetomaLocalmenteAposFalhaAoExcluirPedido()
        {
            var arquivo = Pedido(); var chamadas = 0;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { chamadas++; return Retorno("100", "CHAVE"); });
            TaskNFeABIRecepcaoSinc.ExcluirArquivo = caminho => { if (caminho == arquivo) throw new IOException("falha simulada"); File.Delete(caminho); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            var recuperavel = Path.Combine(pasta, "EmProcessamento", "pedido-ret-nfeabi.xml");
            Assert.Equal(1, chamadas);
            Assert.True(File.Exists(arquivo));
            Assert.True(File.Exists(recuperavel));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));

            File.Delete(Path.Combine(pasta, "pedido-ret-nfeabi.xml"));
            TaskNFeABIRecepcaoSinc.ExcluirArquivo = File.Delete;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { throw new Exception("factory indevida"); };
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            Assert.Equal(1, chamadas);
            Assert.False(File.Exists(arquivo));
            Assert.False(File.Exists(recuperavel));
        }

        [Fact]
        public void CleanupDeRejeicaoRetomaLocalmenteAposFalhaAoExcluirPedido()
        {
            var arquivo = Pedido(); var chamadas = 0;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { chamadas++; return Retorno("999", "CHAVE"); });
            TaskNFeABIRecepcaoSinc.ExcluirArquivo = caminho => { if (caminho == arquivo) throw new IOException("falha simulada"); File.Delete(caminho); };

            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            var recuperavel = Path.Combine(pasta, "EmProcessamento", "pedido-ret-nfeabi.xml");
            Assert.Equal(1, chamadas);
            Assert.True(File.Exists(arquivo));
            Assert.True(File.Exists(recuperavel));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));

            File.Delete(Path.Combine(pasta, "pedido-ret-nfeabi.xml"));
            TaskNFeABIRecepcaoSinc.ExcluirArquivo = File.Delete;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => { throw new Exception("factory indevida"); };
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());

            Assert.Equal(1, chamadas);
            Assert.False(File.Exists(arquivo));
            Assert.False(File.Exists(recuperavel));
        }

        [Fact]
        public void DispatchRealDeProcessarExecutaAutorizacaoSemRede()
        {
            var executou = false;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => { executou = true; return Retorno("999", "CHAVE"); });
            Empresas.Configuracoes[0].PastaValidar = Path.Combine(pasta, "Validar");
            Directory.CreateDirectory(Empresas.Configuracoes[0].PastaValidar);

            var checarConexaoAnterior = ConfiguracaoApp.ChecarConexaoInternet;
            try
            {
                ConfiguracaoApp.ChecarConexaoInternet = false;
                ExecutarEmThread("0", () => new NFe.Service.Processar().ProcessaArquivo(0, Pedido()));
            }
            finally { ConfiguracaoApp.ChecarConexaoInternet = checarConexaoAnterior; }

            var erroDispatch = Path.Combine(pasta, "pedido-ret-nfeabi.err");
            Assert.True(executou, File.Exists(erroDispatch) ? File.ReadAllText(erroDispatch) : "dispatch não criou retorno de erro");
        }

        [Theory]
        [InlineData(false, "Autorizados")]
        [InlineData(true, "Originais")]
        public void SucessoMoveProcEOriginalParaDestinosContratados(bool somenteDistribuicao, string pastaOriginal)
        {
            Empresas.Configuracoes[0].SalvarSomenteXMLDistribuicao = somenteDistribuicao;
            var arquivo = Pedido();
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => Retorno("100", "CHAVE"));
            TaskNFeABIRecepcaoSinc.MoverArquivo = (origem, destino, data) =>
            {
                var alvo = Path.Combine(pasta, destino.ToString(), Empresas.Configuracoes[0].DiretorioSalvarComo.ToString(data), Path.GetFileName(origem));
                Directory.CreateDirectory(Path.GetDirectoryName(alvo)); File.Move(origem, alvo);
            };
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            var dir = Empresas.Configuracoes[0].DiretorioSalvarComo.ToString(new DateTime(2026, 9, 15));
            Assert.True(File.Exists(Path.Combine(pasta, "Autorizados", dir, "pedido-procNFeABI.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, pastaOriginal, dir, "pedido-nfeabi.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));
            Assert.False(File.Exists(arquivo));
            Assert.False(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-ret-nfeabi.xml")));
        }

        [Theory]
        [InlineData("1", "100", "CHAVE", "DIGEST")]
        [InlineData("2", "100", "OUTRA", "DIGEST")]
        [InlineData("2", "101", "CHAVE", "DIGEST")]
        [InlineData("2", "100", "CHAVE", "OUTRO")]
        public void RetornoIncoerenteNaoGeraProc(string ambiente, string protocolo, string chave, string digest)
        {
            var arquivo = Pedido();
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (x, c) => new Fake(() => RetornoIncoerente(ambiente, protocolo, chave, digest));
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(arquivo).Execute());
            Assert.False(File.Exists(Path.Combine(pasta, "EmProcessamento", "pedido-procNFeABI.xml")));
            Assert.True(File.Exists(Path.Combine(pasta, "pedido-ret-nfeabi.xml")));
        }

        public void Dispose()
        {
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = null; TaskNFeABIRecepcaoSinc.MoverArquivo = null; TaskNFeABIRecepcaoSinc.PersistirRetorno = null; TaskNFeABIRecepcaoSinc.ExcluirArquivo = null;
            Empresas.Configuracoes = anterior;
            certificadoTemporario?.Dispose();
            if (Directory.Exists(pasta)) Directory.Delete(pasta, true);
        }

        private string Pedido(string conteudo = null)
        {
            var arquivo = Path.Combine(pasta, "pedido-nfeabi.xml"); File.WriteAllText(arquivo, conteudo ?? Assinado()); return arquivo;
        }
        private string ExecutarCapturandoPedido(string pedido)
        {
            string recebido = null;
            TaskNFeABIRecepcaoSinc.CriarAutorizacao = (xml, configuracao) => { recebido = xml.OuterXml; return new Fake(() => Retorno("999", "CHAVE")); };
            ExecutarEmThread("0", () => new TaskNFeABIRecepcaoSinc(Pedido(pedido)).Execute());
            return recebido;
        }
        private static string Assinado() { return "<NFeABI xmlns=\"http://www.portalfiscal.inf.br/nfeabi\"><infNFeABI Id=\"NFeABICHAVE\" versao=\"1.00\"><ide><cUF>41</cUF><mod>77</mod><tpAmb>2</tpAmb><dhEmi>2026-09-15T10:00:00-03:00</dhEmi></ide></infNFeABI><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><Reference><DigestValue>DIGEST</DigestValue></Reference></SignedInfo></Signature></NFeABI>"; }
        private static string Retorno(string stat, string chave) { var protocolo = stat == "100" ? "<protNFeABI versao=\"1.00\"><infProt><tpAmb>2</tpAmb><verAplic>TESTE</verAplic><chNFeABI>" + chave + "</chNFeABI><dhRecbto>2026-09-15T10:00:00-03:00</dhRecbto><digVal>DIGEST</digVal><cStat>100</cStat><xMotivo>Autorizado</xMotivo></infProt></protNFeABI>" : string.Empty; return "<retNFeABI xmlns=\"http://www.portalfiscal.inf.br/nfeabi\" versao=\"1.00\"><tpAmb>2</tpAmb><verAplic>TESTE</verAplic><cStat>" + stat + "</cStat><xMotivo>Retorno teste</xMotivo><cUF>41</cUF><dhRecbto>2026-09-15T10:00:00-03:00</dhRecbto>" + protocolo + "</retNFeABI>"; }
        private static string RetornoIncoerente(string ambiente, string stat, string chave, string digest) { return "<retNFeABI xmlns=\"http://www.portalfiscal.inf.br/nfeabi\" versao=\"1.00\"><tpAmb>" + ambiente + "</tpAmb><verAplic>TESTE</verAplic><cStat>100</cStat><xMotivo>Autorizado</xMotivo><cUF>41</cUF><dhRecbto>2026-09-15T10:00:00-03:00</dhRecbto><protNFeABI versao=\"1.00\"><infProt><tpAmb>" + ambiente + "</tpAmb><verAplic>TESTE</verAplic><chNFeABI>" + chave + "</chNFeABI><dhRecbto>2026-09-15T10:00:00-03:00</dhRecbto><digVal>" + digest + "</digVal><cStat>" + stat + "</cStat><xMotivo>Autorizado</xMotivo></infProt></protNFeABI></retNFeABI>"; }
        private sealed class Fake : TaskNFeABIRecepcaoSinc.IAutorizacaoNFeABI
        {
            private readonly Func<string> executar; public Fake(Func<string> executar) { this.executar = executar; }
            public XmlDocument Assinado { get { var x = new XmlDocument(); x.LoadXml(AutorizacaoNFeABITests.Assinado()); return x; } }
            public string Executar() { return executar(); } public void Dispose() { }
        }
    }
}
