@echo off
 setlocal

 if /i "%1"=="uninfe" goto uninfe
 if /i "%1"=="dll" goto dll

 echo %1 não foi localizado
 goto fim 
 
:uninfe
   set "caminhoAssinar=\\192.168.0.48\assinar"
   set "caminhoAssinarArquivos=%caminhoAssinar%\arquivos"
   set "caminhoDLLRelease=..\source\uninfe\bin\release"
   
   goto copiarDLLReleaseAssinar
   
:copiarDLLReleaseAssinar
   cls
   copy "%caminhoDLLRelease%\*.dll" "%caminhoAssinarArquivos%" /y
   copy "%caminhoDLLRelease%\*.exe" "%caminhoAssinarArquivos%" /y
  
   echo. > "%caminhoAssinar%\assinar.txt"

   goto loopAssinadoRelease
   
:copiarDLLRelease
   cls
   copy "%caminhoAssinarArquivos%\*.dll" "%caminhoDLLRelease%" /y
   copy "%caminhoAssinarArquivos%\*.exe" "%caminhoDLLRelease%" /y
   del "%caminhoAssinarArquivos%\*.dll" /q
   del "%caminhoAssinarArquivos%\*.exe" /q
   
   goto gerarInstaladores
   
:gerarInstaladores   
   cls
   "c:\Program Files (x86)\Inno Setup 6\ISCC.exe" UniNFe.iss
	pause
	
   copy "C:\projetos\instaladores\iuninfe5.exe" %caminhoAssinarArquivos%
   echo. > "%caminhoAssinar%\assinar.txt"
   goto loopGerarInstaladores
   
:copiarInstaladores
   copy "%caminhoAssinarArquivos%\iuninfe5.exe" "C:\projetos\instaladores" /y
   del "%caminhoAssinarArquivos%\*.dll" /q
   del "%caminhoAssinarArquivos%\*.exe" /q
   
   if /i "%2"=="beta" goto beta   
   goto release   

:loopAssinadoRelease
   cls
   echo Aguardando arquivos serem assinados...
   timeout /t 5 >nul  
   if not exist "%caminhoAssinar%\assinado.txt" goto loopAssinadoRelease
   del "%caminhoAssinar%\assinado.txt" /q
   goto copiarDLLRelease
   
:loopGerarInstaladores
   cls
   echo Aguardando arquivos serem assinados...
   timeout /t 5 >nul  
   if not exist "%caminhoAssinar%\assinado.txt" goto loopGerarInstaladores
   del "%caminhoAssinar%\assinado.txt" /q
   goto copiarInstaladores
   
:beta
   del c:\projetos\instaladores\iuninfe5_beta.exe
   copy c:\projetos\instaladores\iuninfe5.exe c:\projetos\instaladores\iuninfe5_beta.exe
  
   python "c:\program files (x86)\s3cmd\s3cmd" put c:\projetos\instaladores\iuninfe5_beta.exe s3://unimakedownload/iuninfe5_beta.exe --acl-public
   
   call sendftp.bat "iuninfe5_beta.exe"
   
   goto fim

:release   
	cls
   python "c:\program files (x86)\s3cmd\s3cmd" put c:\projetos\instaladores\iuninfe5.exe s3://unimakedownload/iuninfe5.exe --acl-public
	pause 
   
	cls
   call sendftp.bat "iuninfe5.exe"
	pause
   
   goto fim
   
:dll
   set "caminhoAssinar=\\192.168.0.48\assinar"
   set "caminhoAssinarArquivos=%caminhoAssinar%\arquivos"

   copy "c:\projetos\github\Unimake.DFe\source\Unimake.DFe\Compilacao\Release\netstandard2.0\Unimake.Business.DFe.dll" "%caminhoAssinarArquivos%" /y
   copy "c:\projetos\github\Unimake.DFe\source\Unimake.DFe\Compilacao\Release\net472\Unimake.Security.Platform.dll" "%caminhoAssinarArquivos%" /y
   
   echo. > "%caminhoAssinar%\assinar.txt"   
   
   goto loopAssinandoDLL
   
:dllCopiarAssinado   
   copy "%caminhoAssinarArquivos%\Unimake.Business.DFe.dll" "c:\projetos\github\Unimake.DFe\source\Unimake.DFe\Compilacao\Release\netstandard2.0" /y
   copy "%caminhoAssinarArquivos%\Unimake.Security.Platform.dll" "c:\projetos\github\Unimake.DFe\source\Unimake.DFe\Compilacao\Release\net472" /y

   ::Limpar arquivos   
   del "%caminhoAssinarArquivos%\*.dll" /q
   del "%caminhoAssinarArquivos%\*.exe" /q
   
   goto fim

:loopAssinandoDLL
   cls
   echo Aguardando arquivos serem assinados...
   timeout /t 5 >nul  
   if not exist "%caminhoAssinar%\assinado.txt" goto loopAssinandoDLL
   del "%caminhoAssinar%\assinado.txt" /q
   goto dllCopiarAssinado
   
:fim