import { test, expect } from '@willowinc/playwright'

const componentName = 'Textarea'
const groupName = 'Inputs'

test('default placeholder', async ({ storybook, page }) => {
  await page.setViewportSize({ width: 400, height: 400 })
  await storybook.goto(componentName, groupName, 'playground')

  const textarea = page.locator('#storybook-root')
  await expect(textarea).toHaveScreenshot()
})

test('interaction styles with default value', async ({ storybook, page }) => {
  await page.setViewportSize({ width: 400, height: 400 })

  await storybook.goto(componentName, groupName, 'WithDefaultValue')
  const snapshotPrefix = 'default value'
  const locator = page.getByLabel('Label')

  await storybook.testInteractions(snapshotPrefix, locator, undefined, [
    'default',
    'focus',
  ])

  // The background color will change slightly when the element is hovered,
  // but it's too subtle to be detected by the default threshold, so
  // need to adjust the threshold here.
  await locator.hover()
  await expect(locator).toHaveScreenshot(`${snapshotPrefix}-hover.png`, {
    threshold: 0,
  })
})

test('required', async ({ storybook, page }) => {
  await page.setViewportSize({ width: 400, height: 400 })

  await storybook.goto(componentName, groupName, 'Required')

  const Textarea = page.locator('#storybook-root')
  await expect(Textarea).toHaveScreenshot()
})

test('readonly can not be edited', async ({ storybook, page }) => {
  await page.setViewportSize({ width: 400, height: 400 })

  await storybook.goto(componentName, groupName, 'Readonly')

  const Textarea = page.locator('#storybook-root')
  await Textarea.getByLabel('Label').type('new value')

  await expect(Textarea).toHaveScreenshot('readonly.png')
})

test('invalid', async ({ storybook, page }) => {
  await page.setViewportSize({ width: 400, height: 400 })

  await storybook.goto(componentName, groupName, 'Invalid')

  const Textarea = page.locator('#storybook-root')
  await expect(Textarea).toHaveScreenshot('invalid with placeholder.png')

  await Textarea.getByLabel('Label').type('new value')
  await expect(Textarea).toHaveScreenshot('invalid with value.png')
})

test('disabled cannot be edited', async ({ storybook, page }) => {
  await page.setViewportSize({ width: 400, height: 400 })

  await storybook.goto(componentName, groupName, 'Disabled')

  const Textarea = page.locator('#storybook-root')
  await Textarea.getByLabel('Label').type('new value')

  await expect(Textarea).toHaveScreenshot('disabled.png')
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})
