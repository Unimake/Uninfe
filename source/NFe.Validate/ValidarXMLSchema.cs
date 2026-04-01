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
            var status = (int)resultado.StatusValidacao;
            var mensagem = resultado.MensagemRetorno;

            //Definir o nome do arquivo de retorno
            var arquivoRetorno = Functions.ExtrairNomeArq(arquivo, ".xml") + "-ret.xml";

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement("Validacao",
                new XElement("cStat", status),
                new XElement("xMotivo", mensagem)));
            xml.Save(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + arquivoRetorno);


            if (status == (int)StatusValidacao.Validado)
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

            var certificadoDigital = Empresas.Configuracoes[emp].X509Certificado;

            // Busca o código da UF para verificar o padrão, caso seja uma NFSe
            PadraoNFSe padraoNFSe = Functions.BuscaPadraoNFSe(Empresas.Configuracoes[emp].UnidadeFederativaCodigo);

            // Verifica o ambiente para validar o XML, caso exista alguma regra dependendo do ambiente
            var tipoAmbiente = Empresas.Configuracoes[emp].AmbienteCodigo == (int)TipoAmbiente.Homologacao ? TipoAmbiente.Homologacao : TipoAmbiente.Producao;

            var respTecnico = new RespTecnico(Empresas.Configuracoes[emp].RespTecCNPJ,
            Empresas.Configuracoes[emp].RespTecXContato,
            Empresas.Configuracoes[emp].RespTecEmail,
            Empresas.Configuracoes[emp].RespTecTelefone,
            Empresas.Configuracoes[emp].RespTecIdCSRT,
            Empresas.Configuracoes[emp].RespTecCSRT);

            #endregion

            AdicionarResponsavelTecnico(emp, respTecnico, xmlDoc);

            var resultadoValidacao = Validar(xmlDoc, certificadoDigital, tipoAmbiente, padraoNFSe);

            //Verifica se é NFSe para gravar o XML assinado/ XML de retorno corretamente
            var isNFSe = padraoNFSe != PadraoNFSe.None;

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

        private static void AdicionarResponsavelTecnico(int emp, RespTecnico respTecnico, XmlDocument xmlDoc)
        {
            respTecnico.AdicionarResponsavelTecnico(xmlDoc);
        }


        /// <summary>
        /// Valida XML
        /// </summary>
        /// <param name="xmlDoc">Arquivo para ser validado</param>
        /// <param name="certificadoDigital">Certificado digital</param>
        /// <param name="tipoAmbiente">Tipo ambiente</param>
        /// <param name="padraoNFSe">Padrão caso for NFSe</param>
        /// <returns></returns>
        private static ResultadoValidacao Validar(XmlDocument xmlDoc, X509Certificate2 certificado,TipoAmbiente tipoAmbiente, PadraoNFSe padraoNFSe)
        {
            var validar = new ValidarEstruturaXML();

            return validar.ValidarServico(xmlDoc, certificado, tipoAmbiente, padraoNFSe);
        }

    }
}
