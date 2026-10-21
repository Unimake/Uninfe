using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Text;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFeABI;
using AutorizacaoSinc = Unimake.Business.DFe.Servicos.NFeABI.AutorizacaoSinc;
using XmlNFeABI = Unimake.Business.DFe.Xml.NFeABI.NFeABI;

namespace NFe.Service.NFeABI
{
    public class TaskNFeABIRecepcaoSinc : TaskAbst
    {
        internal static Action<AutorizacaoSinc> ExecutarAutorizacao;

        public TaskNFeABIRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.NFeABIAutorizacaoSinc;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var empresa = Empresas.Configuracoes[emp];
            var emProcessamento = Path.Combine(empresa.PastaXmlEnviado,
                PastaEnviados.EmProcessamento.ToString(), Path.GetFileName(NomeArquivoXML));
            var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI);
            Configuracao configuracao = null;
            var finalizandoAutorizacao = false;

            try
            {
                var xml = new XmlNFeABI().LerXML<XmlNFeABI>(ConteudoXML);
                if(xml.InfNFeABI.InfRespTec == null &&
                    (!string.IsNullOrEmpty(empresa.RespTecCNPJ) || !string.IsNullOrEmpty(empresa.RespTecEmail) ||
                     !string.IsNullOrEmpty(empresa.RespTecTelefone) || !string.IsNullOrEmpty(empresa.RespTecXContato)))
                {
                    xml.InfNFeABI.InfRespTec = new InfRespTec
                    {
                        CNPJ = empresa.RespTecCNPJ,
                        Email = empresa.RespTecEmail,
                        Fone = empresa.RespTecTelefone,
                        XContato = empresa.RespTecXContato
                    };
                    // A alteração do conteúdo fiscal exige uma nova assinatura pela DLL.
                    xml.Signature = null;
                }

                configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = empresa.AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.NFeABI,
                    CertificadoDigital = empresa.X509Certificado,
                    ColetarTelemetriaDisponibilidade = true
                };
                ConfiguracaoApp.AplicarConfiguracaoProxy(configuracao);

                using(var autorizacao = new AutorizacaoSinc(xml, configuracao))
                {
                    if(ExecutarAutorizacao == null) autorizacao.Executar();
                    else ExecutarAutorizacao(autorizacao);

                    ConteudoXML = autorizacao.ConteudoXMLAssinado;
                    empresa.CriarSubPastaEnviado();
                    File.WriteAllText(emProcessamento, ConteudoXML.OuterXml, new UTF8Encoding(false));

                    vStrXmlRetorno = autorizacao.RetornoWSString;
                    oGerarXML.XmlRetorno(extensao.EnvioXML, extensao.RetornoXML, vStrXmlRetorno);

                    var resultado = autorizacao.Result;
                    if(resultado.CStat == 100 || resultado.CStat == 150)
                    {
                        finalizandoAutorizacao = true;
                        foreach(var processado in autorizacao.NFeABIProcResults.Values)
                        {
                            FinalizacaoNFeABI.Finalizar(emp, emProcessamento, ConteudoXML, processado.GerarXML().OuterXml);
                        }
                    }
                    else if(resultado.CStat > 0)
                    {
                        oAux.MoveArqErro(emProcessamento);
                        if(empresa.DocumentosRejeitados)
                        {
                            new SendMessageToWhatsApp(emp).AlertNotification(
                                "Rejeição: " + resultado.CStat.ToString("000") + "-" + resultado.XMotivo.Trim(),
                                "UNINFE - NF-e ABI rejeitada");
                        }
                    }

                    Functions.DeletarArquivo(NomeArquivoXML);
                }
            }
            catch(Exception ex)
            {
                try
                {
                    var arquivo = NomeArquivoXML;
                    if(File.Exists(emProcessamento))
                    {
                        arquivo = emProcessamento;
                        if(File.Exists(NomeArquivoXML)) TFunctions.MoveArqErro(NomeArquivoXML);
                    }
                    // Falhas no arquivamento não devem retirar o assinado de EmProcessamento.
                    TFunctions.GravarArqErroServico(arquivo, extensao.EnvioXML, extensao.RetornoERR,
                        ex, ErroPadrao.ErroNaoDetectado, !finalizandoAutorizacao);
                }
                catch(Exception erro)
                {
                    Auxiliar.WriteLog("Falha ao gravar o retorno de erro da autorização da NF-e ABI. " + erro, false);
                }
            }
            finally
            {
                DiagnosticoDisponibilidadeDFeHelper.Gravar(emp, configuracao, NomeArquivoXML, extensao.EnvioXML);
            }
        }
    }
}
