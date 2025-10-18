ECHO Assinando arquivo %1
SET signtool="C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"

SET signtoolParams=sign /f "d:\projetos\unimake-codesign-EXE-DLL.pfx" /p uni-123456! /t http://timestamp.comodoca.com/authenticode

CALL %signtool% %signtoolParams% %1