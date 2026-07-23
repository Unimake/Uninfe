using NFe.Components;
using NFe.Exceptions;
using NFe.Settings;
using System;
using System.IO;
using System.Threading;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFe;
using Unimake.Exceptions;

namespace NFe.Service
{
    public class TaskNFeRecepcao : TaskAbst
    {
        public TaskNFeRecepcao(string arquivo)
        {
            Servico = Servicos.NFeEnviarLote;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public TaskNFeRecepcao(XmlDocument conteudoXML)
        {
            Servico = Servicos.NFeEnviarLote;

            ConteudoXML = conteudoXML;
            ConteudoXML.PreserveWhitespace = false;
            NomeArquivoXML = Empresas.Configuracoes[Empresas.FindEmpresaByThread()].PastaXmlEnvio + "\\temp\\" +
                conteudoXML.GetElementsByTagName(TpcnResources.idLote.ToString())[0].InnerText + Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML;
        }

        #region Classe com os dados do XML do retorno do envio do Lote de NFe

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do recibo do lote
        /// </summary>
        private DadosRecClass dadosRec;

        #endregion Classe com os dados do XML do retorno do envio do Lote de NFe

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var oFluxoNfe = new FluxoNfe();
            var ler = new LerXML();
            Configuracao configuracao = null;

            try
            {
                dadosRec = new DadosRecClass();

                //Ler o XML de Lote para pegar o número do lote que está sendo enviado
                ler.Nfe(ConteudoXML);

                var idLote = ler.oDadosNfe.idLote;

                var xmlNFe = new EnviNFe();
                xmlNFe = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<EnviNFe>(ConteudoXML);

                //remover assinatura gerada pelo ERP para que o UNINFE assine novamente.
                for (var i = 0; i < xmlNFe.NFe.Count; i++)
                {
                    xmlNFe.NFe[i].Signature = null;
                }

                configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = (ler.oDadosNfe.mod == "65" ? TipoDFe.NFCe : TipoDFe.NFe),
                    TipoEmissao = (Unimake.Business.DFe.Servicos.TipoEmissao)(Convert.ToInt32(ler.oDadosNfe.tpEmis)),
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    ColetarTelemetriaDisponibilidade = true
                };

                if (ConfiguracaoApp.Proxy)
                {
                    configuracao.HasProxy = true;
                    configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                    configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                    configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                }

                EnviNFe EnviNFe = null;

                var cStat = 0;
                var xMotivo = string.Empty;

                if (ler.oDadosNfe.mod == "65")
                {
                    // Se na configuração foi informado o número 2, vai configurar para o QrCode versão 2
                    // VersaoQRCodeNFCe da DLL, por padrão, é 3
                    if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe == 2)
                    {
                        configuracao.VersaoQRCodeNFCe = 2;
                    }

                    if (ConteudoXML.GetElementsByTagName("qrCode").Count == 0 && Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                    {
                        if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].IdentificadorCSC.Trim()) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC))
                        {
                            throw new Exception("Para autorizar NFC-e é obrigatório informar nas configurações do UniNFe os campos CSC e IDToken do CSC.");
                        }
                    }
                    if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                    {
                        configuracao.CSC = Empresas.Configuracoes[emp].IdentificadorCSC;
                        configuracao.CSCIDToken = Convert.ToInt32((string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC) ? "0" : Empresas.Configuracoes[emp].TokenCSC));
                    }

                    var autorizacao = new Unimake.Business.DFe.Servicos.NFCe.Autorizacao(xmlNFe, configuracao);
                    ConteudoXML = autorizacao.ConteudoXMLAssinado;
                    SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: XML assinado salvo em EmProcessamento antes do envio NFCe para preservar recuperacao em caso de falha tecnica.");

                    autorizacao.Executar();

                    ConteudoXML = autorizacao.ConteudoXMLAssinado;

                    vStrXmlRetorno = autorizacao.RetornoWSString;

                    EnviNFe = autorizacao.EnviNFe;

                    cStat = autorizacao.Result.CStat;
                    xMotivo = autorizacao.Result.XMotivo;

                    autorizacao.Dispose();
                }
                else
                {
                    var autorizacao = new Unimake.Business.DFe.Servicos.NFe.Autorizacao(xmlNFe, configuracao);
                    ConteudoXML = autorizacao.ConteudoXMLAssinado;
                    SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: XML assinado salvo em EmProcessamento antes do envio NFe para preservar recuperacao em caso de falha tecnica.");

                    autorizacao.Executar();

                    ConteudoXML = autorizacao.ConteudoXMLAssinado;

                    vStrXmlRetorno = autorizacao.RetornoWSString;

                    EnviNFe = autorizacao.EnviNFe;

                    cStat = autorizacao.Result.CStat;
                    xMotivo = autorizacao.Result.XMotivo;

                    autorizacao.Dispose();
                }

                if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    throw new Exception("A SEFAZ, Receita ou prefeitura está com instabilidade, pois o XML retornado pelo Web-Service não pode ser reconhecido. Conteúdo retornado: " + (vStrXmlRetorno ?? "null"));
                }

                if (ler.oDadosNfe.indSinc || EnviNFe.NFe.Count <= 1)
                {
                    Protocolo(vStrXmlRetorno);

                    Auxiliar.WriteLog("TaskNFeRecepcao: Resultado leitura protocolo. cStat=" + dadosRec.cStat + ", nRec=" + dadosRec.nRec, false);
                }
                else
                {
                    Recibo(vStrXmlRetorno, emp);

                    oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.ExtRetorno.Rec, vStrXmlRetorno);
                }

                #region Parte que trata o retorno do lote, ou seja, o número do recibo ou protocolo

                if (dadosRec.cStat == "104" || dadosRec.cStat == "100" || dadosRec.cStat == "120") //Tem estado que retorna como 100 ou 120? Autorizado neste ponto? Não deveria, mas vai que.
                {
                    FinalizarNFeSincrono(vStrXmlRetorno, emp, ler.oDadosNfe.chavenfe);

                    oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                }
                else if (dadosRec.cStat == "103") //Lote recebido com sucesso - Processo da NFe Assíncrono
                {
                    if (dadosRec.tMed > 0)
                    {
                        Thread.Sleep(dadosRec.tMed * 1000);
                    }

                    try
                    {
                        var xmlPedRec = oGerarXML.XmlPedRecNFe(dadosRec.nRec, ler.oDadosNfe.versao, ler.oDadosNfe.mod, emp);

                        var nfeRetRecepcao = new TaskNFeRetRecepcao(xmlPedRec)
                        {
                            chNFe = ler.oDadosNfe.chavenfe,
                            EnviNFe = EnviNFe
                        };

                        nfeRetRecepcao.Execute();
                    }
                    catch (ExceptionEnvioXML)
                    {
                        throw;
                    }
                    catch (ExceptionSemInternet)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        //Atualizar o número do recibo no XML de controle do fluxo de notas enviadas
                        oFluxoNfe.AtualizarTag(ler.oDadosNfe.chavenfe, FluxoNfe.ElementoEditavel.tMed, dadosRec.tMed.ToString());
                        oFluxoNfe.AtualizarTagRec(idLote, dadosRec.nRec);
                    }
                }
                else if (Convert.ToInt32(dadosRec.cStat) > 200 ||
                    Convert.ToInt32(dadosRec.cStat) == 108 || //Verifica se o servidor de processamento está paralisado momentaneamente. Wandrey 13/04/2012
                    Convert.ToInt32(dadosRec.cStat) == 109 || //Verifica se o servidor de processamento está paralisado sem previsão. Wandrey 13/04/2012
                    Convert.ToInt32(dadosRec.cStat) == 114)   //SVC Desativado para o estado em questão.
                {
                    if (ler.oDadosNfe.indSinc)
                    {
                        // OPS!!! Processo sincrono rejeição da SEFAZ, temos que gravar o XML para o ERP, pois no processo síncrono isso não pode ser feito dentro do método Invocar
                        oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                    }

                    //Se o status do retorno do lote for maior que 200 ou for igual a 108 ou 109,
                    //vamos ter que excluir a nota do fluxo, porque ela foi rejeitada pelo SEFAZ
                    //Primeiro vamos mover o xml da nota da pasta EmProcessamento para pasta de XML´s com erro e depois a tira do fluxo
                    //Wandrey 30/04/2009
                    oAux.MoveArqErro(Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + oFluxoNfe.LerTag(ler.oDadosNfe.chavenfe, FluxoNfe.ElementoFixo.ArqNFe));
                    oFluxoNfe.ExcluirNfeFluxo(ler.oDadosNfe.chavenfe);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + cStat.ToString("000") + "-" + xMotivo.Trim(), "UNINFE - Notas estão sendo rejeitadas");
                    }
                }

                #endregion

                //Deleta o arquivo de lote
                Functions.DeletarArquivo(NomeArquivoXML);
            }
            catch (ExceptionEnvioXML ex)
            {
                TrataException(ex, ler.oDadosNfe, emp);
            }
            catch (ExceptionSemInternet ex)
            {
                TrataException(ex, ler.oDadosNfe, emp);
            }
            catch (ValidarXMLException ex)
            {
                SalvarArquivoErroValidacao(emp, ex);
            }
            catch (Exception ex)
            {
                TrataException(ex, ler.oDadosNfe, emp);
            }
            finally
            {
                DiagnosticoDisponibilidadeDFeHelper.Gravar(emp, configuracao, NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML);
            }
        }

        /// <summary>
        /// Gravar arquivo de erro na pasta retorno para o ERP e mover o arquivo do XML da nota para pasta com erro.
        /// </summary>
        /// <param name="emp">Codigo da empresa</param>
        /// <param name="exception">Exceção gerada</param>
        private void SalvarArquivoErroValidacao(int emp, ValidarXMLException exception)
        {
            try
            {
                var nodeListNFe = ConteudoXML.GetElementsByTagName("NFe");

                foreach (var nodeNFe in nodeListNFe)
                {
                    var xmlElementNFe = (XmlElement)nodeNFe;
                    var chaveNFe = ((XmlElement)xmlElementNFe.GetElementsByTagName("infNFe")[0]).GetAttribute("Id");

                    var fluxoNFe = new FluxoNfe();
                    var nomeArqNFe = fluxoNFe.LerTag(chaveNFe, FluxoNfe.ElementoFixo.ArqNFe);
                    var arqXMLNFe = Empresas.Configuracoes[emp].PastaXmlEnvio + "\\temp\\" + nomeArqNFe;

                    TFunctions.GravarArqErroServico(arqXMLNFe, Propriedade.ExtEnvio.NFe, Propriedade.ExtRetorno.Nfe_ERR, exception, ErroPadrao.ValidarXML, true);
                }
            }
            catch
            {
                //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                //Wandrey 16/03/2010
            }
        }

        #endregion Execute

        /// <summary>
        /// Tratar exceção
        /// </summary>
        /// <param name="ex">Objeto com a exception</param>
        /// <param name="dadosNFe">Dados da NFe/NFCe</param>
        private void TrataException(Exception ex, DadosNFeClass dadosNFe, int emp)
        {
            try
            {
                //new FluxoNfe().ExcluirNfeFluxo(dadosNFe.chavenfe);

                if (dadosNFe.indSinc)
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.ExtEnvio.EnvLot, Propriedade.ExtRetorno.ProRec_ERR, ex, ErroPadrao.ErroNaoDetectado, false);
                }
                else
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.ExtRetorno.Rec_ERR, ex, ErroPadrao.ErroNaoDetectado, false);
                }

                PreservarArquivoParaRecuperacao(emp, dadosNFe, ex);
            }
            catch
            {
                //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                //Wandrey 16/03/2010
            }
        }

        #region Recibo

        /// <summary>
        /// Faz a leitura do XML do Recibo do lote enviado e disponibiliza os valores
        /// de algumas tag´s
        /// </summary>
        /// <param name="strXml">String contendo o XML</param>
        private void Recibo(string strXml, int emp)
        {
            dadosRec.cStat =
                dadosRec.nRec = string.Empty;
            dadosRec.tMed = 0;

            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(strXml));

            var retEnviNFeList = xml.GetElementsByTagName("retEnviNFe");

            foreach (XmlNode retEnviNFeNode in retEnviNFeList)
            {
                var retEnviNFeElemento = (XmlElement)retEnviNFeNode;

                dadosRec.cStat = retEnviNFeElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;

                var infRecList = xml.GetElementsByTagName("infRec");

                foreach (XmlNode infRecNode in infRecList)
                {
                    var infRecElemento = (XmlElement)infRecNode;

                    dadosRec.nRec = infRecElemento.GetElementsByTagName(TpcnResources.nRec.ToString())[0].InnerText;
                    dadosRec.tMed = Convert.ToInt32(infRecElemento.GetElementsByTagName(TpcnResources.tMed.ToString())[0].InnerText);

                    if (dadosRec.tMed > 15)
                    {
                        dadosRec.tMed = 15;
                    }

                    if (dadosRec.tMed <= 0)
                    {
                        dadosRec.tMed = Empresas.Configuracoes[emp].TempoConsulta;
                    }
                }
            }
        }

        #endregion Recibo

        #region Protocolo()

        /// <summary>
        /// Faz leitura do protocolo quando configurado para processo Síncrono
        /// </summary>
        /// <param name="strXml">String contendo o XML</param>
        private void Protocolo(string strXml)
        {
            dadosRec.cStat =
                dadosRec.nRec = string.Empty;
            dadosRec.tMed = 0;

            try
            {
                var xml = new XmlDocument();
                xml.Load(Functions.StringXmlToStream(strXml));

                var nomeTagRetorno = xml.DocumentElement != null ? xml.DocumentElement.Name : "retEnviNFe";
                var retEnviNFeList = xml.GetElementsByTagName(nomeTagRetorno);

                foreach (XmlNode retEnviNFeNode in retEnviNFeList)
                {
                    var retEnviNFeElemento = (XmlElement)retEnviNFeNode;

                    if (retEnviNFeElemento.GetElementsByTagName(TpcnResources.cStat.ToString()).Count > 0)
                    {
                        dadosRec.cStat = retEnviNFeElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;
                    }

                    if (retEnviNFeElemento.GetElementsByTagName(TpcnResources.nRec.ToString()).Count > 0)
                    {
                        dadosRec.nRec = retEnviNFeElemento.GetElementsByTagName(TpcnResources.nRec.ToString())[0].InnerText;
                    }
                }

                if (string.IsNullOrWhiteSpace(dadosRec.cStat))
                {
                    Auxiliar.WriteLog("TaskNFeRecepcao.Protocolo: Não foi possível identificar a tag cStat no retorno do envio síncrono.", true);
                }
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao.Protocolo: Erro ao ler retorno do protocolo. " + ex.Message, true);
                throw;
            }
        }

        #endregion Protocolo()

        #region FinalizarNFeSincrono()

        /// <summary>
        /// Finalizar a NFe no processo Síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado da SEFAZ</param>
        /// <param name="emp">Código da empresa para buscar as configurações</param>
        private void FinalizarNFeSincrono(string xmlRetorno, int emp, string chNFe)
        {
            try
            {
                var xml = new XmlDocument();
                xml.Load(Functions.StringXmlToStream(xmlRetorno));

                var protNFe = xml.GetElementsByTagName("protNFe");

                if (protNFe == null || protNFe.Count == 0)
                {
                    Auxiliar.WriteLog("TaskNFeRecepcao.FinalizarNFeSincrono: XML de retorno sem tag protNFe. Chave=" + chNFe, true);
                    throw new Exception("Não foi possível localizar a tag protNFe no retorno do envio síncrono.");
                }

                var fluxoNFe = new FluxoNfe();

                var retRecepcao = new TaskNFeRetRecepcao
                {
                    chNFe = chNFe
                };

                retRecepcao.FinalizarNFe(protNFe, fluxoNFe, emp, ConteudoXML);
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao.FinalizarNFeSincrono: Erro ao finalizar NFe síncrona. " + ex.Message, true);
                throw;
            }
        }

        #endregion FinalizarNFeSincrono()

        /// <summary>
        /// Salvar o arquivo do NFe assinado na pasta EmProcessamento
        /// </summary>
        /// <param name="emp">Codigo da empresa</param>
        private void SalvarArquivoEmProcessamento(int emp) => SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: XML assinado salvo em EmProcessamento.");

        /// <summary>
        /// Salvar o arquivo do NFe assinado na pasta EmProcessamento com log de diagnostico.
        /// </summary>
        /// <param name="emp">Codigo da empresa</param>
        /// <param name="mensagemLog">Mensagem de log para acompanhamento</param>
        private void SalvarArquivoEmProcessamento(int emp, string mensagemLog)
        {
            var msgLog = "";
            try
            {
                Empresas.Configuracoes[emp].CriarSubPastaEnviado();

                var nodeListNFe = ConteudoXML.GetElementsByTagName("NFe");

                foreach (var nodeNFe in nodeListNFe)
                {
                    var xmlElementNFe = (XmlElement)nodeNFe;
                    var chaveNFe = ((XmlElement)xmlElementNFe.GetElementsByTagName("infNFe")[0]).GetAttribute("Id");

                    var fluxoNFe = new FluxoNfe();
                    var nomeArqNFe = fluxoNFe.LerTag(chaveNFe, FluxoNfe.ElementoFixo.ArqNFe);

                    //Se não encontrar o nome do arquivo da NFe no FluxoNFe.XML, vou tentar pegar o nome do arquivo pelos XMLs que estão na pasta TEMP
                    if (string.IsNullOrWhiteSpace(nomeArqNFe))
                    {
                        nomeArqNFe = NomeArquivoXMLTemp(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnvio, "temp"), chaveNFe, "NFe", "infNFe");

                        if (string.IsNullOrWhiteSpace(nomeArqNFe))
                        {
                            nomeArqNFe = NomeArquivoXMLTemp(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEmLote, "temp"), chaveNFe, "NFe", "infNFe");
                        }
                    }

                    var arqEmProcessamento = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado, PastaEnviados.EmProcessamento.ToString(), nomeArqNFe);

                    msgLog += "\r\n arqEmProcessamento = " + arqEmProcessamento;
                    msgLog += "\r\n nomeArqNFe = " + nomeArqNFe;
                    msgLog += "\r\n chaveNFe = " + chaveNFe;
                    msgLog += "\r\n NomeArqXML = " + NomeArquivoXML;

                    var sw = File.CreateText(arqEmProcessamento);
                    sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xmlElementNFe.OuterXml);
                    sw.Close();

                    if (File.Exists(arqEmProcessamento))
                    {
                        Auxiliar.WriteLog(mensagemLog + " Chave=" + chaveNFe + ", arquivo=" + arqEmProcessamento, false);
                        File.Delete(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnvio, "temp", nomeArqNFe));
                        File.Delete(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEmLote, "temp", nomeArqNFe));
                    }
                }
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog(ex.Message + "\r\n" + msgLog, true);
                throw (ex);
            }
        }

        /// <summary>
        /// Em caso de erro move o arquivo, se ainda estiver na pasta temp, para a pasta de erro
        /// </summary>
        /// <param name="emp">Empresa</param>
        private void MoverArquivoErroTemp(int emp)
        {
            var msgLog = "";
            try
            {
                Empresas.Configuracoes[emp].CriarSubPastaEnviado();

                var nodeListNFe = ConteudoXML.GetElementsByTagName("NFe");

                foreach (var nodeNFe in nodeListNFe)
                {
                    var xmlElementNFe = (XmlElement)nodeNFe;
                    var chaveNFe = ((XmlElement)xmlElementNFe.GetElementsByTagName("infNFe")[0]).GetAttribute("Id");

                    var fluxoNFe = new FluxoNfe();
                    var nomeArqNFe = fluxoNFe.LerTag(chaveNFe, FluxoNfe.ElementoFixo.ArqNFe);

                    //Se não encontrar o nome do arquivo da NFe no FluxoNFe.XML, vou tentar pegar o nome do arquivo pelos XMLs que estão na pasta TEMP
                    if (string.IsNullOrWhiteSpace(nomeArqNFe))
                    {
                        nomeArqNFe = NomeArquivoXMLTemp(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnvio, "temp"), chaveNFe, "NFe", "infNFe");

                        if (string.IsNullOrWhiteSpace(nomeArqNFe))
                        {
                            nomeArqNFe = NomeArquivoXMLTemp(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEmLote, "temp"), chaveNFe, "NFe", "infNFe");
                        }
                    }

                    var caminho = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnvio, "temp", nomeArqNFe);
                    TFunctions.MoveArqErro(caminho);
                }
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog(ex.Message + "\r\n" + msgLog, true);
                throw (ex);
            }
        }
        /// <summary>
        /// Preservar a NFe em EmProcessamento quando ocorrer falha tecnica sem retorno fiscal conclusivo.
        /// </summary>
        /// <param name="emp">Empresa</param>
        /// <param name="dadosNFe">Dados da NFe</param>
        /// <param name="ex">Excecao ocorrida</param>
        private void PreservarArquivoParaRecuperacao(int emp, DadosNFeClass dadosNFe, Exception ex)
        {
            try
            {
                SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: Falha tecnica sem retorno fiscal conclusivo. XML mantido em EmProcessamento para recuperacao via consulta situacao. Erro=" + ex.GetAllMessages());
            }
            catch (Exception salvarEx)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao: Falha tecnica sem retorno fiscal conclusivo, mas nao foi possivel confirmar/salvar XML em EmProcessamento. Chave=" + dadosNFe.chavenfe + ", erroOriginal=" + ex.GetAllMessages() + ", erroPreservacao=" + salvarEx.GetAllMessages(), true);
            }
        }

    }
}

