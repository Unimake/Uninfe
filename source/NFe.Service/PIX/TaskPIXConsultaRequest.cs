using EBank.Solutions.Primitives.Billet.Models;
using EBank.Solutions.Primitives.Enumerations;
using EBank.Solutions.Primitives.PIX.Models.Consulta;
using EBank.Solutions.Primitives.PIX.Request.Consulta;
using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.AuthServer.Authentication;
using Unimake.AuthServer.Security.Scope;
using Unimake.EBank.Solutions.Services.PIX;
using Unimake.Primitives.Collections.Page;
using Unimake.Primitives.UDebug;

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