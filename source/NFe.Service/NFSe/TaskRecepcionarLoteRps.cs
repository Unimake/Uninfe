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
                        TFunctions.ExecutaUniDanfe(pathFile, dhEmi, Empresas.Configuracoes[emp]);
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
            var servico = DefinirServico(municipio, conteudoXML, padraoNFSe, emp);
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Configuracao
            {
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
            else if (padraoNFSe == PadraoNFSe.SOFTPLAN)
            {
                configuracao.ClientID = Empresas.Configuracoes[emp].ClientID;
                configuracao.ClientSecret = Empresas.Configuracoes[emp].ClientSecret;

                if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].TokenNFse))
                {
                    configuracao.MunicipioToken = Empresas.Configuracoes[emp].TokenNFse;
                    configuracao.MunicipioTokenValidade = Empresas.Configuracoes[emp].TokenNFSeExpire;
                }
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

            if (padraoNFSe == PadraoNFSe.SOFTPLAN)
            {
                var tokenGeradoUniNFe = Empresas.Configuracoes[emp].TokenNFse;
                var tokenGeradoDLL = configuracao.MunicipioToken.Replace("Bearer ", "");

                if (tokenGeradoUniNFe != tokenGeradoDLL)
                {
                    Empresas.Configuracoes[emp].SalvarConfiguracoesNFSeSoftplan(configuracao.MunicipioUsuario,
                                                                                configuracao.MunicipioSenha,
                                                                                configuracao.ClientID,
                                                                                configuracao.ClientSecret,
                                                                                Empresas.Configuracoes[emp].CNPJ,
                                                                                configuracao.MunicipioTokenValidade,
                                                                                tokenGeradoDLL);
                }
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

        /// <summary>
        /// Define qual o tipo de serviço de envio de NFSe será utilizado. Envio em lote sincrono, Envio em lote assincrono ou envio de uma única NFSe síncrono.
        /// </summary>
        /// <param name="municipio">Código do município para onde será enviado o XML</param>
        /// <param name="doc">Conteúdo do XML da NFSe</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        /// <param name="emp">Empresa que está enviando o XML</param>
        /// <returns>Retorna o tipo de serviço de envio da NFSe da prefeitura será utilizado</returns>
        private Unimake.Business.DFe.Servicos.Servico DefinirServico(int municipio, XmlDocument doc, PadraoNFSe padraoNFSe, int emp)
        {
            var result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;

            switch (padraoNFSe)
            {
                case PadraoNFSe.NOTAINTELIGENTE:
                case PadraoNFSe.BETHA:
                case PadraoNFSe.PRODATA:
                case PadraoNFSe.AVMB:
                case PadraoNFSe.WEBISS:
                case PadraoNFSe.COPLAN:
                case PadraoNFSe.PROPRIOJOINVILLESC:
                case PadraoNFSe.SIMPLISS:
                case PadraoNFSe.SONNER:
                case PadraoNFSe.EL:
                case PadraoNFSe.SMARAPD:
                case PadraoNFSe.BHISS:
                case PadraoNFSe.TRIBUTUS:
                case PadraoNFSe.DSF:
                case PadraoNFSe.DIGIFRED:
                case PadraoNFSe.VERSATEC:
                case PadraoNFSe.QUASAR:
                case PadraoNFSe.ISSNET:
                case PadraoNFSe.ABASE:
                case PadraoNFSe.FIORILLI:
                case PadraoNFSe.EMBRAS:
                case PadraoNFSe.CARIOCA:
                case PadraoNFSe.PUBLICA:
                case PadraoNFSe.GISSONLINE:
                case PadraoNFSe.TIPLAN:
                case PadraoNFSe.PRODEB:
                case PadraoNFSe.PORTAL_FACIL:
                case PadraoNFSe.ELOTECH:
                case PadraoNFSe.E_RECEITA:
                case PadraoNFSe.ABACO:
                case PadraoNFSe.FINTEL:
                case PadraoNFSe.SH3:
                case PadraoNFSe.MODERNIZACAO_PUBLICA:
                case PadraoNFSe.INDAIATUBA_SP:
                case PadraoNFSe.BETHA_CLOUD:
                case PadraoNFSe.FUTURIZE:
                    switch (doc.DocumentElement.Name)
                    {
                        case "EnviarLoteRpsSincronoEnvio":
                        case "EnviarLoteDpsSincronoEnvio":
                        case "ns1:ReqEnvioLoteRPS":
                            if (municipio == 2111300)
                            {
                                result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;
                                break;
                            }

                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRpsSincrono;
                            break;
                        case "EnviarLoteRpsEnvio":
                        case "EnviarLoteDpsEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;
                            break;
                        case "GerarNfseEnvio":
                        case "GerarNovaNfseEnvio":
                        case "DPS":
                        case "e:RecepcionarDpsEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                            break;

                        case "NFSe":
                            if (padraoNFSe == PadraoNFSe.SMARAPD)
                            {
                                result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                            }
                            break;
                    }
                    break;
                case PadraoNFSe.SIGCORP:
                case PadraoNFSe.SYSTEMPRO:
                case PadraoNFSe.FISCO:
                case PadraoNFSe.DESENVOLVECIDADE:
                case PadraoNFSe.VITORIA_ES:
                    switch (doc.DocumentElement.Name)
                    {
                        case "EnviarLoteRpsSincronoEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRpsSincrono;
                            break;
                        case "EnviarLoteRpsEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;
                            break;
                        case "GerarNfseEnvio":
                        case "GerarNota":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                            break;
                    }
                    break;

                case PadraoNFSe.GIAP:
                case PadraoNFSe.PROPRIOGOIANIA:
                case PadraoNFSe.BAUHAUS:
                case PadraoNFSe.NACIONAL:
                case PadraoNFSe.BSITBR:
                case PadraoNFSe.PROPRIOBARUERISP:
                case PadraoNFSe.CENTI:
                case PadraoNFSe.AGILI:
                case PadraoNFSe.IIBRASIL:
                case PadraoNFSe.MEGASOFT:
                case PadraoNFSe.SINSOFT:
                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SOFTPLAN:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                    break;

                case PadraoNFSe.IPM:
                case PadraoNFSe.ADM_SISTEMAS:
                case PadraoNFSe.RLZ_INFORMATICA:
                    switch (doc.DocumentElement.Name)
                    {
                        case "DPS":
                        case "GerarNfseEnvio":
                        case "nfse":
                        case "nota":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                            break;

                        case "EnviarLoteRpsEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;
                            break;

                        case "EnviarLoteRpsSincronoEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRpsSincrono;
                            break;
                    }
                    break;

                case PadraoNFSe.GIF:
                    if (doc.DocumentElement.Name == "DPS")
                    {
                        result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                        break;
                    }
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeEnviarLoteNotas;
                    break;

                case PadraoNFSe.PAULISTANA:
                    switch (doc.DocumentElement.Name)
                    {
                        case "PedidoEnvioLoteRPS":

                            if (Empresas.Configuracoes[Empresas.FindEmpresaByThread()].AmbienteCodigo == (int)TipoAmbiente.Homologacao)
                            {
                                result = Unimake.Business.DFe.Servicos.Servico.NFSeTesteEnvioLoteRps;
                            }
                            else
                            {
                                result = Unimake.Business.DFe.Servicos.Servico.NFSeEnvioLoteRps;
                            }
                            break;

                        case "PedidoEnvioRPS":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeEnvioRps;
                            break;
                    }
                    break;

                case PadraoNFSe.OBARATEC:
                case PadraoNFSe.PRIMAX:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeEmissaoNota;
                    break;

                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.WEBFISCO:
                case PadraoNFSe.JLSOFT:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRpsSincrono;
                    break;

                case PadraoNFSe.PRONIM:
                case PadraoNFSe.SUPERNOVA:
                    switch (doc.DocumentElement.Name)
                    {
                        case "EnviarLoteRpsEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;
                            break;

                        case "GerarNfseEnvio":
                        case "DPS":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                            break;
                    }
                    break;

                case PadraoNFSe.EGOVERNEISS:
                    switch (doc.DocumentElement.Name)
                    {
                        case "EmissaoNotaFiscalLoteRequest":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeEnvioLoteRps;
                            break;

                        case "EmissaoNotaFiscalRequest":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeEnvioRps;
                            break;
                    }
                    break;

                case PadraoNFSe.PUBLICENTER:
                    switch (doc.DocumentElement.Name)
                    {
                        case "tcGrcNFSe":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeGerarNfse;
                            break;

                        case "tcLoteRps":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;
                            break;
                    }
                    break;

                case PadraoNFSe.THEMA:
                    if (Empresas.Configuracoes[emp].RpsSincAssincTHEMA)
                    {
                        result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRpsSincrono;
                    }
                    else
                    {
                        result = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarLoteRps;
                    }

                    break;
            }

            return result;
        }

        /// <summary>
        /// Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município
        /// </summary>
        /// <param name="codMunicipio">Código do município para onde será enviado o XML</param>
        /// <param name="xmlDoc">Conteúdo do XML da NFSe</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        /// <returns>Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município</returns>
        private string DefinirVersaoXML(int codMunicipio, XmlDocument xmlDoc, PadraoNFSe padraoNFSe)
        {
            var versaoXML = "0.00";

            switch (padraoNFSe)
            {
                case PadraoNFSe.NACIONAL:
                    versaoXML = (xmlDoc.GetElementsByTagName(xmlDoc.DocumentElement.Name)[0]).Attributes.GetNamedItem("versao").Value;
                    break;

                case PadraoNFSe.NOBESISTEMAS:
                case PadraoNFSe.BHISS:
                case PadraoNFSe.OBARATEC:
                case PadraoNFSe.EQUIPLANO:
                case PadraoNFSe.MEMORY:
                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.BAUHAUS:
                case PadraoNFSe.SIMPLE:
                case PadraoNFSe.PROPRIOBARUERISP:
                case PadraoNFSe.THEMA:
                case PadraoNFSe.WEBFISCO:
                case PadraoNFSe.AGILI:
                case PadraoNFSe.CARIOCA:
                case PadraoNFSe.SALVADOR_BA:
                case PadraoNFSe.MANAUS_AM:
                case PadraoNFSe.LIBRE:
                case PadraoNFSe.HM2SOLUCOES:
                case PadraoNFSe.EGOVERNE:
                case PadraoNFSe.CECAM:
                case PadraoNFSe.METROPOLIS:
                case PadraoNFSe.ISSONLINE_ASSESSORPUBLICO:
                case PadraoNFSe.EGOVERNEISS:
                case PadraoNFSe.INTERSOL:
                case PadraoNFSe.PUBLICENTER:
                case PadraoNFSe.LEXSOM:
                    versaoXML = "1.00";
                    break;

                case PadraoNFSe.NATALENSE:
                    versaoXML = "2.00";
                    break;

                case PadraoNFSe.PAULISTANA:
                    versaoXML = "1.00";
                    if (xmlDoc.InnerXml.Contains("Versao=\"2\"") || xmlDoc.InnerXml.Contains("Versao=\"2.00\""))
                    {
                        versaoXML = "2.00";
                    }
                    break;

                case PadraoNFSe.PRIMAX:
                    if (xmlDoc.InnerXml.Contains("versao=\"1.01\""))
                    {
                        versaoXML = "1.01";
                        break;
                    }
                    else
                    {
                        versaoXML = "1.00";
                        break;
                    }

                case PadraoNFSe.TINUS:
                    versaoXML = "1.00";
                    if (xmlDoc.InnerXml.Contains("versao=\"2.03\"") || xmlDoc.InnerXml.Contains("abrasf"))
                    {
                        versaoXML = "2.03";
                        break;
                    }
                    break;

                case PadraoNFSe.DBSELLER:
                    versaoXML = "1.00";

                    if (codMunicipio == 4319901 || codMunicipio == 4321600)
                    {
                        versaoXML = "2.04";
                    }

                    break;

                case PadraoNFSe.PRODATA:
                    versaoXML = "2.01";
                    break;

                case PadraoNFSe.BETHA:
                    versaoXML = Functions.GetAttributeXML("LoteRps", "versao", NomeArquivoXML);
                    if (string.IsNullOrWhiteSpace(versaoXML))
                    {
                        if (xmlDoc.GetElementsByTagName("GerarNfseEnvio").Count > 0)
                        {
                            versaoXML = "2.02";
                            break;
                        }
                    }
                    if (!versaoXML.Equals("2.02"))
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.BETHA_CLOUD:
                    versaoXML = "1.01";
                    break;

                case PadraoNFSe.IPM:
                    versaoXML = "2.04";

                    if (xmlDoc.InnerXml.Contains("<nfse"))
                    {
                        versaoXML = "1.20";

                        if (codMunicipio == 4309308 || codMunicipio == 4316006 || codMunicipio == 4314050 ||
                            codMunicipio == 4320206)
                        {
                            versaoXML = "1.00";
                        }
                    }
                    break;

                case PadraoNFSe.DIGIFRED:
                case PadraoNFSe.GIAP:
                case PadraoNFSe.BSITBR:
                case PadraoNFSe.CENTI:
                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SOFTPLAN:
                    versaoXML = "2.00";
                    break;

                case PadraoNFSe.SONNER:
                case PadraoNFSe.PROPRIOGOIANIA:
                case PadraoNFSe.ABASE:
                case PadraoNFSe.FIORILLI:
                case PadraoNFSe.SYSTEMPRO:
                case PadraoNFSe.PRODEB:
                case PadraoNFSe.VITORIA_ES:
                case PadraoNFSe.JLSOFT:
                case PadraoNFSe.SINSOFT:
                case PadraoNFSe.SUPERNOVA:
                    versaoXML = "2.01";
                    break;

                case PadraoNFSe.QUASAR:
                    versaoXML = "2.01";
                    if (ConteudoXML.OuterXml.Contains("DPS"))
                    {
                        versaoXML = "1.01";
                        break;
                    }
                    break;

                case PadraoNFSe.NOTAINTELIGENTE:
                case PadraoNFSe.AVMB:
                case PadraoNFSe.WEBISS:
                case PadraoNFSe.VERSATEC:
                case PadraoNFSe.EMBRAS:
                case PadraoNFSe.PORTAL_FACIL:
                case PadraoNFSe.E_RECEITA:
                case PadraoNFSe.SH3:
                case PadraoNFSe.MEGASOFT:
                case PadraoNFSe.FUTURIZE:
                    versaoXML = "2.02";
                    break;

                case PadraoNFSe.ELOTECH:
                    versaoXML = "2.03";
                    break;

                case PadraoNFSe.FINTEL:
                    versaoXML = "2.02";

                    if (codMunicipio == 4115200)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.SIMPLISS:
                    if ((codMunicipio == 3306305 || codMunicipio == 4202404) && !ConteudoXML.OuterXml.Contains("DPS"))
                    {
                        versaoXML = "2.03";
                        break;
                    }
                    if (ConteudoXML.OuterXml.Contains("DPS"))
                    {
                        versaoXML = "1.01";
                        break;
                    }
                    versaoXML = "3.00";
                    break;

                case PadraoNFSe.COPLAN:
                    versaoXML = "2.03";

                    if (codMunicipio == 3300407)
                    {
                        versaoXML = "2.02";
                    }
                    if (ConteudoXML.InnerXml.Contains("xmlns=\"http://www.sped.fazenda.gov.br/nfse\""))
                    {
                        versaoXML = "1.01";
                    }
                    break;

                case PadraoNFSe.MODERNIZACAO_PUBLICA:
                    versaoXML = "2.02";
                    if (xmlDoc.InnerXml.Contains("versao=\"3.02\""))
                    {
                        versaoXML = "3.02";
                    }
                    break;

                case PadraoNFSe.DSF:
                    versaoXML = "2.03";
                    if ((codMunicipio == 3509502 && ConteudoXML.OuterXml.Contains("ns1:ReqEnvioLoteRPS") || codMunicipio == 5002704 ||
                        codMunicipio == 3303500 || codMunicipio == 2111300))
                    {
                        versaoXML = "1.00";
                    }
                    else if (codMunicipio == 3170206)
                    {
                        versaoXML = "2.04";
                    }
                    else if (ConteudoXML.InnerXml.Contains("versao=\"3.00\""))
                    {
                        versaoXML = "3.00";
                    }

                    break;

                case PadraoNFSe.FISCO:
                case PadraoNFSe.DESENVOLVECIDADE:
                case PadraoNFSe.INDAIATUBA_SP:
                    versaoXML = "2.03";

                    break;

                case PadraoNFSe.ADM_SISTEMAS:
                    versaoXML = "2.03";

                    if (codMunicipio == 1400100)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.RLZ_INFORMATICA:
                    versaoXML = "2.03";

                    if (codMunicipio == 3557105)
                    {
                        versaoXML = "1.00";
                        break;
                    }
                    else if (xmlDoc.OuterXml.Contains("versao=\"1.01\""))
                    {
                        versaoXML = "1.01";
                        break;
                    }
                    break;

                case PadraoNFSe.PRONIM:
                    versaoXML = "2.03";

                    if (codMunicipio == 3113404 || codMunicipio == 4321006 || codMunicipio == 3131703 ||
                        codMunicipio == 4303004 || codMunicipio == 4300109 || codMunicipio == 3143302 ||
                        codMunicipio == 4306932 || codMunicipio == 3302205 || codMunicipio == 3530300)
                    {
                        versaoXML = "2.02";
                    }
                    if (codMunicipio == 3535804 || codMunicipio == 4304507 || codMunicipio == 4321709 ||
                        codMunicipio == 4122404)
                    {
                        versaoXML = "1.00";
                    }
                    if (xmlDoc.OuterXml.Contains(versaoXML = "\"1.01\""))
                    {
                        versaoXML = "1.01";
                    }
                    break;

                case PadraoNFSe.SMARAPD:
                    versaoXML = "2.03";

                    if (codMunicipio == 3551702 || codMunicipio == 3202405)
                    {
                        versaoXML = "1.00";
                    }
                    else if (xmlDoc.OuterXml.Contains("infDPS"))
                    {
                        versaoXML = "1.01";
                    }
                    else if (codMunicipio == 3205002 || codMunicipio == 3516200)
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.PROPRIOJOINVILLESC:
                case PadraoNFSe.TRIBUTUS:
                case PadraoNFSe.IIBRASIL:
                    versaoXML = "2.04";
                    break;

                case PadraoNFSe.ISSNET:
                    versaoXML = "2.04";
                    if (ConteudoXML.OuterXml.Contains("versao=\"1.01\""))
                    {
                        versaoXML = "1.01";
                    }
                    break;

                case PadraoNFSe.GISSONLINE:
                    versaoXML = "2.04";
                    if (ConteudoXML.InnerXml.Contains("versao=\"2.05\""))
                    {
                        versaoXML = "2.05";
                    }
                    break;

                case PadraoNFSe.ABACO:
                    versaoXML = "2.04";

                    if (codMunicipio == 5108402)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.GINFES:
                    versaoXML = "3.00";

                    if (xmlDoc.InnerXml.Contains("xmlns:ns1"))
                    {
                        versaoXML = "3.01";
                    }

                    if (xmlDoc.InnerXml.Contains("versao=\"4.00\""))
                    {
                        versaoXML = "4.00";
                    }
                    break;

                case PadraoNFSe.PUBLICA:
                    versaoXML = "3.00";
                    break;

                case PadraoNFSe.SIGCORP:
                    if (xmlDoc.DocumentElement.Name.Contains("GerarNota"))
                    {
                        if (codMunicipio == 4113700)
                        {
                            versaoXML = "1.03";
                        }
                        else
                        {
                            versaoXML = "3.00";
                        }
                    }
                    else if (!xmlDoc.DocumentElement.Name.Contains("GerarNota"))
                    {
                        if (codMunicipio == 4204202 || codMunicipio == 3131307 ||
                            codMunicipio == 3530805 || codMunicipio == 3145208 ||
                            codMunicipio == 3300704)
                        {
                            versaoXML = "2.04";
                        }
                        else
                        {
                            versaoXML = "2.03";
                        }
                    }
                    break;

                case PadraoNFSe.EL:
                    versaoXML = "2.04";

                    if (codMunicipio == 3201506 || codMunicipio == 3204203)
                    {
                        versaoXML = "1.00";
                    }
                    else if (xmlDoc.OuterXml.Contains("infDPS"))
                    {
                        versaoXML = "1.01";
                    }
                    break;

                case PadraoNFSe.TIPLAN:
                    versaoXML = "2.03";

                    if (codMunicipio == 2611606)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.GIF:
                    if (ConteudoXML.OuterXml.Contains("DPS"))
                    {
                        versaoXML = "1.01";
                        break;
                    }
                    versaoXML = "1.00";
                    break;

                case PadraoNFSe.CONAM:
                    versaoXML = "2.00";

                    if (codMunicipio == 3506102 || codMunicipio == 3509007)
                    {
                        versaoXML = "4.00";
                    }

                    break;

                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Enivo / Gerar NFS-e.");
            }

            return versaoXML;
        }
    }
}