using NFe.Settings;
using System;
using Unimake.AuthServer.Security.Scope;
using Unimake.MessageBroker.Primitives.Model.Notifications;
using Unimake.MessageBroker.Services;

namespace NFe.Service
{
    /// <summary>
    /// Enviar mensagens para o WhatsApp via API u-Messenger
    /// </summary>
    public class SendMessageToWhatsApp
    {
        private int CodigoEmpresa { get; set; }

        public SendMessageToWhatsApp(int codigoEmpresa) => CodigoEmpresa = codigoEmpresa;

        /// <summary>
        /// Enviar mensagens de notificações de alertas
        /// </summary>
        public async void AlertNotification(string text, string title)
        {
            AuthenticatedScope authenticatedScope = null;
            try
            {
                if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[CodigoEmpresa].NumeroUMessenger.Trim()) ||
                    string.IsNullOrWhiteSpace(Empresas.Configuracoes[CodigoEmpresa].AppID.Trim()) ||
                    string.IsNullOrWhiteSpace(Empresas.Configuracoes[CodigoEmpresa].Secret.Trim()))
                {
                    return;
                }

                authenticatedScope = new AuthenticatedScope(new Unimake.Primitives.Security.Credentials.AuthenticationToken
                {
                    AppId = Empresas.Configuracoes[CodigoEmpresa].AppID,
                    Secret = Empresas.Configuracoes[CodigoEmpresa].Secret
                });

                text = text.Replace("\r\n", " ").Replace("  ", " ");
                text = text.Substring(0, (text.Length > 1000 ? 1000 : text.Length));

                var alertNotication = new AlertNotification
                {
                    Testing = false,
                    Text = text,
                    To = new Unimake.MessageBroker.Primitives.Model.Recipient
                    {
                        Destination = Empresas.Configuracoes[CodigoEmpresa].NumeroUMessenger.Trim(),
                    },
                    Title = title
                };

                var messageService = new MessageService(Unimake.MessageBroker.Primitives.Enumerations.MessagingService.WhatsApp);
                await messageService.SendAlertAsync(alertNotication, authenticatedScope);
            }
            catch (Exception ex)
            {
                try
                {
                    Auxiliar.WriteLog("Falha no envio de notificações de alertas para o WhatsApp: " + ex.Message, true);
                }
                catch
                {
                }
            }
            finally
            {
                if (authenticatedScope != null)
                {
                    authenticatedScope.Dispose();
                }
            }
        }
    }
}
