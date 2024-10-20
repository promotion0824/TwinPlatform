import { test } from '@willowinc/playwright'

const componentName = 'Button'
const groupName = 'Buttons'

test('link button hover', async ({ page, storybook }) => {
  await storybook.goto(componentName, groupName, 'button-as-link')

  const linkButton = page.getByRole('link')
  await storybook.testInteractions('linkButton', linkButton, linkButton, [
    'default',
    'hover',
    'focus',
    'active',
  ])
})
