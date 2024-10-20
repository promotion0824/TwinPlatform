import { test, expect } from '@willowinc/playwright'

const componentName = 'Icon'
const groupName = 'Misc'

test('default inherited color', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'playground')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('customized color', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'CustomizedColor')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('icon in a table renders correctly', async ({ page }) => {
  await page.goto(
    '/iframe.html?viewMode=docs&id=design-system-iconography--docs'
  )

  const fastForwardIcon = page
    .getByRole('row', {
      name: 'fast_forward',
    })
    .locator('span')
  await expect(fastForwardIcon).toHaveScreenshot()
})
