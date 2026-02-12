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
            var servico = DefinirServico(municipio, conteudoXML);
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);

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

        private Unimake.Business.DFe.Servicos.Servico DefinirServico(int municipio, XmlDocument doc)
        {
            var result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarLoteRps;

            var padraoNFSe = Functions.BuscaPadraoNFSe(municipio);

            switch (padraoNFSe)
            {
                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SIGCORP:
                case PadraoNFSe.MEMORY:
                case PadraoNFSe.DSF:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNotaValida":
                        case "ns1:ReqConsultaNotas":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNotaValida;
                            break;
                        case "ConsultarLoteRps":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarLoteRps;
                            break;
                    }
                    break;

                case PadraoNFSe.PAULISTANA:
                case PadraoNFSe.ISSONLINE_ASSESSORPUBLICO:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultaLote;
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
                case PadraoNFSe.BETHA:
                    versaoXML = "2.02";

                    if (xmlDoc.DocumentElement.Name.Contains("e:"))
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.NACIONAL:
                    versaoXML = (xmlDoc.GetElementsByTagName(xmlDoc.DocumentElement.Name)[0]).Attributes.GetNamedItem("versao").Value;
                    break;

                case PadraoNFSe.NOBESISTEMAS:
                case PadraoNFSe.BHISS:
                case PadraoNFSe.EQUIPLANO:
                case PadraoNFSe.MEMORY:
                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.PROPRIOBARUERISP:
                case PadraoNFSe.THEMA:
                case PadraoNFSe.AGILI:
                case PadraoNFSe.CARIOCA:
                case PadraoNFSe.SALVADOR_BA:
                case PadraoNFSe.MANAUS_AM:
                case PadraoNFSe.LIBRE:
                case PadraoNFSe.EGOVERNE:
                case PadraoNFSe.METROPOLIS:
                case PadraoNFSe.ISSONLINE_ASSESSORPUBLICO:
                case PadraoNFSe.EGOVERNEISS:
                case PadraoNFSe.INTERSOL:
                case PadraoNFSe.LEXSOM:
                    versaoXML = "1.00";
                    break;

                case PadraoNFSe.NATALENSE:
                    versaoXML = "2.00";
                    break;

                case PadraoNFSe.PAULISTANA:
                    versaoXML = "1.00";
                    if (xmlDoc.InnerXml.Contains("Versao=\"2\"") || xmlDoc.InnerXml.Contains("Versao=\"2.00\""))
                    {
                        versaoXML = "2.00";
                    }
                    break;


                case PadraoNFSe.DBSELLER:
                    versaoXML = "1.00";

                    if (codMunicipio == 4319901 || codMunicipio == 4321600)
                    {
                        versaoXML = "2.04";
                    }

                    break;

                case PadraoNFSe.TINUS:
                    versaoXML = "1.00";
                    if (xmlDoc.InnerXml.Contains("versao=\"2.03\"") || xmlDoc.InnerXml.Contains("abrasf"))
                    {
                        versaoXML = "2.03";
                        break;
                    }
                    break;

                case PadraoNFSe.DIGIFRED:
                case PadraoNFSe.BSITBR:
                    versaoXML = "2.00";
                    break;

                case PadraoNFSe.PRODATA:
                case PadraoNFSe.QUASAR:
                case PadraoNFSe.ABASE:
                case PadraoNFSe.SONNER:
                case PadraoNFSe.PRODEB:
                case PadraoNFSe.VITORIA_ES:
                case PadraoNFSe.SUPERNOVA:
                    versaoXML = "2.01";
                    break;

                case PadraoNFSe.FIORILLI:
                    versaoXML = "2.01";
                    if (xmlDoc.InnerXml.Contains("versao=\"1.00\""))
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.NOTAINTELIGENTE:
                case PadraoNFSe.AVMB:
                case PadraoNFSe.WEBISS:
                case PadraoNFSe.VERSATEC:
                case PadraoNFSe.EMBRAS:
                case PadraoNFSe.PORTAL_FACIL:
                case PadraoNFSe.E_RECEITA:
                case PadraoNFSe.SH3:
                case PadraoNFSe.MODERNIZACAO_PUBLICA:
                case PadraoNFSe.FUTURIZE:
                    versaoXML = "2.02";
                    break;

                case PadraoNFSe.RLZ_INFORMATICA:
                case PadraoNFSe.ELOTECH:
                case PadraoNFSe.DESENVOLVECIDADE:
                case PadraoNFSe.INDAIATUBA_SP:
                case PadraoNFSe.FISCO:
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

                case PadraoNFSe.SMARAPD:
                    versaoXML = "2.03";

                    if (codMunicipio == 3205002 || codMunicipio == 3516200)
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.DSF:
                    versaoXML = "2.03";

                    if ((codMunicipio == 3509502 && (xmlDoc.OuterXml.Contains("ns1:ReqConsultaLote") || xmlDoc.OuterXml.Contains("ns1:ReqConsultaNotas")) || codMunicipio == 5002704
                     || codMunicipio == 3303500 || codMunicipio == 2111300))
                    {
                        versaoXML = "1.00";
                    }
                    else if (codMunicipio == 3170206)
                    {
                        versaoXML = "2.04";
                    }
                    else if (codMunicipio == 3549904 && xmlDoc.OuterXml.Contains("ginfes"))
                    {
                        versaoXML = "3.00";
                    }
                    break;

                case PadraoNFSe.ADM_SISTEMAS:
                    versaoXML = "2.03";

                    if (codMunicipio == 1400100)
                    {
                        versaoXML = "2.01";
                    }

                    break;

                case PadraoNFSe.SIMPLISS:
                    if (codMunicipio == 3306305 || codMunicipio == 4202404)
                    {
                        versaoXML = "2.03";
                        break;
                    }
                    versaoXML = "3.00";
                    break;

                case PadraoNFSe.PRONIM:
                    versaoXML = "2.03";

                    if (codMunicipio == 3113404 || codMunicipio == 4321006 || codMunicipio == 3131703 ||
                        codMunicipio == 4303004 || codMunicipio == 4300109 || codMunicipio == 3143302 ||
                        codMunicipio == 4306932 || codMunicipio == 3302205 || codMunicipio == 3530300)
                    {
                        versaoXML = "2.02";
                    }
                    if (codMunicipio == 3535804 || codMunicipio == 4304507 || codMunicipio == 4321709 ||
                        codMunicipio == 4122404)
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.PROPRIOJOINVILLESC:
                case PadraoNFSe.TRIBUTUS:
                case PadraoNFSe.GISSONLINE:
                    versaoXML = "2.04";
                    break;

                case PadraoNFSe.ISSNET:
                    versaoXML = "1.01";
                    if (xmlDoc.InnerXml.Contains("ConsultarLoteRpsEnvio"))
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.ABACO:
                    versaoXML = "2.04";
                    if (codMunicipio == 5108402)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.IPM:
                    versaoXML = "2.04";
                    break;

                case PadraoNFSe.PROPRIOFORTALEZACE:
                    versaoXML = "4.00";
                    break;

                case PadraoNFSe.GINFES:
                    versaoXML = "3.00";

                    if (xmlDoc.InnerXml.Contains("xmlns:ns1"))
                    {
                        versaoXML = "3.01";
                    }
                    break;

                case PadraoNFSe.PUBLICA:
                    versaoXML = "3.00";
                    break;

                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SIGCORP:
                    versaoXML = "2.03";

                    if (xmlDoc.DocumentElement.Name.Contains("ConsultarNotaValida"))
                    {
                        versaoXML = "3.00";
                    }
                    else if (codMunicipio == 4204202 || codMunicipio == 3131307 ||
                             codMunicipio == 3530805 || codMunicipio == 3145208 ||
                             codMunicipio == 3300704)
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.EL:
                    versaoXML = "2.04";

                    if (codMunicipio == 3201506 || codMunicipio == 3204203)
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.FINTEL:
                    versaoXML = "2.02";

                    if (codMunicipio == 4115200)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.TIPLAN:
                    versaoXML = "2.03";

                    if (codMunicipio == 2611606)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.CONAM:
                    versaoXML = "2.00";

                    if (codMunicipio == 3506102 || codMunicipio == 3509007)
                    {
                        versaoXML = "4.00";
                    }

                    break;

                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Consulta de Lote via RPS.");

            }

            return versaoXML;
        }
    }
}