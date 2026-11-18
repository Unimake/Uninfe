using NFe.Components;
using NFe.Settings;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NFe.UI.Formularios
{
    public partial class FormUpdate : MetroFramework.Forms.MetroForm
    {
        private bool atualizacaoEmAndamento;

        #region Public Constructors

        public FormUpdate() => InitializeComponent();

        #endregion Public Constructors

        #region Eventos

        private void FormUpdate_Load(object sender, EventArgs e)
        {
            ShowTitle();

            prgDownload.Value = 0;
            prgDownload.Visible = false;
            lblProgresso.Text = "";
            uninfeDummy.ClearControls(this, true, true);
        }

        private async void metroButton1_Click(object sender, EventArgs e) => await Atualizar();

        protected async Task Atualizar()
        {
            if (atualizacaoEmAndamento)
            {
                return;
            }

            atualizacaoEmAndamento = true;
            var servicosParados = false;
            UniNFeUpdate update = null;
            try
            {
                if (!ConfirmarAtualizacao())
                {
                    return;
                }

                Auxiliar.WriteLog("Iniciado o processo de Atualização do UniNFe pelo Usuário.", false);
                metroButton1.Enabled = btnAtualizar.Enabled = false;
                prgDownload.Value = 0;
                prgDownload.Visible = true;
                Text = "Baixando a atualização do " + Propriedade.NomeAplicacao;
                lblProgresso.Text = "O monitor permanece ativo durante o download.";

                update = CriarAtualizacao();
                var progresso = new Progress<UpdateProgessEventArgs>(args =>
                {
                    if (!IsDisposed && !Disposing && atualizacaoEmAndamento)
                    {
                        prgDownload.Value = args.ProgressPercentage;
                    }
                });

                await Task.Run(() => update.Baixar(args => ((IProgress<UpdateProgessEventArgs>)progresso).Report(
                    new UpdateProgessEventArgs { BytesRead = args.BytesRead, TotalBytes = args.TotalBytes })));

                if (IsDisposed || Disposing || Propriedade.EncerrarApp)
                {
                    return;
                }

                Auxiliar.WriteLog("Processo de download da atualização do UniNFe foi concluído.", false);
                prgDownload.Value = 100;
                Text = "Instalando a atualização do " + Propriedade.NomeAplicacao;
                lblProgresso.Text = "Parando os serviços para iniciar o instalador.";

                servicosParados = true;
                PararServicos();
                update.InstalarBaixado();
                if (!update.InstaladorIniciado)
                {
                    throw new InvalidOperationException("Não foi possível iniciar o instalador do UniNFe.");
                }

                EncerrarAplicacao();
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("Falha na atualização do UniNFe. " + ex.Message, true, true);
                if (servicosParados && update != null && !update.InstaladorIniciado && !Propriedade.EncerrarApp)
                {
                    try
                    {
                        RetomarServicos();
                    }
                    catch (Exception retomada)
                    {
                        Auxiliar.WriteLog("Falha ao retomar os serviços após erro na atualização. " + retomada.Message, true, true);
                        ex = new InvalidOperationException(ex.Message + "\r\nNão foi possível retomar os serviços: " + retomada.Message, ex);
                    }
                }

                if (!IsDisposed && !Disposing)
                {
                    ExibirFalha(ex);
                }
            }
            finally
            {
                var instaladorIniciado = update != null && update.InstaladorIniciado;
                update?.Dispose();
                atualizacaoEmAndamento = false;
                if (!IsDisposed && !Disposing && !instaladorIniciado)
                {
                    metroButton1.Enabled = btnAtualizar.Enabled = true;
                    prgDownload.Visible = false;
                    lblProgresso.Text = "";
                    ShowTitle();
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (atualizacaoEmAndamento && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        #endregion Eventos

        #region Métodos

        protected virtual bool ConfirmarAtualizacao() => MetroFramework.MetroMessageBox.Show(this,
            "O monitor continuará ativo durante o download. Após a conclusão, os serviços serão parados e o UniNFe será encerrado para instalar a atualização.\r\n\r\nDeseja continuar?", "Atualização do UniNFe",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        protected virtual UniNFeUpdate CriarAtualizacao()
        {
            IWebProxy proxy = null;
            if (ConfiguracaoApp.Proxy)
            {
                proxy = Unimake.Net.Utility.GetProxy(ConfiguracaoApp.ProxyServidor,
                    ConfiguracaoApp.ProxyUsuario, ConfiguracaoApp.ProxySenha,
                    ConfiguracaoApp.ProxyPorta, ConfiguracaoApp.DetectarConfiguracaoProxyAuto);
            }

            return new UniNFeUpdate(proxy);
        }

        protected virtual void PararServicos() => uninfeDummy.mainForm.PararServicos(true);

        protected virtual void RetomarServicos() => uninfeDummy.mainForm.ExecutaServicos();

        protected virtual void EncerrarAplicacao() => Application.Exit();

        protected virtual void ExibirFalha(Exception ex) => MetroFramework.MetroMessageBox.Show(this,
            "Não foi possível concluir a atualização do UniNFe.\r\n\r\nErro: " + ex.Message, "Atualização do UniNFe");

        private void ShowTitle()
        {
            Text = "Atualização do " + Propriedade.NomeAplicacao;
        }

        #endregion Métodos
    }
}
