using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace UniNFe.Test.Abstractions
{
    public abstract class TaskTestContextBase : IDisposable
    {
        #region Private Fields

        private readonly System.Reflection.Assembly assemblyExeOriginal;
        private readonly List<Empresa> configuracoesOriginais;
        private readonly bool executandoPeloUniNFeOriginal;
        private readonly string pastaExecutavelOriginal;

        #endregion Private Fields

        #region Protected Constructors

        protected TaskTestContextBase(string nomeArquivoEnvio, string xmlEnvio)
        {
            PastaTemporaria = Path.Combine(Path.GetTempPath(), "UniNFe.Test", Guid.NewGuid().ToString("N"));
            PastaRetorno = Path.Combine(PastaTemporaria, "retorno");
            Directory.CreateDirectory(PastaRetorno);

            pastaExecutavelOriginal = Propriedade.PastaExecutavel;
            configuracoesOriginais = Empresas.Configuracoes;
            assemblyExeOriginal = Propriedade.AssemblyEXE;
            executandoPeloUniNFeOriginal = Propriedade.ExecutandoPeloUniNFe;

            ArquivoEnvio = Path.Combine(PastaTemporaria, nomeArquivoEnvio);
            File.WriteAllText(ArquivoEnvio, xmlEnvio, Encoding.UTF8);

            Propriedade.PastaExecutavel = AppDomain.CurrentDomain.BaseDirectory;
            Propriedade.AssemblyEXE = typeof(TaskTestContextBase).Assembly;
            Propriedade.ExecutandoPeloUniNFe = true;
        }

        #endregion Protected Constructors

        #region Protected Methods

        protected void DefinirConfiguracoesEmpresas(List<Empresa> configuracoes)
        {
            Empresas.Configuracoes = configuracoes;
        }

        #endregion Protected Methods

        #region Public Properties

        public string ArquivoEnvio { get; }
        public string PastaRetorno { get; }
        public string PastaTemporaria { get; }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            Empresas.Configuracoes = configuracoesOriginais;
            Propriedade.PastaExecutavel = pastaExecutavelOriginal;
            Propriedade.AssemblyEXE = assemblyExeOriginal;
            Propriedade.ExecutandoPeloUniNFe = executandoPeloUniNFeOriginal;

            if(Directory.Exists(PastaTemporaria))
            {
                Directory.Delete(PastaTemporaria, true);
            }
        }

        #endregion Public Methods
    }

    public abstract class TaskTestFixtureBase
    {
        #region Protected Methods

        protected void ExecutarEmThread(string threadName, Action action)
        {
            var thread = new Thread(() => action())
            {
                Name = threadName
            };

            thread.Start();
            thread.Join();
        }

        #endregion Protected Methods

        #region Public Methods

        public bool AguardarArquivo(string arquivo, TimeSpan timeout)
        {
            var limite = DateTime.UtcNow.Add(timeout);

            while(!File.Exists(arquivo) && DateTime.UtcNow < limite)
            {
                Thread.Sleep(200);
            }

            return File.Exists(arquivo);
        }

        public XmlDocument CarregarXml(string arquivo)
        {
            var xml = new XmlDocument();
            xml.Load(arquivo);
            return xml;
        }

        #endregion Public Methods
    }
}