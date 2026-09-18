@echo off
rem A Unimake.Business.DFe usa SolutionDir para localizar seu evento de pos-build.
rem Quando compilada pela solucao do UniNFe, SolutionDir aponta para esta pasta.
call "%~dp0..\..\Unimake.DFe\source\Build.bat" %*
