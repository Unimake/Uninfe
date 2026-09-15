# Auditoria de integração — UAB-004

- Raiz/sufixo: `consStatServNFeABI` + `-ped-sta.xml`.
- Namespace/versão: `http://www.portalfiscal.inf.br/nfeabi`, 1.00.
- Retorno/erro: `-sta.xml` e `-sta.err`.
- DLL: `Unimake.Business.DFe.Servicos.NFeABI.StatusServico`, consumida por ProjectReference em Debug.
- Única chamada fiscal: `StatusServico.Executar()`; o diagnóstico não dispara consulta, DNS, TCP ou TLS adicional.
- Produção: sem endpoint e bloqueada antes do transporte.
- Privacidade: erro e diagnóstico não persistem XML, senha, token, certificado ou endpoint.
- Fora do diff: autorização, protocolo, eventos, UI adicional e UAB-005.

