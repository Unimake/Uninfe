using NFe.Components;
using NFe.Settings;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace NFe.UI
{
    [ToolboxItem(false)]
    public partial class menu : MetroFramework.Controls.MetroUserControl
    {
        public menu()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ConfiguracaoApp.CarregarDadosSobre();

            metroLink_unimake.Visible = !string.IsNullOrEmpty(ConfiguracaoApp.Site);
            metroLink_unimake.Text = ConfiguracaoApp.Site;
            uninfeDummy.mainForm.Text = Propriedade.NomeAplicacao + " - Monitor DF-e";
            metroTile_update.Text = "Atualizar o " + Propriedade.NomeAplicacao;

            var Components = Controls.Cast<object>()
                                                   .Where(obj => !ReferenceEquals(obj, this))
                                                   .OfType<MetroFramework.Controls.MetroTile>();
            foreach (var c in Components)
            {
                c.Style = (MetroFramework.MetroColorStyle)uninfeDummy.xmlParams.ReadValue(Name + "\\" + c.Name, "color", Convert.ToInt16(c.Style));
            }

            UpdateControles();
        }

        public void UpdateControles()
        {
            metroTile_Configuracoes.TileCount = Empresas.Configuracoes.Count;
            metroTile_ValidaXml.Enabled =
                (Empresas.Configuracoes != null && Empresas.Configuracoes.Count > 0);

            metroTile_Servicos.Visible =
                metroTile_CadastroContrib.Visible = Empresas.CountEmpresasNFe > 0;
        }

        public void Show(uninfeOpcoes opcao)
        {
            DisposeAllControls();

            switch (opcao)
            {
                case uninfeOpcoes.opCadastro:
                    metroTile_CAD_Click(null, null);
                    break;

                case uninfeOpcoes.opConfiguracoes:
                    metroTile_CFG_Click(null, null);
                    break;

                case uninfeOpcoes.opServicos:
                    metroTile_STA_Click(null, null);
                    break;

                case uninfeOpcoes.opSobre:
                    metroTile_sobre_Click(null, null);
                    break;

                case uninfeOpcoes.opValidarXML:
                    metroTile_VAL_Click(null, null);
                    break;

                case uninfeOpcoes.opLogs:
                    metroTile_log_Click(null, null);
                    break;
            }
        }

        private void menu_VisibleChanged(object sender, EventArgs e)
        {
            if (!Propriedade.EncerrarApp)
            {
                UpdateControles();

                DisposeAllControls();
            }
        }

        private void DisposeAllControls()
        {
            var Components = uninfeDummy.mainForm.Controls.Cast<object>()
                                                   .Where(obj => !ReferenceEquals(obj, this))
                                                   .OfType<MetroFramework.Controls.MetroUserControl>();
            foreach (var c in Components)
            {
                c.Dispose();
            }
        }

        private bool createControl(Type user)
        {
            bool _novo = false;

            //NFe.UI.uninfeDummy.showError = false;
            VisibleChanged -= menu_VisibleChanged;
            try
            {
                UserControl1 _uc = null;
                ///
                /// processo já existe?
                ///
                foreach (Control __uc in uninfeDummy.mainForm.Controls)
                {
                    if (__uc.GetType().Equals(user))
                    {
                        _uc = __uc as UserControl1;
                        break;
                    }
                }

                _novo = (_uc == null);
                if (_uc == null)
                {
                    ///
                    /// cria o processo
                    _uc = Activator.CreateInstance(user) as UserControl1;
                }
                _uc.UpdateControles();
                if (_novo)
                {
                    uninfeDummy.mainForm.Controls.Add(_uc);
                }
                var Components = uninfeDummy.mainForm.Controls.Cast<object>()
                                                       .Where(obj => !ReferenceEquals(obj, _uc))
                                                       .OfType<MetroFramework.Controls.MetroUserControl>();
                foreach (Control ctrl in Components)
                {
                    ctrl.Visible = false;
                }
                _uc.Visible = true;
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                VisibleChanged += menu_VisibleChanged;
                //NFe.UI.uninfeDummy.showError = true;
            }
            return _novo;
        }

        private void metroTile_CFG_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ConfiguracaoApp.SenhaConfig) && uninfeDummy.TempoExpirou())
            {
                if (!FormSenha.SolicitaSenha(true))
                    return;

                uninfeDummy.UltimoAcessoConfiguracao = DateTime.Now;
            }
            createControl(typeof(userConfiguracoes));
        }

        private void metroTile_log_Click(object sender, EventArgs e)
        {
            createControl(typeof(userLogs));
        }

        private void metroTile_sobre_Click(object sender, EventArgs e)
        {
            createControl(typeof(userSobre));
        }

        private void metroTile_VAL_Click(object sender, EventArgs e)
        {
            createControl(typeof(userValidaXML));
        }

        private void metroTile_update_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ConfiguracaoApp.SenhaConfig) && uninfeDummy.TempoExpirou())
            {
                if (!FormSenha.SolicitaSenha(true))
                    return;

                uninfeDummy.UltimoAcessoConfiguracao = DateTime.Now;
            }

            Formularios.FormUpdate FormUp = new Formularios.FormUpdate();
            FormUp.ShowDialog();
            FormUp.Dispose();
        }

        private void metroTile_STA_Click(object sender, EventArgs e)
        {
            createControl(typeof(userPedidoSituacao));
        }

        private void metroTile_CAD_Click(object sender, EventArgs e)
        {
            createControl(typeof(userCadastro));
        }

        private void metroLink3_Click(object sender, EventArgs e)
        {
            StartPage("http://" + ConfiguracaoApp.Site);
        }

        private void StartPage(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            CurrentTile.Style = (MetroFramework.MetroColorStyle)Convert.ToInt16(e.ClickedItem.Tag);
            CurrentTile.Refresh();

            uninfeDummy.xmlParams.WriteValue(Name + "\\" + CurrentTile.Name, "color", (int)CurrentTile.Style);

            CurrentTile = null;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (CurrentTile != null)
            {
                foreach (var item in contextMenuStrip1.Items)
                {
                    try
                    {
                        if (item.ToString().Equals("Restaurar Padrão"))
                            continue;

                        ((ToolStripMenuItem)item).Checked = Convert.ToInt16(((ToolStripMenuItem)item).Tag) == (int)CurrentTile.Style;
                    }
                    catch
                    {
                    }
                }
            }
        }

        private MetroFramework.Controls.MetroTile CurrentTile = null;

        private void metroTile_Configuracoes_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentTile = (MetroFramework.Controls.MetroTile)sender;
        }

        private void metroTile_Manual_Click(object sender, EventArgs e)
        {
            StartPage(Propriedade.UrlManualUniNFe);
        }

        private void metroTile_Layout_Click(object sender, EventArgs e)
        {
            string pdffile = System.IO.Path.GetTempFileName() + ".pdf";
            try
            {
                new NFe.Service.TaskLayouts().CriaPDFLayout(pdffile);
                if (System.IO.File.Exists(pdffile))
                    StartPage(pdffile);
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}