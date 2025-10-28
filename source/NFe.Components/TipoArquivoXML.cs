using System;
using System.IO;
using System.Linq;
using System.Xml;
using Unimake.Business.DFe.Servicos;
namespace NFe.Components
{
    public class TipoArquivoXML
    {
        public int nRetornoTipoArq { get; set; }
        public string cRetornoTipoArq { get; private set; }

        /// <summary>
        /// Tag que deve ser assinada no XML, se o conteúdo estiver em branco é por que o XML não deve ser assinado
        /// </summary>
        public string TagAssinatura { get; private set; }

        /// <summary>
        /// Tag que tem o atributo ID no XML
        /// </summary>
        public string TagAtributoId { get; private set; }

        /// <summary>
        /// Nome da tag do XML que será assinada (uma segunda tag que tem que ser assinada ex. SubstituirNfse Pelotas-RS)
        /// </summary>
        public string TagAssinatura0 { get; private set; }

        /// <summary>
        /// Nome da tag que tem o atributo ID que será assinada, faz consunto com a TagAssinatura0
        /// </summary>
        public string TagAtributoId0 { get; private set; }

        /// <summary>
        /// Tag que deve ser assinada no XML, se o conteúdo estiver em branco é por que o XML não deve ser assinado
        /// </summary>
        public string TagLoteAssinatura { get; private set; }

        /// <summary>
        /// Tag que tem o atributo ID no XML
        /// </summary>
        public string TagLoteAtributoId { get; private set; }

        public string cArquivoSchema { get; set; }
        public string TargetNameSpace { get; private set; }

        public TipoArquivoXML() { }

        public TipoArquivoXML(string rotaArqXML, int UFCod, bool soValidar)
        {
            XmlDocument conteudoXML = new XmlDocument();
            try
            {
                conteudoXML.Load(rotaArqXML);
            }
            catch
            {
                conteudoXML.LoadXml(File.ReadAllText(rotaArqXML, System.Text.Encoding.UTF8));
            }

            DefinirTipoArq(rotaArqXML, conteudoXML, UFCod, soValidar);
        }

