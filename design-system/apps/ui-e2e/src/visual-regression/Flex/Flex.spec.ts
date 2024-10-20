import { test, expect } from '@willowinc/playwright'

const componentName = 'Flex'
const groupName = 'Layout'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('gap', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Gap')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('column gap', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'ColumnGap')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('row gap', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'RowGap')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('align', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Align')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('justify', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Justify')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('direction', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Direction')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('wrap', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Wrap')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('polymorphic and different children width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'PolymorphicAndDifferentChildrenWidth'
  )
  await expect(storybook.storyContainer).toHaveScreenshot()
})
