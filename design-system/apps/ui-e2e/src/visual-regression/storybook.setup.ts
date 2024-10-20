import { expect, test as setup } from '@playwright/test'

// Load Storybook so that the dark mode theme is set as the default for all following tests
setup('load Storybook', async ({ page }) => {
  await page.goto('/')
  await expect(
    page.getByRole('button', { name: 'Change theme to light mode' })
  ).toBeVisible()

  page
    .context()
    .storageState({ path: `${__dirname}/storybook-storage-state.json` })
})
