using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;

using NFe.Components;
using NFe.Settings;

namespace NFe.Service
{
    /// <summary>
    /// Converter o arquivo de NFe do formato TXT para XML
    /// </summary>
    /// <param name="arquivo">Nome completo do arquivo a ser convertido (Pasta e arquivo)</param>
    /// <remarks>
    /// Autor: Wandrey Mundin Ferreira
    /// </remarks>

    public class ConverterTXT
    {
        public ConverterTXT(string arquivo)
        {
            Auxiliar oAux = new Auxiliar();

            var arquivosXmlGerados = new List<string>();

            string pasta = new FileInfo(arquivo).DirectoryName;
            pasta = pasta.Substring(0, pasta.Length - 5); //Retirar a pasta \Temp do final - Wandrey 03/08/2011

            string ccMessage = string.Empty;
            string ccExtension = Propriedade.ExtRetorno.Nfe_ERR;// "-nfe.err";
            var EXT = Propriedade.Extensao(Propriedade.TipoEnvio.NFe);

            try
            {
                int emp = Empresas.FindEmpresaByThread();

                ///
                /// exclui o arquivo de erro
                /// 
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Path.GetFileName(Functions.ExtrairNomeArq(arquivo, EXT.EnvioTXT) + ccExtension));
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Path.GetFileName(Functions.ExtrairNomeArq(arquivo, EXT.EnvioTXT) + EXT.RetornoXML));
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + Path.GetFileName(arquivo));
                ///
                /// exclui o arquivo TXT original
                /// 
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Path.GetFileNameWithoutExtension(arquivo) + "-orig.txt");

                ///
                /// processa a conversão
                /// 
                var resultadoConversao = new Unimake.Business.DFe.Xml.NFe.NFeTxtConverter().Converter(arquivo);

                //Deu tudo certo com a conversão?
                if (resultadoConversao.Sucesso)
                {
                    ///
                    /// danasa 8-2009
                    /// 
                    if (resultadoConversao.Documentos.Count == 0)
                    {
                        ccMessage = "cStat=02\r\n" +
                            "xMotivo=Falha na conversão. Sem informações para converter o arquivo texto";

                        oAux.MoveArqErro(arquivo, ".txt");
                    }
                    else
                    {
                        //
                        // salva o arquivo texto original
                        //
                        if (pasta.ToLower().Equals(Empresas.Configuracoes[emp].PastaXmlEnvio.ToLower()) || pasta.ToLower().Equals(Empresas.Configuracoes[emp].PastaValidar.ToLower()))
                        {
                            FileInfo ArqOrig = new FileInfo(arquivo);

                            string vvNomeArquivoDestino = Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Path.GetFileNameWithoutExtension(arquivo) + "-orig.txt";
                            ArqOrig.MoveTo(vvNomeArquivoDestino);
                        }
                        ccExtension = "-nfe.txt";
                        ccMessage = "cStat=01\r\n" +
                            "xMotivo=Conversão efetuada com sucesso." + (resultadoConversao.Documentos.Count == 1 ? "" : " Foram convertidas " + resultadoConversao.Documentos.Count.ToString() + " notas fiscais");

                        foreach (Unimake.Business.DFe.Xml.NFe.NFeTxtDocumento documentoConvertido in resultadoConversao.Documentos)
                        {
                            ///
                            /// monta o texto que será gravado no arquivo de aviso ao ERP
                            /// 
                            ccMessage += Environment.NewLine +
                                    "Nota fiscal: " + documentoConvertido.Numero.ToString("000000000") +
                                    " Serie: " + documentoConvertido.Serie.ToString("000") +
                                    " - ChaveNFe: " + documentoConvertido.Chave;

                            string nomeArquivoDestino = Path.Combine(pasta, documentoConvertido.Chave + EXT.EnvioXML);
                            var xml = new XmlDocument();
                            xml.LoadXml(documentoConvertido.Xml);
                            xml.Save(nomeArquivoDestino);
                            arquivosXmlGerados.Add(nomeArquivoDestino);

                            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + documentoConvertido.Chave + EXT.EnvioXML);
                            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + documentoConvertido.Chave + EXT.EnvioTXT);
                        }
                    }
                }
                else
                {
                    ///
                    /// danasa 8-2009
                    /// 
                    ccMessage = "cStat=99\r\n" +
                        "xMotivo=Falha na conversão\r\n" +
                        "MensagemErro=" + resultadoConversao.MensagemErro;
                }
            }
            catch (Exception ex)
            {
                ccMessage = ex.Message;
                ccExtension = Propriedade.ExtRetorno.Nfe_ERR;//"-nfe.err";
            }

            if (!string.IsNullOrEmpty(ccMessage))
            {
                oAux.MoveArqErro(arquivo, ".txt");

                if (ccMessage.StartsWith("cStat=02") || ccMessage.StartsWith("cStat=99"))
                {
                    ///
                    /// exclui todos os XMLs persistidos pela nova conversão se houve erro
                    ///
                    foreach (string arquivoXmlGerado in arquivosXmlGerados)
                    {
                        Functions.DeletarArquivo(arquivoXmlGerado);
                    }
                }
                ///
                /// danasa 8-2009
                /// 
                /// Gravar o retorno para o ERP em formato TXT com o erro ocorrido
                /// 
                oAux.GravarArqErroERP(Functions.ExtrairNomeArq(arquivo, EXT.EnvioTXT) + ccExtension, ccMessage);
            }
        }
    }
}
