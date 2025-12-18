using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
namespace NFe.Service.NFSe
{
    public class TaskConsultaSequenciaLoteNotaRPS : TaskAbst
    {
        public TaskConsultaSequenciaLoteNotaRPS(string arquivo)
        {
            Servico = Servicos.NFSeConsultaSequenciaLoteNotaRPS;
            NomeArquivoXML = arquivo;
        }

        #region Objeto com os dados do XML da consulta nfse

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s da consulta nfse
        /// </summary>
        private DadosPedSeqLoteNotaRPS dadosPedSeqLoteNotaRPS;

        #endregion Objeto com os dados do XML da consulta nfse

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                    Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSeqLoteNotaRPS).EnvioXML) + Propriedade.ExtRetorno.SeqLoteNotaRPS_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                dadosPedSeqLoteNotaRPS = new DadosPedSeqLoteNotaRPS(emp);

                //Criar objetos das classes dos serviços dos webservices do SEFAZ
                var padraoNFSe = Functions.BuscaPadraoNFSe(dadosPedSeqLoteNotaRPS.cMunicipio);

                ExecuteDLL(emp, dadosPedSeqLoteNotaRPS.cMunicipio, padraoNFSe);

            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSeqLoteNotaRPS).EnvioXML, Propriedade.ExtRetorno.SeqLoteNotaRPS_ERR, ex);
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


        private void ExecuteDLL(int emp, int municipio, PadraoNFSe padraoNFSe)
        {
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(NomeArquivoXML);

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedSeqLoteNotaRPS).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedSeqLoteNotaRPS).RetornoXML;
            var versaoXML = DefinirVersaoXML(municipio, conteudoXML, padraoNFSe);
            var servico = DefinirServico(municipio, conteudoXML, padraoNFSe, versaoXML);

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                TipoDFe = Unimake.Business.DFe.Servicos.TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = servico,
                SchemaVersao = versaoXML,
                MunicipioToken = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioSenha = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioUsuario = Empresas.Configuracoes[emp].UsuarioWS
            };

            switch (servico)
            {
                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRpsDisponivel:
                    var NFSeConsultarRpsDisponivel = new Unimake.Business.DFe.Servicos.NFSe.ConsultarRpsDisponivel(conteudoXML, configuracao);
                    NFSeConsultarRpsDisponivel.Executar();

                    vStrXmlRetorno = NFSeConsultarRpsDisponivel.RetornoWSString;

                    NFSeConsultarRpsDisponivel.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeConsultarSequenciaLoteNotaRPS:
                    var NFSeConsultaLoteNotaRPS = new Unimake.Business.DFe.Servicos.NFSe.ConsultarSequenciaLoteNotaRPS(conteudoXML, configuracao);
                    NFSeConsultaLoteNotaRPS.Executar();

                    vStrXmlRetorno = NFSeConsultaLoteNotaRPS.RetornoWSString;

                    NFSeConsultaLoteNotaRPS.Dispose();
                    break;
            }

            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSeqLoteNotaRPS).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedSeqLoteNotaRPS).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }

        private Unimake.Business.DFe.Servicos.Servico DefinirServico(int municipio, XmlDocument doc, PadraoNFSe padraoNFSe, string versaoXML)
        {
            var result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarSequenciaLoteNotaRPS;

            switch (padraoNFSe)
            {
                case PadraoNFSe.BAUHAUS:
                    result = Unimake.Business.DFe.Servicos.Servico.NFSeConsultarRpsDisponivel;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município
        /// </summary>
        /// <param name="codMunicipio">Código do município para onde será enviado o XML</param>
        /// <param name="xmlDoc">Conteúdo do XML da NFSe</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        /// <returns>Retorna a versão do XML que está sendo enviado para o município de acordo com o Padrão/Município</returns>
        private string DefinirVersaoXML(int codMunicipio, XmlDocument xmlDoc, PadraoNFSe padraoNFSe)
        {
            var versaoXML = "0.00";

            switch (padraoNFSe)
            {

                case PadraoNFSe.BAUHAUS:
                case PadraoNFSe.TECNOSISTEMAS:
                case PadraoNFSe.DSF:
                    versaoXML = "1.00";
                    break;

                default:
                    throw new Exception("Padrão de NFSe " + padraoNFSe.ToString() + " não é válido para Consulta de Sequência de Lote via RPS.");
            }

            return versaoXML;
        }

    }
}