using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.CIOT;
using Xunit;
using SituacaoServico = Unimake.Business.DFe.Servicos.CIOT.ConsultarSituacaoTransportador;

namespace UniNFe.Test.CIOT
{
    public class EFreteTransporteHttpTests
    {
        [Fact]
        public async Task ConsultaSituacaoEnviaGetComCorpoNoNet481()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                var porta = ((IPEndPoint)listener.LocalEndpoint).Port;
                var servidor = ReceberRequisicao(listener);
                var configuracao = new Configuracao
                {
                    TipoAmbiente = TipoAmbiente.Homologacao,
                    EFreteIntegrador = "INTEGRADOR-TESTE",
                    EFreteToken = "TOKEN-TESTE"
                };
                var envio = new ConsultarSituacaoTransportador
                {
                    ProvedorCIOT = ProvedorCIOT.EFrete,
                    CpfCnpjInteressado = "12345678000199",
                    CpfCnpjTransportador = "12345678901",
                    RNTRCTransportador = "123456789",
                    PlacasConsulta = new System.Collections.Generic.List<string> { "BRA2E19" }
                };
                var servico = new SituacaoServico(envio, configuracao);
                configuracao.RequestURI = "http://127.0.0.1:" + porta + "/services/Pef/ConsultaSituacaoTransportador";

                servico.Executar();

                var requisicao = await servidor;
                Assert.Equal("GET", requisicao.Metodo);
                Assert.Contains("TOKEN-TESTE", requisicao.Corpo);
                Assert.Equal("PROTO-NET481", servico.Result.Protocolo);
                Assert.True(servico.Result.RNTRCAtivo);
            }
            finally
            {
                listener.Stop();
            }
        }

        private static async Task<RequisicaoRecebida> ReceberRequisicao(TcpListener listener)
        {
            using (var cliente = await listener.AcceptTcpClientAsync())
            using (var stream = cliente.GetStream())
            using (var leitor = new StreamReader(stream, Encoding.ASCII, false, 1024, true))
            {
                var linhaInicial = await leitor.ReadLineAsync();
                var tamanho = 0;
                string linha;
                while (!string.IsNullOrEmpty(linha = await leitor.ReadLineAsync()))
                {
                    if (linha.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    {
                        tamanho = int.Parse(linha.Substring(linha.IndexOf(':') + 1).Trim());
                    }
                }

                var buffer = new char[tamanho];
                var total = 0;
                while (total < tamanho)
                {
                    var lidos = await leitor.ReadAsync(buffer, total, tamanho - total);
                    if (lidos == 0)
                    {
                        break;
                    }
                    total += lidos;
                }

                var json = "{\"Sucesso\":true,\"ProtocoloServico\":\"PROTO-NET481\",\"CpfOuCnpj\":\"12345678901\",\"RNTRC\":\"123456789\",\"RNTRCAtivo\":true,\"TACouEquiparado\":false}";
                var conteudo = Encoding.UTF8.GetBytes(json);
                var cabecalho = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: " + conteudo.Length + "\r\nConnection: close\r\n\r\n");
                await stream.WriteAsync(cabecalho, 0, cabecalho.Length);
                await stream.WriteAsync(conteudo, 0, conteudo.Length);

                return new RequisicaoRecebida
                {
                    Metodo = linhaInicial.Split(' ')[0],
                    Corpo = new string(buffer, 0, total)
                };
            }
        }

        private sealed class RequisicaoRecebida
        {
            internal string Metodo { get; set; }

            internal string Corpo { get; set; }
        }
    }
}

