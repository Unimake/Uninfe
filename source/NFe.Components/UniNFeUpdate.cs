using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace NFe.Components
{
    public class UniNFeUpdate
    {
        #region Private Fields

        private Stream strLocal;
        private Stream strResponse;
        private HttpWebRequest webRequest;
        private HttpWebResponse webResponse;
        private readonly string nomeInstalador;
        private readonly string pastaInstalar;
        private string localArq;
        private string url;

        #endregion Private Fields

        #region Public Properties

        public IWebProxy Proxy { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public UniNFeUpdate(IWebProxy proxy)
        {
#if x86
            nomeInstalador = "iuninfe5_fw46_x86.exe";
#else
            nomeInstalador = "iuninfe5.exe";
#endif

            pastaInstalar = Application.StartupPath;
            localArq = Path.Combine(Application.StartupPath, nomeInstalador);

            Proxy = proxy;
        }

        #endregion Public Constructor

        #region Private Methods

        private void Download(Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            using (var Client = new WebClient())
            {
                var links = BuscaURL();

                for (var i = 0; i < links.Count; i++)
                {
                    url = links[i] + "/" + nomeInstalador;

                    var downloadCompleto = false;

                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                        localArq = Path.Combine(Application.StartupPath, nomeInstalador);

                        // Criar um pedido do arquivo que será baixado
                        webRequest = (HttpWebRequest)WebRequest.Create(url);

                        // Definir dados da conexao do proxy
                        if (Proxy != null)
                        {
                            webRequest.Proxy = Proxy;
                        }

                        // Atribuir autenticação padrão para a recuperação do arquivo
                        webRequest.Credentials = CredentialCache.DefaultCredentials;

                        // Obter a resposta do servidor
                        webResponse = (HttpWebResponse)webRequest.GetResponse();

                        // Perguntar ao servidor o tamanho do arquivo que será baixado
                        var fileSize = webResponse.ContentLength;

                        // Abrir a URL para download
                        strResponse = Client.OpenRead(url);

                        if (!File.Exists(localArq))
                        {
                            File.Create(localArq).Close();
                        }

                        // Criar um novo arquivo a partir do fluxo de dados que será salvo na local disk
                        strLocal = new FileStream(localArq, FileMode.Create, FileAccess.Write, FileShare.None);

                        // Ele irá armazenar o número atual de bytes recuperados do servidor
                        var bytesSize = 0;

                        // Um buffer para armazenar e gravar os dados recuperados do servidor
                        var downBuffer = new byte[2048];

                        var updateProgressArgs = new UpdateProgessEventArgs
                        {
                            BytesRead = bytesSize,
                            TotalBytes = fileSize
                        };

                        // Loop através do buffer - Até que o buffer esteja vazio
                        while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                        {
                            // Gravar os dados do buffer no disco rigido
                            strLocal.Write(downBuffer, 0, bytesSize);
                            updateProgressArgs.BytesRead = strLocal.Length;

                            // Invocar um método para atualizar a barra de progresso
                            if (updateProgressAction != null)
                            {
                                updateProgressAction.Invoke(updateProgressArgs);
                            }
                        }

                        downloadCompleto = true;
                    }
                    catch (IOException)
                    {
                        if (i + 1 >= links.Count)
                        {
                            throw;
                        }
                    }
                    catch (WebException)
                    {
                        if (i + 1 >= links.Count)
                        {
                            throw;
                        }
                    }
                    catch (Exception)
                    {
                        if (i + 1 >= links.Count)
                        {
                            throw;
                        }
                    }

                    finally
                    {
                        // Encerrar as streams
                        if (strResponse != null)
                        {
                            strResponse.Close();
                        }

                        if (strLocal != null)
                        {
                            strLocal.Close();
                        }

                        if (webRequest != null)
                        {
                            webRequest.Abort();
                            webResponse.Close();
                        }
                    }

                    if (downloadCompleto)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Busca a URL para download da atualização do UniNFe
        /// </summary>
        private Dictionary<int, string> BuscaURL()
        {
            var Request = new HttpClient
            {
                BaseAddress = new Uri("https://www.unimake.com.br/webapi/autoupdate/dv/v2/req_getdownloadservers.php")
            };
            Request.DefaultRequestHeaders.Add("X-token", "49edd27c-175d-801b-96b9-c4c0961e6a5a");
            var ResponseString = Request.GetStringAsync("").Result.ToString();

            var Response = new XmlDocument();
            Response.LoadXml(ResponseString);

            var node = Response.GetElementsByTagName("enderecohttp");

            var Links = new Dictionary<int, string>();
            var i = 0;
            foreach (XmlElement link in node)
            {
                Links.Add(i, link.InnerText);
                i++;
            }

            return Links;
        }

        private DateTime AtualizaData(DateTime data)
        {
            localArq = Path.Combine(Application.StartupPath, "UltimaAtualizacao.xml");

            //Adiciona 30 dias após a atualização para verificar se passou o tempo mínimo para atualizar
            data = data.AddDays(30);

            //cria o arquivo xml com a primeira tag "Ultima atualização"
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var xmlElement = new XElement("UltimaAtualizacao");

            xmlElement.Add(new XElement("data", data));


            xml.Add(xmlElement);
            xml.Save(localArq);

            return data;
        }

        #endregion Private Methods

        #region Public Methods

        public void Instalar(Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            try
            {
                // TODO WANDREY: Resolver atualização quando serviço. - Por hora não vai funcionar com serviços, vou ter que analisar melhor. Wandrey
                if (!Propriedade.ServicoRodando)
                {
                    Download(updateProgressAction);

                    //Se estiver rodando o Uninfe como serviço, no windows, temos que parar o serviço antes.
                    //if (Propriedade.ServicoRodando)
                    //{
                    //    var servico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servico_reiniciar.bat");
                    //    Process.Start(servico);
                    //}

                    var parametros = "/SILENT /DIR=" + "\"" + pastaInstalar + "\"";
                    Process.Start(localArq, parametros);
                }
            }
            catch (IOException)
            {
                throw;
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void VerificaVersao()
        {
            //path do arquivo em que fica salvo a data da ultima atualizacao
            localArq = Path.Combine(Application.StartupPath, "UltimaAtualizacao.xml");

            //variável para trabalhar na condição de tempo
            var data = new DateTime();

            if (File.Exists(localArq))
            {
                //carrega o xml, caso ele exista
                var xml = new XmlDocument();
                xml.Load(localArq);

                //Recupera o valor do nó dentro do xml
                var no = xml.GetElementsByTagName("data")[0];

                //Recupera a data como tipo DateTime para fazer a comparação
                data = DateTime.Parse(no.InnerText);
            }
            else
            {
                //"AtualizaData" também cria o primeiro arquivo de configuração de versão do UniNFe; A função de atualizar a data, serve para depois que atualizar o UniNFe  
                data = AtualizaData(DateTime.Now);

            }

            if (data <= DateTime.Now)
            {
                using (var Client = new WebClient())
                {
                    var links = BuscaURL();
                    var downloadCompleto = false;

                    for (var i = 0; i < links.Count; i++)
                    {
                        try
                        {
                            url = links[i] + "/versaouninfe.xml";
                            localArq = Path.Combine(Application.StartupPath, "versaouninfe.xml");

                            // Criar um pedido do arquivo que será baixado
                            webRequest = (HttpWebRequest)WebRequest.Create(url);

                            // Definir dados da conexao do proxy
                            if (Proxy != null)
                            {
                                webRequest.Proxy = Proxy;
                            }

                            // Atribuir autenticação padrão para a recuperação do arquivo
                            webRequest.Credentials = CredentialCache.DefaultCredentials;

                            // Obter a resposta do servidor
                            webResponse = (HttpWebResponse)webRequest.GetResponse();

                            // Perguntar ao servidor o tamanho do arquivo que será baixado
                            var fileSize = webResponse.ContentLength;

                            // Abrir a URL para download
                            strResponse = Client.OpenRead(url);

                            if (!File.Exists(localArq))
                            {
                                File.Create(localArq).Close();
                            }

                            // Criar um novo arquivo a partir do fluxo de dados que será salvo na local disk
                            strLocal = new FileStream(localArq, FileMode.Create, FileAccess.Write, FileShare.None);

                            // Ele irá armazenar o número atual de bytes recuperados do servidor
                            var bytesSize = 0;

                            // Um buffer para armazenar e gravar os dados recuperados do servidor
                            var downBuffer = new byte[2048];

                            var updateProgressArgs = new UpdateProgessEventArgs
                            {
                                BytesRead = bytesSize,
                                TotalBytes = fileSize
                            };

                            // Loop através do buffer - Até que o buffer esteja vazio
                            while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                            {
                                // Gravar os dados do buffer no disco rigido
                                strLocal.Write(downBuffer, 0, bytesSize);
                                updateProgressArgs.BytesRead = strLocal.Length;
                            }
                            strLocal.Close();

                            downloadCompleto = true;

                            //--------------------------------------//
                            //   ler os dados do xml e comparar 
                            //--------------------------------------//
                            //carregando o arquivo
                            var xml = new XmlDocument();
                            xml.Load(localArq);

                            //nó para a leitura
                            XmlNode NoVersao = null;
                            NoVersao = xml.GetElementsByTagName("versao")[0];

                            //Atualiza o Uninfe se a versão atual != da última versão disponibilizada
                            if (Convert.ToInt64(NoVersao.InnerText.Replace(".", "")) > Convert.ToInt64(Propriedade.Versao.Replace(".", "")))
                            {
                                var Result = MessageBox.Show("Deseja atualizar agora?", "Existe uma nova atualização do UniNFe", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                if (Result == DialogResult.Yes)
                                {
                                    //atualiza a data dentro do xml para conferir a versão do UniNFe apenas daqui a 30 dias
                                    data = AtualizaData(DateTime.Now);

                                    Instalar();
                                }
                            }

                        }
                        catch (IOException)
                        {
                            throw;
                        }
                        catch (WebException)
                        {
                            throw;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            // Encerrar as streams
                            if (strResponse != null)
                            {
                                strResponse.Close();
                            }

                            if (strLocal != null)
                            {
                                strLocal.Close();
                            }

                            if (webRequest != null)
                            {
                                webRequest.Abort();
                                webResponse.Close();
                            }
                        }

                        if (downloadCompleto)
                        {
                            break;
                        }
                    }
                }
            }
        }

        #endregion Public Methods
    }

    #region Argumentos

    public class UpdateProgessEventArgs : EventArgs
    {
        /// <summary>
        /// Bytes a serem lidos
        /// </summary>
        public long BytesRead;

        /// <summary>
        /// Total de bytes (tamanho) do arquivo que está sendo efetuado o download
        /// </summary>
        public long TotalBytes;

        /// <summary>
        /// Porcentagem de progresso do download do arquivo
        /// </summary>
        public int ProgressPercentage
        {
            get
            {
                var result = TotalBytes == 0 ? 0 : Convert.ToInt32((BytesRead * 100) / TotalBytes);
                return result;
            }
        }
    }

    #endregion Argumentos
}