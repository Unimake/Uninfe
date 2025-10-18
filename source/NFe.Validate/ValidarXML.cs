using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Unimake.Business.DFe.Servicos;

namespace NFe.Validate
{
    /// <summary>
    /// Classe de validação dos XML´s
    /// </summary>
    public class ValidarXML
    {
        #region Construtores

        public ValidarXML() { }

        public ValidarXML(string arquivoXML, int UFCod, bool soValidar)
        {
            TipoArqXml = new TipoArquivoXML(arquivoXML, UFCod, soValidar);
        }

        public ValidarXML(XmlDocument conteudoXML, int UFCod, bool soValidar)
        {
            TipoArqXml = new TipoArquivoXML("", conteudoXML, UFCod, soValidar);
        }

        /// <summary>
        /// Construtor somente para gravar um XML de retorno de erro de validação, não use este construtor para fins de validação pois vai faltar conteúdo para outro fim.
        /// </summary>
        /// <param name="Arquivo"></param>
        /// <param name="cStat"></param>
        /// <param name="xMotivo"></param>
        public ValidarXML(string arquivo, string xMotivo)
        {
            GravarXMLRetornoValidacao(arquivo, "5", xMotivo);
            new Auxiliar().MoveArqErro(arquivo);
        }

        #endregion Construtores

        public TipoArquivoXML TipoArqXml = null;

        public int Retorno { get; private set; }
        public string RetornoString { get; private set; }

        /// <summary>
        /// Pasta dos schemas para validação do XML
        /// </summary>
        private readonly string PastaSchema = Propriedade.PastaSchemas;

        private string cErro;




        /// <summary>
        /// Método responsável por validar a estrutura do XML de acordo com o schema passado por parâmetro
        /// </summary>
        /// <param name="rotaArqXML">XML a ser validado</param>
        /// <param name="cRotaArqSchema">Schema a ser utilizado na validação</param>
        /// <param name="nsSchema">Namespace contendo a URL do schema</param>
        private void Validar(string rotaArqXML)
        {
            var lArqXML = File.Exists(rotaArqXML);

            if (File.Exists(rotaArqXML))
            {
                var doc = new XmlDocument();
                try
                {
                    doc.Load(rotaArqXML);
                }
                catch
                {
                    doc.LoadXml(System.IO.File.ReadAllText(rotaArqXML, System.Text.Encoding.UTF8));
                }
                Validar(doc, rotaArqXML);
            }
            else
            {
                Retorno = 2;
                RetornoString = "Arquivo XML não foi encontrato";
            }
        }

        /// <summary>
        /// Método responsável por validar a estrutura do XML de acordo com o schema passado por parâmetro
        /// </summary>
        /// <param name="rotaArqXML">Nome do arquivo XML</param>
        /// <param name="conteudoXML">Conteúdo do XML a ser validado</param>
        private void Validar(XmlDocument conteudoXML, string rotaArqXML)
        {
            Retorno = 0;
            RetornoString = "";

            var temXSD = !string.IsNullOrEmpty(TipoArqXml.cArquivoSchema);

            if (File.Exists(TipoArqXml.cArquivoSchema))
            {
                XmlReader xmlReader = null;

                try
                {
                    ValidarInformacaoContingencia(conteudoXML);


                    var settings = new XmlReaderSettings
                    {
                        ValidationType = ValidationType.Schema
                    };

                    var schemas = new XmlSchemaSet();
                    settings.Schemas = schemas;

                    /* Se dentro do XSD houver referência a outros XSDs externos, pode ser necessário ter certas permissões para localizá-lo.
                     * Usa um "Resolver" com as credencias-padrão para obter esse recurso externo. */
                    var resolver = new XmlUrlResolver
                    {
                        Credentials = System.Net.CredentialCache.DefaultCredentials
                    };
                    /* Informa à configuração de leitura do XML que deve usar o "Resolver" criado acima e que a validação deve respeitar
                     * o esquema informado no início. */
                    settings.XmlResolver = resolver;

                    if (TipoArqXml.TargetNameSpace != string.Empty)
                    {
                        schemas.Add(TipoArqXml.TargetNameSpace, TipoArqXml.cArquivoSchema);
                    }
                    else
                    {
                        schemas.Add(NFeStrConstants.NAME_SPACE_NFE, TipoArqXml.cArquivoSchema);
                    }

                    settings.ValidationEventHandler += new ValidationEventHandler(reader_ValidationEventHandler);

                    xmlReader = XmlReader.Create(new StringReader(conteudoXML.OuterXml), settings);

                    cErro = "";
                    try
                    {
                        while (xmlReader.Read()) { }
                    }
                    catch (Exception ex)
                    {
                        cErro = ex.Message;
                    }

                    xmlReader.Close();
                }
                catch (Exception ex)
                {
                    if (xmlReader != null)
                    {
                        xmlReader.Close();
                    }

                    cErro = ex.Message + "\r\n";
                }

                if (cErro != "")
                {
                    Retorno = 1;
                    RetornoString = "Início da validação...\r\n\r\n";
                    RetornoString += "Arquivo XML: " + rotaArqXML + "\r\n";
                    RetornoString += "Arquivo SCHEMA: " + TipoArqXml.cArquivoSchema + "\r\n\r\n";
                    RetornoString += cErro;
                    RetornoString += "\r\n...Final da validação";
                }
            }
            else if (temXSD)
            {
                Retorno = 3;
                RetornoString = "Arquivo XSD (schema) não foi encontrado em " + TipoArqXml.cArquivoSchema;
            }
        }

