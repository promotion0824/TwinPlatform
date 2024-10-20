export default {
  displayName: 'ui',
  preset: '../../jest.preset.js',
  transform: {
    '^.+\\.([tj]sx?|mjs)$': ['babel-jest', { presets: ['@nx/react/babel'] }],
    '^.+\\.svg$': '@nx/react/plugins/jest',
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  coverageDirectory: '../../coverage/libs/ui',
  transformIgnorePatterns: ['node_modules/(?!.*.mjs$)'], // for react-merge-refs
  setupFilesAfterEnv: ['@testing-library/jest-dom'],
}
