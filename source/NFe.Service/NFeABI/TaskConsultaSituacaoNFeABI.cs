using NFe.Components;
using NFe.Settings;
using System;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFeABI;
using ConsultaProtocolo = Unimake.Business.DFe.Servicos.NFeABI.ConsultaProtocolo;

namespace NFe.Service.NFeABI
{
    public class TaskConsultaSituacaoNFeABI : TaskAbst
    {
        internal static Action<ConsultaProtocolo> ExecutarConsulta;

        public TaskConsultaSituacaoNFeABI(string arquivo)
        {
            Servico = Servicos.NFeABIConsultaProtocolo;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            Configuracao configuracao = null;

            try
            {
                var xml = new ConsSitNFeABI().LerXML<ConsSitNFeABI>(ConteudoXML);
                configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.NFeABI,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    ColetarTelemetriaDisponibilidade = true
                };
                ConfiguracaoApp.AplicarConfiguracaoProxy(configuracao);

                using(var consulta = new ConsultaProtocolo(xml, configuracao))
                {
                    if(ExecutarConsulta == null) consulta.Executar();
                    else ExecutarConsulta(consulta);

                    vStrXmlRetorno = consulta.RetornoWSString;
                    XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).RetornoXML);
                    LerRetorno(xml, consulta.Result, emp);
                }
            }
            catch(Exception ex)
            {
                try
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML,
                        Propriedade.ExtRetorno.Sit_ERR, ex);
                }
                catch(Exception erro)
                {
                    Auxiliar.WriteLog("Falha ao gravar o retorno de erro da consulta de situação da NF-e ABI. " + erro, false);
                }
            }
            finally
            {
                DiagnosticoDisponibilidadeDFeHelper.Gravar(emp, configuracao, NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML);
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch(Exception ex)
                {
                    Auxiliar.WriteLog("Falha ao excluir o pedido de consulta de situação da NF-e ABI. " + ex, false);
                }
            }
        }

        private void LerRetorno(ConsSitNFeABI pedido, RetConsSitNFeABI retorno, int emp)
        {
            var empresa = Empresas.Configuracoes[emp];
            if(!Functions.ChaveDFePertenceEmpresa(pedido.ChNFeABI, empresa.CNPJ, empresa.UnidadeFederativaCodigo))
            {
                return;
            }

            var arquivo = FinalizacaoNFeABI.LocalizarArquivo(emp, pedido.ChNFeABI);
            if(arquivo == null) return;

            switch(retorno.CStat)
            {
                case 100:
                case 150:
                case 101: // Cancelada: o protocolo de autorização ainda permite recuperar o processado.
                    var xmlRetorno = new XmlDocument();
                    xmlRetorno.LoadXml(vStrXmlRetorno);
                    var ns = new XmlNamespaceManager(xmlRetorno.NameTable);
                    ns.AddNamespace("abi", NFeStrConstants.NAME_SPACE_NFEABI);
                    var protocolo = xmlRetorno.SelectSingleNode("/abi:retConsSitNFeABI/abi:protNFeABI", ns) as XmlElement;
                    var infProt = protocolo == null ? null : protocolo.SelectSingleNode("abi:infProt", ns);
                    if(infProt == null || infProt.SelectSingleNode("abi:chNFeABI", ns)?.InnerText != pedido.ChNFeABI ||
                        string.IsNullOrWhiteSpace(infProt.SelectSingleNode("abi:nProt", ns)?.InnerText))
                    {
                        return;
                    }

                    var status = infProt.SelectSingleNode("abi:cStat", ns)?.InnerText;
                    if(status != "100" && status != "150") return;

                    var documento = new XmlDocument();
                    documento.Load(arquivo);
                    if(empresa.CompararDigestValueDFeRetornadoSEFAZ)
                    {
                        var digestProtocolo = infProt.SelectSingleNode("abi:digVal", ns)?.InnerText;
                        var digestNota = documento.GetElementsByTagName("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
                        if(!string.IsNullOrEmpty(digestProtocolo) && digestNota.Count > 0 &&
                            !string.IsNullOrEmpty(digestNota[0].InnerText) && digestProtocolo != digestNota[0].InnerText)
                        {
                            oAux.MoveArqErro(arquivo);
                            throw new Exception("O valor do DigestValue da consulta situação é diferente do DigestValue da NF-e ABI.");
                        }
                    }

                    FinalizacaoNFeABI.Finalizar(emp, arquivo, documento,
                        FinalizacaoNFeABI.CriarProcessado(documento, protocolo));
                    break;

                case 216:
                case 217:
                    oAux.MoveArqErro(arquivo);
                    new FluxoNfe().ExcluirNfeFluxo("NFeABI" + pedido.ChNFeABI);
                    RemoverArqTemp("NFeABI" + pedido.ChNFeABI, emp);
                    break;
            }
        }
    }
}
