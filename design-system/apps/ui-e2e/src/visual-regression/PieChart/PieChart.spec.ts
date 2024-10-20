import { test, expect } from '@willowinc/playwright'

const componentName = 'PieChart'
const groupName = 'charts'

test('default Piechart', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'playground')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('Piechart with label', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'show label')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('Piechart with layout', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'layout')
  const stories = storybook.storyRoot.getByTestId('pie-chart')

  const layouts = ['horizontal', 'vertical']
  const positions = ['left', 'center', 'right']
  for (let i = 0; i < layouts.length; i++)
    for (let j = 0; j < positions.length; j++)
      await expect(stories.nth(i * positions.length + j)).toHaveScreenshot(
        `${layouts[i]}-${positions[j]}.png`
      )
})
