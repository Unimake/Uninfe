using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using Unimake.Business.DFe.Servicos;
namespace NFe.Service.NFSe
{
    public class TaskNFSeRecepcionarLoteRps : TaskAbst
    {
        #region Objeto com os dados do XML de lote rps

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do lote rps
        /// </summary>
        private DadosEnvLoteRps oDadosEnvLoteRps;

        #endregion Objeto com os dados do XML de lote rps

        public TaskNFSeRecepcionarLoteRps(string arquivo)
        {
            Servico = Servicos.NFSeRecepcionarLoteRps;

            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }
        
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            if (Empresas.Configuracoes[emp].TempoEnvioNFSe > 0)
            {
                while (true)
                {
                    lock (Smf.RecepcionarLoteRps)
                    {
                        if (Empresas.Configuracoes[emp].DataHoraUltimoEnvioNFSe != DateTime.MinValue)
                        {
                            var diferenca = DateTime.Now - Empresas.Configuracoes[emp].DataHoraUltimoEnvioNFSe;
                            var segundosPassados = diferenca.TotalSeconds;

                            if (segundosPassados < Empresas.Configuracoes[emp].TempoEnvioNFSe)
                            {
                                Thread.Sleep((Empresas.Configuracoes[emp].TempoEnvioNFSe - Convert.ToInt32(segundosPassados)) * 1000);
                            }
                        }

                        Empresas.Configuracoes[emp].DataHoraUltimoEnvioNFSe = DateTime.Now;
                        break;
                    }
                }
            }

            //Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeRecepcionarLoteRps;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).EnvioXML) + Propriedade.ExtRetorno.RetEnvLoteRps_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosEnvLoteRps = new DadosEnvLoteRps(emp);

                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosEnvLoteRps.cMunicipio);

                ExecuteDLL(emp, oDadosEnvLoteRps.cMunicipio, padraoNFSe);

