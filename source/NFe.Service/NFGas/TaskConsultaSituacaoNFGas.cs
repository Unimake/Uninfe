using System;
using NFe.Components;
using NFe.Settings;
using Unimake.Business.DFe.Xml.NFGas;
using Unimake.Business.DFe.Servicos;
using System.Xml;
using System.IO;

namespace NFe.Service.NFGas
{
    public class TaskConsultaSituacaoNFGas : TaskAbst
    {
        public TaskConsultaSituacaoNFGas(string arquivo)
        {
            Servico = Servicos.NFGasConsultaProtocolo;
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
                    var xmlConsSitNFGas = new Unimake.Business.DFe.Xml.NFGas.ConsSitNFGas();
                    xmlConsSitNFGas = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ConsSitNFGas>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                        TipoDFe = TipoDFe.NFGas,
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

                    var consultaProtocolo = new Unimake.Business.DFe.Servicos.NFGas.ConsultaProtocolo(xmlConsSitNFGas, configuracao);
                    consultaProtocolo.Executar();

                    vStrXmlRetorno = consultaProtocolo.RetornoWSString;

                    LerRetornoSitNFGas(xmlConsSitNFGas, consultaProtocolo.Result, emp);

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

        #region LerRetornoSitNFGas()

        private void LerRetornoSitNFGas(ConsSitNFGas xmlConsSitNFGas, RetConsSitNFGas retornoConsSitNFGas, int emp)
        {
            oGerarXML.XmlDistEventoNFGas(emp, vStrXmlRetorno);

            var oLerXml = new LerXML();
            var oFluxoNFe = new FluxoNfe();

            var strChaveNFGas = "NFGas" + xmlConsSitNFGas.ChNFGas;

            var strNomeArqNFGas = oFluxoNFe.LerTag(strChaveNFGas, FluxoNfe.ElementoFixo.ArqNFe);

            if (string.IsNullOrEmpty(strNomeArqNFGas))
            {
                strNomeArqNFGas = strChaveNFGas.Substring(5) + Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML;
            }

            var strArquivoNFGas = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNFGas;

            #region CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var naoEhDaEmpresa = (xmlConsSitNFGas.ChNFGas.Substring(6, 14) != Empresas.Configuracoes[emp].CNPJ ||
                xmlConsSitNFGas.ChNFGas.Substring(0, 2) != Empresas.Configuracoes[emp].UnidadeFederativaCodigo.ToString());

            if (!File.Exists(strArquivoNFGas))
            {
                if (naoEhDaEmpresa)
                {
                    return;
                }

                var arquivos = Directory.GetFiles(Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString(), "*-NFGas.*");

                foreach (var arquivo in arquivos)
                {
                    var arqXML = new XmlDocument();
                    arqXML.Load(arquivo);

                    var chave = ((XmlElement)arqXML.GetElementsByTagName("infNFGas")[0]).GetAttribute("Id").Substring(5);

                    if (chave.Equals(xmlConsSitNFGas.ChNFGas))
                    {
                        strNomeArqNFGas = Path.GetFileName(arquivo);
                        strArquivoNFGas = arquivo;
                        break;
                    }
                }
            }

            #endregion CNPJ da chave não é de uma empresa cadastrada no UniNFe

            var cStatCons = 0;
            var xMotivo = string.Empty;

            if (retornoConsSitNFGas.CStat.ToString() != null)
            {
                cStatCons = retornoConsSitNFGas.CStat;
            }

            if (retornoConsSitNFGas.XMotivo != null)
            {
                xMotivo = retornoConsSitNFGas.XMotivo;
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

                case 100: //Autorizado o uso da NFGas
                case 150: //Autorizado o uso da NFGas, autorização fora de prazo

                    if (retornoConsSitNFGas.ProtNFGas.InfProt != null)
                    {
                        var cStat = retornoConsSitNFGas.ProtNFGas.InfProt.CStat;

                        switch (cStat)
                        {
                            case 100: //Autorizado o uso da NFGas
                            case 150: //Autorizado o uso da NFGas, autorização fora de prazo
                                var docRetorno = new XmlDocument();
                                docRetorno.Load(Functions.StringXmlToStreamUTF8(vStrXmlRetorno));
                                var strProtNFGas = docRetorno.GetElementsByTagName("protNFGas")[0].OuterXml;

                                //Definir o nome do arquivo -procNFGas.xml
                                var strArquivoNFGasProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString()
                                    + "\\" + Functions.ExtrairNomeArq(strArquivoNFGas, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML) + Propriedade.ExtRetorno.ProcNFGas;

                                //Se existir o strArquivoNFGasProc, tem como eu fazer alguma coisa, se ele não existir
                                //Não tenho como fazer mais nada. Wandrey 08/10/2009
                                if (File.Exists(strArquivoNFGas))
                                {
                                    var conteudoXML = new XmlDocument();

                                    try
                                    {
                                        var file = new FileInfo(strArquivoNFGas);
                                        if (file.Length == 0)
                                        {
                                            throw new Exception();
                                        }
                                        else
                                        {
                                            conteudoXML.Load(strArquivoNFGas);
                                            oLerXml.NFGas(conteudoXML);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        goto default;
                                    }

                                    if (Empresas.Configuracoes[emp].CompararDigestValueDFeRetornadoSEFAZ)
                                    {
                                        var digestValueConsultaSituacaoNFGas = retornoConsSitNFGas.ProtNFGas.InfProt.DigVal;
                                        var digestValueNota = conteudoXML.GetElementsByTagName("DigestValue")[0].InnerText;

                                        if (!string.IsNullOrEmpty(digestValueConsultaSituacaoNFGas) && !string.IsNullOrEmpty(digestValueNota))
                                        {
                                            if (!digestValueConsultaSituacaoNFGas.Equals(digestValueNota))
                                            {
                                                oAux.MoveArqErro(strArquivoNFGas);
                                                throw new Exception("O valor do DigestValue da consulta situação é diferente do DigestValue da NFGas");
                                            }
                                        }
                                    }

                                    //Verificar se o -procNFGas.xml existe na pasta de autorizados
                                    var procNFGasJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.ExtRetorno.ProcNFGas);

                                    if (!procNFGasJaNaAutorizada)
                                    {
                                        if (!File.Exists(strArquivoNFGasProc))
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoNFGas: Gerou o arquivo de distribuição através da consulta situação da NFGas.", false);
                                            oGerarXML.XmlDistNFGas(strArquivoNFGas, strProtNFGas, Propriedade.ExtRetorno.ProcNFGas, oLerXml.oDadosNfe.versao);
                                        }
                                    }

                                    //Se o XML de distribuição não estiver ainda na pasta de autorizados
                                    if (!(procNFGasJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.ExtRetorno.ProcNFGas)))
                                    {
                                        //Move a NFGasProc da pasta de NFGas em processamento para a NFGas Autorizada
                                        TFunctions.MoverArquivo(strArquivoNFGasProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                        //Atualizar a situação para que eu só mova o arquivo com final -NFGas.xml para a pasta autorizado se
                                        //a procNFGas já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                        //Isso vai dar uma maior segurança para não deixar sem gerar o -procNFGas.xml. Wandrey 13/12/2012
                                        procNFGasJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.ExtRetorno.ProcNFGas);
                                    }

                                    if (! oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML))
                                    {
                                        //1-Mover a NFGas da pasta de NFGas em processamento para NFGas Autorizada
                                        //2-Só vou mover o -NFGas.xml para a pasta autorizados se já existir a -procNFGas.xml, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procNFGas.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procNFGas.xml. Wandrey 13/12/2012
                                        if (procNFGasJaNaAutorizada)
                                        {
                                            if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                            {
                                                TFunctions.MoverArquivo(strArquivoNFGas, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                            }
                                            else
                                            {
                                                TFunctions.MoverArquivo(strArquivoNFGas, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //1-Se já estiver na pasta de autorizados, vou somente mover ela da pasta de XML´s em processamento
                                        //2-Só vou mover o -NFGas.xml da pasta EmProcessamento se também existir a -procNFGas.xml na pasta autorizados, caso contrário vou manter na pasta EmProcessamento
                                        //  para tentar gerar novamente o -procNFGas.xml
                                        //  Isso vai dar uma maior segurança para não deixar sem gerar o -procNFGas.xml. Wandrey 13/12/2012
                                        if (procNFGasJaNaAutorizada)
                                        {
                                            oAux.MoveArqErro(strArquivoNFGas);
                                        }
                                    }

                                    //Disparar a geração/impressão do UniDanfe. 03/02/2010 - Wandrey
                                    if (procNFGasJaNaAutorizada)
                                    {
                                        try
                                        {
                                            var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                                                        PastaEnviados.Autorizados.ToString() + "\\" +
                                                                        Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                                                        Path.GetFileName(strArquivoNFGas);

                                            // TODO: Ajustar a chamada ao UniDANFE quando a NFGas estiver implementada no software, tanto NFGas quanto evento da NFGas
                                            TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                        }
                                        catch (Exception ex)
                                        {
                                            Auxiliar.WriteLog("TaskConsultaSituacaoNFGas:  (Falha na execução do UniDANFe) " + ex.Message, false);
                                        }
                                    }
                                }

                                if (File.Exists(strArquivoNFGasProc))
                                {
                                    //Se já estiver na pasta de autorizados, vou somente excluir ela da pasta de XML´s em processamento
                                    Functions.DeletarArquivo(strArquivoNFGasProc);
                                }

                                break;

                            default:
                                //Mover o XML da NFGas a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoNFGas);
                                break;
                        }

                        //Deletar a NFGas do arquivo de controle de fluxo
                        oFluxoNFe.ExcluirNfeFluxo(strChaveNFGas);

                        RemoverArqTemp(strChaveNFGas, emp);
                    }

                    break;

                #endregion Nota autorizada

                #region Nota cancelada/substituída/ajustada

                case 101: // Cancelamento de NFGas homologado
                case 102: // Substituição da NFGas homologado
                case 110: // Ajuste de NFGas homologado
                    goto case 100;

                #endregion Nota cancelada/substituída/ajustada

                #region Conteúdo para retirar a nota fiscal do fluxo

                case 9999: //Tirar a nota do fluxo

                    //Mover o XML da NFGas a pasta de XML´s com erro
                    oAux.MoveArqErro(strArquivoNFGas);

                    //Deletar a NFGas do arquivo de controle de fluxo
                    oFluxoNFe.ExcluirNfeFluxo(strChaveNFGas);

                    RemoverArqTemp(strChaveNFGas, emp);

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

        #endregion LerRetornoSitNFGas()
    }
}
