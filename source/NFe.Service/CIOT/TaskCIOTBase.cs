using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.CIOT
{
    public abstract class TaskCIOTBase : TaskAbst
    {
        protected TaskCIOTBase(string arquivo)
        {
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        protected abstract Propriedade.TipoEnvio TipoEnvioXML { get; }

        /// <summary>
        /// Cria o objeto de configuração para consumir o serviço
        /// </summary>
        /// <param name="emp">Identificador da empresa</param>
        /// <returns>Objeto de configuração para o serviço</returns>
        protected Configuracao CriarConfiguracao(int emp)
        {
            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = TipoDFe.CIOT,
                TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoUF = (int)UFBrasil.AN,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
            };

            if (ConfiguracaoApp.Proxy)
            {
                configuracao.HasProxy = true;
                configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
            }

            return configuracao;
        }

        /// <summary>
        /// Grava o XML retornado pelo web-service na pasta de Retorno, utilizando o mesmo nome do arquivo de envio, porém com a extensão de retorno
        /// </summary>
        protected void GravarRetorno()
        {
            oGerarXML.XmlRetorno(Propriedade.Extensao(TipoEnvioXML).EnvioXML, Propriedade.Extensao(TipoEnvioXML).RetornoXML, vStrXmlRetorno);

            if (File.Exists(NomeArquivoXML))
            {
                File.Delete(NomeArquivoXML);
            }
        }

        /// <summary>
        /// Salvar o arquivo da DCe assinado na pasta EmProcessamento
        /// </summary>
        /// <param name="emp">Código da empresa</param>
        /// <param name="arqEmProcessamento">Onde será salvo o XML assinado</param>
        /// <param name="nomeTag">Nome da tag que abre o XML</param>
        protected void SalvarArquivoEmProcessamento(int emp, string arqEmProcessamento, string nomeTag)
        {
            Empresas.Configuracoes[emp].CriarSubPastaEnviado();

            using (var sw = File.CreateText(arqEmProcessamento))
            {
                sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + ConteudoXML.GetElementsByTagName(nomeTag)[0].OuterXml);
            }
        }

        /// <summary>
        /// Grava o erro ocorrido durante o processamento do CIOT
        /// </summary>
        /// <param name="ex">Exceção ocorrida</param>
        protected void GravarErro(Exception ex)
        {
            try
            {
                TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(TipoEnvioXML).EnvioXML, Propriedade.Extensao(TipoEnvioXML).RetornoERR, ex);
            }
            catch { }
        }

        protected void DeletarArquivo()
        {
            try
            {
                Functions.DeletarArquivo(NomeArquivoXML);
            }
            catch { }
        }
    }
}
