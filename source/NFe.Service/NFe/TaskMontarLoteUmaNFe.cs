using NFe.Components;
using NFe.Settings;
using System;
using System.IO;

namespace NFe.Service
{
    /// <summary>
    /// Executar as tarefas pertinentes a assinatura e montagem do lote de uma única nota fiscal eletrônica
    /// </summary>
    public class TaskNFeMontarLoteUmaNFe : TaskAbst
    {
        public TaskNFeMontarLoteUmaNFe(string arquivo)
        {
            Servico = Servicos.NFeMontarLoteUma;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            try
            {
                var emp = Empresas.FindEmpresaByThread();

                var oDadosNfe = LerXMLNFe(ConteudoXML);

                ValidarXMLDFe(ConteudoXML);

                //Montar lote de nfe
                var oFluxoNfe = new FluxoNfe();

                var cError = "";
                try
                {
                    if (!oFluxoNfe.NFeComLote(oDadosNfe.chavenfe))
                    {
                        var nomeArqEmProcessamento = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado, PastaEnviados.EmProcessamento.ToString(), oFluxoNfe.LerTag(oDadosNfe.chavenfe, FluxoNfe.ElementoFixo.ArqNFe));                        
                        var fi = new FileInfo(NomeArquivoXML);

                        //Se não encontrar o nome do arquivo da NFe no FluxoNFe.XML, vou tentar pegar o nome do arquivo pelos XMLs que estão na pasta TEMP
                        if (string.IsNullOrWhiteSpace(nomeArqEmProcessamento))
                        {
                            nomeArqEmProcessamento = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado, PastaEnviados.EmProcessamento.ToString(), fi.Name);
                        }

                        if (!File.Exists(nomeArqEmProcessamento))
                        {
                            var xmlLote = LoteNfe(ConteudoXML, NomeArquivoXML, oDadosNfe.versao, oDadosNfe.mod);

                            var nfeRecepcao = new TaskNFeRecepcao(xmlLote);
                            nfeRecepcao.Execute();
                        }
                        else
                        {
                            // Gravar log e aguardar para tentar novamente na próxima execução
                            Auxiliar.WriteLog($"Já existe um arquivo com o nome '{fi.Name}' em processamento ({nomeArqEmProcessamento}). Aguarde a conclusão antes de tentar gerar um novo XML com o mesmo nome.", false);
                        }
                    }
                    else
                    {
                        // Gravar log e tentar processar novamente na próxima execução
                        Auxiliar.WriteLog($"Já existe um XML com a chave '{oDadosNfe.chavenfe}' em processamento. Aguarde a conclusão antes de tentar gerar o mesmo documento novamente.", false);
                    }
                }
                catch (IOException ex)
                {
                    cError = (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                }
                catch (Exception ex)
                {
                    cError = (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                }

                if (!string.IsNullOrEmpty(cError))
                {
                    try
                    {
                        // grava o arquivo de erro
                        oAux.GravarArqErroERP(Path.GetFileNameWithoutExtension(NomeArquivoXML) + ".err", cError);
                        // move o arquivo para a pasta de erro
                        oAux.MoveArqErro(NomeArquivoXML);
                    }
                    catch
                    {
                        // A principio não vou fazer nada Wandrey 24/04/2011
                    }
                }
            }
            catch { }
        }
    }
}