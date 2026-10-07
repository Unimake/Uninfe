using NFe.Components;
using NFe.Service;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Unimake.Exceptions;
using Xunit;

namespace UniNFe.Test.Autorizacao
{
    [Collection("NFe Recepcao Serial")]
    public class NFeRecepcaoFluxoTests
    {
        private readonly NFeRecepcaoTestFixture fixture;

        public NFeRecepcaoFluxoTests(NFeRecepcaoTestFixture fixture) => this.fixture = fixture;

        [Theory]
        [InlineData(55, false)]
        [InlineData(55, true)]
        [InlineData(65, false)]
        [InlineData(65, true)]
        public void IpiInvalidoPublicaLoteEEncerraFalhaLocalSemComunicarComSefaz(int modelo, bool gravarTXT)
        {
            using(var contexto = new NFeRecepcaoTestContext(modelo: modelo, gravarTXT: gravarTXT, ipiInvalido: true))
            {
                // A fábrica real lança ValidatorDFeException antes de assinar ou criar o transporte.
                fixture.Executar(contexto, (task, xml) => { });

                VerificarReferencias(contexto, gravarTXT);
                var erro = File.ReadAllText(contexto.ArquivoErroLote);
                Assert.True(erro.Contains("O CST do grupo de tributação do IPI não tributado está incorreto. Valor informado: 00"), erro);
                VerificarEncerramentoLocal(contexto);
                var logs = contexto.LerLogs();
                Assert.DoesNotContain("Falha tecnica sem retorno fiscal conclusivo", logs);
                Assert.True(logs.IndexOf("Número do lote publicado", StringComparison.Ordinal) <
                    logs.IndexOf("Retorno de erro gravado", StringComparison.Ordinal));
                Assert.True(logs.IndexOf("Falha local encerrada", StringComparison.Ordinal) <
                    logs.IndexOf("Retorno de erro gravado", StringComparison.Ordinal));
            }
        }

        [Fact]
        public void ConfiguracaoCscAusentePublicaLoteEEncerraNotaSemCriarAutorizacao()
        {
            using(var contexto = new NFeRecepcaoTestContext(modelo: 65))
            {
                contexto.Empresa.VersaoQRCodeNFCe = 2;
                var criou = false;
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    criou = true;
                    throw new Exception("Não deveria criar autorização sem CSC.");
                });

