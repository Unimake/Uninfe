using NFe.Components;
using NFe.Settings;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFCom;
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


        public static ResultadoValidacao Validar(string arquivoXML, int emp, bool retornoArquivo)
        {

            #region variáveis validação
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(arquivoXML);

            var configuracao = new Configuracao
            {
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = Empresas.Configuracoes[emp].AmbienteCodigo == (int)TipoAmbiente.Homologacao ? TipoAmbiente.Homologacao : TipoAmbiente.Producao,
                CSC = Empresas.Configuracoes[emp].IdentificadorCSC,
                CodigoUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo,
                CSCIDToken = Convert.ToInt32((string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC) ? "0" : Empresas.Configuracoes[emp].TokenCSC)),
                PadraoNFSe = Functions.BuscaPadraoNFSe(Empresas.Configuracoes[emp].UnidadeFederativaCodigo)
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
                if (resultadoValidacao.Validado)
                {
                    GravarXMLValidado(arquivoXML, emp, resultadoValidacao.XmlAssinado, isNFSe);
                    GravarXMLRetornoValidacao(arquivoXML, resultadoValidacao, emp, isNFSe);

                }
                else
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
            var modeloDoc = xmlDoc.GetElementsByTagName("mod")[0].InnerText;

            if (tipoDFe == "NFe" || tipoDFe == "enviNFe")
            {
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
