using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.BPe;

namespace NFe.Service.BPe
{
    public class TaskConsultaSituacaoBPe : TaskAbst
    {
        public TaskConsultaSituacaoBPe(string arquivo)
        {
            Servico = Servicos.BPeConsultaProtocolo;
            NomeArquivoXML = arquivo;
            if (vXmlNfeDadosMsgEhXML)
            {
                ConteudoXML.PreserveWhitespace = false;
                ConteudoXML.Load(arquivo);
            }
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                if (vXmlNfeDadosMsgEhXML)
                {
                    var xmlConsSitBPe = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ConsSitBPe>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                        PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                        TipoDFe = TipoDFe.BPe,
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

                    var consultaProtocolo = new Unimake.Business.DFe.Servicos.BPe.ConsultaProtocolo(xmlConsSitBPe, configuracao);
                    consultaProtocolo.Executar();

                    vStrXmlRetorno = consultaProtocolo.RetornoWSString;

                    LerRetornoSitBPe(xmlConsSitBPe, consultaProtocolo.Result, emp);

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
                }
            }
        }

        private void LerRetornoSitBPe(ConsSitBPe xmlConsSitBPe, RetConsSitBPe retornoConsSitBPe, int emp)
        {
            oGerarXML.XmlDistEventoBPe(emp, vStrXmlRetorno);

            var oLerXml = new LerXML();
            var oFluxoNFe = new FluxoNfe();

            var strChaveBPe = "BPe" + xmlConsSitBPe.ChBPe;
            var strNomeArqBPe = oFluxoNFe.LerTag(strChaveBPe, FluxoNfe.ElementoFixo.ArqNFe);

            if (string.IsNullOrEmpty(strNomeArqBPe))
            {
                strNomeArqBPe = strChaveBPe.Substring(3) + Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML;
            }

            var strArquivoBPe = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqBPe;

            var naoEhDaEmpresa = !Functions.ChaveDFePertenceEmpresa(xmlConsSitBPe.ChBPe, Empresas.Configuracoes[emp].CNPJ, Empresas.Configuracoes[emp].UnidadeFederativaCodigo);

            if (!File.Exists(strArquivoBPe))
            {
                if (naoEhDaEmpresa)
                {
                    return;
                }

                var arquivos = Directory.GetFiles(Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString(), "*-bpe.*");

                foreach (var arquivo in arquivos)
                {
                    var arqXML = new XmlDocument();
                    arqXML.Load(arquivo);

                    var chave = ((XmlElement)arqXML.GetElementsByTagName("infBPe")[0]).GetAttribute("Id").Substring(3);

                    if (chave.Equals(xmlConsSitBPe.ChBPe))
                    {
                        strNomeArqBPe = Path.GetFileName(arquivo);
                        strArquivoBPe = arquivo;
                        break;
                    }
                }
            }

            var cStatCons = retornoConsSitBPe.CStat;
            var xMotivo = retornoConsSitBPe.XMotivo ?? string.Empty;

            switch (cStatCons)
            {
                case 252:
                case 226:
                case 478:
                case 236:
                case 482:
                case 600:
                    break;

                case 216:
                case 217:
                    goto case 9999;

                case 100:
                case 150:
                    if (retornoConsSitBPe.ProtBPe != null && retornoConsSitBPe.ProtBPe.Count > 0 && retornoConsSitBPe.ProtBPe[0].InfProt != null)
                    {
                        var infProt = retornoConsSitBPe.ProtBPe[0].InfProt;

                        switch (infProt.CStat)
                        {
                            case 100:
                            case 150:
                                var docRetorno = new XmlDocument();
                                docRetorno.Load(Functions.StringXmlToStreamUTF8(vStrXmlRetorno));
                                var strProtBPe = docRetorno.GetElementsByTagName("protBPe")[0].OuterXml;

                                var strArquivoBPeProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString()
                                    + "\\" + Functions.ExtrairNomeArq(strArquivoBPe, Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML) + Propriedade.ExtRetorno.ProcBPe;

                                if (File.Exists(strArquivoBPe))
                                {
                                    var conteudoXML = new XmlDocument();

                                    try
                                    {
                                        var file = new FileInfo(strArquivoBPe);
                                        if (file.Length == 0)
                                        {
                                            throw new Exception();
                                        }

                                        conteudoXML.Load(strArquivoBPe);
                                        oLerXml.BPe(conteudoXML);
                                    }
                                    catch
                                    {
                                        goto default;
                                    }

                                    if (Empresas.Configuracoes[emp].CompararDigestValueDFeRetornadoSEFAZ)
                                    {
                                        var digestValueConsultaSituacaoBPe = infProt.DigVal;
                                        var digestValueNota = conteudoXML.GetElementsByTagName("DigestValue")[0].InnerText;

                                        if (!string.IsNullOrEmpty(digestValueConsultaSituacaoBPe) && !string.IsNullOrEmpty(digestValueNota))
                                        {
                                            if (!digestValueConsultaSituacaoBPe.Equals(digestValueNota))
                                            {
                                                oAux.MoveArqErro(strArquivoBPe);
                                                throw new Exception("O valor do DigestValue da consulta situação é diferente do DigestValue do BPe");
                                            }
                                        }
                                    }

                                    var procBPeJaNaAutorizada = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML, Propriedade.ExtRetorno.ProcBPe);

                                    if (!procBPeJaNaAutorizada && !File.Exists(strArquivoBPeProc))
                                    {
                                        Auxiliar.WriteLog("TaskConsultaSituacaoBPe: Gerou o arquivo de distribuição através da consulta situação do BPe.", false);
                                        oGerarXML.XmlDistBPe(strArquivoBPe, strProtBPe, Propriedade.ExtRetorno.ProcBPe, oLerXml.oDadosNfe.versao);
                                    }

                                    if (!(procBPeJaNaAutorizada = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML, Propriedade.ExtRetorno.ProcBPe)))
                                    {
                                        TFunctions.MoverArquivo(strArquivoBPeProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                        procBPeJaNaAutorizada = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML, Propriedade.ExtRetorno.ProcBPe);
                                    }

                                    if (!oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML))
                                    {
                                        if (procBPeJaNaAutorizada)
                                        {
                                            if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                            {
                                                TFunctions.MoverArquivo(strArquivoBPe, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                            }
                                            else
                                            {
                                                TFunctions.MoverArquivo(strArquivoBPe, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (procBPeJaNaAutorizada)
                                        {
                                            oAux.MoveArqErro(strArquivoBPe);
                                        }
                                    }
                                }

                                if (File.Exists(strArquivoBPeProc))
                                {
                                    Functions.DeletarArquivo(strArquivoBPeProc);
                                }

                                break;

                            default:
                                oAux.MoveArqErro(strArquivoBPe);
                                break;
                        }

                        oFluxoNFe.ExcluirNfeFluxo(strChaveBPe);
                        RemoverArqTemp(strChaveBPe, emp);
                    }

                    break;

                case 101:
                case 102:
                case 110:
                    goto case 100;

                case 9999:
                    oAux.MoveArqErro(strArquivoBPe);
                    oFluxoNFe.ExcluirNfeFluxo(strChaveBPe);
                    RemoverArqTemp(strChaveBPe, emp);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var enviaMensagemParaWhatsApp = new SendMessageToWhatsApp(emp);
                        enviaMensagemParaWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(cStatCons).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - BPe´s estão sendo rejeitados");
                    }

                    break;

                default:
                    goto case 9999;
            }
        }
    }
}