        private void DefinirTipoArq(string fullPathXML, XmlDocument conteudoXML, int UFCod, bool soValidar)
        {
            bool nfse = (UFCod.ToString().Length == 7) || (UFCod == 202);
            bool reinf = (conteudoXML.DocumentElement.Name == "Reinf");
            bool esocial = (conteudoXML.DocumentElement.Name == "eSocial");

            nRetornoTipoArq = 0;

            cRetornoTipoArq =
                cArquivoSchema =
                TagAssinatura =
                TagAtributoId =
                TagLoteAssinatura =
                TagLoteAtributoId =
                TargetNameSpace = string.Empty;

            string versaoXML = string.Empty;
            string varPadraoNFSe = string.Empty;

            try
            {
                if (!reinf && !esocial)
                {
                    #region Definir padrão NFSe

                    if (Functions.BuscaPadraoNFSe(UFCod) == PadraoNFSe.FIORILLI)
                    {
                        varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-2.01-";
                    }
                    else if (nfse)
                    {
                        switch (UFCod)
                        {
                            case 3304003: //Piraí - RJ (TIPLAN)
                            case 4304507: //Canguçu - RS (Padrão PRONIM 1.00)
                            case 3535804: //Paranapanema - SP (Padrão PRONIM 1.00)
                            case 4321709: //Três Coroas - RS (Padrão PRONIM 1.00)
                            case 4122404: //Rolândia - PR (Padrão PRONIM 1.00)
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-1.00-";
                                break;

                            case 4314050: //Parobé
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-4314050-";
                                break;

                            case 4320008: //Sapucaia do Sul - RS
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-4320008-";
                                break;

                            case 4125506: //São José dos Pinhais-PR (GINFES)
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-4125506-";
                                break;

                            case 2304400: //Fortaleza - CE
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-2304400-";
                                break;

                            case 4113700: //Londrina-PR (SIGCORP_SIGISS)
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-4113700-";
                                break;

                            case 4109401: //Guarapuava-PR
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-4109401-";
                                break;

                            case 3512803: //Cosmópolis-SP
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-3512803-";
                                break;

                            case 5005707: //Naviraí-MS
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-5005707-";
                                break;

                            case 3549904: //São José dos Campos-SP
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-3549904-";
                                break;

                            case 1502400: //São Pedro - SP
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-1502400-";
                                break;

                            case 3509502: //Campinas - SP (padrão DSF)
                            case 2507507: //João Pessoa - PB (padrão DSF)
                            case 2211001: //Teresina - PI (padrão DSF)

                                string[] tagsServicos = { "ns1:ReqEnvioLoteRPS", "ns1:ConsultaSeqRps", "ns1:ReqConsultaNotas",
                                    "ns1:ReqConsultaNFSeRPS", "ns1:ReqCancelamentoNFSe", "ns1:ReqConsultaLote" };

                                if (tagsServicos.Any(conteudoXML.OuterXml.Contains))
                                {
                                    varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-";
                                }
                                else
                                {
                                    varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-2.03-";
                                }

                                break;

                            case 3205002: //Serra-ES (padrão SMARAPD)
                            case 3170206: //Uberlândia-MG (padrão DSF)
                            case 3144805: //Nova Lima-MG (Padrão ABACO)
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-2.04-";
                                break;

                            case 4115200: //Maringá - PR (Padrão FINTEL)
                            case 2611606: //Recife - PE (Padrão TIPLAN)
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-2.01-";
                                break;

                            case 3300407: //Barra Mansa - RJ (Padrão COPLAN)
                            case 4306932: //Entre-Ijuís - RS (Padrão PRONIM 2.02)
                            case 3113404: //Caratinga - MG (Padrão PRONIM 2.02)
                            case 4321006: //Tapera - RS (Padrão PRONIM 2.02)
                            case 4303004: //Cachoeira do Sul - RS (Padrão PRONIM 2.02)
                            case 3143302: //Montes Claros - MG (Padrão PRONIM 2.02)
                            case 3131703: //Itabira - MG (Padrão PRONIM 2.02)
                            case 3302205: //Itaperuna - RJ (Padrão PRONIM 2.02)
                            case 3530300: //Mirassol - SP (Padrão PRONIM 2.02)
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-2.02-";
                                break;

                            case 3550605: //São Roque - RS (padrão PRONIM 2.03)
                            case 4202404: //Blumenau - SC (Padrão SIMPLISS 2.03)
                            case 3306305: //Volta Redonda - RJ (Padrão SIMPLISS 2.03)
                            case 4301057: //Arroio do Sal - RS (Padrão PRONIM 2.03)
                            case 4118501: //Pato Branco - PR (Padrão PRONIM 2.03)
                            case 4314423: //Picada Café - RS (Padrão PRONIM 2.03)
                            case 4313953: //Pantano Grande - RS (Padrão PRONIM 2.03)
                            case 4302808: //Caçapava do Sul - RS (Padrão PRONIM 2.03)
                            case 3507001: //Boituva - SP (Padrão PRONIM 2.03)
                            case 4322400: //Uruguaiana-RS (Padrão PRONIM 2.03)
                            case 4300109: //Agudo - RS (Padrão PRONIM 2.03)
                            case 4310207: //Ijuí - RS (Padrão PRONIM 2.03)
                            case 4308904: //Getúlio Vargas - RS (Padrão PRONIM 2.03)
                            case 4322004: //Triunfo - RS (Padrão PRONIM 2.03)
                            case 3542404: //Regente Feijó - SP (Padrão PRONIM 2.03)
                            case 4124053: //Santa Terezinha de Itaipu - PR (Padrão PRONIM 2.03)
                            case 3550407: //São Pedro - SP (Padrão PRONIM 2.03)
                            case 3554300: //Teodoro Sampaio - SP (Padrão PRONIM 2.03)
                            case 4318309: //São Gabriel - SP (Padrão PRONIM 2.03)
                            case 3550803: //São Sebastião da Grama - SP (Padrão PRONIM 2.03)
                            case 4322509: //Vacaria - RS (Padrão PRONIM 2.03)
                            case 4323002: //Viamão - RS (Padrão PRONIM 2.03)
                            case 3501301: //Álvares Machado - SP (Padrão PRONIM 2.03)
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-2.03-";
                                break;

                            default:
                                varPadraoNFSe = Functions.BuscaPadraoNFSe(UFCod).ToString() + "-";
                                break;

                        }
                    }
                    #endregion Definir padrão NFSe
                }

                InfSchema schema = null;
                string chave = "";
                string versao = string.Empty;

                try
                {
                    string nome = conteudoXML.DocumentElement.Name;

                    switch (nome)
                    {
                        #region EFDReinf

                        case "Reinf":
                            if (!string.IsNullOrEmpty(conteudoXML.DocumentElement.NamespaceURI))
                                versao = conteudoXML.DocumentElement.NamespaceURI.Substring(conteudoXML.DocumentElement.NamespaceURI.Length - 7);

                            chave = nome + "-" + conteudoXML.DocumentElement.FirstChild.Name;

                            if (versao == "1_05_01" || versao == "1_00_00")
                            {
                                break;
                            }
                            chave += "-" + versao;

                            break;

                        #endregion EFDReinf

                        #region eSocial

                        case "eSocial":
                            if (conteudoXML.DocumentElement.FirstChild.Name.Equals("consultaIdentificadoresEvts") ||
                                conteudoXML.DocumentElement.FirstChild.Name.Equals("download"))
                            {
                                chave = nome + "-" + conteudoXML.DocumentElement.FirstChild.Name + "-" + conteudoXML.DocumentElement.FirstChild.FirstChild.NextSibling.Name;
                            }
                            else
                            {
                                chave = nome + "-" + conteudoXML.DocumentElement.FirstChild.Name;
                                if ((conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.Name)[0]).Attributes["xmlns"].Value.ToLower().Contains("v_s_01_01_00"))
                                {
                                    chave += "-v_S_01_01_00";
                                }
                                else if ((conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.Name)[0]).Attributes["xmlns"].Value.ToLower().Contains("v_s_01_02_00"))
                                {
                                    chave += "-v_S_01_02_00";
                                }
                            }
                            break;

                        #endregion eSocial

                        #region NFe, NFCe, CTe, MDFe e NFSe

                        default:
                            if (((XmlElement)conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.Name)[0]).Attributes[TpcnResources.versao.ToString()] != null)
                                versao = ((XmlElement)conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.Name)[0]).Attributes[TpcnResources.versao.ToString()].Value;
                            else if (((XmlElement)conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.FirstChild.Name)[0]).Attributes[TpcnResources.versao.ToString()] != null)
                                versao = ((XmlElement)conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.FirstChild.Name)[0]).Attributes[TpcnResources.versao.ToString()].Value;
                            else if (((XmlElement)conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.Name)[0]).Attributes["versaoModal"] != null)
                                versao = ((XmlElement)conteudoXML.GetElementsByTagName(conteudoXML.DocumentElement.Name)[0]).Attributes["versaoModal"].Value;

                            if (nfse)
                            {
                                switch (Functions.BuscaPadraoNFSe(UFCod))
                                {
                                    case PadraoNFSe.GINFES:
                                        if (conteudoXML.DocumentElement.Name == "e:CancelarNfseEnvio" && conteudoXML.DocumentElement.FirstChild.Name == "Pedido")
                                        {
                                            versaoXML = "-3";
                                        }
                                        break;
                                }
                                if (Functions.BuscaPadraoNFSe(UFCod) == PadraoNFSe.GINFES)
                                {
                                    if (conteudoXML.DocumentElement.Name == "e:CancelarNfseEnvio" && conteudoXML.DocumentElement.FirstChild.Name == "Pedido")
                                    {
                                        versaoXML = "-3";
                                    }
                                }
                            }
                            else if (conteudoXML.DocumentElement.Name.Equals("distDFeInt") && (versao.Equals("1.01") || versao.Equals("1.35")))
                                versaoXML = "-" + versao;

                            if (string.IsNullOrEmpty(varPadraoNFSe))
                            {
                                if (nome.Equals("envEvento") || nome.Equals("eventoCTe"))
                                {
                                    XmlElement cl = (XmlElement)conteudoXML.GetElementsByTagName(TpcnResources.tpEvento.ToString())[0];
                                    if (cl != null)
                                    {
                                        string evento = cl.InnerText;
                                        switch (evento)
                                        {
                                            case "110110":  //XML de Evento da CCe
                                            case "110111":  //XML de Envio de evento de cancelamento
                                            case "110112":  //XML de Envio de evento de cancelamento por substituição
                                            case "110113":  //XML de Envio do evento de contingencia EPEC, CTe
                                            case "110160":  //XML de Envio do evento de Registro Multimodal, CTe
                                            case "111500":  //Evento pedido de prorrogação 1º. prazo
                                            case "111501":  //Evento pedido de prorrogação 2º. prazo
                                            case "111502":  //Evento Cancelamento de Pedido de Prorrogação 1º. Prazo
                                            case "111503":  //Evento Cancelamento de Pedido de Prorrogação 2º. Prazo
                                            case "411500":  //Evento Fisco Resposta ao Pedido de Prorrogação 1º prazo
                                            case "411501":  //Evento Fisco Resposta ao Pedido de Prorrogação 2º prazo
                                            case "411502":  //Evento Fisco Resposta ao Cancelamento de Prorrogação 1º prazo
                                            case "411503":  //Evento Fisco Resposta ao Cancelamento de Prorrogação 2º prazo
                                            case "610110":  //CTe Prestação de Serviços em Desacordo
                                            case "110180":  //CTe Comprovante de Entrega
                                            case "110181":  //CTe Cancelamento Comprovante de Entrega
                                                nome = nome + evento;
                                                break;

                                            case "110140":  //EPEC
                                                string mod = string.Empty;
                                                if (((XmlElement)conteudoXML.GetElementsByTagName("infEvento")[0]).Attributes[TpcnResources.Id.ToString()] != null)
                                                {
                                                    mod = "-" + ((XmlElement)conteudoXML.GetElementsByTagName("infEvento")[0]).Attributes[TpcnResources.Id.ToString()].Value.Substring(28, 2) + "-";
                                                    if (!mod.Equals("-65-"))
                                                        mod = string.Empty;
                                                }

                                                nome = nome + mod + evento;
                                                break;

                                            case "210200":  //XML Evento de manifestação do destinatário
                                            case "210210":  //XML Evento de manifestação do destinatário
                                            case "210220":  //XML Evento de manifestação do destinatário
                                            case "210240":  //XML Evento de manifestação do destinatário
                                                nome = "envConfRecebto";
                                                break;
                                        }
                                    }
                                }
                                else if (nome.Equals("eventoMDFe"))
                                {
                                    XmlElement cl = (XmlElement)conteudoXML.GetElementsByTagName(TpcnResources.tpEvento.ToString())[0];
                                    if (cl != null)
                                    {
                                        nome = "eventoMDFe" + cl.InnerText;
                                    }
                                }
                                else if (nome.Equals("distDFeInt"))
                                {
                                    if (conteudoXML.DocumentElement.NamespaceURI.ToLower().Equals("http://www.portalfiscal.inf.br/cte"))
                                    {
                                        nome = nome + "CTe";
                                    }
                                }
                                else if (nome.Equals("infModal"))
                                {
                                    if (conteudoXML.DocumentElement.NamespaceURI.ToLower().Equals("http://www.portalfiscal.inf.br/cte"))
                                    {
                                        nome = conteudoXML.FirstChild.FirstChild.Name + "-CTe";
                                    }
                                    if (conteudoXML.DocumentElement.NamespaceURI.ToLower().Equals("http://www.portalfiscal.inf.br/mdfe"))
                                    {
                                        nome = conteudoXML.FirstChild.FirstChild.Name + "-MDFe";
                                    }
                                }
                            }

                            if (!nfse)
                                chave = TipoAplicativo.Nfe.ToString().ToUpper() + versaoXML + "-" + nome;
                            else
                                chave = TipoAplicativo.Nfse.ToString().ToUpper() + versaoXML + "-" + varPadraoNFSe + nome;

                            break;

                            #endregion NFe, NFCe, CTe, MDFe e NFSe
                    }

                    schema = SchemaXML.InfSchemas[chave];
                }
                catch
                {
                    if (soValidar && chave.StartsWith(TipoAplicativo.Nfse.ToString().ToUpper() + versaoXML + "-"))
                    {
                        cRetornoTipoArq = fullPathXML;
                        nRetornoTipoArq = -1;
                        return;
                    }
                    throw new Exception(string.Format("Não foi possível identificar o tipo do XML para ser validado, ou seja, o sistema não sabe se é um XML de {0}, consulta, etc. ", string.IsNullOrEmpty(varPadraoNFSe) ? "NF-e/NFC-e/CT-e/MDF-e" : "NFS-e") +
                        "Por favor verifique se não existe algum erro de estrutura do XML que impede sua identificação. (Chave: " + chave + ")");
                }

                nRetornoTipoArq = schema.ID;
                cRetornoTipoArq = schema.Descricao;
                TagAssinatura = schema.TagAssinatura;
                TagAtributoId = schema.TagAtributoId;
                TagAssinatura0 = schema.TagAssinatura0;
                TagAtributoId0 = schema.TagAtributoId0;
                TagLoteAssinatura = schema.TagLoteAssinatura;
                TagLoteAtributoId = schema.TagLoteAtributoId;
                TargetNameSpace = schema.TargetNameSpace;

                if (!string.IsNullOrEmpty(schema.ArquivoXSD))
                {
                    if (nfse)
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, "NFse\\schemas\\" + schema.ArquivoXSD);
                    else
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, "NFe\\schemas\\" + string.Format(schema.ArquivoXSD, versao));
                }

                if (!string.IsNullOrEmpty(schema.TargetNameSpace) && reinf)
                {
                    TargetNameSpace = string.Format(schema.TargetNameSpace, versao);
                }

            }
            catch (Exception ex)
            {
                nRetornoTipoArq = SchemaXML.MaxID + 102;
                cRetornoTipoArq = ex.Message;
            }

            if (nRetornoTipoArq == 0)
            {
                nRetornoTipoArq = SchemaXML.MaxID + 101;
                cRetornoTipoArq = "Não foi possível identificar o arquivo XML";
            }
        }
    }
}