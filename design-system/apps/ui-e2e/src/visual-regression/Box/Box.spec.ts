import { test, expect } from '@willowinc/playwright'
import { baseTheme } from '@willowinc/theme'

const componentName = 'Box'
const groupName = 'Misc'

test('HiddenFrom', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Hidden From')
  const breakpoints = Object.entries(baseTheme.breakpoints)
  for (const [breakpointName, breakpointValue] of breakpoints) {
    const locator = page.getByText(new RegExp(breakpointName, 'i'))

    await page.setViewportSize({
      width: parseInt(breakpointValue) - 100,
      height: 800,
    })
    await expect(locator).toBeVisible()
    await page.setViewportSize({
      width: parseInt(breakpointValue) + 100,
      height: 800,
    })
    await expect(locator).toBeHidden()
  }
})
