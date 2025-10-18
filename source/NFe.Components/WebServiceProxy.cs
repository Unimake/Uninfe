using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Components
{
    public class WebServiceProxy
    {
        #region Propriedades

        /// <summary>
        /// Lista utilizada para armazenar os webservices
        /// </summary>
        public static List<webServices> webServicesList { get; private set; }

        #endregion Propriedades

        #region CarregaWebServicesList()

        /// <summary>
        /// Carrega a lista de webservices definidos no arquivo WebService.XML
        /// </summary>
        public static void CarregaWebServicesList()
        {
            if (webServicesList == null)
            {
                webServicesList = new List<webServices>();
                Propriedade.Municipios = new List<Municipio>();

                var doc = new XmlDocument();
                var config = new Configuracao();
                var stream = config.LoadXmlConfig(Unimake.Business.DFe.Configuration.ArquivoConfigGeral);

                doc.Load(stream);

                var arquivoList = doc.GetElementsByTagName("Arquivo");

                foreach (XmlNode arquivoNode in arquivoList)
                {
                    var elemento = (XmlElement)arquivoNode;
                    if (elemento.GetAttribute("ID").Length >= 3)
                    {
                        int id = Convert.ToInt32(elemento.GetAttribute("ID"));
                        string nome = elemento.GetElementsByTagName("Nome")[0].InnerText;
                        string uf = elemento.GetElementsByTagName("UF")[0].InnerText;
                        PadraoNFSe padrao = PadraoNFSe.None;
                        string padraoStr = elemento.GetElementsByTagName("PadraoNFSe")[0].InnerText;
                        padrao = (PadraoNFSe)Enum.Parse(typeof(PadraoNFSe), padraoStr, true);

                        Propriedade.Municipios.Add(new Municipio(id, uf, nome, padrao));
                    }

                }

                /// danasa 1-2012
                if (File.Exists(Propriedade.NomeArqXMLMunicipios))
                {
                    var contaTentativa = 0;
                    while (contaTentativa <= 2)
                    {
                        try
                        {
                            doc.Load(Propriedade.NomeArqXMLMunicipios);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Functions.WriteLog("Ocorreu um erro na tentativa de carregamento do arquivo " + Propriedade.NomeArqXMLMunicipios + ".\r\n\r\n" +
                                "Erro:\r\n\r\n" + ex.Message, true, true, "");

                            if (contaTentativa++ == 2)
                            {
                                break;
                            }

                            //Forçar recriação do arquivo
                            File.Delete(Propriedade.NomeArqXMLMunicipios);
                            WebServiceNFSe.SalvarXMLMunicipios();
                        }
                    }

                    var estadoList = doc.GetElementsByTagName(NFeStrConstants.Registro);
                }

                /// danasa 1-2012

                var salvaXmlLocal = false;
                LoadArqXMLWebService(Propriedade.NomeArqXMLWebService_NFe, "NFe\\");
                salvaXmlLocal = LoadArqXMLWebService(Propriedade.NomeArqXMLWebService_NFSe, "NFse\\");

                if (salvaXmlLocal)
                {
                    WebServiceNFSe.SalvarXMLMunicipios(null, null, 0, null, false);
                }
            }
        }


        private static bool LoadArqXMLWebService(string filenameWS, string subfolder)
        {
            var salvaXmlLocal = false;

            if (File.Exists(filenameWS))
            {
                var doc = new XmlDocument();

                try
                {
                    doc.Load(filenameWS);
                }
                catch (Exception ex)
                {
                    Functions.WriteLog("Ocorreu um erro na tentativa de carregamento do arquivo " + filenameWS + ".\r\n" +
                        "Acesse novamente o sistema para que se recupere automaticamente do erro.\r\n\r\n" +
                        "Erro:\r\n\r\n" + ex.Message, true, true, "");

                    if (File.Exists(Propriedade.XMLVersaoWSDLXSD))
                    {
                        File.Delete(Propriedade.XMLVersaoWSDLXSD);
                    }

                    Environment.Exit(0);
                }

                var estadoList = doc.GetElementsByTagName(NFeStrConstants.Estado);
                foreach (XmlNode estadoNode in estadoList)
                {
                    var estadoElemento = (XmlElement)estadoNode;
                    if (estadoElemento.Attributes.Count > 0)
                    {
                        if (estadoElemento.Attributes[TpcnResources.UF.ToString()].Value != "XX")
                        {
                            var ID = Convert.ToInt32("0" + Functions.OnlyNumbers(estadoElemento.Attributes[TpcnResources.ID.ToString()].Value));
                            if (ID == 0)
                            {
                                continue;
                            }

                            var Nome = estadoElemento.Attributes[NFeStrConstants.Nome].Value;
                            var UF = estadoElemento.Attributes[TpcnResources.UF.ToString()].Value;

                            ///
                            /// danasa 1-2012
                            ///
                            /// Verifica se o ID já está na lista
                            /// Isto previne que no xml de configuração tenha duplicidade e evita derrubar o programa
                            var ci = (from i in webServicesList where i.ID == ID select i).FirstOrDefault();
                            if (ci == null)
                            {
                                var wsItem = new webServices(ID, Nome, UF);
                                XmlNodeList urlList;

                                urlList = estadoElemento.GetElementsByTagName(NFe.Components.NFeStrConstants.LocalHomologacao);
                                if (urlList.Count > 0)
                                {
                                    PreencheURLw(wsItem.LocalHomologacao,
                                                 NFe.Components.NFeStrConstants.LocalHomologacao,
                                                 urlList.Item(0).OuterXml,
                                                 UF,
                                                 subfolder);
                                }

                                urlList = estadoElemento.GetElementsByTagName(NFe.Components.NFeStrConstants.LocalProducao);
                                if (urlList.Count > 0)
                                {
                                    PreencheURLw(wsItem.LocalProducao,
                                                 NFe.Components.NFeStrConstants.LocalProducao,
                                                 urlList.Item(0).OuterXml,
                                                 UF,
                                                 subfolder);
                                }

                                webServicesList.Add(wsItem);
                            }

                            // danasa 1-2012
                            if (estadoElemento.HasAttribute(NFeStrConstants.Padrao))
                            {
                                try
                                {
                                    var padrao = estadoElemento.Attributes[NFeStrConstants.Padrao].Value;
                                    if (!padrao.Equals(PadraoNFSe.None.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var cc = (from i in Propriedade.Municipios
                                                  where i.CodigoMunicipio == ID
                                                  select i).FirstOrDefault();
                                        if (cc == null)
                                        {
                                            Propriedade.Municipios.Add(new Municipio(ID, UF, Nome, WebServiceNFSe.GetPadraoFromString(padrao)));
                                            salvaXmlLocal = true;
                                        }
                                        else
                                        {
                                            if (!cc.PadraoStr.Equals(padrao) || !cc.UF.Equals(UF) || !cc.Nome.Equals(Nome, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                cc.Padrao = WebServiceNFSe.GetPadraoFromString(padrao);
                                                cc.Nome = Nome;
                                                cc.UF = UF;
                                                salvaXmlLocal = true;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Functions.WriteLog("Falha ao processar o padrão para o município ID " + ID + ". Erro: " + ex.Message, true, true, "");
                                }
                            }

                            // danasa 1-2012
                        }
                    }
                }
            }
            else
            {
                Functions.WriteLog("Ocorreu um erro na tentativa de carregamento do arquivo " + filenameWS + ".\r\n" +
                    "Acesse novamente o sistema para que se recupere automaticamente do erro.", true, true, "");

                if (File.Exists(Propriedade.XMLVersaoWSDLXSD))
                {
                    File.Delete(Propriedade.XMLVersaoWSDLXSD);
                }

                Environment.Exit(0);
            }

            return salvaXmlLocal;
        }

        #endregion CarregaWebServicesList()

        #region PreencheURLw

        private static void PreencheURLw(URLws wsItem, string tagName, string urls, string uf, string subfolder)
        {
            if (urls == "")
            {
                return;
            }

            var appBasePath = Path.Combine(Propriedade.PastaExecutavel, subfolder);
            var doc = new XmlDocument();
            doc.LoadXml(urls);
            var urlList = doc.ChildNodes;
            if (urlList == null)
            {
                return;
            }

            for (var i = 0; i < urlList.Count; ++i)
            {
                for (var j = 0; j < urlList[i].ChildNodes.Count; ++j)
                {
                    var appPath = "";
                    var ClassProperty = wsItem.GetType().GetProperty(urlList[i].ChildNodes[j].Name);
                    if (ClassProperty != null)
                    {
                        appPath = appBasePath + urlList[i].ChildNodes[j].InnerText;

                        if (!string.IsNullOrEmpty(urlList[i].ChildNodes[j].InnerText))
                        {
                            if (urlList[i].ChildNodes[j].InnerText.ToLower().EndsWith("asmx?wsdl"))
                            {
                                appPath = urlList[i].ChildNodes[j].InnerText;
                            }
                            else
                            {
                                if (!File.Exists(appPath))
                                {
                                    appPath = "";
                                }
                            }
                        }
                        else
                        {
                            appPath = "";
                        }

                        ClassProperty.SetValue(wsItem, appPath, null);
                    }

                    if (appPath == "" && !string.IsNullOrEmpty(urlList[i].ChildNodes[j].InnerText))
                    {
                        Console.WriteLine("wsItem <" + urlList[i].ChildNodes[j].InnerText + "> nao encontrada na classe URLws em <" + urlList[i].ChildNodes[j].Name + ">");
                    }
                }
            }
        }

        #endregion PreencheURLw
    }

    public class webServices
    {
        public int ID { get; private set; }

        public string Nome { get; private set; }

        public string UF { get; private set; }

        public URLws LocalHomologacao { get; private set; }

        public URLws LocalProducao { get; private set; }

        public webServices(int id, string nome, string uf)
        {
            LocalHomologacao = new URLws();
            LocalProducao = new URLws();
            ID = id;
            Nome = nome;
            UF = uf;
        }
    }

    public class URLws
    {
        public URLws() =>
            ///
            /// NFS-e
            CancelarNfse =
            ConsultarLoteRps =
            ConsultarNfse =
            ConsultarNfsePorRps =
            ConsultarSituacaoLoteRps =
            ConsultarURLNfse =
            ConsultarNFSePNG =
            ConsultarNFSePDF =
            InutilizarNFSe =
            RecepcionarLoteRps =
            ConsultaSequenciaLoteNotaRPS =
            ConsultarStatusNFse =
            GerarNFSe =

            ///
            /// CFS-e
            RecepcionarLoteCfse =
            CancelarCfse =
            ConsultarLoteCfse =
            ConfigurarTerminalCfse =
            EnviarInformeManutencaoCfse =
            InformeTrasmissaoSemMovimentoCfse =
            ConsultarDadosCadastroCfse =

            ///
            /// NF-e
            NFeRecepcaoEvento =
            NFeConsulta =
            NFeConsultaCadastro =
            NFeDownload =
            NFeInutilizacao =
            NFeManifDest =
            NFeStatusServico =
            NFeAutorizacao =
            NFeRetAutorizacao =

            ///
            /// MDF-e
            MDFeRecepcao =
            MDFeRecepcaoSinc =
            MDFeRetRecepcao =
            MDFeConsulta =
            MDFeStatusServico =
            MDFeRecepcaoEvento =
            MDFeNaoEncerrado =

            ///
            /// DF-e
            DFeRecepcao =

            ///
            /// CT-e
            CTeRecepcao =
            CTeRetRecepcao =
            CTeConsulta =
            CTeStatusServico =
            CTeRecepcaoEvento =
            CTeConsultaCadastro =
            CTeDistribuicaoDFe =
            CteRecepcaoOS = string.Empty;

        #region Propriedades referente as tags do webservice.xml

        // ******** ATENÇÃO *******
        // os nomes das propriedades tem que ser iguais as tags no WebService.xml
        // ******** ATENÇÃO *******

        #region NFe

        public string NFeInutilizacao { get; set; }

        public string NFeConsulta { get; set; }

        public string NFeStatusServico { get; set; }

        public string NFeConsultaCadastro { get; set; }

        public string NFeAutorizacao { get; set; }

        public string NFeRetAutorizacao { get; set; }

        /// <summary>
        /// Recepção de eventos da NFe
        /// </summary>
        public string NFeRecepcaoEvento { get; set; }

        public string NFeDownload { get; set; }

        public string NFeManifDest { get; set; }

        /// <summary>
        /// Distribuição de DFe de interesses de autores (NFe)
        /// </summary>
        public string DFeRecepcao { get; set; }

        #endregion NFe

        #region NFS-e

        /// <summary>
        /// Enviar Lote RPS NFS-e
        /// </summary>
        public string RecepcionarLoteRps { get; set; }

        /// <summary>
        /// Consultar Situação do lote RPS NFS-e
        /// </summary>
        public string ConsultarSituacaoLoteRps { get; set; }

        /// <summary>
        /// Consultar NFS-e por RPS
        /// </summary>
        public string ConsultarNfsePorRps { get; set; }

        /// <summary>
        /// Consultar NFS-e por NFS-e
        /// </summary>
        public string ConsultarNfse { get; set; }

        /// <summary>
        /// Consultar lote RPS
        /// </summary>
        public string ConsultarLoteRps { get; set; }

        /// <summary>
        /// Cancelar NFS-e
        /// </summary>
        public string CancelarNfse { get; set; }

        /// <summary>
        /// Consulta URL de Visualização da NFSe
        /// </summary>
        public string ConsultarURLNfse { get; set; }

        /// <summary>
        /// Consulta a imagem em PNG da Nota
        /// </summary>
        public string ConsultarNFSePNG { get; set; }

        /// <summary>
        /// Consulta a imagem em PDF da Nota
        /// </summary>
        public string ConsultarNFSePDF { get; set; }

        /// <summary>
        /// Inutilização da NFSe
        /// </summary>
        public string InutilizarNFSe { get; set; }

        /// <summary>
        /// Obter o XML da NFSe
        /// </summary>
        public string ObterNotaFiscal { get; set; }

        /// <summary>
        /// Consulta Sequencia Lote Nota RPS
        /// </summary>
        public string ConsultaSequenciaLoteNotaRPS { get; set; }

        /// <summary>
        /// Substituir Nfse
        /// </summary>
        public string SubstituirNfse { get; set; }

        /// <summary>
        /// Consultar as NFS-e que foram recebidas
        /// </summary>
        public string ConsultaNFSeRecebidas { get; set; }

        /// <summary>
        /// Consultar status Nfse
        /// </summary>
        public string ConsultarStatusNFse { get; set; }

        /// <summary>
        /// Consultar as NFS-e tomados
        /// </summary>
        public string ConsultaNFSeTomados { get; set; }

        /// <summary>
        /// Gerar NFSe
        /// </summary>
        public string GerarNFSe { get; set; }

        #endregion NFS-e

        #region CFS-e

        /// <summary>
        /// Enviar lote CFS-e
        /// </summary>
        public string RecepcionarLoteCfse { get; set; }

        /// <summary>
        /// Cancelar CFS-e
        /// </summary>
        public string CancelarCfse { get; set; }

        /// <summary>
        /// Consultar Lote CFS-e
        /// </summary>
        public string ConsultarLoteCfse { get; set; }

        /// <summary>
        /// Consultar Lote CFS-e
        /// </summary>
        public string ConsultarCfse { get; set; }

        /// <summary>
        /// Configurar/ativar terminal CFS-e
        /// </summary>
        public string ConfigurarTerminalCfse { get; set; }

        /// <summary>
        /// Enviar informe manutenção terminal CFS-e
        /// </summary>
        public string EnviarInformeManutencaoCfse { get; set; }

        /// <summary>
        /// Enviar informe de transmissão sem movimento da CFS-e
        /// </summary>
        public string InformeTrasmissaoSemMovimentoCfse { get; set; }

        /// <summary>
        /// Enviar o XML para consultar os dados do cadastro de terminal CFS-e
        /// </summary>
        public string ConsultarDadosCadastroCfse { get; set; }

        #endregion CFS-e

        #region MDF-e

        /// <summary>
        /// Recepção do MDFe Assíncrono
        /// </summary>
        public string MDFeRecepcao { get; set; }

        /// <summary>
        /// Recepção do MDFe Síncrono
        /// </summary>
        public string MDFeRecepcaoSinc { get; set; }
        /// <summary>
        /// Consulta Recibo do lote de MDFe enviado
        /// </summary>
        public string MDFeRetRecepcao { get; set; }

        /// <summary>
        /// Consulta situação do MDFe
        /// </summary>
        public string MDFeConsulta { get; set; }

        /// <summary>
        /// Consulta status do serviço de MDFe
        /// </summary>
        public string MDFeStatusServico { get; set; }

        /// <summary>
        /// Recepcao dos eventos do MDF-e
        /// </summary>
        public string MDFeRecepcaoEvento { get; set; }

        /// <summary>
        /// Consulta dos MDFe´s não encerrados
        /// </summary>
        public string MDFeNaoEncerrado { get; set; }

        #endregion MDF-e

        #region CTe

        /// <summary>
        /// Recepção do CTe
        /// </summary>
        public string CTeRecepcao { get; set; }

        /// <summary>
        /// Consulta recibo do lote de CTe enviado
        /// </summary>
        public string CTeRetRecepcao { get; set; }

        /// <summary>
        /// Consulta Situação do CTe
        /// </summary>
        public string CTeConsulta { get; set; }

        /// <summary>
        /// Consulta Status Serviço do CTe
        /// </summary>
        public string CTeStatusServico { get; set; }

        /// <summary>
        /// Consulta cadastro do contribuinte do CTe
        /// </summary>
        public string CTeConsultaCadastro { get; set; }

        /// <summary>
        /// Recepção de eventos do CTe
        /// </summary>
        public string CTeRecepcaoEvento { get; set; }

        /// <summary>
        /// Distribuição de DFe de interesses de autores (CTe)
        /// </summary>
        public string CTeDistribuicaoDFe { get; set; }

        /// <summary>
        /// Recepção do CTe modelo 67
        /// </summary>
        public string CteRecepcaoOS { get; set; }

        #endregion CTe

        #region EFDReinf

        /// <summary>
        /// Recepção do lote de eventos do EFDReinf
        /// </summary>
        public string RecepcaoLoteReinf { get; set; }

        /// <summary>
        /// Consultas do EFDReinf: totalizações e recibo de entrega
        /// </summary>
        public string ConsultasReinf { get; set; }

        #endregion EFDReinf

        #region eSocial

        /// <summary>
        /// Recepção do lote de eventos do eSocial
        /// </summary>
        public string RecepcaoLoteeSocial { get; set; }

        /// <summary>
        /// Consulta do lote de eventos do eSocial
        /// </summary>
        public string ConsultarLoteeSocial { get; set; }

        /// <summary>
        /// Consulta dos identificadores dos eventos do eSocial: Empregador, Tabela e Trabalhador
        /// </summary>
        public string ConsultarIdentificadoresEventoseSocial { get; set; }

        /// <summary>
        /// Download dos eventos por Id e Número do recibo
        /// </summary>
        public string DownloadEventoseSocial { get; set; }

        #endregion eSocial

        #endregion Propriedades referente as tags do webservice.xml
    }
}