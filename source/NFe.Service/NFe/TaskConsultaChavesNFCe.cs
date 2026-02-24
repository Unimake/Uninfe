using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class TaskConsultaChavesNFCe : TaskAbst
    {
        #region Construtor
        public TaskConsultaChavesNFCe(string arquivo)
        {
            Servico = Servicos.NFCeConsultaChaves;
            NomeArquivoXML = arquivo;
        }
        #endregion

        #region Execute
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            // Definir o serviço que será executado
            Servico = Servicos.NFCeConsultaChaves;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaChavesNFCe).EnvioXML) + Propriedade.ExtRetorno.ConsultaChavesNFCe_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                ExecuteDLL(emp);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaChavesNFCe).EnvioXML, Propriedade.ExtRetorno.ConsultaChavesNFCe_ERR, ex);
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
        /// Executa o serviço de Listagem de Chaves de NFCe utilizando a DLL Unimake.DFe.
        /// </summary>
        /// <param name="emp">ID da Empresa</param>
        private void ExecuteDLL(int emp)
        {
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(NomeArquivoXML);

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaChavesNFCe).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaChavesNFCe).RetornoXML;

            //Extrair dados do XML
            var tpAmb = 2; // Padrão homologação

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Propriedade.ExtRetorno.ConsultaChavesNFCe_ERR);

            var configuracao = new Configuracao
            {
                TipoDFe = TipoDFe.NFCe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (TipoAmbiente)tpAmb,
                CodigoUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo,
                Servico = Unimake.Business.DFe.Servicos.Servico.NFCeConsultaChaves,
                SchemaVersao = "1.00"
            };

            var listagemChavesNFCe = new Unimake.Business.DFe.Servicos.NFCe.ConsultaChaves(conteudoXML.OuterXml, configuracao);
            listagemChavesNFCe.Executar();

            vStrXmlRetorno = listagemChavesNFCe.RetornoWSString;

            //Salvar o XML retornado
            XmlRetorno(finalArqEnvio, finalArqRetorno);
        }
        #endregion ExecuteDLL
    }
}