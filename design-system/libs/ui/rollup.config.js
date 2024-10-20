const url = require('postcss-url')
const postcss = require('rollup-plugin-postcss')

module.exports = (config) => {
  return {
    ...config,
    output: [
      ...config.output.map((output) => ({
        ...output,
        interop: 'compat',
      })),
    ],
    plugins: [
      ...config.plugins.map((plugin) => {
        // replace the original postcss config so that it won't process the css file twice
        if (plugin.name === 'postcss') {
          return postcss({
            modules: false, // set to false to not modify className material-symbols-sharp
            plugins: [
              url({
                url: 'inline',
              }),
            ],
          })
        }

        return plugin
      }),
    ],
  }
}
