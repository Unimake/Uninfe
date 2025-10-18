using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.CTe;
using Unimake.Business.DFe.Xml.CTe;

namespace NFe.Service
{
    public class TaskDFeRecepcaoCTe : TaskDFeRecepcao
    {
        public TaskDFeRecepcaoCTe(string arquivo)
            : base(arquivo) => Servico = Servicos.CTeDistribuicaoDFe;

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            ExtEnvioDFe = Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioXML;
            ExtEnvioDFeTXT = Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioTXT;
            ExtRetornoDFe = Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).RetornoXML;
            ExtRetEnvDFe_ERR = Propriedade.ExtRetorno.retEnvDFeCTe_ERR;

            try
            {
                if (!vXmlNfeDadosMsgEhXML)
                {
                    // -------------------------------------------
                    // Estrutura do TXT
                    // -------------------------------------------
                    // versao|1.00
                    // tpAmb|1
                    // cUFAutor|35
                    // CNPJ|
                    //  ou
                    // CPF|
                    // ultNSU|123456789012345
                    //  ou
                    // NSU|123456789012345
                    // -------------------------------------------
                    var cLinhas = Functions.LerArquivo(NomeArquivoXML);

                    var xml = new DistDFeInt();

                    foreach (var item in cLinhas)
                    {
                        var conteudo = item.Split("|".ToCharArray());
                        var nomeTag = conteudo[0].ToLower();
                        var conteudoTag = conteudo[1];

                        switch (nomeTag)
                        {
                            case "versao":
                                xml.Versao = conteudoTag;
                                break;

                            case "tpamb":
                                xml.TpAmb = (Unimake.Business.DFe.Servicos.TipoAmbiente)Convert.ToInt32(conteudoTag);
                                break;

                            case "cufautor":
                                xml.CUFAutor = (UFBrasil)Convert.ToInt32(conteudoTag);
                                break;

                            case "cnpj":
                                xml.CNPJ = conteudoTag;
                                break;

                            case "cpf":
                                xml.CPF = conteudoTag;
                                break;

                            case "ultnsu":
                                xml.DistNSU = new DistNSU
                                {
                                    UltNSU = conteudoTag
                                };
                                break;

                            case "nsu":
                                xml.ConsNSU = new ConsNSU
                                {
                                    NSU = conteudoTag
                                };
                                break;
                        }
                    };

                    var fileXML = Path.GetFileNameWithoutExtension(NomeArquivoXML) + ".xml";

                    if (NomeArquivoXML.IndexOf(Empresas.Configuracoes[emp].PastaValidar, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        fileXML = Path.Combine(Empresas.Configuracoes[emp].PastaValidar, fileXML);
                    }

                    oGerarXML.RecepcaoDFe(fileXML, xml.GerarXML().OuterXml);
                }
                else
                {
                    var xml = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<DistDFeInt>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                        TipoDFe = TipoDFe.CTe,
                        TipoAmbiente = xml.TpAmb,
                        CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                    };

                    if (ConfiguracaoApp.Proxy)
                    {
                        configuracao.HasProxy = true;
                        configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                        configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                        configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                    }

                    var distribuicaoDFe = new DistribuicaoDFe(xml, configuracao);
                    distribuicaoDFe.Executar();

                    vStrXmlRetorno = distribuicaoDFe.RetornoWSString;

                    LeRetornoDFe(emp, distribuicaoDFe.RetornoWSXML);

                    XmlRetorno(ExtEnvioDFe, ExtRetornoDFe);
                }
            }
            catch (Exception ex)
            {
                WriteLogError(ex);
            }
            finally
            {
                try
                {
                    //Deletar o arquivo de solicitação do serviço
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                }
            }
        }
    }
}