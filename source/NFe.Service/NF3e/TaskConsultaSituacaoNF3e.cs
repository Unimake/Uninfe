using System;
using NFe.Components;
using NFe.Settings;
using Unimake.Business.DFe.Xml.NF3e;
using Unimake.Business.DFe.Servicos;
using System.Xml;
using System.IO;

namespace NFe.Service.NF3e
{
    public class TaskConsultaSituacaoNF3e : TaskAbst
    {
        public TaskConsultaSituacaoNF3e(string arquivo)
        {
            Servico = Servicos.NF3eConsultaProtocolo;
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
                    var xmlConsSitNF3e = new Unimake.Business.DFe.Xml.NF3e.ConsSitNF3e();
                    xmlConsSitNF3e = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ConsSitNF3e>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                        TipoDFe = TipoDFe.NF3e,
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

                    var consultaProtocolo = new Unimake.Business.DFe.Servicos.NF3e.ConsultaProtocolo(xmlConsSitNF3e, configuracao);
                    consultaProtocolo.Executar();

                    vStrXmlRetorno = consultaProtocolo.RetornoWSString;

                    LerRetornoSitNF3e(xmlConsSitNF3e, consultaProtocolo.Result, emp);

                    XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).RetornoXML);
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

        #region LerRetornoSitNF3e()

        private void LerRetornoSitNF3e(ConsSitNF3e xmlConsSitNF3e, RetConsSitNF3e retornoConsSitNF3e, int emp)
        {
            oGerarXML.XmlDistEventoNF3e(emp, vStrXmlRetorno);

            var oLerXml = new LerXML();
            var oFluxoNFe = new FluxoNfe();

            var strChaveNF3e = "NF3e" + xmlConsSitNF3e.ChNF3e;

            var strNomeArqNF3e = oFluxoNFe.LerTag(strChaveNF3e, FluxoNfe.ElementoFixo.ArqNFe);

            if (string.IsNullOrEmpty(strNomeArqNF3e))
            {
                strNomeArqNF3e = strChaveNF3e.Substring(4) + Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML;
            }

            var strArquivoNF3e = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNF3e;

            #region CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var naoEhDaEmpresa = (xmlConsSitNF3e.ChNF3e.Substring(6, 14) != Empresas.Configuracoes[emp].CNPJ ||
                xmlConsSitNF3e.ChNF3e.Substring(0, 2) != Empresas.Configuracoes[emp].UnidadeFederativaCodigo.ToString());

            if (!File.Exists(strArquivoNF3e))
            {
                if (naoEhDaEmpresa)
                {
                    return;
                }

                var arquivos = Directory.GetFiles(Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString(), "*-nf3e.*");

                foreach (var arquivo in arquivos)
                {
                    var arqXML = new XmlDocument();
                    arqXML.Load(arquivo);

                    var chave = ((XmlElement)arqXML.GetElementsByTagName("infNF3e")[0]).GetAttribute("Id").Substring(3);

                    if (chave.Equals(xmlConsSitNF3e.ChNF3e))
                    {
                        strNomeArqNF3e = Path.GetFileName(arquivo);
                        strArquivoNF3e = arquivo;
                        break;
                    }
                }
            }

            #endregion CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var cStatCons = 0;
            var xMotivo = string.Empty;

            if (retornoConsSitNF3e.CStat.ToString() != null)
            {
                cStatCons = retornoConsSitNF3e.CStat;
            }

            if (retornoConsSitNF3e.XMotivo != null)
            {
                xMotivo = retornoConsSitNF3e.XMotivo;
            }

            switch(cStatCons)
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

                case 100: //Autorizado o uso da NF3e
                case 150: //Autorizado o uso da NF3e, autorização fora de prazo

