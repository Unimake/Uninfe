using System;
using NFe.Components;
using NFe.Settings;
using Unimake.Business.DFe.Xml.DCe;
using Unimake.Business.DFe.Servicos;
using System.Xml;
using System.IO;

namespace NFe.Service.DCe
{
    public class TaskConsultaSituacaoDCe : TaskAbst
    {
        public TaskConsultaSituacaoDCe(string arquivo)
        {
            Servico = Servicos.DCeConsultaProtocolo;
            NomeArquivoXML = arquivo;
            if (vXmlNfeDadosMsgEhXML)
            {
                ConteudoXML.PreserveWhitespace = false;
                ConteudoXML.Load(arquivo);
            }
        }

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                if (vXmlNfeDadosMsgEhXML)
                {
                    var xmlConsSitDCe = new Unimake.Business.DFe.Xml.DCe.ConsSitDCe();
                    xmlConsSitDCe = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ConsSitDCe>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                        TipoDFe = TipoDFe.DCe,
                        TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                        CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                    };

                    if (ConfiguracaoApp.Proxy)
                    {
                        configuracao.HasProxy = true;
                        configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                        configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                        configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                    }

                    var consultaProtocolo = new Unimake.Business.DFe.Servicos.DCe.ConsultaProtocolo(xmlConsSitDCe, configuracao);
                    consultaProtocolo.Executar();

                    vStrXmlRetorno = consultaProtocolo.RetornoWSString;

                    LerRetornoSitDCe(xmlConsSitDCe, consultaProtocolo.Result, emp);