                Assert.False(criou);
                VerificarReferencias(contexto, false);
                Assert.Contains("CSC e IDToken", File.ReadAllText(contexto.ArquivoErroLote));
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FalhaDeSchemaPublicaReferenciasEGravaErroDeCadaNota(bool emProcessamento)
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: 2, gravarTXT: true,
                emLote: !emProcessamento, emProcessamento: emProcessamento))
            {
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidarXMLException("Schema inválido para teste.");
                });

                VerificarReferencias(contexto, true);
                for(var i = 0; i < contexto.Notas.Count; i++)
                {
                    var erro = Path.Combine(contexto.PastaRetorno,
                        Functions.ExtrairNomeArq(contexto.NomeNota(i), Propriedade.ExtEnvio.NFe) + Propriedade.ExtRetorno.Nfe_ERR);
                    Assert.Contains("Schema inválido para teste.", File.ReadAllText(erro));
                }

                Assert.False(File.Exists(contexto.ArquivoErroLote));
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void FalhaDeComunicacaoPublicaReferenciasEPreservaNotasNoFluxo(int quantidade)
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: quantidade, gravarTXT: true))
            {
                var executou = false;
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                    new AutorizacaoNFeFake(lote, xml, () =>
                    {
                        executou = true;
                        Assert.All(Enumerable.Range(0, quantidade), i => Assert.False(File.Exists(contexto.Referencia(i))));
                        throw new IOException("Comunicação interrompida após tentativa de envio.");
                    }));

                Assert.True(executou);
                VerificarReferencias(contexto, true);
                Assert.Contains("Comunicação interrompida", File.ReadAllText(contexto.ArquivoErroLote));
                for(var i = 0; i < quantidade; i++)
                {
                    Assert.True(File.Exists(Path.Combine(contexto.PastaEmProcessamento, contexto.NomeNota(i))));
                    Assert.False(File.Exists(contexto.Notas[i].NomeArquivoXML));
                    Assert.False(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, contexto.NomeNota(i))));
                }

                fixture.VerificarFluxo(contexto, true);
            }
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(2, true)]
        public void NotasAutorizadasPublicamTodasReferenciasSomenteDepoisDaExecucao(int quantidade, bool gravarTXT)
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: quantidade, gravarTXT: gravarTXT))
            {
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                    new AutorizacaoNFeFake(lote, xml, () =>
                    {
                        Assert.All(Enumerable.Range(0, quantidade), i => Assert.False(File.Exists(contexto.Referencia(i))));
                    }, RetornoProcessado(contexto, 100)));

                VerificarReferencias(contexto, gravarTXT);
                Assert.False(File.Exists(contexto.ArquivoErroLote));
                Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "000000000000201" + Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML)));
                var pastaAutorizados = Path.Combine(contexto.Empresa.PastaXmlEnviado, PastaEnviados.Autorizados.ToString());
                Assert.Equal(quantidade, Directory.GetFiles(pastaAutorizados, "*" + Propriedade.ExtRetorno.ProcNFe,
                    SearchOption.AllDirectories).Length);
                Assert.Empty(Directory.GetFiles(contexto.PastaEmProcessamento));
                fixture.VerificarFluxo(contexto, false);
            }
        }

        [Fact]
        public void RejeicaoFiscalMantemRetornoXmlEEncerraNotas()
        {
            using(var contexto = new NFeRecepcaoTestContext())
            {
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                    new AutorizacaoNFeFake(lote, xml, () => { },
                        "<retEnviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\">" +
                        "<tpAmb>2</tpAmb><cStat>539</cStat><xMotivo>Rejeição teste</xMotivo><cUF>41</cUF></retEnviNFe>"));

                VerificarReferencias(contexto, false);
                Assert.False(File.Exists(contexto.ArquivoErroLote));
                var retorno = Path.Combine(contexto.PastaRetorno, "000000000000201" + Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML);
                Assert.Contains("<cStat>539</cStat>", File.ReadAllText(retorno));
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LoteComVariasNotasPublicaTodasReferenciasNoErroLocal(bool gravarTXT)
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: 2, gravarTXT: gravarTXT, emLote: true))
            {
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidatorDFeException("Tributação inválida em uma das notas.");
                });

                VerificarReferencias(contexto, gravarTXT);
                Assert.Contains("Tributação inválida", File.ReadAllText(contexto.ArquivoErroLote));
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Fact]
        public void PublicacaoParcialPreservaTxtPendenteEErroOriginal()
        {
            using(var contexto = new NFeRecepcaoTestContext(gravarTXT: true))
            {
                // Um diretório no destino impede somente a publicação do TXT.
                Directory.CreateDirectory(contexto.Referencia(0, ".txt"));
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidatorDFeException("Erro fiscal original.");
                });

                Assert.True(File.Exists(contexto.Referencia(0)));
                Assert.False(File.Exists(contexto.Referencia(0, temporaria: true)));
                Assert.True(File.Exists(contexto.Referencia(0, ".txt", true)));
                Assert.Contains("Erro fiscal original.", File.ReadAllText(contexto.ArquivoErroLote));
                Assert.Contains("Falha ao publicar o número do lote", contexto.LerLogs());
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Fact]
        public void FalhaDePublicacaoAposSucessoNaoRepeteXmlPublicadoEPreservaReferenciaPendente()
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: 2))
            {
                Directory.CreateDirectory(contexto.Referencia(1));
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                    new AutorizacaoNFeFake(lote, xml, () => { }, RetornoProcessado(contexto, 100)));

                Assert.True(File.Exists(contexto.Referencia(0)));
                Assert.False(File.Exists(contexto.Referencia(0, temporaria: true)));
                Assert.True(File.Exists(contexto.Referencia(1, temporaria: true)));
                var erro = File.ReadAllText(contexto.ArquivoErroLote);
                Assert.DoesNotContain("Não foi possível localizar o arquivo", erro);
                Assert.DoesNotContain("Could not find file", erro);
                Assert.Contains("Falha ao publicar o número do lote", contexto.LerLogs());
                fixture.VerificarFluxo(contexto, true);
                Assert.Equal(2, Directory.GetFiles(contexto.PastaEmProcessamento, "*" + Propriedade.ExtEnvio.NFe).Length);
            }
        }

        [Fact]
        public void FalhaAoMoverNotaPreservaOriginalEFluxoSemImpedirRetornoDeErro()
        {
            using(var contexto = new NFeRecepcaoTestContext())
            {
                Directory.CreateDirectory(Path.Combine(contexto.Empresa.PastaXmlErro, contexto.NomeNota(0)));
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidatorDFeException("Erro fiscal original.");
                });

                VerificarReferencias(contexto, false);
                Assert.Contains("Erro fiscal original.", File.ReadAllText(contexto.ArquivoErroLote));
                Assert.True(File.Exists(contexto.Notas[0].NomeArquivoXML));
                Assert.Contains("Falha ao encerrar nota com erro local", contexto.LerLogs());
                fixture.VerificarFluxo(contexto, true);
            }
        }

        [Fact]
        public void FalhaAoMoverNotaComSchemaInvalidoNaoImpedeErroPorNota()
        {
            using(var contexto = new NFeRecepcaoTestContext(emProcessamento: true))
            {
                Directory.CreateDirectory(Path.Combine(contexto.Empresa.PastaXmlErro, contexto.NomeNota(0)));
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidarXMLException("Schema inválido original.");
                });

                VerificarReferencias(contexto, false);
                var erro = Path.Combine(contexto.PastaRetorno, "nota1" + Propriedade.ExtRetorno.Nfe_ERR);
                Assert.Contains("Schema inválido original.", File.ReadAllText(erro));
                Assert.True(File.Exists(contexto.Notas[0].NomeArquivoXML));
                fixture.VerificarFluxo(contexto, true);
            }
        }

        [Fact]
        public void NotaSemEntradaNoFluxoEhIdentificadaPelaChaveNoArquivoCorreto()
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: 2))
            {
                fixture.Executar(contexto, (task, xml) =>
                {
                    new FluxoNfe().ExcluirNfeFluxo(contexto.Chave(1));
                    task.CriarAutorizacao = (lote, configuracao) =>
                    {
                        throw new ValidatorDFeException("Erro fiscal original.");
                    };
                });

                VerificarReferencias(contexto, false);
                VerificarEncerramentoLocal(contexto);
                for(var i = 0; i < contexto.Notas.Count; i++)
                {
                    var arquivo = fixture.CarregarXml(Path.Combine(contexto.Empresa.PastaXmlErro, contexto.NomeNota(i)));
                    Assert.Equal(contexto.Chave(i), ((XmlElement)arquivo.GetElementsByTagName("infNFe")[0]).GetAttribute("Id"));
                }
            }
        }

        [Fact]
        public void PastaEmLoteNaoConfiguradaNaoImpedeEncerramentoLocal()
        {
            using(var contexto = new NFeRecepcaoTestContext())
            {
                contexto.Empresa.PastaXmlEmLote = string.Empty;
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidatorDFeException("Erro fiscal original.");
                });

                VerificarReferencias(contexto, false);
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Fact]
        public void RetornoAnteriorEhSubstituidoEPublicacaoSingularContinuaCompativel()
        {
            using(var contexto = new NFeRecepcaoTestContext(gravarTXT: true))
            {
                File.WriteAllText(contexto.Referencia(0), "<DadosLoteNfe><NumeroLoteGerado>200</NumeroLoteGerado></DadosLoteNfe>");
                File.WriteAllText(contexto.Referencia(0, ".txt"), "200;");
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidatorDFeException("Erro local.");
                }, propriedadesSingulares: true);

                VerificarReferencias(contexto, true);
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Fact]
        public void LoteProntoDoErpNaoPublicaReferenciasAdicionais()
        {
            using(var contexto = new NFeRecepcaoTestContext())
            {
                fixture.Executar(contexto, (task, xml) => task.CriarAutorizacao = (lote, configuracao) =>
                {
                    throw new ValidatorDFeException("Erro local.");
                }, lotePronto: true);

                Assert.Empty(Directory.GetFiles(contexto.PastaRetorno, "*-num-lot.*"));
                Assert.Contains("Erro local.", File.ReadAllText(contexto.ArquivoErroLote));
                VerificarEncerramentoLocal(contexto);
            }
        }

        [Fact]
        public void NovaMontagemReiniciaColecaoDeReferencias()
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: 2, gravarTXT: true))
            {
                fixture.EmThread(() =>
                {
                    fixture.PrepararFluxo(contexto);
                    var gerador = new GerarXML(0);
                    gerador.LoteNfe(Servicos.NFeMontarLoteUma, contexto.Notas, "4.00", "55");
                    Assert.Equal(2, gerador.ArquivosTemporariosNumeroLote.Count);
                    gerador.LoteNfe(Servicos.NFeMontarLoteUma, contexto.Notas.Take(1).ToList(), "4.00", "55");

                    var referencia = Assert.Single(gerador.ArquivosTemporariosNumeroLote);
                    Assert.Equal(contexto.Referencia(0, temporaria: true), referencia.Key, ignoreCase: true);
                    Assert.Equal(contexto.Referencia(0, ".txt", true), referencia.Value, ignoreCase: true);
                    Assert.Contains("<NumeroLoteGerado>202</NumeroLoteGerado>", File.ReadAllText(referencia.Key));
                    Assert.Equal("202;", File.ReadAllText(referencia.Value));
                });
            }
        }

        [Fact]
        public void ColecaoMantemSomenteReferenciasDoXmlDeLoteRetornado()
        {
            using(var contexto = new NFeRecepcaoTestContext(quantidade: 51))
            {
                fixture.EmThread(() =>
                {
                    fixture.PrepararFluxo(contexto);
                    var gerador = new GerarXML(0);
                    var xml = gerador.LoteNfe(Servicos.NFeMontarLoteUma, contexto.Notas, "4.00", "55");

                    Assert.Equal(1, xml.GetElementsByTagName("NFe").Count);
                    var referencia = Assert.Single(gerador.ArquivosTemporariosNumeroLote);
                    Assert.Equal(contexto.Referencia(50, temporaria: true), referencia.Key, ignoreCase: true);
                    Assert.Contains("<NumeroLoteGerado>202</NumeroLoteGerado>", File.ReadAllText(referencia.Key));
                });
            }
        }

        private void VerificarReferencias(NFeRecepcaoTestContext contexto, bool gravarTXT)
        {
            for(var i = 0; i < contexto.Notas.Count; i++)
            {
                var xml = fixture.CarregarXml(contexto.Referencia(i));
                Assert.Equal("201", xml.GetElementsByTagName("NumeroLoteGerado")[0].InnerText);
                Assert.Equal(gravarTXT, File.Exists(contexto.Referencia(i, ".txt")));
                if(gravarTXT)
                {
                    Assert.Equal("201;", File.ReadAllText(contexto.Referencia(i, ".txt")));
                }

                Assert.False(File.Exists(contexto.Referencia(i, temporaria: true)));
                Assert.False(File.Exists(contexto.Referencia(i, ".txt", true)));
            }
        }

        private void VerificarEncerramentoLocal(NFeRecepcaoTestContext contexto)
        {
            for(var i = 0; i < contexto.Notas.Count; i++)
            {
                Assert.True(File.Exists(Path.Combine(contexto.Empresa.PastaXmlErro, contexto.NomeNota(i))));
                Assert.False(File.Exists(contexto.Notas[i].NomeArquivoXML));
                Assert.False(File.Exists(Path.Combine(contexto.PastaEmProcessamento, contexto.NomeNota(i))));
            }

            fixture.VerificarFluxo(contexto, false);
        }

        private static string RetornoProcessado(NFeRecepcaoTestContext contexto, int cStatNota)
        {
            var protocolos = string.Join(string.Empty, Enumerable.Range(0, contexto.Notas.Count).Select(i =>
                "<protNFe versao=\"4.00\"><infProt><tpAmb>2</tpAmb><verAplic>TESTE</verAplic><chNFe>" +
                contexto.Chave(i).Substring(3) + "</chNFe><dhRecbto>2026-10-07T10:01:00-03:00</dhRecbto>" +
                "<nProt>141260000000001</nProt><digVal>dGVzdGU=</digVal><cStat>" + cStatNota + "</cStat>" +
                "<xMotivo>Resultado fiscal teste</xMotivo></infProt></protNFe>"));
            return "<retEnviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><verAplic>TESTE</verAplic>" +
                "<cStat>104</cStat><xMotivo>Lote processado</xMotivo><cUF>41</cUF>" + protocolos + "</retEnviNFe>";
        }
    }
}