        private void reader_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            cErro += "Linha: " + e.Exception.LineNumber + " Coluna: " + e.Exception.LinePosition + " Erro: " + e.Exception.Message + "\r\n";
        }

        #region ValidarArqXML()

        /// <summary>
        /// Valida o arquivo XML
        /// </summary>
        /// <param name="arquivo">Nome do arquivo XML a ser validado</param>
        /// <returns>
        /// Se retornar uma string em branco, significa que o XML foi
        /// validado com sucesso, ou seja, não tem nenhum erro. Se o retorno
        /// tiver algo, algum erro ocorreu na validação.
        /// </returns>
        public string ValidarArqXML(string arquivo)
        {
            var cRetorna = "";

            if (TipoArqXml.nRetornoTipoArq >= 1 && TipoArqXml.nRetornoTipoArq <= SchemaXML.MaxID)
            {
                Validar(arquivo);
                if (Retorno != 0)
                {
                    cRetorna = "XML INCONSISTENTE!\r\n\r\n" + RetornoString;
                }
            }
            else
            {
                cRetorna = "XML INCONSISTENTE!\r\n\r\n" + TipoArqXml.cRetornoTipoArq;
            }

            return cRetorna;
        }

        #endregion ValidarArqXML()

        #region ValidarArqXML()

        /// <summary>
        /// Valida o arquivo XML
        /// </summary>
        /// <param name="conteudoXML">Conteudo do XML a ser validado</param>
        /// <param name="arquivo">Nome do arquivo XML que será validado</param>
        /// <returns>
        /// Se retornar uma string em branco, significa que o XML foi
        /// validado com sucesso, ou seja, não tem nenhum erro. Se o retorno
        /// tiver algo, algum erro ocorreu na validação.
        /// </returns>
        public string ValidarArqXML(XmlDocument conteudoXML, string arquivo)
        {
            var cRetorna = "";

            if (TipoArqXml.nRetornoTipoArq >= 1 && TipoArqXml.nRetornoTipoArq <= SchemaXML.MaxID)
            {
                Validar(conteudoXML, arquivo);

                if (Retorno != 0)
                {
                    cRetorna = "XML INCONSISTENTE!\r\n\r\n" + RetornoString;
                }
            }
            else
            {
                cRetorna = "XML INCONSISTENTE!\r\n\r\n" + TipoArqXml.cRetornoTipoArq;
            }

            return cRetorna;
        }

        #endregion ValidarArqXML()

        #region ValidarAssinarXML()

