using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace NFe.Service
{
    /// <summary>
    /// Classe responsável por montar lote de várias NFes
    /// </summary>
    public class TaskNFeMontarLoteVarias : TaskAbst
    {
        public TaskNFeMontarLoteVarias()
        {
            Servico = Servicos.NFeMontarLoteVarias;
            oGerarXML.Servico = Servico;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var arquivosNFe = new List<string>();

            //Aguardar a assinatura de todos os arquivos da pasta de lotes
            arquivosNFe = oAux.ArquivosPasta(Empresas.Configuracoes[emp].PastaXmlEmLote, "*" + Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML);

            if (NomeArquivoXML.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioTXT) >= 0)
            {
                try
                {
                    var xml = new StringBuilder();
                    xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    xml.Append("<MontarLoteNFe>");
                    foreach (var filename in File.ReadAllLines(NomeArquivoXML, Encoding.Default))
                    {
                        xml.AppendFormat("<ArquivoNFe>{0}</ArquivoNFe>", filename + (filename.ToLower().EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML) ? "" : Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML));
                    }
                    xml.Append("</MontarLoteNFe>");
                    File.WriteAllText(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEmLote, Path.GetFileName(NomeArquivoXML.Replace(Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioTXT, Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioXML))), xml.ToString());

                    //Deletar o arquivo de solicitação de montagem do lote de NFe
                    var oArquivo = new FileInfo(NomeArquivoXML);
                    oArquivo.Delete();
                }
                catch (Exception ex)
                {
                    try
                    {
                        TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioTXT, Propriedade.ExtRetorno.MontarLote_ERR, ex);
                    }
                    catch
                    {
                        //Se deu algum erro na hora de gravar o arquivo de erro de retorno para o ERP, infelizmente não poderemos fazer nada
                        //pois deve estar ocorrendo alguma falha de rede, hd, permissão de acesso a pasta ou arquivos, etc. Wandrey 22/03/2010
                        //TODO: Não poderia gravar algum LOG para análise? Erro de rede normalmente é erro de IO
                    }
                }
            }
            else
            {
                var notas = new List<ArquivoXMLDFe>();
                FileStream fsArquivo = null;
                var fluxoNfe = new FluxoNfe();

                try
                {
                    try
                    {
                        var doc = new XmlDocument(); //Criar instância do XmlDocument Class
                        fsArquivo = new FileStream(NomeArquivoXML, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); //Abrir um arquivo XML usando FileStream
                        doc.Load(fsArquivo); //Carregar o arquivo aberto no XmlDocument

                        var versaoXml = string.Empty;
                        var modeloDFe = string.Empty;

                        //Verificar se ainda existe algum arquivo de NFe para ser processado
                        DateTime startTime;
                        DateTime stopTime;
                        TimeSpan elapsedTime;

                        long elapsedMillieconds;
                        startTime = DateTime.Now;

                        var documentoList = doc.GetElementsByTagName("MontarLoteNFe"); //Pesquisar o elemento Documento no arquivo XML
                        bool todosGerados = true;
                        bool naoTemNoEnvio = false;
                        while (true)
                        {
                            stopTime = DateTime.Now;
                            elapsedTime = stopTime.Subtract(startTime);
                            elapsedMillieconds = (int)elapsedTime.TotalMilliseconds;

                            foreach (XmlNode documentoNode in documentoList)
                            {
                                var documentoElemento = (XmlElement)documentoNode;

                                var QtdeArquivo = documentoElemento.GetElementsByTagName("ArquivoNFe").Count;

                                for (var d = 0; d < QtdeArquivo; d++)
                                {
                                    var arquivo = documentoElemento.GetElementsByTagName("ArquivoNFe")[d].InnerText;
                                    var arquivoNFeEnvio = Empresas.Configuracoes[emp].PastaXmlEmLote + "\\" + arquivo;
                                    var arquivoNFeTemp = Empresas.Configuracoes[emp].PastaXmlEmLote + "\\temp\\" + arquivo;

                                    if (!File.Exists(arquivoNFeTemp))
                                    {
                                        todosGerados = false;
                                        if (!File.Exists(arquivoNFeEnvio))
                                        {
                                            naoTemNoEnvio = true;
                                        }

                                        break;
                                    }
                                }
                            }

                            if (naoTemNoEnvio || todosGerados)
                            {
                                break;
                            }

                            todosGerados = true;

                            if (elapsedMillieconds >= 60000) //60.000 ms que corresponde á 60 segundos que corresponde a 1 minuto
                            {
                                break;
                            }

                            Thread.Sleep(2000); //2 segundos
                        }

                        documentoList = doc.GetElementsByTagName("MontarLoteNFe"); //Pesquisar o elemento Documento no arquivo XML
                        foreach (XmlNode documentoNode in documentoList)
                        {
                            var documentoElemento = (XmlElement)documentoNode;

                            var QtdeArquivo = documentoElemento.GetElementsByTagName("ArquivoNFe").Count;

                            for (var d = 0; d < QtdeArquivo; d++)
                            {
                                var arquivoNFe = Empresas.Configuracoes[emp].PastaXmlEmLote + "\\temp\\" + documentoElemento.GetElementsByTagName("ArquivoNFe")[d].InnerText;

                                if (File.Exists(arquivoNFe))
                                {
                                    var conteudoXMLNFe = new XmlDocument();
                                    conteudoXMLNFe.Load(arquivoNFe);

                                    var oDadosNfe = LerXMLNFe(conteudoXMLNFe);

                                    if (string.IsNullOrEmpty(versaoXml))
                                    {
                                        versaoXml = oDadosNfe.versao;
                                    }
                                    if (string.IsNullOrWhiteSpace(modeloDFe))
                                    {
                                        modeloDFe = oDadosNfe.mod;
                                    }

                                    if (!fluxoNfe.NFeComLote(oDadosNfe.chavenfe))
                                    {
                                        notas.Add(new ArquivoXMLDFe() { NomeArquivoXML = arquivoNFe, ConteudoXML = conteudoXMLNFe });
                                    }
                                    else
                                    {
                                        throw new Exception("Arquivo: " + arquivoNFe + " já está no fluxo de envio e não será incluído em novo lote.");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Arquivo: " + arquivoNFe + " não existe e não será incluído no lote!");
                                }
                            }
                        }

                        fsArquivo.Close();

                        var xmlLote = LoteNfe(notas, versaoXml, modeloDFe);
                        var nfeRecepcao = new TaskNFeRecepcao(xmlLote);
                        nfeRecepcao.Execute();
                    }
                    catch (Exception ex)
                    {
                        if (fsArquivo != null)
                        {
                            fsArquivo.Close();
                        }

                        TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioXML, Propriedade.ExtRetorno.MontarLote_ERR, ex);
                    }

                    //Deletar o arquivo de solicitão de montagem do lote de NFe
                    var oArquivo = new FileInfo(NomeArquivoXML);
                    oArquivo.Delete();
                }
                catch (Exception ex)
                {
                    try
                    {
                        TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioXML, Propriedade.ExtRetorno.MontarLote_ERR, ex);
                    }
                    catch
                    {
                        //Se deu algum erro na hora de gravar o arquivo de erro de retorno para o ERP, infelizmente não poderemos fazer nada
                        //pois deve estar ocorrendo alguma falha de rede, hd, permissão de acesso a pasta ou arquivos, etc. Wandrey 22/03/2010
                        //TODO: Não poderia gravar algum LOG para análise? Erro de rede normalmente é erro de IO
                    }
                }
            }
        }
    }
}