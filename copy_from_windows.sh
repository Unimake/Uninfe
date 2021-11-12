mkdir -p ~/code
rm -r /media/windows/Uninfe/source/backend/UniNFe.API/UniNFe.API/bin
rm -r /media/windows/Uninfe/source/backend/UniNFe.API/UniNFe.API/obj
rm -r /media/windows/Uninfe/source/backend/UniNFe.API/UniNFe.Database/bin
rm -r /media/windows/Uninfe/source/backend/UniNFe.API/UniNFe.Database/obj
rm -r /media/windows/Uninfe/source/frontend/uninfe-app/node_modules
cp -R /media/windows/Uninfe ~/code/
cd ~/code/Uninfe/source/frontend/uninfe-app
exec bash
npm install
cd ~/code/Uninfe