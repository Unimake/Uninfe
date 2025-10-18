using NFe.Settings;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace NFe.Components.Info
{
    public class Aplicacao
    {
        /// <summary>
        /// Grava XML com algumas informações do aplicativo, dentre elas os dados do certificado digital configurado nos parâmetros, versão, última modificação, etc.
        /// </summary>
        /// <param name="sArquivo">Pasta e nome do arquivo XML a ser gravado com as informações</param>
        public void GravarXMLInformacoes(string sArquivo, bool somenteConfigGeral)
        {
            int emp = Empresas.FindEmpresaByThread();

            string cStat = "1";
            string xMotivo = "Consulta efetuada com sucesso";

            //Ler os dados do certificado digital
            string sSubject = "";
            string sValIni = "";
            string sValFin = "";

            if (!somenteConfigGeral)
            {
                var cert = new Unimake.Business.Security.CertificadoDigital();
                X509Certificate2 certificadoEncontrado = null;

                if (Empresas.Configuracoes[emp].UsaCertificado)
                {
                    try
                    {
                        certificadoEncontrado = Empresas.Configuracoes[emp].X509Certificado;

                        sSubject = cert.GetSubject(certificadoEncontrado);
                        sValIni = cert.GetNotBefore(certificadoEncontrado);
                        sValFin = cert.GetNotAfter(certificadoEncontrado);
                    }
                    catch (Exception)
                    {
                        cStat = "2";
                        xMotivo = "Certificado digital não foi localizado";
                    }
                }
                else
                    xMotivo = "Empresa sem certificado digital informado e/ou não necessário";
            }

            //danasa 22/7/2011
            //pega a data da ultima modificação do 'uninfe.exe' diretamente porque pode ser que esteja sendo executado o serviço
            //então, precisamos dos dados do uninfe.exe e não do serviço
            string dtUltModif;

            dtUltModif = File.GetLastWriteTime(Propriedade.NomeAplicacao + ".exe").ToString("dd/MM/yyyy - HH:mm:ss");

            object oXmlGravar = null;
            bool isXml = false;

            //Gravar o XML com as informações do aplicativo
            try
            {
                if (File.Exists(Path.GetFileNameWithoutExtension(sArquivo) + ".err"))
                {
                    File.Delete(Path.GetFileNameWithoutExtension(sArquivo) + ".err");
                }

                if (Path.GetExtension(sArquivo).ToLower() == ".txt")
                {
                    oXmlGravar = new System.IO.StringWriter();
                }
                else
                {
                    isXml = true;

                    XmlWriterSettings oSettings = new XmlWriterSettings();
                    UTF8Encoding c = new UTF8Encoding(true);

                    //Para começar, vamos criar um XmlWriterSettings para configurar nosso XML
                    oSettings.Encoding = c;
                    oSettings.Indent = true;
                    oSettings.IndentChars = "";
                    oSettings.NewLineOnAttributes = false;
                    oSettings.OmitXmlDeclaration = false;

                    //Agora vamos criar um XML Writer
                    oXmlGravar = XmlWriter.Create(sArquivo, oSettings);
                }
                //Abrir o XML
                if (isXml)
                {
                    ((XmlWriter)oXmlGravar).WriteStartDocument();
                    ((XmlWriter)oXmlGravar).WriteStartElement("retConsInf");
                }
                Functions.GravaTxtXml(oXmlGravar, NFe.Components.TpcnResources.cStat.ToString(), cStat);
                Functions.GravaTxtXml(oXmlGravar, NFe.Components.TpcnResources.xMotivo.ToString(), xMotivo);

                if (!somenteConfigGeral)
                {
                    if (Empresas.Configuracoes[emp].UsaCertificado)
                    {
                        //Dados do certificado digital
                        if (isXml) ((XmlWriter)oXmlGravar).WriteStartElement("DadosCertificado");
                        Functions.GravaTxtXml(oXmlGravar, "sSubject", sSubject);
                        Functions.GravaTxtXml(oXmlGravar, "dValIni", sValIni);
                        Functions.GravaTxtXml(oXmlGravar, "dValFin", sValFin);
                        if (isXml) ((XmlWriter)oXmlGravar).WriteEndElement(); //DadosCertificado
                    }
                }

                //Dados gerais do Aplicativo
                if (isXml) ((XmlWriter)oXmlGravar).WriteStartElement("DadosUniNfe");
                Functions.GravaTxtXml(oXmlGravar, NFe.Components.TpcnResources.versao.ToString(), Propriedade.Versao);
                Functions.GravaTxtXml(oXmlGravar, "dUltModif", dtUltModif);
                Functions.GravaTxtXml(oXmlGravar, "PastaExecutavel", Propriedade.PastaExecutavel);
                Functions.GravaTxtXml(oXmlGravar, "NomeComputador", Environment.MachineName);
                Functions.GravaTxtXml(oXmlGravar, "UsuarioComputador", Environment.UserName);
                Functions.GravaTxtXml(oXmlGravar, "ExecutandoPeloServico", Propriedade.ServicoRodando.ToString());

                if (ConfiguracaoApp.ChecarConexaoInternet)
                {
                    Functions.GravaTxtXml(oXmlGravar, "ConexaoInternet", Functions.HasInternetConnection(ConfiguracaoApp.Proxy, ConfiguracaoApp.ProxyServidor, ConfiguracaoApp.ProxyUsuario, ConfiguracaoApp.ProxySenha, ConfiguracaoApp.ProxyPorta, ConfiguracaoApp.DetectarConfiguracaoProxyAuto).ToString());
                }
                else
                {
                    Functions.GravaTxtXml(oXmlGravar, "ConexaoInternet", "False");
                }

                if (isXml) ((XmlWriter)oXmlGravar).WriteEndElement(); //DadosUniNfe

                //Dados das configurações do aplicativo
                if (isXml) ((XmlWriter)oXmlGravar).WriteStartElement(NFeStrConstants.nfe_configuracoes);
                //Functions.GravaTxtXml(oXmlGravar, NFe.Components.NFeStrConstants.DiretorioSalvarComo, Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString());

                if (!somenteConfigGeral)
                {
                    bool hasFTP = false;
                    foreach (var pT in Empresas.Configuracoes[emp].GetType().GetProperties())
                    {
                        if (pT.CanWrite)
                        {
                            if (pT.Name.Equals("diretorioSalvarComo")) continue;

                            if (isXml)
                            {
                                if (!hasFTP && pT.Name.StartsWith("FTP"))
                                {
                                    ((XmlWriter)oXmlGravar).WriteStartElement("FTP");
                                    hasFTP = true;
                                }
                                else
                                    if (hasFTP && !pT.Name.StartsWith("FTP"))
                                {
                                    ((XmlWriter)oXmlGravar).WriteEndElement();
                                    hasFTP = false;
                                }
                            }
                            object v = pT.GetValue(Empresas.Configuracoes[emp], null);
                            NFe.Components.Functions.GravaTxtXml(oXmlGravar, pT.Name, v == null ? "" : v.ToString());
                        }
                    }
                    if (hasFTP && isXml) ((XmlWriter)oXmlGravar).WriteEndElement();
                }

                //Finalizar o XML
                if (isXml)
                {
                    ((XmlWriter)oXmlGravar).WriteEndElement(); //nfe_configuracoes
                    ((XmlWriter)oXmlGravar).WriteEndElement(); //retConsInf
                    ((XmlWriter)oXmlGravar).WriteEndDocument();
                    ((XmlWriter)oXmlGravar).Flush();
                    // throw new Exception("teste");
                }
                else
                {
                    ((StringWriter)oXmlGravar).Flush();
                    File.WriteAllText(sArquivo, ((StringWriter)oXmlGravar).GetStringBuilder().ToString());
                }
            }
            catch (Exception ex)
            {
                Functions.DeletarArquivo(sArquivo);
                ///
                /// danasa 8-2009
                ///
                Auxiliar oAux = new Auxiliar();
                oAux.GravarArqErroERP(Path.GetFileNameWithoutExtension(sArquivo) + ".err", ex.Message);
            }
            finally
            {
                if (oXmlGravar != null)
                {
                    if (isXml)
                    {

                        ((XmlWriter)oXmlGravar).Close();
                    }
                    else
                    {
                        ((StringWriter)oXmlGravar).Close();
                    }
                }
            }
        }

        #region AppExecutando()

        /// <summary>
        /// Verifica se a aplicação já está executando ou não
        /// </summary>
        /// <returns>True=Aplicação está executando</returns>
        public static Boolean UniNFeSevicoAppExecutando()
        {
            Propriedade.ExecutandoPeloUniNFe = false; //Executado pelo UniNfeServico

            try
            {
                Empresas.CarregaConfiguracao();

                Empresas.CanRun(null);

                // Se puder executar a aplicação aqui será recriado todos os arquivos de .lock,
                // pois podem ter sofridos alterações de configurações nas pastas
                Empresas.CreateLockFile();
            }
            catch (NFe.Components.Exceptions.AppJaExecutando ex)
            {
                Auxiliar.WriteLog(ex.Message, false);

                return true;
            }
            catch (NFe.Components.Exceptions.ProblemaExecucaoUniNFe ex)
            {
                Auxiliar.WriteLog(ex.Message, false);
            }
            catch
            {
                //Não preciso retornar nada, somente evito fechar o aplicativo
                //Esta exceção pode ocorrer quando não existe nenhuma empresa cadastrada
                //ainda, ou seja, é a primeira vez que estamos entrando no aplicativo
            }

            return false;
        }

        #endregion AppExecutando()

        #region AppExecutando()

        /// <summary>
        /// Verifica se a aplicação já está executando ou não
        /// </summary>
        /// <returns>True=Aplicação está executando</returns>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 21/07/2011
        /// </remarks>
        public static Boolean AppExecutando()
        {
            bool executando = false;

            Empresas.CarregaConfiguracao();

            try
            {
                Empresas.CanRun(null);

                // Se puder executar a aplicação aqui será recriado todos os arquivos de .lock,
                // pois podem ter sofridos alterações de configurações nas pastas
                Empresas.CreateLockFile();

                string procName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                if (System.Diagnostics.Process.GetProcessesByName(procName).Length > 1)
                {
                    executando = true;
                }
            }
            catch (NFe.Components.Exceptions.AppJaExecutando ex)
            {
                Empresas.ExisteErroDiretorio = true;
                Empresas.ErroCaminhoDiretorio = ex.Message;
                executando = true;
            }
            catch
            {
            }

            return executando;
        }

        #endregion AppExecutando()
    }
}