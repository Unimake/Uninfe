using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
namespace NFe.Service.NFSe
{
    public class TaskNFSeConsultaSituacaoLoteRps : TaskAbst
    {
        #region Objeto com os dados do XML de consulta situação do lote rps

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do pedido de consulta da situação do lote rps
        /// </summary>
        private DadosPedSitLoteRps oDadosPedSitLoteRps;

        #endregion Objeto com os dados do XML de consulta situação do lote rps

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            ///
            /// extensao permitida: PedSitLoteRps = "-ped-sitloterps.xml";
            ///
            /// Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeConsultarSituacaoLoteRps;

            try
            {
                oDadosPedSitLoteRps = new DadosPedSitLoteRps(emp);
                //Ler o XML para pegar parâmetros de envio
                //LerXML ler = new LerXML();
                PedSitLoteRps(NomeArquivoXML);
                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosPedSitLoteRps.cMunicipio);

                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                        Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitLoteRps).EnvioXML) + Propriedade.ExtRetorno.SitLoteRps_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                ExecuteDLL(emp, oDadosPedSitLoteRps.cMunicipio, padraoNFSe);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitLoteRps).EnvioXML, Propriedade.ExtRetorno.SitLoteRps_ERR, ex);
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

        #region PedSitLoteRps()

        /// <summary>
        /// Fazer a leitura do conteúdo do XML de consulta situação do lote rps e disponibilizar conteúdo em um objeto para analise
        /// </summary>
        /// <param name="arquivoXML">Arquivo XML que é para efetuar a leitura</param>
        private void PedSitLoteRps(string arquivoXML)
        {
            //int emp = Empresas.FindEmpresaByThread();

            //XmlDocument doc = new XmlDocument();
            //doc.Load(arquivoXML);

            //XmlNodeList infConsList = doc.GetElementsByTagName("ConsultarSituacaoLoteRpsEnvio");

            //foreach (XmlNode infConsNode in infConsList)
            //{
            //    XmlElement infConsElemento = (XmlElement)infConsNode;
            //}
        }

        #endregion PedSitLoteRps()

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

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitLoteRps).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitLoteRps).RetornoXML;
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);
            var servico = DefinirServico(municipio, conteudoXML);

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                TipoDFe = Unimake.Business.DFe.Servicos.TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = servico,
                SchemaVersao = versaoXML
            };

            switch (servico)
            {
                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultaInformacoesLote:
                    var consultaInformacoesLote = new Unimake.Business.DFe.Servicos.NFSe.ConsultaInformacoesLote(conteudoXML, configuracao);
                    consultaInformacoesLote.Executar();

                    vStrXmlRetorno = consultaInformacoesLote.RetornoWSString;

                    consultaInformacoesLote.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarSituacaoLoteRps:
                    var consultarSituacaoLoteRps = new Unimake.Business.DFe.Servicos.NFSe.ConsultarSituacaoLoteRps(conteudoXML, configuracao);
                    consultarSituacaoLoteRps.Executar();

                    vStrXmlRetorno = consultarSituacaoLoteRps.RetornoWSString;

                    consultarSituacaoLoteRps.Dispose();
                    break;


                case Unimake.Business.DFe.Servicos.Servico.NFSeObterCriticaLote:
                    var obterCriticaLote = new Unimake.Business.DFe.Servicos.NFSe.ObterCriticaLote(conteudoXML, configuracao);
                    obterCriticaLote.Executar();

                    vStrXmlRetorno = obterCriticaLote.RetornoWSString;

                    obterCriticaLote.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRequerimentoCancelamento:
                    var consultarRequerimentoCancelamento = new Unimake.Business.DFe.Servicos.NFSe.ConsultarRequerimentoCancelamento(conteudoXML, configuracao);
                    consultarRequerimentoCancelamento.Executar();

                    vStrXmlRetorno = consultarRequerimentoCancelamento.RetornoWSString;

                    consultarRequerimentoCancelamento.Dispose();
                    break;
            }

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSubstNfse).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedSubstNfse).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }

        private Unimake.Business.DFe.Servicos.Servico DefinirServico(int municipio, XmlDocument doc)
        {
            var result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarSituacaoLoteRps;

            var padraoNFSe = Functions.BuscaPadraoNFSe(municipio);

            switch (padraoNFSe)
            {
                case PadraoNFSe.PAULISTANA:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultaInformacoesLote;
                    break;

                case PadraoNFSe.GIF:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeObterCriticaLote;
                    break;

                case PadraoNFSe.AGILI:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRequerimentoCancelamento;
                    break;
            }

            return result;
        }

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
                case PadraoNFSe.GIF:
                case PadraoNFSe.BHISS:
                case PadraoNFSe.EQUIPLANO:
                case PadraoNFSe.MEMORY:
                case PadraoNFSe.AGILI:
                case PadraoNFSe.SALVADOR_BA:
                case PadraoNFSe.MANAUS_AM:
                case PadraoNFSe.NATALENSE:
                case PadraoNFSe.THEMA:
                case PadraoNFSe.EGOVERNE:
                case PadraoNFSe.METROPOLIS:
                case PadraoNFSe.INTERSOL:
                case PadraoNFSe.LEXSOM:
                case PadraoNFSe.PRONIM:
                    versaoXML = "1.00";
                    break;

                case PadraoNFSe.PAULISTANA:
                    versaoXML = "1.00";
                    if (xmlDoc.InnerXml.Contains("Versao=\"2\"") || xmlDoc.InnerXml.Contains("Versao=\"2.00\""))
                    {
                        versaoXML = "2.00";
                    }
                    break;


                case PadraoNFSe.TIPLAN:
                    versaoXML = "2.01";

                    if (codMunicipio == 3304003)
                    {
                        versaoXML = "2.03";
                    }
                    break;

                case PadraoNFSe.FINTEL:
                    versaoXML = "2.02";

                    if (codMunicipio == 4115200)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.TINUS:
                    versaoXML = "1.00";
                    if (xmlDoc.InnerXml.Contains("versao=\"2.03\""))
                    {
                        versaoXML = "2.03";
                        break;
                    }
                    break;


                case PadraoNFSe.SIMPLISS:   //Versão 2.03 não possui esse serviço -> Blumenau - SC e Volta Redonda - RJ
                case PadraoNFSe.GINFES:
                case PadraoNFSe.DSF:        //DSF não possui este serviço, porém, São José dos Campos - SP aceita layout do padrão GINFES e aceita este serviço na versão 3.00
                    versaoXML = "3.01";
                    break;

                case PadraoNFSe.PUBLICA:
                    versaoXML = "3.00";
                    break;
                case PadraoNFSe.ABACO:
                    versaoXML = "2.04";

                    if (codMunicipio == 5108402)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.EL:

                    versaoXML = "2.04";
                    if (codMunicipio == 3201506 || codMunicipio == 3204203)
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.DBSELLER:
                    versaoXML = "1.00";

                    if (codMunicipio == 4319901 || codMunicipio == 4321600)
                    {
                        versaoXML = "2.04";
                    }

                    break;

                case PadraoNFSe.BETHA:
                    versaoXML = "1.00";
                    break;

                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Consulta de Situação de Lote RPS.");

            }

            return versaoXML;
        }
    }
}