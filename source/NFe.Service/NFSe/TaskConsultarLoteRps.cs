using EBank.Solutions.Primitives.PIX.Models;
using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.ServiceModel;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.NFSe
{
    /// <summary>
    /// Consultar uma NFS-e por RPS
    /// </summary>
    public class TaskNFSeConsultarLoteRps : TaskAbst
    {
        #region Objeto com os dados do XML de lote rps

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do lote rps
        /// </summary>
        private DadosPedLoteRps oDadosPedSitNfseRps;

        #endregion Objeto com os dados do XML de lote rps

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            ///
            /// extensao permitida: PedLoteRps = "-ped-loterps.xml";
            ///
            /// Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeConsultarLoteRps;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).EnvioXML) + Propriedade.ExtRetorno.LoteRps_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                // var ler = new LerXML();
                oDadosPedSitNfseRps = new DadosPedLoteRps(emp);

                PedLoteRps(emp, NomeArquivoXML);
                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosPedSitNfseRps.cMunicipio);

                ExecuteDLL(emp, oDadosPedSitNfseRps.cMunicipio, padraoNFSe);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).EnvioXML, Propriedade.ExtRetorno.LoteRps_ERR, ex);
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

        #region PedLoteRps()

        /// <summary>
        /// Fazer a leitura do conteúdo do XML de lote rps e disponibiliza o conteúdo em um objeto para analise
        /// </summary>
        /// <param name="arquivoXML">Arquivo XML que é para efetuar a leitura</param>
        private void PedLoteRps(int emp, string arquivoXML)
        {
        }

        #endregion PedLoteRps()

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

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).RetornoXML;
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
                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNotaValida:
                    var consultarNotaValida = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNotaValida(conteudoXML, configuracao);
                    consultarNotaValida.Executar();

                    vStrXmlRetorno = consultarNotaValida.RetornoWSString;

                    consultarNotaValida.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarLoteRps:
                    var consultarLoteRps = new Unimake.Business.DFe.Servicos.NFSe.ConsultarLoteRps(conteudoXML, configuracao);
                    consultarLoteRps.Executar();

                    vStrXmlRetorno = consultarLoteRps.RetornoWSString;

                    consultarLoteRps.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultaLote:
                    var consultaLote = new Unimake.Business.DFe.Servicos.NFSe.ConsultaLote(conteudoXML, configuracao);
                    consultaLote.Executar();

                    vStrXmlRetorno = consultaLote.RetornoWSString;

                    consultaLote.Dispose();
                    break;
            }

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }


    }
}
