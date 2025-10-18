@Echo Off
set arqftp=%1

If Exist send-ftp-log-%arqftp%.xml del send-ftp-log-%arqftp%.xml

Echo Enviando arquivos para FTP...
"c:\Program Files (x86)\WinSCP\WinSCP.exe" /console /script=c:\projetos\uninfe\trunk\setup\send-ftp.scr /xmllog=c:\projetos\uninfe\trunk\setup\send-ftp-log-%arqftp%.xml /loglevel=0
if errorlevel 1 goto erro
goto fim
 
:Erro
Echo Erros ocorridos na trasferencia, verifique send-ftp-log.xml
Start c:\projetos\uninfe\trunk\setup\send-ftp-log-%arqftp%.xml
Pause
Goto Fim2

:Fim
Echo Transferencias ocorridas com sucesso!!!!
Goto Fim2

:Fim2

