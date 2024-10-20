import { test, expect } from '@willowinc/playwright'

const componentName = 'Badge'
const groupName = 'Data Display'

test('sizes', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Size')

  const sizes = ['xs', 'sm', 'md', 'lg']
  const stories = storybook.storyRoot.getByTestId('badge')

  for (let i = 0; i < sizes.length; i++) {
    await expect(stories.nth(i)).toHaveScreenshot(`size-${sizes[i]}.png`)
  }
})

test('Variant', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Variant')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('Bold colors', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Color')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('Prefix and suffix', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Prefix and suffix')
  const sizes = ['xs', 'sm', 'md', 'lg']
  const stories = storybook.storyRoot.getByTestId('badge')

  for (let i = 0; i < sizes.length; i++) {
    await expect(stories.nth(i)).toHaveScreenshot(
      `size-${sizes[i]}-with-prefix-suffix.png`
    )
  }
})

test('Dot colors', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'DotColors')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('Muted colors', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'MutedColors')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('Subtle colors', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'SubtleColors')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('Outline colors', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'OutlineColors')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('a badge inside a flex container with a small width should keep a full size prefix (dot, for example)', async ({
  storybook,
  page,
}) => {
  await storybook.gotoHidden(componentName, 'SquishedDot')
  await expect(page.getByTestId('badge')).toHaveScreenshot('squished-dot.png')
})
