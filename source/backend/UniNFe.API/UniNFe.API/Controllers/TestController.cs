using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.NFe;
using Unimake.Business.DFe.Xml.NFe;

namespace UniNFe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            //var xml = new ConsStatServ
            //{
            //    Versao = "4.00",
            //    CUF = UFBrasil.SE,
            //    TpAmb = TipoAmbiente.Homologacao
            //};

            //var configuracao = new Configuracao
            //{
            //    TipoDFe = TipoDFe.NFe,
            //    TipoEmissao = TipoEmissao.Normal,
            //    HasProxy = false,
            //    ProxyAutoDetect = false,
            //    ProxyUser = "",
            //    ProxyPassword = ""
            //};

            //var statusServico = new StatusServico(xml, configuracao);
            //statusServico.Executar();

            var fi = new FileInfo(@"c:\teste\certificado.pfx");
            var senha = "";

            var x509Cert = new X509Certificate2();

            using (var fs = fi.OpenRead())
            {
                var buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                x509Cert = new X509Certificate2(buffer, senha);
            }

            return Ok();
        }
    }
}