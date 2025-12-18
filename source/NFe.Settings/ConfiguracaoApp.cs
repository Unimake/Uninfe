using NFe.Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Unimake.Business.DFe.Security;

namespace NFe.Settings
{
    #region Classe ConfiguracaoApp
    /// <summary>
    /// Classe responsável por realizar algumas tarefas na parte de configurações da aplicação.
    /// Arquivo de configurações: UniNfeConfig.xml
    /// </summary>
    public class ConfiguracaoApp
    {
        #region NfeConfiguracoes

        /// <summary>
        /// Enumerador com as tags do xml nfe_Configuracoes
        /// </summary>
        private enum NfeConfiguracoes
        {
            Proxy = 0,
            ProxyServidor,
            ProxyUsuario,
            ProxySenha,
            ProxyPorta,
            SenhaConfig,
            ChecarConexaoInternet,
            GravarLogOperacaoRealizada,
            DetectarProxyAuto,
            ConfirmaSaida,
            ManterAtualizado,
            NaoMostrarNovamente,                                 //Checkbox "Não mostrar novamente" do formulário sobre atualizar automaticamente
            AppID,                                              //EBank
            Secret,                                             //EBank            
            ChecarCNPJCPFCertificado
        }

        #endregion NfeConfiguracoes

        #region Propriedades

        #region ChecarConexaoInternet

        public static bool ChecarConexaoInternet { get; set; }

        #endregion ChecarConexaoInternet

        #region GravarLogOperacoesRealizadas

        public static bool GravarLogOperacoesRealizadas { get; set; }

        #endregion GravarLogOperacoesRealizadas

        #region Propriedades para controle de servidor proxy

        public static bool Proxy { get; set; }
        public static bool DetectarConfiguracaoProxyAuto { get; set; }
        public static string ProxyServidor { get; set; }
        public static string ProxyUsuario { get; set; }
        public static string ProxySenha { get; set; }
        public static int ProxyPorta { get; set; }

        #endregion Propriedades para controle de servidor proxy

        #region Propriedades para tela de sobre

        public static string NomeEmpresa { get; set; }
        public static string Site { get; set; }
        public static string SiteProduto { get; set; }
        public static string Email { get; set; }

        #endregion Propriedades para tela de sobre

        #region SenhaConfig

        public static string SenhaConfig { get; set; }
        public static string AppID { get; set; }
        public static string Secret { get; set; }

        #endregion SenhaConfig

        #region Prorpiedades utilizadas no inicio do sistema

        public static Stopwatch ExecutionTime { get; set; }
        public static bool ConfirmaSaida { get; set; }
        public static bool ManterAtualizado { get; set; }
        public static bool NaoMostrarNovamente { get; set; }

        /// <summary>
        /// UniNFe foi executado com o parâmetro /silent, ou seja, modo silencioso
        /// </summary>
        public static bool ExecutadoModoSilencioso { get; set; }

        #endregion Prorpiedades utilizadas no inicio do sistema

        #endregion Propriedades

        #region Métodos gerais

        public static bool ExtractResourceToDisk(System.Reflection.Assembly ass, string s, string fileoutput)
        {
            var extraido = false;
            using (var FileReader = new StreamReader(ass.GetManifestResourceStream(s)))
            {
                if (!Directory.Exists(Path.GetDirectoryName(fileoutput)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileoutput));
                }

                using (var FileWriter = new StreamWriter(fileoutput))
                {
                    FileWriter.Write(FileReader.ReadToEnd());
                    FileWriter.Close();
                    extraido = true;
                }
            }
            return extraido;
        }

        #region Extrae os arquivos necessarios a executacao

        internal class loadResources
        {
            public string cErros { get; private set; }

            #region load()

