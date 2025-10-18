using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.DARE;

namespace NFe.Service.DARE
{
    public class TaskConsultasReceitasDARE : TaskAbst
    {
        public TaskConsultasReceitasDARE(string arquivo)
        {
            Servico = Servicos.ConsultaReceitasDARE;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaReceitasDARE).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaReceitasDARE).RetornoXML;

            try
            {
                var xmlConsultaReceita = new Receitas();

                xmlConsultaReceita = xmlConsultaReceita.LerXML<Receitas>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.DARE,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    Servico = Unimake.Business.DFe.Servicos.Servico.DAREReceita,
                    SchemaVersao = "1.00",
                    ApiKey = Empresas.Configuracoes[emp].SenhaWS
                };

                var receitaDARE = new Unimake.Business.DFe.Servicos.DARE.ReceitasDARE(xmlConsultaReceita, configuracao);
                receitaDARE.Executar();

                ConteudoXML = receitaDARE.ConteudoXMLAssinado;

                vStrXmlRetorno = receitaDARE.RetornoWSString;

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                /// grava o arquivo no FTP
                var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaReceitasDARE).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaReceitasDARE).RetornoXML);

                if (File.Exists(filenameFTP))
                {
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaReceitasDARE).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.ConsultaReceitasDARE).RetornoERR, ex);
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

        #endregion Execute
    }
}
