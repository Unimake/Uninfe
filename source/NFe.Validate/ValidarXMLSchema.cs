using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using static Unimake.Business.DFe.ValidarEstruturaXML;

namespace NFe.Validate
{
    public class ValidarXMLSchema
    {


        /// <summary>
        /// Gravar Retorno da Validação do XML.
        /// </summary>
        /// <param name="arquivo">Arquivo validado</param>
        /// <param name="resultado">Resultado da validação</param>
        /// <param name="emp">Indice da empresa</param>
        private static void GravarXMLRetornoValidacao(string arquivo, ResultadoValidacao resultado, int emp, bool isNFSe)
        {
            var status = resultado.StatusValidacao;
            var mensagem = resultado.MensagemRetorno;

            //Definir o nome do arquivo de retorno
            var arquivoRetorno = Functions.ExtrairNomeArq(arquivo, ".xml") + "-ret.xml";

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement("Validacao",
                new XElement("cStat", status),
                new XElement("xMotivo", mensagem)));
            xml.Save(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + arquivoRetorno);


            if (status == "1")
            {
                if (!arquivo.ToLower().Contains((Empresas.Configuracoes[emp].PastaXmlEmLote.Trim() + "\\temp").ToLower()) || isNFSe)
                {
                    File.Delete(arquivo);
                }

            }
            else
            {
                var arqErro = Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Functions.ExtrairNomeArq(arquivo, ".xml") + ".xml");
                if (File.Exists(arqErro))
                {
                    File.Delete(arqErro);
                }

                File.Move(arquivo, arqErro);
            }
        }



        /// <summary>
        /// Gravar XML assinado e validado.
        /// </summary>
        /// <param name="arquivoXML">caminho do arquivo</param>
        /// <param name="emp">índice da empresa</param>
        /// <param name="xmlSalvar">XMl assinado</param>
        private static void GravarXMLValidado(string arquivoXML, int emp, XmlDocument xmlSalvar, bool isNFSe)
        {
            if (arquivoXML.ToLower().Contains((Empresas.Configuracoes[emp].PastaXmlEmLote.Trim() + "\\temp").ToLower()) && !isNFSe)
            {
                //Gravar XML assinado
                var SW_2 = File.CreateText(arquivoXML);
                SW_2.Write(xmlSalvar.OuterXml);
                SW_2.Close();

            }
            else
            {
                var pasta = Empresas.Configuracoes[emp].PastaValidado;

                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }

                var arquivoNovo = Path.Combine(pasta, Path.GetFileName(arquivoXML));

                //Gravar XML assinado e validado na subpasta "Validados"
                var SW_2 = File.CreateText(arquivoNovo);
                SW_2.Write(xmlSalvar.OuterXml);
                SW_2.Close();
            }

        }


        public static ResultadoValidacao Validar(XmlDocument xmlDoc, int emp, bool retornoArquivo, string arquivoXML = null)
        {
            #region variáveis validação
            var codigoConfiguracao = Empresas.Configuracoes[emp].UnidadeFederativaCodigo;
            var padraoNFSe = Functions.BuscaPadraoNFSe(codigoConfiguracao);
            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = Empresas.Configuracoes[emp].AmbienteCodigo == (int)TipoAmbiente.Homologacao ? TipoAmbiente.Homologacao : TipoAmbiente.Producao,
                CSC = Empresas.Configuracoes[emp].IdentificadorCSC,
                CodigoUF = codigoConfiguracao,
                CodigoMunicipio = padraoNFSe == PadraoNFSe.None ? 0 : codigoConfiguracao,
                CSCIDToken = Convert.ToInt32((string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC) ? "0" : Empresas.Configuracoes[emp].TokenCSC)),
                PadraoNFSe = padraoNFSe
            };


            var respTecnico = new RespTecnico(Empresas.Configuracoes[emp].RespTecCNPJ,
            Empresas.Configuracoes[emp].RespTecXContato,
            Empresas.Configuracoes[emp].RespTecEmail,
            Empresas.Configuracoes[emp].RespTecTelefone,
            Empresas.Configuracoes[emp].RespTecIdCSRT,
            Empresas.Configuracoes[emp].RespTecCSRT);

            #endregion

            AdicionarResponsavelTecnico(xmlDoc, respTecnico);
            PrepararConfiguracaoQRCode(xmlDoc, configuracao, emp);

            var resultadoValidacao = Validar(xmlDoc, configuracao);

            //Verifica se é NFSe para gravar o XML assinado/ XML de retorno corretamente
            var isNFSe = configuracao.PadraoNFSe != PadraoNFSe.None;

            if (retornoArquivo)
            {
                //Caso seja necessário retornar é obrigatório que o arquivoXML tenha sido passado
                if (arquivoXML is null)
                {
                    throw new ArgumentNullException(nameof(arquivoXML), "O arquivo XML é necessário para gerar retorno");
                }

                if (resultadoValidacao.Validado)
                {
                    GravarXMLValidado(arquivoXML, emp, resultadoValidacao.XmlAssinado, isNFSe);
                    GravarXMLRetornoValidacao(arquivoXML, resultadoValidacao, emp, isNFSe);

                }
                else if (!(resultadoValidacao.StatusValidacao.Equals("5")))
                {
                    GravarXMLRetornoValidacao(arquivoXML, resultadoValidacao, emp, isNFSe);
                    new Auxiliar().MoveArqErro(arquivoXML);
                }

            }

            return resultadoValidacao;

        }


        /// <summary>
        /// Adicionar responsável técnico
        /// </summary>
        /// <param name="emp">Empresa</param>
        /// <param name="respTecnico">Objeto responsável técnico</param>
        /// <param name="xmlDoc">arquivo xml</param>
        private static void AdicionarResponsavelTecnico(XmlDocument xmlDoc, RespTecnico respTecnico)
        {
            respTecnico.AdicionarResponsavelTecnico(xmlDoc);
        }


        private static void PrepararConfiguracaoQRCode(XmlDocument xmlDoc, Configuracao config, int emp)
        {
            var tipoDFe = xmlDoc.DocumentElement?.Name;

            if (tipoDFe == "NFe" || tipoDFe == "enviNFe")
            {
                var modeloDoc = xmlDoc.GetElementsByTagName("mod")[0].InnerText;

                if (modeloDoc == ((int)ModeloDFe.NFCe).ToString())
                {
                    if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe != 2)
                    {
                        config.VersaoQRCodeNFCe = 3;
                    }

                    if (xmlDoc.GetElementsByTagName("qrCode").Count == 0 && Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                    {
                        if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].IdentificadorCSC.Trim()) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC))
                        {
                            throw new Exception("Para autorizar NFC-e é obrigatório informar nas configurações do UniNFe os campos CSC e IDToken do CSC.");
                        }
                    }

                    if (Empresas.Configuracoes[emp].VersaoQRCodeNFCe == 2)
                    {
                        config.CSC = Empresas.Configuracoes[emp].IdentificadorCSC;
                        config.CSCIDToken = Convert.ToInt32(
                            string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC) ? "0" : Empresas.Configuracoes[emp].TokenCSC
                        );
                    }
                }
            }
        }


        /// <summary>
        /// Valida XML
        /// </summary>
        /// <param name="xmlDoc">Arquivo para ser validado</param>
        /// <returns></returns>
        private static ResultadoValidacao Validar(XmlDocument xmlDoc, Configuracao configuracao)
        {
            var validar = new ValidarEstruturaXML();

            return validar.ValidarServico(xmlDoc, configuracao);
        }

    }
}
