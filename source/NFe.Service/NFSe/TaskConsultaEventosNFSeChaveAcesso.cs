using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.NFSe
{
    internal class TaskConsultaEventosNFSeChaveAcesso : TaskAbst
    {
        #region Objeto com os dados do XML de consulta de eventos por chave de acesso
        /// <summary>
        /// Dados da consulta de eventos da NFSe por chave de acesso
        /// </summary>
        private DadosConsEventosNFSeChaveAcesso oDadosConsEventosNFSeChaveAcesso;
        #endregion

        #region Execute
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            //Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeConsultarEventosNFSeChaveAcesso;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedConsEventosNFSeChaveAcesso).EnvioXML) + Propriedade.ExtRetorno.ConsEventosNFSeChaveAcesso_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosConsEventosNFSeChaveAcesso = new DadosConsEventosNFSeChaveAcesso(emp);

                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosConsEventosNFSeChaveAcesso.cMunicipio);

                ExecuteDLL(emp, oDadosConsEventosNFSeChaveAcesso.cMunicipio, padraoNFSe);
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

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedConsEventosNFSeChaveAcesso).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedConsEventosNFSeChaveAcesso).RetornoXML;
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                TipoDFe = Unimake.Business.DFe.Servicos.TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarEventosNFSeChaveAcesso,
                SchemaVersao = versaoXML,
            };

            var consultarEventos = new Unimake.Business.DFe.Servicos.NFSe.ConsultaEventosNFSeChaveAcesso(conteudoXML.OuterXml, configuracao);
            consultarEventos.Executar();

            vStrXmlRetorno = consultarEventos.RetornoWSString;

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            if (consultarEventos.NFSesRecebidas != null && consultarEventos.NFSesRecebidas.Count >= 1)
            {
                var pastaXmlNfse = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, "NFSe Eventos NACIONAL");
                Directory.CreateDirectory(pastaXmlNfse);

                consultarEventos.GravarXMLNFSe(pastaXmlNfse);
            }

            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Propriedade.Extensao(Propriedade.TipoEnvio.PedConsEventosNFSeChaveAcesso).RetornoXML);
            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }

            consultarEventos.Dispose();
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
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Consulta de Eventos de NFS-e por Chave de Acesso");
            }

            return versaoXML;
        }

        #endregion DefinirVersaoXML
    }
}
