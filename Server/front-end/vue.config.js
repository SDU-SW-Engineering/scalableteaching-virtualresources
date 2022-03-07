module.exports = {
    devServer: {
        disableHostCheck: true,
        proxy: 'https://test.virtualresources.sdu.dk',
        port:8080,
        public: '0.0.0.0:8080'
    },
    chainWebpack: config => {
        config
            .plugin('html')
            .tap(args => {
                args[0].title = 'Virtual Resources'
                return args
            })
    }
}