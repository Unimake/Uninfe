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

        private readonly string nomeInstalador;
        private readonly string pastaInstalar;
        private string localArq;
        private string url;
        private static readonly HttpClient StaticHttpClient = new HttpClient();

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
            var links = BuscaURL();
            foreach (var link in links)
            {
                url = link + "/" + nomeInstalador;
                var downloadCompleto = false;
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    localArq = Path.Combine(Application.StartupPath, nomeInstalador);

                    var webRequest = (HttpWebRequest)WebRequest.Create(url); // NÃO usar using
                    try
                    {
                        if (Proxy != null)
                        {
                            webRequest.Proxy = Proxy;
                        }

                        webRequest.Credentials = CredentialCache.DefaultCredentials;
                        using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                        using (var strResponse = webResponse.GetResponseStream())
                        using (var strLocal = new FileStream(localArq, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var fileSize = webResponse.ContentLength;
                            var downBuffer = new byte[8192]; // buffer maior para performance
                            int bytesSize;
                            var updateProgressArgs = new UpdateProgessEventArgs { BytesRead = 0, TotalBytes = fileSize };
                            while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                            {
                                strLocal.Write(downBuffer, 0, bytesSize);
                                updateProgressArgs.BytesRead = strLocal.Length;
                                updateProgressAction?.Invoke(updateProgressArgs);
                            }

                            downloadCompleto = true;
                        }
                    }
                    finally
                    {
                        webRequest.Abort();
                    }
                }
                catch (IOException ex)
                {
                    Functions.WriteLog($"Erro de IO ao baixar update: {ex.Message}", true, true, "");
                    if (link == links[links.Count - 1])
                    {
                        throw;
                    }
                }
                catch (WebException ex)
                {
                    Functions.WriteLog($"Erro de Web ao baixar update: {ex.Message}", true, true, "");
                    if (link == links[links.Count - 1])
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Functions.WriteLog($"Erro inesperado ao baixar update: {ex.Message}", true, true, "");
                    if (link == links[links.Count - 1])
                    { 
                        throw; 
                    }
                }

                if (downloadCompleto)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Busca a URL para download da atualização do UniNFe
        /// </summary>
        private List<string> BuscaURL()
        {
            var client = StaticHttpClient;
            client.BaseAddress = new Uri("https://www.unimake.com.br/webapi/autoupdate/dv/v2/req_getdownloadservers.php");
            if (!client.DefaultRequestHeaders.Contains("X-token"))
            {
                client.DefaultRequestHeaders.Add("X-token", "49edd27c-175d-801b-96b9-c4c0961e6a5a");
            }
            
            var responseString = client.GetStringAsync("").Result;
            var response = new XmlDocument();
            response.LoadXml(responseString);
            var node = response.GetElementsByTagName("enderecohttp");
            var links = new List<string>();
            foreach (XmlElement link in node)
            {
                links.Add(link.InnerText);
            }

            return links;
        }

        private DateTime AtualizaData(DateTime data)
        {
            localArq = Path.Combine(Application.StartupPath, "UltimaAtualizacao.xml");
            data = data.AddDays(30);
            
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var xmlElement = new XElement("UltimaAtualizacao");
            xmlElement.Add(new XElement("data", data));
            xml.Add(xmlElement);
            xml.Save(localArq);

            return data;
        }

        private void DownloadArquivo(string url, string destino, Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var webRequest = (HttpWebRequest)WebRequest.Create(url); // NÃO usar using
            try
            {
                if (Proxy != null)
                    webRequest.Proxy = Proxy;

                webRequest.Credentials = CredentialCache.DefaultCredentials;
                using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                using (var strResponse = webResponse.GetResponseStream())
                using (var strLocal = new FileStream(destino, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var fileSize = webResponse.ContentLength;
                    var downBuffer = new byte[8192];
                    int bytesSize;
                    var updateProgressArgs = new UpdateProgessEventArgs { BytesRead = 0, TotalBytes = fileSize };
                    while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                    {
                        strLocal.Write(downBuffer, 0, bytesSize);
                        updateProgressArgs.BytesRead = strLocal.Length;
                        updateProgressAction?.Invoke(updateProgressArgs);
                    }
                }
            }
            finally
            {
                webRequest.Abort();
            }
        }

        #endregion Private Methods

        #region Public Methods

        public void Instalar(Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            try
            {
                if (!Propriedade.ServicoRodando)
                {
                    Download(updateProgressAction);

                    var parametros = "/SILENT /DIR=\"" + pastaInstalar + "\"";
                    Process.Start(localArq, parametros);
                }
            }
            catch (Exception ex)
            {
                Functions.WriteLog($"Erro ao instalar update: {ex.Message}", true, true, "");
                throw;
            }
        }

        public void VerificaVersao()
        {
            localArq = Path.Combine(Application.StartupPath, "UltimaAtualizacao.xml");
            var data = new DateTime();
            if (File.Exists(localArq))
            {
                var xml = new XmlDocument();
                xml.Load(localArq);
                var no = xml.GetElementsByTagName("data")[0];
                data = DateTime.Parse(no.InnerText);
            }
            else
            {
                data = AtualizaData(DateTime.Now);
            }
            if (data <= DateTime.Now)
            {
                var links = BuscaURL();
                foreach (var link in links)
                {
                    try
                    {
                        var versaoUrl = link + "/versaouninfe.xml";
                        var destino = Path.Combine(Application.StartupPath, "versaouninfe.xml");
                        DownloadArquivo(versaoUrl, destino);

                        var xml = new XmlDocument();
                        xml.Load(destino);
                        XmlNode NoVersao = xml.GetElementsByTagName("versao")[0];
                        if (Convert.ToInt64(NoVersao.InnerText.Replace(".", "")) > Convert.ToInt64(Propriedade.Versao.Replace(".", "")))
                        {
                            var Result = MessageBox.Show("Deseja atualizar agora?", "Existe uma nova atualização do UniNFe", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (Result == DialogResult.Yes)
                            {
                                data = AtualizaData(DateTime.Now);
                                Instalar();
                            }
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Functions.WriteLog($"Erro ao verificar versão: {ex.Message}", true, true, "");

                        if (link == links[links.Count - 1]) throw;
                    }
                }
            }
        }

        #endregion Public Methods
    }

    #region Argumentos

    public class UpdateProgessEventArgs : EventArgs
    {
        public long BytesRead;
        public long TotalBytes;
        public int ProgressPercentage => TotalBytes == 0 ? 0 : Convert.ToInt32((BytesRead * 100) / TotalBytes);
    }

    #endregion Argumentos
}