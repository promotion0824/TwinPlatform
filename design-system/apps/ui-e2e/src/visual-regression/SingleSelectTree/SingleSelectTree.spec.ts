import { expect, test } from '@willowinc/playwright'
import {
  clickTreeNode,
  closeBuildingComponent,
  openArchitecturalBuildingComponent,
  openBuildingComponent,
} from '../utils/interactiveWithTreeNode'

const componentName = 'SingleSelectTree'
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

test('expanded children', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await openBuildingComponent(page)
  await openArchitecturalBuildingComponent(page)
  await page.locator('body').hover()
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('selected node', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await clickTreeNode(page, 'Asset')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('selected child indicator', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await openBuildingComponent(page)
  await clickTreeNode(page, 'Structural Building Component')
  await closeBuildingComponent(page)
  await page.locator('body').hover()
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('horizontal layout with label width', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'HorizontalLayoutWithLabelWidth'
  )

  await expect(storybook.storyRoot).toHaveScreenshot()
})
