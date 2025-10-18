using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.UI
{
    public partial class userValidaXML : UserControl1
    {
        #region Private Fields

        private FileInfo arqDestino = null;
        private int Emp;
        private WebBrowser wb = null;

        #endregion Private Fields

        #region Private Methods

        private void btn_Validar_Click(object sender, EventArgs e)
        {
            LimparEPosicionarTC();

            string arquivoXML = edtFilename.Text.Replace("\"", "");

            try
            {
                if (arquivoXML == "" || !File.Exists(arquivoXML))
                {
                    textBox_resultado.Text = "Arquivo não encontrado.";
                    return;
                }

                // Copiar o arquivo XML para temporários para assinar e depois vou validar o que está nos temporários
                FileInfo fileInfo = new FileInfo(arquivoXML);
                string arquivo = Path.GetTempPath() + fileInfo.Name;

                arqDestino = new FileInfo(arquivo);

                if (arqDestino.Exists) arqDestino.Delete();

                fileInfo.CopyTo(arquivo, true);

                // Remover atributo de somente leitura que pode gerar problemas no acesso do arquivo
                Service.TFunctions.RemoveSomenteLeitura(arquivo);

                // Desviar para nova rotina, se ela não atender determinados arquivos, vamos validar da forma antiga, sem a DLL do UniNFe
                // De futuro vamos apagar todo o conteúdo da rotina antiga.

                #region Nova rotina de validação

                var validarXMLNew = new ValidarXMLNew();

                try
                {
                    if (validarXMLNew.Validar(arquivo, false, this.Emp))
                    {
                        edtTipoarquivo.Text = validarXMLNew.TipoArquivoXML;
                        textBox_resultado.Text = "XML validado com sucesso!";

                        return;
                    }
                }
                catch (Exception ex)
                {
                    edtTipoarquivo.Text = validarXMLNew.TipoArquivoXML;
                    textBox_resultado.Text = ex.Message;

                    return;
                }

                #endregion Nova rotina de validação

                #region Rotina antiga de validação

                NFe.Service.TaskValidar val = new Service.TaskValidar();
                val.NomeArquivoXML = arqDestino.FullName;
                val.Execute();
                int codUF = Empresas.Configuracoes[Emp].UnidadeFederativaCodigo;
                // Detectar o tipo do arquivo
                PadraoNFSe padraoNFSe = Functions.BuscaPadraoNFSe(Empresas.Configuracoes[Emp].UnidadeFederativaCodigo);
                if (padraoNFSe == PadraoNFSe.BETHA)
                {
                    string versao = Functions.GetAttributeXML("LoteRps", "versao", arquivo);
                    if (versao.Equals("2.02"))
                        codUF = 202;
                }
                Validate.ValidarXML validarXML = new Validate.ValidarXML(arquivo, codUF, false);

                string resultValidacao = "";

                XmlDocument conteudoXML = new XmlDocument();
                conteudoXML.PreserveWhitespace = true;
                try
                {
                    conteudoXML.Load(arquivo);
                }
                catch
                {
                    conteudoXML.LoadXml(File.ReadAllText(arquivo, System.Text.Encoding.UTF8));
                }

                textBox_resultado.Text = validarXML.TipoArqXml.cRetornoTipoArq;

                // Refatoração efetuada para utilizar a nova biblioteca de assinatura estática da DLL
                if (validarXML.TipoArqXml.nRetornoTipoArq >= 1 && validarXML.TipoArqXml.nRetornoTipoArq <= SchemaXML.MaxID)
                {
                    edtTipoarquivo.Text = validarXML.TipoArqXml.cRetornoTipoArq;

                    var tipoArquivo = new TipoArquivoXML(arquivo, Empresas.Configuracoes[Emp].UnidadeFederativaCodigo, false);
                    X509Certificate2 certificado = Empresas.Configuracoes[Emp].X509Certificado;

                    var algoritmo = (conteudoXML.DocumentElement.Name.Equals("Reinf") || conteudoXML.DocumentElement.Name.Equals("eSocial"))
                        ? Unimake.Business.DFe.Security.AlgorithmType.Sha256
                        : Unimake.Business.DFe.Security.AlgorithmType.Sha1;

                    bool lValidar = false;

                    try
                    {
                        if (!string.IsNullOrEmpty(tipoArquivo.TagAssinatura))
                        {
                            Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, tipoArquivo.TagAssinatura, tipoArquivo.TagAtributoId, certificado, algoritmo, true);
                        }

                        if (!string.IsNullOrEmpty(tipoArquivo.TagAssinatura0))
                        {
                            Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, tipoArquivo.TagAssinatura0, tipoArquivo.TagAtributoId0, certificado, algoritmo, true);
                        }

                        // Assinar o lote, se existir
                        if (!string.IsNullOrEmpty(tipoArquivo.TagLoteAssinatura))
                        {
                            Unimake.Business.DFe.Security.AssinaturaDigital.Assinar(conteudoXML, tipoArquivo.TagLoteAssinatura, tipoArquivo.TagLoteAtributoId, certificado, algoritmo, true);
                        }

                        conteudoXML.Save(arquivo);
                        lValidar = true;
                    }
                    catch (Exception ex)
                    {
                        lValidar = false;
                        textBox_resultado.Text = "Ocorreu um erro ao tentar assinar o XML: \r\n\r\n" +
                            validarXML.TipoArqXml.cRetornoTipoArq + "\r\n" + ex.Message;
                    }

                    if (lValidar == true)
                    {
                        // Validar o arquivo
                        if (validarXML.TipoArqXml.nRetornoTipoArq >= 1 && validarXML.TipoArqXml.nRetornoTipoArq <= SchemaXML.MaxID)
                        {
                            ///danasa: 12/2013
                            validarXML.ValidarArqXML(arquivo);
                            if (string.IsNullOrEmpty(validarXML.TipoArqXml.cArquivoSchema))
                            {
                                textBox_resultado.Text = "XML não possui schema de validação, sendo assim não é possível validar XML";
                            }
                            else if (validarXML.Retorno == 0 && string.IsNullOrEmpty(resultValidacao))
                            {
                                textBox_resultado.Text = "Arquivo validado com sucesso!";
                            }
                            else if (!string.IsNullOrEmpty(resultValidacao))
                            {
                                textBox_resultado.Text = resultValidacao;
                            }
                            else
                            {
                                textBox_resultado.Text = "XML INCONSISTENTE!\r\n\r\n" + validarXML.RetornoString;
                            }
                        }
                        else
                        {
                            textBox_resultado.Text = "XML INCONSISTENTE!\r\n\r\n" + validarXML.TipoArqXml.cRetornoTipoArq;
                        }
                    }
                }
                try
                {
                    if (wb == null)
                    {
                        wb = new WebBrowser();
                        wb.Parent = metroTabPage2;
                        wb.Dock = DockStyle.Fill;
                        wb.DocumentCompleted += webBrowser1_DocumentCompleted;
                    }
                    wb.Visible = true;
                    wb.Navigate(arquivo);
                }
                catch
                {
                    webBrowser1_DocumentCompleted(null, null);
                }

                #endregion Rotina antiga de validação
            }
            catch (Exception ex)
            {
                textBox_resultado.Text = ex.Message + "\r\n" + ex.StackTrace;

                if (wb != null)
                    wb.Visible = false;
            }
        }

        private void cbEmpresas_SelectedIndexChanged(object sender, EventArgs e)
        {
            Emp = -1;
            try
            {
                if (cbEmpresas.SelectedValue != null)
                {
                    var list = (cbEmpresas.DataSource as System.Collections.ArrayList)[cbEmpresas.SelectedIndex] as NFe.Components.ComboElem;
                    Emp = Empresas.FindConfEmpresaIndex(list.Valor, NFe.Components.EnumHelper.StringToEnum<TipoAplicativo>(list.Servico));

                    uninfeDummy.xmlParams.WriteValue(GetType().Name, "last_empresa", cbEmpresas.SelectedIndex);
                    uninfeDummy.xmlParams.Save();
                }
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "");
            }
            finally
            {
                edtFilename.Enabled =
                    btn_Validar.Enabled = Emp >= 0;
            }
        }

        private void edtFilename_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MetroFramework.Controls.MetroTextBox control = (MetroFramework.Controls.MetroTextBox)sender;
                int x = control.ClientRectangle.Width - control.Icon.Size.Width;
                if (e.Location.X >= x)
                {
                    string path = uninfeDummy.xmlParams.ReadValue(GetType().Name, "path", "");

                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        dlg.RestoreDirectory = true;
                        dlg.Filter = "";

                        if (Empresas.Configuracoes[Emp].Servico == TipoAplicativo.Nfse)
                        {
                            dlg.Filter += "Arquivos de NFSe|*" + Propriedade.Extensao(Propriedade.TipoEnvio.EnvLoteRps).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedLoteRps).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitLoteRps).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSe).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedSitNFSeRps).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedInuNFSe).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedNFSePNG).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedNFSePDF).EnvioXML +
                                ";*" + Propriedade.Extensao(Propriedade.TipoEnvio.PedNFSeXML).EnvioXML;
                        }
                        else
                        {
                            dlg.Filter = "Todos os arquivos|*.xml";

                            dlg.Filter += string.Format("|Arquivos da NFe/NFCe (*.*{0})|*{0}", Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML);
                            dlg.Filter += string.Format("|Arquivos de CTe (*.*{0})|*{0}", Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML);
                            dlg.Filter += string.Format("|Arquivos de DFe (*.*{0})|*{0}", Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFe).EnvioXML);
                            dlg.Filter += string.Format("|Arquivos de DFe (*.*{0})|*{0}", Propriedade.Extensao(Propriedade.TipoEnvio.EnvDFeCTe).EnvioXML);
                            dlg.Filter += string.Format("|Arquivos de MDFe (*.*{0})|*{0}", Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML);

                            dlg.Filter += string.Format("|Arquivos de eventos (*.*{0},*.*{1},*.*{2},*.*{3})|*{0};*{1};*{2};*{3}",
                                Propriedade.Extensao(Propriedade.TipoEnvio.EnvCCe).EnvioXML,
                                Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioXML,
                                Propriedade.Extensao(Propriedade.TipoEnvio.EnvCancelamento).EnvioXML,
                                Propriedade.Extensao(Propriedade.TipoEnvio.EnvManifestacao).EnvioXML);

                            dlg.Filter += string.Format("|Arquivos do eSocial (*.*{0},*.*{1},*.*{2})|*{0};*{1};*{2}",
                                Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_evt).EnvioXML,
                                Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).EnvioXML,
                                Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_loteevt).EnvioXML);

                            dlg.Filter += string.Format("|Arquivos do EFDReinf (*.*{0},*.*{1})|*{0};*{1}",
                                Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_evt).EnvioXML,
                                Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_loteevt).EnvioXML);
                        }

                        if (!string.IsNullOrEmpty(path))
                            dlg.InitialDirectory = path;

                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            if (!string.IsNullOrEmpty(dlg.FileName))
                            {
                                uninfeDummy.xmlParams.WriteValue(GetType().Name, "path", Path.GetDirectoryName(dlg.FileName));
                                uninfeDummy.xmlParams.Save();

                                edtFilename.Text = dlg.FileName;

                                btn_Validar_Click(null, null);
                            }
                        }
                    }
                }
            }
        }

        private void LimparEPosicionarTC()
        {
            metroTabControl.SelectedIndex = 0;
            textBox_resultado.Clear();
            edtTipoarquivo.Clear();
        }

        private void textBox_arqxml_TextChanged(object sender, EventArgs e)
        {
            LimparEPosicionarTC();
            btn_Validar.Enabled = !string.IsNullOrEmpty(edtFilename.Text);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (arqDestino != null)
                if (arqDestino.Exists)
                    arqDestino.Delete();

            arqDestino = null;
        }

        #endregion Private Methods

        #region Public Constructors

        public userValidaXML()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Public Methods

        public override void UpdateControles()
        {
            base.UpdateControles();

            cbEmpresas.SelectedIndexChanged -= cbEmpresas_SelectedIndexChanged;
            cbEmpresas.DisplayMember = NFeStrConstants.Nome;
            cbEmpresas.ValueMember = "Key";
            cbEmpresas.DataSource = Auxiliar.CarregaEmpresa(false);
            cbEmpresas.SelectedIndexChanged += cbEmpresas_SelectedIndexChanged;

            int posicao = uninfeDummy.xmlParams.ReadValue(GetType().Name, "last_empresa", 0);
            if (posicao >= (cbEmpresas.DataSource as System.Collections.ArrayList).Count)
                posicao = 0;

            cbEmpresas.SelectedIndex = posicao;
            cbEmpresas_SelectedIndexChanged(null, null);
            btn_Validar.Enabled = false;

            Timer t = new Timer();
            t.Interval = 50;
            t.Tick += (sender, e) =>
            {
                ((Timer)sender).Stop();
                ((Timer)sender).Dispose();

                edtFilename.Focus();
            };
            t.Start();
        }

        #endregion Public Methods
    }
}