import { defineConfig, devices } from '@playwright/test'
import { nxE2EPreset } from '@nx/playwright/preset'
import { workspaceRoot } from '@nx/devkit'

// For CI, you may want to set BASE_URL to the deployed application.
const [baseURL, webServerCommand] = process.env['CI']
  ? ['http://localhost:4444/', 'npx nx run ui:storybook:ci']
  : ['http://localhost:4400', 'npm start']

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  ...nxE2EPreset(__filename, { testDir: './src/visual-regression' }),
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  /* Run your local dev server before starting the tests */
  webServer: {
    command: webServerCommand,
    url: baseURL,
    reuseExistingServer: !process.env['CI'],
    cwd: workspaceRoot,
  },
  // see https://playwright.dev/docs/api/class-testconfig#test-config-snapshot-path-template
  // for more information
  snapshotPathTemplate:
    '{testDir}/{testFileDir}/{testFileName}-snapshots/{arg}-{projectName}{ext}',
  maxFailures: 0,
  timeout: 30 * 1000,
  expect: {
    timeout: 5000,
  },
  use: {
    actionTimeout: 0,
    baseURL,
    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'retain-on-failure',
  },
  outputDir: '../../dist/apps/ui-e2e/test-results',
  reporter: process.env['CI']
    ? [
        // use blob for ci shards so that we could merge reports into one for artifacts
        ['blob', { outputDir: '../../dist/apps/ui-e2e/all-blob-reports' }],
      ]
    : [
        [
          'html',
          {
            outputFolder: '../../dist/apps/ui-e2e/playwright-report',
            open: 'never', // disable report opening after test
          },
        ],
        [
          'json',
          {
            outputFile:
              '../../dist/apps/ui-e2e/playwright-report/test-results.json',
          },
        ],
      ],
  // Run all tests in parallel.
  fullyParallel: true,
  // Fail the build if you accidentally left test.only in the source code.
  forbidOnly: true,
  retries: 4,
  projects: [
    // {
    //   name: 'chromium',
    //   use: { ...devices['Desktop Chrome'] },
    // },
    // {
    //   name: 'firefox',
    //   use: { ...devices['Desktop Firefox'] },
    // },
    // {
    //   name: 'webkit',
    //   use: { ...devices['Desktop Safari'] },
    // },
    // mobile browsers support
    // {
    // name: 'Mobile Chrome',
    // use: { ...devices['Pixel 5'] },
    // },
    // {
    // name: 'Mobile Safari',
    // use: { ...devices['iPhone 12'] },
    // },
    // branded browsers
    // {
    // name: 'Microsoft Edge',
    // use: { ...devices['Desktop Edge'], channel: 'msedge' },
    // },
    {
      name: 'Setup Storybook',
      testMatch: /storybook\.setup\.ts/,
    },
    {
      name: 'Google Chrome',
      dependencies: ['Setup Storybook'],
      use: {
        ...devices['Desktop Chrome'],
        channel: 'chrome',
        storageState: `${__dirname}/src/visual-regression/storybook-storage-state.json`,
      },
    },
  ],
})
