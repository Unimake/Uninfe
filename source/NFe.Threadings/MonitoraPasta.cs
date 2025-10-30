using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace NFe.Threadings
{
    /// <summary>
    /// Executar as thread´s dos serviços do UniNFe e NFe conforme necessário
    /// </summary>
    /// <remarks>
    /// Autor: Wandrey Mundin Ferreira
    /// Data: 04/04/2011
    /// </remarks>
    public class MonitoraPasta
    {
        #region Construtores

        /// <summary>
        /// Construtor
        /// </summary>
        public MonitoraPasta() => MonitorarPasta();

        #endregion Construtores

        #region Propriedades

        /// <summary>
        /// Tipos de arquivos a serem monitorados pelo UniNFe
        /// </summary>
        public static List<FileSystemWatcher> fsw = new List<FileSystemWatcher>();

        #endregion Propriedades
        private static void RestartService()
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servico_reiniciar.bat");
            p.Start();
        }


        #region MonitorarPastas()

        /// <summary>
        /// Executa as thread´s de monitoração de pastas
        /// </summary>
        private void MonitorarPasta()
        {
            #region Vou verificar se na pasta geral tem o arquivo para forçar fechar o uninfe, se tiver, vou excluir, pois acabei de entrar no sistema

            var dirInfo = new DirectoryInfo(Components.Propriedade.PastaGeral);
            var files = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

            foreach (var fi in files)
            {
                var sext = Components.Propriedade.Extensao(Components.Propriedade.TipoEnvio.sair_XML);
                if (fi.Name.EndsWith(sext.EnvioTXT) || fi.Name.EndsWith(sext.EnvioXML))
                {
                    fi.Delete();
                }
            }

            #endregion

            fsw.Clear();

            for (var i = 0; i < Empresas.Configuracoes.Count; i++)
            {
                var pastas = new List<string>
                {
                    Empresas.Configuracoes[i].PastaXmlEnvio
                };

                if (!string.IsNullOrEmpty(Empresas.Configuracoes[i].PastaXmlEmLote) &&
                    Empresas.Configuracoes[i].Servico != Components.TipoAplicativo.Nfse)
                {
                    pastas.Add(Empresas.Configuracoes[i].PastaXmlEmLote);
                }

                pastas.Add(Empresas.Configuracoes[i].PastaValidar);

                #region Pasta impressão dfe em contingência

                if (Directory.Exists(Empresas.Configuracoes[i].PastaContingencia))
                {
                    pastas.Add(Empresas.Configuracoes[i].PastaContingencia);
                }

                #endregion Pasta impressão dfe em contingência

                fsw.Add(new FileSystemWatcher(pastas, "*.xml,*.txt"));
                fsw[fsw.Count - 1].OnFileChanged += new FileSystemWatcher.FileChangedHandler(fsw_OnFileChanged);
                fsw[fsw.Count - 1].StartWatch(i);
            }

            #region Pasta Geral            

            fsw.Add(new FileSystemWatcher(Components.Propriedade.PastaGeral, "*.xml,*.txt"));
            fsw[fsw.Count - 1].OnFileChanged += new FileSystemWatcher.FileChangedHandler(fsw_OnFileChanged);
            fsw[fsw.Count - 1].StartWatch(-1);

            #endregion Pasta Geral
        }

        #endregion MonitorarPastas()

        #region LocalizaEmpresa()

        /// <summary>
        /// Localiza a empresa da qual o arquivo faz parte para processar com as configurações corretas
        /// </summary>
        /// <param name="fullPath">Nome do arquivo completo (com pasta e tudo)</param>
        /// <returns>Index da empresa dentro da lista de configurações</returns>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 26/04/2011
        /// </remarks>
        private int LocalizaEmpresa(FileInfo fi)
        {
            var empresa = -1;

            try
            {
                ///
                ///danasa 3/11/2011
                var fullName = ConfiguracaoApp.RemoveEndSlash(fi.Directory.FullName.ToLower());
                ///
                /// "EndsWith" é para pegar apenas se terminar com, já que nas empresas pode ter um nome 'temp' no meio das definicoes das pastas
                if (fullName.EndsWith("\\temp"))
                {
                    /// exclui o 'arquivo' temp.
                    fullName = Path.GetDirectoryName(fullName);
                }

                for (var i = 0; i < Empresas.Configuracoes.Count; i++)
                {
                    if (fullName == Empresas.Configuracoes[i].PastaXmlEnvio.ToLower() ||
                        fullName == Empresas.Configuracoes[i].PastaXmlEmLote.ToLower() ||
                        fullName == Empresas.Configuracoes[i].PastaValidar.ToLower() ||
                        fullName == Empresas.Configuracoes[i].PastaContingencia.ToLower())
                    {
                        empresa = i;
                        break;
                    }
                }
            }
            catch
            {
            }

            return empresa;
        }

        #endregion LocalizaEmpresa()

        #region Eventos

        /// <summary>
        /// Evento que executa thread´s para processar os arquivos que são colocados nas pastas que estão sendo monitoradas pela FileSystemWatcher
        /// </summary>
        /// <param name="fi">Arquivo detectado pela FileSystemWatcher</param>
        private void fsw_OnFileChanged(FileInfo fi)
        {
            try
            {
                int empresa;
                var arq = fi.FullName.ToLower();

                if (fi.Directory.FullName.ToLower().EndsWith("geral\\temp"))
                {
                    ///
                    /// encerra o UniNFe no arquivo -sair.xml
                    ///
                    var sext = Components.Propriedade.Extensao(Components.Propriedade.TipoEnvio.sair_XML);
                    if (arq.EndsWith(sext.EnvioTXT) || arq.EndsWith(sext.EnvioXML))
                    {
                        File.Delete(fi.FullName);
                        Empresas.ClearLockFiles(false);
                        if (!Components.Propriedade.ExecutandoPeloUniNFe)
                        {
                            if (Components.ServiceProcess.StatusService(Components.Propriedade.ServiceName) == System.ServiceProcess.ServiceControllerStatus.Running)
                            {
                                Components.ServiceProcess.StopService(Components.Propriedade.ServiceName, 40000);
                            }
                        }
                        else
                        {
                            ThreadService.Stop();
                        }

                        ThreadService.NotifyIconUniNFe.Visible = false; //Esta propriedade recebe a notifyIcon1 do Form_Main.cs da NFe.UI, então o conteúdo alterado aqui afeta a notify da Form_Main.cs, esta foi a forma que encontramos de limpar o ícone do systray quando é forçado fechar o aplicativo.
                        Environment.Exit(0);

                        return;
                    }

                    string ExtRetorno = null;
                    string finalArqErro = null;
                    Exception exx = null;

                    ///
                    /// restart o UniNFe
                    ///
                    var uext = Components.Propriedade.Extensao(Components.Propriedade.TipoEnvio.pedRestart);
                    if (arq.EndsWith(uext.EnvioTXT) || arq.EndsWith(uext.EnvioXML))
                    {
                        #region ---Reinicia o UniNFe

                        File.Delete(fi.FullName);
                        try
                        {

                            if (Propriedade.ServicoRodando)
                            {
                                RestartService();
                            }
                            else
                            {
                                System.Diagnostics.Process.Start(Components.Propriedade.PastaExecutavel + "\\uninfe.exe", "/restart");
                            }

                            return;
                        }
                        catch (Exception ex)
                        {
                            ExtRetorno = (arq.EndsWith(".xml") ? uext.EnvioXML : uext.EnvioTXT);
                            finalArqErro = uext.EnvioXML.Replace(".xml", ".err");
                            exx = ex;
                        }

                        #endregion ---Reinicia o UniNFe
                    }

                    if (ExtRetorno != null)
                    {
                        try
                        {
                            Service.TFunctions.GravarArqErroServico(fi.FullName, ExtRetorno, finalArqErro, exx);
                        }
                        catch { }
                        return;
                    }

                    ///
                    /// solicitacao de layouts
                    ///
                    var lext = Components.Propriedade.Extensao(Components.Propriedade.TipoEnvio.pedLayouts);
                    if (arq.EndsWith(lext.EnvioTXT) || arq.EndsWith(lext.EnvioXML))
                    {
                        var l = new Service.TaskLayouts
                        {
                            NomeArquivoXML = fi.FullName
                        };
                        l.Execute();
                        return;
                    }
                    empresa = 0; //Vou criar fixo como 0 quando for na pasta geral, pois na pasta geral não tem como detectar qual é a empresa. Wandrey 20/03/2013
                }
                else
                {
                    empresa = LocalizaEmpresa(fi);
                }

                if (empresa >= 0)
                {
                    /*<#8084>
                     * Aqui foi modificado porque a ThreadControl deixou de existir.
                     * E todo o processamento que antes existia na thread control foi deixado apenas no método Run(), que é chamado abaixo
                     *
                     * Marcelo
                     */
                    new ThreadItem(fi, empresa).Run();
                    //</#8084>
                }
                else
                {
                    Auxiliar.WriteLog(fi.FullName + " - Não localizou a empresa.", true);
                }
            }
            catch (Exception ex)
            {
                if (fi.Directory.Name.ToLower().EndsWith("geral\\temp"))
                {
                    Components.Functions.WriteLog(ex.Message + "\r\n" + ex.StackTrace, false, true, "");
                }
                else
                {
                    Auxiliar.WriteLog(ex.Message + "\r\n" + ex.StackTrace, false);
                }
            }
        }

        #endregion Eventos
    }
}