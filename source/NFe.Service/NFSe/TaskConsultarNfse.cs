using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Threading;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.NFSe
{
    public class TaskNFSeConsultar : TaskAbst
    {
        #region Objeto com os dados do XML da consulta nfse

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s da consulta nfse
        /// </summary>
        private DadosPedSitNfse oDadosPedSitNfse;

        #endregion Objeto com os dados do XML da consulta nfse

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            if (Empresas.Configuracoes[emp].TempoEnvioNFSe > 0)
            {
                if (Empresas.Configuracoes[emp].DataHoraUltimoEnvioNFSe != DateTime.MinValue)
                {
                    var diferenca = DateTime.Now - Empresas.Configuracoes[emp].DataHoraUltimoEnvioNFSe;
                    var segundosPassados = diferenca.TotalSeconds;

                    if (segundosPassados < Empresas.Configuracoes[emp].TempoEnvioNFSe)
                    {
                        Thread.Sleep((Empresas.Configuracoes[emp].TempoEnvioNFSe - Convert.ToInt32(segundosPassados)) * 1000);
                    }
                }

                Empresas.Configuracoes[emp].DataHoraUltimoEnvioNFSe = DateTime.Now;
            }

            ///
            /// extensao permitida:  PedSitNfse = "-ped-sitnfse.xml"
            ///
            /// Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeConsultar;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML) + Propriedade.ExtRetorno.SitNfse_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosPedSitNfse = new DadosPedSitNfse(emp);
                PedSitNfse(NomeArquivoXML);
                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosPedSitNfse.cMunicipio);

                ExecuteDLL(emp, oDadosPedSitNfse.cMunicipio, padraoNFSe);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML, Propriedade.ExtRetorno.SitNfse_ERR, ex);
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

        #region PedSitNfse()

        /// <summary>
        /// Fazer a leitura do conteúdo do XML de consulta nfse por numero e disponibiliza conteúdo em um objeto para analise
        /// </summary>
        /// <param name="arquivoXML">Arquivo XML que é para efetuar a leitura</param>
        private void PedSitNfse(string arquivoXML)
        {
        }

        #endregion PedSitNfse()

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

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).RetornoXML;
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

            if (padraoNFSe == PadraoNFSe.WEBFISCO)
            {
                XmlElement root = conteudoXML.DocumentElement;
                XmlNode firstElement = root.FirstChild;
                XmlNode tagUsuario = conteudoXML.CreateElement("usuario");
                XmlNode tagSenha = conteudoXML.CreateElement("pass");

                tagUsuario.InnerText = configuracao.MunicipioUsuario;
                tagSenha.InnerText = configuracao.MunicipioSenha;
                root.InsertBefore(tagUsuario, firstElement);
                root.InsertBefore(tagSenha, firstElement);

                conteudoXML.AppendChild(root);
            }

            switch (servico)
            {
                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultaLote:
                    var consultarNfseLote = new Unimake.Business.DFe.Servicos.NFSe.ConsultaLote(conteudoXML, configuracao);
                    consultarNfseLote.Executar();

                    vStrXmlRetorno = consultarNfseLote.RetornoWSString;

                    consultarNfseLote.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse:
                    var consultarNfse = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfse(conteudoXML, configuracao);
                    consultarNfse.Executar();

                    vStrXmlRetorno = consultarNfse.RetornoWSString;

                    consultarNfse.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa:
                    var consultarNfseFaixa = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfseFaixa(conteudoXML, configuracao);
                    consultarNfseFaixa.Executar();

                    vStrXmlRetorno = consultarNfseFaixa.RetornoWSString;

                    consultarNfseFaixa.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNotaPrestador:
                    var consultarNotaPrestador = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNotaPrestador(conteudoXML, configuracao);
                    consultarNotaPrestador.Executar();

                    vStrXmlRetorno = consultarNotaPrestador.RetornoWSString;

                    consultarNotaPrestador.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado:
                    var consultarNfseServicoPrestado = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfseServicoPrestado(conteudoXML, configuracao);
                    consultarNfseServicoPrestado.Executar();

                    vStrXmlRetorno = consultarNfseServicoPrestado.RetornoWSString;

                    consultarNfseServicoPrestado.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRpsServicoPrestado:
                    var ConsultarRpsServicoPrestado = new Unimake.Business.DFe.Servicos.NFSe.ConsultarRpsServicoPrestado(conteudoXML, configuracao);
                    ConsultarRpsServicoPrestado.Executar();

                    vStrXmlRetorno = ConsultarRpsServicoPrestado.RetornoWSString;

                    ConsultarRpsServicoPrestado.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfsePorRps:
                    var NFSeConsultarNfsePorRps = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfsePorRps(conteudoXML, configuracao);
                    NFSeConsultarNfsePorRps.Executar();

                    vStrXmlRetorno = NFSeConsultarNfsePorRps.RetornoWSString;

                    NFSeConsultarNfsePorRps.Dispose();
                    break;
            }


            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }


    }
}