            /// <summary>
            /// Exporta os WSDLs e Schemas da DLL para as pastas do UniNFe
            /// </summary>
            public void load()
            {
                if (Empresas.Configuracoes.Count == 0)
                {
                    ///
                    /// OPS!!! nenhuma empresa ainda cadastrada, então gravamos o log na pasta de log do uninfe
                    ConfiguracaoApp.GravarLogOperacoesRealizadas = true;
                }

                Propriedade.Estados = null;

                try
                {
                    var ass = Assembly.LoadFile(Propriedade.PastaExecutavel + "\\NFe.Components.Wsdl.dll");
                    var x = ass.GetManifestResourceNames();
                    if (x.GetLength(0) > 0)
                    {
                        string fileoutput = null;
                        var okFiles = new List<string>();

                        var afiles = (from d in x
                                      where d.StartsWith("NFe.Components.Wsdl.NF")
                                      select d);

                        foreach (var s in afiles)
                        {
                            fileoutput = s.Replace("NFe.Components.Wsdl.", Propriedade.PastaExecutavel + "\\");
                            if (fileoutput == null)
                            {
                                continue;
                            }

                            if (fileoutput.ToLower().EndsWith(".xsd"))
                            {
                                /// Ex: NFe.Components.Wsdl.NFe.NFe.xmldsig-core-schema_v1.01.xsd
                                ///
                                /// pesquisa pelo nome do XSD
                                var plast = fileoutput.ToLower().LastIndexOf("_v");
                                if (plast == -1)
                                {
                                    plast = fileoutput.IndexOf(".xsd") - 1;
                                }

                                while (fileoutput[plast] != '.' && plast >= 0)
                                {
                                    --plast;
                                }

                                var fn = fileoutput.Substring(plast + 1);
                                fileoutput = fileoutput.Substring(0, plast).Replace(".", "\\") + "\\" + fn;
                            }
                            else
                            {
                                fileoutput = (fileoutput.Substring(0, fileoutput.LastIndexOf('.')) + "####" +
                                                fileoutput.Substring(fileoutput.LastIndexOf('.') + 1)).Replace(".", "\\").Replace("####", ".");
                            }

                            ExtractResourceToDisk(ass, s, fileoutput);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var xMotivo = "Não foi possível atualizar pacotes de Schemas/WSDLs.";

                    Auxiliar.WriteLog(cErros = xMotivo + Environment.NewLine + ex.Message, false);

                    if (Empresas.Configuracoes.Count > 0)
                    {
                        var emp = Empresas.FindEmpresaByThread();
                        var oAux = new Auxiliar();
                        oAux.GravarArqErroERP(Empresas.Configuracoes[emp].CNPJ + ".err", cErros);
                    }
                }
            }

            #endregion load()
        }

        #endregion Extrae os arquivos necessarios a executacao

        #region StartVersoes

        public static void StartVersoes()
        {
            new loadResources().load();

            ConfiguracaoApp.CarregarDados();
            //ConfiguracaoApp.DownloadArquivoURLConsultaDFe();

            if (!Propriedade.ServicoRodando || Propriedade.ExecutandoPeloUniNFe)
            {
                ConfiguracaoApp.CarregarDadosSobre();
            }

            try
            {
                SchemaXML.CriarListaIDXML();
            }
            catch (Exception ex)
            {
                ///
                /// essa mensagem nunca será exibida ao usuário, porque se ela for exibida, você terá que ajustar
                ///
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        #endregion StartVersoes

        #region CarregarDados()

        /// <summary>
        /// Carrega as configurações realizadas na Aplicação gravadas no XML UniNfeConfig.xml para
        /// propriedades, para facilitar a leitura das informações necessárias para as transações da NF-e.
        /// </summary>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// </remarks>
        public static void CarregarDados()
        {
            var vArquivoConfig = Propriedade.PastaExecutavel + "\\" + Propriedade.NomeArqConfig;
            XmlDocument doc = null;
            if (File.Exists(vArquivoConfig))
            {
                try
                {
                    doc = new XmlDocument();
                    doc.Load(vArquivoConfig);

                    XmlNodeList configList = null;

                    configList = doc.GetElementsByTagName(NFeStrConstants.nfe_configuracoes);

                    foreach (XmlNode nodeConfig in configList)
                    {
                        var elementConfig = (XmlElement)nodeConfig;

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.DetectarProxyAuto.ToString())[0] != null)
                        {
                            ConfiguracaoApp.DetectarConfiguracaoProxyAuto = Convert.ToBoolean(elementConfig[NfeConfiguracoes.DetectarProxyAuto.ToString()].InnerText);
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.Proxy.ToString())[0] != null)
                        {
                            ConfiguracaoApp.Proxy = Convert.ToBoolean(elementConfig[NfeConfiguracoes.Proxy.ToString()].InnerText);
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.ProxyServidor.ToString())[0] != null)
                        {
                            ConfiguracaoApp.ProxyServidor = elementConfig[NfeConfiguracoes.ProxyServidor.ToString()].InnerText.Trim();
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.ProxyUsuario.ToString())[0] != null)
                        {
                            ConfiguracaoApp.ProxyUsuario = elementConfig[NfeConfiguracoes.ProxyUsuario.ToString()].InnerText.Trim();
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.ProxySenha.ToString())[0] != null)
                        {
                            ConfiguracaoApp.ProxySenha = Criptografia.descriptografaSenha(elementConfig[NfeConfiguracoes.ProxySenha.ToString()].InnerText.Trim());
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.ProxyPorta.ToString())[0] != null)
                        {
                            ConfiguracaoApp.ProxyPorta = Convert.ToInt32(elementConfig[NfeConfiguracoes.ProxyPorta.ToString()].InnerText.Trim());
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.SenhaConfig.ToString())[0] != null)
                        {
                            ConfiguracaoApp.SenhaConfig = elementConfig[NfeConfiguracoes.SenhaConfig.ToString()].InnerText.Trim();
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.ChecarConexaoInternet.ToString())[0] != null)
                        {
                            ConfiguracaoApp.ChecarConexaoInternet = Convert.ToBoolean(elementConfig[NfeConfiguracoes.ChecarConexaoInternet.ToString()].InnerText);
                        }
                        else
                        {
                            ConfiguracaoApp.ChecarConexaoInternet = true;
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.GravarLogOperacaoRealizada.ToString())[0] != null)
                        {
                            ConfiguracaoApp.GravarLogOperacoesRealizadas = Convert.ToBoolean(elementConfig[NfeConfiguracoes.GravarLogOperacaoRealizada.ToString()].InnerText);
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.ManterAtualizado.ToString())[0] != null)
                        {
                            ConfiguracaoApp.ManterAtualizado = Convert.ToBoolean(elementConfig[NfeConfiguracoes.ManterAtualizado.ToString()].InnerText);
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.NaoMostrarNovamente.ToString())[0] != null)
                        {
                            ConfiguracaoApp.NaoMostrarNovamente = Convert.ToBoolean(elementConfig[NfeConfiguracoes.NaoMostrarNovamente.ToString()].InnerText);
                        }

                        if (elementConfig.GetElementsByTagName(NfeConfiguracoes.ConfirmaSaida.ToString())[0] != null)
                        {
                            ConfiguracaoApp.ConfirmaSaida = Convert.ToBoolean(elementConfig[NfeConfiguracoes.ConfirmaSaida.ToString()].InnerText);
                        }
                        else
                        {
                            ConfiguracaoApp.ConfirmaSaida = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ///
                    /// danasa 8-2009
                    /// como reportar ao usuario que houve erro de leitura do arquivo de configuracao?
                    /// tem um usuário que postou um erro de leitura deste arquivo e não sabia como resolver.
                    ///
                    ///
                    /// danasa 8-2009
                    ///
                    if (!Propriedade.ServicoRodando || Propriedade.ExecutandoPeloUniNFe)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                finally
                {
                    if (doc != null)
                    {
                        doc = null;
                    }
                }
            }
            else
            {
                ChecarConexaoInternet = true;
            }
            //Carregar a lista de webservices disponíveis
            try
            {
                Functions.CarregarMunicipio();
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog(ex.Message, false);
            }

        }

        #endregion CarregarDados()

        #region CarregarDadosSobre()

        /// <summary>
        /// Carrega informações da tela de sobre
        /// </summary>
        /// <remarks>
        /// Autor: Leandro Souza
        /// </remarks>
        public static void CarregarDadosSobre()
        {
            var vArquivoConfig = Propriedade.PastaExecutavel + "\\" + Propriedade.NomeArqConfigSobre;

            //ConfiguracaoApp.NomeEmpresa = "Unimake Software";
            //ConfiguracaoApp.Site = "www.unimake.com.br";
            //ConfiguracaoApp.SiteProduto = ConfiguracaoApp.Site + "/uninfe";
            //ConfiguracaoApp.Email = "nfe@unimake.com.br";

            if (File.Exists(vArquivoConfig))
            {
                XmlTextReader oLerXml = null;
                try
                {
                    //Carregar os dados do arquivo XML de configurações da Aplicação
                    oLerXml = new XmlTextReader(vArquivoConfig);

                    while (oLerXml.Read())
                    {
                        if (oLerXml.NodeType == XmlNodeType.Element)
                        {
                            if (oLerXml.Name == NFeStrConstants.nfe_configuracoes)
                            {
                                while (oLerXml.Read())
                                {
                                    if (oLerXml.NodeType == XmlNodeType.Element)
                                    {
                                        if (oLerXml.Name == "NomeEmpresa") { oLerXml.Read(); ConfiguracaoApp.NomeEmpresa = oLerXml.Value; }
                                        else if (oLerXml.Name == "Site") { oLerXml.Read(); ConfiguracaoApp.Site = oLerXml.Value.Trim(); }
                                        else if (oLerXml.Name == "SiteProduto") { oLerXml.Read(); ConfiguracaoApp.SiteProduto = oLerXml.Value.Trim(); }
                                        else if (oLerXml.Name == "Email") { oLerXml.Read(); ConfiguracaoApp.Email = oLerXml.Value.Trim(); }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (oLerXml != null)
                    {
                        oLerXml.Close();
                    }
                }
            }
        }

        #endregion CarregarDadosSobre()

        #region Gravar XML com as empresas cadastradas

        public void GravarArqEmpresas()
        {
            var empresasele = new XElement("Empresa");
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));

            foreach (var empresa in Empresas.Configuracoes)
            {
                empresasele.Add(new XElement(NFeStrConstants.Registro,
                                new XAttribute(TpcnResources.CNPJ.ToString(), empresa.CNPJ),
                                new XAttribute(NFeStrConstants.Servico, ((int)empresa.Servico).ToString()),
                                new XElement(NFeStrConstants.Nome, empresa.Nome.Trim())));
            }
            xml.Add(empresasele);
            xml.Save(Propriedade.NomeArqEmpresas);
        }

        #endregion Gravar XML com as empresas cadastradas

        #region GravarConfigGeral()

        /// <summary>
        /// Gravar as configurações gerais
        /// </summary>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 30/07/2010
        /// </remarks>
        public void GravarConfigGeral(bool configuracaoPorArquivo = false)
        {
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var elementos = new XElement(NFeStrConstants.nfe_configuracoes);
            elementos.Add(new XElement(NfeConfiguracoes.DetectarProxyAuto.ToString(), ConfiguracaoApp.DetectarConfiguracaoProxyAuto.ToString()));
            elementos.Add(new XElement(NfeConfiguracoes.Proxy.ToString(), ConfiguracaoApp.Proxy.ToString()));
            elementos.Add(new XElement(NfeConfiguracoes.ProxyServidor.ToString(), ConfiguracaoApp.ProxyServidor));
            elementos.Add(new XElement(NfeConfiguracoes.ProxyUsuario.ToString(), ConfiguracaoApp.ProxyUsuario));
            elementos.Add(new XElement(NfeConfiguracoes.ProxyPorta.ToString(), ConfiguracaoApp.ProxyPorta.ToString()));
            elementos.Add(new XElement(NfeConfiguracoes.ProxySenha.ToString(), Criptografia.criptografaSenha(ConfiguracaoApp.ProxySenha)));
            elementos.Add(new XElement(NfeConfiguracoes.ChecarConexaoInternet.ToString(), ConfiguracaoApp.ChecarConexaoInternet.ToString()));
            elementos.Add(new XElement(NfeConfiguracoes.GravarLogOperacaoRealizada.ToString(), ConfiguracaoApp.GravarLogOperacoesRealizadas.ToString()));
            elementos.Add(new XElement(NfeConfiguracoes.ConfirmaSaida.ToString(), ConfiguracaoApp.ConfirmaSaida.ToString()));
            elementos.Add(new XElement(NfeConfiguracoes.ManterAtualizado.ToString(), ConfiguracaoApp.ManterAtualizado.ToString()));
            elementos.Add(new XElement(NfeConfiguracoes.NaoMostrarNovamente.ToString(), ConfiguracaoApp.NaoMostrarNovamente.ToString()));
            if (!string.IsNullOrEmpty(ConfiguracaoApp.SenhaConfig))
            {

                if (!configuracaoPorArquivo)
                {
                    ConfiguracaoApp.SenhaConfig = Functions.GerarMD5(ConfiguracaoApp.SenhaConfig);
                }

                elementos.Add(new XElement(NfeConfiguracoes.SenhaConfig.ToString(), ConfiguracaoApp.SenhaConfig));
            }
            xml.Add(elementos);
            xml.Save(Propriedade.PastaExecutavel + "\\" + Propriedade.NomeArqConfig);
        }

        #endregion GravarConfigGeral()

        #region ValidarConfig()

        internal class xValid
        {
            public bool valid;
            public string folder;
            public string msg1;
            public string msg2;

            public xValid(string _folder, string _msg1, string _msg2, bool _valid)
            {
                valid = _valid;
                msg1 = _msg1;
                msg2 = _msg2 + " - '" + (!string.IsNullOrEmpty(_folder) ? "VAZIO" : _folder) + "'";
                folder = _folder;
            }
        }

        private Dictionary<string, int> _folders;

        private string AddEmpresaNaLista(string folder)
        {
            try
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    _folders.Add(folder.ToLower(), 0);
                }

                return "";
            }
            catch
            {
                return "Não é permitido a utilização de pastas idênticas na mesma ou entre empresas.\r\nPasta utilizada: \r\n" + folder;
            }
        }

        /// <summary>
        /// Verifica se algumas das informações das configurações tem algum problema ou falha
        /// </summary>
        /// <param name="validarCertificado">Se valida se tem certificado informado ou não nas configurações</param>
        public void ValidarConfig(bool validarCertificado, Empresa empresaValidada)
        {
            var erro = string.Empty;
            var validou = true;

            _folders = new Dictionary<string, int>();

            foreach (var emp in Empresas.Configuracoes)
            {
                #region Remover End Slash e Corrigir caminho absoluto

                emp.RemoveEndSlash();
                emp.FixAbsolutePath();

                #endregion

                #region Verificar a duplicação de nome de pastas que não pode existir

                if ((erro = AddEmpresaNaLista(emp.PastaXmlEnvio)) == "")
                {
                    if ((erro = AddEmpresaNaLista(emp.PastaXmlRetorno)) == "")
                    {
                        if ((erro = AddEmpresaNaLista(emp.PastaXmlErro)) == "")
                        {
                            if ((erro = AddEmpresaNaLista(emp.PastaValidar)) == "")
                            {
                                if ((erro = AddEmpresaNaLista(emp.PastaXmlEnviado)) == "")
                                {
                                    if (emp.Servico != TipoAplicativo.Nfse)
                                    {
                                        if ((erro = AddEmpresaNaLista(emp.PastaXmlEmLote)) == "")
                                        {
                                            if ((erro = AddEmpresaNaLista(emp.PastaBackup)) == "")
                                            {
                                                erro = AddEmpresaNaLista(emp.PastaDownloadNFeDest);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (IsPastaEnviadoPath(emp.PastaXmlRetorno, emp.PastaXmlEnviado))
                {
                    erro += "\r\nNão é permitido informar o conteúdo da pasta enviados na pasta de retorno.";
                }

                if (emp.Servico != TipoAplicativo.Nfse && !string.IsNullOrWhiteSpace(emp.PastaXmlEnviado))
                {
                    if (IsPastaEnviadoPath(emp.PastaXmlErro, emp.PastaXmlEnviado))
                    {
                        erro += "\r\nNão é permitido informar o conteúdo da pasta enviados na pasta de xml com erros.";
                    }
                }

                if (erro != "")
                {
                    erro += "\r\nNa empresa: " + emp.Nome + "\r\n" + emp.CNPJ;
                    validou = false;
                    break;
                }

                #endregion Verificar a duplicação de nome de pastas que não pode existir
            }

            if (validou)
            {
                var empFrom = 0;
                var empTo = Empresas.Configuracoes.Count;

                if (empresaValidada != null)
                {
                    ///
                    /// quando alterada uma configuracao pelo visual, valida apenas a empresa sendo alterada
                    ///
                    empFrom = empTo = Empresas.FindConfEmpresaIndex(empresaValidada.CNPJ, empresaValidada.Servico);
                    if (empFrom == -1)
                    {
                        throw new Exception("Não foi possivel encontrar a empresa para validação");
                    }

                    ++empTo;

                    if (empresaValidada.Servico == TipoAplicativo.NFCe)
                    {
                        if (!string.IsNullOrEmpty(empresaValidada.IdentificadorCSC) && string.IsNullOrEmpty(empresaValidada.TokenCSC))
                        {
                            throw new Exception("É obrigatório informar o IDToken quando informado o CSC.");
                        }
                        else if (string.IsNullOrEmpty(empresaValidada.IdentificadorCSC) && !string.IsNullOrEmpty(empresaValidada.TokenCSC))
                        {
                            throw new Exception("É obrigatório informar o CSC quando informado o IDToken.");
                        }
                    }
                }

                for (var i = empFrom; i < empTo; i++)
                {
                    var empresa = Empresas.Configuracoes[i];

                    var xNomeCNPJ = "\r\n" + empresa.Nome + "\r\n" + empresa.CNPJ;

                    #region Verificar se tem alguma pasta em branco

                    var _xValids = new List<xValid>();

                    switch (empresa.Servico)
                    {
                        case TipoAplicativo.Nfse:
                            _xValids.Add(new xValid(empresa.PastaXmlEnvio, "Informe a pasta de envio dos arquivos XML.", "A pasta de envio dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlRetorno, "Informe a pasta de envio dos arquivos XML.", "A pasta de retorno dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlErro, "Informe a pasta para arquivamento temporário dos arquivos XML que apresentaram erros.", "A pasta para arquivamento temporário dos arquivos XML com erro informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaValidar, "Informe a pasta onde será gravado os arquivos XML somente para ser validado pela aplicação.", "A pasta para validação de XML´s informada não existe.", true));
                            //_xValids.Add(new xValid(empresa.PastaXmlEnviado, "Informe a pasta para arquivamento dos arquivos XML enviados.", "A pasta para arquivamento dos arquivos XML enviados informada não existe.", true));
                            break;

                        case TipoAplicativo.EFDReinf:
                        case TipoAplicativo.eSocial:
                        case TipoAplicativo.EFDReinfeSocial:
                            _xValids.Add(new xValid(empresa.PastaXmlEnvio, "Informe a pasta de envio dos arquivos XML.", "A pasta de envio dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlRetorno, "Informe a pasta de envio dos arquivos XML.", "A pasta de retorno dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlErro, "Informe a pasta para arquivamento temporário dos arquivos XML que apresentaram erros.", "A pasta para arquivamento temporário dos arquivos XML com erro informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaValidar, "Informe a pasta onde será gravado os arquivos XML somente para ser validado pela aplicação.", "A pasta para validação de XML´s informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlEnviado, "Informe a pasta para arquivamento dos arquivos XML enviados.", "A pasta para arquivamento dos arquivos XML enviados informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaBackup, "", "A pasta para backup dos XML enviados informada não existe.", false));
                            break;

                        case TipoAplicativo.NF3e:
                        case TipoAplicativo.NFCom:
                            _xValids.Add(new xValid(empresa.PastaXmlEnvio, "Informe a pasta de envio dos arquivos XML.", "A pasta de envio dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlRetorno, "Informe a pasta de envio dos arquivos XML.", "A pasta de retorno dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlErro, "Informe a pasta para arquivamento temporário dos arquivos XML que apresentaram erros.", "A pasta para arquivamento temporário dos arquivos XML com erro informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaValidar, "Informe a pasta onde será gravado os arquivos XML somente para ser validado pela aplicação.", "A pasta para validação de XML´s informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlEnviado, "Informe a pasta para arquivamento dos arquivos XML enviados.", "A pasta para arquivamento dos arquivos XML enviados informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaBackup, "", "A pasta para backup dos XML enviados informada não existe.", false));
                            _xValids.Add(new xValid(empresa.PastaDanfeMon, "", "A pasta informada para gravação do XML da NFe para o DANFeMon não existe.", false));
                            _xValids.Add(new xValid(empresa.PastaExeUniDanfe, "", "A pasta do executável do UniDANFe informada não existe.", false));
                            _xValids.Add(new xValid(empresa.PastaConfigUniDanfe, "", "A pasta do arquivo de configurações do UniDANFe informada não existe.", false));
                            break;

                        case TipoAplicativo.Nfe:
                        case TipoAplicativo.Cte:
                        case TipoAplicativo.MDFe:
                        case TipoAplicativo.NFCe:
                        case TipoAplicativo.SATeMFE:
                        case TipoAplicativo.Todos:
                        case TipoAplicativo.Nulo:
                        default:
                            _xValids.Add(new xValid(empresa.PastaXmlEnvio, "Informe a pasta de envio dos arquivos XML.", "A pasta de envio dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlRetorno, "Informe a pasta de envio dos arquivos XML.", "A pasta de retorno dos arquivos XML informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlErro, "Informe a pasta para arquivamento temporário dos arquivos XML que apresentaram erros.", "A pasta para arquivamento temporário dos arquivos XML com erro informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaValidar, "Informe a pasta onde será gravado os arquivos XML somente para ser validado pela aplicação.", "A pasta para validação de XML´s informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlEmLote, "Informe a pasta de envio dos arquivos XML em lote.", "A pasta de envio das notas fiscais eletrônicas em lote informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaXmlEnviado, "Informe a pasta para arquivamento dos arquivos XML enviados.", "A pasta para arquivamento dos arquivos XML enviados informada não existe.", true));
                            _xValids.Add(new xValid(empresa.PastaBackup, "", "A pasta para backup dos XML enviados informada não existe.", false));
                            _xValids.Add(new xValid(empresa.PastaDownloadNFeDest, "", "A pasta para arquivamento das NFe de destinatários informada não existe.", false));
                            _xValids.Add(new xValid(empresa.PastaDanfeMon, "", "A pasta informada para gravação do XML da NFe para o DANFeMon não existe.", false));
                            _xValids.Add(new xValid(empresa.PastaExeUniDanfe, "", "A pasta do executável do UniDANFe informada não existe.", false));
                            _xValids.Add(new xValid(empresa.PastaConfigUniDanfe, "", "A pasta do arquivo de configurações do UniDANFe informada não existe.", false));
                            break;
                    }

                    foreach (var val in _xValids)
                    {
                        if (val.valid && string.IsNullOrEmpty(val.folder))
                        {
                            erro = val.msg1 + xNomeCNPJ;
                            validou = false;
                            break;
                        }
                        else
                            if (!string.IsNullOrEmpty(val.folder))
                        {
                            if (!Directory.Exists(val.folder))
                            {
                                if (empresa.CriaPastasAutomaticamente)
                                {
                                    Directory.CreateDirectory(val.folder);
                                }
                                else
                                {
                                    erro = val.msg2 + xNomeCNPJ;
                                    validou = false;
                                    break;
                                }
                            }
                        }
                    }

#if f
                    List<string> diretorios = new List<string>();
                    List<string> mensagens = new List<string>();

                    diretorios.Add(empresa.PastaXmlEnvio); mensagens.Add("Informe a pasta de envio dos arquivos XML.");
                    diretorios.Add(empresa.PastaXmlRetorno); mensagens.Add("Informe a pasta de retorno dos arquivos XML.");
                    diretorios.Add(empresa.PastaXmlErro); mensagens.Add("Informe a pasta para arquivamento temporário dos arquivos XML que apresentaram erros.");
                    diretorios.Add(empresa.PastaValidar); mensagens.Add("Informe a pasta onde será gravado os arquivos XML somente para ser validado pela Aplicação.");
                    if (empresa.Servico != TipoAplicativo.Nfse)
                    {
                        diretorios.Add(empresa.PastaXmlEmLote); mensagens.Add("Informe a pasta de envio dos arquivos XML em lote.");
                        diretorios.Add(empresa.PastaXmlEnviado); mensagens.Add("Informe a pasta para arquivamento dos arquivos XML enviados.");
                    }

                    for (int b = 0; b < diretorios.Count; b++)
                    {
                        if (diretorios[b].Equals(string.Empty))
                        {
                            erro = mensagens[b] + xNomeCNPJ;
                            validou = false;
                            break;
                        }
                    }
#endif
                    ///
                    /// informacoes do FTP
                    /// danasa 7/7/2011
                    if (empresa.FTPIsAlive && validou)
                    {
                        if (empresa.Servico != TipoAplicativo.Nfse)
                        {
                            if (string.IsNullOrEmpty(empresa.FTPPastaAutorizados))
                            {
                                erro = "Informe a pasta do FTP de destino dos autorizados" + xNomeCNPJ;
                                validou = false;
                            }
                        }
                        else
                            if (string.IsNullOrEmpty(empresa.FTPPastaRetornos))
                        {
                            erro = "Informe a pasta do FTP de destino dos retornos" + xNomeCNPJ;
                            validou = false;
                        }
                    }

                    #endregion Verificar se tem alguma pasta em branco

                    #region Verificar se o certificado foi informado

                    if (validarCertificado && empresa.UsaCertificado && validou)
                    {
                        if (empresa.CertificadoInstalado && empresa.CertificadoDigitalThumbPrint.Equals(string.Empty))
                        {
                            erro = "Selecione o certificado digital a ser utilizado na autenticação dos serviços." + xNomeCNPJ;
                            validou = false;
                        }
                        if (!empresa.CertificadoInstalado && validou)
                        {
                            if (empresa.CertificadoArquivo.Equals(string.Empty))
                            {
                                erro = "Informe o local de armazenamento do certificado digital a ser utilizado na autenticação dos serviços." + xNomeCNPJ;
                                validou = false;
                            }
                            else if (!File.Exists(empresa.CertificadoArquivo))
                            {
                                erro = "Arquivo do certificado digital a ser utilizado na autenticação dos serviços não foi encontrado." + xNomeCNPJ;
                                validou = false;
                            }
                            else if (empresa.CertificadoSenha.Equals(string.Empty))
                            {
                                erro = "Informe a senha do certificado digital a ser utilizado na autenticação dos serviços." + xNomeCNPJ;
                                validou = false;
                            }
                            else
                            {
                                try
                                {
                                    using (var fs = new FileStream(empresa.CertificadoArquivo, FileMode.Open, FileAccess.Read))
                                    {
                                        var buffer = new byte[fs.Length];
                                        fs.Read(buffer, 0, buffer.Length);
                                        empresa.X509Certificado = new X509Certificate2(buffer, empresa.CertificadoSenha);
                                    }
                                }
                                catch (System.Security.Cryptography.CryptographicException ex)
                                {
                                    erro = ex.Message + "." + xNomeCNPJ;
                                    validou = false;
                                }
                                catch (Exception ex)
                                {
                                    erro = ex.Message + "." + xNomeCNPJ;
                                    validou = false;
                                }
                            }
                        }
                    }

                    #endregion Verificar se o certificado foi informado

                    #region Verificar se as pastas informadas existem

                    if (validou)
                    {
                        //Fazer um pequeno ajuste na pasta de configuração do unidanfe antes de verificar sua existência
                        if (empresa.PastaConfigUniDanfe.Trim() != string.Empty)
                        {
                            if (!string.IsNullOrEmpty(empresa.PastaConfigUniDanfe))
                            {
                                while (empresa.PastaConfigUniDanfe.Substring(empresa.PastaConfigUniDanfe.Length - 6, 6).ToLower() == @"\dados" &&
                                    !string.IsNullOrEmpty(empresa.PastaConfigUniDanfe))
                                {
                                    empresa.PastaConfigUniDanfe = empresa.PastaConfigUniDanfe.Substring(0, empresa.PastaConfigUniDanfe.Length - 6);
                                }
                            }
                            empresa.PastaConfigUniDanfe = empresa.PastaConfigUniDanfe.Replace("\r\n", "").Trim();
                            //empresa.PastaConfigUniDanfe = empresa.PastaConfigUniDanfe;
                        }

                        if (empresa.PastaXmlEnvio.ToLower().EndsWith("geral"))
                        {
                            erro = "Pasta de envio não pode terminar com a subpasta 'geral'." + xNomeCNPJ;
                            validou = false;
                        }
                        else if (empresa.PastaXmlEmLote.ToLower().EndsWith("geral"))
                        {
                            erro = "Pasta de envio em lote não pode terminar com a subpasta 'geral'." + xNomeCNPJ;
                            validou = false;
                        }
                        else if (empresa.PastaValidar.ToLower().EndsWith("geral"))
                        {
                            erro = "Pasta de validação não pode terminar com a subpasta 'geral'." + xNomeCNPJ;
                            validou = false;
                        }
                        else if (empresa.PastaXmlEnvio.ToLower().EndsWith("temp"))
                        {
                            erro = "Pasta de envio não pode terminar com a subpasta 'temp'." + xNomeCNPJ;
                            validou = false;
                        }
                        else if (empresa.PastaXmlEmLote.ToLower().EndsWith("temp"))
                        {
                            erro = "Pasta de envio em lote não pode terminar com a subpasta 'temp'." + xNomeCNPJ;
                            validou = false;
                        }
                        else if (empresa.PastaValidar.ToLower().EndsWith("temp"))
                        {
                            erro = "Pasta de validação não pode terminar com a subpasta 'temp'." + xNomeCNPJ;
                            validou = false;
                        }
                        else if (empresa.PastaXmlErro.ToLower().EndsWith("temp"))
                        {
                            erro = "Pasta de XML's com erro na tentativa de envio não pode terminar com a subpasta 'temp'." + xNomeCNPJ;
                            validou = false;
                        }

#if f

                        if (validou)
                        {
                            diretorios.Clear(); mensagens.Clear();
                            diretorios.Add(empresa.PastaXmlEnvio.Trim()); mensagens.Add("A pasta de envio dos arquivos XML informada não existe.");
                            diretorios.Add(empresa.PastaXmlRetorno.Trim()); mensagens.Add("A pasta de retorno dos arquivos XML informada não existe.");
                            diretorios.Add(empresa.PastaXmlErro.Trim()); mensagens.Add("A pasta para arquivamento temporário dos arquivos XML com erro informada não existe.");
                            diretorios.Add(empresa.PastaValidar.Trim()); mensagens.Add("A pasta para validação de XML´s informada não existe.");
                            if (empresa.Servico != TipoAplicativo.Nfse)
                            {
                                diretorios.Add(empresa.PastaXmlEnviado.Trim()); mensagens.Add("A pasta para arquivamento dos arquivos XML enviados informada não existe.");
                                diretorios.Add(empresa.PastaBackup.Trim()); mensagens.Add("A pasta para backup dos XML enviados informada não existe.");
                                diretorios.Add(empresa.PastaDownloadNFeDest.Trim()); mensagens.Add("A pasta para arquivamento das NFe de destinatários informada não existe.");
                                diretorios.Add(empresa.PastaXmlEmLote.Trim()); mensagens.Add("A pasta de envio das notas fiscais eletrônicas em lote informada não existe.");
                                diretorios.Add(empresa.PastaDanfeMon.Trim()); mensagens.Add("A pasta informada para gravação do XML da NFe para o DANFeMon não existe.");
                                diretorios.Add(empresa.PastaExeUniDanfe.Trim()); mensagens.Add("A pasta do executável do UniDANFe informada não existe.");
                                diretorios.Add(empresa.PastaConfigUniDanfe.Trim()); mensagens.Add("A pasta do arquivo de configurações do UniDANFe informada não existe.");
                            }

                            for (int b = 0; b < diretorios.Count; b++)
                            {
                                if (!string.IsNullOrEmpty(diretorios[b]))
                                {
                                    if (!Directory.Exists(diretorios[b]))
                                    {
                                        if (empresa.CriaPastasAutomaticamente)
                                        {
                                            Directory.CreateDirectory(diretorios[b]);
                                        }
                                        else
                                        {
                                            erro = mensagens[b].Trim() + xNomeCNPJ;
                                            validou = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
#endif

                        #region Criar pasta Temp dentro da pasta de envio, envio em lote e validar

                        //Criar pasta Temp dentro da pasta de envio, envio em Lote e Validar. Wandrey 03/08/2011
                        if (validou)
                        {
                            if (Directory.Exists(empresa.PastaXmlEnvio.Trim()))
                            {
                                if (!Directory.Exists(empresa.PastaXmlEnvio.Trim() + "\\Temp"))
                                {
                                    Directory.CreateDirectory(empresa.PastaXmlEnvio.Trim() + "\\Temp");
                                }
                            }

                            if (Directory.Exists(empresa.PastaXmlEmLote.Trim()))
                            {
                                if (!Directory.Exists(empresa.PastaXmlEmLote.Trim() + "\\Temp"))
                                {
                                    Directory.CreateDirectory(empresa.PastaXmlEmLote.Trim() + "\\Temp");
                                }
                            }

                            if (Directory.Exists(empresa.PastaValidar.Trim()))
                            {
                                if (!Directory.Exists(empresa.PastaValidar.Trim() + "\\Temp"))
                                {
                                    Directory.CreateDirectory(empresa.PastaValidar.Trim() + "\\Temp");
                                }
                            }
                        }

                        #endregion Criar pasta Temp dentro da pasta de envio, envio em lote e validar
                    }

                    #endregion Verificar se as pastas informadas existem

                    #region Verificar se as pastas configuradas do unidanfe estão corretas

                    if (empresa.Servico != TipoAplicativo.Nfse && validou)
                    {
                        if (empresa.PastaExeUniDanfe.Trim() != string.Empty)
                        {
                            if (!File.Exists(empresa.PastaExeUniDanfe + "\\unidanfe.exe"))
                            {
                                erro = "O executável do UniDANFe não foi localizado na pasta informada." + xNomeCNPJ;
                                validou = false;
                            }
                        }

                        if (validou && empresa.PastaConfigUniDanfe.Trim() != string.Empty)
                        {
                            //Verificar a existência o arquivo de configuração
                            if (!File.Exists(empresa.PastaConfigUniDanfe + "\\dados\\ConfigUD.tps"))
                            {
                                erro = "O arquivo de configuração do UniDANFe não foi localizado na pasta informada." + xNomeCNPJ;
                                validou = false;
                            }
                        }
                    }

                    #endregion Verificar se as pastas configuradas do unidanfe estão corretas

                    #region Verificar se o IDToken informado é menor que 6 caracteres

                    if (!string.IsNullOrEmpty(empresa.TokenCSC) && empresa.TokenCSC.Length < 6)
                    {
                        erro = "O IDToken deve ter 6 caracteres." + xNomeCNPJ;
                        validou = false;
                    }

                    #endregion Verificar se o IDToken informado é menor que 6 caracteres
                }
            }

            #region Ticket: #110

            /* Validar se já existe uma instancia utilizando estes diretórios
             * Marcelo
             * 03/06/2013
             */
            if (validou)
            {
                //Se encontrar algum arquivo de lock nos diretórios, não permitir que seja executado
                try
                {
                    Empresas.CanRun(empresaValidada);
                }
                catch (NFe.Components.Exceptions.AppJaExecutando ex)
                {
                    erro = ex.Message;
                }

                validou = string.IsNullOrEmpty(erro);
            }

            #endregion Ticket: #110

            if (!validou)
            {
                throw new Exception(erro);
            }
        }

        #endregion ValidarConfig()

        #region ReconfigurarUniNFe()

        /// <summary>
        /// Método responsável por reconfigurar automaticamente o UniNFe a partir de um XML com as
        /// informações necessárias.
        /// O Método grava um arquivo na pasta de retorno do UniNFe com a informação se foi bem
        /// sucedida a reconfiguração ou não.
        /// </summary>
        /// <param name="cArquivoXml">Nome e pasta do arquivo de configurações gerado pelo ERP para atualização das configurações do uninfe</param>
        public void ReconfigurarUniNFe(string cArquivoXml)
        {
            var emp = Empresas.FindEmpresaByThread();


            var cStat = "";
            var xMotivo = "";
            var lErro = false;
            var lEncontrouTag = false;

            try
            {
                /*
                    Bug #171857 - Problema ao tentar alterar as configurações do uninfe através do arquivo de configuração (altConfUniNFe)
                                  "Quando a senha do certificado possui um '&' gera erro"
                */

                AjusteEComercial(cArquivoXml);

                if (Path.GetExtension(cArquivoXml).ToLower() != ".txt" && ExcluirEmpresa(cArquivoXml))
                {
                    cStat = "1";
                    xMotivo = "Empresa excluída com sucesso";
                    lErro = false;

                    ConfiguracaoApp.CarregarDados();
                    ConfiguracaoApp.CarregarDadosSobre();
                    Empresas.CarregaConfiguracao();
                }
                else
                {
                    emp = CadastrarEmpresa(cArquivoXml, emp);

                    ///
                    /// danasa - 12/2019
                    /// Salva o serviço porque se o usuario informar novamente a tag->Servico 
                    /// e esta tag for diferente da tag->Servico da chave, evitamos erro
                    /// 
                    var currServico = Empresas.Configuracoes[emp].Servico;
                    var checarCNPJCPFCertificado = false;

                    if (Path.GetExtension(cArquivoXml).ToLower() == ".txt")
                    {
                        #region Formato TXT

                        var cLinhas = Functions.LerArquivo(cArquivoXml);

                        lEncontrouTag = Functions.PopulateClasse(Empresas.Configuracoes[emp], cLinhas);

                        foreach (var texto in cLinhas)
                        {
                            var dados = texto.Split('|');
                            var nElementos = dados.GetLength(0);
                            if (nElementos <= 1)
                            {
                                continue;
                            }

                            switch (dados[0].ToLower())
                            {
                                case "proxy": //Se a tag <Proxy> existir ele pega o novo conteúdo
                                    ConfiguracaoApp.Proxy = (nElementos == 2 ? Convert.ToBoolean(dados[1].Trim()) : false);
                                    lEncontrouTag = true;
                                    break;

                                case "proxyservidor": //Se a tag <ProxyServidor> existir ele pega o novo conteúdo
                                    ConfiguracaoApp.ProxyServidor = (nElementos == 2 ? dados[1].Trim() : "");
                                    lEncontrouTag = true;
                                    break;

                                case "proxyusuario": //Se a tag <ProxyUsuario> existir ele pega o novo conteúdo
                                    ConfiguracaoApp.ProxyUsuario = (nElementos == 2 ? dados[1].Trim() : "");
                                    lEncontrouTag = true;
                                    break;

                                case "proxysenha": //Se a tag <ProxySenha> existir ele pega o novo conteúdo
                                    ConfiguracaoApp.ProxySenha = (nElementos == 2 ? dados[1].Trim() : "");
                                    lEncontrouTag = true;
                                    break;

                                case "proxyporta": //Se a tag <ProxyPorta> existir ele pega o novo conteúdo
                                    ConfiguracaoApp.ProxyPorta = (nElementos == 2 ? Convert.ToInt32("0" + dados[1].Trim()) : 0);
                                    lEncontrouTag = true;
                                    break;

                                case "checarconexaointernet": //Se a tag <ChecarConexaoInternet> existir ele pega o novo conteúdo
                                    ConfiguracaoApp.ChecarConexaoInternet = (nElementos == 2 ? Convert.ToBoolean(dados[1].Trim()) : true);
                                    lEncontrouTag = true;
                                    break;

                                case "gravarlogoperacaorealizada":
                                    ConfiguracaoApp.GravarLogOperacoesRealizadas = (nElementos == 2 ? Convert.ToBoolean(dados[1].Trim()) : true);
                                    lEncontrouTag = true;
                                    break;

                                case "senhaconfig": //Se a tag <senhaconfig> existir ele pega o novo conteúdo
                                    ConfiguracaoApp.SenhaConfig = Functions.GerarMD5((nElementos == 2 ? dados[1].Trim() : ""));
                                    lEncontrouTag = true;
                                    break;

                                case "checarcnpjcpfcertificado":
                                    checarCNPJCPFCertificado = (nElementos == 2 ? Convert.ToBoolean(dados[1].Trim()) : false);
                                    lEncontrouTag = true;
                                    break;
                            }
                        }

                        #endregion Formato TXT
                    }
                    else
                    {
                        #region Formato XML

                        var doc = new XmlDocument();
                        try
                        {
                            doc.Load(cArquivoXml);
                        }
                        catch
                        {
                            doc.LoadXml(System.IO.File.ReadAllText(cArquivoXml, System.Text.Encoding.UTF8));
                        }

                        var ConfUniNfeList = doc.GetElementsByTagName("altConfUniNFe");

                        foreach (XmlNode ConfUniNfeNode in ConfUniNfeList)
                        {
                            var ConfUniNfeElemento = (XmlElement)ConfUniNfeNode;
                            lEncontrouTag = Functions.PopulateClasse(Empresas.Configuracoes[emp], ConfUniNfeElemento);

                            //Se a tag <Proxy> existir ele pega o novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.Proxy.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.Proxy = Convert.ToBoolean(ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.Proxy.ToString())[0].InnerText);
                                lEncontrouTag = true;
                            }
                            //Se a tag <ProxyServidor> existir ele pega o novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxyServidor.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.ProxyServidor = ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxyServidor.ToString())[0].InnerText;
                                lEncontrouTag = true;
                            }
                            //Se a tag <ProxyUsuario> existir ele pega o novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxyUsuario.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.ProxyUsuario = ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxyUsuario.ToString())[0].InnerText;
                                lEncontrouTag = true;
                            }
                            //Se a tag <ProxySenha> existir ele pega o novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxySenha.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.ProxySenha = ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxySenha.ToString())[0].InnerText;
                                lEncontrouTag = true;
                            }
                            //Se a tag <ProxyPorta> existir ele pega o novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxyPorta.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.ProxyPorta = Convert.ToInt32("0" + ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ProxyPorta.ToString())[0].InnerText);
                                lEncontrouTag = true;
                            }
                            //Se a tag <ChecarConexaoInternet> existir ele pega o novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ChecarConexaoInternet.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.ChecarConexaoInternet = Convert.ToBoolean(ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ChecarConexaoInternet.ToString())[0].InnerText);
                                lEncontrouTag = true;
                            }
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.GravarLogOperacaoRealizada.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.GravarLogOperacoesRealizadas = Convert.ToBoolean(ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.GravarLogOperacaoRealizada.ToString())[0].InnerText);
                                lEncontrouTag = true;
                            }
                            //Se a tag <SenhaConfig> existir ele pega no novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.SenhaConfig.ToString()).Count != 0)
                            {
                                if (!string.IsNullOrEmpty(ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.SenhaConfig.ToString())[0].InnerText))
                                {
                                    ConfiguracaoApp.SenhaConfig = Functions.GerarMD5(ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.SenhaConfig.ToString())[0].InnerText);
                                }
                                else
                                {
                                    ConfiguracaoApp.SenhaConfig = "";
                                }
                                lEncontrouTag = true;
                            }
                            //Se a tag <ConfirmaSaida> existir ele pega novo conteúdo
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ConfirmaSaida.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.Proxy = Convert.ToBoolean(ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ConfirmaSaida.ToString())[0].InnerText);
                                lEncontrouTag = true;
                            }
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.AppID.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.Secret = ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.AppID.ToString())[0].InnerText;
                                lEncontrouTag = true;
                            }
                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.Secret.ToString()).Count != 0)
                            {
                                ConfiguracaoApp.Secret = ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.Secret.ToString())[0].InnerText;
                                lEncontrouTag = true;
                            }

                            if (ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ChecarCNPJCPFCertificado.ToString()).Count != 0)
                            {
                                checarCNPJCPFCertificado = Convert.ToBoolean(ConfUniNfeElemento.GetElementsByTagName(NfeConfiguracoes.ChecarCNPJCPFCertificado.ToString())[0].InnerText);
                            }
                        }

                        #endregion Formato XML
                    }
                    Empresas.Configuracoes[emp].Servico = currServico;

                    if (lEncontrouTag)
                    {
                        if (ConfiguracaoApp.Proxy &&
                            (ConfiguracaoApp.ProxyPorta == 0 ||
                            string.IsNullOrEmpty(ConfiguracaoApp.ProxyServidor) ||
                            string.IsNullOrEmpty(ConfiguracaoApp.ProxyUsuario) ||
                            string.IsNullOrEmpty(ConfiguracaoApp.ProxySenha)))
                        {
                            throw new Exception(NFeStrConstants.proxyError);
                        }
                        Empresas.Configuracoes[emp].RemoveEndSlash();
                        Empresas.Configuracoes[emp].FixAbsolutePath();
                        Empresas.CriarPasta(false);

                        //Se o certificado digital for o instalado no windows, vamos tentar buscar ele no repositório para ver se existe.
                        if (Empresas.Configuracoes[emp].CertificadoInstalado)
                        {
                            var oX509Cert = new X509Certificate2();
                            var store = new X509Store("MY", StoreLocation.CurrentUser);
                            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                            var collection = store.Certificates;
                            var collection1 = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                            var collection2 = collection.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false);

                            //Primeiro tento encontrar pelo thumbprint
                            var collection3 = collection2.Find(X509FindType.FindByThumbprint, Empresas.Configuracoes[emp].CertificadoDigitalThumbPrint, false);
                            if (collection3.Count <= 0)
                            {
                                //Se não encontrou pelo thumbprint tento pelo SerialNumber pegando o mesmo thumbprint que veio no arquivo de configurações para ver se não encontro.
                                collection3 = collection2.Find(X509FindType.FindBySerialNumber, Empresas.Configuracoes[emp].CertificadoDigitalThumbPrint, false);

                                if (collection3.Count <= 0)
                                {
                                    throw new Exception("Certificado digital informado não foi localizado no repositório do windows.");
                                }

                                Empresas.Configuracoes[emp].CertificadoDigitalThumbPrint = collection3[0].Thumbprint;
                            }
                        }
                        else
                        {
                            Empresas.Configuracoes[emp].X509Certificado = Empresas.Configuracoes[emp].BuscaConfiguracaoCertificado();
                        }

                        if (!string.IsNullOrEmpty(ConfiguracaoApp.AppID) || !string.IsNullOrEmpty(ConfiguracaoApp.Secret))
                        {
                            Empresas.Configuracoes[emp].AppID = ConfiguracaoApp.AppID;
                            Empresas.Configuracoes[emp].Secret = ConfiguracaoApp.Secret;
                        }

                        if (checarCNPJCPFCertificado)
                        {
                            var certificadoSubject = string.Empty;
                            var cmp = new ConfiguracaoApp();

                            if (!cmp.EhIgualDocumento(Empresas.Configuracoes[emp].X509Certificado.Subject, Empresas.Configuracoes[emp].CNPJ))
                            {
                                throw new Exception(
                                    "CNPJ/CPF do certificado configurado é diferente do CNPJ/CPF cadastrado. " +
                                    "Configuração abortada.");
                            }
                        }

                        ///
                        /// salva a configuracao da empresa
                        ///

                        Empresas.Configuracoes[emp].SalvarConfiguracao(false, true);

                        /// salva o arquivo da lista de empresas
                        GravarArqEmpresas();

                        /// salva as configuracoes gerais
                        GravarConfigGeral(true);

                        cStat = "1";
                        xMotivo = "Configuração do " + Propriedade.NomeAplicacao + " alterada com sucesso";
                        lErro = false;
                    }
                    else
                    {
                        cStat = "2";
                        xMotivo = "Ocorreu uma falha ao tentar alterar a configuracao do " + Propriedade.NomeAplicacao + ": Nenhuma tag padrão de configuração foi localizada no XML";
                        lErro = true;
                    }
                }
            }
            catch (Exception ex)
            {
                cStat = "2";
                xMotivo = "Ocorreu uma falha ao tentar alterar a configuracao do " + Propriedade.NomeAplicacao + ": " + ex.Message;
                lErro = true;
            }

            try
            {
                //Gravar o XML de retorno com a informação do sucesso ou não na reconfiguração
                var arqInfo = new FileInfo(cArquivoXml);
                var pastaRetorno = string.Empty;
                if (arqInfo.DirectoryName.ToLower().Trim() == Propriedade.PastaGeralTemporaria.ToLower().Trim())
                {
                    pastaRetorno = Propriedade.PastaGeralRetorno;
                }
                else
                {
                    pastaRetorno = Empresas.Configuracoes[emp].PastaXmlRetorno;
                    ///
                    /// se der erro na atualizacao e se for solicitada a alteracao da pasta de retorno,
                    /// verificamos se ainda assim ela existe
                    ///
                    /// Nao existindo, gravamos o retorno na pasta de retorno do UniNFe
                    ///
                    if (!Directory.Exists(pastaRetorno) && lErro)
                    {
                        pastaRetorno = Propriedade.PastaGeralRetorno;
                    }
                }

                string nomeArqRetorno;
                var EXT = Propriedade.Extensao(Propriedade.TipoEnvio.AltCon);
                if (Path.GetExtension(cArquivoXml).ToLower() == ".txt")
                {
                    nomeArqRetorno = Functions.ExtrairNomeArq(cArquivoXml, EXT.EnvioTXT) + EXT.RetornoTXT;
                }
                else
                {
                    nomeArqRetorno = Functions.ExtrairNomeArq(cArquivoXml, EXT.EnvioXML) + EXT.RetornoXML;
                }

                var cArqRetorno = pastaRetorno + "\\" + nomeArqRetorno;

                try
                {
                    var oArqRetorno = new FileInfo(cArqRetorno);
                    if (oArqRetorno.Exists == true)
                    {
                        oArqRetorno.Delete();
                    }

                    if (Path.GetExtension(cArquivoXml).ToLower() == ".txt")
                    {
                        File.WriteAllText(cArqRetorno, "cStat|" + cStat + "\r\nxMotivo|" + xMotivo + "\r\n");
                    }
                    else
                    {
                        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                                                new XElement("retAltConfUniNFe",
                                                    new XElement(NFe.Components.TpcnResources.cStat.ToString(), cStat),
                                                    new XElement(NFe.Components.TpcnResources.xMotivo.ToString(), xMotivo),
                                                    new XElement("CertificadoDigitalThumbPrint", Empresas.Configuracoes[emp].CertificadoDigitalThumbPrint)));
                        xml.Save(cArqRetorno);
                    }
                }
                catch (Exception ex)
                {
                    //Ocorreu erro na hora de gerar o arquivo de erro para o ERP
                    ///
                    /// danasa 8-2009
                    ///
                    var oAux = new Auxiliar();
                    oAux.GravarArqErroERP(Path.GetFileNameWithoutExtension(cArqRetorno) + ".err", xMotivo + Environment.NewLine + ex.Message);
                }
            }
            finally
            {
                //Se deu algum erro tenho que voltar as configurações como eram antes, ou seja
                //exatamente como estavam gravadas no XML de configuração
                if (lErro)
                {
                    ConfiguracaoApp.CarregarDados();
                    ConfiguracaoApp.CarregarDadosSobre();
                    Empresas.CarregaConfiguracao();

                    #region Ticket: #110

                    Empresas.CreateLockFile(true);

                    #endregion Ticket: #110
                }

                try
                {
                    //Deletar o arquivo de configurações automáticas gerado pelo ERP
                    Functions.DeletarArquivo(cArquivoXml);
                }
                catch
                {
                    //Não vou fazer nada, so trato a exceção para evitar fechar o aplicativo. Wandrey 09/03/2010
                }
            }
        }

        /// <summary>
        /// Ajusta o arquivo de configuração do UniNFe para evitar erro de "&" no arquivo XML
        /// </summary>
        /// <param name="cArquivoXml">Caminho do arquivo xml</param>
        private static void AjusteEComercial(string cArquivoXml)
        {
            var conteudoCorrigido = default(string);

            using (var reader = new StreamReader(cArquivoXml))
            {
                var conteudo = reader.ReadToEnd();
                conteudoCorrigido = conteudo.Replace("&", "&amp;");
            }

            using (var writer = new StreamWriter(cArquivoXml))
            {
                writer.Write(conteudoCorrigido);
            }
        }

        #endregion ReconfigurarUniNFe()

        #region ExcluirEmpresa

        /// <summary>
        /// Excluir a empresa e suas configurações
        /// </summary>
        /// <param name="arquivoXml">Nome e pasta do arquivo de configurações gerado pelo ERP para atualização das configurações do uninfe</param>
        /// <returns>
        /// S - Excluido com sucesso.
        /// E - Erro durante a exclusão.
        /// N - Não teve solicitação de exclusão no XML.
        /// </returns>
        private bool ExcluirEmpresa(string arquivoXml)
        {
            var retorna = false;

            try
            {
                var doc2 = new XmlDocument();
                doc2.Load(arquivoXml);

                var altConfUniNFeList = doc2.GetElementsByTagName("altConfUniNFe");
                if (altConfUniNFeList.Count > 0)
                {
                    var altConfUniNFeElement = (XmlElement)altConfUniNFeList[0];

                    if (altConfUniNFeElement.GetElementsByTagName("DadosEmpresa").Count > 0)
                    {
                        var dadosEmpresaElement = (XmlElement)altConfUniNFeElement.GetElementsByTagName("DadosEmpresa")[0];
                        if (dadosEmpresaElement.GetElementsByTagName("ExcluirEmpresa").Count > 0)
                        {
                            var excluir = Convert.ToBoolean(dadosEmpresaElement.GetElementsByTagName("ExcluirEmpresa")[0].InnerText);
                            if (excluir)
                            {
                                try
                                {
                                    var cnpj = dadosEmpresaElement.GetAttribute("CNPJ");
                                    var servico = dadosEmpresaElement.GetAttribute("Servico");
                                    var _empresa = Empresas.FindConfEmpresa(cnpj, EnumHelper.StringToEnum<TipoAplicativo>(servico));
                                    if (_empresa != null)
                                    {
                                        Empresas.Configuracoes.Remove(_empresa);
                                        new ConfiguracaoApp().GravarArqEmpresas();

                                        retorna = true;
                                    }
                                    else
                                    {
                                        throw new Exception("Empresa não localizada (" + cnpj + "). Exclusão não foi efetuada.");
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return retorna;
        }

        #endregion ExcluirEmpresa

        #region RemoveEndSlash

        /// <summary>
        /// Remover barra invertida no final do caminho da pasta.
        /// </summary>
        /// <param name="value">Pasta para analisar e remover barra no final do caminho</param>
        /// <returns>Pasta sem a barra no final</returns>
        public static string RemoveEndSlash(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = new DirectoryInfo(value).FullName;
                value = value.TrimEnd('\\');
            }
            else
            {
                value = string.Empty;
            }

            return value.Replace("\r\n", "").Trim();
        }

        #endregion RemoveEndSlash

        #region FixAbsolutePath()

        /// <summary>
        /// Pasta para analisar o caminho absoluto
        /// </summary>
        /// <param name="pasta">Pasta a ser analisada</param>
        /// <returns>Retorna o caminho completo, com a unidade, se for o caso.</returns>
        public static string FixAbsolutePath(string pasta)
        {
            // Se o caminho for nulo ou vazio, retorna como está
            if (string.IsNullOrWhiteSpace(pasta))
            {
                return pasta;
            }

            // Verifica se o caminho é absoluto ou é um caminho de rede UNC
            if (Path.IsPathRooted(pasta) || pasta.StartsWith(@"\\"))
            {
                // Retorna o caminho como está, pois já é válido
                return Path.GetFullPath(pasta);
            }

            // Se não for absoluto nem UNC, assume que o caminho é relativo e adiciona "C:\"
            pasta = Path.Combine(@"C:\", pasta.TrimStart('\\'));

            // Retorna o caminho corrigido como absoluto
            return Path.GetFullPath(pasta);
        }

        #endregion

        #region CadastrarEmpresa()

        private int CadastrarEmpresa(string arqXML, int emp)
        {
            var cnpj = "";
            var nomeEmp = "";
            var servico = "";
            var temEmpresa = false;

            if (Path.GetExtension(arqXML).ToLower() == ".xml")
            {
                var doc = new XmlDocument();
                doc.Load(arqXML);

                var dadosEmpresa = (XmlElement)doc.GetElementsByTagName("DadosEmpresa")[0];

                if (dadosEmpresa != null)
                {
                    #region Nome da empresa

                    if (dadosEmpresa.GetElementsByTagName("Nome")[0] != null)
                    {
                        nomeEmp = dadosEmpresa.GetElementsByTagName("Nome")[0].InnerText;
                        temEmpresa = true;
                    }
                    else if (dadosEmpresa.GetElementsByTagName("nome")[0] != null)
                    {
                        nomeEmp = dadosEmpresa.GetElementsByTagName("nome")[0].InnerText;
                        temEmpresa = true;
                    }

                    #endregion Nome da empresa

                    #region CNPJ

                    if (!string.IsNullOrEmpty(dadosEmpresa.GetAttribute(NFe.Components.TpcnResources.CNPJ.ToString())))
                    {
                        cnpj = dadosEmpresa.GetAttribute(NFe.Components.TpcnResources.CNPJ.ToString());
                        temEmpresa = true;
                    }
                    else if (!string.IsNullOrEmpty(dadosEmpresa.GetAttribute("cnpj")))
                    {
                        cnpj = dadosEmpresa.GetAttribute("cnpj");
                        temEmpresa = true;
                    }
                    else if (!string.IsNullOrEmpty(dadosEmpresa.GetAttribute("Cnpj")))
                    {
                        cnpj = dadosEmpresa.GetAttribute("Cnpj");
                        temEmpresa = true;
                    }

                    #endregion CNPJ

                    #region Servico

                    if (!string.IsNullOrEmpty(dadosEmpresa.GetAttribute("Servico")))
                    {
                        servico = dadosEmpresa.GetAttribute("Servico");
                        temEmpresa = true;
                    }
                    else if (!string.IsNullOrEmpty(dadosEmpresa.GetAttribute("servico")))
                    {
                        servico = dadosEmpresa.GetAttribute("servico");
                        temEmpresa = true;
                    }

                    #endregion Servico
                }
            }
            else
            {
                var cLinhas = Functions.LerArquivo(arqXML);

                foreach (var texto in cLinhas)
                {
                    var dados = texto.Split('|');
                    var nElementos = dados.GetLength(0);
                    if (nElementos <= 1)
                    {
                        continue;
                    }

                    switch (dados[0].ToLower())
                    {
                        case "nome":
                            nomeEmp = dados[1].Trim();
                            temEmpresa = true;
                            break;

                        case "cnpj":
                            cnpj = dados[1].Trim();
                            temEmpresa = true;
                            break;

                        case "servico":
                            servico = dados[1].Trim();
                            temEmpresa = true;
                            break;
                    }
                }
            }

            if (temEmpresa)
            {
                if (string.IsNullOrEmpty(cnpj) || string.IsNullOrEmpty(nomeEmp) || string.IsNullOrEmpty(servico))
                {
                    throw new Exception("Não foi possível localizar os dados da empresa no arquivo de configuração. (CNPJ/Nome ou Tipo de Serviço)");
                }

                if (char.IsLetter(servico, 0))
                {
                    var lista = NFe.Components.EnumHelper.ToStrings(typeof(TipoAplicativo));
                    if (!lista.Contains(servico))
                    {
                        throw new Exception(string.Format("Serviço deve ser ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11} ou {12})",
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Nfe),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Cte),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Nfse),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.MDFe),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.NFCe),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.SATeMFE),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.EFDReinf),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.eSocial),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.EFDReinfeSocial),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.GNREeDARE),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Todos),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.NF3e),
                            NFe.Components.EnumHelper.GetDescription(TipoAplicativo.NFCom)));
                    }

                    ///
                    /// veio como 'NFe, NFCe, CTe, MDFe ou NFSe
                    /// converte para numero correspondente
                    servico = ((int)NFe.Components.EnumHelper.StringToEnum<TipoAplicativo>(servico)).ToString();
                }
                else
                {
                    if (!("0,1,2,3,4,5,6,7,8,9,10,11,12").Contains(servico))
                    {
                        throw new Exception(string.Format("Serviço deve ser ({0} p/{1}, {2} p/{3}, {4} p/{5}, {6} p/{7}, {8} p/{9}, {10} p/{11}, {12} p/{13}, {14} p/{15}, {16} p/{17}, {18} p/{19}, {20} p/{21}, {22} p/{23}, {24} p/{25})",
                            (int)TipoAplicativo.Nfe, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Nfe),
                            (int)TipoAplicativo.Cte, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Cte),
                            (int)TipoAplicativo.Nfse, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Nfse),
                            (int)TipoAplicativo.MDFe, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.MDFe),
                            (int)TipoAplicativo.NFCe, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.NFCe),
                            (int)TipoAplicativo.SATeMFE, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.SATeMFE),
                            (int)TipoAplicativo.EFDReinf, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.EFDReinf),
                            (int)TipoAplicativo.eSocial, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.eSocial),
                            (int)TipoAplicativo.EFDReinfeSocial, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.EFDReinfeSocial),
                            (int)TipoAplicativo.GNREeDARE, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.GNREeDARE),
                            (int)TipoAplicativo.Todos, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.Todos),
                            (int)TipoAplicativo.NF3e, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.NF3e),
                            (int)TipoAplicativo.NFCom, NFe.Components.EnumHelper.GetDescription(TipoAplicativo.NFCom)));
                    }
                }
                if (Empresas.FindConfEmpresa(cnpj.Trim(), (TipoAplicativo)Convert.ToInt16(servico)) == null)
                {
                    var empresa = new Empresa
                    {
                        CNPJ = cnpj,
                        Nome = nomeEmp,
                        Servico = (TipoAplicativo)Convert.ToInt16(servico)
                    };
                    Empresas.Configuracoes.Add(empresa);

                    //GravarArqEmpresas();  //tirado daqui pq ele somente grava quando a empresa for gravada com sucesso
                }

                return Empresas.FindConfEmpresaIndex(cnpj, (TipoAplicativo)Convert.ToInt16(servico));
            }
            else
            {
                return emp;
            }
        }

        #endregion CadastrarEmpresa()

        #region CertificadosInstalados()

        public void CertificadosInstalados(string arquivo)
        {
            var lConsultar = false;
            var lErro = false;
            var arqRet = Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.ConsCertificado).EnvioXML) +
                                  Propriedade.Extensao(Propriedade.TipoEnvio.ConsCertificado).RetornoXML;
            var tmp_arqRet = Path.Combine(Propriedade.PastaGeralTemporaria, arqRet);
            var cStat = "";
            var xMotivo = "";

            try
            {
                var doc = new XmlDocument();
                doc.Load(arquivo);

                foreach (XmlElement item in doc.DocumentElement)
                {
                    lConsultar = doc.DocumentElement.GetElementsByTagName("xServ")[0].InnerText.Equals("CONS-CERTIFICADO", StringComparison.InvariantCultureIgnoreCase);
                }

                if (lConsultar)
                {
                    var store = new X509Store("MY", StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    var collection = store.Certificates;
                    var collection1 = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                    var collection2 = collection.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false);

                    #region Cria XML de retorno

                    if (File.Exists(tmp_arqRet))
                    {
                        File.Delete(tmp_arqRet);
                    }

                    var RetCertificados = new XmlDocument();

                    XmlNode raiz = RetCertificados.CreateElement("Certificados");
                    RetCertificados.AppendChild(raiz);

                    RetCertificados.Save(tmp_arqRet);

                    #endregion Cria XML de retorno

                    #region Monta XML de Retorno com dados do Certificados Instalados

                    for (var i = 0; i < collection2.Count; i++)
                    {
                        #region layout retorno

                        /*layout de retorno - Renan Borges
                        <Certificados>
                        <ThumbPrint ID="999...">
                        <Subject>XX...</Subject>
                        <ValidadeInicial>dd/dd/dddd</ValidadeInicial>
                        <ValidadeFinal>dd/dd/dddd</ValidadeFinal>
                        <A3>true</A3>
                        </Certificados>
                        */

                        #endregion layout retorno

                        var _X509Cert = collection2[i];

                        var docGerar = new XmlDocument();
                        docGerar.Load(tmp_arqRet);

                        XmlNode Registro = docGerar.CreateElement("ThumbPrint");
                        var IdThumbPrint = docGerar.CreateAttribute(TpcnResources.ID.ToString());
                        IdThumbPrint.Value = _X509Cert.Thumbprint.ToString();
                        Registro.Attributes.Append(IdThumbPrint);

                        XmlNode Subject = docGerar.CreateElement("Subject");
                        XmlNode ValidadeInicial = docGerar.CreateElement("ValidadeInicial");
                        XmlNode ValidadeFinal = docGerar.CreateElement("ValidadeFinal");
                        XmlNode A3 = docGerar.CreateElement("A3");
                        XmlNode SerialNumber = docGerar.CreateElement("SerialNumber");

                        Subject.InnerText = _X509Cert.Subject.ToString();
                        ValidadeInicial.InnerText = _X509Cert.NotBefore.ToShortDateString();
                        ValidadeFinal.InnerText = _X509Cert.NotAfter.ToShortDateString();
                        A3.InnerText = _X509Cert.IsA3().ToString().ToLower();
                        SerialNumber.InnerText = _X509Cert.SerialNumber;

                        docGerar.SelectSingleNode("Certificados").AppendChild(Registro);
                        Registro.AppendChild(Subject);
                        Registro.AppendChild(ValidadeInicial);
                        Registro.AppendChild(ValidadeFinal);
                        Registro.AppendChild(A3);
                        Registro.AppendChild(SerialNumber);

                        docGerar.Save(tmp_arqRet);
                    }

                    #endregion Monta XML de Retorno com dados do Certificados Instalados

                    #region Monta XML de retorno com os certificados do tipo A1 que estão configurados no UniNFe com base no arquivo .PFX

                    foreach (var item in Empresas.Configuracoes)
                    {
                        if (item.UsaCertificado && !item.CertificadoInstalado)
                        {
                            if (!string.IsNullOrWhiteSpace(item.CertificadoArquivo))
                            {
                                if (item.X509Certificado == null)
                                {
                                    continue;
                                }

                                var docGerar = new XmlDocument();
                                docGerar.Load(tmp_arqRet);
                                var _X509Cert = item.X509Certificado;

                                docGerar.Load(tmp_arqRet);

                                XmlNode Registro = docGerar.CreateElement("ThumbPrint");
                                var IdThumbPrint = docGerar.CreateAttribute(TpcnResources.ID.ToString());
                                IdThumbPrint.Value = _X509Cert.Thumbprint.ToString();
                                Registro.Attributes.Append(IdThumbPrint);

                                XmlNode Subject = docGerar.CreateElement("Subject");
                                XmlNode ValidadeInicial = docGerar.CreateElement("ValidadeInicial");
                                XmlNode ValidadeFinal = docGerar.CreateElement("ValidadeFinal");
                                XmlNode A3 = docGerar.CreateElement("A3");
                                XmlNode SerialNumber = docGerar.CreateElement("SerialNumber");
                                XmlNode PastaCertificado = docGerar.CreateElement("PastaCertificado");

                                Subject.InnerText = _X509Cert.Subject.ToString();
                                ValidadeInicial.InnerText = _X509Cert.NotBefore.ToShortDateString();
                                ValidadeFinal.InnerText = _X509Cert.NotAfter.ToShortDateString();
                                A3.InnerText = _X509Cert.IsA3().ToString().ToLower();
                                SerialNumber.InnerText = _X509Cert.SerialNumber;
                                PastaCertificado.InnerText = item.CertificadoArquivo.Trim();

                                docGerar.SelectSingleNode("Certificados").AppendChild(Registro);
                                Registro.AppendChild(Subject);
                                Registro.AppendChild(ValidadeInicial);
                                Registro.AppendChild(ValidadeFinal);
                                Registro.AppendChild(A3);
                                Registro.AppendChild(SerialNumber);
                                Registro.AppendChild(PastaCertificado);

                                docGerar.Save(tmp_arqRet);
                            }
                        }
                    }

                    #endregion Monta XML de retorno com os certificados do tipo A1 que estão configurados no UniNFe com base no arquivo .PFX
                }
            }
            catch (Exception ex)
            {
                cStat = "2";
                xMotivo = "Não foi possível realizar a consulta dos certificados digitais instalados no computador onde o " + Propriedade.NomeAplicacao + " está instalado. (Erro: " + ex.GetAllMessages() + ")";
                lErro = true;
                File.Delete(tmp_arqRet);
            }
            finally
            {
                var cArqRetorno = Propriedade.PastaGeralRetorno + "\\" + arqRet;

                #region XML de Retorno para ERP

                try
                {
                    var oArqRetorno = new FileInfo(cArqRetorno);
                    if (oArqRetorno.Exists == true)
                    {
                        oArqRetorno.Delete();
                    }

                    if (!lConsultar && !lErro)
                    {
                        cStat = "3";
                        xMotivo = "Não foi possível realizar a consulta dos certificados digitais instalados no computador onde o " + Propriedade.NomeAplicacao + " está instalado. (xServ não identificado)";
                    }

                    if (lErro || !lConsultar)
                    {
                        File.Delete(tmp_arqRet);

                        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                                                new XElement("retCadConfUniNFe",
                                                    new XElement(NFe.Components.TpcnResources.cStat.ToString(), cStat),
                                                    new XElement(NFe.Components.TpcnResources.xMotivo.ToString(), xMotivo)));
                        xml.Save(cArqRetorno);
                    }
                    else
                    {
                        if (File.Exists(cArqRetorno))
                        {
                            File.Delete(cArqRetorno);
                        }

                        if (File.Exists(arquivo))
                        {
                            File.Delete(arquivo);
                        }

                        File.Move(tmp_arqRet, Propriedade.PastaGeralRetorno + "\\" + arqRet);
                    }
                }
                catch (Exception ex)
                {
                    //Ocorreu erro na hora de gerar o arquivo de erro para o ERP
                    var oAux = new Auxiliar();
                    oAux.GravarArqErroERP(Path.GetFileNameWithoutExtension(cArqRetorno) + ".err", xMotivo + Environment.NewLine + ex.Message);
                }

                #endregion XML de Retorno para ERP
            }
        }

        #endregion CertificadosInstalados()        

        private bool IsPastaEnviadoPath(string pasta, string PastaEnviado)
        {
            if (pasta.Trim().ToLower() == PastaEnviado.Trim().ToLower() ||
                pasta.Trim().ToLower() == (PastaEnviado + "\\" + PastaEnviados.Autorizados.ToString()).Trim().ToLower() ||
                pasta.Trim().ToLower() == (PastaEnviado + "\\" + PastaEnviados.Denegados.ToString()).Trim().ToLower() ||
                pasta.Trim().ToLower() == (PastaEnviado + "\\" + PastaEnviados.EmProcessamento.ToString()).Trim().ToLower())
            {
                return true;
            }

            if (PastaEnviado.Trim().ToLower().Contains(pasta.Trim().ToLower()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Verifica se CNPJ do certificado é o mesmo da empresa e exibe aviso
        /// </summary>
        /// <param name="certificado">Certificado a ser verificado</param>
        /// <param name="docCadastroEmpresa"> CNPJ ou CPF da empresa cadastrado</param>
        public bool EhIgualDocumento(string certificado, string docCadastroEmpresa)
        {

            if (certificado == null || string.IsNullOrEmpty(docCadastroEmpresa))
            {
                throw new Exception("Ocorreu um erro ao tentar ler o certificado ou a documentação da empresa");
            }

            try
            {
                var docCertificado = ExtrairCNPJCPFCertificado(certificado);

                if (string.IsNullOrEmpty(docCertificado))
                {
                    throw new Exception("O certificado digital não contém CNPJ ou CPF válido.");
                }
                if (!CNPJCPFMesmaRaiz(docCertificado, docCadastroEmpresa))
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Ocorreu um erro ao tentar ler o conteúdo do CNPJ: " + ex.Message);
            }

            return true;
        }

        /// <summary>
        /// Extrair CNPJ do certificado
        /// </summary>
        /// <param name="certificado">Certificado</param>
        /// <returns>CNPJ encontrado no certificado, ou string vazia</returns>
        private string ExtrairCNPJCPFCertificado(string certificado)
        {
            if (certificado == null)
            {
                return string.Empty;
            }

            try
            {
                var m = Regex.Match(certificado, @"\b(\d{10,14})\b");
                if (m.Success)
                {
                    string digits = m.Groups[1].Value;
                    int target = digits.Length <= 11 ? 11 : 14;
                    return digits.PadLeft(target, '0');
                }
            }
            catch
            {
                //Caso erro, não interrompe o processo
            }

            return string.Empty;

        }

        /// <summary>
        /// Verifica se os CNPJs possuem a mesma raiz (8 primeiros dígitos)
        /// </summary>
        /// <param name="docCertificado">CNPJ ou CPF do certificado</param>
        /// <param name="docCadastroEmpresa">CNPJ ou CPF da empresa</param>
        /// <returns>True se mesma raiz, false se contrário</returns>
        private bool CNPJCPFMesmaRaiz(string docCertificado, string docCadastroEmpresa)
        {
            if (string.IsNullOrEmpty(docCertificado) || string.IsNullOrEmpty(docCadastroEmpresa))
            {
                return false;
            }

            docCertificado = Regex.Replace(docCertificado, @"[^\d]", "");
            docCadastroEmpresa = Regex.Replace(docCadastroEmpresa, @"[^\d]", "");

            if (docCadastroEmpresa.Length == 14 && docCertificado.Length == 14)
            {
                return docCertificado.Substring(0, 8) == docCadastroEmpresa.Substring(0, 8);
            }
            else if (docCadastroEmpresa.Length == 11 && docCertificado.Length == 11)
            {
                return docCertificado == docCadastroEmpresa;
            }
            else
            {
                return false; // CNPJ e CPF devem ter o mesmo tamanho
            }
        }


        #endregion Métodos gerais
    }

    #endregion Classe ConfiguracaoApp
}