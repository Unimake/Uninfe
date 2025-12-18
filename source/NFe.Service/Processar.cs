using NFe.Components;
using NFe.Components.Exceptions;
using NFe.Components.Info;
using NFe.ConvertTxt;
using NFe.Exceptions;
using NFe.SAT;
using NFe.Service.CCG;
using NFe.Service.DARE;
using NFe.Service.EFDReinf;
using NFe.Service.GNRE;
using NFe.Service.NF3e;
using NFe.Service.NFCom;
using NFe.Settings;
using NFe.Validate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Unimake.Business.DFe.Security;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class Processar
    {
        #region ProcessaArquivo()

        public void ProcessaArquivo(int emp, string arquivo)
        {
            try
            {
                var servico = Servicos.Nulo;
                try
                {
                    #region Carregar PIN A3 se ainda não carregou

                    //Não pode carregar o PIN se o arquivo processado foi colocado na pasta GERAL, ou gera erro e para alguns serviços de funcionar, por exemplo a consulta informações do uninfe via pasta geral. 14/08/2021
                    if (Path.GetDirectoryName(arquivo).ToLower() != Propriedade.PastaGeralTemporaria.ToLower())
                    {
                        CarregarPINA3(emp);
                    }

                    #endregion

                    if (emp == -1)
                    {
                        ValidarExtensao(arquivo);
                    }

                    // Só vou validar a extensão em homologação, pois depois que o desenvolvedor fez toda a integração, acredito que ele não vá mais gerar extensões erradas, com isso evito ficar validando todas as vezes arquivos corretos. Wandrey 17/09/2016
                    else if (Empresas.Configuracoes[emp].AmbienteCodigo == (int)TipoAmbiente.Homologacao)
                    {
                        ValidarExtensao(arquivo);
                    }

                    try
                    {
                        servico = DefinirTipoServico(emp, arquivo);

                        if (servico == Servicos.Nulo)
                        {
                            throw new Exception("Não pode identificar o tipo de serviço baseado no arquivo " + arquivo);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Não pode identificar o tipo de serviço baseado no arquivo " + arquivo + " - Motivo: " + ex.GetAllMessages());
                    }

                    switch (servico)
                    {
                        #region NFS-e

                        case Servicos.NFSeCancelar:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeCancelar());
                            break;

                        case Servicos.NFSeConsultar:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultar());
                            break;

                        case Servicos.NFSeConsultarLoteRps:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultarLoteRps());
                            break;

                        case Servicos.NFSeConsultarPorRps:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultarPorRps());
                            break;

                        case Servicos.NFSeConsultarSituacaoLoteRps:
                        case Servicos.NFSeConsultarSituacaoCancelamento:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultaSituacaoLoteRps());
                            break;

                        case Servicos.NFSeConsultarURL:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultarURL());
                            break;

                        case Servicos.NFSeRecepcionarLoteRps:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeRecepcionarLoteRps(arquivo));
                            break;

                        case Servicos.NFSeInutilizarNFSe:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskInutilizarNfse());
                            break;

                        case Servicos.NFSeConsultarNFSePDF:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskConsultarNfsePDF());
                            break;

                        case Servicos.NFSeObterNotaFiscal:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskObterNotaFiscal());
                            break;

                        case Servicos.NFSeConsultaSequenciaLoteNotaRPS:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskConsultaSequenciaLoteNotaRPS(arquivo));
                            break;

                        case Servicos.NFSeSubstituirNfse:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskSubstituirNfse(arquivo));
                            break;

                        case Servicos.NFSeConsultarNFSeRecebidas:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskConsultarNfseRecebidas(arquivo));
                            break;

                        case Servicos.NFSeConsultarNFSeEmitidas:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskConsultarNfseEmitidas(arquivo));
                            break;

                        case Servicos.NFSeConsultarNFSeTomados:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskConsultarNfseTomados(arquivo));
                            break;

                        case Servicos.NFSeConsultarConvenioMunicipal:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultarConvenioMunicipal());
                            break;

                        case Servicos.NFSeConsultarAliquotasMunicipais:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultarAliquotaMunicipal());
                            break;

                        case Servicos.NFSeConsultarHistoricoAliquotasMunicipais:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultarHistoricoAliquotaMunicipal());
                            break;

                        case Servicos.NFSeConsultarRegimesEspeciaisMunicipais:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskConsultarRegimesEspeciais());
                            break;

                        case Servicos.NFSeConsultarRetencoesMunicipais:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskConsultarRetencoesMunicipais());
                            break;

                        case Servicos.NFSeConsultarBeneficioMunicipal:
                            DirecionarArquivo(emp, true, true, arquivo, new NFSe.TaskNFSeConsultarBeneficiosMunicipais());
                            break;

                        #endregion NFS-e

                        #region SAT/CF-e

                        case Servicos.SATConsultar:
                            var consulta = new SATProxy(Servicos.SATConsultar, Empresas.Configuracoes[emp], arquivo);
                            consulta.Enviar();
                            consulta.SaveResponse();
                            break;

                        case Servicos.SATExtrairLogs:
                            var extrairLog = new SATProxy(Servicos.SATExtrairLogs, Empresas.Configuracoes[emp], arquivo);
                            extrairLog.Enviar();
                            extrairLog.SaveResponse();
                            break;

                        case Servicos.SATConsultarStatusOperacional:
                            var consultaOp = new SATProxy(Servicos.SATConsultarStatusOperacional, Empresas.Configuracoes[emp], arquivo);
                            consultaOp.Enviar();
                            consultaOp.SaveResponse();
                            break;

                        case Servicos.SATTesteFimAFim:
                            var testeFim = new SATProxy(Servicos.SATTesteFimAFim, Empresas.Configuracoes[emp], arquivo);
                            testeFim.Enviar();
                            testeFim.SaveResponse();
                            break;

                        case Servicos.SATTrocarCodigoDeAtivacao:
                            var trocaCodigo = new SATProxy(Servicos.SATTrocarCodigoDeAtivacao, Empresas.Configuracoes[emp], arquivo);
                            trocaCodigo.Enviar();
                            trocaCodigo.SaveResponse();
                            break;

                        case Servicos.SATEnviarDadosVenda:
                            var enviaVenda = new SATProxy(Servicos.SATEnviarDadosVenda, Empresas.Configuracoes[emp], arquivo);
                            enviaVenda.Enviar();
                            var xmlVenda = enviaVenda.GerarXmlSATAutorizado();

                            if (!string.IsNullOrEmpty(xmlVenda))
                            {
                                var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                    PastaEnviados.Autorizados.ToString() + "\\" +
                                    Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(DateTime.Now) +
                                    Path.GetFileName(xmlVenda);

                                TFunctions.MoverArquivo(xmlVenda, PastaEnviados.Autorizados);
                                new GerarXML(emp).XmlParaFTP(emp, strArquivoDist); //Mover para FTP

                                if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].PastaExeUniDanfe))
                                {
                                    TFunctions.ExecutaUniDanfe(strArquivoDist, DateTime.Now, Empresas.Configuracoes[emp]);
                                }
                            }

                            enviaVenda.SaveRetorno();
                            break;

                        case Servicos.SATConverterNFCe:
                            var converte = new SATProxy(Servicos.SATConverterNFCe, Empresas.Configuracoes[emp], arquivo);
                            converte.Enviar();
                            converte.SaveResponse();
                            break;

                        case Servicos.SATCancelarUltimaVenda:
                            var cancela = new SATProxy(Servicos.SATCancelarUltimaVenda, Empresas.Configuracoes[emp], arquivo);
                            cancela.Enviar();
                            var xmlCancelamento = cancela.SaveResponse();

                            if (!string.IsNullOrEmpty(xmlCancelamento))
                            {
                                var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                    PastaEnviados.Autorizados.ToString() + "\\" +
                                    Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(DateTime.Now) +
                                    Path.GetFileName(xmlCancelamento);

                                TFunctions.MoverArquivo(xmlCancelamento, PastaEnviados.Autorizados);
                                TFunctions.ExecutaUniDanfe(strArquivoDist, DateTime.Today, Empresas.Configuracoes[emp]);
                            }
                            break;

                        case Servicos.SATConfigurarInterfaceDeRede:
                            var configuraRede = new SATProxy(Servicos.SATConfigurarInterfaceDeRede, Empresas.Configuracoes[emp], arquivo);
                            configuraRede.Enviar();
                            configuraRede.SaveResponse();
                            break;

                        case Servicos.SATAssociarAssinatura:
                            var associar = new SATProxy(Servicos.SATAssociarAssinatura, Empresas.Configuracoes[emp], arquivo);
                            associar.Enviar();
                            associar.SaveResponse();
                            break;

                        case Servicos.SATAtivar:
                            var ativa = new SATProxy(Servicos.SATAtivar, Empresas.Configuracoes[emp], arquivo);
                            ativa.Enviar();
                            ativa.SaveResponse();
                            break;

                        case Servicos.SATBloquear:
                            var bloquear = new SATProxy(Servicos.SATBloquear, Empresas.Configuracoes[emp], arquivo);
                            bloquear.Enviar();
                            bloquear.SaveResponse();
                            break;

                        case Servicos.SATDesbloquear:
                            var desbloquear = new SATProxy(Servicos.SATDesbloquear, Empresas.Configuracoes[emp], arquivo);
                            desbloquear.Enviar();
                            desbloquear.SaveResponse();
                            break;

                        case Servicos.SATConsultarNumeroSessao:
                            var consultaSessao = new SATProxy(Servicos.SATConsultarNumeroSessao, Empresas.Configuracoes[emp], arquivo);
                            consultaSessao.Enviar();
                            consultaSessao.SaveResponse();
                            break;

                        #endregion SAT/CF-e

                        #region NFe

                        case Servicos.NFeAssinarValidarEnvioEmLote:
                            CertVencido(emp);
                            AssinarValidarNFe(arquivo);
                            break;

                        case Servicos.ConsultaCadastroContribuinte:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCadastroContribuinte(arquivo));
                            break;

                        case Servicos.EventoRecepcao:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskNFeEventos(arquivo));
                            break;

                        case Servicos.NFeConsultaStatusServico:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskNFeConsultaStatus(arquivo));
                            break;

                        case Servicos.NFeConverterTXTparaXML:
                            ConverterTXTparaXML(arquivo);
                            break;

                        case Servicos.NFeEnviarLote:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskNFeRecepcao(arquivo));
                            break;

                        case Servicos.NFeGerarChave:
                            GerarChaveNFe(arquivo);
                            break;

                        case Servicos.NFeInutilizarNumeros:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskNFeInutilizacao(arquivo));
                            break;

                        case Servicos.NFePedidoSituacaoLote:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskNFeRetRecepcao(arquivo));
                            break;

                        case Servicos.NFeMontarLoteUma:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskNFeMontarLoteUmaNFe(arquivo));
                            break;

                        case Servicos.NFeMontarLoteVarias:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskNFeMontarLoteVarias());
                            break;

                        case Servicos.NFePedidoConsultaSituacao:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskNFeConsultaSituacao(arquivo));
                            break;

                        #endregion NFe

                        #region MDFe

                        case Servicos.MDFeAssinarValidarEnvioEmLote:
                            CertVencido(emp);
                            AssinarValidarMDFe(arquivo);
                            break;

                        case Servicos.MDFeConsultaNaoEncerrado:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskMDFeConsNaoEncerrado(arquivo));
                            break;

                        case Servicos.MDFeConsultaStatusServico:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskMDFeConsultaStatus(arquivo));
                            break;

                        case Servicos.MDFeEnviarSinc:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskMDFeRecepcaoSinc(arquivo));
                            break;

                        case Servicos.MDFePedidoConsultaSituacao:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskMDFeConsultaSituacao(arquivo));
                            break;

                        case Servicos.MDFeRecepcaoEvento:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskMDFeEventos(arquivo));
                            break;

                        #endregion MDFe

                        #region CTe

                        case Servicos.CTeAssinarValidarEnvioEmLote:
                            CertVencido(emp);
                            AssinarValidarCTe(arquivo);
                            break;

                        case Servicos.CTeConsultaStatusServico:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCTeConsultaStatus(arquivo));
                            break;

                        case Servicos.CTeEnviarSinc:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCTeRecepcaoSinc(arquivo));
                            break;

                        case Servicos.CTeEnviarSimp:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCTeRecepcaoSimp(arquivo));
                            break;

                        case Servicos.CTePedidoConsultaSituacao:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCTeConsultaSituacao(arquivo));
                            break;

                        case Servicos.CTeRecepcaoEvento:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCTeEventos(arquivo));
                            break;

                        case Servicos.CteRecepcaoOS:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCTeRecepcaoOS(arquivo));
                            break;

                        #endregion CTe

                        #region DFe

                        case Servicos.DFeEnviar:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskDFeRecepcao(arquivo));
                            break;

                        case Servicos.CTeDistribuicaoDFe:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskDFeRecepcaoCTe(arquivo));
                            break;

                        #endregion DFe

                        #region EFDReinf

                        case Servicos.RecepcaoLoteReinf:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskRecepcaoLoteReinf(arquivo));
                            break;

                        case Servicos.ConsultasReinf:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskConsultasReinf(arquivo));
                            break;

                        case Servicos.ConsultaLoteAssincReinf:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskConsultaLoteAssincronoReinf(arquivo));
                            break;

                        #endregion EFDReinf

                        #region eSocial

                        case Servicos.RecepcaoLoteeSocial:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskRecepcaoLoteeSocial(arquivo));
                            break;

                        case Servicos.ConsultarLoteeSocial:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskConsultarLoteeSocial(arquivo));
                            break;

                        case Servicos.ConsultarIdentificadoresEventoseSocial:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskConsultarIdentificadoresEventoseSocial(arquivo));
                            break;

                        case Servicos.DownloadEventoseSocial:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskDownloadEventoseSocial(arquivo));
                            break;

                        #endregion eSocial

                        #region GNRE
                        case Servicos.ConsultaResultadoLoteGNRE:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskConsultaResultadoLoteGNRE(arquivo));
                            break;

                        case Servicos.LoteRecepcaoGNRE:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskLoteRecepcaoGNRE(arquivo));
                            break;

                        case Servicos.ConsultaConfigUfGNRE:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskConsultaConfigUfGNRE(arquivo));
                            break;
                        #endregion

                        #region CCG
                        case Servicos.CCGConsGTIN:
                            DirecionarArquivo(emp, true, true, arquivo, new TaskCcgConsGTIN(arquivo));
                            break;

                        #endregion

                        #region PIX

                        case Servicos.PIXCobrancaCreateRequest:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskPIXCobrancaCreateRequest(arquivo));
                            break;

                        case Servicos.PIXGetRequest:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskPIXGetRequest(arquivo));
                            break;

                        case Servicos.PIXConsultaRequest:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskPIXConsultaRequest(arquivo));
                            break;

                        #endregion

                        #region eBoleto

                        case Servicos.BoletoRegistrar:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskBoletoRegistrar(arquivo));
                            break;

                        case Servicos.BoletoCancelar:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskBoletoCancelar(arquivo));
                            break;

                        case Servicos.BoletoConsultar:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskBoletoConsultar(arquivo));
                            break;

                        case Servicos.BoletoAlterarVencto:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskBoletoAlterarVencto(arquivo));
                            break;

                        case Servicos.BoletoEnviarInstrucao:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskBoletoEnviarInstrucao(arquivo));
                            break;

                        case Servicos.BoletoInformarPagto:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskBoletoInformarPagto(arquivo));
                            break;

                        #endregion 

                        #region uMessenger

                        case Servicos.UMessenger:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskUMessenger(arquivo));
                            break;

                        #endregion uMessenger

                        #region DARE

                        case Servicos.RecepcaoDARE:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskRecepcaoDARE(arquivo));
                            break;

                        case Servicos.ConsultaReceitasDARE:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskConsultasReceitasDARE(arquivo));
                            break;

                        #endregion DARE

                        #region NF3e

                        case Servicos.NF3eStatusServico:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskConsultaStatusNF3e(arquivo));
                            break;

                        case Servicos.NF3eAutorizacaoSinc:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskNF3eRecepcaoSinc(arquivo));
                            break;

                        case Servicos.NF3eConsultaProtocolo:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskConsultaSituacaoNF3e(arquivo));
                            break;

                        case Servicos.NF3eRecepcaoEvento:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskNF3eEventos(arquivo));
                            break;

                        #endregion NF3e

                        #region NFCom

                        case Servicos.NFComStatusServico:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskConsultaStatusNFCom(arquivo));
                            break;

                        case Servicos.NFComConsultaProtocolo:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskConsultaSituacaoNFCom(arquivo));
                            break;

                        case Servicos.NFComAutorizacaoSinc:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskNFComRecepcaoSinc(arquivo));
                            break;

                        case Servicos.NFComRecepcaoEvento:
                            DirecionarArquivo(emp, false, true, arquivo, new TaskNFComEventos(arquivo));
                            break;

                            #endregion NFCom
                    }

                    #region Serviços em comum

                    switch (servico)
                    {
                        case Servicos.EnviarFTP:
                            EnviarFTP(arquivo, emp);
                            break;

                        case Servicos.AssinarValidar:
                            if (!arquivo.ToLower().Contains(Empresas.Configuracoes[emp].PastaValidar.ToLower()) || Empresas.Configuracoes[emp].AmbienteCodigo != 2)
                            {
                                CertVencido(emp);
                            }
                            AssinarValidar(arquivo);
                            break;

                        case Servicos.UniNFeAlterarConfiguracoes:
                            ReconfigurarUniNFe(arquivo);
                            break;

                        case Servicos.UniNFeConsultaGeral:
                            ConsultarGeral(arquivo);
                            break;

                        case Servicos.UniNFeUpdate:
                            new UniNFeUpdate(DefinirProxy()).Instalar();
                            break;

                        case Servicos.UniNFeConsultaInformacoes:
                            ConsultaInformacoesUniNFe(arquivo);
                            break;

                        case Servicos.DANFEImpressao:
                            DirecionarArquivo(emp, false, false, arquivo, new TaskDanfe());
                            break;

                        case Servicos.DANFEImpressao_Contingencia:
                            DirecionarArquivo(emp, false, false, arquivo, new TaskDanfeContingencia());
                            break;

                        case Servicos.DANFERelatorio:
                            DirecionarArquivo(emp, false, false, arquivo, new TaskDanfeReport());
                            break;
                    }

                    #endregion Serviços em comum
                }
                catch (ExceptionSemInternet ex)
                {
                    GravaErroERP(arquivo, servico, ex, ex.ErrorCode);
                }
                catch (CertificadoDigitalException ex)
                {
                    GravaErroERP(arquivo, servico, ex, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    switch (servico)
                    {
                        case Servicos.SATConsultar:
                        case Servicos.SATEnviarDadosVenda:
                        case Servicos.SATConverterNFCe:
                        case Servicos.NFeConsultaStatusServico:
                        case Servicos.UniNFeUpdate:
                        case Servicos.Nulo:

                            /// 7/2012 <<< danasa
                            ///o erp nao precisa esperar pelo tempo excedido, então retornamos um arquivo .err
                            ///
                            GravaErroERP(arquivo, servico, ex, ErroPadrao.ErroNaoDetectado);
                            break;

                        default:
                            break;
                    }
                }
            }
            catch { }
        }

        private void EnviarFTP(string arquivo, int emp)
        {
            var doc = new XmlDocument();
            doc.Load(arquivo);

            if (doc.GetElementsByTagName("PastaDestino").Count > 0)
            {
                var pastaDestino = doc.GetElementsByTagName("PastaDestino")[0].InnerText;

                var listEnviarFTP = doc.GetElementsByTagName("EnviarFTP");
                foreach (var nodeItem in listEnviarFTP)
                {
                    var elementItem = (XmlElement)nodeItem;

                    var listArquivo = elementItem.GetElementsByTagName("NomeArquivo");
                    foreach (var nodeArquivo in listArquivo)
                    {
                        var elementArquivo = (XmlElement)nodeArquivo;
                        Empresas.Configuracoes[emp].SendToFTP(elementArquivo.InnerText, pastaDestino);
                    }
                }
            }
            else
            {
                throw new Exception("Não foi possível localizar a tag <PastaDestino>, por favor, informe esta pasta no XML dos arquivos a serem enviados pelo FTP.");
            }
        }


        /// <summary>
        /// Carrega o PIN do A3 se ainda não carregou
        /// </summary>
        /// <param name="emp">PIN de qual empresa?</param>
        private void CarregarPINA3(int emp)
        {
            if (!string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].CertificadoPIN) && !Empresas.Configuracoes[emp].CertificadoPINCarregado)
            {
                try
                {
                    if (Empresas.Configuracoes[emp].X509Certificado == null)
                    {
                        Empresas.Configuracoes[emp].X509Certificado = Empresas.Configuracoes[emp].BuscaConfiguracaoCertificado();
                    }

                    Empresas.Configuracoes[emp].X509Certificado.SetPinPrivateKey(Empresas.Configuracoes[emp].CertificadoPIN);
                    Empresas.Configuracoes[emp].CertificadoPINCarregado = true;
                }
                catch
                {
                }
            }
        }

        #endregion ProcessaArquivo()

        #region ValidarExtensao

        private void ValidarExtensao(string arquivo)
        {
            var extOk = false;
            var extensoes = "";
            foreach (Propriedade.TipoEnvio item in Enum.GetValues(typeof(Propriedade.TipoEnvio)))
            {
                var EXT = Propriedade.Extensao(item);

                if (extensoes != "")
                {
                    extensoes += ", ";
                }

                extensoes += EXT.EnvioXML;

                if (arquivo.EndsWith(EXT.EnvioXML, StringComparison.InvariantCultureIgnoreCase))
                {
                    extOk = true;
                    break;
                }
                if (!string.IsNullOrEmpty(EXT.EnvioTXT))
                {
                    extensoes += (EXT.descricao != "" ? " ou " : ", ") + EXT.EnvioTXT;
                    if (arquivo.EndsWith(EXT.EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        extOk = true;
                        break;
                    }
                }
                if (EXT.descricao != "")
                {
                    extensoes += ": \"" + EXT.descricao + "\"\r\n";
                }
            }
            if (!extOk)
            {
                throw new Exception("Não pode identificar o tipo de arquivo baseado no arquivo '" + arquivo + "'\r\nExtensões permitidas: " + extensoes);
            }
        }

        #endregion ValidarExtensao

        #region DefinirTipoServico()

        private Servicos DefinirTipoServico(int empresa, string fullPath)
        {
            var tipoServico = Servicos.Nulo;

            var arq = fullPath.ToLower().Trim();

            #region Serviços que funcionam tanto na pasta Geral como na pasta da Empresa

            if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsCertificado).EnvioXML) >= 0)
            {
                tipoServico = Servicos.UniNFeConsultaGeral;
            }
            else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.AltCon).EnvioXML) >= 0 ||
                     arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.AltCon).EnvioTXT) >= 0)
            {
                tipoServico = Servicos.UniNFeAlterarConfiguracoes;
            }
            else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvImpressaoDanfe).EnvioXML) >= 0 ||
                     arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvImpressaoDanfe).EnvioTXT) >= 0)
            {
                tipoServico = Servicos.DANFEImpressao;
            }
            else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvDanfeReport).EnvioXML) >= 0 ||
                     arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvDanfeReport).EnvioTXT) >= 0)
            {
                tipoServico = Servicos.DANFERelatorio;
            }
            else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.Update).EnvioXML) >= 0 ||
                     arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.Update).EnvioTXT) >= 0)
            {
                tipoServico = Servicos.UniNFeUpdate;
            }
            else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).EnvioXML) >= 0 ||
                     arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).EnvioTXT) >= 0)
            {
                tipoServico = Servicos.UniNFeConsultaInformacoes;
            }

            #endregion Serviços que funcionam tanto na pasta Geral como na pasta da Empresa

            else
            {
                if (arq.IndexOf(Empresas.Configuracoes[empresa].PastaContingencia.ToLower()) >= 0)
                {
                    tipoServico = Servicos.DANFEImpressao_Contingencia;
                }
                else if (arq.IndexOf(Empresas.Configuracoes[empresa].PastaValidar.ToLower()) >= 0)
                {
                    tipoServico = Servicos.AssinarValidar;
                }
                else
                {
                    var infArq = new FileInfo(arq);
                    var pastaArq = ConfiguracaoApp.RemoveEndSlash(infArq.DirectoryName).ToLower().Trim();
                    var pastaLote = ConfiguracaoApp.RemoveEndSlash(Empresas.Configuracoes[empresa].PastaXmlEmLote).ToLower().Trim();
                    var pastaEnvio = ConfiguracaoApp.RemoveEndSlash(Empresas.Configuracoes[empresa].PastaXmlEnvio).ToLower().Trim();
                    if (pastaArq.EndsWith("\\temp"))
                    {
                        pastaArq = Path.GetDirectoryName(pastaArq);
                    }

                    #region Arquivos com extensão txt (Somente NFe tem TXT)

                    if (fullPath.ToLower().EndsWith(".txt"))
                    {
                        if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.NFePedidoConsultaSituacao;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.NFeConsultaStatusServico;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsCad).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.ConsultaCadastroContribuinte;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.NFeInutilizarNumeros;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.NFeConverterTXTparaXML;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.GerarChaveNFe).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.NFeGerarChave;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.UniNFeConsultaInformacoes;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCCe).EnvioTXT) >= 0 ||
                                arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioTXT) >= 0 ||
                                arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCancelamento).EnvioTXT) >= 0 ||
                                arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvManifestacao).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.EventoRecepcao;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.DFeEnviar;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.CTeDistribuicaoDFe;
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioTXT) >= 0)
                        {
                            if (arq.IndexOf(Empresas.Configuracoes[empresa].PastaXmlEmLote.ToLower().Trim()) >= 0)
                            {
                                tipoServico = Servicos.NFeMontarLoteVarias;
                            }
                        }
                        else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.CCG_consgtin).EnvioTXT) >= 0)
                        {
                            tipoServico = Servicos.CCGConsGTIN;
                        }

                    }

                    #endregion Arquivos com extensão txt (Somente NFe tem TXT)

                    else

                    #region Arquivos com extensão XML

                    {
                        var nameTag = "";
                        var lastChildName = "";
                        try
                        {
                            var doc = new XmlDocument();
                            doc.Load(fullPath);

                            nameTag = doc.DocumentElement.Name;
                            lastChildName = doc.DocumentElement.LastChild.Name;
                        }
                        catch
                        {
                            throw;
                        }


                        switch (nameTag)
                        {
                            #region DFe

                            case "distDFeInt":
                                tipoServico = Servicos.DFeEnviar;

                                if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.CTeDistribuicaoDFe;
                                }
                                break;

                            #endregion DFe

                            #region MDFe

                            case "consMDFeNaoEnc":
                                tipoServico = Servicos.MDFeConsultaNaoEncerrado;
                                break;

                            case "consStatServMDFe":
                                tipoServico = Servicos.MDFeConsultaStatusServico;
                                break;

                            case "MDFe":
                                tipoServico = Servicos.MDFeEnviarSinc;
                                break;

                            case "consSitMDFe":
                                tipoServico = Servicos.MDFePedidoConsultaSituacao;
                                break;

                            case "eventoMDFe":
                                tipoServico = Servicos.MDFeRecepcaoEvento;
                                break;

                            #endregion MDFe

                            #region CTe

                            case "consStatServCte":
                            case "consStatServCTe":
                                tipoServico = Servicos.CTeConsultaStatusServico;
                                break;

                            case "CTe":
                                tipoServico = Servicos.CTeEnviarSinc;
                                break;

                            case "CTeSimp":
                                tipoServico = Servicos.CTeEnviarSimp;
                                break;

                            case "consSitCTe":
                                tipoServico = Servicos.CTePedidoConsultaSituacao;
                                break;

                            case "eventoCTe":
                                tipoServico = Servicos.CTeRecepcaoEvento;
                                break;

                            case "CTeOS":
                                tipoServico = Servicos.CteRecepcaoOS;
                                break;

                            #endregion CTe

                            #region NFe

                            case "consStatServ":
                                tipoServico = Servicos.NFeConsultaStatusServico;
                                break;

                            case "NFe":
                                if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConverterSAT).EnvioXML) >= 0)
                                {
                                    goto default;
                                }

                                if (pastaArq == pastaLote)
                                {
                                    tipoServico = Servicos.NFeAssinarValidarEnvioEmLote;
                                }
                                else if (pastaArq == pastaEnvio)
                                {
                                    tipoServico = Servicos.NFeMontarLoteUma;
                                }

                                break;

                            case "enviNFe":
                                tipoServico = Servicos.NFeEnviarLote;
                                break;

                            case "consReciNFe":
                                tipoServico = Servicos.NFePedidoSituacaoLote;
                                break;

                            case "consSitNFe":
                                tipoServico = Servicos.NFePedidoConsultaSituacao;
                                break;

                            case "inutNFe":
                                tipoServico = Servicos.NFeInutilizarNumeros;
                                break;

                            case "envEvento":
                                tipoServico = Servicos.EventoRecepcao;
                                break;

                            case "ConsCad":
                                tipoServico = Servicos.ConsultaCadastroContribuinte;
                                break;

                            case "MontarLoteNFe":
                                if (arq.IndexOf(Empresas.Configuracoes[empresa].PastaXmlEmLote.ToLower().Trim()) >= 0)
                                {
                                    tipoServico = Servicos.NFeMontarLoteVarias;
                                }
                                break;

                            case "gerarChave":
                                tipoServico = Servicos.NFeGerarChave;
                                break;

                            #endregion NFe

                            #region EFDReinf

                            case "Reinf":
                                switch (lastChildName)
                                {
                                    case "ConsultaInformacoesConsolidadas":
                                    case "ConsultaReciboEvento":
                                        tipoServico = Servicos.ConsultasReinf;
                                        break;

                                    case "ConsultaResultadoFechamento2099":
                                        tipoServico = Servicos.ConsultaFechamento2099Reinf;
                                        break;

                                    case "loteEventos":
                                    case "envioLoteEventos":
                                        tipoServico = Servicos.RecepcaoLoteReinf;
                                        break;

                                    case "ConsultaLoteAssincrono":
                                        tipoServico = Servicos.ConsultaLoteAssincReinf;
                                        break;

                                    default:
                                        throw new Exception("Para envio dos eventos do EFDReinf gere o arquivo de lote, o que tem o prefixo final igual a -reinf-loteevt.xml\r\n" +
                                            "Para envio da consulta do lote de eventos, gere o arquivo com o prefixo final igual a -reinf-consloteevt.xml\r\n\r\n" +
                                            "Os modelos de arquivos do EFDReinf estão disponíveis no link www.unimake.com.br/uninfe/modelosxml/efdreinf");
                                }
                                break;

                            #endregion EFDReinf

                            #region eSocial

                            case "eSocial":
                                switch (lastChildName)
                                {
                                    case "consultaLoteEventos":
                                        tipoServico = Servicos.ConsultarLoteeSocial;
                                        break;

                                    case "envioLoteEventos":
                                        tipoServico = Servicos.RecepcaoLoteeSocial;
                                        break;

                                    case "consultaIdentificadoresEvts":
                                        tipoServico = Servicos.ConsultarIdentificadoresEventoseSocial;
                                        break;

                                    case "download":
                                        tipoServico = Servicos.DownloadEventoseSocial;
                                        break;

                                    default:
                                        throw new Exception("Para envio dos eventos do eSocial gere o arquivo de lote, o que contém como prefixo final do nome do arquivo igual -esocial-loteevt.xml e como tags inicias as seguintes:\r\n\r\n" +
                                            "<eSocial xmlns=\"http://www.esocial.gov.br/schema/lote/eventos/envio/v1_1_1\">\r\n" +
                                            "<envioLoteEventos grupo=\"1\">\r\n\r\n" +
                                            "Para envio da consulta do lote de eventos, gere o arquivo com o prefixo final igual a -esocial-consloteevt.xml\r\n\r\n" +
                                            "Os modelos de arquivos do eSocial estão disponíveis no link https://www.unimake.com.br/uninfe/modelos.php?p=esocial");
                                }
                                break;

                            #endregion eSocial

                            #region GNRE

                            case "TConsLote_GNRE":
                                tipoServico = Servicos.ConsultaResultadoLoteGNRE;
                                break;

                            case "TLote_GNRE":
                                tipoServico = Servicos.LoteRecepcaoGNRE;
                                break;

                            case "TConsultaConfigUf":
                                tipoServico = Servicos.ConsultaConfigUfGNRE;
                                break;

                            #endregion

                            #region CCG

                            case "consGTIN":
                                tipoServico = Servicos.CCGConsGTIN;
                                break;

                            #endregion

                            #region PIX

                            case "PIXCobrancaCreateRequest":
                                tipoServico = Servicos.PIXCobrancaCreateRequest;
                                break;

                            case "PIXGetRequest":
                                tipoServico = Servicos.PIXGetRequest;
                                break;

                            case "PIXConsultaRequest":
                                tipoServico = Servicos.PIXConsultaRequest;
                                break;

                            #endregion

                            #region eBoleto

                            case "BoletoRegistrar":
                                tipoServico = Servicos.BoletoRegistrar;
                                break;

                            case "BoletoCancelar":
                                tipoServico = Servicos.BoletoCancelar;
                                break;

                            case "BoletoConsultar":
                                tipoServico = Servicos.BoletoConsultar;
                                break;

                            case "BoletoAlterarVencto":
                                tipoServico = Servicos.BoletoAlterarVencto;
                                break;

                            case "BoletoEnviarInstrucao":
                                tipoServico = Servicos.BoletoEnviarInstrucao;
                                break;

                            case "BoletoInformarPagto":
                                tipoServico = Servicos.BoletoInformarPagto;
                                break;

                            #endregion

                            #region uMessenger

                            case "uMessenger":
                                tipoServico = Servicos.UMessenger;
                                break;

                            #endregion

                            #region DARE

                            case "Receitas":
                                tipoServico = Servicos.ConsultaReceitasDARE;
                                break;

                            case "Dare":
                            case "DareLote":
                                tipoServico = Servicos.RecepcaoDARE;
                                break;

                            #endregion DARE

                            #region NF3e

                            case "consStatServNF3e":
                                tipoServico = Servicos.NF3eStatusServico;
                                break;

                            case "consSitNF3e":
                                tipoServico = Servicos.NF3eConsultaProtocolo;
                                break;

                            case "NF3e":
                                tipoServico = Servicos.NF3eAutorizacaoSinc;
                                break;

                            case "eventoNF3e":
                                tipoServico = Servicos.NF3eRecepcaoEvento;
                                break;

                            #endregion NF3e

                            #region NFCom

                            case "consStatServNFCom":
                                tipoServico = Servicos.NFComStatusServico;
                                break;

                            case "consSitNFCom":
                                tipoServico = Servicos.NFComConsultaProtocolo;
                                break;

                            case "NFCom":
                                tipoServico = Servicos.NFComAutorizacaoSinc;
                                break;

                            case "eventoNFCom":
                                tipoServico = Servicos.NFComRecepcaoEvento;
                                break;

                            #endregion NFCom


                            #region Geral

                            case "ConsInf":
                                tipoServico = Servicos.UniNFeConsultaInformacoes;
                                break;

                            case "EnviarFTP":
                                tipoServico = Servicos.EnviarFTP;
                                break;

                            #endregion Geral

                            default:

                                #region NFS-e

                                if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarLoteRps;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeCancelar;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSitLoteRps).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarSituacaoLoteRps;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeRecepcionarLoteRps;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultar;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarPorRps;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedURLNFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarURL;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedURLNFSeSerie).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarURLSerie;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedNFSePNG).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarNFSePNG;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedInuNFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeInutilizarNFSe;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedNFSePDF).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarNFSePDF;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedNFSeXML).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeObterNotaFiscal;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSeqLoteNotaRPS).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultaSequenciaLoteNotaRPS;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSubstNfse).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeSubstituirNfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRec).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarNFSeRecebidas;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedNFSeEmit).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarNFSeEmitidas;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeTom).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarNFSeTomados;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedStaNFse).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarStatusNota;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSitCanNFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarSituacaoCancelamento;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedConvenio).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarConvenioMunicipal;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedAliquotas).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarAliquotasMunicipais;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedHistorico).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarHistoricoAliquotasMunicipais;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedRegimes).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarRegimesEspeciaisMunicipais;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedRetencoes).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarRetencoesMunicipais;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedBeneficio).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.NFSeConsultarBeneficioMunicipal;
                                }

                                #endregion NFS-e

                                #region CFS-e

                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.RecepcionarLoteCfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedCanCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.CancelarCfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.ConsultarLoteCfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSitCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.ConsultarCfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvConfigTermCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.ConfigurarTerminalCfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvInfManutCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.EnviarInformeManutencaoCfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnvInfSemMovCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.InformeTrasmissaoSemMovimentoCfse;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsDadosCadCFSe).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.ConsultarDadosCadastroCfse;
                                }

                                #endregion CFS-e

                                #region SAT/CFe

                                if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsultarSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATConsultar;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ExtrairLogsSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATExtrairLogs;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.TesteFimAFimSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATTesteFimAFim;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsultarStatusOperacionalSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATConsultarStatusOperacional;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.TrocarCodigoDeAtivacaoSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATTrocarCodigoDeAtivacao;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.EnviarDadosVendaSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATEnviarDadosVenda;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConverterSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATConverterNFCe;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.CancelarUltimaVendaSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATCancelarUltimaVenda;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConfigurarInterfaceDeRedeSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATConfigurarInterfaceDeRede;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.AssociarAssinaturaSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATAssociarAssinatura;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.BloquearSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATBloquear;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.AtivarSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATAtivar;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.DesbloquearSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATDesbloquear;
                                }
                                else if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsultarNumeroSessaoSAT).EnvioXML) >= 0)
                                {
                                    tipoServico = Servicos.SATConsultarNumeroSessao;
                                }

                                #endregion SAT/CFe

                                break;
                        }
                    }

                    #endregion Arquivos com extensão XML
                }
            }

            return tipoServico;
        }

        #endregion DefinirTipoServico()

        #region AssinarValidarNFe()

        /// <summary>
        /// Assinar e Validar todos os arquivos XML de notas fiscais encontrados na pasta informada por parâmetro
        /// </summary>
        /// <param name="arquivo">Arquivo a ser validado e assinado</param>
        protected void AssinarValidarNFe(string arquivo)
        {
            var nfe = new TaskNFeAssinarValidar
            {
                NomeArquivoXML = arquivo
            };
            nfe.AssinarValidarXMLNFe();
        }

        #endregion AssinarValidarNFe()

        #region AssinarValidarCTe()

        /// <summary>
        /// Assinar e Validar todos os arquivos XML de notas fiscais encontrados na pasta informada por parâmetro
        /// </summary>
        /// <param name="arquivo">Arquivo a ser validado e assinado</param>
        protected void AssinarValidarCTe(string arquivo)
        {
            var nfe = new TaskCTeAssinarValidar
            {
                NomeArquivoXML = arquivo
            };
            nfe.AssinarValidarXMLNFe();
        }

        #endregion AssinarValidarCTe()

        #region AssinarValidarMDFe()

        /// <summary>
        /// Assinar e Validar todos os arquivos XML de notas fiscais encontrados na pasta informada por parâmetro
        /// </summary>
        /// <param name="arquivo">Arquivo a ser validado e assinado</param>
        protected void AssinarValidarMDFe(string arquivo)
        {
            var nfe = new TaskMDFeAssinarValidar
            {
                NomeArquivoXML = arquivo
            };
            nfe.AssinarValidarXMLNFe();
        }

        #endregion AssinarValidarMDFe()

        #region AssinarValidar()

        /// <summary>
        /// Executa as tarefas pertinentes ao processo de somente assinar e validar os arquivos
        /// </summary>
        /// <param name="arquivo">Arquivo a ser assinado e validado</param>
        protected void AssinarValidar(string arquivo)
        {
            try
            {
                var emp = Empresas.FindEmpresaByThread();

                if (!arquivo.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (new ValidarXMLNew().Validar(arquivo, true, emp))
                    {
                        return;
                    }
                }

                Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaValidado, Path.GetFileName(Path.ChangeExtension(arquivo, ".xml"))));
                Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Path.GetFileName(Path.ChangeExtension(arquivo, ".xml"))));
                Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Path.GetFileName(arquivo)));

                if (arquivo.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        #region DFe

                        var temp = Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).EnvioTXT);
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, temp + Propriedade.ExtRetorno.retEnvDFe_ERR));
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, temp + Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).EnvioXML));
                        DirecionarArquivo(emp, false, false, arquivo, new TaskDFeRecepcao(arquivo));

                        #endregion DFe
                    }

                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        #region DFe

                        var temp = Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioTXT);
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, temp + Propriedade.ExtRetorno.retEnvDFeCTe_ERR));
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, temp + Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioXML));
                        DirecionarArquivo(emp, false, false, arquivo, new TaskDFeRecepcaoCTe(arquivo));

                        #endregion DFe
                    }

                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.ConsCad).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        #region Consulta ao cadastro de contribuinte

                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.ConsCad).EnvioTXT) + Propriedade.ExtRetorno.ConsCad_ERR));
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.ConsCad).EnvioTXT) + Propriedade.Extensao(Propriedade.TipoEnvio.ConsCad).RetornoXML));
                        DirecionarArquivo(emp, false, false, arquivo, new TaskCadastroContribuinte(arquivo));

                        #endregion Consulta ao cadastro de contribuinte
                    }

                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        new ConverterTXT(arquivo);
                    }

                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCCe).EnvioTXT, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCancelamento).EnvioTXT, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvManifestacao).EnvioTXT, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        #region Eventos

                        if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCCe).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.EnvCCe).EnvioTXT) + Propriedade.ExtRetorno.retEnvCCe_ERR));
                        }

                        if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCancelamento).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.EnvCancelamento).EnvioTXT) + Propriedade.ExtRetorno.retCancelamento_ERR));
                        }

                        if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvManifestacao).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.EnvManifestacao).EnvioTXT) + Propriedade.ExtRetorno.retManifestacao_ERR));
                        }

                        if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioTXT) + Propriedade.ExtRetorno.Eve_ERR));
                        }

                        DirecionarArquivo(emp, false, false, arquivo, new TaskNFeEventos(arquivo));

                        #endregion Eventos
                    }

                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        #region Inutilizacao

                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioTXT) + Propriedade.ExtRetorno.Inu_ERR));
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioTXT) + Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).RetornoXML));
                        DirecionarArquivo(emp, false, false, arquivo, new TaskNFeInutilizacao(arquivo));

                        #endregion Inutilizacao
                    }

                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioTXT, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioTXT) + Propriedade.ExtRetorno.Sta_ERR));
                        DirecionarArquivo(emp, false, false, arquivo, new TaskNFeConsultaStatus(arquivo));
                    }

                    if (arquivo.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioTXT) >= 0)
                    {
                        Functions.DeletarArquivo(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioTXT) + Propriedade.ExtRetorno.Sit_ERR));
                        DirecionarArquivo(emp, false, false, arquivo, new TaskNFeConsultaSituacao(arquivo));
                    }
                }
                else
                {
                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCCe).EnvioXML, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvCancelamento).EnvioXML, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.EnvManifestacao).EnvioXML, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioXML, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML, StringComparison.InvariantCultureIgnoreCase) ||
                        arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML, StringComparison.InvariantCultureIgnoreCase))
                    {
                        DirecionarArquivo(arquivo);
                    }

                    var validar = new ValidarXML(arquivo, Empresas.Configuracoes[emp].UnidadeFederativaCodigo, true);
                    validar.ValidarAssinarXML(arquivo);
                }
            }
            catch (Exception ex)
            {
                new ValidarXML(arquivo, "Ocorreu um erro ao assinar o XML: " + ex.Message);
            }
        }

        #endregion AssinarValidar()

        #region ConsultaInformacoesUniNFe()

        /// <summary>
        /// Executa as tarefas pertinentes a consulta das informações do UniNFe
        /// </summary>
        /// <param name="arquivo">Arquivo a ser tratado/param>
        protected void ConsultaInformacoesUniNFe(string arquivo)
        {
            try
            {
                GravarXMLDadosCertificado(arquivo);
            }
            catch
            {
            }
        }

        #endregion ConsultaInformacoesUniNFe()

        #region ConverterTXTparaXML()

        /// <summary>
        /// Executa as tarefas pertinentes da conversão de NF-e em TXT para XML
        /// </summary>
        /// <param name="arquivo">Nome do arquivo a ser convertido</param>
        protected void ConverterTXTparaXML(string arquivo)
        {
            try
            {
                new ConverterTXT(arquivo);
            }
            catch
            {
            }
        }

        #endregion ConverterTXTparaXML()

        #region LimpezaTemporario()

        /// <summary>
        /// Executar as tarefas pertinentes a limpeza de arquivos temporários
        /// </summary>
        public void LimpezaTemporario()
        {
            Thread.Sleep(600000); // 10 minutos para executar a primeira vez para evitar apagar arquivo que estava na pasta de envio e foi para a temp

            while (true)
            {
                ExecutaLimpeza();

                Thread.Sleep(new TimeSpan(1, 0, 0, 0));
            }
        }

        #endregion LimpezaTemporario()

        #region EmProcessamento()

        /// <summary>
        /// Executar as tarefas pertinentes a analise das notas em processamento a mais de 3 minutos
        /// </summary>
        public void EmProcessamento()
        {
            var hasAll = false;

            while (true)
            {
                for (var i = 0; i < Empresas.Configuracoes.Count; i++)
                {
                    if (Empresas.Configuracoes[i].Servico == TipoAplicativo.Nfse ||
                        Empresas.Configuracoes[i].Servico == TipoAplicativo.SATeMFE ||
                        Empresas.Configuracoes[i].Servico == TipoAplicativo.eSocial ||
                        Empresas.Configuracoes[i].Servico == TipoAplicativo.EFDReinf ||
                        Empresas.Configuracoes[i].Servico == TipoAplicativo.EFDReinfeSocial ||
                        Empresas.Configuracoes[i].Servico == TipoAplicativo.GNREeDARE)
                    {
                        continue;
                    }

                    var nfe = new NFeEmProcessamento();
                    nfe.Analisar(i);

                    hasAll = true;
                }
                if (hasAll)
                {
                    Thread.Sleep(60000); //Dorme por 1 minuto
                }
                else
                {
                    break;
                }
            }
        }

        #endregion EmProcessamento()

        #region GerarChaveNFe()

        /// <summary>
        /// Executa tarefas pertinentes a geração da Chave da NFe solicitado pelo ERP
        /// </summary>
        /// <param name="arquivo">Arquivo a ser tratado</param>
        protected void GerarChaveNFe(string arquivo)
        {
            var fi = new FileInfo(arquivo);

            // processa arquivos XML
            if (fi.Extension.ToLower() == ".xml")
            {
                new NFeW().GerarChaveNFe(arquivo, true);
            }

            // processa arquivos TXT
            else
            {
                new NFeW().GerarChaveNFe(arquivo, false);
            }
        }

        #endregion GerarChaveNFe()

        #region GerarXMLPedRec()

        /// <summary>
        /// Executa as tarefas pertinentes a geração dos pedidos de consulta situação de lote da NFe
        /// </summary>
        /// <param name="nfe">Objeto da classe servico NFe</param>
        public void GerarXMLPedRec(object nfe)
        {
            var oNFe = (TaskNFeGerarXMLPedRec)nfe;

            while (true)
            {
                for (var i = 0; i < Empresas.Configuracoes.Count; i++)
                {
                    var empresa = Empresas.Configuracoes[i];

                    if (!string.IsNullOrEmpty(empresa.PastaEmpresa) && empresa.Servico != TipoAplicativo.Nfse)
                    {
                        GerarXMLPedRec(i, oNFe);
                    }
                }

                Thread.Sleep(2000);
            }
        }

        #endregion GerarXMLPedRec()

        #region DirecionarArquivo()

        /// <summary>
        /// Direcionar o arquivo
        /// </summary>
        /// <param name="arquivos">Arquivo</param>
        /// <param name="metodo">Método a ser executado do serviço da NFe</param>
        /// <param name="nfe">Objeto do serviço da NFe a ser executado</param>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 18/04/2011
        /// </remarks>
        private void DirecionarArquivo(int emp, bool veCertificado, bool veConexao, string arquivo, object taskClass)
        {
            try
            {
                if (veCertificado)
                {
                    CertVencido(emp);
                }

                if (veConexao)
                {
                    IsConnectedToInternet();
                }

                //Processa ou envia o XML
                EnviarArquivo(emp, arquivo, taskClass, "Execute");
            }
            catch (CertificadoDigitalException ex)
            {
                throw ex;
            }
            catch (ExceptionSemInternet ex)
            {
                throw ex;
            }
            catch
            {
                //Não pode ser tratado nenhum erro aqui, visto que já estão sendo tratados e devidamente retornados
                //para o ERP no ponto da execução dos serviços. Foi muito bem testado e analisado. Wandrey 09/03/2010
            }
        }

        private void DirecionarArquivo(string arquivo)
        {
            //Processa ou envia o XML
            var valclass = new TaskValidar();

            //Definir o tipo do serviço
            var tipoServico = valclass.GetType();

            //Definir o arquivo XML para a classe UniNfeClass
            tipoServico.InvokeMember("NomeArquivoXML", BindingFlags.SetProperty, null, valclass, new object[] { arquivo });
            tipoServico.InvokeMember("Execute", BindingFlags.InvokeMethod, null, valclass, null);
        }

        #endregion DirecionarArquivo()

        #region EnviarArquivo()

        /// <summary>
        /// Analisa o tipo do XML que está na pasta de envio e executa a operação necessária. Exemplo: Envia ao SEFAZ, reconfigura o UniNFE, etc...
        /// </summary>
        /// <param name="cArquivo">Nome do arquivo XML a ser enviado ou analisado</param>
        /// <param name="oNfe">Objeto da classe UniNfeClass a ser utilizado nas operações</param>
        private void EnviarArquivo(int emp, string arquivo, object nfe, string metodo)
        {
            //Definir o tipo do serviço
            var tipoServico = nfe.GetType();

            //Definir o arquivo XML para a classe UniNfeClass
            tipoServico.InvokeMember("NomeArquivoXML", BindingFlags.SetProperty, null, nfe, new object[] { arquivo });

            var doExecute = arquivo.IndexOf(Empresas.Configuracoes[emp].PastaValidar, StringComparison.InvariantCultureIgnoreCase) >= 0;
            if (!doExecute)
            {
                if (Empresas.Configuracoes[emp].tpEmis != (int)TipoEmissao.ContingenciaFSIA &&
                    Empresas.Configuracoes[emp].tpEmis != (int)TipoEmissao.ContingenciaFSDA &&
                    Empresas.Configuracoes[emp].tpEmis != (int)TipoEmissao.ContingenciaOffLine &&
                    Empresas.Configuracoes[emp].tpEmis != (int)TipoEmissao.ContingenciaEPEC) //Confingência em formulário de segurança e EPEC não envia na hora, tem que aguardar voltar para normal.
                {
                    doExecute = true;
                }
                else
                {
                    if (nfe is TaskDFeRecepcao ||
                        nfe is TaskNFeRetRecepcao ||
                        nfe is TaskNFeConsultaStatus ||
                        nfe is TaskNFeConsultaSituacao ||
                        nfe is TaskDanfeContingencia ||
                        nfe is TaskDanfe ||
                        nfe is TaskCadastroContribuinte ||
                        nfe is TaskCTeConsultaStatus ||
                        nfe is TaskCTeConsultaSituacao ||
                        nfe is TaskCTeRecepcaoOS ||
                        nfe is TaskMDFeConsultaStatus ||
                        nfe is TaskMDFeConsultaSituacao ||
                        nfe is TaskMDFeConsNaoEncerrado ||
                        (nfe is TaskNFeEventos && Empresas.Configuracoes[emp].tpEmis == (int)TipoEmissao.ContingenciaEPEC) ||
                        (nfe is TaskCTeEventos && Empresas.Configuracoes[emp].tpEmis == (int)TipoEmissao.ContingenciaEPEC) ||
                        nfe is TaskRecepcaoLoteReinf ||
                        nfe is TaskRecepcaoLoteeSocial ||
                        nfe is TaskConsultarLoteeSocial ||
                        nfe is TaskConsultarIdentificadoresEventoseSocial ||
                        nfe is TaskDownloadEventoseSocial ||
                        nfe is TaskConsultaResultadoLoteGNRE ||
                        nfe is TaskLoteRecepcaoGNRE ||
                        nfe is TaskPIXConsultaRequest ||
                        nfe is TaskPIXCobrancaCreateRequest ||
                        nfe is TaskPIXGetRequest)
                    {
                        doExecute = true;
                    }
                }
            }
            if (doExecute)
            {
                tipoServico.InvokeMember(metodo, BindingFlags.InvokeMethod, null, nfe, null);
            }
        }

        #endregion EnviarArquivo()

        #region GravarXMLDadosCertificado()

        /// <summary>
        /// Gravar o XML de retorno com as informações do UniNFe para o aplicativo de ERP
        /// </summary>
        /// <param name="oNfe">Objeto da classe UniNfeClass para conseguir pegar algumas informações para gravar o XML</param>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>29/01/2009</date>
        private void GravarXMLDadosCertificado(string ArquivoXml)
        {
            var emp = Empresas.FindEmpresaByThread();
            var oAux = new Auxiliar();
            var somenteConfigGeral = false;

            string sArqRetorno;
            if (Path.GetDirectoryName(ArquivoXml).ToLower() == Components.Propriedade.PastaGeralTemporaria.ToLower())
            {
                somenteConfigGeral = true;
                if (Path.GetExtension(ArquivoXml).ToLower() == ".txt")
                {
                    sArqRetorno = Propriedade.PastaGeralRetorno + "\\" +
                                  Functions.ExtrairNomeArq(ArquivoXml, Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).EnvioTXT) +
                                  Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).RetornoTXT;
                }
                else
                {
                    sArqRetorno = Propriedade.PastaGeralRetorno + "\\" +
                                  Functions.ExtrairNomeArq(ArquivoXml, Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).EnvioXML) +
                                  Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).RetornoXML;
                }
            }
            else
            {
                if (Path.GetExtension(ArquivoXml).ToLower() == ".txt")
                {
                    sArqRetorno = Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                  Functions.ExtrairNomeArq(ArquivoXml, Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).EnvioTXT) +
                                  Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).RetornoTXT;
                }
                else
                {
                    sArqRetorno = Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                  Functions.ExtrairNomeArq(ArquivoXml, Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).EnvioXML) +
                                  Propriedade.Extensao(Propriedade.TipoEnvio.ConsInf).RetornoXML;
                }
            }

            try
            {
                var app = new Aplicacao();

                //Deletar o arquivo de solicitação do serviço
                var oArquivo = new FileInfo(ArquivoXml);
                oArquivo.Delete();

                oArquivo = new FileInfo(sArqRetorno);
                if (oArquivo.Exists)
                {
                    oArquivo.Delete();
                }

                app.GravarXMLInformacoes(sArqRetorno, somenteConfigGeral);
            }
            catch (Exception ex)
            {
                try
                {
                    oAux.GravarArqErroERP(Path.GetFileNameWithoutExtension(sArqRetorno) + ".err", ex.Message);
                }
                catch
                {
                    //Se também falhou gravar o arquivo de retorno para o ERP, infelizmente não posso fazer mais nada. Deve estar com algum problema na rede, HD, permissão de acesso as pastas, etc... Wandrey 09/03/2010
                }
            }
        }

        #endregion GravarXMLDadosCertificado()

        #region ReconfigurarUniNFe()

        /// <summary>
        /// Reconfigura o UniNFe, gravando as novas informações na tela de configuração
        /// </summary>
        /// <param name="cArquivo">Nome do arquivo XML contendo as novas configurações</param>
        protected void ReconfigurarUniNFe(string cArquivo)
        {
            var reconfigurou = false;
            try
            {
                if (!Propriedade.ProcessoReconfigurarUniNFeEmAndamento)
                {
                    reconfigurou = true;
                    Propriedade.ProcessoReconfigurarUniNFeEmAndamento = true;

                    var oConfig = new ConfiguracaoApp();
                    oConfig.ReconfigurarUniNFe(cArquivo);
                }
            }
            catch
            {
            }
            finally
            {
                if (reconfigurou)
                {
                    Propriedade.ProcessoReconfigurarUniNFeEmAndamento = false;
                    Functions.DeletarArquivo(cArquivo);
                }
            }
        }

        #endregion ReconfigurarUniNFe()

        #region GerarXMLPedRec()

        /// <summary>
        /// Gera o XML de consulta do recibo do lote de notas enviadas
        /// </summary>
        /// <param name="empresa">Index da empresa que é para gerar os pedidos de consulta do recibo do lote da nfe</param>
        /// <param name="nfe">Objeto da classe ServicoNfe</param>
        /// <by>Wandrey Mundin Ferreira</by>
        private void GerarXMLPedRec(int empresa, TaskNFeGerarXMLPedRec nfe)
        {
            var fluxoNfe = new FluxoNfe(empresa);

            try
            {
                //Criar a lista dos recibos a serem consultados no SEFAZ
                var recibos = fluxoNfe.CriarListaRec();

                for (var i = 0; i < recibos.Count; i++)
                {
                    var reciboCons = recibos[i];
                    var tempoConsulta = reciboCons.tMed;

                    if (tempoConsulta > 30)
                    {
                        tempoConsulta = 30; //Tempo previsto no manual da SEFAZ, isso foi feito pq o ambiente SVAN está retornando na consulta recibo, tempo superior a 160, mas não está com problema, é erro no calculo deste tempo. Wandrey
                    }

                    if (tempoConsulta < Empresas.Configuracoes[empresa].TempoConsulta)
                    {
                        tempoConsulta = Empresas.Configuracoes[empresa].TempoConsulta;
                    }

                    if (tempoConsulta < 3)
                    {
                        tempoConsulta = 3;
                    }

                    if (DateTime.Now.Subtract(reciboCons.dPedRec).Seconds >= tempoConsulta)
                    {
                        //Atualizar a tag da data e hora da ultima consulta do recibo aumentando 180 segundos (3 minutos) para evitar consumo indevido
                        fluxoNfe.AtualizarDPedRec(reciboCons.nRec, DateTime.Now.AddSeconds(180));
                        nfe.XmlPedRec(empresa, reciboCons.nRec, reciboCons.versao, reciboCons.mod);
                    }
                }
            }
            catch
            {
                
            }
        }

        #endregion GerarXMLPedRec()

        #region Executa Limpeza

        /// <summary>
        /// executa a limpeza das pastas temp e retorno
        /// </summary>
        /// <by>http://desenvolvedores.net/marcelo</by>
        private void ExecutaLimpeza()
        {
            lock (this)
            {
                //Limpar conteúdo da pasta de LOG, mas manter 60 dias de informação
                Limpar(-1, Propriedade.PastaLog, "", 60);

                for (var i = 0; i < Empresas.Configuracoes.Count; i++)
                {
                    //Limpar conteúdo da pasta temp que fica dentro da pasta de envio de cada empresa a cada 10 dias
                    Limpar(i, Empresas.Configuracoes[i].PastaXmlEnvio, "temp", 10);
                    Limpar(i, Empresas.Configuracoes[i].PastaValidar, "temp", 10);   //danasa 12/8/2011
                    Limpar(i, Empresas.Configuracoes[i].PastaXmlEmLote, "temp", 10);   //Wandrey 05/10/2011

                    if (Empresas.Configuracoes[i].DiasLimpeza == 0)
                    {
                        continue;
                    }

                    #region temporario

                    Limpar(i, Empresas.Configuracoes[i].PastaXmlErro, "", Empresas.Configuracoes[i].DiasLimpeza);

                    #endregion temporario

                    #region retorno

                    Limpar(i, Empresas.Configuracoes[i].PastaXmlRetorno, "", Empresas.Configuracoes[i].DiasLimpeza);

                    #endregion retorno
                }
            }
        }

        private void Limpar(int empresa, string diretorio, string subdir, int diasLimpeza)
        {
            // danasa 27-2-2011
            if (diasLimpeza == 0 || string.IsNullOrEmpty(diretorio))
            {
                return;
            }

            if (!Directory.Exists(diretorio))
            {
                return;   //danasa 12/8/2011
            }

            if (!string.IsNullOrEmpty(subdir))
            {
                diretorio += "\\" + subdir;
            }

            if (!Directory.Exists(diretorio))
            {
                return;   //danasa 12/8/2011
            }

            try
            {
                //recupera os arquivos da pasta temporario
                var files = Directory.GetFiles(diretorio, "*.*", SearchOption.AllDirectories);
                var UltimaData = DateTime.Today.AddDays(-diasLimpeza);

                foreach (var file in files)
                {
                    var fi = new FileInfo(file);

                    //usar a última data de acesso, e não a data de criação
                    if (fi.LastWriteTime <= UltimaData)
                    {
                        try
                        {
                            fi.Delete();
                        }
                        catch
                        {
                            //td bem... nao deu para excluir. fica pra próxima
                        }
                    }
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                if (empresa >= 0 && Empresas.Configuracoes.Count > 0)
                {
                    Functions.WriteLog(Empresas.Configuracoes[empresa].Nome + "\r\n" + ex.Message, false, true, Empresas.Configuracoes[empresa].CNPJ);
                }
                else
                {
                    Functions.WriteLog("Geral:\r\n" + ex.Message, false, true, "");
                }
            }
        }

        #endregion Executa Limpeza

        #region CertVencido

        /// <summary>
        /// Verificar se o certificado digital está vencido
        /// </summary>
        /// <param name="emp">Empresa que é para ser verificado o certificado digital</param>
        /// <remarks>
        /// Retorna uma exceção CertificadoDigitalException caso o certificado esteja vencido
        /// </remarks>
        protected void CertVencido(int emp)
        {
            if (Empresas.Configuracoes[emp].UsaCertificado)
            {
                if (new Unimake.Business.Security.CertificadoDigital().Vencido(Empresas.Configuracoes[emp].X509Certificado))
                {
                    throw new CertificadoDigitalException(ErroPadrao.CertificadoVencido, "(" + Empresas.Configuracoes[emp].X509Certificado.NotBefore.ToString() + " a " + Empresas.Configuracoes[emp].X509Certificado.NotAfter.ToString() + ")");
                }
            }
        }

        #endregion CertVencido

        #region IsConnectedToInternet()

        /// <summary>
        /// Verifica se a conexão com a internet está OK
        /// </summary>
        /// <remarks>
        /// Retorna uma exceção ExceptionSemInternet caso a internet não esteja OK
        /// </remarks>
        protected void IsConnectedToInternet()
        {
            //Verificar antes se tem conexão com a internet, se não tiver já gera uma exceção no padrão já esperado pelo ERP
            if (ConfiguracaoApp.ChecarConexaoInternet)
            {
                //            if(!Functions.IsConnectedToInternet())
                if (!Functions.HasInternetConnection(ConfiguracaoApp.Proxy, ConfiguracaoApp.ProxyServidor, ConfiguracaoApp.ProxyUsuario, ConfiguracaoApp.ProxySenha, ConfiguracaoApp.ProxyPorta, ConfiguracaoApp.DetectarConfiguracaoProxyAuto))
                {
                    throw new ExceptionSemInternet(ErroPadrao.FalhaInternet);
                }
            }
        }

        #endregion IsConnectedToInternet()

        #region GravaErroERP()

        /// <summary>
        /// Gravar o erro ocorrido para o ERP
        /// </summary>
        /// <param name="arquivo">Nome do arquivo que seria processado</param>
        /// <param name="extRet">Extensão do arquivo de erro a ser gravado</param>
        /// <param name="servico">Serviço que está sendo executado</param>
        /// <param name="ex">Exception gerada</param>
        protected void GravaErroERP(string arquivo, Servicos servico, Exception ex, ErroPadrao erroPadrao)
        {
            var extRetERR = string.Empty;
            var extRet = string.Empty;

            switch (servico)
            {
                #region NFe / CTe / MDFe

                case Servicos.NFeInutilizarNumeros:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Inu_ERR;
                    break;

                case Servicos.CTePedidoConsultaSituacao:
                case Servicos.NFePedidoConsultaSituacao:
                case Servicos.MDFePedidoConsultaSituacao:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Sit_ERR;
                    break;

                case Servicos.CTeConsultaStatusServico:
                case Servicos.NFeConsultaStatusServico:
                case Servicos.MDFeConsultaStatusServico:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Sta_ERR;
                    break;

                case Servicos.NFePedidoSituacaoLote:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.ProRec_ERR;
                    break;

                case Servicos.ConsultaCadastroContribuinte:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.ConsCad).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.ConsCad_ERR;
                    break;

                case Servicos.CTeEnviarSimp:
                case Servicos.CTeEnviarSinc:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Cte_ERR;
                    break;

                case Servicos.NFeMontarLoteUma:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Nfe_ERR;
                    break;

                case Servicos.MDFeEnviarSinc:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.MDFe_ERR;
                    break;

                case Servicos.NFeMontarLoteVarias:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.MontarLote).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.MontarLote_ERR;
                    break;

                case Servicos.NFeEnviarLote:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Rec_ERR;
                    break;

                case Servicos.CTeAssinarValidarEnvioEmLote:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Cte_ERR;
                    break;

                case Servicos.MDFeAssinarValidarEnvioEmLote:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.MDFe_ERR;
                    break;

                case Servicos.NFeAssinarValidarEnvioEmLote:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Nfe_ERR;
                    break;

                case Servicos.EventoRecepcao:
                case Servicos.CTeRecepcaoEvento:
                case Servicos.MDFeRecepcaoEvento:
                case Servicos.EventoEPEC:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Eve_ERR;
                    break;

                case Servicos.EventoCCe:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.EnvCCe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.retEnvCCe_ERR;
                    break;

                case Servicos.EventoManifestacaoDest:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.EnvManifestacao).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.retManifestacao_ERR;
                    break;

                case Servicos.EventoCancelamento:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.EnvCancelamento).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.retCancelamento_ERR;
                    break;

                #endregion NFe / CTe / MDFe

                #region NFSe

                case Servicos.NFSeRecepcionarLoteRps:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.RetEnvLoteRps_ERR;
                    break;

                case Servicos.NFSeConsultarSituacaoLoteRps:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitLoteRps).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.SitLoteRps_ERR;
                    break;

                case Servicos.NFSeConsultarPorRps:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.SitNfseRps_ERR;
                    break;

                case Servicos.NFSeConsultar:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.SitNfse_ERR;
                    break;

                case Servicos.NFSeConsultarLoteRps:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.LoteRps_ERR;
                    break;

                case Servicos.NFSeCancelar:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.CanNfse_ERR;
                    break;

                case Servicos.NFSeConsultarConvenioMunicipal:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedConvenio).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Convenio_ERR;
                    break;

                case Servicos.NFSeConsultarAliquotasMunicipais:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedAliquotas).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Aliquotas_ERR;
                    break;

                case Servicos.NFSeConsultarHistoricoAliquotasMunicipais:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedHistorico).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Historico_ERR;
                    break;

                case Servicos.NFSeConsultarRegimesEspeciaisMunicipais:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedRegimes).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Regimes_ERR;
                    break;

                case Servicos.NFSeConsultarRetencoesMunicipais:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedRetencoes).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Retencoes_ERR;
                    break;

                case Servicos.NFSeConsultarBeneficioMunicipal:
                    extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedBeneficio).EnvioXML;
                    extRetERR = Propriedade.ExtRetorno.Beneficio_ERR;
                    break;

                #endregion NFSe

                #region Diversos

                //case Servicos.UniNFeAlterarConfiguracoes:
                //case Servicos.AssinarValidar:
                //case Servicos.UniNFeConsultaInformacoes:
                case Servicos.NFeConverterTXTparaXML:

                //case Servicos.EmProcessamento:
                //case Servicos.NFeGerarChave:
                case Servicos.UniNFeLimpezaTemporario:

                    //Não tem definição pois não gera arquivo .ERR
                    break;

                #endregion Diversos

                default:
                    if (arquivo.EndsWith(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML))
                    {
                        extRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML;
                        extRetERR = Propriedade.ExtRetorno.Sit_ERR;
                    }
                    else
                    {
                        //Como não foi possível identificar o tipo do servico vou mudar somente a extensão para .err pois isso pode acontecer caso exista erro na estrutura do XML.
                        //Renan - 05/03/2014
                        extRet = ".xml";
                        extRetERR = ".err";
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(extRet))
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(arquivo, extRet, extRetERR, ex, erroPadrao, true);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 02/06/2011
                }
            }
        }

        #endregion GravaErroERP()

        #region ConsultaGeral()

        protected void ConsultarGeral(string arquivo)
        {
            var arq = arquivo.ToLower().Trim();

            if (arq.IndexOf(Propriedade.Extensao(Propriedade.TipoEnvio.ConsCertificado).EnvioXML) >= 0)
            {
                ConsultaCertificados(arquivo);
            }
        }

        protected void ConsultaCertificados(string arquivo)
        {
            var oConfig = new ConfiguracaoApp();
            oConfig.CertificadosInstalados(arquivo);
        }

        #endregion ConsultaGeral()

        #region ConsultaDFe()

        /// <summary>
        /// Executa o processo de consulta DFe das empresas cadastradas no UniNFe e já deixa tudo disponibilizado para o ERP.
        /// </summary>
        public void ConsultaDFe()
        {
            var hasAll = false;

            while (true)
            {
                for (var i = 0; i < Empresas.Configuracoes.Count; i++)
                {
                    if (Empresas.Configuracoes[i].Servico != TipoAplicativo.Nfe &&
                        Empresas.Configuracoes[i].Servico != TipoAplicativo.Cte &&
                        Empresas.Configuracoes[i].Servico != TipoAplicativo.Todos)
                    {
                        continue;
                    }

                    var gerarXML = new GerarXML(i);
                    gerarXML.XMLDistribuicaoDFe(Servicos.DFeEnviar, Empresas.Configuracoes[i].AmbienteCodigo, Empresas.Configuracoes[i].UnidadeFederativaCodigo, "1.01", Empresas.Configuracoes[i].CNPJ, "000000000000000");

                    hasAll = true;
                }
                if (hasAll)
                {
                    Thread.Sleep(720000); //Dorme por 12 minutos, para atender o problema do consumo indevido da SEFAZ
                }
                else
                {
                    break;
                }
            }
        }

        #endregion ConsultaDFe()

        #region Definir Proxy

        private IWebProxy DefinirProxy()
        {
            IWebProxy result = null;

            if (ConfiguracaoApp.Proxy)
            {
                result = Unimake.Net.Utility.GetProxy(ConfiguracaoApp.ProxyServidor,
                    ConfiguracaoApp.ProxyUsuario,
                    ConfiguracaoApp.ProxySenha,
                    ConfiguracaoApp.ProxyPorta,
                    ConfiguracaoApp.DetectarConfiguracaoProxyAuto);
            }

            return result;
        }

        #endregion Definir Proxy
    }
}