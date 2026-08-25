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
            var resolucao = ResolucaoCentralizadaNFSe.Resolver(conteudoXML, padraoNFSe, municipio);
            var versaoXML = resolucao.Versao;
            var servico = resolucao.Servico;

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
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


    }
}
