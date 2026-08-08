using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    /// <summary>
    /// Integração com o aplicativo UniDANFE.
    /// </summary>
    public static class UniDanfe
    {
        #region RenomearXmlRelatorioEmail()

        private static void RenomearXmlRelatorioEmail(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }

            var sx = (string)e.Argument;
            var emp = Convert.ToInt32(sx.Split('|')[0]);
            var fm = sx.Split('|')[1];

            var relname = new string[]{
                    Empresas.Configuracoes[emp].PastaExeUniDanfe + "\\rel_email_enviar.xml",
                    Empresas.Configuracoes[emp].PastaExeUniDanfe + "\\rel_email_enviados.xml",
                    Empresas.Configuracoes[emp].PastaExeUniDanfe + "\\rel_email_erros.xml"
                };

            System.Threading.Thread.Sleep(1000);
            var passo = 0;
            while (!(sender as System.ComponentModel.BackgroundWorker).CancellationPending)
            {
                foreach (var s in relname)
                {
                    if (File.Exists(s))
                    {
                        if (!Functions.FileInUse(s))
                        {
                            var _out = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, fm.Replace(".txt", ".xml"));
                            if (File.Exists(_out))
                            {
                                File.Delete(_out);
                            }

                            File.Move(s, _out);
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                e.Cancel = (++passo > 10);
                System.Threading.Thread.Sleep(100);
            }
        }

        #endregion RenomearXmlRelatorioEmail()

        #region ExecutarRelatorioEmail()

        public static void ExecutarRelatorioEmail(int emp, DateTime datai, DateTime dataf, bool imprimir = false, string exportarPasta = "Enviados", string filename = "")
        {
            if (Empresas.Configuracoes[emp].PastaExeUniDanfe != string.Empty &&
                File.Exists(Empresas.Configuracoes[emp].PastaExeUniDanfe + "\\unidanfe.exe"))
            {
                System.Diagnostics.Process.Start(Empresas.Configuracoes[emp].PastaExeUniDanfe + "\\unidanfe.exe",
                    string.Format("rel_email=1 datai=\"{0:yyyy-MM-dd}\" dataf=\"{1:yyyy-MM-dd}\" imprimir={2} pasta=\"{3}\"",
                                    datai, dataf, imprimir ? 1 : 0, exportarPasta));

                if (!imprimir)
                {
                    var worker = new System.ComponentModel.BackgroundWorker
                    {
                        WorkerSupportsCancellation = true
                    };
                    worker.RunWorkerCompleted += ((sender, e) => ((System.ComponentModel.BackgroundWorker)sender).Dispose());
                    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(RenomearXmlRelatorioEmail);
                    worker.RunWorkerAsync(emp + "|" + filename);
                }
            }
        }

        #endregion ExecutarRelatorioEmail()

        private static string CaminhoArquivoEnviado(Empresa emp, PastaEnviados pasta, DateTime dataEmissao, int deslocamento, string nomeArquivo)
        {
            return Path.Combine(
                emp.PastaXmlEnviado,
                pasta.ToString(),
                TFunctions.getSubFolder(dataEmissao, deslocamento, emp.DiretorioSalvarComo),
                Path.GetFileName(nomeArquivo));
        }

        private static bool SalvaXmlNaRaiz(DiretorioSalvarComo diretorioSalvarComo)
        {
            var formatoDiretorio = diretorioSalvarComo.ToString();
            return string.IsNullOrEmpty(formatoDiretorio) || formatoDiretorio.Equals("Raiz");
        }

        private static string LocalizarArquivoDistribuicao(string nomeArquivoRecebido, string nomeArquivoDistribuicao, DateTime dataEmissao, Empresa emp)
        {
            var arquivoNoDiretorioRecebido = Path.Combine(Path.GetDirectoryName(nomeArquivoRecebido), Path.GetFileName(nomeArquivoDistribuicao));
            if (File.Exists(arquivoNoDiretorioRecebido))
            {
                return arquivoNoDiretorioRecebido;
            }

            if (string.IsNullOrEmpty(Path.GetDirectoryName(nomeArquivoDistribuicao)))
            {
                nomeArquivoDistribuicao = Path.Combine(
                    emp.PastaXmlEnviado,
                    PastaEnviados.Autorizados.ToString(),
                    emp.DiretorioSalvarComo.ToString(dataEmissao),
                    Path.GetFileName(nomeArquivoDistribuicao));
            }

            if (File.Exists(nomeArquivoDistribuicao))
            {
                return nomeArquivoDistribuicao;
            }

            for (var deslocamento = 0; deslocamento < 60; deslocamento++)
            {
                var arquivoLocalizado = CaminhoArquivoEnviado(emp, PastaEnviados.Autorizados, dataEmissao, deslocamento, nomeArquivoDistribuicao);
                if (!File.Exists(arquivoLocalizado))
                {
                    arquivoLocalizado = CaminhoArquivoEnviado(emp, PastaEnviados.Denegados, dataEmissao, deslocamento, nomeArquivoDistribuicao);
                }

                if (File.Exists(arquivoLocalizado))
                {
                    return arquivoLocalizado;
                }

                if (SalvaXmlNaRaiz(emp.DiretorioSalvarComo))
                {
                    break;
                }
            }

            return nomeArquivoDistribuicao;
        }

        private static string LocalizarArquivoCancelamento(string nomeArquivoRecebido, string chaveDFe, string tipo, string extensaoEvento, DateTime dataEmissao, Empresa emp)
        {
            for (var deslocamento = 0; deslocamento < 60; deslocamento++)
            {
                var nomeArquivoCancelamento = chaveDFe + string.Format("_{0}_01{1}", (int)NFe.ConvertTxt.tpEventos.tpEvCancelamentoNFe, extensaoEvento);
                var arquivoCancelamento = CaminhoArquivoEnviado(emp, PastaEnviados.Autorizados, dataEmissao, deslocamento, nomeArquivoCancelamento);

                if (!File.Exists(arquivoCancelamento) && (tipo.Equals("nfe") || tipo.Equals("ds")))
                {
                    nomeArquivoCancelamento = chaveDFe + "-procCancNFe.xml";
                    arquivoCancelamento = CaminhoArquivoEnviado(emp, PastaEnviados.Autorizados, dataEmissao, deslocamento, nomeArquivoCancelamento);
                }

                if (!File.Exists(arquivoCancelamento))
                {
                    var arquivoNoDiretorioRecebido = Path.Combine(Path.GetDirectoryName(nomeArquivoRecebido), Path.GetFileName(nomeArquivoCancelamento));
                    if (File.Exists(arquivoNoDiretorioRecebido))
                    {
                        arquivoCancelamento = arquivoNoDiretorioRecebido;
                    }
                }

                if (File.Exists(arquivoCancelamento))
                {
                    return arquivoCancelamento;
                }

                if ((!tipo.Equals("nfe") && !tipo.Equals("ds")) || SalvaXmlNaRaiz(emp.DiretorioSalvarComo))
                {
                    break;
                }
            }

            return null;
        }

        public static void Executar(string nomeArquivoRecebido, DateTime dataEmissaoNFe, Empresa emp, Dictionary<string, string> args = null)
        {
#if DEBUG
            Auxiliar.WriteLog("ExecutaUniDanfe: Preparando a execução do UniDANFe p/ o arquivo: \"" + nomeArquivoRecebido + "\"", false);
#endif
            const string erroMsg = "Arquivo {0} não encontrado para impressão do DANFE/DACTE/CCe/DAMDFe {1}";

            //Disparar a geração/impressão do UniDanfe. 03/02/2010 - Wandrey
            if (!string.IsNullOrEmpty(emp.PastaExeUniDanfe) &&
                File.Exists(Path.Combine(emp.PastaExeUniDanfe, "unidanfe.exe")))
            {
                Auxiliar.WriteLog("ExecutaUniDanfe: Preparando a execução do UniDANFe.", false);

                var arqProcNFe = string.Empty;
                var fExtensao = string.Empty;
                var fEmail = "";
                var tipo = "";
                var tempFile = "";
                var fAuxiliar = "";
                var epecTipo = "";
                var denegada = false;
                var temCancelamento = false;
                var isEPEC = false;
                var cancelamentoNfe = false;

                if (!File.Exists(nomeArquivoRecebido))
                {
                    throw new Exception(string.Format(erroMsg, nomeArquivoRecebido, ""));
                }

                var doc = new XmlDocument();
                doc.Load(nomeArquivoRecebido);

                switch (doc.DocumentElement.Name)
                {
                    case "nfcomProc":
                        arqProcNFe = nomeArquivoRecebido;
                        tipo = "nfcom";
                        break;

                    case "NFSe":
                        arqProcNFe = nomeArquivoRecebido;
                        tipo = "nfse";
                        break;

                    case "procInutNFe":
                        arqProcNFe = nomeArquivoRecebido;
                        tipo = "inut";
                        break;

                    case "nfeProc":
                        arqProcNFe = nomeArquivoRecebido;
                        break;

                    case "NFe":
                        foreach (var el3 in doc.GetElementsByTagName("ide"))
                        {
                            if (((XmlElement)el3).GetElementsByTagName(NFe.Components.TpcnResources.tpEmis.ToString())[0] != null)
                            {
                                tipo = ((XmlElement)el3).GetElementsByTagName(NFe.Components.TpcnResources.mod.ToString())[0].InnerText.Equals("55") ? "nfe" : "nfce";
                            }
                        }
                        arqProcNFe = nomeArquivoRecebido;
                        break;

                    case "cteOSProc":
                    case "CTeOS":
                        tipo = "cteos";
                        arqProcNFe = nomeArquivoRecebido;

                        ///
                        /// le o protocolo de autorizacao
                        ///
                        if (doc.DocumentElement.Name.Equals("cteOSProc"))
                        {
                            foreach (var el3 in doc.GetElementsByTagName("protCTe"))
                            {
                                if (((XmlElement)el3).GetElementsByTagName(NFe.Components.TpcnResources.cStat.ToString())[0] != null)
                                {
                                    var cStat = ((XmlElement)el3).GetElementsByTagName(NFe.Components.TpcnResources.cStat.ToString())[0].InnerText;
                                    switch (cStat)
                                    {
                                        //denegada
                                        case "110":
                                        case "301":
                                            denegada = true;
                                            break;
                                    }
                                    break;
                                }
                            }
                        }
                        break;

                    case "cteProc":
                    case "CTe":
                        tipo = "cte";
                        arqProcNFe = nomeArquivoRecebido;

                        ///
                        /// le o protocolo de autorizacao
                        ///
                        if (doc.DocumentElement.Name.Equals("cteProc"))
                        {
                            foreach (var el3 in doc.GetElementsByTagName("protCTe"))
                            {
                                if (((XmlElement)el3).GetElementsByTagName(NFe.Components.TpcnResources.cStat.ToString())[0] != null)
                                {
                                    var cStat = ((XmlElement)el3).GetElementsByTagName(NFe.Components.TpcnResources.cStat.ToString())[0].InnerText;
                                    switch (cStat)
                                    {
                                        //denegada
                                        case "110":
                                        case "301":
                                        case "302":
                                        case "303":
                                        case "304":
                                        case "305":
                                        case "306":
                                            denegada = true;
                                            break;
                                    }
                                    break;
                                }
                            }
                        }
                        break;

                    case "mdfeProc":
                    case "MDFe":
                        tipo = "mdfe";
                        arqProcNFe = nomeArquivoRecebido;
                        break;

                    case "procCancNFe": //cancelamento antigo
                        {
                            temCancelamento = true;
                            tipo = "nfe";
                            var cl = (XmlElement)doc.GetElementsByTagName(NFe.Components.TpcnResources.chNFe.ToString())[0];
                            if (cl != null)
                            {
                                tempFile = cl.InnerText;
                                arqProcNFe = cl.InnerText + Propriedade.ExtRetorno.ProcNFe;
                            }
                        }
                        break;

                    case "CFe":
                    case "CFeCanc":
                        tipo = "cfe";
                        arqProcNFe = nomeArquivoRecebido;
                        break;

                    case "procEventoNFe":
                    case "procEventoCTe":
                    case "procEventoMDFe":
                        {
                            var cl = (XmlElement)doc.GetElementsByTagName(NFe.Components.TpcnResources.tpEvento.ToString())[0];
                            if (cl != null)
                            {
                                switch ((NFe.ConvertTxt.tpEventos)Convert.ToInt32(cl.InnerText))
                                {
                                    case ConvertTxt.tpEventos.tpEvCCe:
                                        switch (doc.DocumentElement.Name)
                                        {
                                            case "procEventoCTe":
                                                tipo = "ccte";
                                                cl = (XmlElement)doc.GetElementsByTagName(TpcnResources.chCTe.ToString())[0];
                                                break;

                                            case "procEventoMDFe":

                                                ///
                                                /// nao existe CCe de MDFe, mas fica aqui por enquanto
                                                tipo = "ccemdfe";
                                                cl = null;
                                                break;

                                            default:
                                                tipo = "cce";
                                                cl = (XmlElement)doc.GetElementsByTagName(NFe.Components.TpcnResources.chNFe.ToString())[0];
                                                break;
                                        }
                                        break;

                                    case ConvertTxt.tpEventos.tpEvCancelamentoNFe:
                                    case ConvertTxt.tpEventos.tpEvCancelamentoSubstituicaoNFCe:
                                        temCancelamento = true;
                                        switch (doc.DocumentElement.Name)
                                        {
                                            case "procEventoCTe":
                                                tipo = "cte";
                                                cl = (XmlElement)doc.GetElementsByTagName(TpcnResources.chCTe.ToString())[0];
                                                break;

                                            case "procEventoMDFe":
                                                tipo = "canmdfe";
                                                cl = (XmlElement)doc.GetElementsByTagName(TpcnResources.chMDFe.ToString())[0];
                                                break;

                                            default:
                                                tipo = "nfe";
                                                cancelamentoNfe = true;
                                                cl = (XmlElement)doc.GetElementsByTagName(TpcnResources.chNFe.ToString())[0];
                                                break;
                                        }
                                        break;

                                    case ConvertTxt.tpEventos.tpEvEPEC:
                                        cl = null;
                                        isEPEC = true;
                                        epecTipo = doc.DocumentElement.Name;
                                        break;

                                    default:

                                        ///
                                        /// tipo de evento desconhecido
                                        ///
                                        throw new Exception("Arquivo de evento " + nomeArquivoRecebido + " desconhecido para impressão do DANFE/DACTE/CCe/DAMDFe");
                                }

                                if (cl != null)
                                {
                                    ///
                                    /// le o nome do arquivo de distribuicao da NFe/CTe
                                    ///
                                    switch (tipo)
                                    {
                                        case "nfe":
                                        case "cce":
                                            arqProcNFe = cl.InnerText + Propriedade.ExtRetorno.ProcNFe;
                                            break;

                                        case "cte":
                                        case "cteos":
                                        case "ccte":
                                            arqProcNFe = cl.InnerText + Propriedade.ExtRetorno.ProcCTe;
                                            break;

                                        case "canmdfe":
                                            arqProcNFe = cl.InnerText + Propriedade.ExtRetorno.ProcMDFe;
                                            break;
                                    }
                                }
                            }
                        }
                        break;

                    default:
                        if (!nomeArquivoRecebido.EndsWith(Propriedade.ExtRetorno.Den))
                        {
                            ///
                            /// tipo de arquivo desconhecido
                            ///
                            throw new Exception("Arquivo " + nomeArquivoRecebido + " desconhecido para impressão do DANFE/DACTE/CCe/DAMDFe");
                        }
                        break;
                }

                if (isEPEC)
                {
                    switch (epecTipo)
                    {
                        case "procEventoCTe":
                            fExtensao = Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML;
                            break;

                        case "procEventoMDFe":
                            fExtensao = Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML;
                            break;

                        default:    //pode ser NFe
                            fExtensao = Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML;
                            break;
                    }
                    var xTemp = Path.GetFileName(Functions.ExtrairNomeArq(nomeArquivoRecebido, Propriedade.ExtRetorno.ProcEventoNFe)) + fExtensao;

                    xTemp = xTemp.Replace("_" + ((int)ConvertTxt.tpEventos.tpEvEPEC).ToString() + "_01", "");

                    ///
                    /// pesquisa pelo arquivo da NFe/NFCe/MDFe/CTe
                    ///
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(nomeArquivoRecebido), xTemp)))
                    {
                        arqProcNFe = Path.Combine(Path.GetDirectoryName(nomeArquivoRecebido), xTemp);
                    }
                    else
                    {
                        var fTemp = Directory.GetFiles(emp.PastaXmlEnvio, xTemp, SearchOption.AllDirectories);
                        if (fTemp.Length == 0)
                        {
                            fTemp = Directory.GetFiles(emp.PastaXmlEnviado, xTemp, SearchOption.AllDirectories);
                            if (fTemp.Length == 0)
                            {
                                if (emp.tpEmis != (int)TipoEmissao.Normal)
                                {
                                    fTemp = Directory.GetFiles(emp.PastaContingencia,
                                                               Path.GetFileName(Functions.ExtrairNomeArq(nomeArquivoRecebido, Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioXML) + fExtensao),
                                                               SearchOption.AllDirectories);
                                    if (fTemp.Length == 0)
                                    {
                                        fTemp = Directory.GetFiles(emp.PastaValidado, xTemp, SearchOption.TopDirectoryOnly);
                                        if (fTemp.Length == 0)
                                        {
                                            fTemp = Directory.GetFiles(emp.PastaContingencia, xTemp, SearchOption.TopDirectoryOnly);
                                        }
                                    }
                                }
                                if (fTemp.Length == 0)
                                {
                                    ///
                                    /// OPS!!! EPEC <-> denegado?
                                    ///
                                    xTemp = Functions.ExtrairNomeArq(xTemp, fExtensao) + Propriedade.ExtRetorno.Den;
                                    if (File.Exists(Path.Combine(Path.GetDirectoryName(nomeArquivoRecebido), xTemp)))
                                    {
                                        arqProcNFe = Path.Combine(Path.GetDirectoryName(nomeArquivoRecebido), xTemp);
                                    }
                                    else
                                    {
                                        fTemp = Directory.GetFiles(emp.PastaXmlEnviado + "\\" + PastaEnviados.Denegados.ToString(), xTemp, SearchOption.AllDirectories);
                                    }
                                }
                            }
                        }

                        if (fTemp.Length > 0)
                        {
                            arqProcNFe = fTemp[0];
                        }
                    }
                    if (string.IsNullOrEmpty(arqProcNFe) || !File.Exists(arqProcNFe))
                    {
                        throw new Exception(string.Format(erroMsg, xTemp, ""));
                    }
                }

                if (nomeArquivoRecebido.EndsWith(Propriedade.ExtRetorno.Den))
                {
                    arqProcNFe = nomeArquivoRecebido;
                }

                if (!string.IsNullOrEmpty(arqProcNFe))
                {
                    arqProcNFe = LocalizarArquivoDistribuicao(nomeArquivoRecebido, arqProcNFe, dataEmissaoNFe, emp);

                    if (!File.Exists(arqProcNFe))
                    {
                        throw new Exception(string.Format(erroMsg, Path.GetFileName(arqProcNFe), ": (" + tipo + ")"));
                    }

                    if (tipo.Equals("nfe") || tipo.Equals("nfce") || tipo.Equals("cce") || tipo == "")
                    {
                        ///
                        /// le o xml da NFe/NFCe
                        ///
                        var nfer = new NFe.ConvertTxt.nfeRead();
                        nfer.ReadFromXml(arqProcNFe);
                        fEmail = nfer.nfe.dest.email;

                        if (tipo == "" || cancelamentoNfe)
                        {
                            if (nfer.nfe.ide.tpImp == ConvertTxt.TpcnTipoImpressao.tiDANFESimplificado)
                            {
                                //DANFE simplificado
                                tipo = "ds";
                            }
                            else
                            {
                                tipo = (nfer.nfe.ide.mod == ConvertTxt.TpcnMod.modNFCe ? "nfce" : "nfe");
                            }
                        }
                        switch (nfer.nfe.protNFe.cStat)
                        {
                            case 110:
                            case 205:
                            case 301:
                            case 302:
                            case 303:
                                denegada = true;
                                break;

                            default:
                                if (arqProcNFe.Equals(nomeArquivoRecebido))
                                {
                                    tempFile = nfer.nfe.infNFe.ID.Replace("NFe", "").Replace("NFCe", "");
                                }

                                break;
                        }
                    }

                    if (!temCancelamento && !denegada && tempFile != "")
                    {
                        ///
                        /// mandou imprimir pelo -procNFe, -procMDFe ou -procCTe, verifica se tem o xml de cancelamento
                        ///
                        switch (tipo)
                        {
                            case "nfe":
                            case "nfce":
                            case "ds":
                                fExtensao = Propriedade.ExtRetorno.ProcEventoNFe;
                                break;

                            case "cte":
                            case "cteos":
                                fExtensao = Propriedade.ExtRetorno.ProcEventoCTe;
                                break;

                            case "mdfe":
                                fExtensao = Propriedade.ExtRetorno.ProcEventoMDFe;
                                break;

                            default:
                                fExtensao = "";
                                break;
                        }
                        if (!string.IsNullOrEmpty(fExtensao))
                        {
                            var arquivoCancelamento = LocalizarArquivoCancelamento(nomeArquivoRecebido, tempFile, tipo, fExtensao, dataEmissaoNFe, emp);
                            if (!string.IsNullOrEmpty(arquivoCancelamento))
                            {
                                //TODO André/Wandrey: Tem que fazer o tratamento do cancelamento por substituição da nfce, por enquanto, não vai funcionar a impressão quando cancelamento for por substuituição
                                doc.Load(arquivoCancelamento);
                                nomeArquivoRecebido = arquivoCancelamento;
                                temCancelamento = true;
                            }
                        }
                    }
                }

                if (File.Exists(arqProcNFe) || File.Exists(nomeArquivoRecebido))
                {
                    var Args = "";

                    if (tipo.Equals("cte") || tipo.Equals("cteos"))
                    {
                        Args += " EE=1";    //EnviarEmail
                        if (!string.IsNullOrEmpty(emp.EmailDanfe) && !emp.AdicionaEmailDanfe)
                        {
                            Args += " E=\"" + emp.EmailDanfe + "\"";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(fEmail))
                        {
                            if (args != null)
                            {
                                args.TryGetValue("email", out fEmail);
                            }
                        }

                        ///
                        /// se tem um e-mail definido nos parametros da empresa
                        ///

                        if (!string.IsNullOrEmpty(emp.EmailDanfe))
                        {
                            if (!emp.AdicionaEmailDanfe)
                            {
                                fEmail = emp.EmailDanfe;
                            }
                            else
                            {
                                fEmail += ";" + emp.EmailDanfe;
                            }
                        }

                        if (!string.IsNullOrEmpty(fEmail))
                        {
                            fEmail = fEmail.Replace(";", ",").TrimStart(new char[] { ',', ' ' }).TrimEnd(new char[] { ',' });

                            if (!string.IsNullOrEmpty(fEmail))
                            {
                                Args += " EE=1";    //EnviarEmail
                                Args += " E=\"" + fEmail + "\"";
                                Args += " IEX=1";   //IgnorarEmail principal
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(emp.PastaConfigUniDanfe))
                    {
                        Args += " PC=\"" + emp.PastaConfigUniDanfe + "\"";
                    }

                    if (isEPEC)
                    {
                        Args += " P=2"; //numero de cópias
                    }
                    else
                    {
                        if (args != null)
                        {
                            if (args.TryGetValue("copias", out var copias))
                            {
                                if (!copias.Equals("-1") && Convert.ToInt32("0" + copias) > 0)
                                {
                                    Args += " P=" + copias;
                                }
                            }
                        }
                    }

                    var configDanfe = "";
                    if (isEPEC)
                    {
                        ///
                        /// define como arquivo principal o XML da NFe/NFCe/MDFe/CTe
                        ///
                        Args += " A=\"" + arqProcNFe + "\"";

                        ///
                        /// define como arquivo adicional o enviado a esta chamada
                        ///
                        Args += " AD=\"" + nomeArquivoRecebido + "\"";
                        if (epecTipo.Equals("procEventoMDFe"))
                        {
                            Args += " T=damdfe";
                        }
                        else
                            if (epecTipo.Equals("procEventoCTe"))
                            {
                                Args += " T=dacte";
                            }
                            else
                            {
                                Args += " T=danfe";
                            }

                        configDanfe = emp.ConfiguracaoDanfe;
                    }
                    else
                    {
                        switch (tipo)
                        {
                            case "nfe":
                            case "nfce":
                                Args += " A=\"" + arqProcNFe + "\"";
                                Args += " T=danfe";
                                configDanfe = emp.ConfiguracaoDanfe;
                                break;

                            case "nfcom":
                                Args += " A=\"" + arqProcNFe + "\"";
                                configDanfe = emp.ConfiguracaoDanfe;
                                break;

                            case "ds":
                                Args += " A=\"" + arqProcNFe + "\"";
                                Args += " T=ds";
                                configDanfe = emp.ConfiguracaoDanfe;
                                break;

                            case "mdfe":
                                Args += " A=\"" + arqProcNFe + "\"";
                                Args += " T=damdfe";
                                configDanfe = emp.ConfiguracaoDanfe;
                                break;

                            case "cteos":
                                Args += " A=\"" + arqProcNFe + "\"";
                                Args += " T=dacteos";
                                configDanfe = emp.ConfiguracaoDanfe;
                                break;

                            case "cte":
                                Args += " A=\"" + arqProcNFe + "\"";
                                Args += " T=dacte";
                                configDanfe = emp.ConfiguracaoDanfe;
                                break;

                            default:
                                if (File.Exists(arqProcNFe))
                                {
                                    switch (tipo)
                                    {
                                        case "cce":
                                        case "ccte":
                                            Args += " A=\"" + nomeArquivoRecebido + "\"";
                                            Args += " N=\"" + arqProcNFe + "\"";
                                            configDanfe = emp.ConfiguracaoCCe;
                                            break;

                                        case "canmdfe":
                                            Args += " A=\"" + nomeArquivoRecebido + "\"";
                                            Args += " N=\"" + arqProcNFe + "\"";
                                            tipo = "";
                                            break;

                                        default:
                                            Args += " A=\"" + arqProcNFe + "\"";
                                            break;
                                    }
                                }
                                else
                                {
                                    Args += " A=\"" + nomeArquivoRecebido + "\"";
                                }
                                if (!string.IsNullOrEmpty(tipo))
                                {
                                    Args += " T=" + tipo;
                                }

                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(configDanfe))
                    {
                        Args += " C=\"" + configDanfe + "\"";
                    }

                    if (temCancelamento)
                    {
                        Args += " CC=1"; //Cancelamento
                    }

                    var temps = "";

                    if (args != null)
                    {
                        if (args.TryGetValue("impressora", out temps))
                        {
                            if (!string.IsNullOrEmpty(temps))
                            {
                                Args += " I=\"" + temps + "\"";
                            }
                        }

                        if (args.TryGetValue("anexos", out temps))
                        {
                            if (!string.IsNullOrEmpty(temps))
                            {
                                var an = 1;
                                foreach (var af in temps.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    Args += " anexo" + an.ToString() + "=\"" + af.Replace("\"", "") + "\"";
                                    ++an;
                                    if (an > 6)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        if (args.TryGetValue("opcoes", out temps))
                        {
                            if (!string.IsNullOrEmpty(temps))
                            {
                                Args += " " + temps + " ";   //opcoes do UniDANFE
                            }
                        }

                        if (args.TryGetValue("np", out temps))
                        {
                            if (!string.IsNullOrEmpty(temps))
                            {
                                Args += " NP=\"" + temps + "\"";   //NomePDF
                                Args += " M=0"; //NAO Imprimir
                                Args += " V=0"; //NAO Visualizar
                            }
                        }

                        if (args.TryGetValue("pp", out temps))
                        {
                            if (!string.IsNullOrEmpty(temps))
                            {
                                Args += " PP=\"" + temps + "\"";   //PastaPDF
                            }
                        }

                        if (args.TryGetValue("plq", out temps))
                        {
                            if (!string.IsNullOrEmpty(temps))
                            {
                                Args += " plq=\"" + temps + "\"";   //pasta local ou da rede para onde a imagem do QR
                            }
                        }

                        ///
                        /// define o arquivo de saida de erros
                        ///
                        args.TryGetValue("auxiliar", out fAuxiliar);
                    }

                    temps = Path.GetFileName(nomeArquivoRecebido).Replace(".xml", "");

                    if (string.IsNullOrEmpty(fAuxiliar))
                    {
                        ///
                        /// formata o arquivo auxiliar com base no arquivo enviado para impressao
                        ///
                        /// 999999-procNFe.xml -> aux-99999-procNFe-danfe-erros.txt
                        /// 999999-procCTe.xml -> aux-99999-procCTe-danfe-erros.txt
                        /// 999999-procMDFe.xml -> aux-99999-procMDFe-danfe-erros.txt
                        /// 999999-procEventoNFe.xml -> aux-99999-procEventoNFe-danfe-erros.txt
                        ///
                        fAuxiliar = temps + "-danfe-erros.txt";
                    }

                    //saida erros para arquivo e nome do arquivo de erro
                    Args += " S=A AE=\"" + Path.Combine(emp.PastaXmlRetorno, Path.GetFileName(fAuxiliar)) + "\"";
                    Auxiliar.WriteLog("ExecutaUniDanfe: Iniciou a execução do UniDANFe.", false);
                    System.Diagnostics.Process.Start(Path.Combine(emp.PastaExeUniDanfe, "unidanfe.exe"), Args);
                    Auxiliar.WriteLog("ExecutaUniDanfe: Encerrou a execução do UniDANFe.", false);

                    if (args != null)
                    {
                        var fFileNameRetornoOk = temps + NFe.Components.Propriedade.Extensao(Propriedade.TipoEnvio.EnvImpressaoDanfe).RetornoXML;

                        ///
                        /// formata o arquivo de retorno ao ERP com base no arquivo enviado para impressao
                        /// 999999-procNFe.xml -> 99999-procNFe-ret-danfe.xml
                        /// 999999-procCTe.xml -> 99999-procCTe-ret-danfe.xml
                        /// 999999-procMDFe.xml -> 99999-procMDFe-ret-danfe.xml
                        /// 999999-procEventoNFe.xml -> 99999-procEventoNFe-ret-danfe.xml
                        tipo = "";
                        if (args.TryGetValue("xml", out tipo))
                        {
                            if (tipo == "0")    //é TXT?
                            {
                                fFileNameRetornoOk = NFe.Components.Functions.ExtrairNomeArq(fFileNameRetornoOk, NFe.Components.Propriedade.Extensao(Propriedade.TipoEnvio.EnvImpressaoDanfe).RetornoXML) +
                                                     NFe.Components.Propriedade.Extensao(Propriedade.TipoEnvio.EnvImpressaoDanfe).RetornoTXT;
                            }
                        }

                        if (fFileNameRetornoOk.EndsWith(NFe.Components.Propriedade.Extensao(Propriedade.TipoEnvio.EnvImpressaoDanfe).RetornoXML))
                        {
                            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                                                    new XElement("DANFE",
                                                        new XElement(NFe.Components.TpcnResources.cStat.ToString(), "1"),
                                                        new XElement("Argumentos", Args)));
                            xml.Save(Path.Combine(emp.PastaXmlRetorno, fFileNameRetornoOk));
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(emp.PastaXmlRetorno, fFileNameRetornoOk), "cStat|1\n\rArgumentos|" + Args + "\n\r");
                        }
                    }
                }
            }
        }
    }
}