                AnalisarRetorno(vStrXmlRetorno, padraoNFSe, emp);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).EnvioXML, Propriedade.ExtRetorno.RetEnvLoteRps_ERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 31/08/2011
                }
            }
            finally
            {
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de cancelamento de NFe, infelizmente
                    //não posso fazer mais nada, o UniNFe vai tentar mandar o arquivo novamente para o webservice, pois ainda não foi excluido.
                    //Wandrey 31/08/2011
                }
            }
        }

        /// <summary>
        /// Analisar o XML retornado se for ambiente nacional e se a nota tiver sido autorizada vamos salvar o XML na pasta autorizados
        /// </summary>
        /// <param name="vStrXmlRetorno">XML Retornado</param>
        /// <param name="padraoNFSe">Padrão da NFSe</param>
        /// <param name="emp">Codigo da empresa</param>
        private void AnalisarRetorno(string vStrXmlRetorno, PadraoNFSe padraoNFSe, int emp)
        {
            if (padraoNFSe != PadraoNFSe.NACIONAL)
            {
                return;
            }

            var pastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado;
            if (string.IsNullOrWhiteSpace(pastaEnviado))
            {
                return;
            }

            try
            {
                var autorizou = false;
                var doc = new XmlDocument();
                doc.LoadXml(vStrXmlRetorno);
                var nfseNodes = doc.GetElementsByTagName("NFSe");
                foreach (XmlElement nfse in nfseNodes)
                {
                    var infNFSe = nfse["infNFSe"] as XmlElement;
                    if (infNFSe == null)
                    {
                        continue;
                    }

                    var cStat = infNFSe["cStat"]?.InnerText;
                    var autorizados = new HashSet<string> { "100", "101", "102", "103", "107" }; // Somente autorizadas
                    if (!autorizados.Contains(cStat))
                    {
                        continue;
                    }
                    autorizou = true;

                    var dps = infNFSe["DPS"] as XmlElement;
                    var infDPS = dps?["infDPS"] as XmlElement;
                    var dhEmiTexto = infDPS?["dhEmi"]?.InnerText;
                    if (string.IsNullOrWhiteSpace(dhEmiTexto))
                    {
                        continue;
                    }

                    if (!DateTimeOffset.TryParseExact(dhEmiTexto, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dhEmiOffset))
                    {
                        continue;
                    }

                    var dhEmi = dhEmiOffset.DateTime;

                    var id = infNFSe.GetAttribute("Id");
                    if (string.IsNullOrWhiteSpace(id) || id.Length <= 3)
                    {
                        continue;
                    }

                    var nameArq = id.Substring(3) + "-procnfse.xml";
                    var pathFile = Path.Combine(pastaEnviado, PastaEnviados.Autorizados.ToString(), Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(dhEmi), nameArq);

                    var dir = Path.GetDirectoryName(pathFile);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (!File.Exists(pathFile))
                    {
                        File.WriteAllText(pathFile, vStrXmlRetorno);
                    }

                    //Disparar UniDANFE
                    try
                    {
                        UniDanfe.Executar(pathFile, dhEmi, Empresas.Configuracoes[emp]);
                    }
                    catch (Exception ex)
                    {
                        Auxiliar.WriteLog("TaskRecepcionarLoteRps: (Falha na execução do UniDANFe) " + ex.Message, false);
                    }
                }

                if (!autorizou)
                {
                    Functions.Move(NomeArquivoXML, Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Path.GetFileName(NomeArquivoXML))); //Move o arquivo para a pasta de erro
                }
            }
            catch (Exception ex)
            {
                // Logar erro se necessário, mas não lançar para não interromper o fluxo
                Auxiliar.WriteLog($"Erro ao salvar o XML da NFSe autorizado: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Executa o serviço utilizando a DLL do UniNFe.
        /// </summary>
        /// <param name="emp">Empresa que está enviando o XML</param>
        /// <param name="municipio">Código do município para onde será enviado o XML</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        private void ExecuteDLL(int emp, int municipio, PadraoNFSe padraoNFSe)
        {
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(NomeArquivoXML);

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).RetornoXML;
            var resolucao = ResolucaoCentralizadaNFSe.Resolver(
                conteudoXML,
                padraoNFSe,
                municipio,
                (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo);
            var versaoXML = resolucao.Versao;
            var servico = resolucao.Servico;

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = servico,
                SchemaVersao = versaoXML,
                MunicipioToken = Empresas.Configuracoes[emp].SenhaWS,
                TokenSoap = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioSenha = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioUsuario = Empresas.Configuracoes[emp].UsuarioWS
            };

            if (padraoNFSe == PadraoNFSe.WEBFISCO)
            {
                XmlElement root = conteudoXML.DocumentElement;
                XmlNode firstElement = root.FirstChild;
                XmlNode tagUsuario = conteudoXML.CreateElement("usuario");
                XmlNode tagSenha = conteudoXML.CreateElement("pass");

                tagUsuario.InnerText = configuracao.MunicipioUsuario;
                tagSenha.InnerText = configuracao.MunicipioSenha;
                root.InsertBefore(tagUsuario, firstElement);
                root.InsertBefore(tagSenha, firstElement);

                conteudoXML.AppendChild(root);
            }

            switch (servico)
            {
                case Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse:
                    var gerarNfse = new Unimake.Business.DFe.Servicos.NFSe.GerarNfse(conteudoXML, configuracao);
                    gerarNfse.Executar();
                    vStrXmlRetorno = gerarNfse.RetornoWSString;

                    gerarNfse.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps:
                    var recepcionarLoteRps = new Unimake.Business.DFe.Servicos.NFSe.RecepcionarLoteRps(conteudoXML, configuracao);
                    recepcionarLoteRps.Executar();
                    vStrXmlRetorno = recepcionarLoteRps.RetornoWSString;

                    recepcionarLoteRps.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRpsSincrono:
                    var recepcionarLoteRpsSincrono = new Unimake.Business.DFe.Servicos.NFSe.RecepcionarLoteRpsSincrono(conteudoXML, configuracao);
                    recepcionarLoteRpsSincrono.Executar();
                    vStrXmlRetorno = recepcionarLoteRpsSincrono.RetornoWSString;

                    recepcionarLoteRpsSincrono.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeEnvioLoteRps:
                    var envioLoteRps = new Unimake.Business.DFe.Servicos.NFSe.EnvioLoteRps(conteudoXML, configuracao);
                    envioLoteRps.Executar();
                    vStrXmlRetorno = envioLoteRps.RetornoWSString;

                    envioLoteRps.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeEnvioRps:
                    var envioRps = new Unimake.Business.DFe.Servicos.NFSe.EnvioRps(conteudoXML, configuracao);
                    envioRps.Executar();
                    vStrXmlRetorno = envioRps.RetornoWSString;

                    envioRps.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeTesteEnvioLoteRps:
                    var testeEnvioLoteRps = new Unimake.Business.DFe.Servicos.NFSe.TesteEnvioLoteRps(conteudoXML, configuracao);
                    testeEnvioLoteRps.Executar();
                    vStrXmlRetorno = testeEnvioLoteRps.RetornoWSString;
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeEmissaoNota:
                    var emissaoNota = new Unimake.Business.DFe.Servicos.NFSe.EmissaoNota(conteudoXML, configuracao);
                    emissaoNota.Executar();
                    vStrXmlRetorno = emissaoNota.RetornoWSString;

                    emissaoNota.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeEnviarLoteNotas:
                    var enviarLoteNotas = new Unimake.Business.DFe.Servicos.NFSe.EnviarLoteNotas(conteudoXML, configuracao);
                    enviarLoteNotas.Executar();
                    vStrXmlRetorno = enviarLoteNotas.RetornoWSString;

                    enviarLoteNotas.Dispose();
                    break;
            }


            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }


    }
}