                    if (retornoConsSitNF3e.ProtNF3e.InfProt != null)
                    {
                        var cStat = retornoConsSitNF3e.ProtNF3e.InfProt.CStat;

                        switch(cStat)
                        {
                            case 100: //Autorizado o uso da NF3e
                            case 150: //Autorizado o uso da NF3e, autorização fora de prazo
                                var strProtNF3e = retornoConsSitNF3e.ProtNF3e.GerarXML().ToString();

                                //Definir o nome do arquivo -procNF3e.xml
                                var strArquivoNF3eProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString()
                                    + "\\" + Functions.ExtrairNomeArq(strArquivoNF3e, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML) + Propriedade.ExtRetorno.ProcNF3e;

                                //Se existir o strArquivoNF3eProc, tem como eu fazer alguma coisa, se ele não existir
                                //Não tenho como fazer mais nada. Wandrey 08/10/2009
                                if (File.Exists(strArquivoNF3e))
                                {
                                    var conteudoXML = new XmlDocument();

                                    try
                                    {
                                        var file = new FileInfo(strArquivoNF3e);
                                        if (file.Length == 0)
                                        {
                                            throw new Exception();
                                        }
                                        else
                                        {
                                            conteudoXML.Load(strArquivoNF3e);
                                            oLerXml.NF3e(conteudoXML);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        goto default;
                                    }

                                    if (Empresas.Configuracoes[emp].CompararDigestValueDFeRetornadoSEFAZ)
                                    {
                                        var digestValueConsultaSituacaoNF3e = retornoConsSitNF3e.ProtNF3e.InfProt.DigVal;
                                        var digestValueNota = conteudoXML.GetElementsByTagName("DigestValue")[0].InnerText;

                                        if (!string.IsNullOrEmpty(digestValueConsultaSituacaoNF3e) && !string.IsNullOrEmpty(digestValueNota))
                                        {
                                            if (!digestValueConsultaSituacaoNF3e.Equals(digestValueNota))
                                            {
                                                oAux.MoveArqErro(strArquivoNF3e);
                                                throw new Exception("O valor do DigestValue da consulta situação é diferente do DigestValue da NF3e");
                                            }
                                        }
                                    }

                                    //Verificar se a -nf3e.xml existe na pasta de autorizados
                                    var nf3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML);

                                    //Verificar se o -procNF3e.xml existe na pasta de autorizados
                                    var procNF3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.ExtRetorno.ProcNF3e);

                                    if (!procNF3eJaNaAutorizada)
                                    {
                                        if (!File.Exists(strArquivoNF3eProc))
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoNF3e: Gerou o arquivo de distribuição através da consulta situação da NF3e.", false);
                                            oGerarXML.XmlDistNF3e(strArquivoNF3e, strProtNF3e, Propriedade.ExtRetorno.ProcNF3e, oLerXml.oDadosNfe.versao);
                                        }
                                    }

                                    //Se o XML de distribuição não estiver ainda na pasta de autorizados
                                    if (!(procNF3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.ExtRetorno.ProcNF3e)))
                                    {
                                        //Move a nf3eProc da pasta de NF3E em processamento para a NF3e Autorizada
                                        TFunctions.MoverArquivo(strArquivoNF3eProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                        //Atualizar a situação para que eu só mova o arquivo com final -NF3e.xml para a pasta autorizado se
                                        //a procNF3e já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                        //Isso vai dar uma maior segurança para não deixar sem gerar o -procNF3e.xml. Wandrey 13/12/2012
                                        procNF3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.ExtRetorno.ProcNF3e);
                                    }

                                    //Se a NF3e não existir ainda na pasta de autorizados
                                    if (!(nf3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML)))
                                    {
                                        //1-Mover a NF3e da pasta de NF3e em processamento para NF3e Autorizada
                                        //2-Só vou mover o -nf3e.xml para a pasta autorizados se já existir a -procNF3e.xml, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procNF3e.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procNF3e.xml. Wandrey 13/12/2012
                                        if (procNF3eJaNaAutorizada)
                                        {
                                            if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                            {
                                                TFunctions.MoverArquivo(strArquivoNF3e, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                            }
                                            else
                                            {
                                                TFunctions.MoverArquivo(strArquivoNF3e, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //1-Se já estiver na pasta de autorizados, vou somente mover ela da pasta de XML´s em processamento
                                        //2-Só vou mover o -nf3e.xml da pasta EmProcessamento se também existir a -procNF3e.xml na pasta autorizados, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procNF3e.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procNF3e.xml. Wandrey 13/12/2012
                                        if (procNF3eJaNaAutorizada)
                                        {
                                            oAux.MoveArqErro(strArquivoNF3e);
                                        }
                                    }

                                    //Disparar a geração/impressão do UniDanfe. 03/02/2010 - Wandrey
                                    if (procNF3eJaNaAutorizada)
                                    {
                                        try
                                        {
                                            var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                                                        PastaEnviados.Autorizados.ToString() + "\\" +
                                                                        Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                                                        Path.GetFileName(strArquivoNF3e);

                                            // TODO: Ajustar a chamada ao UniDANFE quando a NF3e estiver implementada no software, tanto NF3e quanto evento da NF3e
                                            TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                        }
                                        catch (Exception ex)
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoNF3e:  (Falha na execução do UniDANFe) " + ex.Message, false);
                                        }
                                    }
                                }

                                if (File.Exists(strArquivoNF3eProc))
                                {
                                    //Se já estiver na pasta de autorizados, vou somente excluir ela da pasta de XML´s em processamento
                                    Functions.DeletarArquivo(strArquivoNF3eProc);
                                }

                                break;

                            default:
                                //Mover o XML da NF3e a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoNF3e);
                                break;
                        }

                        //Deletar a NF3E do arquivo de controle de fluxo
                        oFluxoNFe.ExcluirNfeFluxo(strChaveNF3e);
                        RemoverArqTemp(strChaveNF3e, emp);
                    }

                    break;

                #endregion Nota autorizada

                #region Nota cancelada/substituída/ajustada

                case 101: // Cancelamento de NF3e homologado
                case 102: // Substituição da NF3e homologado
                case 110: // Ajuste de NF3e homologado
                    goto case 100;

                #endregion Nota cancelada/substituída/ajustada

                #region Conteúdo para retirar a nota fiscal do fluxo

                case 9999: //Tirar a nota do fluxo

                    //Mover o XML da NF3e a pasta de XML´s com erro
                    oAux.MoveArqErro(strArquivoNF3e);

                    //Deletar a NF3e do arquivo de controle de fluxo
                    oFluxoNFe.ExcluirNfeFluxo(strChaveNF3e);

                    RemoverArqTemp(strChaveNF3e, emp);

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

        #endregion LerRetornoSitNF3e()
    }
}
