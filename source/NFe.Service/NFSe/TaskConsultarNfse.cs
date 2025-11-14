using NFe.Components;
using NFe.Settings;
using NFSe.Components;
using System;
using System.IO;
using System.ServiceModel;
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
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);
            var servico = DefinirServico(municipio, conteudoXML, padraoNFSe, versaoXML);

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
                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultaLote:
                    var consultarNfseLote = new Unimake.Business.DFe.Servicos.NFSe.ConsultaLote(conteudoXML, configuracao);
                    consultarNfseLote.Executar();

                    vStrXmlRetorno = consultarNfseLote.RetornoWSString;
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse:
                    var consultarNfse = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfse(conteudoXML, configuracao);
                    consultarNfse.Executar();

                    vStrXmlRetorno = consultarNfse.RetornoWSString;
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa:
                    var consultarNfseFaixa = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfseFaixa(conteudoXML, configuracao);
                    consultarNfseFaixa.Executar();

                    vStrXmlRetorno = consultarNfseFaixa.RetornoWSString;
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNotaPrestador:
                    var consultarNotaPrestador = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNotaPrestador(conteudoXML, configuracao);
                    consultarNotaPrestador.Executar();

                    vStrXmlRetorno = consultarNotaPrestador.RetornoWSString;
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado:
                    var consultarNfseServicoPrestado = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfseServicoPrestado(conteudoXML, configuracao);
                    consultarNfseServicoPrestado.Executar();

                    vStrXmlRetorno = consultarNfseServicoPrestado.RetornoWSString;
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRpsServicoPrestado:
                    var ConsultarRpsServicoPrestado = new Unimake.Business.DFe.Servicos.NFSe.ConsultarRpsServicoPrestado(conteudoXML, configuracao);
                    ConsultarRpsServicoPrestado.Executar();

                    vStrXmlRetorno = ConsultarRpsServicoPrestado.RetornoWSString;
                    break;
                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfsePorRps:
                    var NFSeConsultarNfsePorRps = new Unimake.Business.DFe.Servicos.NFSe.ConsultarNfsePorRps(conteudoXML, configuracao);
                    NFSeConsultarNfsePorRps.Executar();

                    vStrXmlRetorno = NFSeConsultarNfsePorRps.RetornoWSString;
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
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).RetornoXML);

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
        /// <param name="versaoXML">Versão do XML</param>
        /// <returns>Retorna o tipo de serviço de envio da NFSe da prefeitura será utilizado</returns>
        private Unimake.Business.DFe.Servicos.Servico DefinirServico(int municipio, XmlDocument doc, PadraoNFSe padraoNFSe, string versaoXML)
        {
            var result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse;

            switch (padraoNFSe)
            {
                case PadraoNFSe.SONNER:
                case PadraoNFSe.SMARAPD:
                case PadraoNFSe.TRIBUTUS:
                case PadraoNFSe.DSF:
                case PadraoNFSe.DIGIFRED:
                case PadraoNFSe.WEBISS:
                case PadraoNFSe.QUASAR:
                case PadraoNFSe.EL:
                case PadraoNFSe.COPLAN:
                case PadraoNFSe.SIMPLISS:
                case PadraoNFSe.ISSNET:
                case PadraoNFSe.FIORILLI:
                case PadraoNFSe.EMBRAS:
                case PadraoNFSe.FINTEL:
                case PadraoNFSe.PUBLICA:
                case PadraoNFSe.GISSONLINE:
                case PadraoNFSe.LIBRE:
                case PadraoNFSe.NATALENSE:
                case PadraoNFSe.RLZ_INFORMATICA:
                case PadraoNFSe.THEMA:
                case PadraoNFSe.HM2SOLUCOES:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNfseServicoPrestadoEnvio":
                        case "con:ConsultarNfseServicoPrestadoEnvio":
                        case "ns4:ConsultarNfseServicoPrestadoEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado;
                            break;
                        case "ConsultarNfseFaixaEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                            break;
                        case "ConsultarNfseEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfsePorRps;

                            if (municipio == 3549904 && doc.OuterXml.Contains("ginfes"))
                            {
                                result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse;
                            }

                            break;
                    }
                    break;

                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.ADM_SISTEMAS:
                case PadraoNFSe.ELOTECH:
                case PadraoNFSe.INDAIATUBA_SP:
                case PadraoNFSe.MODERNIZACAO_PUBLICA:
                case PadraoNFSe.SUPERNOVA:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNfseServicoPrestadoEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado;
                            break;

                        case "ConsultarNfseFaixaEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                            break;
                    }
                    break;

                case PadraoNFSe.PRODATA:
                case PadraoNFSe.AVMB:
                case PadraoNFSe.VERSATEC:
                case PadraoNFSe.PROPRIOBARUERISP:
                case PadraoNFSe.SYSTEMPRO:
                case PadraoNFSe.E_RECEITA:
                case PadraoNFSe.SH3:
                case PadraoNFSe.SINSOFT:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                    break;

                case PadraoNFSe.ABACO:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNfse":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse;
                            break;
                        case "ConsultarNfseFaixaEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                            break;
                    }
                    break;

                case PadraoNFSe.GIF:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNfse":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse;
                            break;
                        case "pedidoLoteNFSe":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultaLote;
                            break;
                        case "ConsultarNfseServicoPrestadoEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado;
                            break;
                    }
                    break;

                case PadraoNFSe.IPM:
                case PadraoNFSe.TIPLAN:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNfseServicoPrestadoEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado;
                            break;

                        case "ConsultarNfseFaixaEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                            break;

                        case "nfse":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse;
                            break;
                    }
                    break;

                case PadraoNFSe.BETHA:
                case PadraoNFSe.BETHA_CLOUD:
                    if (versaoXML == "2.02")
                    {
                        switch(doc.DocumentElement.Name)
                        {
                            case "ConsultarNfseServicoPrestadoEnvio":
                                result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado;
                                break;

                            case "ConsultarNfseFaixaEnvio":
                                result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                                break;
                        }
                    }
                    if(versaoXML == "1.00")
                    {
                        result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse;
                        break;
                    }
                        break;
                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SIGCORP:
                case PadraoNFSe.AGILI:
                case PadraoNFSe.FISCO:
                case PadraoNFSe.PRODEB:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNfseFaixaEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                            break;
                        case "ConsultarNotaPrestador":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNotaPrestador;
                            break;
                        case "ConsultarNfseServicoPrestadoEnvio":
                        case "ConsultarNfseServicoPrestado":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseServicoPrestado;
                            break;
                        case "ConsultarRpsServicoPrestadoEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRpsServicoPrestado;
                            break;
                    }
                    break;

                case PadraoNFSe.BHISS:
                    switch (doc.DocumentElement.Name)
                    {
                        case "ConsultarNfseFaixaEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfseFaixa;
                            break;
                        case "ConsultarNfseEnvio":
                            result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfse;
                            break;
                    }
                    break;

                case PadraoNFSe.CENTI:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarNfsePorRps;
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
                case PadraoNFSe.PRODATA:
                    versaoXML = Functions.GetAttributeXML("LoteRps", "versao", NomeArquivoXML);
                    if (string.IsNullOrWhiteSpace(versaoXML))
                    {
                        versaoXML = "2.01";
                    }

                    break;

                case PadraoNFSe.BETHA:
                case PadraoNFSe.BETHA_CLOUD:
                    versaoXML = "2.02";

                    if (xmlDoc.DocumentElement.Name.Contains("e:"))
                    {
                        versaoXML = "1.00";
                    }
                    break;

                case PadraoNFSe.NOBESISTEMAS:
                case PadraoNFSe.BHISS:
                case PadraoNFSe.EQUIPLANO:
                case PadraoNFSe.NACIONAL:
                case PadraoNFSe.MEMORY:
                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.BAUHAUS:
                case PadraoNFSe.TINUS:
                case PadraoNFSe.SIMPLE:
                case PadraoNFSe.PROPRIOBARUERISP:
                case PadraoNFSe.THEMA:
                case PadraoNFSe.GIF:
                case PadraoNFSe.WEBFISCO:
                case PadraoNFSe.AGILI:
                case PadraoNFSe.CARIOCA:
                case PadraoNFSe.SALVADOR_BA:
                case PadraoNFSe.MANAUS_AM:
                case PadraoNFSe.LIBRE:
                case PadraoNFSe.NATALENSE:
                case PadraoNFSe.HM2SOLUCOES:
                case PadraoNFSe.EGOVERNE:
                case PadraoNFSe.CECAM:
                case PadraoNFSe.METROPOLIS:
                case PadraoNFSe.ISSONLINE_ASSESSORPUBLICO:
                case PadraoNFSe.INTERSOL:
                case PadraoNFSe.LEXSOM:
                    versaoXML = "1.00";
                    break;


                case PadraoNFSe.IPM:
                    versaoXML = "2.04";

                    if (xmlDoc.InnerXml.Contains("<nfse"))
                    {
                        versaoXML = "1.20";

                        if (codMunicipio == 4309308 || codMunicipio == 4316006 || codMunicipio == 4314050 ||
                            codMunicipio == 4320206)
                        {
                            versaoXML = "1.00";
                        }
                    }
                    break;


                case PadraoNFSe.PAULISTANA:
                case PadraoNFSe.DIGIFRED:
                case PadraoNFSe.GIAP:
                case PadraoNFSe.CENTI:
                case PadraoNFSe.SOFTPLAN:
                    versaoXML = "2.00";
                    break;

                case PadraoNFSe.SONNER:
                case PadraoNFSe.QUASAR:
                case PadraoNFSe.FIORILLI:
                case PadraoNFSe.SYSTEMPRO:
                case PadraoNFSe.PRODEB:
                case PadraoNFSe.VITORIA_ES:
                case PadraoNFSe.SINSOFT:
                case PadraoNFSe.SUPERNOVA:
                    versaoXML = "2.01";
                    break;

                case PadraoNFSe.AVMB:
                case PadraoNFSe.WEBISS:
                case PadraoNFSe.VERSATEC:
                case PadraoNFSe.EMBRAS:
                case PadraoNFSe.E_RECEITA:
                case PadraoNFSe.SH3:
                case PadraoNFSe.MODERNIZACAO_PUBLICA:
                    versaoXML = "2.02";
                    break;

                case PadraoNFSe.FISCO:
                case PadraoNFSe.ELOTECH:
                case PadraoNFSe.INDAIATUBA_SP:
                    versaoXML = "2.03";
                    break;

                case PadraoNFSe.COPLAN:
                    versaoXML = "2.03";

                    if (codMunicipio == 3300407)
                    {
                        versaoXML = "2.02";
                    }
                    break;

                case PadraoNFSe.RLZ_INFORMATICA:
                    versaoXML = "2.03";

                    if (codMunicipio == 3557105)
                    {
                        versaoXML = "1.00";
                    }

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
                    break; ;

                case PadraoNFSe.DSF:
                    versaoXML = "2.03";

                    if (codMunicipio == 3170206)
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

                case PadraoNFSe.TRIBUTUS:
                case PadraoNFSe.ISSNET:
                case PadraoNFSe.GISSONLINE:
                    versaoXML = "2.04";
                    break;

                case PadraoNFSe.ABACO:
                    versaoXML = "2.04";

                    if (codMunicipio == 5108402)
                    {
                        versaoXML = "2.01";
                    }
                    break;

                case PadraoNFSe.GINFES:
                case PadraoNFSe.PUBLICA:
                    versaoXML = "3.00";

                    break;

                case PadraoNFSe.SIGISSWEB:
                case PadraoNFSe.SIGCORP:
                    versaoXML = "2.03";

                    if (xmlDoc.DocumentElement.Name.Contains("ConsultarNotaPrestador"))
                    {
                        versaoXML = "3.00";
                    }
                    else if (codMunicipio == 4113700)
                    {
                        versaoXML = "1.03";
                    }
                    else if (codMunicipio == 3131307 || codMunicipio == 3530805 || codMunicipio == 4204202 ||
                             codMunicipio == 3145208 || codMunicipio == 3300704)
                    {
                        versaoXML = "2.04";
                    }

                    break;
                case PadraoNFSe.SMARAPD:
                    versaoXML = "2.03";

                    if (codMunicipio == 3551702 || codMunicipio == 3202405)
                    {
                        versaoXML = "1.00";
                    }
                    else if (codMunicipio == 3205002 || codMunicipio == 3516200)
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

                    if (codMunicipio == 3304003)
                    {
                        versaoXML = "1.00";
                    }
                    if (codMunicipio == 2611606)
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

                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Consulta de NFSe.");

            }

            return versaoXML;
        }
    }
}