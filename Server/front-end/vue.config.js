const { defineConfig } = require('@vue/cli-service')
const NodePolyfillPlugin = require("node-polyfill-webpack-plugin")

module.exports = {
    devServer: {
        disableHostCheck: true,
        proxy: 'https://test.virtualresources.sdu.dk',
        port:8080,
        public: '0.0.0.0:8080'
    },
    transpileDependencies:true,
    configureWebpack: {
        plugins: [
            new NodePolyfillPlugin()
        ]
    },
    chainWebpack: config => {
        config
            .plugin('html')
            .tap(args => {
                args[0].title = 'Virtual Resources'
                return args
            })
        config.resolve
    }
}
