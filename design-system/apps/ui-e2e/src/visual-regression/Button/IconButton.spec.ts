import { test, expect } from '@willowinc/playwright'

const componentName = 'Button'
const groupName = 'Buttons'

test('icon button kinds', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'iconOnly')

  const buttons = page.getByRole('button')
  const screenshots = [
    'primary.png',
    'secondary.png',
    'negative.png',
    'primary-transparent.png',
    'secondary-transparent.png',
    'negative-transparent.png',
    'primary-no-background.png',
    'secondary-no-background.png',
    'negative-no-background.png',
  ]

  for (let i = 0; i < screenshots.length; i++) {
    await expect(buttons.nth(i)).toHaveScreenshot(screenshots[i])
  }
})

test('icon button size large', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'large-icon-button')

  const buttons = page.getByRole('button', { name: 'info' })
  await expect(buttons.first()).toHaveScreenshot('large.png')
})
