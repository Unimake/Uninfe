using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
namespace NFe.Service.NFSe
{
    public class TaskConsultarNfseTomados : TaskAbst
    {
        public TaskConsultarNfseTomados(string arquivo)
        {
            Servico = Servicos.NFSeConsultarNFSeTomados;
            NomeArquivoXML = arquivo;
        }

        #region Objeto com os dados do XML da consulta nfse

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s da consulta nfse
        /// </summary>
        private DadosPedSitNfse dadosXML;

        #endregion Objeto com os dados do XML da consulta nfse

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                    Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).RetornoERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                dadosXML = new DadosPedSitNfse(emp);
                var padraoNFSe = Functions.BuscaPadraoNFSe(dadosXML.cMunicipio);

                ExecuteDLL(emp, dadosXML.cMunicipio, padraoNFSe);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).EnvioXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).RetornoERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 31/08/2011
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
                    //Se falhou algo na hora de deletar o XML de cancelamento de NFe, infelizmente
                    //não posso fazer mais nada, o UniNFe vai tentar mandar o arquivo novamente para o webservice, pois ainda não foi excluido.
                    //Wandrey 31/08/2011
                }
            }
        }

        #endregion Execute

        /// <summary>
        /// Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município
        /// </summary>
        /// <param name="codMunicipio">Código do município para onde será enviado o XML</param>
        /// <param name="xmlDoc">Conteúdo do XML da NFSe</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        /// <returns>Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município</returns>
        private void ExecuteDLL(int emp, int municipio, PadraoNFSe padraoNFSe)
        {
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(NomeArquivoXML);

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).RetornoXML;
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);
            var servico = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoTomado;

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                TipoDFe = Unimake.Business.DFe.Servicos.TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = servico,
                SchemaVersao = versaoXML,
                MunicipioToken = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioSenha = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioUsuario = Empresas.Configuracoes[emp].UsuarioWS
            };

            var consultarNfseServicoTomado = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfseServicoTomado(conteudoXML, configuracao);
            consultarNfseServicoTomado.Executar();

            vStrXmlRetorno = consultarNfseServicoTomado.RetornoWSString;

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }

            consultarNfseServicoTomado.Dispose();
        }

        /// <summary>
        /// Executa o serviço utilizando a DLL do UniNFe.
        /// </summary>
        /// <param name="emp">Empresa que está enviando o XML</param>
        /// <param name="municipio">Código do município para onde será enviado o XML</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        private string DefinirVersaoXML(int codMunicipio, XmlDocument xmlDoc, PadraoNFSe padraoNFSe)
        {
            var versaoXML = "0.00";

            switch (padraoNFSe)
            {
                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.GIF:
                case PadraoNFSe.LIBRE:
                case PadraoNFSe.HM2SOLUCOES:
                    versaoXML = "1.00";
                    break;

                case PadraoNFSe.DIGIFRED:
                    versaoXML = "2.00";
                    break;

                case PadraoNFSe.SONNER:
                case PadraoNFSe.QUASAR:
                case PadraoNFSe.FIORILLI:
                case PadraoNFSe.PRODEB:
                case PadraoNFSe.VITORIA_ES:
                case PadraoNFSe.SUPERNOVA:
                    versaoXML = "2.01";
                    break;

                case PadraoNFSe.BETHA:
                case PadraoNFSe.VERSATEC:
                case PadraoNFSe.WEBISS:
                case PadraoNFSe.EMBRAS:
                case PadraoNFSe.SH3:
                case PadraoNFSe.MODERNIZACAO_PUBLICA:
                case PadraoNFSe.FUTURIZE:
                    versaoXML = "2.02";
                    break;

                case PadraoNFSe.SIMPLISS:
                    if (codMunicipio == 3306305 || codMunicipio == 4202404)
                    {
                        versaoXML = "2.03";
                        break;
                    }
                    versaoXML = "3.00";
                    break;

                case PadraoNFSe.DSF:
                case PadraoNFSe.FISCO:
                case PadraoNFSe.RLZ_INFORMATICA:
                case PadraoNFSe.ELOTECH:
                case PadraoNFSe.DESENVOLVECIDADE:
                case PadraoNFSe.TIPLAN:
                case PadraoNFSe.INDAIATUBA_SP:
                    versaoXML = "2.03";
                    break;

                case PadraoNFSe.COPLAN:
                    versaoXML = "2.03";

                    if (codMunicipio == 3300407)
                    {
                        versaoXML = "2.02";
                    }
                    if (xmlDoc.InnerXml.Contains("xmlns=\"http://www.sped.fazenda.gov.br/nfse\""))
                    {
                        versaoXML = "1.01";
                    }
                    break;

                case PadraoNFSe.ADM_SISTEMAS:
                    versaoXML = "2.03";

                    if (codMunicipio == 1400100)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.EL:
                case PadraoNFSe.TRIBUTUS:
                case PadraoNFSe.GISSONLINE:
                    versaoXML = "2.04";
                    break;

                case PadraoNFSe.ISSNET:
                    versaoXML = "1.01";
                    if (xmlDoc.GetElementsByTagName("Pedido").Count > 0)
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SIGCORP:
                    versaoXML = "2.03";

                    if (codMunicipio == 4204202 || codMunicipio == 3131307 || codMunicipio == 3530805 || codMunicipio == 3145208)
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.SMARAPD:
                    versaoXML = "2.03";

                    if (codMunicipio == 3205002 || codMunicipio == 3516200)
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.FINTEL:
                    versaoXML = "2.02";

                    if (codMunicipio == 4115200)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Consulta de NFSe Tomadas.");
            }

            return versaoXML;
        }
    }
}