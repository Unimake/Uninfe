module.exports  = {
    publicPath: process.env.NODE_ENV  ===  'production'  ?  './'  :  '/',
    transpileDependencies: [
      'vuetify'
    ],
    pluginOptions: {
        electronBuilder: {
            nodeIntegration: true
        }
    }
}