import { test, expect } from '@willowinc/playwright'

const componentName = 'Menu'
const groupName = 'Overlays'

test('menu items should have correct style', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'MenuItemStyles')

  const menuItems = page.getByRole('menuitem')

  await storybook.testInteractions(
    'secondary-menu-item',
    menuItems.first(),
    menuItems.first(),
    ['default', 'hover']
  )

  await storybook.testInteractions(
    'disabled-secondary-menu-item',
    menuItems.nth(1),
    menuItems.nth(1),
    ['default']
  )

  await storybook.testInteractions(
    'negative-menu-item',
    menuItems.nth(2),
    menuItems.nth(2),
    ['default', 'hover']
  )

  await storybook.testInteractions(
    'disabled-negative-menu-item',
    menuItems.nth(3),
    menuItems.nth(3),
    ['default']
  )
})

test('divider and label should have correct style', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'DividerAndLabel')

  await expect(page.getByRole('menu')).toHaveScreenshot()
})

test('submenus should have correct style', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'SubMenus')

  await expect(storybook.storyContainer).toHaveScreenshot()
})

// this test will failed in jest with waitFor in pipeline,
// so use playwright to run it instead.
test('click second SubMenu Target should only close its own SubMenu', async ({
  storybook,
  page,
}) => {
  await storybook.goto(componentName, groupName, 'SubMenus')
  await expect(page.getByRole('menu')).toHaveCount(3)

  const subMenuTriggers = page.getByRole('menuitem', {
    name: 'Submenu item label',
  })
  await subMenuTriggers.nth(1).click()

  // wait for dropdown close
  await page.waitForTimeout(300)

  await expect(page.getByRole('menu')).toHaveCount(2)
})

test.describe('radio in menu', () => {
  test.beforeEach(async ({ storybook }) => {
    await storybook.gotoHidden(componentName, 'Radios')
  })

  test('should have correct layout', async ({ page }) => {
    await expect(page.getByRole('menu')).toHaveScreenshot()
  })

  test('long label for radio should be wrapped', async ({ page }) => {
    const longRadio = page.getByRole('menuitem', {
      name: 'long',
    })
    await expect(longRadio).toHaveScreenshot()
  })

  test('all single radios should be able to be selected', async ({ page }) => {
    const longLabelRadio = page.getByRole('radio', {
      name: 'long',
    })
    const singleRadio = page.getByRole('radio', {
      name: 'single',
    })
    await longLabelRadio.click()
    await singleRadio.click()

    await expect(longLabelRadio).toBeChecked()
    await expect(singleRadio).toBeChecked()
  })

  test('only one radio can be selected in a group', async ({ page }) => {
    const costRadio = page.getByRole('radio', {
      name: 'cost',
    })

    await costRadio.click()

    await expect(costRadio).toBeChecked()

    const energyRadio = page.getByRole('radio', {
      name: 'energy',
    })

    await energyRadio.click()

    await expect(costRadio).not.toBeChecked()
    await expect(energyRadio).toBeChecked()
  })

  test('click blank area of radio can select', async ({ page }) => {
    const costRadioLabel = page.getByText('cost')

    await costRadioLabel.click({ position: { x: 80, y: 2 } })

    await expect(
      page.getByRole('radio', {
        name: 'cost',
      })
    ).toBeChecked()
  })
})

test.describe('checkbox in menu', () => {
  test.beforeEach(async ({ storybook }) => {
    await storybook.gotoHidden(componentName, 'Checkboxes')
  })

  test('should have correct layout', async ({ page }) => {
    await expect(page.getByRole('menu')).toHaveScreenshot()
  })

  test('all single checkboxes should be able to be selected', async ({
    page,
  }) => {
    const longLabelCheckbox = page.getByRole('checkbox', {
      name: 'long',
    })
    const singleCheckbox = page.getByRole('checkbox', {
      name: 'single',
    })
    await longLabelCheckbox.click()
    await singleCheckbox.click()

    await expect(longLabelCheckbox).toBeChecked()
    await expect(singleCheckbox).toBeChecked()
  })

  test('all checkboxes can be selected in a group', async ({ page }) => {
    const costCheckbox = page.getByRole('checkbox', {
      name: 'cost',
    })

    await costCheckbox.click()

    await expect(costCheckbox).toBeChecked()

    const energyCheckbox = page.getByRole('checkbox', {
      name: 'energy',
    })

    await energyCheckbox.click()

    await expect(costCheckbox).toBeChecked()
    await expect(energyCheckbox).toBeChecked()
  })

  test('click blank area of checkbox can select', async ({ page }) => {
    const costCheckboxLabel = page.getByText('cost')

    await costCheckboxLabel.click({ position: { x: 80, y: 2 } })

    await expect(
      page.getByRole('checkbox', {
        name: 'cost',
      })
    ).toBeChecked()
  })
})

test('free width menu should have max menu item width', async ({
  storybook,
  page,
}) => {
  await storybook.gotoHidden(componentName, 'FreeWidthMenu')
  await expect(page.getByRole('menu')).toHaveScreenshot()
})