                    XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).RetornoXML);

                    consultaProtocolo.Dispose();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML, Propriedade.ExtRetorno.Sit_ERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 09/03/2010
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
                    //Se falhou algo na hora de deletar o XML de pedido da consulta da situação da NFe, infelizmente
                    //não posso fazser mais nada, o UniNFe vai tentar mantar o arquivo novamente para o webservice, pois ainda não foi excluido.
                    //Wandrey 22/03/2010
                }
            }
        }

        #endregion Execute

        #region LerRetornoSitDCe()

        private void LerRetornoSitDCe(ConsSitDCe xmlConsSitDCe, RetConsSitDCe retornoConsSitDCe, int emp)
        {
            oGerarXML.XmlDistEventoDCe(emp, vStrXmlRetorno);

            var oLerXml = new LerXML();
            var oFluxoNFe = new FluxoNfe();

            var strChaveDCe = "DCe" + xmlConsSitDCe.ChDCe;

            var strNomeArqDCe = oFluxoNFe.LerTag(strChaveDCe, FluxoNfe.ElementoFixo.ArqNFe);

            if (string.IsNullOrEmpty(strNomeArqDCe))
            {
                strNomeArqDCe = strChaveDCe.Substring(4) + Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML;
            }

            var strArquivoDCe = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqDCe;

            #region CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var naoEhDaEmpresa = !Functions.ChaveDFePertenceEmpresa(xmlConsSitDCe.ChDCe, Empresas.Configuracoes[emp].CNPJ, Empresas.Configuracoes[emp].UnidadeFederativaCodigo);

            if (!File.Exists(strArquivoDCe))
            {
                if (naoEhDaEmpresa)
                {
                    return;
                }

                var arquivos = Directory.GetFiles(Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString(), "*-DCe.*");

                foreach (var arquivo in arquivos)
                {
                    var arqXML = new XmlDocument();
                    arqXML.Load(arquivo);

                    var chave = ((XmlElement)arqXML.GetElementsByTagName("infDCe")[0]).GetAttribute("Id").Substring(3);

                    if (chave.Equals(xmlConsSitDCe.ChDCe))
                    {
                        strNomeArqDCe = Path.GetFileName(arquivo);
                        strArquivoDCe = arquivo;
                        break;
                    }
                }
            }

            #endregion CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var cStatCons = 0;
            var xMotivo = string.Empty;

            if (retornoConsSitDCe.CStat.ToString() != null)
            {
                cStatCons = retornoConsSitDCe.CStat;
            }

            if (retornoConsSitDCe.XMotivo != null)
            {
                xMotivo = retornoConsSitDCe.XMotivo;
            }

            switch (cStatCons)
            {
                #region Validação das regras de negócios da consulta a NF-e

                case 252:
                case 226:
                case 478:
                case 236:
                case 482:
                case 600:
                    break;

                #endregion Validação das regras de negócios da consulta a NF-e

                #region Nota fiscal rejeitada

                case 216:
                case 217:
                    goto case 9999; //Tirar a nota do fluxo

                #endregion Nota fiscal rejeitada

                #region Nota autorizada

                case 100: //Autorizado o uso da DCe
                case 150: //Autorizado o uso da DCe, autorização fora de prazo

                    if (retornoConsSitDCe.ProtDCe.InfProt != null)
                    {
                        var cStat = retornoConsSitDCe.ProtDCe.InfProt.CStat;

                        switch (cStat)
                        {
                            case 100: //Autorizado o uso da DCe
                            case 150: //Autorizado o uso da DCe, autorização fora de prazo
                                var strProtDCe = retornoConsSitDCe.ProtDCe.GerarXML().ToString();

                                //Definir o nome do arquivo -procDCe.xml
                                var strArquivoDCeProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString()
                                    + "\\" + Functions.ExtrairNomeArq(strArquivoDCe, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML) + Propriedade.ExtRetorno.ProcDCe;

                                //Se existir o strArquivoDCeProc, tem como eu fazer alguma coisa, se ele não existir
                                //Não tenho como fazer mais nada. Wandrey 08/10/2009
                                if (File.Exists(strArquivoDCe))
                                {
                                    var conteudoXML = new XmlDocument();

                                    try
                                    {
                                        var file = new FileInfo(strArquivoDCe);
                                        if (file.Length == 0)
                                        {
                                            throw new Exception();
                                        }
                                        else
                                        {
                                            conteudoXML.Load(strArquivoDCe);
                                            oLerXml.DCe(conteudoXML);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        goto default;
                                    }

                                    if (Empresas.Configuracoes[emp].CompararDigestValueDFeRetornadoSEFAZ)
                                    {
                                        var digestValueConsultaSituacaoDCe = retornoConsSitDCe.ProtDCe.InfProt.DigVal;
                                        var digestValueNota = conteudoXML.GetElementsByTagName("DigestValue")[0].InnerText;

                                        if (!string.IsNullOrEmpty(digestValueConsultaSituacaoDCe) && !string.IsNullOrEmpty(digestValueNota))
                                        {
                                            if (!digestValueConsultaSituacaoDCe.Equals(digestValueNota))
                                            {
                                                oAux.MoveArqErro(strArquivoDCe);
                                                throw new Exception("O valor do DigestValue da consulta situação é diferente do DigestValue da DCe");
                                            }
                                        }
                                    }

                                    //Verificar se o -procDCe.xml existe na pasta de autorizados
                                    var procDCeJaNaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.ExtRetorno.ProcDCe);

                                    if (!procDCeJaNaAutorizada)
                                    {
                                        if (!File.Exists(strArquivoDCeProc))
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoDCe: Gerou o arquivo de distribuição através da consulta situação da DCe.", false);
                                            oGerarXML.XmlDistDCe(strArquivoDCe, strProtDCe, Propriedade.ExtRetorno.ProcDCe, oLerXml.oDadosNfe.versao);
                                        }
                                    }

                                    //Se o XML de distribuição não estiver ainda na pasta de autorizados
                                    if (!(procDCeJaNaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.ExtRetorno.ProcDCe)))
                                    {
                                        //Move a DCeProc da pasta de DCe em processamento para a DCe Autorizada
                                        TFunctions.MoverArquivo(strArquivoDCeProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                        //Atualizar a situação para que eu só mova o arquivo com final -DCe.xml para a pasta autorizado se
                                        //a procDCe já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                        //Isso vai dar uma maior segurança para não deixar sem gerar o -procDCe.xml. Wandrey 13/12/2012
                                        procDCeJaNaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.ExtRetorno.ProcDCe);
                                    }

                                    if (!oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML))
                                    {
                                        //1-Mover a DCe da pasta de DCe em processamento para DCe Autorizada
                                        //2-Só vou mover o -DCe.xml para a pasta autorizados se já existir a -procDCe.xml, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procDCe.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procDCe.xml. Wandrey 13/12/2012
                                        if (procDCeJaNaAutorizada)
                                        {
                                            if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                            {
                                                TFunctions.MoverArquivo(strArquivoDCe, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                            }
                                            else
                                            {
                                                TFunctions.MoverArquivo(strArquivoDCe, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //1-Se já estiver na pasta de autorizados, vou somente mover ela da pasta de XML´s em processamento
                                        //2-Só vou mover o -DCe.xml da pasta EmProcessamento se também existir a -procDCe.xml na pasta autorizados, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procDCe.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procDCe.xml. Wandrey 13/12/2012
                                        if (procDCeJaNaAutorizada)
                                        {
                                            oAux.MoveArqErro(strArquivoDCe);
                                        }
                                    }

                                    //Disparar a geração/impressão do UniDanfe. 03/02/2010 - Wandrey
                                    if (procDCeJaNaAutorizada)
                                    {
                                        try
                                        {
                                            var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                                                        PastaEnviados.Autorizados.ToString() + "\\" +
                                                                        Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                                                        Path.GetFileName(strArquivoDCe);

                                            // TODO: Ajustar a chamada ao UniDANFE quando a DCe estiver implementada no software, tanto DCe quanto evento da DCe
                                            TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                        }
                                        catch (Exception ex)
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoDCe:  (Falha na execução do UniDANFe) " + ex.Message, false);
                                        }
                                    }
                                }

                                if (File.Exists(strArquivoDCeProc))
                                {
                                    //Se já estiver na pasta de autorizados, vou somente excluir ela da pasta de XML´s em processamento
                                    Functions.DeletarArquivo(strArquivoDCeProc);
                                }

                                break;

                            default:
                                //Mover o XML da DCe a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoDCe);
                                break;
                        }

                        //Deletar a DCe do arquivo de controle de fluxo
                        oFluxoNFe.ExcluirNfeFluxo(strChaveDCe);

                        RemoverArqTemp(strChaveDCe, emp);
                    }

                    break;

                #endregion Nota autorizada

                #region Nota cancelada/substituída/ajustada

                case 101: // Cancelamento de DCe homologado
                case 102: // Substituição da DCe homologado
                case 110: // Ajuste de DCe homologado
                    goto case 100;

                #endregion Nota cancelada/substituída/ajustada

                #region Conteúdo para retirar a nota fiscal do fluxo

                case 9999: //Tirar a nota do fluxo

                    //Mover o XML da DCe a pasta de XML´s com erro
                    oAux.MoveArqErro(strArquivoDCe);

                    //Deletar a DCe do arquivo de controle de fluxo
                    oFluxoNFe.ExcluirNfeFluxo(strChaveDCe);

                    RemoverArqTemp(strChaveDCe, emp);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var enviaMensagemParaWhatsApp = new SendMessageToWhatsApp(emp);
                        enviaMensagemParaWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(cStatCons).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - Notas estão sendo rejeitadas");
                    }

                    break;

                #endregion Conteúdo para retirar a nota fiscal do fluxo

                default:
                    goto case 9999;
            }
        }

        #endregion LerRetornoSitDCe()
    }
}
