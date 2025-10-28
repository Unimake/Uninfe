ECHO OFF
CHCP 65001
::Variáveis
SET filesDir=D:\projetos\github\Unimake.DFe\source\Unimake.DFe\Compilacao\INTEROP_Release\
SET istool="c:\Program Files (x86)\Inno Setup 6\ISCC.exe"
SET "caminhoAssinar=\\192.168.0.48\assinar"

::Prepara
DEL /S /Q %cd%\err
RD /S /Q %filesDir%

@ECHO ----------------------------------------------------------------------------------
@ECHO Compilando Unimake.DFe
@ECHO ----------------------------------------------------------------------------------

dotnet build D:\projetos\github\Unimake.DFe\source\Unimake.DFe.sln --configuration INTEROP_Release --force

pause

@ECHO:
@ECHO:
@ECHO:
@ECHO:
@ECHO:
@ECHO ----------------------------------------------------------------------------------
@ECHO Limpando diretório de release
@ECHO ----------------------------------------------------------------------------------

::Apaga os arquivos desnecessários
DEL /S /Q %filesDir%\*.xml
DEL /S /Q %filesDir%\*.pdb
DEL /S /Q %filesDir%\*.json
DEL /S /Q %filesDir%\App.config
DEL /S /Q %filesDir%\TesteDLL_Unimake.Business.DFe.exe
DEL /S /Q %filesDir%\Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll
DEL /S /Q %filesDir%\TesteDLL_Unimake.Business.DFe.exe.config
:: Esta dll tem que pegar da pasta do VB6
DEL /S /Q %filesDir%\System.Security.Cryptography.Xml.dll 

::Apaga os arquivos desnecessários
DEL /S /Q %filesDir%\net462\*.xml
DEL /S /Q %filesDir%\net462\*.pdb
DEL /S /Q %filesDir%\net462\*.json
DEL /S /Q %filesDir%\net462\App.config

::Apaga os arquivos desnecessários
DEL /S /Q %filesDir%\net472\*.xml
DEL /S /Q %filesDir%\net472\*.pdb
DEL /S /Q %filesDir%\net472\*.json
DEL /S /Q %filesDir%\net472\App.config
DEL /S /Q %filesDir%\net472\Unimake.Business.DFe.dll

::Copia a Unimake.Utils
copy C:\projetos\github\Unimake.DFe\source\Unimake.DFe.Test\bin\Release\netcoreapp3.1\Unimake.Utils.dll %filesDir%\netstandard2.0
copy C:\projetos\github\Unimake.DFe\source\Unimake.DFe.Test\bin\Release\netcoreapp3.1\Unimake.Cryptography.dll %filesDir%\netstandard2.0
copy C:\projetos\github\Unimake.DFe\source\Unimake.DFe.Test\bin\Release\netcoreapp3.1\Unimake.Extensions.dll %filesDir%\netstandard2.0

::Ações
@ECHO ----------------------------------------------------------------------------------
@ECHO Assinando executáveis e dlls
@ECHO ----------------------------------------------------------------------------------
copy %filesDir%\netstandard2.0 \\192.168.0.48\assinar\arquivos

echo. > "%caminhoAssinar%\assinar.txt"

Goto loopAssinarStandard

:loopAssinarStandard
   echo Aguardando arquivos serem assinados...
   timeout /t 5 >nul  
   if not exist "%caminhoAssinar%\assinado.txt" goto loopAssinarStandard
   del "%caminhoAssinar%\assinado.txt" /q
   goto copiarStandard

:copiarStandard

   copy \\192.168.0.48\assinar\arquivos %filesDir%\netstandard2.0
   del \\192.168.0.48\assinar\arquivos\*.dll

   copy %filesDir%\net472 \\192.168.0.48\assinar\arquivos
   
   echo. > "%caminhoAssinar%\assinar.txt"
   
   goto loopAssinarNet472
   
:loopAssinarNet472
   echo Aguardando arquivos serem assinados...
   timeout /t 5 >nul  
   if not exist "%caminhoAssinar%\assinado.txt" goto loopAssinarNet472
   del "%caminhoAssinar%\assinado.txt" /q
   goto copiarNet472
   
:copiarNet472   
   copy \\192.168.0.48\assinar\arquivos %filesDir%\net472
   del \\192.168.0.48\assinar\arquivos\*.dll
	
	goto gerarSetup

:gerarSetup
@ECHO ----------------------------------------------------------------------------------
@ECHO Compilando script
@ECHO ----------------------------------------------------------------------------------

CALL %istool% Unimake.DFe.iss


@ECHO:
@ECHO ----------------------------------------------------------------------------------
@ECHO Verifique as mensagens de erro. Pressione CTRL+C para terminar a compilação ou ...
@ECHO ----------------------------------------------------------------------------------

@ECHO:
@ECHO:
@ECHO:
@ECHO:
@ECHO:
@ECHO:
@ECHO ----------------------------------------------------------------------------------
@ECHO Assinando o instalador
@ECHO ----------------------------------------------------------------------------------

copy d:\projetos\instaladores\Install_Unimake.DFe.exe \\192.168.0.48\assinar\arquivos\Install_Unimake.DFe.exe
echo. > "%caminhoAssinar%\assinar.txt"

:loopAssinarSetup
   echo Aguardando arquivos serem assinados...
   timeout /t 5 >nul  
   if not exist "%caminhoAssinar%\assinado.txt" goto loopAssinarSetup
   del "%caminhoAssinar%\assinado.txt" /q

   copy \\192.168.0.48\assinar\arquivos\Install_Unimake.DFe.exe d:\projetos\instaladores\Install_Unimake.DFe.exe
   del \\192.168.0.48\assinar\arquivos\Install_Unimake.DFe.exe
   python "c:\program files (x86)\s3cmd\s3cmd" put "D:\projetos\instaladores\Install_Unimake.DFe.exe" s3://unimakedownload/Install_Unimake.DFe.exe --acl-public
   call sendftp.bat "Install_Unimake.DFe.exe"
  
   goto ok

:ok
 exit /B 0