using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

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

                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(arquivo);
                    var xmlValidado = ValidarXMLSchema.Validar(xmlDoc, Emp, false);
                    if (!(xmlValidado.StatusValidacao.Equals("5")))
                    {
                        edtTipoarquivo.Text = xmlValidado.Descricao;
                        textBox_resultado.Text = xmlValidado.MensagemRetorno;

                        return;
                    }
                }
                catch (Exception ex)
                {
                    textBox_resultado.Text = "Ocorreu um erro na validação do XML: \r\n\r\n" + ex.Message;
                    return;
                }
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