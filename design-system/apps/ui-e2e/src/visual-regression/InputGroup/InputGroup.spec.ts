import { test, expect } from '@willowinc/playwright'

const componentName = 'InputGroup'
const groupName = 'Inputs'

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test.describe('each child should have complete border when focus', () => {
  test.beforeEach(async ({ storybook }) => {
    await storybook.gotoHidden(componentName, 'MultipleChildren')
  })

  test('TextInput in InputGroup', async ({ storybook, page }) => {
    const textInput = page.getByRole('textbox').first()
    await storybook.testInteractions('TextInput', textInput, textInput, [
      'focus',
    ])
  })

  test('Select in InputGroup', async ({ storybook, page }) => {
    const select = page.getByRole('textbox').nth(1)
    await storybook.testInteractions('Select', select, select, [
      'click' /* click Combobox to focus the input */,
    ])
  })

  test('Button in InputGroup', async ({ storybook, page }) => {
    const button = page.getByRole('button', { name: 'Button' })
    await storybook.testInteractions('Button', button, button, ['focus'])
  })

  test('Menu in InputGroup', async ({ storybook, page }) => {
    const menuButton = page.getByRole('button', { name: 'toggle menu' })
    await storybook.testInteractions('Menu', menuButton, menuButton, ['focus'])
  })
})

test('children in InputGroup should have correct borders', async ({
  storybook,
}) => {
  await storybook.gotoHidden(componentName, 'MultipleChildren')

  await expect(storybook.storyContainer).toHaveScreenshot()
})
