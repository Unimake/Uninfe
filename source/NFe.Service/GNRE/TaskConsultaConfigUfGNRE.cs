using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.GNRE;
using Unimake.Business.DFe.Xml.GNRE;

namespace NFe.Service.GNRE
{
    public class TaskConsultaConfigUfGNRE : TaskAbst
    {
        public TaskConsultaConfigUfGNRE(string arquivo)
        {
            Servico = Servicos.ConsultaConfigUfGNRE;

            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                    Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).RetornoERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + (new FileInfo(NomeArquivoXML).Name));
                
                var xml = new TConsultaConfigUf();
                xml = xml.LerXML<TConsultaConfigUf>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.GNRE,
                    TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    CodigoUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo,
                    Servico = Unimake.Business.DFe.Servicos.Servico.GNREConsultaConfigUF,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo
                };

                var consultaConfigUF = new ConsultaConfigUF(xml, configuracao);
                consultaConfigUF.Executar();

                vStrXmlRetorno = consultaConfigUF.RetornoWSString;

                XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).RetornoXML);

                /// grava o arquivo no FTP
                var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                    Functions.ExtrairNomeArq(NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).RetornoXML);

                if(File.Exists(filenameFTP))
                {
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
                }
            }
            catch(Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.GNRE_consconfiguf).RetornoERR, ex);
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
