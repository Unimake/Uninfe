using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class TaskDownloadNFCe : TaskAbst
    {
        #region Construtor
        public TaskDownloadNFCe(string arquivo)
        {
            Servico = Servicos.NFCeDownloadXML;
            NomeArquivoXML = arquivo;
        }
        #endregion

        #region Execute
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            //Definir o serviço que será executado para a classe
            Servico = Servicos.NFCeDownloadXML;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.DownloadNFCe).EnvioXML) + Propriedade.ExtRetorno.DownloadNFCe_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                ExecuteDLL(emp);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.DownloadNFCe).EnvioXML, Propriedade.ExtRetorno.DownloadNFCe_ERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno.ERR(de erro) para o ERP
                }
            }
            finally
            {
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML
                }
            }
        }
        #endregion

        #region ExecuteDLL

        /// <summary>
        /// Executa o serviço de download da NFCe utilizando a DLL.
        /// </summary>
        /// <param name="emp">Empresa que está solicitando o download</param>
        private void ExecuteDLL(int emp)
        {
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(NomeArquivoXML);

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.DownloadNFCe).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.DownloadNFCe).RetornoXML;

            //Extrair dados do XML
            var tpAmb = 2; // Padrão homologação

            if (conteudoXML.GetElementsByTagName("tpAmb").Count > 0)
            {
                tpAmb = Convert.ToInt32(conteudoXML.GetElementsByTagName("tpAmb")[0].InnerText);
            }

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Propriedade.ExtRetorno.DownloadNFCe_ERR);

            var configuracao = new Configuracao
            {
                TipoDFe = TipoDFe.NFCe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (TipoAmbiente)tpAmb,
                CodigoUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo,
                Servico = Unimake.Business.DFe.Servicos.Servico.NFCeDownloadXML
            };

            var downloadNFCe = new Unimake.Business.DFe.Servicos.NFCe.DownloadXML(conteudoXML.OuterXml, configuracao);
            downloadNFCe.Executar();

            vStrXmlRetorno = downloadNFCe.RetornoWSString;

            //Salvar o XML retornado
            XmlRetorno(finalArqEnvio, finalArqRetorno);

            //Gravar NFCe e eventos do retorno se autorizada
            if (downloadNFCe.Result != null && downloadNFCe.Result.CStat == "200")
            {
                try
                {
                    downloadNFCe.GravarXMLProc(Empresas.Configuracoes[emp].PastaXmlRetorno);
                }
                catch (Exception ex)
                {
                    //Logar erro mas não interromper o fluxo
                    Auxiliar.WriteLog($"Erro ao gravar XMLs do download NFCe: {ex.Message}", false);
                }
            }

            downloadNFCe.Dispose();
        }

        #endregion ExecuteDLL
    }
}
