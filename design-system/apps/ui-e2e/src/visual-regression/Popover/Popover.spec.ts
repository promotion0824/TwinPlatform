import { test, expect } from '@willowinc/playwright'

const componentName = 'Popover'

test('popover dropdown', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'RightStart')

  const rightStartDropdown = page.getByRole('dialog', { name: 'right-start' })

  // the arrow should not cover the content text
  await expect(rightStartDropdown).toHaveScreenshot(
    'right-start-dropdown.png',
    { threshold: 0 }
  )
})
