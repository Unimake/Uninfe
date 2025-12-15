using NFe.Components;
using NFe.Components.Info;
using NFe.Settings;
using System;
using System.Reflection;
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

                if (Aplicacao.UniNFeSevicoAppExecutando())
                {
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
            var o = ConfiguracaoApp.GravarLogOperacoesRealizadas;
            ConfiguracaoApp.GravarLogOperacoesRealizadas = true;
            Auxiliar.WriteLog(msg, false);
            ConfiguracaoApp.GravarLogOperacoesRealizadas = o;
        }
    }
}
