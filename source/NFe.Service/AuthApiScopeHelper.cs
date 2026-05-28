using NFe.Settings;
using System;
using System.Xml;
using Unimake.AuthServer.Security.Scope;
using Unimake.Primitives.UDebug;

namespace NFe.Service
{
    public static class AuthApiScopeHelper
    {
        public static bool GetTesting(XmlElement tags)
        {
            var testing = false;

            if (tags.GetElementsByTagName("Testing").Count > 0)
            {
                testing = Convert.ToBoolean(tags.GetElementsByTagName("Testing")[0].InnerText);
            }

            return testing;
        }

        public static bool ResolveUseHomologServer(XmlElement tags, XmlElement serviceElement)
        {
            var testing = GetTesting(tags);
            return ResolveUseHomologServer(testing, serviceElement);
        }

        public static bool ResolveUseHomologServer(bool testing, XmlElement serviceElement)
        {
            var useHomologServer = testing;

            if (!testing)
            {
                if (serviceElement.GetElementsByTagName("UseHomologServer").Count > 0)
                {
                    useHomologServer = Convert.ToBoolean(serviceElement.GetElementsByTagName("UseHomologServer")[0].InnerText);
                }
            }

            return useHomologServer;
        }

        public static DebugScope<DebugStateObject> CreateDebugScopeIfNeeded(bool useHomologServer, string anotherServerUrl)
        {
            if (!useHomologServer)
            {
                return null;
            }

            return new DebugScope<DebugStateObject>(new DebugStateObject
            {
                AuthServerUrl = "https://auth.sandbox.unimake.software/api/auth/",
                AnotherServerUrl = anotherServerUrl
            });
        }

        public static AuthenticatedScope CreateAuthenticatedScope(string appId, string secret)
        {
            return new AuthenticatedScope(new Unimake.Primitives.Security.Credentials.AuthenticationToken
            {
                AppId = appId,
                Secret = secret
            });
        }

        public static AuthenticatedScope CreateAuthenticatedScopeEBank(int emp)
        {
            return CreateAuthenticatedScope(Empresas.Configuracoes[emp].AppID, Empresas.Configuracoes[emp].Secret);
        }

        public static AuthenticatedScope CreateAuthenticatedScopeUMessenger(int emp)
        {
            return CreateAuthenticatedScope(
                Empresas.Configuracoes[emp].MesmosDadosEb_Mb ? Empresas.Configuracoes[emp].AppID : Empresas.Configuracoes[emp].AppID_UMessenger,
                Empresas.Configuracoes[emp].MesmosDadosEb_Mb ? Empresas.Configuracoes[emp].Secret : Empresas.Configuracoes[emp].Secret_UMessenger);
        }
    }
}