        /// <summary>
        /// Assina digitalmente, valida contra o schema XSD correspondente e, por fim, gerencia os arquivos de resultado.
        /// </summary>
        /// <param name="arquivo">Nome do arquivo XML a ser validado e assinado</param>
        /// <para>
        /// Histórico: Este método é uma refatoração do original criado por Wandrey Mundin Ferreira (28/05/2009)
        /// para utilizar a nova biblioteca de assinatura estática da DLL.
        /// </para>
        public void ValidarAssinarXML(string arquivo)
        {
            var emp = Empresas.FindEmpresaByThread();
            var assinou = false;
            XmlDocument conteudoXML = null;
            try
            {
                conteudoXML = new XmlDocument();
                conteudoXML.PreserveWhitespace = true;
                conteudoXML.Load(arquivo);

                if (Empresas.Configuracoes[emp].Servico == TipoAplicativo.Nfse && Empresas.Configuracoes[emp].UnidadeFederativaCodigo == 3550308)
                {
                    Unimake.Business.DFe.Utility.XMLUtility.EncryptTagAssinaturaNFSe(PadraoNFSe.PAULISTANA, conteudoXML, Empresas.Configuracoes[emp].X509Certificado);
                }

                var respTecnico = new RespTecnico(Empresas.Configuracoes[emp].RespTecCNPJ,
                    Empresas.Configuracoes[emp].RespTecXContato,
                    Empresas.Configuracoes[emp].RespTecEmail,
                    Empresas.Configuracoes[emp].RespTecTelefone,
                    Empresas.Configuracoes[emp].RespTecIdCSRT,
                    Empresas.Configuracoes[emp].RespTecCSRT);

                bool salvaXML = respTecnico.AdicionarResponsavelTecnico(conteudoXML);
                if (salvaXML)
                    conteudoXML.Save(arquivo);

                if (TipoArqXml.nRetornoTipoArq >= 1 && TipoArqXml.nRetornoTipoArq <= SchemaXML.MaxID)
                {
                    // var tipoArquivo = new TipoArquivoXML(arquivo, Empresas.Configuracoes[emp].UnidadeFederativaCodigo, false);
                    X509Certificate2 certificado = Empresas.Configuracoes[emp].X509Certificado;

                    if (TipoArqXml.TargetNameSpace.Contains("reinf") &&
                       (TipoArqXml.TargetNameSpace.Contains("envioLoteEventos") || TipoArqXml.TargetNameSpace.Contains("envioLoteEventosAssincrono"))) // Lote de eventos do EFDReinf
                    {

                        AssinarLote("Reinf", conteudoXML, certificado);
                    }
                    else if (TipoArqXml.TargetNameSpace.Contains("esocial") && TipoArqXml.TargetNameSpace.Contains("lote/eventos")) // Lote de eventos do eSocial
                    {
                        AssinarLote("eSocial", conteudoXML, certificado, false);
                    }
                    else if (TipoArqXml.TagAssinatura == "Reinf")
                    {
                        Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, TipoArqXml.TagAssinatura, TipoArqXml.TagAtributoId, certificado, Unimake.Business.DFe.Security.AlgorithmType.Sha256);
                    }
                    else if (TipoArqXml.TagAssinatura == "eSocial")
                    {
                        Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, TipoArqXml.TagAssinatura, TipoArqXml.TagAtributoId, certificado, Unimake.Business.DFe.Security.AlgorithmType.Sha256, false);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(TipoArqXml.TagAssinatura))
                        {
                            Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, TipoArqXml.TagAssinatura, TipoArqXml.TagAtributoId, certificado);
                        }

