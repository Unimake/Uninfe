using System;
using Xunit;

namespace UniNFe.Test.Abstractions
{
    public class TaskTestFixtureBaseTests : TaskTestFixtureBase
    {
        [Fact]
        public void ExecutarEmThreadPropagaExcecaoDaThreadDeTrabalho()
        {
            var esperada = new InvalidOperationException("falha simulada");

            var recebida = Assert.Throws<InvalidOperationException>(() =>
                ExecutarEmThread("teste", () => { throw esperada; }));

            Assert.Same(esperada, recebida);
        }
    }
}
