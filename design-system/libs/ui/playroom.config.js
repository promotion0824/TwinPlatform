const path = require('path')

module.exports = {
  components: './src/index.ts',
  outputPath: '../../dist/playroom',
  frameComponent: './playroom/FrameComponent.tsx',
  baseUrl: '/playroom/',
  webpackConfig: () => ({
    module: {
      rules: [
        {
          test: /\.tsx?$/,
          use: [
            {
              loader: 'babel-loader',
              options: {
                presets: [
                  '@babel/preset-typescript',
                  ['@babel/preset-react', { runtime: 'automatic' }],
                ],
              },
            },
          ],
        },
        {
          test: /\.css$/i,
          exclude: /node_modules\/(?!(@mantine|material-symbols)\/).*/,
          use: ['style-loader', 'css-loader'],
        },
        {
          test: /\.svg$/,
          loader: 'svg-inline-loader',
        },
      ],
    },
    resolve: {
      extensions: ['.ts', '.tsx', '.js', '.jsx'],
      alias: {
        '@willowinc/palette': path.resolve(
          __dirname,
          '../palette/src/index.ts'
        ),
        '@willowinc/theme': path.resolve(__dirname, '../theme/src/index.ts'),
      },
    },
  }),
}
