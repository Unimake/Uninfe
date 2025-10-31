using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Unimake.Exceptions;
using ServicosCTe = Unimake.Business.DFe.Servicos.CTe;
using ServicosCTeOS = Unimake.Business.DFe.Servicos.CTeOS;
using ServicosEFDReinf = Unimake.Business.DFe.Servicos.EFDReinf;
using ServicosMDFe = Unimake.Business.DFe.Servicos.MDFe;
using ServicosNFe = Unimake.Business.DFe.Servicos.NFe;
using XmlCTeOS = Unimake.Business.DFe.Xml.CTeOS;

namespace NFe.Validate
{
    public class ValidarXMLNew
    {
        #region Private Methods

        /// <summary>
        /// Na tentativa de somente validar ou assinar o XML se encontrar um erro vai ser retornado um XML para o ERP
        /// </summary>
        /// <param name="arquivo">Nome do arquivo XML validado</param>
        /// <param name="PastaXMLRetorno">Pasta de retorno para ser gravado o XML</param>
        /// <param name="cStat">Status da validação</param>
        /// <param name="xMotivo">Status descritivo da validação</param>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>28/05/2009</date>
        private void GravarXMLRetornoValidacao(string arquivo, string cStat, string xMotivo)
        {
            var emp = Empresas.FindEmpresaByThread();

            //Definir o nome do arquivo de retorno
            var arquivoRetorno = Functions.ExtrairNomeArq(arquivo, ".xml") + "-ret.xml";

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement("Validacao",
                new XElement("cStat", cStat),
                new XElement("xMotivo", xMotivo)));
            xml.Save(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + arquivoRetorno);

            if (cStat == "1")
            {
                if (!arquivo.ToLower().Contains((Empresas.Configuracoes[emp].PastaXmlEmLote.Trim() + "\\temp").ToLower()))
                {
                    File.Delete(arquivo);
                }
            }
            else
            {
                var arqErro = Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Functions.ExtrairNomeArq(arquivo, ".xml") + ".xml");
                if (File.Exists(arqErro))
                {
                    File.Delete(arqErro);
                }

