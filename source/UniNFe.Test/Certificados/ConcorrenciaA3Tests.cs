using NFe.Settings;
using NFe.Threadings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Xunit;

namespace UniNFe.Test.Certificados
{
    [Collection("Certificados Serial")]
    public class ConcorrenciaA3Tests : IDisposable
    {
        private readonly List<Empresa> configuracoesAnteriores;

        public ConcorrenciaA3Tests()
        {
            configuracoesAnteriores = Empresas.Configuracoes;
            Empresas.Configuracoes = new List<Empresa>
            {
                CriarEmpresa("1111", "AA11"),
                CriarEmpresa("2222", "BB22")
            };
        }

        [Fact]
        public void CertificadosA3DistintosNuncaExecutamSimultaneamente()
        {
            var executando = 0;
            var maximo = 0;
            ThreadItem.ThreadStartHandler inicio = item =>
            {
                var atual = Interlocked.Increment(ref executando);
                if (atual > maximo) maximo = atual;
                Thread.Sleep(80);
                Interlocked.Decrement(ref executando);
            };

            ThreadItem.OnStarted += inicio;
            try
            {
                var primeiro = CriarThreadItem(0, "a.xml");
                var segundo = CriarThreadItem(1, "b.xml");
                var thread1 = new Thread(primeiro.Run);
                var thread2 = new Thread(segundo.Run);

                thread1.Start();
                thread2.Start();

                Assert.True(thread1.Join(3000));
                Assert.True(thread2.Join(3000));
                Assert.Equal(1, maximo);
            }
            finally
            {
                ThreadItem.OnStarted -= inicio;
            }
        }

        [Fact]
        public void ExcecaoLiberaSemaforoParaOperacaoSeguinte()
        {
            var chamadas = 0;
            ThreadItem.ThreadStartHandler inicio = item =>
            {
                if (Interlocked.Increment(ref chamadas) == 1)
                {
                    throw new InvalidOperationException("Falha simulada durante operação A3.");
                }
            };

            ThreadItem.OnStarted += inicio;
            try
            {
                var thread1 = new Thread(CriarThreadItem(0, "falha.xml").Run);
                var thread2 = new Thread(CriarThreadItem(1, "seguinte.xml").Run);

                thread1.Start();
                Assert.True(thread1.Join(3000));
                thread2.Start();

                Assert.True(thread2.Join(3000));
                Assert.Equal(2, chamadas);
            }
            finally
            {
                ThreadItem.OnStarted -= inicio;
            }
        }

        public void Dispose()
        {
            Empresas.Configuracoes = configuracoesAnteriores;
        }

        private static Empresa CriarEmpresa(string pin, string thumbprint)
        {
            return new Empresa
            {
                UsaCertificado = true,
                CertificadoPIN = pin,
                CertificadoDigitalThumbPrint = thumbprint,
                X509Certificado = new X509Certificate2()
            };
        }

        private static ThreadItem CriarThreadItem(int empresa, string nome)
        {
            return new ThreadItem(new FileInfo(Path.Combine(Path.GetTempPath(), nome)), empresa);
        }
    }
}
