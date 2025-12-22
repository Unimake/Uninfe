using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.NFSe
{
    internal class TaskRecepcionarEventos : TaskAbst
    {
        #region Objeto com os dados do XML do pedido de registro de evento
        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do pedido de registro de evento
        /// </summary>
        private DadosPedRegEvento oDadosPedRegEvento;
        #endregion

        #region Execute
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            //Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeRecepcionarEventosDiversos;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRegEvento).EnvioXML) + Propriedade.ExtRetorno.PedRegEvento_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosPedRegEvento = new DadosPedRegEvento(emp);

                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosPedRegEvento.cMunicipio);

                ExecuteDLL(emp, oDadosPedRegEvento.cMunicipio, padraoNFSe);
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

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedRegEvento).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedRegEvento).RetornoXML;
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                TipoDFe = Unimake.Business.DFe.Servicos.TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = Unimake.Business.DFe.Servicos.Servico.NFSeRecepcionarEventosDiversos,
                SchemaVersao = versaoXML,
                MunicipioToken = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioSenha = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioUsuario = Empresas.Configuracoes[emp].UsuarioWS
            };

            var registrarEvento = new Unimake.Business.DFe.Servicos.NFSe.RecepcionarEventosNfse(conteudoXML, configuracao);
            registrarEvento.Executar();

            vStrXmlRetorno = registrarEvento.RetornoWSString;

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRegEvento).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedRegEvento).RetornoXML);
            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }

            registrarEvento.Dispose();
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
            var versaoXML = "1.01";

            switch (padraoNFSe)
            {
                case PadraoNFSe.NACIONAL:
                    if (xmlDoc.DocumentElement.GetElementsByTagName("versao").Count > 0)
                    {
                        versaoXML = xmlDoc.DocumentElement.GetElementsByTagName("versao")[0].InnerText;
                    }
                    break;

                default:
                    versaoXML = "1.01";
                    break;
            }

            return versaoXML;
        }

        #endregion DefinirVersaoXML
    }
}