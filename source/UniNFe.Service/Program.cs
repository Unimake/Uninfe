using NFe.Components;
using NFe.Components.Info;
using NFe.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using Topshelf;

namespace UniNFe.Service
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += Unimake.Business.DFe.Xml.AssemblyResolver.AssemblyResolve;

                //Esta deve ser a primeira linha do Main, não coloque nada antes dela. Wandrey 31/07/2009
                Propriedade.AssemblyEXE = Assembly.GetExecutingAssembly();

                using (var identidade = WindowsIdentity.GetCurrent())
                using (var processo = Process.GetCurrentProcess())
                {
                    WriteLog($"[Inicialização do serviço] Conta efetiva: {identidade.Name}; " +
                        $"autenticação: {identidade.AuthenticationType}; execução interativa: {Environment.UserInteractive}; " +
                        $"PID: {processo.Id}; sessão: {processo.SessionId}; " +
                        $"processo de 64 bits: {Environment.Is64BitProcess}; CLR: {Environment.Version}; " +
                        $"executável: {Propriedade.AssemblyEXE.Location}; diretório atual: {Directory.GetCurrentDirectory()}.");
                }

                WriteLog("[Inicialização do serviço] Verificando configurações e bloqueios de execução.");
                if (Aplicacao.UniNFeSevicoAppExecutando())
                {
                    WriteLog("[Inicialização do serviço] Execução interrompida pela verificação de instância em execução.");
                    return;
                }

                ///
                /// https://macoratti.net/18/05/c_servtop1.htm
                ///
                /// http://topshelf-project.com/
                /// https://github.com/Topshelf/Topshelf
                /// 
                HostFactory.Run(p =>
                {
                    p.Service<UniNFeService>(s =>
                    {
                        s.ConstructUsing(st => new UniNFeService());
                        s.WhenContinued(st => st.Start());
                        s.WhenStarted(st => st.Start());

                        s.WhenPaused(st => st.Stop());
                        s.WhenStopped(st => st.Stop());
                        s.WhenShutdown(st => st.OnShutdown());
                    });
                    p.RunAsLocalService();

                    p.SetDescription("UniNFe - Nota fiscal eletrônica");
                    p.SetDisplayName("UniNFeServico");
                    p.SetServiceName("UniNFeServico");
                });
            }
            catch (Exception ex)
            {
                WriteLog($"Erro crítico no serviço: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Gerar LOG
        /// </summary>
        /// <param name="msg">Mensagem a ser grava no LOG</param>
        public static void WriteLog(string msg)
        {
            Auxiliar.WriteLog(msg, false, true);
        }
    }
}
