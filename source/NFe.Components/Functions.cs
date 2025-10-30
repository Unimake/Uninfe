using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Components
{
    public static class Functions
    {
        #region MemoryStream

        /// <summary>
        /// Método responsável por converter uma String contendo a estrutura de um XML em uma Stream para
        /// ser lida pela XMLDocument
        /// </summary>
        /// <returns>String convertida em Stream</returns>
        /// <remarks>Conteúdo do método foi fornecido pelo Marcelo da desenvolvedores.net</remarks>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>20/04/2009</date>
        public static MemoryStream StringXmlToStream(string strXml)
        {
            var byteArray = new byte[strXml.Length];
            var encoding = new System.Text.ASCIIEncoding();
            byteArray = encoding.GetBytes(strXml);
            var memoryStream = new MemoryStream(byteArray);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public static MemoryStream StringXmlToStreamUTF8(string strXml)
        {
            var byteArray = new byte[strXml.Length];
            var encoding = new System.Text.UTF8Encoding();
            byteArray = encoding.GetBytes(strXml);
            var memoryStream = new MemoryStream(byteArray);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        #endregion MemoryStream

        #region Move()

        /// <summary>
        /// Mover arquivo para uma determinada pasta
        /// </summary>
        /// <param name="arquivoOrigem">Arquivo de origem (arquivo a ser movido)</param>
        /// <param name="arquivoDestino">Arquivo de destino (destino do arquivo)</param>
        public static void Move(string arquivoOrigem, string arquivoDestino)
        {
            if (!Directory.Exists(Path.GetDirectoryName(arquivoDestino)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(arquivoDestino));
            }
            else if (File.Exists(arquivoDestino))
            {
                File.Delete(arquivoDestino);
            }

            File.Move(arquivoOrigem, arquivoDestino);
        }

        #endregion Move()

        #region DeletarArquivo()

        /// <summary>
        /// Excluir arquivos do HD
        /// </summary>
        /// <param name="Arquivo">Nome do arquivo a ser excluido.</param>
        /// <date>05/08/2009</date>
        /// <by>Wandrey Mundin Ferreira</by>
        public static void DeletarArquivo(string arquivo)
        {
            if (File.Exists(arquivo))
            {
                File.Delete(arquivo);
            }
        }

        #endregion DeletarArquivo()

        #region CodigoParaUF()

        public static string CodigoParaUF(int codigo)
        {
            try
            {
                var es = Propriedade.Estados.First(s => s.CodigoMunicipio == codigo);
                return es.UF;
            }
            catch
            {
                return "";
            }
        }

        #endregion CodigoParaUF()

        #region UFParaCodigo()

        public static int UFParaCodigo(string uf)
        {
            try
            {
                var es = Propriedade.Estados.First(s => s.UF.Equals(uf));
                return es.CodigoMunicipio;
            }
            catch
            {
                return 0;
            }
        }

        #endregion UFParaCodigo()

        #region PadraoNFe()

        public static PadraoNFSe BuscaPadraoNFSe(int municipio)
        {
            var result = PadraoNFSe.None;

            foreach (var mun in Propriedade.Municipios)
            {
                if (mun.CodigoMunicipio == municipio)
                {
                    result = mun.Padrao;
                }
            }

            return result;
        }

        #endregion PadraoNFe()

        #region OnlyNumbers()

        /// <summary>
        /// Remove caracteres não-numéricos e retorna.
        /// </summary>
        /// <param name="text">valor a ser convertido</param>
        /// <returns>somente números com decimais</returns>
        public static object OnlyNumbers(object text)
        {
            var flagNeg = false;

            if (text == null || text.ToString().Length == 0)
            {
                return "";
            }

            var ret = "";

            foreach (var c in text.ToString().ToCharArray())
            {
                if (c.Equals('.') == true || c.Equals(',') == true || char.IsNumber(c) == true)
                {
                    ret += c.ToString();
                }
                else if (c.Equals('-') == true)
                {
                    flagNeg = true;
                }
            }

            if (flagNeg == true)
            {
                ret = "-" + ret;
            }

            return ret;
        }

        #endregion OnlyNumbers()

        #region OnlyNumbers()

        /// <summary>
        /// Remove caracteres não-numéricos e retorna.
        /// </summary>
        /// <param name="text">valor a ser convertido</param>
        /// <param name="additionalChars">caracteres adicionais a serem removidos</param>
        /// <returns>somente números com decimais</returns>
        public static object OnlyNumbers(object text, string removeChars)
        {
            var ret = OnlyNumbers(text).ToString();

            foreach (var c in removeChars.ToCharArray())
            {
                ret = ret.Replace(c.ToString(), "");
            }

            return ret;
        }

        #endregion OnlyNumbers()

        #region Gerar MD5

        public static string GerarMD5(string valor)
        {
            // Cria uma nova intância do objeto que implementa o algoritmo para
            // criptografia MD5
            var md5Hasher = System.Security.Cryptography.MD5.Create();

            // Criptografa o valor passado
            var valorCriptografado = md5Hasher.ComputeHash(Encoding.Default.GetBytes(valor));

            // Cria um StringBuilder para passarmos os bytes gerados para ele
            var strBuilder = new StringBuilder();

            // Converte cada byte em um valor hexadecimal e adiciona ao
            // string builder
            // and format each one as a hexadecimal string.
            for (var i = 0; i < valorCriptografado.Length; i++)
            {
                strBuilder.Append(valorCriptografado[i].ToString("x2"));
            }

            // retorna o valor criptografado como string
            return strBuilder.ToString();
        }

        #endregion Gerar MD5

        #region LerArquivo()

        /// <summary>
        /// Le arquivos no formato TXT
        /// Retorna uma lista do conteudo do arquivo
        /// </summary>
        /// <param name="cArquivo"></param>
        /// <returns></returns>
        public static List<string> LerArquivo(string cArquivo)
        {
            var lstRetorno = new List<string>();
            if (File.Exists(cArquivo))
            {
                using (var txt = new StreamReader(cArquivo, Encoding.Default, true))
                {
                    try
                    {
                        var cLinhaTXT = txt.ReadLine();
                        while (cLinhaTXT != null)
                        {
                            var dados = cLinhaTXT.Split('|');
                            if (dados.GetLength(0) > 1)
                            {
                                lstRetorno.Add(cLinhaTXT);
                            }
                            cLinhaTXT = txt.ReadLine();
                        }
                    }
                    finally
                    {
                        txt.Close();
                    }
                    if (lstRetorno.Count == 0)
                    {
                        throw new Exception("Arquivo: " + cArquivo + " vazio");
                    }
                }
            }
            return lstRetorno;
        }

        #endregion LerArquivo()

        #region ExtrairNomeArq()

        /// <summary>
        /// Extrai o nome do arquivo de uma determinada string. Este não mantem a pasta que ele está localizado, fica somente o nome do arquivo.
        /// </summary>
        /// <param name="arquivo">string contendo o caminho e nome do arquivo que é para ser extraído o conteúdo desejado</param>
        /// <param name="finalArq">string contendo o final do nome do arquivo que é para ser retirado do nome</param>
        /// <returns>Retorna somente o nome do arquivo de acordo com os parâmetros passado</returns>
        /// <example>
        /// MessageBox.Show(ExtrairNomeArq("C:\\TESTE\\NFE\\ENVIO\\ArqSituacao-ped-sta.xml", "-ped-sta.xml"));
        /// //Será demonstrado no message a string "ArqSituacao"
        ///
        /// MessageBox.Show(ExtrairNomeArq("C:\\TESTE\\NFE\\ENVIO\\ArqSituacao-ped-sta.xml", ".xml"));
        /// //Será demonstrado no message a string "ArqSituacao-ped-sta"
        /// </example>
        public static string ExtrairNomeArq(string arquivo, string finalArq)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                return "";
            }

            var fi = new FileInfo(arquivo);
            var ret = fi.Name;
            var retorno = "";

            if (!string.IsNullOrEmpty(finalArq) && finalArq.Length == 4 && finalArq.StartsWith("."))
            {
                return ret.Substring(0, ret.Length - finalArq.Length);
            }

            ///
            /// alteracao feita pq um usuario comentou que estava truncando uma parte do nome original do arquivo
            ///
            /// se o nome do arquivo for: 123456790-nfe.xml e
            ///             finalArq for:      -ret-nfe.xml, retornaria: 12345
            /// ou
            /// se o nome do arquivo for: 123456790-ret-nfe.xml e
            ///             finalArq for:              -nfe.xml, retornaria: 123456789-ret
            ///
            /*
                -pro-rec.err
                -pro-rec.xml
                -rec.err
                -rec.xml
             */

            ///
            /// pesquisa primeiro pela lista de retornos, porque geralmente os nomes são maiores que os de envio
            /// isso evita conflito de nomes como por ex: -cons-cad.xml x -ret-cons-cad.xml
            ///
            foreach (var pS in typeof(Propriedade.ExtRetorno).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var extensao = pS.GetValue(null).ToString();

                if (ret.EndsWith(extensao, StringComparison.InvariantCultureIgnoreCase))
                {
                    retorno = ret.Substring(0, ret.Length - extensao.Length);
                    break;
                }
            }

            if (retorno == "")
            {
                foreach (Propriedade.TipoEnvio item in Enum.GetValues(typeof(Propriedade.TipoEnvio)))
                {
                    var EXT = Propriedade.Extensao(item);

                    ///
                    /// pesquisa primeiro pelas extensões de retorno, pois geralmente, elas são maiores que as de envio
                    ///
                    if (!string.IsNullOrEmpty(EXT.RetornoXML))
                    {
                        if (ret.EndsWith(EXT.RetornoXML, StringComparison.InvariantCultureIgnoreCase))
                        {
                            retorno = ret.Substring(0, ret.Length - EXT.RetornoXML.Length);
                        }
                    }

                    if (!string.IsNullOrEmpty(EXT.RetornoTXT))
                    {
                        if (ret.EndsWith(EXT.RetornoTXT, StringComparison.InvariantCultureIgnoreCase))
                        {
                            retorno = ret.Substring(0, ret.Length - EXT.RetornoTXT.Length);
                        }
                    }

                    if (ret.EndsWith(EXT.EnvioXML, StringComparison.InvariantCultureIgnoreCase))
                    {
                        retorno = ret.Substring(0, ret.Length - EXT.EnvioXML.Length);
                    }

                    if (!string.IsNullOrEmpty(EXT.EnvioTXT))
                    {
                        if (ret.EndsWith(EXT.EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                        {
                            retorno = ret.Substring(0, ret.Length - EXT.EnvioTXT.Length);
                        }
                    }
                }
            }

            if (retorno == "")
            {
                if (!string.IsNullOrEmpty(finalArq))
                {
                    if (ret.EndsWith(finalArq, StringComparison.InvariantCultureIgnoreCase))
                    {
                        retorno = ret.Substring(0, ret.Length - finalArq.Length);
                    }
                }
            }

            if (retorno != "")
            {
                if (retorno.ToLower().EndsWith("-ped"))
                {
                    return retorno.Substring(0, retorno.ToLower().IndexOf("-ped"));
                }

                if (retorno.ToLower().EndsWith("-ret"))
                {
                    return retorno.Substring(0, retorno.ToLower().IndexOf("-ret"));
                }

                if (retorno.ToLower().EndsWith("-con"))
                {
                    return retorno.Substring(0, retorno.ToLower().IndexOf("-con"));
                }

                if (retorno.ToLower().EndsWith("-env"))
                {
                    return retorno.Substring(0, retorno.ToLower().IndexOf("-env"));
                }

                return retorno.TrimEnd(new char[] { '-' });
            }

            return fi.Name;
        }

        #endregion ExtrairNomeArq()

        public static string ExtractExtension(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return (value.IndexOf('.') >= 0 ? Path.ChangeExtension(("X" + value), "").Substring(1).Replace(".", "") : value);
        }

        public static string GetAttributeXML(string node, string attribute, string file)
        {
            var result = "";
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(file);

            var elementos = (XmlElement)conteudoXML.GetElementsByTagName(node)[0];
            if (elementos != null)
            {
                result = elementos.GetAttribute(attribute);
            }

            return result;
        }

        #region FileInUse()

        /// <summary>
        /// detectar se o arquivo está em uso
        /// </summary>
        /// <param name="file">caminho do arquivo</param>
        /// <returns>true se estiver em uso</returns>
        /// <by>http://desenvolvedores.net/marcelo</by>
        [System.Diagnostics.DebuggerHidden()]
        public static bool FileInUse(string file)
        {
            var ret = false;

            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Close();//fechar o arquivo para nao dar erro em outras aplicações
                }
            }
            catch
            {
                ret = true;
            }

            return ret;
        }

        #endregion FileInUse()

        #region LerTag()

        /// <summary>
        /// Busca o nome de uma determinada TAG em um Elemento do XML para ver se existe, se existir retorna seu conteúdo com um ponto e vírgula no final do conteúdo.
        /// </summary>
        /// <param name="Elemento">Elemento a ser pesquisado o Nome da TAG</param>
        /// <param name="NomeTag">Nome da Tag</param>
        /// <returns>Conteúdo da tag</returns>
        /// <date>05/08/2009</date>
        /// <by>Wandrey Mundin Ferreira</by>
        public static string LerTag(XmlElement Elemento, string NomeTag) => LerTag(Elemento, NomeTag, true);

        #endregion LerTag()

        #region LerTag()

        /// <summary>
        /// Busca o nome de uma determinada TAG em um Elemento do XML para ver se existe, se existir retorna seu conteúdo, com ou sem um ponto e vírgula no final do conteúdo.
        /// </summary>
        /// <param name="Elemento">Elemento a ser pesquisado o Nome da TAG</param>
        /// <param name="NomeTag">Nome da Tag</param>
        /// <param name="RetornaPontoVirgula">Retorna com ponto e vírgula no final do conteúdo da tag</param>
        /// <returns>Conteúdo da tag</returns>
        /// <date>05/08/2009</date>
        /// <by>Wandrey Mundin Ferreira</by>
        public static string LerTag(XmlElement Elemento, string NomeTag, bool RetornaPontoVirgula)
        {
            var Retorno = string.Empty;

            if (Elemento.GetElementsByTagName(NomeTag).Count != 0)
            {
                if (RetornaPontoVirgula)
                {
                    Retorno = Elemento.GetElementsByTagName(NomeTag)[0].InnerText.Replace(";", " ");  //danasa 19-9-2009
                    Retorno += ";";
                }
                else
                {
                    Retorno = Elemento.GetElementsByTagName(NomeTag)[0].InnerText;  //Wandrey 07/10/2009
                }
            }

            return Retorno;
        }

        public static string LerTag(XmlElement Elemento, string NomeTag, string defaultValue)
        {
            var result = LerTag(Elemento, NomeTag, false);
            if (string.IsNullOrEmpty(result))
            {
                result = defaultValue;
            }

            return result;
        }

        #endregion LerTag()

        /// <summary>
        /// Verifica a conexão com a internet e retorna verdadeiro se conectado com sucesso
        /// </summary>
        /// <returns></returns>
        public static bool HasInternetConnection(bool temProxy, string proxyServidor, string proxyUsuario, string proxySenha, int proxyPorta, bool proxyDetectarAutomaticamente = false) => Unimake.Net.Utility.HasInternetConnection((temProxy ? Unimake.Net.Utility.GetProxy(proxyServidor, proxyUsuario, proxySenha, proxyPorta, proxyDetectarAutomaticamente) : null));
                
        #region getDateTime()

        public static DateTime GetDateTime(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DateTime.MinValue;
            }

            int _ano = Convert.ToInt16(value.Substring(0, 4));
            int _mes = Convert.ToInt16(value.Substring(5, 2));
            int _dia = Convert.ToInt16(value.Substring(8, 2));
            if (value.Contains("T") && value.Contains(":"))
            {
                int _hora = Convert.ToInt16(value.Substring(11, 2));
                int _min = Convert.ToInt16(value.Substring(14, 2));
                int _seg = Convert.ToInt16(value.Substring(17, 2));
                return new DateTime(_ano, _mes, _dia, _hora, _min, _seg);
            }
            return new DateTime(_ano, _mes, _dia);
        }

        #endregion getDateTime()

        public static ArrayList CarregarMunicipios()
        {
            var municipios = new ArrayList();

            Propriedade.Municipios.ForEach((mun) => { municipios.Add(new ComboElem(mun.UF, mun.CodigoMunicipio, mun.Nome)); });

            municipios.Sort(new OrdenacaoPorNome());

            return municipios;
        }

        /// <summary>
        /// Carrega os municípios do arquivo de configuração na lista de municípios
        /// </summary>
        public static void CarregarMunicipio()
        {
            if (Propriedade.Municipios == null)
            {
                Propriedade.Municipios = new List<Municipio>();
            }

            if (Propriedade.Municipios.Count <= 0)
            {
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
            }
        }

        public static ArrayList CarregaEstados()
        {
            var UF = new ArrayList();
            foreach (var estado in Propriedade.Estados)
            {
                UF.Add(new ComboElem(estado.UF, estado.CodigoMunicipio, estado.Nome));
            }

            UF.Sort(new OrdenacaoPorNome());

            return UF;
        }              

        #region Ticket: #110

        /*
         * Marcelo
         * 03/06/2013
         */

        /// <summary>
        /// Retorna o endereço IP desta estação
        /// </summary>
        /// <returns>Endereço ip da estação</returns>
        public static string GetIPAddress()
        {
            var hostEntry = Dns.GetHostEntry(Environment.MachineName);
            var ip = (
                       from addr in hostEntry.AddressList
                       where addr.AddressFamily.ToString() == "InterNetwork"
                       select addr.ToString()
                ).FirstOrDefault();

            return ip;
        }

        #endregion Ticket: #110

        [System.Diagnostics.DebuggerHidden()]
        public static void CopyObjectTo(this object Source, object Destino)
        {
            foreach (var pS in Source.GetType().GetProperties())
            {
                if (!pS.CanWrite)
                {
                    continue;
                }

                foreach (var pT in Destino.GetType().GetProperties())
                {
                    if (!pT.Name.Equals(pS.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        (pT.GetSetMethod()).Invoke(Destino, new object[] { pS.GetGetMethod().Invoke(Source, null) });
                    }
                    catch //(Exception ex)
                    {
                        //Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public static bool SetProperty(object _this, string propName, object value)
        {
            try
            {
                var aProperties = _this.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
                if (aProperties != null)
                {
                    SetProperty(_this, aProperties, value);
                    return true;
                }
                return false;
            }
            catch //(Exception ex)
            {
                //Console.WriteLine("AAAAAAAAAAAAAA>>>>>>>>>"+ex.Message);
                return false;
            }
        }

        public static void SetProperty(object _this, PropertyInfo propertyInfo, object value)
        {
            if (value == null)
            {
                propertyInfo.SetValue(_this, null, null);
            }
            else
            {
                switch (propertyInfo.PropertyType.Name)
                {
                    case "Int32":
                        propertyInfo.SetValue(_this, Convert.ToInt32(value), null);
                        break;

                    case "String":
                        propertyInfo.SetValue(_this, value.ToString(), null);
                        break;

                    case "Boolean":
                        propertyInfo.SetValue(_this, Convert.ToBoolean(value), null);
                        break;

                    case "Double":
                        propertyInfo.SetValue(_this, Convert.ToDouble(value), null);
                        break;

                    case "Decimal":
                        propertyInfo.SetValue(_this, Convert.ToDecimal(value), null);
                        break;

                    case "DateTime":
                        propertyInfo.SetValue(_this, Convert.ToDateTime(value), null);
                        break;

                    default:
                        switch (propertyInfo.PropertyType.FullName)
                        {
                            case "NFe.Components.TipoAplicativo":
                                var ta1 = (NFe.Components.TipoAplicativo)Enum.Parse(typeof(NFe.Components.TipoAplicativo), value.ToString(), true);
                                propertyInfo.SetValue(_this, ta1, null);
                                break;

                            case "NFe.Components.TipoAmbiente":
                                var ta2 = (TipoAmbiente)Enum.Parse(typeof(TipoAmbiente), value.ToString(), true);
                                propertyInfo.SetValue(_this, ta2, null);
                                break;

                            case "NFe.Components.TipoEmissao":
                                var ta3 = (TipoEmissao)Enum.Parse(typeof(TipoEmissao), value.ToString(), true);
                                propertyInfo.SetValue(_this, ta3, null);
                                break;

                            case "NFe.Components.DiretorioSalvarComo":

                                //propertyInfo.SetValue(_this, value.ToString(), null);
                                break;

                            default:
                                throw new Exception(propertyInfo.Name + "..." + propertyInfo.PropertyType.FullName + "...." + value.ToString());
                        }
                        break;
                }
            }
        }

        private static bool populateClasse(object classe, string[] origem)
        {
            var allClassToProperties = classe.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

            var lEncontrou = false;
            foreach (var xi in origem)
            {
                if (!string.IsNullOrEmpty(xi))
                {
                    var xii = xi.Split(new char[] { '|' });
                    if (xii.Length == 2)
                    {
                        if (xii[0].Equals("DiretorioSalvarComo", StringComparison.InvariantCultureIgnoreCase))
                        {
                            xii[0] = "diretorioSalvarComo";
                        }

                        var pi = (from i in allClassToProperties where i.Name.Equals(xii[0], StringComparison.InvariantCultureIgnoreCase) select i).FirstOrDefault();
                        if (pi == null)
                        {
                            Console.WriteLine(xi + ": NOT FOUND");
                        }
                        else
                        {
                            Functions.SetProperty(classe, pi, xii[1]);
                            lEncontrou = true;
                        }
                    }
                }
            }
            return lEncontrou;
        }

        public static bool PopulateClasse(object classe, object origem)
        {
            var lEncontrou = false;

            if (origem.GetType().IsAssignableFrom(typeof(XmlElement)))
            {
                var temp = new List<string>();
                var xx = (origem as XmlElement).ChildNodes;
                foreach (var xa in xx)
                {
                    temp.Add((xa as XmlElement).Name + "|" + (xa as XmlElement).InnerText);
                }
                lEncontrou = populateClasse(classe, temp.ToArray());
            }
            else
            {
                if (origem.GetType().IsAssignableFrom(typeof(string[])))
                {
                    lEncontrou = populateClasse(classe, origem as string[]);
                }
                else
                    if (origem.GetType().IsAssignableFrom(typeof(List<string>)))
                {
                    lEncontrou = populateClasse(classe, (origem as List<string>).ToArray());
                }
                else
                {
                    throw new Exception("Tipo de dados da origem desconhecido. (" + origem.GetType().ToString() + ")");
                }
            }
            return lEncontrou;
        }

        public static void GravaTxtXml(object w, string fieldname, string content)
        {
            var method = w.GetType().GetMethod("WriteElementString", new Type[] { typeof(string), typeof(string) });
            if (method == null)
            {
                method = w.GetType().GetMethod("WriteLine", new Type[] { typeof(string) });
                method.Invoke(w, new object[] { fieldname + "|" + content });
            }
            else
            {
                method.Invoke(w, new object[] { fieldname, content });
            }
        }

        #region WriteLog()

        public static void WriteLog(string msg, bool gravarStackTrace, bool geraLog, string CNPJEmpresa)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine(msg);
#endif
            if (geraLog)
            {
                if (!string.IsNullOrEmpty(CNPJEmpresa))
                {
                    CNPJEmpresa += "_";
                }

                var fileName = Propriedade.PastaLog + "\\uninfe_" +
                    (string.IsNullOrEmpty(CNPJEmpresa) ? "" : CNPJEmpresa) +
                    DateTime.Now.ToString("yyyy-MMM-dd") + ".log";

                DateTime startTime;
                DateTime stopTime;
                TimeSpan elapsedTime;

                long elapsedMillieconds;
                startTime = DateTime.Now;

                while (true)
                {
                    stopTime = DateTime.Now;
                    elapsedTime = stopTime.Subtract(startTime);
                    elapsedMillieconds = (int)elapsedTime.TotalMilliseconds;

                    StreamWriter arquivoWS = null;
                    try
                    {
                        //Se for para gravar ot race
                        if (gravarStackTrace)
                        {
                            msg += "\r\nSTACK TRACE:";
                            msg += "\r\n" + Environment.StackTrace;
                        }

                        arquivoWS = new StreamWriter(fileName, true, Encoding.UTF8);
                        arquivoWS.WriteLine(DateTime.Now.ToLongTimeString() + " - [Versão UniNFe: " + Propriedade.Versao + "] - " + msg);
                        arquivoWS.Flush();
                        arquivoWS.Close();
                        break;
                    }
                    catch
                    {
                        if (arquivoWS != null)
                        {
                            arquivoWS.Close();
                        }

                        if (elapsedMillieconds >= 60000) //60.000 ms que corresponde á 60 segundos que corresponde a 1 minuto
                        {
                            break;
                        }
                    }
                    Thread.Sleep(2);
                }
            }
        }

        #endregion WriteLog()

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <param name="resultFolder"></param>
        /// <param name="ex"></param>
        public static void GravarErroMover(string file, string resultFolder, string ex)
        {
            if (!string.IsNullOrEmpty(resultFolder) && Directory.Exists(Path.GetDirectoryName(resultFolder)) && !string.IsNullOrEmpty(file))
            {
                var infFile = new FileInfo(file);
                var extFile = infFile.Name.Replace(infFile.Extension, "");
                var extError = extFile + ".err";

                var nomearq = resultFolder + "\\" + extError;

                var write = new StreamWriter(nomearq);
                write.Write(ex);
                write.Flush();
                write.Close();
                write.Dispose();
            }
            else
            {
                WriteLog(ex, false, true, "");
            }
        }

        /// <summary>
        /// Retorna um Base64 com 28 caracteres
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToBase64Hex(string value)
        {
            var countChars = value.Length;
            var bytes = new byte[countChars / 2];

            for (var i = 0; i < countChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            }

            return Convert.ToBase64String(bytes);
        }
    }
}