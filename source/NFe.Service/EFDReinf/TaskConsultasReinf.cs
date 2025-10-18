using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using XmlReinf = Unimake.Business.DFe.Xml.EFDReinf;
using ServicoReinf = Unimake.Business.DFe.Servicos.EFDReinf;

namespace NFe.Service
{
    public class TaskConsultasReinf : TaskAbst
    {
        public TaskConsultasReinf(string arquivo)
        {
            Servico = Servicos.ConsultasReinf;

            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            int emp = Empresas.FindEmpresaByThread();

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).RetornoERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.EFDReinf,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    Servico = Unimake.Business.DFe.Servicos.Servico.EFDReinfConsultaReciboEvento,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                var xmlConsultaRecibo = new XmlReinf.ReinfConsulta();
                xmlConsultaRecibo = xmlConsultaRecibo.LerXML<XmlReinf.ReinfConsulta>(ConteudoXML);

                var servicoConsultaRecibo = new ServicoReinf.ConsultaReciboEvento(xmlConsultaRecibo, configuracao);
                servicoConsultaRecibo.Executar();

                ConteudoXML = servicoConsultaRecibo.ConteudoXMLAssinado;

                vStrXmlRetorno = servicoConsultaRecibo.RetornoWSString;

                var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).EnvioXML;
                var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).RetornoXML;

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                ///
                /// grava o arquivo no FTP
                string filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).RetornoXML);
                if (File.Exists(filenameFTP))
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);

            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_cons).RetornoERR, ex);
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
    }
}