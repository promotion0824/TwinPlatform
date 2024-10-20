import { test, expect } from '@willowinc/playwright'

const componentName = 'Button'
const groupName = 'Buttons'

const buttonVariants = [
  {
    storyName: 'kind',
    buttons: [
      { name: 'primary', id: 'primary' },
      { name: 'secondary', id: 'secondary' },
      { name: 'negative', id: 'negative' },
    ],
  },
  {
    storyName: 'transparent',
    buttons: [
      { name: 'primary transparent', id: 'primary-transparent' },
      { name: 'secondary transparent', id: 'secondary-transparent' },
      { name: 'negative transparent', id: 'negative-transparent' },
    ],
  },
  {
    storyName: 'noBackground',
    buttons: [
      { name: 'primary', id: 'primary-noBackground' },
      { name: 'secondary', id: 'secondary-noBackground' },
      { name: 'negative', id: 'negative-noBackground' },
    ],
  },
]

buttonVariants.forEach(({ storyName, buttons }) => {
  buttons.forEach(({ name, id }) => {
    // eslint-disable-next-line playwright/expect-expect
    test(`${name} ${storyName} with interactions`, async ({
      storybook,
      page,
    }) => {
      await storybook.goto(componentName, groupName, storyName)

      const button = page.getByRole('button', { name })
      await storybook.testInteractions(id, button, button, [
        'default',
        'hover',
        'focus',
        'active',
      ])
    })
  })
})

test('button size', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'size')

  const medium = page.getByRole('button', { name: 'medium' })
  await expect(medium).toHaveScreenshot('medium.png')

  const large = page.getByRole('button', { name: 'large' })
  await expect(large).toHaveScreenshot('large.png')
})

test('button prefix and suffix', async ({ page, storybook }) => {
  await storybook.goto(componentName, groupName, 'prefix-and-suffix')

  const prefix = page.getByRole('button', { name: 'info Prefix', exact: true })
  await expect(prefix).toHaveScreenshot('prefix.png')

  const suffix = page.getByRole('button', { name: 'Suffix info', exact: true })
  await expect(suffix).toHaveScreenshot('suffix.png')

  const both = page.getByRole('button', { name: 'prefix and suffix' })
  await expect(both).toHaveScreenshot('both-prefix-suffix.png')
})

const disabledButtonConfigs = [
  { name: 'primary disabled', screenshot: 'disabled-primary.png' },
  { name: 'secondary disabled', screenshot: 'disabled-secondary.png' },
  { name: 'negative disabled', screenshot: 'disabled-negative.png' },
  {
    name: 'primary transparent',
    screenshot: 'disabled-primary-transparent.png',
  },
  {
    name: 'secondary transparent',
    screenshot: 'disabled-secondary-transparent.png',
  },
  {
    name: 'negative transparent',
    screenshot: 'disabled-negative-transparent.png',
  },
  {
    name: 'primary no background',
    screenshot: 'disabled-primary-no-background.png',
  },
  {
    name: 'secondary no background',
    screenshot: 'disabled-secondary-no-background.png',
  },
  {
    name: 'negative no background',
    screenshot: 'disabled-negative-no-background.png',
  },
]
test('disabled', async ({ page, storybook }) => {
  await storybook.goto(componentName, groupName, 'disabled')

  for (const { name, screenshot } of disabledButtonConfigs) {
    const button = page.getByRole('button', { name })
    await expect(button).toHaveScreenshot(screenshot)
  }
})

test('button loading', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'loading')

  const buttons = page.getByRole('button')
  const screenshots = [
    'loading-primary.png',
    'loading-secondary.png',
    'loading-negative.png',
    'loading-primary-transparent.png',
    'loading-secondary-transparent.png',
    'loading-negative-transparent.png',
    'loading-primary-no-background.png',
    'loading-secondary-no-background.png',
    'loading-negative-no-background.png',
  ]

  for (let i = 0; i < screenshots.length; i++) {
    await expect(buttons.nth(i)).toHaveScreenshot(screenshots[i])
  }
})

test('loading (large)', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'LoadingLarge')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('loading (prefix)', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'LoadingPrefix')
  await expect(storybook.storyContainer).toHaveScreenshot()
})

test('loading (suffix)', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'LoadingSuffix')
  await expect(storybook.storyContainer).toHaveScreenshot()
})