                        if (!string.IsNullOrEmpty(TipoArqXml.TagAssinatura0))
                        {
                            Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, TipoArqXml.TagAssinatura0, TipoArqXml.TagAtributoId0, certificado);
                        }

                        // Assinar o lote, se existir
                        if (!string.IsNullOrEmpty(TipoArqXml.TagLoteAssinatura))
                        {
                            Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, TipoArqXml.TagLoteAssinatura, TipoArqXml.TagLoteAtributoId, certificado);
                        }
                    }
                    conteudoXML.Save(arquivo);
                    assinou = true;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    GravarXMLRetornoValidacao(arquivo, "2", "Ocorreu um erro ao assinar o XML: " + ex.Message);
                    new Auxiliar().MoveArqErro(arquivo);
                }
                catch
                {
                    //Se deu algum erro na hora de gravar o retorno do erro para o ERP, infelizmente não posso fazer nada.
                    //Isso pode acontecer se falhar rede, hd, problema de permissão de pastas, etc... Wandrey 23/03/2010
                }
            }

            if (assinou)
            {
                // Validar o Arquivo XML
                if (TipoArqXml.nRetornoTipoArq >= 1 && TipoArqXml.nRetornoTipoArq <= SchemaXML.MaxID)
                {
                    try
                    {
                        Validar(conteudoXML, arquivo);
                        if (Retorno != 0)
                        {
                            GravarXMLRetornoValidacao(arquivo, "3", "Ocorreu um erro ao validar o XML: " + RetornoString);
                            new Auxiliar().MoveArqErro(arquivo);
                        }
                        else
                        {
                            if (!Directory.Exists(Empresas.Configuracoes[emp].PastaValidado))
                            {
                                Directory.CreateDirectory(Empresas.Configuracoes[emp].PastaValidado);
                            }

                            var ArquivoNovo = Empresas.Configuracoes[emp].PastaValidado + "\\" + Path.GetFileName(arquivo);

                            Functions.Move(arquivo, ArquivoNovo);

                            GravarXMLRetornoValidacao(arquivo, "1", "XML assinado e validado com sucesso.");
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            GravarXMLRetornoValidacao(arquivo, "4", "Ocorreu um erro ao validar o XML: " + ex.Message);
                            new Auxiliar().MoveArqErro(arquivo);
                        }
                        catch
                        {
                            //Se deu algum erro na hora de gravar o retorno do erro para o ERP, infelizmente não posso fazer nada.
                            //Isso pode acontecer se falhar rede, hd, problema de permissão de pastas, etc... Wandrey 23/03/2010
                        }
                    }
                }
                else
                {
                    if (TipoArqXml.nRetornoTipoArq == -1)
                    {
                        ///
                        /// OPS!!! Arquivo de NFS-e enviado p/ a pasta de validação, mas não existe definicao de schemas p/ sua validacao
                        ///
                        GravarXMLRetornoValidacao(arquivo, "6", "XML não validado contra o schema da prefeitura. XML: " + TipoArqXml.cRetornoTipoArq);
                        new Auxiliar().MoveArqErro(arquivo);
                    }
                    else
                    {
                        try
                        {
                            GravarXMLRetornoValidacao(arquivo, "5", "Ocorreu um erro ao validar o XML: " + TipoArqXml.cRetornoTipoArq);
                            new Auxiliar().MoveArqErro(arquivo);
                        }
                        catch
                        {
                            //Se deu algum erro na hora de gravar o retorno do erro para o ERP, infelizmente não posso fazer nada.
                            //Isso pode acontecer se falhar rede, hd, problema de permissão de pastas, etc... Wandrey 23/03/2010
                        }
                    }
                }
            }

        }

        public void AssinarLote(string tagName, XmlDocument conteudoXML, X509Certificate2 certificado, bool definirURI = true)
        {
            var eventoNodeList = conteudoXML.GetElementsByTagName("evento");

            foreach (XmlNode eventoNode in eventoNodeList)
            {
                var operacaoNodeList = ((XmlElement)eventoNode).GetElementsByTagName(tagName);
                if (operacaoNodeList.Count > 0)
                {

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(operacaoNodeList[0].OuterXml);

                    Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(xmlDoc, tagName, tagName, certificado, Unimake.Business.DFe.Security.AlgorithmType.Sha256, definirURI);

                    var newNode = conteudoXML.ImportNode(xmlDoc.DocumentElement, true);
                    eventoNode.RemoveChild(operacaoNodeList[0]);
                    eventoNode.AppendChild(newNode);
                }
            }
        }

        #endregion ValidarAssinarXML()

        #region GravarXMLRetornoValidacao()

        /// <summary>
        /// Na tentativa de somente validar ou assinar o XML se encontrar um erro vai ser retornado um XML para o ERP
        /// </summary>
        /// <param name="arquivo">Nome do arquivo XML validado</param>
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
                new XElement(TpcnResources.cStat.ToString(), cStat),
                new XElement(TpcnResources.xMotivo.ToString(), xMotivo)));
            xml.Save(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + arquivoRetorno);
        }

        #endregion GravarXMLRetornoValidacao()

        public void ValidarInformacaoContingencia(XmlDocument conteudoXML)
        {
            var tipoServico = conteudoXML.DocumentElement.Name;

            if (!string.IsNullOrEmpty(tipoServico))
            {
                if (tipoServico.Equals("NFe") || tipoServico.Equals("CTe"))
                {
                    var tipoEmissao = conteudoXML.GetElementsByTagName("tpEmis")[0]?.InnerText;

                    if (!string.IsNullOrEmpty(tipoEmissao))
                    {
                        var tpEmissao = (TipoEmissao)Convert.ToInt32(tipoEmissao);

                        switch (tpEmissao)
                        {
                            case TipoEmissao.ContingenciaFSIA:
                            case TipoEmissao.ContingenciaFSDA:
                            case TipoEmissao.ContingenciaOffLine:
                                var dhCont = conteudoXML.GetElementsByTagName("dhCont")[0]?.InnerText;
                                var xJust = conteudoXML.GetElementsByTagName("xJust")[0]?.InnerText;

                                if (string.IsNullOrEmpty(dhCont) || string.IsNullOrEmpty(xJust))
                                {
                                    throw new Exception("XML em contingência e não foi informado a data, hora e justificativa da entrada em contingência, tags <dhCont> e <xJust>.");
                                }

                                break;
                        }
                    }
                }
            }
        }
    }
}