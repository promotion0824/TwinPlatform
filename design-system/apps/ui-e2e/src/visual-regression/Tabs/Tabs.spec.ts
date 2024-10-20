import { test, expect, Page } from '@willowinc/playwright'

const componentName = 'Tabs'
const groupName = 'Navigation'

const collapsibleTabsButton = (page: Page) =>
  page.locator('.collapsible-tabs-button')

test('variants', async ({ storybook, page }) => {
  const variants = [
    { variant: 'default', storyName: 'playground' },
    { variant: 'outline', storyName: 'outline' },
    { variant: 'pills', storyName: 'pills' },
  ]

  for (const variantStory of variants) {
    const { variant, storyName } = variantStory
    await storybook.goto(componentName, groupName, storyName)

    const selectedTab = page
      .getByRole('tab', {
        selected: true,
      })
      .first()

    await storybook.testInteractions(
      `${variant}-selected-tab`,
      selectedTab,
      selectedTab,
      ['default', 'hover']
    )

    const unselectedTab = page
      .getByRole('tab', {
        selected: false,
        disabled: false,
      })
      .first()

    await storybook.testInteractions(
      `${variant}-unselected-tab`,
      unselectedTab,
      unselectedTab,
      ['default', 'hover']
    )

    const disabledTab = page
      .getByRole('tab', {
        disabled: true,
      })
      .first()

    await storybook.testInteractions(
      `${variant}-disabled-tab`,
      disabledTab,
      disabledTab,
      ['default', 'hover']
    )
  }
})

test('outline list', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Outline')

  await expect(page.getByRole('tablist').first()).toHaveScreenshot()

  // select another tab
  await page
    .getByRole('tab', {
      selected: false,
      disabled: false,
    })
    .first()
    .click()

  await expect(page.getByRole('tablist').first()).toHaveScreenshot()
})

test('prefix - default', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'Icons')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('prefix - outline', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Outline')
  await expect(page.getByRole('tablist').nth(1)).toHaveScreenshot()
})

test('prefix - pills', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'Pills')
  await expect(page.getByRole('tablist').nth(1)).toHaveScreenshot()
})

test('suffix - default', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'DefaultSuffix')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('suffix - outline', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'OutlineSuffix')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('suffix - pills', async ({ storybook }) => {
  await storybook.gotoHidden(componentName, 'PillsSuffix')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

// These collapsible tab tests are done here as offsetWidth doesn't work in Jest,
// so they can't be done alongside our regular unit tests.

const variants = ['Default', 'Outline', 'Pills']

test.describe('collapsible tabs variants', async () => {
  for (const variant of variants) {
    const expectedCollapsedTabsCount = variant === 'Outline' ? 3 : 2

    test(`${variant} tabs should collapse if there is no room for them`, async ({
      page,
      storybook,
    }) => {
      await storybook.goto(
        componentName,
        groupName,
        `CollapsibleTabs${variant}`
      )

      await expect(collapsibleTabsButton(page)).toHaveText(
        `+${expectedCollapsedTabsCount}`
      )

      await expect(storybook.storyRoot).toHaveScreenshot(
        `${variant.toLowerCase()}-collapsible-tabs.png`
      )
    })

    const expectedCollapsedTabs =
      variant === 'Outline'
        ? ['Home', 'Music', 'Movies', 'Contacts']
        : ['Music', 'Movies', 'Contacts']

    test(`collapsed ${variant} tabs should be accessible in a dropdown menu`, async ({
      page,
      storybook,
    }) => {
      await storybook.goto(
        componentName,
        groupName,
        `CollapsibleTabs${variant}`
      )

      await page.locator('.collapsible-tabs-button').click()

      expect(page.getByText(expectedCollapsedTabs.join())).toBeTruthy()
    })

    test(`selected ${variant} tab should move out of the collapsed tab menu`, async ({
      page,
      storybook,
    }) => {
      await storybook.goto(
        componentName,
        groupName,
        `CollapsibleTabs${variant}`
      )

      await collapsibleTabsButton(page).click()

      const contactsTab = page.getByText('Contacts')
      await contactsTab.click()

      await expect(page.getByRole('menu')).toBeHidden()
      expect(page.getByText('Contacts')).not.toBeNull()
    })
  }
})

test('tabs should recalculate how many are collapsed if the tab content changes', async ({
  page,
  storybook,
}) => {
  await storybook.gotoHidden(componentName, `CollapsibleTabsAsync`)
  await expect(collapsibleTabsButton(page)).toHaveText('+4')
  await expect(storybook.storyRoot).toHaveScreenshot(
    'async-collapsible-tabs.png'
  )
})

test('tabs should recalculate how many are collapsed if the tabs are added/removed', async ({
  page,
  storybook,
}) => {
  await storybook.gotoHidden(componentName, `ChangingTabs`)
  await expect(collapsibleTabsButton(page)).toHaveText('+2')
})
