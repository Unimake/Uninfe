using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.NFe;
using Unimake.Business.DFe.Xml.NFe;

namespace NFe.Service
{
    public class TaskDFeRecepcao : TaskAbst
    {
        protected string ExtEnvioDFe { get; set; }
        protected string ExtEnvioDFeTXT { get; set; }
        protected string ExtRetornoDFe { get; set; }
        protected string ExtRetEnvDFe_ERR { get; set; }

        public TaskDFeRecepcao(string arquivo)
        {
            Servico = Servicos.DFeEnviar;
            NomeArquivoXML = arquivo;
            if (vXmlNfeDadosMsgEhXML)
            {
                ConteudoXML.PreserveWhitespace = false;
                ConteudoXML.Load(arquivo);
            }
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            ExtEnvioDFe = Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).EnvioXML;
            ExtEnvioDFeTXT = Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).EnvioTXT;
            ExtRetornoDFe = Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).RetornoXML;
            ExtRetEnvDFe_ERR = Propriedade.ExtRetorno.retEnvDFe_ERR;

            try
            {
                if (!vXmlNfeDadosMsgEhXML)
                {
                    // -------------------------------------------
                    // Estrutura do TXT
                    // -------------------------------------------
                    // versao|1.00
                    // tpAmb|1
                    // cUFAutor|35
                    // CNPJ|
                    //  ou
                    // CPF|
                    // ultNSU|123456789012345
                    //  ou
                    // NSU|123456789012345
                    //  ou
                    // chNFe|41170706117473000150550010000463191912756548
                    // -------------------------------------------
                    var cLinhas = Functions.LerArquivo(NomeArquivoXML);

                    var xml = new DistDFeInt();

                    foreach (var item in cLinhas)
                    {
                        var conteudo = item.Split("|".ToCharArray());
                        var nomeTag = conteudo[0].ToLower();
                        var conteudoTag = conteudo[1];

                        switch (nomeTag)
                        {
                            case "versao":
                                xml.Versao = conteudoTag;
                                break;

                            case "tpamb":
                                xml.TpAmb = (Unimake.Business.DFe.Servicos.TipoAmbiente)Convert.ToInt32(conteudoTag);
                                break;

                            case "cufautor":
                                xml.CUFAutor = (UFBrasil)Convert.ToInt32(conteudoTag);
                                break;

                            case "cnpj":
                                xml.CNPJ = conteudoTag;
                                break;

                            case "cpf":
                                xml.CPF = conteudoTag;
                                break;

                            case "ultnsu":
                                xml.DistNSU = new DistNSU
                                {
                                    UltNSU = conteudoTag
                                };
                                break;

                            case "nsu":
                                xml.ConsNSU = new ConsNSU
                                {
                                    NSU = conteudoTag
                                };
                                break;

                            case "chnfe":
                                xml.ConsChNFe = new ConsChNFe
                                {
                                    ChNFe = conteudoTag
                                };
                                break;
                        }
                    };

                    var fileXML = Path.GetFileNameWithoutExtension(NomeArquivoXML) + ".xml";

                    if (NomeArquivoXML.IndexOf(Empresas.Configuracoes[emp].PastaValidar, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        fileXML = Path.Combine(Empresas.Configuracoes[emp].PastaValidar, fileXML);
                    }

                    oGerarXML.RecepcaoDFe(fileXML, xml.GerarXML().OuterXml);
                }
                else
                {
                    var xml = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<DistDFeInt>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                        TipoDFe = TipoDFe.NFe,
                        TipoAmbiente = xml.TpAmb,
                        CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                    };

                    if (ConfiguracaoApp.Proxy)
                    {
                        configuracao.HasProxy = true;
                        configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                        configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                        configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                    }

                    var distribuicaoDFe = new DistribuicaoDFe(xml, configuracao);
                    distribuicaoDFe.Executar();

                    vStrXmlRetorno = distribuicaoDFe.RetornoWSString;

                    LeRetornoDFe(emp, distribuicaoDFe.RetornoWSXML);

                    XmlRetorno(ExtEnvioDFe, ExtRetornoDFe);

                    distribuicaoDFe.Dispose();
                }
            }
            catch (Exception ex)
            {
                WriteLogError(ex);
            }
            finally
            {
                try
                {
                    //Deletar o arquivo de solicitação do serviço
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                }
            }
        }

        protected void LeRetornoDFe(int emp, XmlDocument retornoXML)
        {
            try
            {
                ///
                /// pega o nome base dos arquivos a serem gravados
                ///
                var fileRetorno2 = Functions.ExtrairNomeArq(NomeArquivoXML, ExtEnvioDFe);
                ///
                /// pega o nome do arquivo de retorno
                ///
                var fileRetorno = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  fileRetorno2 + ExtRetornoDFe);

                ///
                /// cria a pasta para comportar as notas e eventos retornados já descompactados
                ///
                var folderTerceiros = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, "dfe");
                if (!Directory.Exists(folderTerceiros))
                {
                    Directory.CreateDirectory(folderTerceiros);
                }

                ///
                /// exclui todos os arquivos que foram envolvidos no retorno
                ///
                foreach (var item in Directory.GetFiles(folderTerceiros, fileRetorno2 + "-*.xml", SearchOption.TopDirectoryOnly))
                {
                    if (!Functions.FileInUse(item))
                    {
                        File.Delete(item);
                    }
                }

                var envEventoList = retornoXML.GetElementsByTagName("retDistDFeInt");
                foreach (XmlNode ret1Node in envEventoList)
                {
                    var ret1Elemento = (XmlElement)ret1Node;

                    var ret1List = ret1Elemento.GetElementsByTagName("loteDistDFeInt");
                    foreach (XmlNode ret in ret1List)
                    {
                        ExtraiDFe(ret, "docZip", folderTerceiros, fileRetorno2, emp, fileRetorno);
                    }
                }
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("LeRetornoNFe: " + ex.Message, false);
                ///
                /// Wandrey.
                /// Foi tudo processado mas houve algum erro na descompactacao dos retornos
                /// Se gravar o arquivo com extensao .err, o ERP pode ignorar o XML de retorno, que está correto
                ///
                //WriteLogError(ex);
            }
        }

        private void ExtraiDFe(XmlNode ret, string tagNameDoc, string folderTerceiros, string fileRetorno2, int emp, string fileRetorno)
        {
            for (var n = 0; n < ret.ChildNodes.Count; ++n)
            {
                if (ret.ChildNodes[n].Name.Equals(tagNameDoc))
                {
                    var FileToFtp = "";
                    var NSU = "";
                    if (ret.ChildNodes[n].Attributes[TpcnResources.NSU.ToString()] == null)
                    {
                        NSU = "000000000000000";
                    }
                    else
                    {
                        NSU = ret.ChildNodes[n].Attributes[TpcnResources.NSU.ToString()].Value;
                    }

                    ///
                    /// descompacta o conteúdo
                    ///
                    var xmlRes = TFunctions.Decompress(ret.ChildNodes[n].InnerText);

                    var docXML = new XmlDocument();
                    docXML.Load(Functions.StringXmlToStreamUTF8(xmlRes));

                    if (string.IsNullOrEmpty(xmlRes))
                    {
                        Auxiliar.WriteLog("LeRetornoNFe: Não foi possível descompactar o conteúdo da NSU: " + NSU, false);
                    }
                    else
                    {
                        #region NFe

                        if (ret.ChildNodes[n].Attributes["schema"].InnerText.StartsWith("resEvento"))
                        {
                            FileToFtp = Path.Combine(folderTerceiros, fileRetorno2 + "-" + NSU + Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).RetornoXML);
                        }
                        else if (ret.ChildNodes[n].Attributes["schema"].InnerText.StartsWith("procEventoNFe"))
                        {
                            var chNFe = Functions.LerTag(((XmlElement)((XmlElement)docXML.GetElementsByTagName("evento")[0]).GetElementsByTagName("infEvento")[0]), "chNFe", false);
                            var tpEvento = Functions.LerTag(((XmlElement)((XmlElement)docXML.GetElementsByTagName("evento")[0]).GetElementsByTagName("infEvento")[0]), "tpEvento", false);
                            var nSeqEvento = Functions.LerTag(((XmlElement)((XmlElement)docXML.GetElementsByTagName("evento")[0]).GetElementsByTagName("infEvento")[0]), "nSeqEvento", false);

                            if (Empresas.Configuracoes[emp].ArqNSU && Convert.ToInt64((string.IsNullOrWhiteSpace(NSU) ? "0" : NSU)) > 0)
                            {
                                FileToFtp = Path.Combine(folderTerceiros, fileRetorno2 + "-" + NSU + Propriedade.ExtRetorno.ProcEventoNFe);
                            }
                            else
                            {
                                FileToFtp = Path.Combine(folderTerceiros, chNFe + "_" + tpEvento + "_" + nSeqEvento.PadLeft(2, '0') + Propriedade.ExtRetorno.ProcEventoNFe);
                            }
                        }
                        else if (ret.ChildNodes[n].Attributes["schema"].InnerText.StartsWith("procNFe"))
                        {
                            var chave = ((XmlElement)docXML.GetElementsByTagName("infNFe")[0]).GetAttribute("Id").Substring(3, 44);

                            if (Empresas.Configuracoes[emp].ArqNSU && Convert.ToInt64((string.IsNullOrWhiteSpace(NSU) ? "0" : NSU)) > 0)
                            {
                                FileToFtp = Path.Combine(folderTerceiros, fileRetorno2 + "-" + NSU + Propriedade.ExtRetorno.ProcNFe);
                            }
                            else
                            {
                                FileToFtp = Path.Combine(folderTerceiros, chave + Propriedade.ExtRetorno.ProcNFe);
                            }
                        }
                        else if (ret.ChildNodes[n].Attributes["schema"].InnerText.StartsWith("resNFe"))
                        {
                            FileToFtp = Path.Combine(folderTerceiros, fileRetorno2 + "-" + NSU + Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML);
                        }

                        #endregion NFe

                        #region CTe

                        else if (ret.ChildNodes[n].Attributes["schema"].InnerText.StartsWith("procEventoCTe"))
                        {
                            var chCTe = Functions.LerTag(((XmlElement)((XmlElement)docXML.GetElementsByTagName("eventoCTe")[0]).GetElementsByTagName("infEvento")[0]), "chCTe", false);
                            var tpEvento = Functions.LerTag(((XmlElement)((XmlElement)docXML.GetElementsByTagName("eventoCTe")[0]).GetElementsByTagName("infEvento")[0]), "tpEvento", false);
                            var nSeqEvento = Functions.LerTag(((XmlElement)((XmlElement)docXML.GetElementsByTagName("eventoCTe")[0]).GetElementsByTagName("infEvento")[0]), "nSeqEvento", false);

                            var versaoSchemaCTe = "3.00";
                            var listEventoCTe = docXML.GetElementsByTagName("eventoCTe");
                            if (listEventoCTe.Count > 0)
                            {
                                var elementEventoCTe = (XmlElement)listEventoCTe[0];
                                versaoSchemaCTe = elementEventoCTe.GetAttribute("versao");
                            }

                            if (Empresas.Configuracoes[emp].ArqNSU)
                            {
                                FileToFtp = Path.Combine(folderTerceiros, fileRetorno2 + "-" + NSU + Propriedade.ExtRetorno.ProcEventoCTe);
                            }
                            else
                            {
                                FileToFtp = Path.Combine(folderTerceiros, chCTe + "_" + tpEvento + "_" + nSeqEvento.PadLeft((versaoSchemaCTe.Equals("3.00") ? 2 : 3), '0') + Propriedade.ExtRetorno.ProcEventoCTe);
                            }
                        }
                        else if (ret.ChildNodes[n].Attributes["schema"].InnerText.StartsWith("procCTe"))
                        {
                            var chave = ((XmlElement)docXML.GetElementsByTagName("infCte")[0]).GetAttribute("Id").Substring(3, 44);

                            if (Empresas.Configuracoes[emp].ArqNSU)
                            {
                                FileToFtp = Path.Combine(folderTerceiros, fileRetorno2 + "-" + NSU + Propriedade.ExtRetorno.ProcCTe);
                            }
                            else
                            {
                                FileToFtp = Path.Combine(folderTerceiros, chave + Propriedade.ExtRetorno.ProcCTe);
                            }
                        }

                        #endregion CTe

                        else
                        {
                            Auxiliar.WriteLog("LerRetornoDFe: Não foi possível ler o schema", false);
                        }

                        if (FileToFtp != "")
                        {
                            if (!File.Exists(FileToFtp))
                            {
                                File.WriteAllText(FileToFtp, xmlRes);
                            }

                            var vFolder = Empresas.Configuracoes[emp].FTPPastaRetornos;
                            if (!string.IsNullOrEmpty(vFolder))
                            {
                                try
                                {
                                    Empresas.Configuracoes[emp].SendFileToFTP(FileToFtp, vFolder);
                                }
                                catch (Exception ex)
                                {
                                    ///
                                    /// grava um arquivo de erro com extensão "FTP" para diferenciar dos arquivos de erro
                                    oAux.GravarArqErroERP(Path.ChangeExtension(fileRetorno, ".ftp"), ex.Message);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void WriteLogError(Exception ex)
        {
            string extRet;

            if (vXmlNfeDadosMsgEhXML)
            {
                extRet = ExtEnvioDFe;
            }
            else
            {
                extRet = ExtEnvioDFeTXT;
            }

            try
            {
                //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                TFunctions.GravarArqErroServico(NomeArquivoXML, extRet, ExtRetEnvDFe_ERR, ex);
            }
            catch
            {
                //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                //Wandrey 09/03/2010
            }
        }
    }
}