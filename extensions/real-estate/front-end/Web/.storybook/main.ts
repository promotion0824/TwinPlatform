import TsconfigPathsPlugin from 'tsconfig-paths-webpack-plugin'
import type { StorybookConfig } from '@storybook/react-webpack5'

const config: StorybookConfig = {
  framework: '@storybook/react-webpack5',
  stories: [
    '../packages/**/*.mdx',
    '../packages/**/*.stories.@(js|jsx|ts|tsx)',
  ],
  addons: ['@storybook/addon-links', '@storybook/addon-essentials'],
  docs: {
    autodocs: true,
  },
  staticDirs: [
    {
      from: './root',
      to: '/',
    },
    {
      from: '../packages/platform/src/public',
      to: '/public',
    },
  ],

  webpackFinal: async (config) => {
    /*
     * Because we are using a custom beta svg loader and customized webpack configuration
     * our project is incompatible with default storybook settings.
     *
     * The below code inserts custom loaders into the storybook build process.
     *
     * It should be removed if we remove our custom webpack config.
     */

    if (config.module?.rules) {
      config.module.rules = [
        ...config.module.rules.map((rule) => {
          // For some reason these rules can be of type RuleSetRule or '...' ???
          if (rule !== '...') {
            if (rule.test) {
              if (/svg/.test(rule.test.toString())) {
                // Stop the default Storybook loaders for SVG files
                return { ...rule, exclude: /\.svg$/i }
              }
              if (/css/.test(rule.test.toString())) {
                // Stop the default Storybook loaders for CSS files
                return { ...rule, exclude: /\.css$/i }
              }
            }
          }
          return rule
        }),
        // Use our custom SVG loader
        {
          test: /\.svg$/i,
          use: ['svg-react-loader'],
        },
        {
          test: /\.css$/i,
          use: [
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
          test: /\.css$/i,
          use: ['style-loader', 'css-loader'],
          include: /node_modules/,
        },
      ]
    }

    if (config.resolve) {
      config.resolve.plugins = [
        ...(config.resolve.plugins || []),
        new TsconfigPathsPlugin({
          extensions: config.resolve.extensions,
        }),
      ]
    }

    config.node = {
      __filename: true,
    }

    return config
  },
}

export default config
