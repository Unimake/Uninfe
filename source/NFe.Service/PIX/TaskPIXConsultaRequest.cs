using NFe.Components;

namespace NFe.Service
{
    public class TaskPIXConsultaRequest : TaskPIXGetRequest
    {
        public TaskPIXConsultaRequest(string arquivo) : base(arquivo)
        {
            Servico = Servicos.PIXConsultaRequest;
        }
    }
}
