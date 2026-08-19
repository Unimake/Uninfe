using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
namespace NFe.Service.NFSe
{
    public class TaskNFSeConsultarPorRps : TaskAbst
    {
        #region Objeto com os dados do XML de lote rps

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do lote rps
        /// </summary>
        private DadosPedSitNfseRps oDadosPedSitNfseRps;

        #endregion Objeto com os dados do XML de lote rps

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            ///
            /// extensao permitida: PedSitNfseRps = "-ped-sitnfserps.xml";
            ///
            /// Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeConsultarPorRps;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).EnvioXML) + Propriedade.ExtRetorno.SitNfseRps_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosPedSitNfseRps = new DadosPedSitNfseRps(emp);
                //var ler = new LerXML();
                PedSitNfseRps(emp, NomeArquivoXML);
                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosPedSitNfseRps.cMunicipio);

                ExecuteDLL(emp, oDadosPedSitNfseRps.cMunicipio, padraoNFSe);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).EnvioXML, Propriedade.ExtRetorno.SitNfseRps_ERR, ex);
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

        #region PedSitNfseRps()

        /// <summary>
        /// Fazer a leitura do conteúdo do XML de lote rps e disponibiliza o conteúdo em um objeto para analise
        /// </summary>
        /// <param name="arquivoXML">Arquivo XML que é para efetuar a leitura</param>
        private void PedSitNfseRps(int emp, string arquivoXML)
        {
        }

        #endregion PedSitNfseRps()

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

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).RetornoXML;
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
                SchemaVersao = versaoXML,
                MunicipioToken = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioSenha = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioUsuario = Empresas.Configuracoes[emp].UsuarioWS
            };

            switch (servico)
            {
                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfsePorRps:
                    var consultarNfsePorRps = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfsePorRps(conteudoXML, configuracao);
                    consultarNfsePorRps.Executar();

                    vStrXmlRetorno = consultarNfsePorRps.RetornoWSString;

                    consultarNfsePorRps.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultaNotaFiscal:
                    var consultaNotaFiscal = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNotaFiscal(conteudoXML, configuracao);
                    consultaNotaFiscal.Executar();

                    vStrXmlRetorno = consultaNotaFiscal.RetornoWSString;

                    consultaNotaFiscal.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRpsServicoPrestado:
                    var consultarRpsServicoPrestado = new Unimake.Business.DFe.Servicos.NFSe.ConsultarRpsServicoPrestado(conteudoXML, configuracao);
                    consultarRpsServicoPrestado.Executar();

                    vStrXmlRetorno = consultarRpsServicoPrestado.RetornoWSString;

                    consultarRpsServicoPrestado.Dispose();
                    break;

            }


            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }


    }
}
