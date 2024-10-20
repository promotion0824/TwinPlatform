const webpack = require('webpack')
const path = require('path')
const { CleanWebpackPlugin } = require('clean-webpack-plugin')
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin')
const CopyPlugin = require('copy-webpack-plugin')
const MiniCssExtractPlugin = require('mini-css-extract-plugin')
const ReactRefreshPlugin = require('@pmmmwh/react-refresh-webpack-plugin')
const TerserPlugin = require('terser-webpack-plugin')
const TsconfigPathsPlugin = require('tsconfig-paths-webpack-plugin')
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin')

const isProduction = process.env.NODE_ENV === 'production'
const typeCheck = process.env.TYPE_CHECK === 'true'

module.exports = ({
  dirname,
  entry = { main: './src/index.js' },
  plugins = [],
  externals = {},
  devServer,
}) => ({
  mode: process.env.NODE_ENV,

  context: dirname,

  entry: { ...entry },

  output: {
    path: path.resolve(dirname, 'dist'),
    publicPath: '/',
    filename: isProduction
      ? 'public/[name].[chunkhash].js'
      : 'public/[name].js',
  },

  module: {
    rules: [
      ...(typeCheck
        ? [
            {
              test: /\.tsx?$/,
              loader: 'ts-loader',
              options: {
                transpileOnly: true,
              },
            },
          ]
        : []),
      {
        test: /\.[jt]sx?$/,
        loader: 'babel-loader',
        options: {
          compact: isProduction,
          plugins: isProduction ? [] : [require.resolve('react-refresh/babel')],
        },
        exclude: /node_modules/,
      },
      {
        test: /\.m?js/,
        resolve: {
          fullySpecified: false,
        },
      },
      {
        test: /\.css$/,
        use: isProduction
          ? [
              MiniCssExtractPlugin.loader,
              {
                loader: 'css-loader',
                options: {
                  modules: true,
                },
              },
              'postcss-loader',
            ]
          : [
              'style-loader',
              {
                loader: 'css-loader',
                options: {
                  modules: {
                    localIdentName: '[path][name]__[local]--[hash:base64:5]',
                  },
                },
              },
              'postcss-loader',
            ],
        exclude: /node_modules/,
      },
      {
        test: /\.css$/,
        use: [
          isProduction ? MiniCssExtractPlugin.loader : 'style-loader',
          'css-loader',
        ],
        include: /node_modules/,
      },
      {
        test: /\.(jpg|png|ttf)$/,
        loader: 'file-loader',
        options: {
          name: isProduction ? 'public/[hash].[ext]' : 'public/[name].[ext]',
        },
        exclude: /node_modules/,
      },
      {
        test: /\.svg$/,
        loader: 'svg-react-loader',
        exclude: /node_modules/,
      },
    ],
  },

  resolve: {
    extensions: ['.tsx', '.ts', '.js', '.jsx'],
    plugins: [new TsconfigPathsPlugin()],
  },

  plugins: [
    new webpack.DefinePlugin(
      Object.fromEntries(
        [
          'NODE_ENV',
          'MOCK_SERVICE_WORKER',
          'BUILD_BUILDNUMBER',
          'ARCGIS_BASE_URL',
        ].map((name) => [
          `process.env.${name}`,
          JSON.stringify(process.env[name]),
        ])
      )
    ),
    new CleanWebpackPlugin({ verbose: isProduction }),
    new CopyPlugin({ patterns: [{ from: './src/public', to: './public' }] }),
    ...(isProduction
      ? [
          new MiniCssExtractPlugin({
            filename: 'public/[name].[contenthash].css',
          }),
        ]
      : [new ReactRefreshPlugin(), new webpack.HotModuleReplacementPlugin()]),
    ...(typeCheck
      ? [
          new ForkTsCheckerWebpackPlugin({
            typescript: {
              configFile: path.join(process.cwd(), 'tsconfig.json'),
            },
          }),
        ]
      : []),
    ...plugins,
  ],

  externals: { ...externals },

  optimization: {
    minimizer: [
      new TerserPlugin({
        exclude: /3d-viewer/,
        terserOptions: {
          mangle: {
            /**
             * Autodesk forge viewer only accepts function name userFunction for property data access
             * Function name 'userFunction' should not be changed
             * Reference: https://forge.autodesk.com/en/docs/viewer/v7/developers_guide/advanced_options/propdb-queries/
             */
            reserved: ['userFunction'],
          },
        },
      }),
      new CssMinimizerPlugin(),
    ],
    runtimeChunk: 'single',
    moduleIds: 'deterministic',
    splitChunks: {
      chunks: 'all',
      minSize: 0,
      cacheGroups: {
        vendor: {
          test: /node_modules/,
          name: 'vendor',
          chunks: 'all',
        },
        ui: {
          test: /(\/|\\)(packages|mobile)(\/|\\)ui(\/|\\)/,
          name: 'ui',
        },
      },
    },
  },

  devtool: isProduction ? undefined : 'eval-cheap-module-source-map',

  devServer: {
    open: true,
    historyApiFallback: {
      // Some twins have dots in their IDs - this lets us refresh the dev server
      // on a path with such an ID in it.
      disableDotRule: true,
    },
    hot: true,
    devMiddleware: {
      stats: 'errors-only',
    },
    client: {
      logging: 'warn',
    },
    ...devServer,
  },
})
