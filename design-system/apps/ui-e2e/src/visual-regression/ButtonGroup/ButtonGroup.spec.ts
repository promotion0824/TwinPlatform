import { expect, test } from '@willowinc/playwright'

const componentName = 'ButtonGroup'
const groupName = 'Buttons'

test('button group', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
