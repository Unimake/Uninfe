using System.Reflection;
using System.Xml;
using NFe.Service;
using Xunit;

namespace UniNFe.Test.eBoleto
{
    public class TaskBoletoInformarPagtoTests
    {
        [Theory]
        [InlineData("<BoletoInformarPagto versao=\"1.00\"><NumeroNoBanco>123</NumeroNoBanco></BoletoInformarPagto>", "BoletoCancelar")]
        [InlineData("<BoletoCancelarResponse><Status>0</Status><Motivo>OK</Motivo></BoletoCancelarResponse>", "BoletoInformarPagtoResponse")]
        public void DeveConverterSomenteRaizMantendoConteudo(string conteudoXml, string nomeRaiz)
        {
            var metodo = typeof(TaskBoletoInformarPagto).GetMethod(
                "ConverterRaiz",
                BindingFlags.Static | BindingFlags.NonPublic);

            var convertido = (string)metodo.Invoke(null, new object[] { conteudoXml, nomeRaiz });
            var original = new XmlDocument();
            var resultado = new XmlDocument();
            original.LoadXml(conteudoXml);
            resultado.LoadXml(convertido);

            Assert.Equal(nomeRaiz, resultado.DocumentElement?.Name);
            Assert.Equal(original.DocumentElement?.InnerXml, resultado.DocumentElement?.InnerXml);
        }
    }
}
