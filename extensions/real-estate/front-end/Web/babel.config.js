const path = require('path')

module.exports = {
  presets: [
    [
      '@babel/preset-react',
      {
        runtime: 'automatic',
      },
    ],
    '@babel/preset-typescript',
  ],
  plugins: [
    '@babel/plugin-proposal-class-properties',
    '@babel/plugin-proposal-export-default-from',
    '@babel/plugin-proposal-nullish-coalescing-operator',
    '@babel/plugin-proposal-optional-chaining',
    '@babel/plugin-syntax-dynamic-import',
    'macros',
    'styled-components',
  ],
  env: {
    test: {
      presets: ['@babel/preset-env', '@babel/preset-typescript'],
    },
  },
  overrides: [
    {
      test: ['./packages/platform'],
      plugins: [
        [
          'babel-plugin-module-resolver',
          {
            root: ['./packages/platform/src'],
            alias: {
              '@willow/ui': ['./packages/ui/src'],
              '@willow/campus': ['./packages/campus/src'],
              '@willow/common': ['./packages/common/src'],
            },
          },
        ],
      ],
    },
    {
      test: ['./packages/ui'],
      plugins: [
        [
          'babel-plugin-module-resolver',
          {
            root: ['./packages/ui/src'],
          },
        ],
      ],
    },
    {
      test: ['./packages/mobile/ui'],
      plugins: [
        [
          'babel-plugin-module-resolver',
          {
            root: ['./packages/mobile/ui/src'],
          },
        ],
      ],
    },
    {
      test: [
        // Use platform path separator to ensure consistent build in different platform.
        // i.e: Windows as /.\\packages\\mobile(?!\\ui), Unix as /.\/packages\/mobile(?!\/ui)
        new RegExp(
          `.\\${path.sep}packages\\${path.sep}mobile(?!\\${path.sep}ui)`
        ),
      ],
      plugins: [
        [
          'babel-plugin-module-resolver',
          {
            root: ['./packages/mobile/src'],
            alias: {
              '@willow/mobile-ui': ['./packages/mobile/ui/src'],
              '@willow/common': ['./packages/common/src'],
            },
          },
        ],
      ],
    },
  ],
}
