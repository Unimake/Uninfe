@echo off
setlocal enabledelayedexpansion

:loop
 cls
 
 set arquivoAssinarTXT=c:\assinar\assinar.txt
 set pastaArquivos=\assinar\arquivos
 set signTool="c:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
 set signToolParams=sign /n "Unimake Solucoes Corporativas LT" /fd SHA256 /t http://timestamp.digicert.com
 
 @echo Apos 5 segundos, se existir o c:\assinar\assinar.txt, vamos assinar os arquivos.

 timeout /t 1 >nul  
 if not exist "%arquivoAssinarTXT%" goto loop

 set "listaArquivos="

 for %%F in ("%pastaArquivos%\*.*") do (
     set "listaArquivos=!listaArquivos!%%~nxF "
 )

 start c:\assinar\sendKeys.vbs
 
 cd %pastaArquivos%
 
 call %signTool% %signToolParams% %listaArquivos% 

 cd\assinar
 del %arquivoAssinarTXT% 
 echo . > "c:\assinar\assinado.txt"   
 
 goto loop

:fim
 endlocal