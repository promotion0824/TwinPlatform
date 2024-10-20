import { test, expect } from '@willowinc/playwright'
import { kebabCase } from 'lodash'

const componentName = 'Avatar'
const groupName = 'Data Display'

test('content and sizes', async ({ storybook, page }) => {
  const contentStories = [
    'SizesWithCirclePlaceholder',
    'SizesWithRectanglePlaceholder',
    'SizesWithCircleImage',
    'SizesWithCircleInitials',
    'SizesWithCircleIcon',
  ]
  for (const storyName of contentStories) {
    await storybook.gotoHidden(componentName, storyName)

    const avatars = page.getByTestId('avatar')

    await expect(avatars.first()).toHaveScreenshot(
      `${kebabCase(storyName)}-sm.png`
    )
    await expect(avatars.nth(1)).toHaveScreenshot(
      `${kebabCase(storyName)}-md.png`
    )
    await expect(avatars.nth(2)).toHaveScreenshot(
      `${kebabCase(storyName)}-lg.png`
    )
  }
})

test('variants', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'VariantsWithCircle')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('colors', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Color')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('with tooltip', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'WithTooltip')
  const avatar = page.getByTestId('avatar')
  await avatar.hover()

  await expect(page.getByText('Tooltip Content')).toBeVisible()
})
