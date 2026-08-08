using NFe.Components;
using NFe.Settings;
using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class TFunctions
    {
        #region GravarArqErroServico()

        /// <summary>
        /// Grava um arquivo texto com os erros ocorridos durante as operações para que o ERP possa tratá-los
        /// </summary>
        /// <param name="arquivo">Nome do arquivo que está sendo processado</param>
        /// <param name="finalArqEnvio">string final do nome do arquivo que é para ser substituida na gravação do arquivo de erro</param>
        /// <param name="finalArqErro">string final do nome do arquivo que é para ser utilizado no nome do arquivo de erro</param>
        /// <param name="exception">Exception gerada</param>
        public static void GravarArqErroServico(string arquivo, string finalArqEnvio, string finalArqErro, Exception exception) => GravarArqErroServico(arquivo, finalArqEnvio, finalArqErro, exception, ErroPadrao.ErroNaoDetectado, true);

        #endregion GravarArqErroServico()

        #region GravarArqErroServico()

        /// <summary>
        /// Grava um arquivo texto com um erros ocorridos durante as operações para que o ERP possa tratá-los
        /// </summary>
        /// <param name="arquivo">Nome do arquivo que está sendo processado</param>
        /// <param name="finalArqEnvio">string final do nome do arquivo que é para ser substituida na gravação do arquivo de Erro</param>
        /// <param name="finalArqErro">string final do nome do arquivo que é para ser utilizado no nome do arquivo de erro</param>
        /// <param name="exception">Exception gerada</param>
        /// <param name="moveArqErro">Move o arquivo informado no parametro "arquivo" para a pasta de XML com ERRO</param>
        /// <param name="nomeArqRetorno">Nome do arquivo de retorno, caso não queira gravar um nome diferente do informado no parametro "arquivo"</param>
        /// <param name="erroPadrao">Informe o erro padrão do UniNFe</param>
        public static void GravarArqErroServico(string arquivo, string finalArqEnvio, string finalArqErro, Exception exception, ErroPadrao erroPadrao, bool moveArqErro, string nomeArqRetorno = "")
        {
            var ex = exception.GetLastException();
            var erroMessage = MontaStringErro(ex.Message, ex.StackTrace, ex.Source, ex.GetType().ToString(), ex.TargetSite.ToString(), ex.GetHashCode().ToString(), erroPadrao);
            GravarArqErroServico(arquivo, finalArqEnvio, finalArqErro, erroMessage, moveArqErro, nomeArqRetorno);

            EnviarMB(exception);
        }


        /// <summary>
        /// Envia mensagem para o uMessage
        /// </summary>
        private static void EnviarMB(Exception exception)
        {
            try
            {
                var emp = Empresas.FindEmpresaByThread();

                if (Empresas.Configuracoes[emp].ErrosUniNFe)
                {
                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                    sendMessageToWhatsApp.AlertNotification(exception.GetAllMessages(), "UNINFE - falha no envio dos XML");
                }
            }
            catch { }
        }

        /// <summary>
        /// Grava um arquivo texto com um erros ocorridos durante as operações para que o ERP possa tratá-los
        /// </summary>
        /// <param name="arquivo">Nome do arquivo que está sendo processado</param>
        /// <param name="finalArqEnvio">string final do nome do arquivo que é para ser substituida na gravação do arquivo de Erro</param>
        /// <param name="finalArqErro">string final do nome do arquivo que é para ser utilizado no nome do arquivo de erro</param>
        /// <param name="erroMessage">Mensagem de erro ocorrida ou mensagem da exceção ocorrida</param>
        /// <param name="moveArqErro">Move o arquivo informado no parametro "arquivo" para a pasta de XML com ERRO</param>
        /// <param name="nomeArqRetorno">Nome do arquivo de retorno, caso não queira gravar um nome diferente do informado no parametro "arquivo"</param>
        private static void GravarArqErroServico(string arquivo, string finalArqEnvio, string finalArqErro, string erroMessage, bool moveArqErro, string nomeArqRetorno = "")
        {
            var emp = Empresas.FindEmpresaByThread();

            //Qualquer erro ocorrido o aplicativo vai mover o XML com falha da pasta de envio
            //para a pasta de XML´s com erros. Futuramente ele é excluido quando outro igual
            //for gerado corretamente.
            if (moveArqErro)
            {
                MoveArqErro(arquivo);
            }

            //Grava arquivo de ERRO para o ERP
            var pastaRetorno = Empresas.Configuracoes[emp].PastaXmlRetorno;
            var fi = new FileInfo(arquivo);
            if (fi.Directory.FullName.ToLower().EndsWith("geral\\temp"))
            {
                pastaRetorno = Propriedade.PastaGeralRetorno;
            }

            var arqErro = pastaRetorno + "\\" + Functions.ExtrairNomeArq((string.IsNullOrEmpty(nomeArqRetorno) ? arquivo : nomeArqRetorno), finalArqEnvio) + finalArqErro;

            try
            {
                // Gerar log do erro
                Auxiliar.WriteLog(erroMessage, true);
            }
            catch
            {
            }

            File.WriteAllText(arqErro, erroMessage);

            // grava o arquivo de erro no FTP
            new GerarXML(emp).XmlParaFTP(emp, arqErro);
        }

        #region MontaStringErro()

        /// <summary>
        /// Montar a string do erro da exception
        /// </summary>
        /// <param name="exception">Objeto da exception</param>
        /// <param name="erroPadrao">ErroPadrao</param>
        /// <returns>Retorna uma string com o erro ocorrido.</returns>
        private static string MontaStringErro(string message, string stackTrace, string source, string getType, string targetSite, string hashCode, ErroPadrao erroPadrao)
        {
            var erroMessage = string.Empty;

            erroMessage += "Versão UniNFe|" + Propriedade.Versao + " - " + Propriedade.DataHoraUltimaModificacaoAplicacao + "\r\n" +
                "ErrorCode|" + ((int)erroPadrao).ToString("0000000000") +
                "\r\n" +
                "Message|" + message +
                "\r\n" +
                "StackTrace|" + stackTrace +
                "\r\n" +
                "Source|" + source +
                "\r\n" +
                "Type|" + getType +
                "\r\n" +
                "TargetSite|" + targetSite +
                "\r\n" +
                "HashCode|" + hashCode;

            return erroMessage;
        }

        #endregion MontaStringErro()

        #endregion GravarArqErroServico()

        #region MoveArqErro

        /// <summary>
        /// Move arquivos XML com erro para uma pasta de xml´s com erro configurados no UniNFe.
        /// </summary>
        /// <param name="cArquivo">Nome do arquivo a ser movido para a pasta de XML´s com erro</param>
        /// <example>this.MoveArqErro(this.vXmlNfeDadosMsg)</example>
        public static void MoveArqErro(string Arquivo) => MoveArqErro(Arquivo, Path.GetExtension(Arquivo));

        #endregion MoveArqErro

        #region MoveArqErro()

        /// <summary>
        /// Move arquivos com a extensão informada e que está com erro para uma pasta de xml´s/arquivos com erro configurados no UniNFe.
        /// </summary>
        /// <param name="cArquivo">Nome do arquivo a ser movido para a pasta de XML´s com erro</param>
        /// <param name="ExtensaoArq">Extensão do arquivo que vai ser movido. Ex: .xml</param>
        /// <example>this.MoveArqErro(this.vXmlNfeDadosMsg, ".xml")</example>
        private static void MoveArqErro(string Arquivo, string ExtensaoArq)
        {
            var emp = Empresas.FindEmpresaByThread();

            if (File.Exists(Arquivo))
            {
                var oArquivo = new FileInfo(Arquivo);

                if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].PastaXmlErro) && Directory.Exists(Empresas.Configuracoes[emp].PastaXmlErro))
                {
                    var vNomeArquivo = Empresas.Configuracoes[emp].PastaXmlErro + "\\" + Functions.ExtrairNomeArq(Arquivo, ExtensaoArq) + ExtensaoArq;

                    Functions.Move(Arquivo, vNomeArquivo);

                    Auxiliar.WriteLog("O arquivo " + Arquivo + " foi movido para " + vNomeArquivo, true);
                }
                else
                {
                    //Antes estava deletando o arquivo, agora vou retornar uma mensagem de erro
                    //pois não podemos excluir, pode ser coisa importante. Wandrey 25/02/2011
                    throw new Exception("A pasta de XML´s com erro informada nas configurações não existe, por favor verifique.");

                    //oArquivo.Delete();
                }
            }
        }

        #endregion MoveArqErro()

        #region MoverArquivo()

        /// <summary>
        /// Move arquivos da nota fiscal eletrônica para suas respectivas pastas
        /// </summary>
        /// <param name="Arquivo">Nome do arquivo a ser movido</param>
        /// <param name="PastaXMLEnviado">Pasta de XML´s enviados para onde será movido o arquivo</param>
        /// <param name="SubPastaXMLEnviado">SubPasta de XML´s enviados para onde será movido o arquivo</param>
        /// <param name="PastaBackup">Pasta para Backup dos XML´s enviados</param>
        /// <param name="Emissao">Data de emissão da Nota Fiscal ou Data Atual do envio do XML para separação dos XML´s em subpastas por Ano e Mês</param>
        /// <date>16/07/2008</date>
        /// <by>Wandrey Mundin Ferreira</by>
        public static void MoverArquivo(string arquivo, PastaEnviados subPastaXMLEnviado, DateTime emissao, string nomeArquivoDestino)
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {

                #region Criar pastas que receberão os arquivos

                Empresas.Configuracoes[emp].CriarSubPastaEnviado();

                //Criar Pasta do Mês para gravar arquivos enviados autorizados ou denegados
                var nomePastaEnviado = string.Empty;
                var destinoArquivo = string.Empty;
                switch (subPastaXMLEnviado)
                {
                    case PastaEnviados.EmProcessamento:
                        nomePastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString();
                        destinoArquivo = nomePastaEnviado + "\\" + (string.IsNullOrEmpty(nomeArquivoDestino) ? Path.GetFileName(arquivo) : nomeArquivoDestino);
                        break;

                    case PastaEnviados.Autorizados:
                        nomePastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                           PastaEnviados.Autorizados.ToString() + "\\" +
                                           Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(emissao);
                        destinoArquivo = nomePastaEnviado + Path.GetFileName(arquivo);
                        goto default;

                    case PastaEnviados.Denegados:
                        nomePastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                           PastaEnviados.Denegados.ToString() + "\\" +
                                           Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(emissao);

                        if (arquivo.ToLower().EndsWith(Propriedade.ExtRetorno.Den))
                        {
                            destinoArquivo = Path.Combine(nomePastaEnviado, Path.GetFileName(arquivo));
                        }
                        else
                        {
                            destinoArquivo = Path.Combine(nomePastaEnviado, Functions.ExtrairNomeArq(arquivo, Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML) + Propriedade.ExtRetorno.Den);
                        }

                        goto default;

                    case PastaEnviados.Originais:
                        nomePastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                           PastaEnviados.Originais.ToString() + "\\" +
                                           Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(emissao);

                        destinoArquivo = nomePastaEnviado + Path.GetFileName(arquivo);
                        goto default;

                    default:
                        GarantirDiretorio(nomePastaEnviado, "TFunctions.MoverArquivo - pasta de destino");
                        break;
                }

                #endregion Criar pastas que receberão os arquivos

                //Se conseguiu criar a pasta ele move o arquivo, caso contrário
                if (Directory.Exists(nomePastaEnviado))
                {
                    #region Mover o XML para a pasta de XML´s enviados

                    //Se for para mover para a Pasta EmProcessamento
                    if (subPastaXMLEnviado == PastaEnviados.EmProcessamento)
                    {
                        //Se já existir o arquivo na pasta EmProcessamento vamos mover
                        //ele para a pasta com erro antes para evitar exceção. Wandrey 05/07/2011
                        if (File.Exists(destinoArquivo))
                        {
                            var destinoErro = Empresas.Configuracoes[emp].PastaXmlErro + "\\" + Path.GetFileName(arquivo);
                            File.Move(destinoArquivo, destinoErro);

                            //danasa 11-4-2012
                            Auxiliar.WriteLog("Arquivo \"" + destinoArquivo + "\" movido para a pasta \"" + Empresas.Configuracoes[emp].PastaXmlErro + "\".", true);
                        }

                        File.Move(arquivo, destinoArquivo);
                    }
                    else
                    {
                        //Se já existir o arquivo na pasta autorizados ou denegado, não vou mover o novo arquivo para lá, pois posso estar sobrepondo algum arquivo importante
                        //Sendo assim se o usuário quiser forçar mover, tem que deletar da pasta autorizados ou denegados manualmente, com isso evitamos perder um XML importante.
                        //Wandrey 05/07/2011
                        if (!File.Exists(destinoArquivo))
                        {
                            File.Move(arquivo, destinoArquivo);
                        }
                        else
                        {
                            var destinoErro = Empresas.Configuracoes[emp].PastaXmlErro + "\\" + Path.GetFileName(arquivo);
                            File.Move(arquivo, destinoErro);

                            //danasa 11-4-2012
                            Auxiliar.WriteLog("Arquivo \"" + arquivo + "\" movido para a pasta \"" + Empresas.Configuracoes[emp].PastaXmlErro + "\".", true);
                        }
                    }

                    #endregion Mover o XML para a pasta de XML´s enviados

                    if (subPastaXMLEnviado == PastaEnviados.Autorizados || subPastaXMLEnviado == PastaEnviados.Denegados)
                    {
                        #region Copiar XML para a pasta de BACKUP
                        try
                        {

                            //Fazer um backup do XML que foi copiado para a pasta de enviados
                            //para uma outra pasta para termos uma maior segurança no arquivamento
                            //Normalmente esta pasta é em um outro computador ou HD
                            if (Empresas.Configuracoes[emp].PastaBackup.Trim() != "")
                            {
                                //Criar Pasta do Mês para gravar arquivos enviados
                                var nomePastaBackup = string.Empty;
                                switch (subPastaXMLEnviado)
                                {
                                    case PastaEnviados.Autorizados:
                                        nomePastaBackup = Empresas.Configuracoes[emp].PastaBackup + "\\" +
                                            PastaEnviados.Autorizados.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(emissao);
                                        goto default;

                                    case PastaEnviados.Denegados:
                                        nomePastaBackup = Empresas.Configuracoes[emp].PastaBackup + "\\" +
                                            PastaEnviados.Denegados.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(emissao);
                                        goto default;

                                    case PastaEnviados.Originais:
                                        nomePastaBackup = Empresas.Configuracoes[emp].PastaBackup + "\\" +
                                            PastaEnviados.Originais.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(emissao);
                                        goto default;

                                    default:
                                        if (!Directory.Exists(nomePastaBackup))
                                        {
                                            Directory.CreateDirectory(nomePastaBackup);
                                        }
                                        break;
                                }

                                //Se conseguiu criar a pasta ele move o arquivo, caso contrário
                                if (Directory.Exists(nomePastaBackup))
                                {
                                    //Mover o arquivo da nota fiscal para a pasta de backup
                                    var destinoBackup = nomePastaBackup + Path.GetFileName(arquivo);
                                    if (File.Exists(destinoBackup))
                                    {
                                        File.Delete(destinoBackup);
                                    }
                                    File.Copy(destinoArquivo, destinoBackup);
                                }
                                else
                                {
                                    throw new Exception("Pasta de backup informada nas configurações não existe. (Pasta: " + nomePastaBackup + ")");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Auxiliar.WriteLog("Não foi possível copiar o XML para pasta de backup. Erro: " + ex.GetAllMessages(), true);
                        }

                        #endregion

                        #region Copiar o XML para a pasta do DanfeMon, se configurado para isso

                        CopiarXMLPastaDanfeMon(destinoArquivo);

                        #endregion Copiar o XML para a pasta do DanfeMon, se configurado para isso

                        #region Copiar o XML para o FTP

                        new GerarXML(emp).XmlParaFTP(emp, destinoArquivo);

                        #endregion Copiar o XML para o FTP
                    }
                }
                else
                {
                    throw new Exception("Pasta para arquivamento dos XML´s enviados não existe. (Pasta: " + nomePastaEnviado + ")");
                }
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("TFunctions.MoverArquivo: Falha ao mover arquivo. Arquivo=" + arquivo + ", SubPasta=" + subPastaXMLEnviado + ", Emissao=" + emissao.ToString("yyyy-MM-dd") + ". Erro: " + ex.GetAllMessages(), true);
                throw;
            }
        }

        #endregion MoverArquivo()

        private static void GarantirDiretorio(string diretorio, string contexto)
        {
            if (string.IsNullOrWhiteSpace(diretorio))
            {
                throw new Exception(contexto + ": Caminho do diretório vazio.");
            }

            if (!Directory.Exists(diretorio))
            {
                Directory.CreateDirectory(diretorio);
            }

            if (!Directory.Exists(diretorio))
            {
                throw new Exception(contexto + ": Não foi possível criar ou localizar o diretório " + diretorio);
            }
        }

        #region MoverArquivo()

        /// <summary>
        /// Move arquivos da nota fiscal eletrônica para suas respectivas pastas
        /// </summary>
        /// <param name="Arquivo">Nome do arquivo a ser movido</param>
        /// <param name="PastaXMLEnviado">Pasta de XML´s enviados para onde será movido o arquivo</param>
        /// <param name="SubPastaXMLEnviado">SubPasta de XML´s enviados para onde será movido o arquivo</param>
        /// <date>05/08/2009</date>
        /// <by>Wandrey Mundin Ferreira</by>
        public static void MoverArquivo(string Arquivo, PastaEnviados SubPastaXMLEnviado) => MoverArquivo(Arquivo, SubPastaXMLEnviado, DateTime.Now, "");

        /// <summary>
        /// Move arquivos da nota fiscal eletrônica para suas respectivas pastas
        /// </summary>
        /// <param name="Arquivo">Nome do arquivo a ser movido</param>
        /// <param name="PastaXMLEnviado">Pasta de XML´s enviados para onde será movido o arquivo</param>
        /// <param name="SubPastaXMLEnviado">SubPasta de XML´s enviados para onde será movido o arquivo</param>
        /// <date>05/08/2009</date>
        /// <by>Wandrey Mundin Ferreira</by>
        public static void MoverArquivo(string Arquivo, PastaEnviados SubPastaXMLEnviado, string nomeArquivoDestino) => MoverArquivo(Arquivo, SubPastaXMLEnviado, DateTime.Now, nomeArquivoDestino);

        /// <summary>
        /// Move arquivos da nota fiscal eletrônica para suas respectivas pastas
        /// </summary>
        /// <param name="arquivo">Nome do arquivo a ser movido</param>
        /// <param name="subPastaXMLEnviado">Pasta de XML´s enviados para onde será movido o arquivo</param>
        /// <param name="emissao">Data de emissão da Nota Fiscal ou Data Atual do envio do XML para separação dos XML´s em subpastas por Ano e Mês</param>
        public static void MoverArquivo(string arquivo, PastaEnviados subPastaXMLEnviado, DateTime emissao) => MoverArquivo(arquivo, subPastaXMLEnviado, emissao, "");

        #endregion MoverArquivo()

        #region CopiarXMLPastaDanfeMon()

        /// <summary>
        /// Copia o XML da NFe para a pasta monitorada pelo DANFEMon para que o mesmo imprima o DANFe.
        /// A copia só é efetuada de o UniNFe estiver configurado para isso.
        /// </summary>
        /// <param name="arquivoCopiar">Nome do arquivo com as pastas e subpastas a ser copiado</param>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 20/04/2010
        /// </remarks>
        public static void CopiarXMLPastaDanfeMon(string arquivoCopiar)
        {
            var emp = Empresas.FindEmpresaByThread();

            if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].PastaDanfeMon))
            {
                if (Directory.Exists(Empresas.Configuracoes[emp].PastaDanfeMon))
                {
                    if ((arquivoCopiar.ToLower().Contains(Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.ExtRetorno.ProcNFe.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonProcNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.ExtRetorno.Den.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonDenegadaNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.ExtRetorno.ProcCTe.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonProcNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.ExtRetorno.ProcMDFe.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonProcNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.ExtRetorno.ProcEventoNFe.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonProcNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.ExtRetorno.ProcEventoCTe.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonProcNFe) ||
                        (arquivoCopiar.ToLower().Contains(Propriedade.ExtRetorno.ProcEventoMDFe.ToLower()) && Empresas.Configuracoes[emp].XMLDanfeMonProcNFe))
                    {
                        //Montar o nome do arquivo de destino
                        var arqDestino = Empresas.Configuracoes[emp].PastaDanfeMon + "\\" + Path.GetFileName(arquivoCopiar);

                        //Copiar o arquivo para o destino
                        File.Copy(arquivoCopiar, arqDestino, true);
                    }
                }
            }
        }

        #endregion CopiarXMLPastaDanfeMon()


        #region getSubFolder()

        public static string getSubFolder(DateTime value, int ndias, DiretorioSalvarComo salvarComo)
        {
            if (salvarComo.ToString().Contains("D"))
            {
                return salvarComo.ToString(value.AddDays(ndias * -1));
            }

            if (salvarComo.ToString().Contains("M"))
            {
                return salvarComo.ToString(value.AddMonths(ndias * -1));
            }

            return "";
        }



        #endregion getSubFolder()

        #region RemoveSomenteLeitura()

        /// <summary>
        /// Metodo que remove atributo de Somente Leitura do Arquivo caso o mesmo estiver marcado, evitando problemas no acesso do arquivo.
        /// Renan - 26/11/13
        /// </summary>
        /// <param name="file">Arquivo a remover o atributo</param>
        public static void RemoveSomenteLeitura(string file)
        {
            var attributes = File.GetAttributes(file);

            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                // Show the file.
                attributes = RemoveAttribute(attributes, FileAttributes.ReadOnly);
                File.SetAttributes(file, attributes);
            }
        }

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove) => attributes & ~attributesToRemove;

        #endregion RemoveSomenteLeitura()

        #region Decompress

        public static string Decompress(string input)
        {
            var enc = input.ToCharArray();
            var dec = Convert.FromBase64CharArray(enc, 0, enc.Length);

            var encodedDataAsBytes = Convert.FromBase64String(input);
            using (System.IO.Stream comp = new System.IO.MemoryStream(encodedDataAsBytes))
            {
                using (System.IO.Stream decomp = new System.IO.Compression.GZipStream(comp, System.IO.Compression.CompressionMode.Decompress, false))
                {
                    using (var sr = new System.IO.StreamReader(decomp))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        #endregion Decompress

        public static T ToEnum<T>(string value) => (T)Enum.Parse(typeof(T), value, true);

        public static int DefiniMunicioPadrao(PadraoNFSe padraoNFse, int municipio)
        {
            return Functions.DefinirMunicipioPadraoNFSe(padraoNFse, municipio);
        }


        #region Métodos auxiliares para buscar valores no XML e tratar a ausência da tag

        public static string GetXmlValue(XmlDocument xml, string tagName)
        {
            var node = xml.GetElementsByTagName(tagName);

            return node.Count > 0 ? node[0].InnerText : string.Empty;
        }

        public static int? GetXmlIntValue(XmlDocument xml, string tagName, bool returnNull)
        {
            var value = GetXmlValue(xml, tagName);

            return int.TryParse(value, out var result) ? result : (returnNull ? (int?)null : 0);
        }

        public static int GetXmlIntValue(XmlDocument xml, string tagName)
        {
            var value = GetXmlValue(xml, tagName);

            return int.TryParse(value, out var result) ? result : 0;
        }

        public static decimal GetXmlDecimalValue(XmlDocument xml, string tagName)
        {
            var value = GetXmlValue(xml, tagName);
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0m;
        }

        public static DateTime GetXmlDateTimeValue(XmlDocument xml, string tagName)
        {
            var value = GetXmlValue(xml, tagName);

            return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : DateTime.MinValue;
        }

        public static char GetXmlCharValue(XmlDocument xml, string tagName)
        {
            var value = GetXmlValue(xml, tagName);

            return !string.IsNullOrEmpty(value) ? value[0] : 'N';
        }

        public static bool GetXmlBoolValue(XmlDocument xml, string tagName)
        {
            var value = GetXmlValue(xml, tagName);
            return bool.TryParse(value, out var result) && result;
        }

        public static DateTime GetXmlDateTimeValue(XmlElement element, string tagName)
        {
            var node = element.GetElementsByTagName(tagName);
            var value = node.Count > 0 ? node[0].InnerText : string.Empty;
            return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : DateTime.MinValue;
        }

        public static int GetXmlIntValue(XmlElement element, string tagName)
        {
            var node = element.GetElementsByTagName(tagName);
            var value = node.Count > 0 ? node[0].InnerText : string.Empty;
            return int.TryParse(value, out var result) ? result : 0;
        }

        public static decimal GetXmlDecimalValue(XmlElement element, string tagName)
        {
            var node = element.GetElementsByTagName(tagName);
            var value = node.Count > 0 ? node[0].InnerText : string.Empty;
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0m;
        }

        public static string GetXmlValue(XmlElement element, string tagName)
        {
            var node = element.GetElementsByTagName(tagName);
            return node.Count > 0 ? node[0].InnerText : string.Empty;
        }

        public static bool GetXmlBoolValue(XmlElement element, string tagName)
        {
            var value = GetXmlValue(element, tagName);
            return bool.TryParse(value, out var result) && result;
        }

        #endregion
    }
}
