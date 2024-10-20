// We mostly don't transform files in node_modules since that takes time,
// but some modules need to be transformed because they contain import or
// export statements. Note the double negative here: this is for
// transform*Ignore*Patterns, but we use a negative lookahead `?!`, so the
// modules listed here are modules that we *are* transforming. See
// https://www.regular-expressions.info/lookaround.html
const nodeModulesToTransform = [
  'd3-scale',
  'd3-array',
  'd3-color',
  'd3-dispatch',
  'd3-drag',
  'd3-ease',
  'd3-format',
  'd3-interpolate',
  'd3-selection',
  'd3-time',
  'd3-transition',
  'd3-zoom',
  'internmap',
  'react-flow-renderer',
  // jest compile error occurs when running useEsriAuth.js
  '@arcgis',
  '@esri',
  '@stencil',
  'uuid',
  '@willowinc/palette',
  'react-merge-refs',
  'vis-network/standalone',
]

module.exports = {
  testTimeout: 10000, // there are tests that can take close to 5 seconds to finish running, so we extend default timeout duration
  setupFilesAfterEnv: ['./setupTests.js'],
  testEnvironment: 'jsdom',
  moduleNameMapper: {
    // Support css modules in files imported from tests
    '\\.(css|less|scss|sss|styl)$': '<rootDir>/node_modules/jest-css-modules',

    // Allows us to do `jest.mock("@willow/ui")`, `jest.mock("@willow/common")`.
    '^@willow/ui(.*)$': '<rootDir>/packages/ui/src$1',
    '^@willow/common(.*)$': '<rootDir>/packages/common/src$1',
    '^@willow/campus(.*)$': '<rootDir>/packages/campus/src$1',
    '^@willow/mobile-ui(.*)$': '<rootDir>/packages/mobile/ui/src$1',
  },
  transform: {
    // Use our Babel settings to transform our source files.
    '^.+\\.[jt]sx?$': 'babel-jest',
    '^.+\\.mjs$': 'babel-jest', // to support mjs file from react-merge-refs
    '^.+\\.svg$': '<rootDir>/jest-svg-transformer.js',
    '^.+\\.(png|jpg|gif)$': '<rootDir>/jest-image-transformer.js',
  },
  transformIgnorePatterns: [
    `node_modules/(?!${nodeModulesToTransform.join('|')})`,
  ],
  testPathIgnorePatterns: [
    '/node_modules/',
    '/modelDefinition/',
    '/cypress/',
    '/testUtils/',
  ],
}
