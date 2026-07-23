using NFe.Components;
using NFe.Settings;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;

namespace NFe.Service
{
    internal static class DiagnosticoDisponibilidadeDFeHelper
    {
        public static void Gravar(int emp, Configuracao configuracao, string nomeArquivoXML, string extensaoEnvio)
        {
            if (configuracao == null)
            {
                return;
            }

            try
            {
                var diagnostico = new DiagnosticoDisponibilidadeDFe(configuracao);
                var resultado = diagnostico.ObterDiagnosticoPassivo();

                if (resultado.Status != StatusDisponibilidade.Operacional)
                {
                    resultado = diagnostico.Executar();
                }

                var nomeArquivo = Functions.ExtrairNomeArq(nomeArquivoXML, extensaoEnvio) +
                    Propriedade.ExtRetorno.DiagnosticoDisponibilidadeDFe;
                var caminhoArquivo = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, nomeArquivo);

                var configuracaoXml = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    OmitXmlDeclaration = false
                };

                using (var writer = XmlWriter.Create(caminhoArquivo, configuracaoXml))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("DiagnosticoDisponibilidadeDFe");
                    writer.WriteElementString("TipoDFe", resultado.TipoDFe.ToString());
                    writer.WriteElementString("UF", resultado.UFBrasil.ToString());
                    writer.WriteElementString("TipoAmbiente", resultado.TipoAmbiente.ToString());
                    writer.WriteElementString("Inicio", resultado.Inicio.ToString("o", CultureInfo.InvariantCulture));
                    writer.WriteElementString("DuracaoTotalMilissegundos", resultado.DuracaoTotalMilissegundos.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("Status", resultado.Status.ToString());
                    writer.WriteElementString("OrigemProvavel", resultado.OrigemProvavel.ToString());
                    writer.WriteElementString("Descricao", resultado.Descricao);
                    writer.WriteStartElement("Sondas");

                    for (var i = 0; i < resultado.Sondas.Count; i++)
                    {
                        var sonda = resultado.Sondas.GetItem(i);
                        writer.WriteStartElement("Sonda");
                        writer.WriteElementString("Servico", sonda.Servico);
                        writer.WriteElementString("Endpoint", sonda.Endpoint);
                        writer.WriteElementString("Protocolo", sonda.Protocolo);
                        writer.WriteElementString("Fonte", sonda.Fonte.ToString());
                        writer.WriteElementString("DataHora", sonda.DataHora.ToString("o", CultureInfo.InvariantCulture));
                        writer.WriteElementString("IdadeSegundos", sonda.IdadeSegundos.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("Status", sonda.Status.ToString());
                        writer.WriteElementString("TipoFalha", sonda.TipoFalha.ToString());
                        writer.WriteElementString("DuracaoMilissegundos", sonda.DuracaoMilissegundos.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("HttpStatusCode", sonda.HttpStatusCode.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("CStat", sonda.CStat.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("XMotivo", sonda.XMotivo);
                        writer.WriteElementString("Excecao", sonda.Excecao);
                        writer.WriteElementString("DoCache", XmlConvert.ToString(sonda.DoCache));
                        writer.WriteElementString("Essencial", XmlConvert.ToString(sonda.Essencial));
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Auxiliar.WriteLog("Não foi possível gravar o diagnóstico de disponibilidade do DFe. " + ex.GetAllMessages(), true);
                }
                catch
                {
                    //O diagnóstico não pode interromper ou alterar o retorno do serviço para o ERP.
                }
            }
        }
    }
}
