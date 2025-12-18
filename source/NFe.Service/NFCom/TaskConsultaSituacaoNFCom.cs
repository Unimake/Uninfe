using System;
using NFe.Components;
using NFe.Settings;
using Unimake.Business.DFe.Xml.NFCom;
using Unimake.Business.DFe.Servicos;
using System.Xml;
using System.IO;

namespace NFe.Service.NFCom
{
    public class TaskConsultaSituacaoNFCom : TaskAbst
    {
        public TaskConsultaSituacaoNFCom(string arquivo)
        {
            Servico = Servicos.NFComConsultaProtocolo;
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
                    var xmlConsSitNFCom = new Unimake.Business.DFe.Xml.NFCom.ConsSitNFCom();
                    xmlConsSitNFCom = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ConsSitNFCom>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                        TipoDFe = TipoDFe.NFCom,
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

                    var consultaProtocolo = new Unimake.Business.DFe.Servicos.NFCom.ConsultaProtocolo(xmlConsSitNFCom, configuracao);
                    consultaProtocolo.Executar();

                    vStrXmlRetorno = consultaProtocolo.RetornoWSString;

                    LerRetornoSitNFCom(xmlConsSitNFCom, consultaProtocolo.Result, emp);

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

        #region LerRetornoSitNFCom()

        private void LerRetornoSitNFCom(ConsSitNFCom xmlConsSitNFCom, RetConsSitNFCom retornoConsSitNFCom, int emp)
        {
            oGerarXML.XmlDistEventoNFCom(emp, vStrXmlRetorno);

            var oLerXml = new LerXML();
            var oFluxoNFe = new FluxoNfe();

            var strChaveNFCom = "NFCom" + xmlConsSitNFCom.ChNFCom;

            var strNomeArqNFCom = oFluxoNFe.LerTag(strChaveNFCom, FluxoNfe.ElementoFixo.ArqNFe);

            if (string.IsNullOrEmpty(strNomeArqNFCom))
            {
                strNomeArqNFCom = strChaveNFCom.Substring(4) + Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML;
            }

            var strArquivoNFCom = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNFCom;

            #region CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var naoEhDaEmpresa = (xmlConsSitNFCom.ChNFCom.Substring(6, 14) != Empresas.Configuracoes[emp].CNPJ ||
                xmlConsSitNFCom.ChNFCom.Substring(0, 2) != Empresas.Configuracoes[emp].UnidadeFederativaCodigo.ToString());

            if (!File.Exists(strArquivoNFCom))
            {
                if (naoEhDaEmpresa)
                {
                    return;
                }

                var arquivos = Directory.GetFiles(Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString(), "*-NFCom.*");

                foreach (var arquivo in arquivos)
                {
                    var arqXML = new XmlDocument();
                    arqXML.Load(arquivo);

                    var chave = ((XmlElement)arqXML.GetElementsByTagName("infNFCom")[0]).GetAttribute("Id").Substring(3);

                    if (chave.Equals(xmlConsSitNFCom.ChNFCom))
                    {
                        strNomeArqNFCom = Path.GetFileName(arquivo);
                        strArquivoNFCom = arquivo;
                        break;
                    }
                }
            }

            #endregion CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var cStatCons = 0;
            var xMotivo = string.Empty;

            if (retornoConsSitNFCom.CStat.ToString() != null)
            {
                cStatCons = retornoConsSitNFCom.CStat;
            }

            if (retornoConsSitNFCom.XMotivo != null)
            {
                xMotivo = retornoConsSitNFCom.XMotivo;
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

                case 100: //Autorizado o uso da NFCom
                case 150: //Autorizado o uso da NFCom, autorização fora de prazo

                    if (retornoConsSitNFCom.ProtNFCom.InfProt != null)
                    {
                        var cStat = retornoConsSitNFCom.ProtNFCom.InfProt.CStat;

                        switch (cStat)
                        {
                            case 100: //Autorizado o uso da NFCom
                            case 150: //Autorizado o uso da NFCom, autorização fora de prazo
                                var strProtNFCom = retornoConsSitNFCom.ProtNFCom.GerarXML().ToString();

                                //Definir o nome do arquivo -procNFCom.xml
                                var strArquivoNFComProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString()
                                    + "\\" + Functions.ExtrairNomeArq(strArquivoNFCom, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML) + Propriedade.ExtRetorno.ProcNFCom;

                                //Se existir o strArquivoNFComProc, tem como eu fazer alguma coisa, se ele não existir
                                //Não tenho como fazer mais nada. Wandrey 08/10/2009
                                if (File.Exists(strArquivoNFCom))
                                {
                                    var conteudoXML = new XmlDocument();

                                    try
                                    {
                                        var file = new FileInfo(strArquivoNFCom);
                                        if (file.Length == 0)
                                        {
                                            throw new Exception();
                                        }
                                        else
                                        {
                                            conteudoXML.Load(strArquivoNFCom);
                                            oLerXml.NFCom(conteudoXML);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        goto default;
                                    }

                                    if (Empresas.Configuracoes[emp].CompararDigestValueDFeRetornadoSEFAZ)
                                    {
                                        var digestValueConsultaSituacaoNFCom = retornoConsSitNFCom.ProtNFCom.InfProt.DigVal;
                                        var digestValueNota = conteudoXML.GetElementsByTagName("DigestValue")[0].InnerText;

                                        if (!string.IsNullOrEmpty(digestValueConsultaSituacaoNFCom) && !string.IsNullOrEmpty(digestValueNota))
                                        {
                                            if (!digestValueConsultaSituacaoNFCom.Equals(digestValueNota))
                                            {
                                                oAux.MoveArqErro(strArquivoNFCom);
                                                throw new Exception("O valor do DigestValue da consulta situação é diferente do DigestValue da NFCom");
                                            }
                                        }
                                    }

                                    //Verificar se o -procNFCom.xml existe na pasta de autorizados
                                    var procNFComJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.ExtRetorno.ProcNFCom);

                                    if (!procNFComJaNaAutorizada)
                                    {
                                        if (!File.Exists(strArquivoNFComProc))
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoNFCom: Gerou o arquivo de distribuição através da consulta situação da NFCom.", false);
                                            oGerarXML.XmlDistNFCom(strArquivoNFCom, strProtNFCom, Propriedade.ExtRetorno.ProcNFCom, oLerXml.oDadosNfe.versao);
                                        }
                                    }

                                    //Se o XML de distribuição não estiver ainda na pasta de autorizados
                                    if (!(procNFComJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.ExtRetorno.ProcNFCom)))
                                    {
                                        //Move a NFComProc da pasta de NFCom em processamento para a NFCom Autorizada
                                        TFunctions.MoverArquivo(strArquivoNFComProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                        //Atualizar a situação para que eu só mova o arquivo com final -NFCom.xml para a pasta autorizado se
                                        //a procNFCom já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                        //Isso vai dar uma maior segurança para não deixar sem gerar o -procNFCom.xml. Wandrey 13/12/2012
                                        procNFComJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.ExtRetorno.ProcNFCom);
                                    }

                                    if (! oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML))
                                    {
                                        //1-Mover a NFCom da pasta de NFCom em processamento para NFCom Autorizada
                                        //2-Só vou mover o -NFCom.xml para a pasta autorizados se já existir a -procNFCom.xml, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procNFCom.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procNFCom.xml. Wandrey 13/12/2012
                                        if (procNFComJaNaAutorizada)
                                        {
                                            if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                            {
                                                TFunctions.MoverArquivo(strArquivoNFCom, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                            }
                                            else
                                            {
                                                TFunctions.MoverArquivo(strArquivoNFCom, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //1-Se já estiver na pasta de autorizados, vou somente mover ela da pasta de XML´s em processamento
                                        //2-Só vou mover o -NFCom.xml da pasta EmProcessamento se também existir a -procNFCom.xml na pasta autorizados, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procNFCom.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procNFCom.xml. Wandrey 13/12/2012
                                        if (procNFComJaNaAutorizada)
                                        {
                                            oAux.MoveArqErro(strArquivoNFCom);
                                        }
                                    }

                                    //Disparar a geração/impressão do UniDanfe. 03/02/2010 - Wandrey
                                    if (procNFComJaNaAutorizada)
                                    {
                                        try
                                        {
                                            var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                                                        PastaEnviados.Autorizados.ToString() + "\\" +
                                                                        Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                                                        Path.GetFileName(strArquivoNFCom);

                                            // TODO: Ajustar a chamada ao UniDANFE quando a NFCom estiver implementada no software, tanto NFCom quanto evento da NFCom
                                            TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                        }
                                        catch (Exception ex)
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoNFCom:  (Falha na execução do UniDANFe) " + ex.Message, false);
                                        }
                                    }
                                }

                                if (File.Exists(strArquivoNFComProc))
                                {
                                    //Se já estiver na pasta de autorizados, vou somente excluir ela da pasta de XML´s em processamento
                                    Functions.DeletarArquivo(strArquivoNFComProc);
                                }

                                break;

                            default:
                                //Mover o XML da NFCom a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoNFCom);
                                break;
                        }

                        //Deletar a NFCom do arquivo de controle de fluxo
                        oFluxoNFe.ExcluirNfeFluxo(strChaveNFCom);

                        RemoverArqTemp(strChaveNFCom, emp);
                    }

                    break;

                #endregion Nota autorizada

                #region Nota cancelada/substituída/ajustada

                case 101: // Cancelamento de NFCom homologado
                case 102: // Substituição da NFCom homologado
                case 110: // Ajuste de NFCom homologado
                    goto case 100;

                #endregion Nota cancelada/substituída/ajustada

                #region Conteúdo para retirar a nota fiscal do fluxo

                case 9999: //Tirar a nota do fluxo

                    //Mover o XML da NFCom a pasta de XML´s com erro
                    oAux.MoveArqErro(strArquivoNFCom);

                    //Deletar a NFCom do arquivo de controle de fluxo
                    oFluxoNFe.ExcluirNfeFluxo(strChaveNFCom);

                    RemoverArqTemp(strChaveNFCom, emp);

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

        #endregion LerRetornoSitNFCom()
    }
}
