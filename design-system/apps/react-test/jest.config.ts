/* eslint-disable */
export default {
  displayName: 'react-test',
  preset: '../../jest.preset.js',
  transform: {
    // order matters
    '^.+\\.([tj]sx?|mjs)$': ['babel-jest', { presets: ['@nx/react/babel'] }],
    '^(?!.*\\.(js|jsx|ts|tsx|css|json)$)': '@nx/react/plugins/jest',
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory: '../../coverage/apps/react-test',
  transformIgnorePatterns: ['node_modules/(?!.*.mjs$)'], // for react-merge-refs
  setupFiles: ['jest-canvas-mock'],
}
