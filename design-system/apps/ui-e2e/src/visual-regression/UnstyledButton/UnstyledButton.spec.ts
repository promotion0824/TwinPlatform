import { test } from '@willowinc/playwright'

const componentName = 'UnstyledButton'
const groupName = 'Buttons'

test('focus', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')

  await storybook.testInteractions(
    'unstyled-button',
    page.getByText('Unstyled button'),
    // Targeting something smaller wouldn't show any part of an incorrect focus state
    storybook.storyRoot,
    ['focus']
  )
})
