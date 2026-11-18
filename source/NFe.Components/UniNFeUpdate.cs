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
    public class UniNFeUpdate : IDisposable
    {
        #region Private Fields

        private readonly string nomeInstalador;
        private readonly string pastaInstalar;
        private readonly string localInstalador;
        private bool downloadConcluido;
        private string url;
        private readonly HttpClient httpClient = new HttpClient();
        private const int TimeoutAtualizacao = 10000;

        #endregion Private Fields

        #region Propriedades

        public IWebProxy Proxy { get; set; }
        public bool InstaladorIniciado { get; private set; }

        protected virtual bool ServicoEmExecucao => Propriedade.ServicoRodando;
        protected virtual string VersaoAtual => Propriedade.Versao;

        #endregion Propriedades

        #region Public Constructor

        public UniNFeUpdate(IWebProxy proxy) : this(proxy, Application.StartupPath)
        {
        }

        protected UniNFeUpdate(IWebProxy proxy, string pasta)
        {
#if _BETA
            nomeInstalador = "iuninfe5_beta.exe";
#else
            nomeInstalador = "iuninfe5.exe";
#endif

            pastaInstalar = pasta;
            localInstalador = Path.Combine(pastaInstalar, nomeInstalador);

            Proxy = proxy;
            httpClient.Timeout = TimeSpan.FromMilliseconds(TimeoutAtualizacao);
        }

        #endregion Public Constructor

        #region Métodos auxiliares

        protected virtual void Download(Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            var links = BuscaURL();
            foreach (var link in links)
            {
                url = link + "/" + nomeInstalador;
                var downloadCompleto = false;
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    var webRequest = (HttpWebRequest)WebRequest.Create(url);
                    try
                    {
                        webRequest.Timeout = TimeoutAtualizacao;
                        webRequest.ReadWriteTimeout = TimeoutAtualizacao;

                        if (Proxy != null)
                        {
                            webRequest.Proxy = Proxy;
                        }

                        webRequest.Credentials = CredentialCache.DefaultCredentials;
                        using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                        using (var strResponse = webResponse.GetResponseStream())
                        using (var strLocal = new FileStream(localInstalador, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var fileSize = webResponse.ContentLength;
                            var downBuffer = new byte[8192];
                            int bytesSize;
                            var updateProgressArgs = new UpdateProgessEventArgs { BytesRead = 0, TotalBytes = fileSize };
                            while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                            {
                                strLocal.Write(downBuffer, 0, bytesSize);
                                updateProgressArgs.BytesRead = strLocal.Length;
                                InvokeProgress(updateProgressAction, updateProgressArgs);
                            }
                            if (strLocal.Length == 0 || (fileSize >= 0 && strLocal.Length != fileSize))
                            {
                                throw new IOException("O download do instalador está incompleto.");
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
        protected virtual List<string> BuscaURL()
        {
            var client = httpClient;
            if (!client.DefaultRequestHeaders.Contains("X-token"))
            {
                client.DefaultRequestHeaders.Add("X-token", "49edd27c-175d-801b-96b9-c4c0961e6a5a");
            }

            var responseString = client.GetStringAsync("https://www.unimake.com.br/webapi/autoupdate/dv/v2/req_getdownloadservers.php").GetAwaiter().GetResult();
            var response = new XmlDocument();
            response.LoadXml(responseString);
            var node = response.GetElementsByTagName("enderecohttp");
            var links = new List<string>();
            foreach (XmlElement link in node)
            {
                links.Add(link.InnerText);
            }

            if (links.Count == 0)
            {
                throw new IOException("Nenhum servidor de atualização está disponível.");
            }

            return links;
        }

        private DateTime AtualizaData(DateTime data)
        {
            var localArq = Path.Combine(pastaInstalar, "UltimaAtualizacao.xml");
            data = data.AddDays(30);

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var xmlElement = new XElement("UltimaAtualizacao");
            xmlElement.Add(new XElement("data", data));
            xml.Add(xmlElement);
            xml.Save(localArq);

            return data;
        }

        protected virtual void DownloadArquivo(string url, string destino, Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                webRequest.Timeout = TimeoutAtualizacao;
                webRequest.ReadWriteTimeout = TimeoutAtualizacao;

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
                        InvokeProgress(updateProgressAction, updateProgressArgs);
                    }
                }
            }
            finally
            {
                webRequest.Abort();
            }
        }

        private void InvokeProgress(Action<UpdateProgessEventArgs> callback, UpdateProgessEventArgs args)
        {
            if (callback == null) return;
            if (callback.Target is Control control && control.InvokeRequired)
            {
                control.Invoke(callback, args);
            }
            else
            {
                callback(args);
            }
        }

        #endregion Métodos auxiliares

        #region Public Methods

        public void Baixar(Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            downloadConcluido = false;
            Download(updateProgressAction);
            if (!File.Exists(localInstalador) || new FileInfo(localInstalador).Length == 0)
            {
                throw new IOException("O download do instalador não foi concluído.");
            }
            AtualizaData(DateTime.Now);
            downloadConcluido = true;
        }

        // Preserva a atualização solicitada por arquivo ERP.
        public void Instalar(Action<UpdateProgessEventArgs> updateProgressAction = null)
        {
            if (!ServicoEmExecucao)
            {
                Baixar(updateProgressAction);
                InstalarBaixado();
            }
        }

        public void InstalarBaixado()
        {
            try
            {
                if (!downloadConcluido || !File.Exists(localInstalador))
                {
                    throw new InvalidOperationException("Conclua o download antes de iniciar o instalador.");
                }

                if (ServicoEmExecucao)
                {
                    throw new InvalidOperationException("Pare o serviço do UniNFe antes de iniciar o instalador.");
                }

                ExecutarInstalador(localInstalador, "/SILENT /DIR=\"" + pastaInstalar + "\"");
                InstaladorIniciado = true;
            }
            catch (Exception ex)
            {
                Functions.WriteLog($"Erro ao instalar update: {ex.Message}", true, true, "");
                throw;
            }
        }

        protected virtual void ExecutarInstalador(string arquivo, string parametros)
        {
            using (var processo = Process.Start(arquivo, parametros))
            {
                if (processo == null)
                {
                    throw new InvalidOperationException("Não foi possível iniciar o instalador do UniNFe.");
                }
            }
        }

        public bool VerificaVersao()
        {
            var localArq = Path.Combine(pastaInstalar, "UltimaAtualizacao.xml");
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
                        var destino = Path.Combine(pastaInstalar, "versaouninfe.xml");
                        DownloadArquivo(versaoUrl, destino);

                        var xml = new XmlDocument();
                        xml.Load(destino);
                        XmlNode NoVersao = xml.GetElementsByTagName("versao")[0];
                        return Convert.ToInt64(NoVersao.InnerText.Replace(".", "")) > Convert.ToInt64(VersaoAtual.Replace(".", ""));
                    }
                    catch (Exception ex)
                    {
                        Functions.WriteLog($"Erro ao verificar versão: {ex.Message}", true, true, "");

                        if (link == links[links.Count - 1]) throw;
                    }
                }
            }

            return false;
        }

        public void Dispose() => httpClient.Dispose();

        #endregion Public Methods
    }

    #region Argumentos

    public class UpdateProgessEventArgs : EventArgs
    {
        public long BytesRead;
        public long TotalBytes;
        public int ProgressPercentage => TotalBytes <= 0 ? 0 : (int)Math.Min(100, Math.Max(0, BytesRead * 100.0 / TotalBytes));
    }

    #endregion Argumentos
}
