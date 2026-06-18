@ECHO OFF

ECHO Removendo registro 32 Bits
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe Unimake.Business.DFe.dll /unregister
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe Unimake.Security.Platform.dll /unregister

ECHO Removendo registro 64 Bits
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe Unimake.Business.DFe.dll /unregister
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe Unimake.Security.Platform.dll /unregister

EXIT 0