# Uninfe

This is a project created as a proof of concept to validate if it is possible to migrate and run Uninfe application to be multi platform (windows/linux/mac) using ElectronJS and .NET 5.0.

## Usage

In order to start the backend and frontend there are a few scripts that can be used to facilitate that process.

### Windows

Currently there are no scripts to be used for Windows, you can use Visual Studio directly to run the backend and nodejs/npm for the frontend.

### Linux/Mac

* Open two terminal windows
* On the first terminal execute: `./start_be.sh`
* On the second terminal execute: `./start_fe.sh`
* The front end will open automatically
* To open swagger page for the API, open the following URL: `https://localhost:5001/swagger/index.html`