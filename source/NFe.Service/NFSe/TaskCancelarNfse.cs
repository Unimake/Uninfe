using NFe.Components;
using NFe.Settings;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFSe.NACIONAL;

namespace NFe.Service.NFSe
{
    public class TaskNFSeCancelar : TaskAbst
    {
        #region Private Fields

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do pedido de cancelamento
        /// </summary>
        private DadosPedCanNfse oDadosPedCanNfse;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Fazer a leitura do conteúdo do XML de cancelamento de NFS-e e disponibilizar conteúdo em um objeto para analise
        /// </summary>
        /// <param name="arquivoXML">Arquivo XML que é para efetuar a leitura</param>
        private void PedCanNfse(int emp, string arquivoXML)
        {
        }

        #endregion Private Methods

        #region Public Methods

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            //Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeCancelar;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML) + Propriedade.ExtRetorno.CanNfse_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosPedCanNfse = new DadosPedCanNfse(emp);
                PedCanNfse(emp, NomeArquivoXML);
                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosPedCanNfse.cMunicipio);

                ExecuteDLL(emp, oDadosPedCanNfse.cMunicipio, padraoNFSe);

                AnalisarRetorno(vStrXmlRetorno, padraoNFSe, emp);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML, Propriedade.ExtRetorno.CanNfse_ERR, ex);
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

        /// <summary>
        /// Analisar o XML retornado se for ambiente nacional e se o evento  tiver sido autorizado vamos salvar o XML na pasta autorizados
        /// </summary>
        /// <param name="vStrXmlRetorno">XML Retornado</param>
        /// <param name="padraoNFSe">Padrão da NFSe</param>
        /// <param name="emp">Codigo da empresa</param>
        private void AnalisarRetorno(string vStrXmlRetorno, PadraoNFSe padraoNFSe, int emp)
        {
            if (padraoNFSe != PadraoNFSe.NACIONAL)
            {
                return;
            }

            var pastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado;
            if (string.IsNullOrWhiteSpace(pastaEnviado))
            {
                return;
            }

            try
            {
                var autorizou = false;
                var doc = new XmlDocument();
                doc.LoadXml(vStrXmlRetorno);
                var eventoNodes = doc.GetElementsByTagName("evento");

                foreach (XmlElement evento in eventoNodes)
                {
                    var infEvento = evento["infEvento"] as XmlElement;
                    if (infEvento == null)
                    {
                        continue;
                    }

                    autorizou = true;

                    var dhProcTexto = infEvento?["dhProc"]?.InnerText;
                    if (string.IsNullOrWhiteSpace(dhProcTexto))
                    {
                        continue;
                    }

                    if (!DateTimeOffset.TryParseExact(dhProcTexto, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dhProcOffset))
                    {
                        continue;
                    }

                    var dhProc = dhProcOffset.DateTime;
                    var id = infEvento.GetAttribute("Id");
                    if (string.IsNullOrWhiteSpace(id) || id.Length <= 3)
                    {
                        continue;
                    }

                    var nameArq = id.Substring(3) + "-proceventonfse.xml";
                    var pathFile = Path.Combine(pastaEnviado, PastaEnviados.Autorizados.ToString(), Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(dhProc), nameArq);

                    var dir = Path.GetDirectoryName(pathFile);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (!File.Exists(pathFile))
                    {
                        File.WriteAllText(pathFile, vStrXmlRetorno);
                    }
                }

                if (!autorizou)
                {
                    Functions.Move(NomeArquivoXML, Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Path.GetFileName(NomeArquivoXML))); //Move o arquivo para a pasta de erro
                }
            }
            catch (Exception ex)
            {
                // Logar erro se necessário, mas não lançar para não interromper o fluxo
                Auxiliar.WriteLog($"Erro ao salvar o XML do Evento da NFSe autorizado: {ex.Message}", false);
            }
        }

        #endregion Public Methods

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

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).RetornoXML;
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
                TokenSoap = Empresas.Configuracoes[emp].SenhaWS,
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
            else if (padraoNFSe == PadraoNFSe.SOFTPLAN)
            {
                configuracao.ClientID = Empresas.Configuracoes[emp].ClientID;
                configuracao.ClientSecret = Empresas.Configuracoes[emp].ClientSecret;

                if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].TokenNFse))
                {
                    configuracao.MunicipioToken = Empresas.Configuracoes[emp].TokenNFse;
                    configuracao.MunicipioTokenValidade = Empresas.Configuracoes[emp].TokenNFSeExpire;
                }
            }

            switch (servico)
            {
                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNfse:
                    var cancelarNfse = new Unimake.Business.DFe.Servicos.NFSe.CancelarNfse(conteudoXML, configuracao);
                    cancelarNfse.Executar();
                    vStrXmlRetorno = cancelarNfse.RetornoWSString;

                    cancelarNfse.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelamentoNfe:
                    var cancelamentoNfe = new Unimake.Business.DFe.Servicos.NFSe.CancelamentoNfe(conteudoXML, configuracao);
                    cancelamentoNfe.Executar();
                    vStrXmlRetorno = cancelamentoNfe.RetornoWSString;

                    cancelamentoNfe.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelaNota:
                    var cancelaNota = new Unimake.Business.DFe.Servicos.NFSe.CancelaNota(conteudoXML, configuracao);
                    cancelaNota.Executar();
                    vStrXmlRetorno = cancelaNota.RetornoWSString;

                    cancelaNota.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNotaFiscal:
                    var cancelarNotaFiscal = new Unimake.Business.DFe.Servicos.NFSe.CancelarNotaFiscal(conteudoXML, configuracao);
                    cancelarNotaFiscal.Executar();
                    vStrXmlRetorno = cancelarNotaFiscal.RetornoWSString;

                    cancelarNotaFiscal.Dispose();
                    break;
            }

            if (padraoNFSe == PadraoNFSe.SOFTPLAN)
            {
                var tokenGeradoUniNFe = Empresas.Configuracoes[emp].TokenNFse;
                var tokenGeradoDLL = configuracao.MunicipioToken.Replace("Bearer ", "");

                if (tokenGeradoUniNFe != tokenGeradoDLL)
                {
                    Empresas.Configuracoes[emp].SalvarConfiguracoesNFSeSoftplan(configuracao.MunicipioUsuario,
                                                                                configuracao.MunicipioSenha,
                                                                                configuracao.ClientID,
                                                                                configuracao.ClientSecret,
                                                                                Empresas.Configuracoes[emp].CNPJ,
                                                                                configuracao.MunicipioTokenValidade,
                                                                                tokenGeradoDLL);
                }
            }

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }

        /// <summary>
        /// Define qual o tipo de serviço de envio de NFSe será utilizado. Envio em lote sincrono, Envio em lote assincrono ou envio de uma única NFSe síncrono.
        /// </summary>
        /// <param name="municipio">Código do município para onde será enviado o XML</param>
        /// <param name="doc">Conteúdo do XML da NFSe</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        /// <returns>Retorna o tipo de serviço de envio da NFSe da prefeitura será utilizado</returns>
        private Unimake.Business.DFe.Servicos.Servico DefinirServico(int municipio, XmlDocument doc)
        {
            var result = Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNfse;

            var padraoNFSe = Functions.BuscaPadraoNFSe(municipio);

            switch (padraoNFSe)
            {
                case PadraoNFSe.PAULISTANA:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeCancelamentoNfe;
                    break;

                case PadraoNFSe.OBARATEC:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeCancelaNota;
                    break;

                case PadraoNFSe.PRIMAX:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNotaFiscal;
                    break;

                case PadraoNFSe.GIF:
                    if (doc.DocumentElement.Name == "pedRegEvento")
                    {
                        result = Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNfse;
                        break;
                    }
                    else
                    {
                        result = Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNotaFiscal;
                        break;
                    }

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

                case PadraoNFSe.BETHA_CLOUD:
                    versaoXML = "1.01";
                    break;

                case PadraoNFSe.NACIONAL:
                    versaoXML = (xmlDoc.GetElementsByTagName(xmlDoc.DocumentElement.Name)[0]).Attributes.GetNamedItem("versao").Value;
                    break;

                case PadraoNFSe.NOBESISTEMAS:
                case PadraoNFSe.BHISS:
                case PadraoNFSe.OBARATEC:
                case PadraoNFSe.EQUIPLANO:
                case PadraoNFSe.MEMORY:
                case PadraoNFSe.BAUHAUS:
                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.SIMPLE:
                case PadraoNFSe.THEMA:
                case PadraoNFSe.WEBFISCO:
                case PadraoNFSe.AGILI:
                case PadraoNFSe.CARIOCA:
                case PadraoNFSe.LIBRE:
                case PadraoNFSe.HM2SOLUCOES:
                case PadraoNFSe.EGOVERNE:
                case PadraoNFSe.CECAM:
                case PadraoNFSe.METROPOLIS:
                case PadraoNFSe.ISSONLINE_ASSESSORPUBLICO:
                case PadraoNFSe.EGOVERNEISS:
                case PadraoNFSe.INTERSOL:
                case PadraoNFSe.PUBLICENTER:
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

                case PadraoNFSe.PRIMAX:
                    if (xmlDoc.InnerXml.Contains("versao=\"1.01\""))
                    {
                        versaoXML = "1.01";
                        break;
                    }
                    else
                    {
                        versaoXML = "1.00";
                        break;
                    }

                case PadraoNFSe.TINUS:
                    versaoXML = "1.00";
                    if (xmlDoc.InnerXml.Contains("versao=\"2.03\""))
                    {
                        versaoXML = "2.03";
                        break;
                    }
                    break;

                case PadraoNFSe.DBSELLER:
                    versaoXML = "1.00";

                    if (codMunicipio == 4319901 || codMunicipio == 4321600)
                    {
                        versaoXML = "2.04";
                    }

                    break;

                case PadraoNFSe.IPM:
                    versaoXML = "1.20";

                    if (xmlDoc.InnerXml.Contains("<CancelarNfseEnvio>"))
                    {
                        versaoXML = "2.04";
                    }
                    else if (codMunicipio == 4309308 || codMunicipio == 4316006 || codMunicipio == 4314050 ||
                             codMunicipio == 4320206)
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.DIGIFRED:
                case PadraoNFSe.GIAP:
                case PadraoNFSe.BSITBR:
                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SOFTPLAN:
                case PadraoNFSe.CENTI:
                    versaoXML = "2.00";
                    break;

                case PadraoNFSe.PRODATA:
                case PadraoNFSe.SONNER:
                case PadraoNFSe.ABASE:
                case PadraoNFSe.FIORILLI:
                case PadraoNFSe.SYSTEMPRO:
                case PadraoNFSe.PRODEB:
                case PadraoNFSe.VITORIA_ES:
                case PadraoNFSe.JLSOFT:
                case PadraoNFSe.SINSOFT:
                case PadraoNFSe.SUPERNOVA:
                    versaoXML = "2.01";
                    break;

                case PadraoNFSe.QUASAR:
                    versaoXML = "2.01";
                    if (xmlDoc.InnerXml.Contains("versao=\"1.01\""))
                    {
                        versaoXML = "1.01";
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


                case PadraoNFSe.FISCO:
                case PadraoNFSe.RLZ_INFORMATICA:
                case PadraoNFSe.ELOTECH:
                case PadraoNFSe.DESENVOLVECIDADE:
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

                case PadraoNFSe.DSF:
                    versaoXML = "2.03";

                    if ((codMunicipio == 3509502 && xmlDoc.OuterXml.Contains("ns1:ReqCancelamentoNFSe") || codMunicipio == 5002704 ||
                        codMunicipio == 3303500 || codMunicipio == 2111300))
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
                    if (codMunicipio == 3306305 || (codMunicipio == 4202404 && !xmlDoc.DocumentElement.Name.Contains("pedRegEvento")))

                    {
                        versaoXML = "2.03";
                        break;
                    }
                    if (xmlDoc.DocumentElement.Name.Contains("pedRegEvento"))
                    {
                        versaoXML = "1.01";
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

                case PadraoNFSe.SMARAPD:
                    versaoXML = "2.03";


                    if (codMunicipio == 3551702 || codMunicipio == 3202405)
                    {
                        versaoXML = "1.00";
                    }
                    else if (xmlDoc.OuterXml.Contains("infPedReg"))
                    {
                        versaoXML = "1.01";
                    }
                    else if (codMunicipio == 3205002 || codMunicipio == 3516200)
                    {
                        versaoXML = "2.04";
                    }
                    break;

                case PadraoNFSe.PROPRIOJOINVILLESC:
                case PadraoNFSe.TRIBUTUS:
                case PadraoNFSe.GISSONLINE:
                    versaoXML = "2.04";
                    break;

                case PadraoNFSe.ISSNET:
                    versaoXML = "2.04";
                    if (xmlDoc.GetElementsByTagName("pedRegEvento").Count > 0)
                    {
                        versaoXML = "1.01";
                    }
                    break;

                case PadraoNFSe.ABACO:
                    versaoXML = "2.04";
                    if (codMunicipio == 5108402)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.GINFES:
                    versaoXML = "3.00";

                    if (xmlDoc.GetElementsByTagName("Prestador").Count > 0)
                    {
                        versaoXML = "2.00";
                    }
                    break;

                case PadraoNFSe.PUBLICA:
                    versaoXML = "3.00";
                    break;

                case PadraoNFSe.SIGCORP:
                    if (xmlDoc.DocumentElement.Name.Contains("CancelarNota"))
                    {
                        if (codMunicipio == 4113700)
                        {
                            versaoXML = "1.03";
                        }
                        else
                        {
                            versaoXML = "3.00";
                        }
                    }
                    else if (!xmlDoc.DocumentElement.Name.Contains("CancelarNota"))
                    {
                        if (codMunicipio == 4204202)
                        {
                            versaoXML = "2.04";
                        }
                        else
                        {
                            versaoXML = "2.03";
                        }
                    }
                    if (codMunicipio == 3131307 || codMunicipio == 3530805 ||
                        codMunicipio == 3145208 || codMunicipio == 3300704)
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

                case PadraoNFSe.GIF:
                    if (xmlDoc.DocumentElement.Name.Contains("pedRegEvento"))
                    {
                        versaoXML = "1.01";
                        break;
                    }
                    versaoXML = "1.00";
                    break;

                case PadraoNFSe.CONAM:
                    versaoXML = "2.00";

                    if (codMunicipio == 3506102)
                    {
                        versaoXML = "4.00";
                    }

                    break;


                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Cancelamento de NFSe.");



            }

            return versaoXML;
        }
    }
}