import { test, expect } from '@willowinc/playwright'

const componentName = 'FileUpload'
const groupName = 'Inputs'

test('default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('loading', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await page.locator('input').setInputFiles(`${__dirname}/sample.png`)
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('uploaded file', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await page.locator('input').setInputFiles(`${__dirname}/sample.png`)
  await expect(page.getByText('check_circle')).toBeVisible()
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('rejected file', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Playground')
  await page.locator('input').setInputFiles(`${__dirname}/sample.txt`)
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('failed upload', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'UploadErrors')
  await page.locator('input').setInputFiles(`${__dirname}/sample.png`)
  await expect(page.getByText('report')).toBeVisible()
  await expect(storybook.storyContainer).toHaveScreenshot()
})
