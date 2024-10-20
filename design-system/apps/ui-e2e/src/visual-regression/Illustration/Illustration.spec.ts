import { test, expect } from '@willowinc/playwright'

const componentName = 'Illustration'

test('No Permissions', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'NoPermissions')

  await expect(
    page.getByRole('img', { name: 'no-permissions-dark' })
  ).toHaveScreenshot()
})

test('No Data', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'NoData')

  await expect(
    page.getByRole('img', { name: 'no-data-dark' })
  ).toHaveScreenshot()
})

test('No Results', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'NoResults')

  await expect(
    page.getByRole('img', { name: 'no-results-dark' })
  ).toHaveScreenshot()
})

test('Not Found', async ({ storybook, page }) => {
  await storybook.gotoHidden(componentName, 'NotFound')

  await expect(
    page.getByRole('img', { name: 'not-found-dark' })
  ).toHaveScreenshot()
})
