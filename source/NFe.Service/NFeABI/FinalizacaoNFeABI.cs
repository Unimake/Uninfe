using NFe.Components;
using NFe.Settings;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace NFe.Service.NFeABI
{
    /// <summary>
    /// Leitura e arquivamento compartilhados pela autorização e pela recuperação via consulta.
    /// </summary>
    internal static class FinalizacaoNFeABI
    {
        internal static string LocalizarArquivo(int emp, string chave)
        {
            var pasta = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado, PastaEnviados.EmProcessamento.ToString());
            if(!Directory.Exists(pasta)) return null;

            var nome = new FluxoNfe().LerTag("NFeABI" + chave, FluxoNfe.ElementoFixo.ArqNFe);
            if(string.IsNullOrEmpty(nome)) nome = chave + Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML;
            var candidato = Path.Combine(pasta, Path.GetFileName(nome));
            if(File.Exists(candidato) && ContemChave(candidato, chave)) return candidato;

            foreach(var arquivo in Directory.GetFiles(pasta, "*" + Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML))
            {
                if(ContemChave(arquivo, chave)) return arquivo;
            }
            return null;
        }

        private static bool ContemChave(string arquivo, string chave)
        {
            var xml = new XmlDocument();
            xml.Load(arquivo);
            var inf = xml.GetElementsByTagName("infNFeABI", NFeStrConstants.NAME_SPACE_NFEABI);
            return inf.Count > 0 && ((XmlElement)inf[0]).GetAttribute("Id") == "NFeABI" + chave;
        }

        internal static string CriarProcessado(XmlDocument documento, XmlElement protocolo)
        {
            var processado = new XmlDocument();
            var raiz = processado.CreateElement("nfeabiProc", NFeStrConstants.NAME_SPACE_NFEABI);
            raiz.SetAttribute("versao", ((XmlElement)documento.GetElementsByTagName("infNFeABI")[0]).GetAttribute("versao"));
            processado.AppendChild(raiz);
            raiz.AppendChild(processado.ImportNode(documento.DocumentElement, true));
            raiz.AppendChild(processado.ImportNode(protocolo, true));
            return processado.OuterXml;
        }

        internal static void Finalizar(int emp, string arquivo, XmlDocument documento, string processado)
        {
            var empresa = Empresas.Configuracoes[emp];
            var data = DateTimeOffset.Parse(documento.GetElementsByTagName("dhEmi")[0].InnerText, CultureInfo.InvariantCulture).DateTime;
            var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML;
            var nomeProc = Functions.ExtrairNomeArq(arquivo, extensao) + Propriedade.ExtRetorno.ProcNFeABI;
            var arquivoProc = Path.Combine(Path.GetDirectoryName(arquivo), nomeProc);
            var procAutorizado = Path.Combine(empresa.PastaXmlEnviado, PastaEnviados.Autorizados.ToString(),
                empresa.DiretorioSalvarComo.ToString(data), nomeProc);

            // O original só sai de EmProcessamento depois que o processado estiver arquivado.
            if(!File.Exists(procAutorizado))
            {
                File.WriteAllText(arquivoProc, processado, new UTF8Encoding(false));
                TFunctions.MoverArquivo(arquivoProc, PastaEnviados.Autorizados, data);
            }
            if(!File.Exists(procAutorizado))
            {
                throw new IOException("Não foi possível arquivar o XML processado da NF-e ABI.");
            }

            if(File.Exists(arquivo))
            {
                TFunctions.MoverArquivo(arquivo, empresa.SalvarSomenteXMLDistribuicao
                    ? PastaEnviados.Originais : PastaEnviados.Autorizados, data);
            }
            if(File.Exists(arquivoProc)) Functions.DeletarArquivo(arquivoProc);

            var chave = ((XmlElement)documento.GetElementsByTagName("infNFeABI")[0]).GetAttribute("Id");
            new FluxoNfe().ExcluirNfeFluxo(chave);
        }
    }
}
