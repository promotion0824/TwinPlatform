import type { StorybookConfig } from '@storybook/react-webpack5'
import remarkGfm from 'remark-gfm'

const config: StorybookConfig = {
  framework: {
    name: '@storybook/react-webpack5',
    options: {},
  },
  docs: {
    autodocs: false,
  },
  addons: [
    '@storybook/addon-essentials',
    '@storybook/addon-themes',
    '@nx/react/plugins/storybook',
    '@storybook/addon-a11y',
    'storybook-dark-mode',
    {
      name: '@storybook/addon-docs',
      options: {
        mdxPluginOptions: {
          mdxCompileOptions: {
            remarkPlugins: [remarkGfm],
          },
        },
      },
    },
  ],
  stories: [],
  staticDirs: ['../src/docs/_assets'],
  // managerHead is accidentally broken and not included in the type definition,
  // https://github.com/storybookjs/storybook/issues/22597#issuecomment-1559288939
  // Could use it after it's fixed and upgraded, so that we could also share the
  // the config for manager-head.html
  previewHead: (head) => `
  ${head}
  <!-- install Poppins for each preview iframe -->
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
  <link
    href="https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap"
    rel="stylesheet"
  />
  <!-- prevent all search engines that support the noindex rule from indexing a page on your site -->
  <meta name="robots" content="noindex" />
  `,

  webpackFinal: async (config) => {
    config.node = {
      __filename: true,
    }

    // Add an image loader
    config.module?.rules?.push({
      test: /\.(png|jpe?g|gif|webp)$/,
      loader: require.resolve('url-loader'),
      options: {
        limit: 10000, // 10kB
        name: '[name].[hash:7].[ext]',
      },
    })

    return config
  },
}

export const rootMain = {
  ...config,

  // Disable the cache with a noop while the storybook cache is broken
  // See https://github.com/storybookjs/storybook/issues/13795#issuecomment-1192075981
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  managerWebpack: (config: any, options: any) => {
    if (options.cache) {
      options.cache.set = () => Promise.resolve()
    }

    return config
  },
}
