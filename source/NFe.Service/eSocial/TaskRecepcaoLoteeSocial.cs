using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class TaskRecepcaoLoteeSocial : TaskAbst
    {
        public TaskRecepcaoLoteeSocial(string arquivo)
        {
            Servico = Servicos.RecepcaoLoteeSocial;

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
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).RetornoERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                //Serializa o conteúdo do xml
                var recepcaoLoteESocial = new Unimake.Business.DFe.Xml.ESocial.ESocialEnvioLoteEventos();
                recepcaoLoteESocial = recepcaoLoteESocial.LerXML<Unimake.Business.DFe.Xml.ESocial.ESocialEnvioLoteEventos>(ConteudoXML);

                if (ConteudoXML.OuterXml.IndexOf("v_S_01_03_00") > 0)
                {
                    recepcaoLoteESocial.VersaoSchema = "v_S_01_03_00";
                }

                //Configuração mínima para o consumo do serviço
                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.ESocial,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                };

                //Envio do loteESocial
                var autorizacaoESocial = new Unimake.Business.DFe.Servicos.ESocial.EnviarLoteEventosESocial(recepcaoLoteESocial, configuracao);
                autorizacaoESocial.Executar();

                ConteudoXML = autorizacaoESocial.ConteudoXMLAssinado;
                vStrXmlRetorno = autorizacaoESocial.RetornoWSString;

                var retorno = autorizacaoESocial.Result.RetornoEnvioLoteEventos;

                MoverPastaProcessamento(retorno);

                var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).EnvioXML;
                var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).RetornoXML;

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                ///
                /// grava o arquivo no FTP
                string filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).RetornoXML);
                if (File.Exists(filenameFTP))
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);

                autorizacaoESocial.Dispose();
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).RetornoERR, ex);
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
                    TFunctions.MoveArqErro(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de cancelamento de NFe, infelizmente
                    //não posso fazer mais nada, o UniNFe vai tentar mandar o arquivo novamente para o webservice, pois ainda não foi excluido.
                    //Wandrey 31/08/2011
                }
            }
        }

        private void MoverPastaProcessamento(Unimake.Business.DFe.Xml.ESocial.Retorno.RetornoEnvioLoteEventos retorno)
        {
            var codigoResposta = retorno.Status.CdResposta.ToString();

            if (codigoResposta.Equals("201") && !String.IsNullOrEmpty(codigoResposta))
            {
                ConteudoXML.Save(NomeArquivoXML);

                var protocoloEnvio = retorno.DadosRecepcaoLote.ProtocoloEnvio.ToString();

                TFunctions.MoverArquivo(NomeArquivoXML, PastaEnviados.EmProcessamento, $"{protocoloEnvio}.xml");
            }
        }
    }
}