                File.Move(arquivo, arqErro);
            }
        }

        private void GravarXmlRetornoWarnings(string arquivo, List<ValidatorDFeException> xMotivo)
        {
            var emp = Empresas.FindEmpresaByThread();

            var arquivoRetorno = Functions.ExtrairNomeArq(arquivo, ".xml") + "-warning.xml";

            var warnings = new List<XElement>();

            foreach (var warning in xMotivo)
            {
                warnings.Add(new XElement("Warning", warning.Message));
            }

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement("Validacao",
                new XElement("Warnings", warnings)));
            xml.Save(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + arquivoRetorno);

        }

        private void Validar(TipoXML tipoXML, Configuracao configuracao, ValidarSchema validarSchema, bool retornoArquivo, XmlDocument xmlSalvar, string arquivoXML, XmlDocument xmlDoc, int emp)
        {
            var schema = configuracao.TipoDFe.ToString() + "." + configuracao.SchemaArquivo;
            switch (configuracao.TipoDFe)
            {
                case TipoDFe.NFCe:
                    schema = TipoDFe.NFe.ToString() + "." + configuracao.SchemaArquivo;
                    break;

                case TipoDFe.CTeOS:
                    schema = TipoDFe.CTe.ToString() + "." + configuracao.SchemaArquivo;
                    break;
            }

            TipoArquivoXML = EnumHelper.GetEnumItemDescription(tipoXML);

            validarSchema.Validar(xmlDoc, schema, configuracao.TargetNS);

            if (!validarSchema.Success)
            {
                var erro = "Ocorreu um erro ao validar o XML: " + validarSchema.ErrorMessage;


                if (retornoArquivo)
                {
                    GravarXMLRetornoValidacao(arquivoXML, "2", erro);
                    new Auxiliar().MoveArqErro(arquivoXML);
                }
                else
                {
                    throw new Exception(erro);
                }
            }
            else
            {
                if (retornoArquivo)
                {
                    if (arquivoXML.ToLower().Contains((Empresas.Configuracoes[emp].PastaXmlEmLote.Trim() + "\\temp").ToLower()))
                    {
                        //Gravar XML assinado
                        var SW_2 = File.CreateText(arquivoXML);
                        SW_2.Write(xmlSalvar.OuterXml);
                        SW_2.Close();
                    }
                    else
                    {
                        var pasta = Empresas.Configuracoes[emp].PastaValidado;

                        if (!Directory.Exists(pasta))
                        {
                            Directory.CreateDirectory(pasta);
                        }

                        var arquivoNovo = Path.Combine(pasta, Path.GetFileName(arquivoXML));

                        //Gravar XML assinado e validado na subpasta "Validados"
                        var SW_2 = File.CreateText(arquivoNovo);
                        SW_2.Write(xmlSalvar.OuterXml);
                        SW_2.Close();
                    }

                    GravarXMLRetornoValidacao(arquivoXML, "1", "XML assinado e validado com sucesso.");
                }
            }
        }

        private void XmlValidarCTe(TipoXML tipoXML, Configuracao configuracao, ValidarSchema validarSchema, bool retornoArquivo, XmlDocument xmlSalvar, string arquivoXML, XmlDocument xmlDoc, int emp)
        {
            if (configuracao.SchemasEspecificos.Count > 0)
            {
                #region Validar o XML geral

                configuracao.SchemaArquivo = configuracao.SchemasEspecificos["1"].SchemaArquivo; //De qualquer modal o xml de validação da parte geral é o mesmo, então vou pegar do número 1, pq tanto faz.
                Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);

                #endregion Validar o XML geral

                #region Validar a parte específica de modal do CTe

                foreach (XmlElement itemCTe in xmlDoc.GetElementsByTagName("CTe"))
                {
                    var modal = string.Empty;

                    foreach (XmlElement itemIde in itemCTe.GetElementsByTagName("ide"))
                    {
                        modal = itemIde.GetElementsByTagName("modal")[0].InnerText;
                    }

                    foreach (XmlElement itemInfModal in itemCTe.GetElementsByTagName("infModal"))
                    {
                        var xmlEspecifico = new XmlDocument();
                        xmlEspecifico.LoadXml(itemInfModal.InnerXml);

                        configuracao.SchemaArquivo = configuracao.SchemasEspecificos[modal.Substring(1, 1)].SchemaArquivoEspecifico;
                        Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlEspecifico, emp);
                    }
                }

                #endregion Validar a parte específica de modal do CTe
            }
        }

        private void XmlValidarEventoNFe(TipoXML tipoXML, Configuracao configuracao, ValidarSchema validarSchema, bool retornoArquivo, XmlDocument xmlSalvar, string arquivoXML, XmlDocument xmlDoc, int emp)
        {
            if (xmlDoc.GetElementsByTagName("tpEvento")[0] == null)
            {
                throw new Exception("Não foi possível localizar a tag tpEvento no XML para identificar o tipo do evento a ser validado.");
            }

            var tpEvento = xmlDoc.GetElementsByTagName("tpEvento")[0].InnerText;

            #region Validar o XML Geral

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[tpEvento].SchemaArquivo;

            Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);

            #endregion Validar o XML Geral

            #region Validar o Modal do Evento

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[tpEvento].SchemaArquivoEspecifico;

            var listEvento = xmlDoc.GetElementsByTagName("evento");
            for (var i = 0; i < listEvento.Count; i++)
            {
                var elementEvento = (XmlElement)listEvento[i];

                if (elementEvento.GetElementsByTagName("infEvento")[0] != null)
                {
                    var elementInfEvento = (XmlElement)elementEvento.GetElementsByTagName("infEvento")[0];
                    var xmlEspecifico = new XmlDocument();
                    xmlEspecifico.LoadXml(elementInfEvento.GetElementsByTagName("detEvento")[0].OuterXml);

                    Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlEspecifico, emp);
                }
            }

            #endregion Validar o Modal do Evento
        }

        private void XmlValidarEventoCTe(TipoXML tipoXML, Configuracao configuracao, ValidarSchema validarSchema, bool retornoArquivo, XmlDocument xmlSalvar, string arquivoXML, XmlDocument xmlDoc, int emp)
        {
            if (xmlDoc.GetElementsByTagName("tpEvento")[0] == null)
            {
                throw new Exception("Não foi possível localizar a tag tpEvento no XML para identificar o tipo do evento a ser validado.");
            }

            var tpEvento = xmlDoc.GetElementsByTagName("tpEvento")[0].InnerText;

            #region Validar o XML Geral

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[tpEvento].SchemaArquivo;

            Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);

            #endregion Validar o XML Geral

            #region Validar o Modal do Evento

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[tpEvento].SchemaArquivoEspecifico;

            var listEvento = xmlDoc.GetElementsByTagName("eventoCTe");
            for (var i = 0; i < listEvento.Count; i++)
            {
                var elementEvento = (XmlElement)listEvento[i];

                if (elementEvento.GetElementsByTagName("infEvento")[0] != null)
                {
                    var elementInfEvento = (XmlElement)elementEvento.GetElementsByTagName("infEvento")[0];
                    var xmlEspecifico = new XmlDocument();
                    xmlEspecifico.LoadXml(elementInfEvento.GetElementsByTagName(elementInfEvento.GetElementsByTagName("detEvento")[0].FirstChild.Name)[0].OuterXml);

                    Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlEspecifico, emp);
                }
            }

            #endregion Validar o Modal do Evento
        }

        private void XmlValidarEventoMDFe(TipoXML tipoXML, Configuracao configuracao, ValidarSchema validarSchema, bool retornoArquivo, XmlDocument xmlSalvar, string arquivoXML, XmlDocument xmlDoc, int emp)
        {
            if (xmlDoc.GetElementsByTagName("tpEvento")[0] == null)
            {
                throw new Exception("Não foi possível localizar a tag tpEvento no XML para identificar o tipo do evento a ser validado.");
            }

            var tpEvento = xmlDoc.GetElementsByTagName("tpEvento")[0].InnerText;

            #region Validar o XML Geral

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[tpEvento].SchemaArquivo;

            Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);

            #endregion Validar o XML Geral

            #region Validar o Modal do Evento

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[tpEvento].SchemaArquivoEspecifico;

            var listEvento = xmlDoc.GetElementsByTagName("eventoMDFe");
            for (var i = 0; i < listEvento.Count; i++)
            {
                var elementEvento = (XmlElement)listEvento[i];

                if (elementEvento.GetElementsByTagName("infEvento")[0] != null)
                {
                    var elementInfEvento = (XmlElement)elementEvento.GetElementsByTagName("infEvento")[0];
                    var xmlEspecifico = new XmlDocument();
                    xmlEspecifico.LoadXml(elementInfEvento.GetElementsByTagName(elementInfEvento.GetElementsByTagName("detEvento")[0].FirstChild.Name)[0].OuterXml);

                    Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlEspecifico, emp);
                }
            }

            #endregion Validar o Modal do Evento
        }

        private void XmlValidarMDFe(TipoXML tipoXML, Configuracao configuracao, ValidarSchema validarSchema, bool retornoArquivo, XmlDocument xmlSalvar, string arquivoXML, XmlDocument xmlDoc, int emp)
        {
            var modal = 0;

            if (configuracao.SchemasEspecificos.Count > 0)
            {
                if (xmlDoc.GetElementsByTagName("MDFe").Count <= 0)
                {
                    throw new Exception("A tag obrigatória <MDFe> não foi localizada no XML.");
                }
                var elementMDFe = (XmlElement)xmlDoc.GetElementsByTagName("MDFe")[0];

                if (elementMDFe.GetElementsByTagName("infMDFe").Count <= 0)
                {
                    throw new Exception("A tag obrigatória <infMDFe>, do grupo de tag <MDFe>, não foi localizada no XML.");
                }
                var elementInfMDFe = (XmlElement)elementMDFe.GetElementsByTagName("infMDFe")[0];

                if (elementInfMDFe.GetElementsByTagName("ide").Count <= 0)
                {
                    throw new Exception("A tag obrigatória <ide>, do grupo de tag <MDFe><infMDFe>, não foi localizada no XML.");
                }
                var elementIde = (XmlElement)elementInfMDFe.GetElementsByTagName("ide")[0];

                if (elementIde.GetElementsByTagName("modal").Count <= 0)
                {
                    throw new Exception("A tag obrigatória <modal>, do grupo de tag <MDFe><infMDFe><ide>, não foi localizada no XML.");
                }

                modal = Convert.ToInt32(elementIde.GetElementsByTagName("modal")[0].InnerText);
            }

            #region Validar o XML geral

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[modal.ToString()].SchemaArquivo; //De qualquer modal o xml de validação da parte geral é o mesmo, então vou pegar do número 1, pq tanto faz.
            Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);

            #endregion Validar o XML geral

            #region Validar a parte específica de modal do MDFe

            var xmlEspecifico = new XmlDocument();
            foreach (XmlElement item in xmlDoc.GetElementsByTagName("infModal"))
            {
                xmlEspecifico.LoadXml(item.InnerXml);
            }

            configuracao.SchemaArquivo = configuracao.SchemasEspecificos[modal.ToString()].SchemaArquivoEspecifico;
            Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlEspecifico, emp);

            #endregion Validar a parte específica de modal do MDFe
        }

        private void ValidarXmlEventoEFDReinf(TipoXML tipoXML, Configuracao configuracao, ValidarSchema validarSchema, bool retornoArquivo, string arquivoXML, XmlDocument xmlDoc, int emp)
        {
            var xmlEvento = xmlDoc.GetElementsByTagName("Reinf")[1];

            var nomeEvento = xmlEvento.FirstChild.Name;
            configuracao.SchemaArquivo = configuracao.TiposEventosEspecificos[nomeEvento.ToString()].SchemaArquivoEvento;
            configuracao.TargetNS = configuracao.TiposEventosEspecificos[nomeEvento.ToString()].TargetNS;

            xmlDoc.LoadXml(xmlEvento.OuterXml);

            Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlDoc, arquivoXML, xmlDoc, emp);
        }

        #endregion Private Methods

        #region Public Properties

        public string TipoArquivoXML { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Validar XML
        /// </summary>
        /// <param name="arquivoXML">Caminho completo do arquivo XML a ser validado</param>
        /// <param name="retornoArquivo">Gerar arquivos na pasta de retorno com a resposta da validação? Se false, não vai gerar os retornos bem como não vai movimentar o arquivo validado para a subpasta validados</param>
        /// <returns>Retorna se efetuou a validação ou não (Se não detectar o tipo do arquivo ele não roda a validação)</returns>
        public bool Validar(string arquivoXML, bool retornoArquivo, int indiceEmpresa)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(arquivoXML);

            return Validar(xmlDoc, retornoArquivo, arquivoXML, indiceEmpresa);
        }

        /// <summary>
        /// Validar XML
        /// </summary>
        /// <param name="xmlDoc">XML a ser validado</param>
        /// <param name="retornoArquivo">Gerar arquivos na pasta de retorno com a resposta da validação? Se false, não vai gerar os retornos bem como não vai movimentar o arquivo validado para a subpasta validados</param>
        /// <returns>Retorna se efetuou a validação ou não (Se não detectar o tipo do arquivo ele não roda a validação)</returns>
        public bool Validar(XmlDocument xmlDoc, bool retornoArquivo, string arquivoXML, int indiceEmpresa)
        {
            List<ValidatorDFeException> warnings = null;
            var emp = indiceEmpresa;
            try
            {
                var xmlSalvar = new XmlDocument();

                var configuracao = new Configuracao
                {
                    TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                var validarSchema = new ValidarSchema();

                var tipoXML = XMLUtility.DetectXMLType(xmlDoc);

                var jaValidou = false;

                #region Adicionar o responsável técnico no XML da NFe, NFCe, CTe e MDFe

                var respTecnico = new RespTecnico(Empresas.Configuracoes[emp].RespTecCNPJ,
                    Empresas.Configuracoes[emp].RespTecXContato,
                    Empresas.Configuracoes[emp].RespTecEmail,
                    Empresas.Configuracoes[emp].RespTecTelefone,
                    Empresas.Configuracoes[emp].RespTecIdCSRT,
                    Empresas.Configuracoes[emp].RespTecCSRT);

                respTecnico.AdicionarResponsavelTecnico(xmlDoc);

                #endregion

                AppDomain.CurrentDomain.AssemblyResolve += Unimake.Business.DFe.Xml.AssemblyResolver.AssemblyResolve;

                switch (tipoXML)
                {
                    #region XML da NFe

                    case TipoXML.NFeStatusServico:
                        configuracao.TipoDFe = TipoDFe.NFe;
                        var statusServico = new ServicosNFe.StatusServico(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = statusServico.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    case TipoXML.NFeConsultaSituacao:
                        configuracao.TipoDFe = TipoDFe.NFe;
                        var consultaProtocolo = new ServicosNFe.ConsultaProtocolo(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = consultaProtocolo.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    case TipoXML.NFeConsultaRecibo:
                        configuracao.TipoDFe = TipoDFe.NFe;
                        var retAutorizacaoNFe = new ServicosNFe.RetAutorizacao(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = retAutorizacaoNFe.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    case TipoXML.NFeConsultaCadastro:
                        configuracao.TipoDFe = TipoDFe.NFe;
                        var consultaCadastroNFe = new ServicosNFe.ConsultaCadastro(xmlDoc.OuterXml, configuracao);
                        xmlDoc = consultaCadastroNFe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        break;

                    case TipoXML.NFeEnvioEvento:
                        xmlSalvar.LoadXml(xmlDoc.OuterXml);

                        // Remover a tpEmis se existir no XML para não dar erro de validação
                        var nodes = xmlDoc.GetElementsByTagName("tpEmis");

                        // Copia os nós para uma lista separada
                        var nodesToRemove = new List<XmlNode>();
                        foreach (XmlNode node in nodes)
                        {
                            nodesToRemove.Add(node);
                        }

                        // Agora remove com segurança
                        foreach (var node in nodesToRemove)
                        {
                            node.ParentNode.RemoveChild(node);
                        }

                        configuracao.TipoDFe = TipoDFe.NFe;
                        var recepcaoEventoNFe = new ServicosNFe.RecepcaoEvento(xmlDoc.OuterXml, configuracao);
                        xmlDoc = recepcaoEventoNFe.ConteudoXMLAssinado;

                        XmlValidarEventoNFe(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);
                        jaValidou = true;
                        break;

                    case TipoXML.NFeInutilizacao:
                        configuracao.TipoDFe = TipoDFe.NFe;
                        var inutilizacaoNFe = new ServicosNFe.Inutilizacao(xmlDoc.OuterXml, configuracao);
                        xmlDoc = inutilizacaoNFe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        break;

                    case TipoXML.NFeDistribuicaoDFe:
                        configuracao.TipoDFe = TipoDFe.NFe;
                        var distribuicaoDFe = new ServicosNFe.DistribuicaoDFe(xmlDoc.OuterXml, configuracao);
                        xmlDoc = distribuicaoDFe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        break;

                    case TipoXML.NFe:
                        var xmlNFe = "<enviNFe versao=\"4.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\">\r\n\t<idLote>000000000000001</idLote>\r\n\t<indSinc>0</indSinc>";
                        xmlNFe += xmlDoc.GetElementsByTagName("NFe")[0].OuterXml;
                        xmlNFe += "</enviNFe>";

                        if (xmlDoc.GetElementsByTagName("mod").Count < 0)
                        {
                            throw new Exception("Tag obrigatória <mod> não foi localizada no grupo de tag <NFe><infNFe><ide>.");
                        }
                        var modeloDoc = xmlDoc.GetElementsByTagName("mod")[0].InnerText;

                        if (modeloDoc == ((int)ModeloDFe.NFCe).ToString())
                        {
                            if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe == 3)
                            {
                                configuracao.VersaoQRCodeNFCe = 3;
                            }

                            if (xmlDoc.GetElementsByTagName("qrCode").Count == 0 && Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                            {
                                if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].IdentificadorCSC.Trim()) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC))
                                {
                                    throw new Exception("Para autorizar NFC-e é obrigatório informar nas configurações do UniNFe os campos CSC e IDToken do CSC.");
                                }
                            }

                            configuracao.TipoDFe = TipoDFe.NFCe;
                            if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                            {
                                configuracao.CSC = Empresas.Configuracoes[emp].IdentificadorCSC;
                                configuracao.CSCIDToken = Convert.ToInt32((string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC) ? "0" : Empresas.Configuracoes[emp].TokenCSC));
                            }

                            var autorizacao = new Unimake.Business.DFe.Servicos.NFCe.Autorizacao(xmlNFe, configuracao);
                            warnings = autorizacao.Warnings;
                            xmlDoc = autorizacao.ConteudoXMLAssinado;
                        }
                        else
                        {
                            configuracao.TipoDFe = TipoDFe.NFe;
                            var autorizacao = new ServicosNFe.Autorizacao(xmlNFe, configuracao);
                            warnings = autorizacao.Warnings;
                            xmlDoc = autorizacao.ConteudoXMLAssinado;
                        }

                        xmlSalvar.LoadXml(xmlDoc.GetElementsByTagName("NFe")[0].OuterXml);
                        break;

                    case TipoXML.NFeEnvioEmLote:
                        if (xmlDoc.GetElementsByTagName("mod").Count < 0)
                        {
                            throw new Exception("Tag obrigatória <mod> não foi localizada no grupo de tag <enviNFe><NFe><infNFe><ide>.");
                        }
                        var modeloDocXml = xmlDoc.GetElementsByTagName("mod")[0].InnerText;

                        if (modeloDocXml == ((int)ModeloDFe.NFCe).ToString())
                        {
                            if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe == 3)
                            {
                                configuracao.VersaoQRCodeNFCe = 3;
                            }

                            if (xmlDoc.GetElementsByTagName("qrCode").Count == 0 && Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3) 
                            {
                                if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].IdentificadorCSC.Trim()) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC))
                                {
                                    throw new Exception("Para autorizar NFC-e é obrigatório informar nas configurações do UniNFe os campos CSC e IDToken do CSC.");
                                }
                            }

                            configuracao.TipoDFe = TipoDFe.NFCe;
                            if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                            {
                                configuracao.CSC = Empresas.Configuracoes[emp].IdentificadorCSC;
                                configuracao.CSCIDToken = Convert.ToInt32((string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC) ? "0" : Empresas.Configuracoes[emp].TokenCSC));
                            }

                            var autorizacao = new Unimake.Business.DFe.Servicos.NFCe.Autorizacao(xmlDoc.OuterXml, configuracao);
                            warnings = autorizacao.Warnings;
                            xmlDoc = autorizacao.ConteudoXMLAssinado;

                            //TODO: WANDREY - WARNINGS

                        }
                        else
                        {
                            configuracao.TipoDFe = TipoDFe.NFe;
                            var autorizacao = new ServicosNFe.Autorizacao(xmlDoc.OuterXml, configuracao);
                            warnings = autorizacao.Warnings;
                            xmlDoc = autorizacao.ConteudoXMLAssinado;
                        }

                        xmlSalvar = xmlDoc;
                        break;

                    #endregion XML da NFe

                    #region XML do CTe

                    case TipoXML.CTeConsultaSituacao:
                        configuracao.TipoDFe = TipoDFe.CTe;
                        var consultaProtocoloCTe = new ServicosCTe.ConsultaProtocolo(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = consultaProtocoloCTe.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    case TipoXML.CTeStatusServico:
                        configuracao.TipoDFe = TipoDFe.CTe;
                        var statusServicoCTe = new ServicosCTe.StatusServico(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = statusServicoCTe.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    case TipoXML.CTeEnvioEvento:
                        configuracao.TipoDFe = TipoDFe.CTe;
                        var recepcaoEventoCTe = new ServicosCTe.RecepcaoEvento(xmlDoc.OuterXml, configuracao);
                        xmlDoc = recepcaoEventoCTe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        XmlValidarEventoCTe(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);
                        jaValidou = true;
                        break;

                    case TipoXML.CTeDistribuicaoDFe:
                        configuracao.TipoDFe = TipoDFe.CTe;
                        var distribuicaoCTe = new ServicosCTe.DistribuicaoDFe(xmlDoc.OuterXml, configuracao);
                        xmlDoc = distribuicaoCTe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        break;

                    case TipoXML.CTe:
                        configuracao.TipoDFe = TipoDFe.CTe;
                        var autorizacaoSincCTe = new ServicosCTe.AutorizacaoSinc(xmlDoc.OuterXml, configuracao); xmlDoc = autorizacaoSincCTe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        XmlValidarCTe(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);
                        jaValidou = true;
                        break;


                    case TipoXML.CTeOS:
                        var xmlCTeOS = new XmlCTeOS.CTeOS();
                        xmlCTeOS = XMLUtility.Deserializar<XmlCTeOS.CTeOS>(xmlDoc);
                        configuracao.TipoDFe = TipoDFe.CTeOS;
                        var autorizacaoCTeOS = new ServicosCTeOS.Autorizacao(xmlCTeOS, configuracao);
                        xmlDoc = autorizacaoCTeOS.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        break;

                    case TipoXML.CTeSimp:
                        configuracao.TipoDFe = TipoDFe.CTe;
                        var autorizacaoSimpCTe = new ServicosCTe.AutorizacaoSimp(xmlDoc.OuterXml, configuracao); xmlDoc = autorizacaoSimpCTe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        XmlValidarCTe(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);
                        jaValidou = true;
                        break;

                    #endregion XML do CTe

                    #region XML do MDFe

                    case TipoXML.MDFeEnvioEvento:
                        configuracao.TipoDFe = TipoDFe.MDFe;
                        var recepcaoEventoMDFe = new ServicosMDFe.RecepcaoEvento(xmlDoc.OuterXml, configuracao);
                        xmlDoc = recepcaoEventoMDFe.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;
                        XmlValidarEventoMDFe(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);
                        jaValidou = true;
                        break;

                    case TipoXML.MDFe:
                        configuracao.TipoDFe = TipoDFe.MDFe;
                        var autorizacaoSinc = new ServicosMDFe.AutorizacaoSinc(xmlDoc.OuterXml, configuracao);
                        xmlDoc = autorizacaoSinc.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;

                        XmlValidarMDFe(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);
                        jaValidou = true;
                        break;

                    case TipoXML.MDFeConsultaNaoEncerrado:
                        configuracao.TipoDFe = TipoDFe.MDFe;
                        var consMDFeNaoEnc = new ServicosMDFe.ConsNaoEnc(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = consMDFeNaoEnc.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    case TipoXML.MDFeConsultaSituacao:
                        configuracao.TipoDFe = TipoDFe.MDFe;
                        var consultaProtocoloMDFe = new ServicosMDFe.ConsultaProtocolo(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = consultaProtocoloMDFe.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    case TipoXML.MDFeStatusServico:
                        configuracao.TipoDFe = TipoDFe.MDFe;
                        var statusServicoMDFe = new ServicosMDFe.StatusServico(xmlDoc.OuterXml, configuracao);
                        xmlSalvar = xmlDoc;
                        xmlDoc = statusServicoMDFe.ConteudoXMLAssinado; // Depois de criar o xmlSalvar pq não posso apagar tags de uso específico do UNINFE que pode ter sido gerado pelo ERP.
                        break;

                    #endregion XML do MDFe

                    #region XML do EFD Reinf

                    case TipoXML.EFDReinfEnvioLoteEventos:
                        configuracao.TipoDFe = TipoDFe.EFDReinf;

                        var envioLoteEventos = new ServicosEFDReinf.RecepcionarLoteAssincrono(xmlDoc.OuterXml, configuracao);
                        xmlDoc = envioLoteEventos.ConteudoXMLAssinado;
                        xmlSalvar = xmlDoc;

                        break;

                    case TipoXML.EFDReinfEvento:
                        configuracao.TipoDFe = TipoDFe.EFDReinf;

                        var xmlReinf = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Reinf xmlns=\"http://www.reinf.esocial.gov.br/schemas/envioLoteEventosAssincrono/v1_00_00\"><envioLoteEventos><ideContribuinte><tpInsc>1</tpInsc><nrInsc>00000000000000</nrInsc></ideContribuinte><eventos><evento Id=\"ID1000000000000002021052608080800001\">";
                        xmlReinf += xmlDoc.GetElementsByTagName("Reinf")[0].OuterXml;
                        xmlReinf += "</evento></eventos></envioLoteEventos></Reinf>";
                        var servicoRecepcionarLoteReinf = new ServicosEFDReinf.RecepcionarLoteAssincrono(xmlReinf, configuracao);

                        xmlDoc = servicoRecepcionarLoteReinf.ConteudoXMLAssinado;
                        ValidarXmlEventoEFDReinf(tipoXML, configuracao, validarSchema, retornoArquivo, arquivoXML, xmlDoc, emp);
                        jaValidou = true;

                        break;

                    #endregion XML do EFD Reinf

                    default:
                        return false;
                }

                if (!jaValidou)
                {
                    Validar(tipoXML, configuracao, validarSchema, retornoArquivo, xmlSalvar, arquivoXML, xmlDoc, emp);
                }
            }
            catch (XmlException ex)
            {
                var erro = ex.GetAllMessages();
                if (retornoArquivo)
                {
                    GravarXMLRetornoValidacao(arquivoXML, "3", erro);
                    new Auxiliar().MoveArqErro(arquivoXML);
                }
                else
                {
                    throw new Exception(erro);
                }
            }
            catch (Exception ex)
            {
                var erro = ex.GetAllMessages();

                if (retornoArquivo)
                {
                    GravarXMLRetornoValidacao(arquivoXML, "3", erro);
                    new Auxiliar().MoveArqErro(arquivoXML);
                }
                else
                {
                    throw new Exception(erro);
                }
            }
            finally
            {

                if (warnings != null && warnings.Count > 0 && Empresas.Configuracoes[emp].GravarWarnings)
                {
                    GravarXmlRetornoWarnings(arquivoXML, warnings);
                }
            }
            return true;
        }
        #endregion Public Methods
    }
}