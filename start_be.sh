#!/bin/sh
cd ~/code/Uninfe/source/backend/UniNFe.API/UniNFe.Database
dotnet ef database update
cd ~/code/Uninfe/source/backend/UniNFe.API/UniNFe.API
dotnet run