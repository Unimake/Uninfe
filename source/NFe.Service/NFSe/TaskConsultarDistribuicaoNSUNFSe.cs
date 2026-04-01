using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.NFSe
{
    internal class TaskConsultarDistribuicaoNSUNFSe : TaskAbst
    {
        #region Objeto com os dados do XML de consulta NSU
        /// <summary>
        /// Dados da consulta de NSU da NFSe
        /// </summary>
        private DadosConsNsuNfse oDadosConsNsuNfse;
        #endregion

        #region Execute
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            //Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeConsultarDistribuicaoNFSeNSU;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedConsNsuNfse).EnvioXML) + Propriedade.ExtRetorno.ConsNsuNfse_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosConsNsuNfse = new DadosConsNsuNfse(emp);

                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosConsNsuNfse.cMunicipio);

                ExecuteDLL(emp, oDadosConsNsuNfse.cMunicipio, padraoNFSe);
            }
            catch
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
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
        /// Executa o serviço utilizando a DLL do UniNFe.
        /// </summary>
        /// <param name="emp">Empresa que está enviando o XML</param>
        /// <param name="municipio">Código do município para onde será enviado o XML</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        private void ExecuteDLL(int emp, int municipio, PadraoNFSe padraoNFSe)
        {
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(NomeArquivoXML);

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedConsNsuNfse).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedConsNsuNfse).RetornoXML;
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                TipoDFe = Unimake.Business.DFe.Servicos.TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarDistribuicaoNFSeNSU,
                SchemaVersao = versaoXML,
            };

            var consultarNsu = new Unimake.Business.DFe.Servicos.NFSe.ConsultarDistribuicaoNFSeNSU(conteudoXML.OuterXml, configuracao);
            consultarNsu.Executar();

            vStrXmlRetorno = consultarNsu.RetornoWSString;

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            if (consultarNsu.NFSesRecebidas != null && consultarNsu.NFSesRecebidas.Count >= 1)
            {
                var pastaXmlNfse = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, "NFSe NSU NACIONAL");
                Directory.CreateDirectory(pastaXmlNfse);

                consultarNsu.GravarXMLNFSe(pastaXmlNfse);
            }

            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedConsNsuNfse).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedConsNsuNfse).RetornoXML);
            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }

            consultarNsu.Dispose();
        }

        #endregion ExecuteDLL

        #region DefinirVersaoXML

        /// <summary>
        /// Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município
        /// </summary>
        /// <param name="codMunicipio">Código do município para onde será enviado o XML</param>
        /// <param name="xmlDoc">Conteúdo do XML da NFSe</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        /// <returns>Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município</returns>
        private string DefinirVersaoXML(int codMunicipio, XmlDocument xmlDoc, PadraoNFSe padraoNFSe)
        {
            var versaoXML = "0.00";

            switch (padraoNFSe)
            {
                case PadraoNFSe.NACIONAL:
                    versaoXML = "1.01";
                    break;

                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Consulta de NSU de NFS-e");
            }

            return versaoXML;
        }

        #endregion DefinirVersaoXML
    }
}