import { expect, test } from '@willowinc/playwright'
import { clickTreeNode } from '../utils/interactiveWithTreeNode'

const componentName = 'DrilldownTree'
const groupName = 'Inputs'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('hover', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await page.getByText('Building Component').hover()
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('selected node', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await clickTreeNode(page, 'Building Component')
  await clickTreeNode(page, 'Architectural Building Component')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('searchable', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Searchable')
  await page.getByTestId('tree-search-input').fill('Ceiling')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

// This test wouldn't work in Jest 😢
test('should show all previous nodes once a search is cleared', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'Searchable')

  await clickTreeNode(page, 'Building Component')
  await clickTreeNode(page, 'Architectural Building Component')
  await clickTreeNode(page, 'Floor')
  expect(await page.getByRole('treeitem').all()).toHaveLength(4)

  await page.getByTestId('tree-search-input').fill('Space')
  expect(await page.getByRole('treeitem').all()).toHaveLength(1)

  await page.getByTestId('tree-search-input').fill('')
  expect(await page.getByRole('treeitem').all()).toHaveLength(4)
})
