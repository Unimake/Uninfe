using NFe.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Unimake.Business.DFe.Security;

namespace NFe.Settings
{
    public class Empresas
    {
        /// <summary>
        /// Caminho das pastas com erro no caminho dos diretorios
        /// </summary>
        public static string ErroCaminhoDiretorio { get; set; }
        /// <summary>
        /// Propriedade para exibição  de mensagem de erro referente ao erro no caminho das pastas informadas
        /// </summary>
        public static bool ExisteErroDiretorio { get; set; }

        public static List<Empresa> Configuracoes = new List<Empresa>();

        /// <summary>
        /// Verifica se já existe alguma instância do UniNFe executando para os diretórios informados
        /// <para>Se existir, retorna uma mensagem com todos os diretórios que estão executando uma instânmcia do UniNFe</para>
        /// </summary>
        /// <param name="showMessage">Se verdadeiro, irá exibir a mensage e retornar o resultado
        /// <para>O padrão é verdadeiro</para></param>
        /// <returns></returns>
        public static void CanRun(Empresa empresaX)
        {
            if (Empresas.Configuracoes == null || Empresas.Configuracoes.Count == 0)
            {
                return;
            }

            //se no diretório de envio existir o arquivo "nome da máquina.locked" o diretório já está sendo atendido por alguma instancia do UniNFe

            foreach (var emp in Empresas.Configuracoes)
            {
                if (empresaX != null && (!empresaX.CNPJ.Equals(emp.CNPJ) || !empresaX.Servico.Equals(emp.Servico)))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(emp.PastaBase))
                {
                    throw new Components.Exceptions.ProblemaExecucaoUniNFe("Pasta de envio da empresa '" + emp.Nome + "' não está definida.");
                }
                else
                {
                    var dir = emp.PastaBase;

                    if (!Directory.Exists(dir))
                    {
                        throw new Components.Exceptions.ProblemaExecucaoUniNFe("Pasta de envio da empresa '" + emp.Nome + "' não existe.");
                    }
                    else
                    {
                        var fileName = string.Format("{0}-{1}.lock", Propriedade.NomeAplicacao, Environment.MachineName);
                        var filePath = string.Format("{0}\\{1}", dir, fileName);

                        //se já existe um arquivo de lock e o nome do arquivo for diferente desta máquina
                        //não pode deixar executar                        

                        var fileLock = (from x in
                                               (from f in Directory.GetFiles(dir, "*" + Propriedade.NomeAplicacao + "*.lock")
                                                select new FileInfo(f))
                                        where !x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)
                                        select x.FullName).FirstOrDefault();

                        if (Propriedade.NomeAplicacao.ToLower() == "uninfeservico" && string.IsNullOrEmpty(fileLock))
                        {
                            var filename2 = string.Format("{0}-{1}.lock", "UniNFe", Environment.MachineName);
                            fileLock = (from x in
                                            (from f in Directory.GetFiles(dir, "*UniNFe" + "*.lock")
                                             select new FileInfo(f))
                                        where !x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)
                                        select x.FullName).FirstOrDefault();
                        }

                        if (!string.IsNullOrEmpty(fileLock))
                        {
                            throw new Components.Exceptions.AppJaExecutando("Já existe uma instância do UniNFe em Execução que atende a conjunto de pastas: " +
                                dir + " (*Incluindo subdiretórios).\r\n\r\n" +
                                "Nome da estação que está executando: " + fileName.Replace(Propriedade.NomeAplicacao + "-", "").Replace(".lock", ""));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cria os arquivos de lock para os diretórios de envio que esta instância vai atender.
        /// <param name="clearIfExist">Se verdadeiro, irá excluir os arquivos existentes antes de recriar</param>
        /// </summary>
        public static void CreateLockFile(bool clearIfExist = false)
        {
            if (Empresas.Configuracoes == null || Empresas.Configuracoes.Count == 0)
            {
                return;
            }

            if (clearIfExist)
            {
                ClearLockFiles(false);
            }

            var diretorios = (from d in Empresas.Configuracoes
                              select d.PastaBase);

            foreach (var dir in diretorios)
            {
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    var file = string.Format("{0}\\{1}-{2}.lock", dir, Propriedade.NomeAplicacao, Environment.MachineName);
                    var fi = new FileInfo(file);

                    using (var sw = new StreamWriter(file, false)
                    {
                        AutoFlush = true
                    })
                    {
                        sw.WriteLine("Iniciado em: {0:dd/MM/yyyy hh:mm:ss}", DateTime.Now);
                        sw.WriteLine("Estação: {0}", Environment.MachineName);
                        sw.WriteLine("IP: {0}", Functions.GetIPAddress());
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }

        #region CarregaConfiguracao()

        /// <summary>
        /// Carregar as configurações de todas as empresas na coleção "Configuracoes" 
        /// </summary>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 29/07/2010
        /// </remarks>
        public static void CarregaConfiguracao(bool uniNFeServico = false)
        {
            Empresas.Configuracoes.Clear();
            Empresas.ExisteErroDiretorio = false;
            Empresas.CriarPasta(true);

            if (File.Exists(Propriedade.NomeArqEmpresas))
            {
                var nomeArq2 = Propriedade.PastaExecutavel + "\\UniNfeEmpresa2.xml";
                var nomeArq3 = Propriedade.PastaExecutavel + "\\UniNfeEmpresa3.xml";

                #region Testar arquivo de empresa, se erro vai buscar os backups

                if (!AbrirArqEmpresa(nomeArq2))
                {
                    if (!AbrirArqEmpresa(nomeArq3))
                    {
                        AbrirArqEmpresa();
                    }
                }

                #endregion

                XElement axml;

                try
                {
                    axml = XElement.Load(Propriedade.NomeArqEmpresas);
                }
                catch
                {
                    throw;
                }

                try
                {
                    if (TestarArqEmpresa(Propriedade.NomeArqEmpresas, false)) //Segundo parâmetro sempre tem que ser falso quando é o arquivo principal das empresas
                    {
                        if (!TestarArqEmpresa(nomeArq2, true))
                        {
                            File.Copy(Propriedade.NomeArqEmpresas, nomeArq2, true);
                        }

                        if (!TestarArqEmpresa(nomeArq3, true))
                        {
                            File.Copy(Propriedade.NomeArqEmpresas, nomeArq3, true);
                        }
                    }

                    var b1 = axml.Descendants(NFeStrConstants.Registro);
                    foreach (var item in b1)
                    {
                        var empresa = new Empresa
                        {
                            CNPJ = item.Attribute(TpcnResources.CNPJ.ToString()).Value,
                            Nome = item.Element(NFeStrConstants.Nome).Value.Trim(),
                            Servico = Propriedade.TipoAplicativo
                        };
                        if (item.Attribute(NFeStrConstants.Servico) != null)
                        {
                            empresa.Servico = (TipoAplicativo)Convert.ToInt16(item.Attribute(NFeStrConstants.Servico).Value.Trim());
                        }

                        string cArqErro = null;
                        var erro = false;

                        try
                        {
                            var tipoerro = 0;
                            var rc = empresa.BuscaConfiguracao(ref tipoerro);
                            switch (tipoerro)
                            {
                                case 1:
                                    erro = true;
                                    throw new Exception(rc);
                                case 2: //Não localizou o certificado digital na hora de chamar o empresa.BuscaConfiguracao()
                                    //Quando é pelo serviço ele testa o certificado digital em outro ponto, justamente para não gerar o erro, dando tempo para o windows carregar o que é necessário para o bom funcionamento antes.
                                    if (!uniNFeServico)
                                    {
                                        throw new Exception(rc);
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                ///
                                /// nao acessar o metodo Auxiliar.GravarArqErroERP(string Arquivo, string Erro) já que nela tem a pesquisa da empresa
                                /// com base em "int emp = Empresas.FindEmpresaByThread();" e neste ponto ainda não foi criada
                                /// as thread's
                                cArqErro = CriaArquivoDeErro(empresa);

                                //Grava arquivo de ERRO para o ERP
                                File.WriteAllText(cArqErro, ex.Message);
                            }
                            catch { }
                        }
                        if (!erro)
                        {
                            ///
                            /// mesmo com erro, adicionar a lista para que o usuário possa altera-la
                            empresa.ChecaCaminhoDiretorio();

                            if (!string.IsNullOrEmpty(Empresas.ErroCaminhoDiretorio) && Empresas.ExisteErroDiretorio)
                            {
                                try
                                {
                                    if (cArqErro == null)
                                    {
                                        cArqErro = CriaArquivoDeErro(empresa);
                                    }
                                    //Grava arquivo de ERRO para o ERP
                                    File.AppendAllText(cArqErro, "Erros de diretorios:\r\n\r\n" + Empresas.ErroCaminhoDiretorio, Encoding.Default);
                                }
                                catch { }
                            }
                            Configuracoes.Add(empresa);
                        }
                    }
                }
                catch
                {
                    throw;
                }

            }

            if (!Empresas.ExisteErroDiretorio)
            {
                Empresas.CriarPasta(false);
            }

            //Carregar PIN do A3 para que o usuário não precise digitar
            for (var i = 0; i < Empresas.Configuracoes.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(Empresas.Configuracoes[i].CertificadoPIN) && !Empresas.Configuracoes[i].CertificadoPINCarregado)
                {
                    try
                    {
                        Empresas.Configuracoes[i].X509Certificado.SetPinPrivateKey(Empresas.Configuracoes[i].CertificadoPIN);
                        Empresas.Configuracoes[i].CertificadoPINCarregado = true;
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Testar se o arquivo de empresa está integro, sem falhas.
        /// </summary>
        /// <param name="arqEmpresa"></param>
        /// <returns>Arquivo está ok</returns>
        public static bool TestarArqEmpresa(string arqEmpresa, bool testarIgualdadeComArquivoPrincipal)
        {
            var ok = true;

            if (!File.Exists(arqEmpresa))
            {
                ok = false;
            }
            else if (new FileInfo(arqEmpresa).Length <= 100)
            {
                ok = false;
            }
            else
            {
                try
                {
                    var axml = XElement.Load(arqEmpresa);
                }
                catch
                {
                    ok = false;
                }
            }

            if (ok && testarIgualdadeComArquivoPrincipal)
            {
                var doc1 = new XmlDocument();
                var doc2 = new XmlDocument();
                doc1.Load(Propriedade.NomeArqEmpresas); //Arquivo de empresas principal
                doc2.Load(arqEmpresa); //Arquivo de backup do arquivo de empresas

                if (doc2.OuterXml != doc1.OuterXml)
                {
                    ok = false;
                }
            }

            return ok;
        }

        #endregion

        public static bool AbrirArqEmpresa(string nomeArq = "")
        {
            var abriu = false;

            try
            {
                var axml = XElement.Load(Propriedade.NomeArqEmpresas);
                abriu = true;
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(nomeArq))
                {
                    CopiarSalvaEmpresa(nomeArq);
                }
            }

            return abriu;
        }

        private static void CopiarSalvaEmpresa(string nomeArq)
        {
            if (TestarArqEmpresa(nomeArq, false)) //Neste ponto o segundo parâmetro sempre tem que ser falso, pois não posso testar a igualdade do arquivo principal com o de backup, pois neste ponto entende-se que o principal está danificado, ou nem teria chegado aqui.
            {
                try
                {
                    File.Copy(nomeArq, Propriedade.NomeArqEmpresas, true);
                }
                catch { }
            }
        }

        public static string GetUF(int codigoUnidade)
        {
            if (codigoUnidade < 100)    //desconsidera empresa que é só NFS-e
            {
                try
                {
                    return Propriedade.Estados.Where(p => p.CodigoMunicipio == codigoUnidade).Select(p => p.UF).First();
                }
                catch { }
            }

            return null;
        }

        private static string CriaArquivoDeErro(Empresa empresa)
        {
            string cArqErro;

            if (string.IsNullOrEmpty(empresa.PastaXmlRetorno))
            {
                cArqErro = Path.Combine(Propriedade.PastaGeralRetorno, string.Format(Propriedade.NomeArqERRUniNFe, DateTime.Now.ToString("yyyyMMddTHHmmss")));
            }
            else
            {
                cArqErro = Path.Combine(empresa.PastaXmlRetorno, string.Format(Propriedade.NomeArqERRUniNFe, DateTime.Now.ToString("yyyyMMddTHHmmss")));
            }

            if (!Directory.Exists(Path.GetDirectoryName(cArqErro)))
            {
                cArqErro = Path.Combine(Propriedade.PastaLog, Path.GetFileName(cArqErro));
            }
            return cArqErro;
        }

        /// <summary>
        /// Exclui todos os arquivos de lock existentes nas configurações de pasta das empresas
        /// <param name="confirm">Se verdadeiro confirma antes de apagar os arquivos</param>
        /// </summary>
        public static bool ClearLockFiles(bool confirm = true)
        {
            if (Empresas.Configuracoes == null || Empresas.Configuracoes.Count == 0)
            {
                return true;
            }

            var result = false;

            if (confirm && MessageBox.Show("Exclui os arquivos de \".lock\" configurados para esta instância?\r\nA aplicação será encerrada ao terminar a exclusão dos arquivos.\r\n\r\n\tTem certeza que deseja continuar? ", "Arquivos de .lock", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return false;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                foreach (var empresa in Empresas.Configuracoes)
                {
                    empresa.DeleteLockFile();
                }
                if (confirm)
                {
                    MessageBox.Show("Arquivos de \".lock\" excluídos com sucesso.", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                result = true;
            }
            catch (Exception ex)
            {
                if (confirm)
                {
                    MessageBox.Show(ex.Message, "Erro!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            return result;
        }

        #region CriarPasta()
        /// <summary>
        /// Criar as pastas para todas as empresas cadastradas e configuradas no sistema se as mesmas não existirem
        /// </summary>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>29/09/2009</date>
        public static void CriarPasta(bool onlygeral)
        {
            if (onlygeral)
            {
                if (!Directory.Exists(Propriedade.PastaGeral))
                {
                    Directory.CreateDirectory(Propriedade.PastaGeral);
                }

                if (!Directory.Exists(Propriedade.PastaGeralRetorno))
                {
                    Directory.CreateDirectory(Propriedade.PastaGeralRetorno);
                }

                if (!Directory.Exists(Propriedade.PastaGeralTemporaria))
                {
                    Directory.CreateDirectory(Propriedade.PastaGeralTemporaria);
                }

                if (!Directory.Exists(Propriedade.PastaLog))
                {
                    Directory.CreateDirectory(Propriedade.PastaLog);
                }
            }
            else
            {
                foreach (var empresa in Empresas.Configuracoes)
                {
                    empresa.CriarPastasDaEmpresa();
                }
                Empresas.CriarSubPastaEnviado();
            }
        }
        #endregion

        #region CriarSubPastaEnviado()
        /// <summary>
        /// Criar as subpastas (Autorizados/Denegados/EmProcessamento) dentro da pasta dos XML´s enviados para todas as empresas cadastradas e configuradas
        /// </summary>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Date: 20/04/2010
        /// </remarks>
        private static void CriarSubPastaEnviado()
        {
            for (var i = 0; i < Empresas.Configuracoes.Count; i++)
            {
                Empresas.Configuracoes[i].CriarSubPastaEnviado();
            }
        }
        #endregion

        #region #10316
        /*
         * Solução para o problema do certificado do tipo A3
         * Marcelo
         * 29/07/2013
         */
        #region Reset certificado
        /// <summary>
        /// Reseta o certificado da empresa e recria o mesmo
        /// </summary>
        /// <param name="index">identificador da empresa</param>
        /// <returns></returns>
        public static X509Certificate2 ResetCertificado(int index)
        {
            var empresa = Empresas.Configuracoes[index];
            if (empresa.UsaCertificado)
            {
                empresa.X509Certificado.Reset();

                Thread.Sleep(0);

                empresa.X509Certificado = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //Ajustar o certificado digital de String para o tipo X509Certificate2
                var store = new X509Store("MY", StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var collection = store.Certificates;
                X509Certificate2Collection collection1;
                if (!string.IsNullOrEmpty(empresa.CertificadoDigitalThumbPrint))
                {
                    collection1 = collection.Find(X509FindType.FindByThumbprint, empresa.CertificadoDigitalThumbPrint, false);
                }
                else
                {
                    collection1 = collection.Find(X509FindType.FindBySubjectDistinguishedName, empresa.Certificado, false);
                }

                for (var i = 0; i < collection1.Count; i++)
                {
                    //Verificar a validade do certificado
                    if (DateTime.Compare(DateTime.Now, collection1[i].NotAfter) == -1)
                    {
                        empresa.X509Certificado = collection1[i];
                        break;
                    }
                }

                //Se não encontrou nenhum certificado com validade correta, vou pegar o primeiro certificado, porem vai travar na hora de tentar enviar a nota fiscal, por conta da validade. Wandrey 06/04/2011
                if (empresa.X509Certificado == null && collection1.Count > 0)
                {
                    empresa.X509Certificado = collection1[0];
                }
            }
            return empresa.X509Certificado;

        }
        #endregion
        #endregion

        #region FindConfEmpresa()
        /// <summary>
        /// Procurar o cnpj na coleção das empresas
        /// </summary>
        /// <param name="cnpj">CNPJ a ser pesquisado</param>
        /// <param param name="servico">Serviço a ser pesquisado</param>
        /// <returns>objeto empresa localizado, null se nada for localizado</returns>
        public static Empresa FindConfEmpresa(string cnpj, TipoAplicativo servico)
        {
            Empresa retorna = null;
            foreach (var empresa in Empresas.Configuracoes)
            {
                if (empresa.CNPJ.Equals(cnpj) && empresa.Servico.Equals(servico))
                {
                    retorna = empresa;
                    break;
                }
            }
            return retorna;
        }
        #endregion

        #region FindConfEmpresaIndex()
        /// <summary>
        /// Procurar o cnpj na coleção das empresas
        /// </summary>
        /// <param name="cnpj">CNPJ a ser pesquisado</param>
        /// <param name="servico">Serviço a ser pesquisado</param>
        /// <returns>Retorna o index do objeto localizado ou null se nada for localizado</returns>
        public static int FindConfEmpresaIndex(string cnpj, TipoAplicativo servico)
        {
            var retorna = -1;

            for (var i = 0; i < Empresas.Configuracoes.Count; i++)
            {
                var empresa = Empresas.Configuracoes[i];

                if (empresa.CNPJ.Equals(cnpj) && empresa.Servico.Equals(servico))
                {
                    retorna = i;
                    break;
                }
            }
            return retorna;
        }
        #endregion

        /// <summary>
        /// Retorna a empresa pela thread atual
        /// </summary>
        /// <returns></returns>
        public static int FindEmpresaByThread() => Convert.ToInt32(Thread.CurrentThread.Name);

        /// <summary>
        /// Localiza o código da empresa com base no nome pasta envio
        /// </summary>
        /// <param name="arquivo">Pasta + nome do arquivo que está sendo enviado</param>
        /// <returns>Retorna o código da empresa</returns>
        public static int FindEmpresaByFolder(string arquivo)
        {
            var fi = new FileInfo(arquivo);

            var empresa = -1;

            try
            {
                var fullName = ConfiguracaoApp.RemoveEndSlash(fi.Directory.FullName.ToLower());

                /// "EndsWith" é para pegar apenas se terminar com, já que nas empresas pode ter um nome 'temp' no meio das definicoes das pastas
                if (fullName.EndsWith("\\temp"))
                {
                    /// exclui o 'arquivo' temp.
                    fullName = Path.GetDirectoryName(fullName);
                }

                for (var i = 0; i < Empresas.Configuracoes.Count; i++)
                {
                    if (fullName == Empresas.Configuracoes[i].PastaXmlEnvio.ToLower() ||
                        fullName == Empresas.Configuracoes[i].PastaXmlEmLote.ToLower() ||
                        fullName == Empresas.Configuracoes[i].PastaValidar.ToLower() ||
                        fullName == Empresas.Configuracoes[i].PastaContingencia.ToLower())
                    {
                        empresa = i;
                        break;
                    }
                }
            }
            catch
            {
            }

            return empresa;
        }

        #region Valid()
        /// <summary>
        /// Retorna se o indice da coleção que foi pesquisado é valido ou não
        /// </summary>
        /// <param name="index">Indice a ser validado</param>
        /// <returns>Retorna true or false</returns>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 30/07/2010
        /// </remarks>
        public static bool Valid(int index)
        {
            var retorna = true;
            if (index.Equals(-1))
            {
                retorna = false;
            }

            return retorna;
        }
        #endregion

        #region Valid()
        /// <summary>
        /// Retorna se o objeto da coleção que foi pesquisado é valido ou não
        /// </summary>
        /// <param name="empresa">Objeto da empresa</param>
        /// <returns>Retorna true or false</returns>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 30/07/2010
        /// </remarks>
        public static bool Valid(Empresa empresa)
        {
            var retorna = true;
            if (empresa.Equals(null))
            {
                retorna = false;
            }

            return retorna;
        }
        #endregion

        #region verificaPasta
        public static void VerificaPasta(Empresa empresa, XmlElement configElemento, string tagName, string descricao, bool isObrigatoria)
        {
            var node = configElemento.GetElementsByTagName(tagName)[0];
            if (node != null)
            {
                if (!isObrigatoria && node.InnerText.Trim() == "")
                {
                    return;
                }

                if (isObrigatoria && node.InnerText.Trim() == "")
                {
                    Empresas.ExisteErroDiretorio = true;
                    ErroCaminhoDiretorio += "Empresa: " + empresa.Nome + "   : \"" + descricao + "\"\r\n";
                }
                else
                    if (!Directory.Exists(node.InnerText.Trim()) && node.InnerText.Trim() != "")
                {
                    Empresas.ExisteErroDiretorio = true;
                    ErroCaminhoDiretorio += "Empresa: " + empresa.Nome + "   Pasta: " + node.InnerText.Trim() + "\r\n";
                }
            }
            else
            {
                if (isObrigatoria)
                {
                    Empresas.ExisteErroDiretorio = true;
                    ErroCaminhoDiretorio += "Empresa: " + empresa.Nome + "   : \"" + descricao + "\"\r\n";
                }
            }
        }
        #endregion

        #region Conta quantas empresas sao para NFe/CTe/MDFe/NFCe
        public static int CountEmpresasNFe
        {
            get
            {
                if (Configuracoes == null)
                {
                    return 0;
                }

                return Configuracoes.Where(x => x.Servico != TipoAplicativo.Nfse).Count();
            }
        }
        #endregion

        #region Conta quantas empresas sao para NFSe
        public static int CountEmpresasNFse
        {
            get
            {
                if (Configuracoes == null)
                {
                    return 0;
                }

                return Configuracoes.Where(x => x.Servico == TipoAplicativo.Nfse).Count();
            }
        }
        #endregion
    }
}
