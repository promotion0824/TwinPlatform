import { test, expect } from '@willowinc/playwright'

const componentName = 'PillGroup'
const groupName = 'data-display'

test('Customized gap', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'CustomizedGap')

  await page.setViewportSize({ width: 180, height: 200 })

  await expect(page.getByText('Label 1Label 2Label 3')).toHaveScreenshot()
})
