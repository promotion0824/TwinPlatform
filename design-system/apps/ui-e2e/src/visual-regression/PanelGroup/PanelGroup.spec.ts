/* eslint-disable playwright/no-wait-for-timeout */
import { test, expect, Page } from '@willowinc/playwright'

const componentName = 'PanelGroup'
const groupName = 'Layout'

/**
 * The handle was rendered as a default one initially, wait for it rendered
 * as our customized one then take screenshot.
 */
const waitForDraggableHandle = (page: Page) =>
  expect(page.getByText('drag_handle').first()).toBeVisible()
test('horizontal fixed panels', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'horizontal fixed panels')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('horizontal resizable panels', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'horizontal resizable panels')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('horizontal resizable and collapsible panels', async ({ storybook }) => {
  await storybook.goto(
    componentName,
    groupName,
    'horizontal resizable and collapsible panels'
  )
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('vertical fixed panels', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'vertical fixed panels')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('vertical resizable panels', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'vertical resizable panels')

  await waitForDraggableHandle(page)
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('vertical resizable and collapsible panels', async ({
  storybook,
  page,
}) => {
  await storybook.goto(
    componentName,
    groupName,
    'vertical resizable and collapsible panels'
  )
  // test marked as flaky, will wait for the content load before screenshot
  await expect(page.getByText('Panel 1')).toBeInViewport()

  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('complex panels', async ({ storybook, page }) => {
  await storybook.goto(componentName, groupName, 'complex panels')

  await waitForDraggableHandle(page)
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('horizontal panels with collapse/expand interactions', async ({
  storybook,
  page,
}) => {
  await storybook.gotoHidden(componentName, 'horizontal panels')

  await collapsePanel(page, 'panel 1')
  await collapsePanel(page, 'panel 2')
  await collapsePanel(page, 'panel 3')

  await expect(storybook.storyRoot).toHaveScreenshot(
    'horizontal-panels-with-collapse-expand-interactions-all-collapsed.png'
  )

  await expandPanel(page, 'panel-1')
  await expandPanel(page, 'panel-2')
  await expandPanel(page, 'panel-3')

  await expect(storybook.storyRoot).toHaveScreenshot(
    'horizontal-panels-with-collapse-expand-interactions-all-expanded.png'
  )
})

test('vertical panels with collapse/expand interactions', async ({
  storybook,
  page,
}) => {
  await storybook.gotoHidden(componentName, 'vertical panels')

  await collapsePanel(page, 'panel 1')
  await collapsePanel(page, 'panel 2')
  await collapsePanel(page, 'panel 3')

  await expect(storybook.storyRoot).toHaveScreenshot(
    'vertical-panels-with-collapse-expand-interactions-all-collapsed.png'
  )

  await expandPanel(page, 'panel-1')
  await expandPanel(page, 'panel-2')
  await expandPanel(page, 'panel-3')

  await expect(storybook.storyRoot).toHaveScreenshot(
    'vertical-panels-with-collapse-expand-interactions-all-expanded.png'
  )
})

test('complex panels with collapse/expand interactions', async ({
  storybook,
  page,
}) => {
  await storybook.gotoHidden(componentName, 'CollapsibleComplexPanels')

  await collapsePanel(page, 'panel 1')
  await collapsePanel(page, 'panel 2')
  await collapsePanel(page, 'panel 3')
  await collapsePanel(page, 'panel 4')
  await collapsePanel(page, 'panel 5')

  await expect(storybook.storyRoot).toHaveScreenshot(
    'complex-panels-with-collapse-expand-interactions-all-collapsed.png'
  )

  await expandPanel(page, 'panel-1')
  await expandPanel(page, 'panel-2')
  await expandPanel(page, 'panel-3')
  await expandPanel(page, 'panel-4')
  await expandPanel(page, 'panel-5')

  await expect(storybook.storyRoot).toHaveScreenshot(
    'complex-panels-with-collapse-expand-interactions-all-expanded.png'
  )
})

test('header variants', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'header variants')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('tabs variant', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'tabs variant')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('header controls', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'header controls')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('footer', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'footer')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('hide header border', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'hide header border')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test('small gap size', async ({ storybook }) => {
  await storybook.goto(componentName, groupName, 'small gap size')
  await expect(storybook.storyRoot).toHaveScreenshot()
})

test.describe('layout status after reload', () => {
  test.beforeEach(async ({ storybook }) => {
    await storybook.gotoHidden(componentName, 'PersistedNestedPanels')
  })
  test('should be persistent with first vertical panel collapsed', async ({
    storybook,
    page,
    context,
  }) => {
    await collapsePanel(page, 'panel 1')

    await page.reload()

    await waitForDraggableHandle(page)
    expect(
      (await context.storageState()).origins[0].localStorage.length
    ).not.toEqual(0)
    await expect(
      page.getByTestId('panel-1').getByTestId('expand-panel')
    ).toBeVisible()
    await expect(storybook.storyContainer).toHaveScreenshot()
  })
  test('should be persistent with first horizontal panel collapsed', async ({
    storybook,
    page,
    context,
  }) => {
    await collapsePanel(page, 'panel 2')
    await page.reload()

    await waitForDraggableHandle(page)
    expect(
      (await context.storageState()).origins[0].localStorage.length
    ).not.toEqual(0)
    await expect(page.getByText('Panel 2')).toBeHidden()
    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('should be persistent with a panel collapse and a resizing ', async ({
    storybook,
    page,
  }) => {
    await collapsePanel(page, 'panel 2')

    const panel1 = await getPanel(page, 'panel 1')
    const handle1 = await resizeHandle(page, 0, 0)
    // drag handle1 up
    await handle1.dragTo(panel1, {
      sourcePosition: { x: 100, y: 2 },
      targetPosition: { x: 10, y: 120 },
    })

    const panel4 = await getPanel(page, 'panel 4')
    const handle3 = await resizeHandle(page, 2, 0)
    // drag handle3 down
    await handle3.dragTo(panel4, {
      sourcePosition: { x: 100, y: 2 },
      targetPosition: { x: 10, y: 30 },
    })

    // wait for the 100ms resizing debounce to take action in react-resizable-panels
    await page.waitForTimeout(105)
    await page.reload()

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('should be retrieved correctly when the collapsed panel number changed', async ({
    storybook,
    page,
  }) => {
    // resize panel1 which is a different PanelGroup
    const panel1 = await getPanel(page, 'panel 1')
    const handle1 = await resizeHandle(page, 0, 0)
    // drag handle1 up
    await handle1.dragTo(panel1, {
      sourcePosition: { x: 100, y: 2 },
      targetPosition: { x: 10, y: 120 },
    })

    // resize panel4 when all panels are expanded
    const panel5 = await getPanel(page, 'panel 5')
    const handle4 = await resizeHandle(page, 2, 1)
    // drag handle4 down
    await handle4.dragTo(panel5, {
      sourcePosition: { x: 100, y: 2 },
      targetPosition: { x: 10, y: 40 },
    })
    // important: wait for the 100ms resizing debounce to take action in react-resizable-panels before we collapse next panel
    await page.waitForTimeout(105)

    // resize when only have panel3 and panel5
    await collapsePanel(page, 'panel 4')
    const panel4 = await getPanel(page, 'panel 4')
    const handle3 = await resizeHandle(page, 2, 0)
    // drag handle3 down
    await handle3.dragTo(panel4, {
      sourcePosition: { x: 100, y: 2 },
      targetPosition: { x: 10, y: 40 },
    })
    // important: wait for the 100ms resizing debounce to take action in react-resizable-panels before we collapse next panel
    await page.waitForTimeout(105)

    // resize when only have panel3 and panel4
    await expandPanel(page, 'panel-4')
    await collapsePanel(page, 'panel 5')
    const panel3 = await getPanel(page, 'panel 3')
    // drag handle3 up
    await handle3.dragTo(panel3, {
      sourcePosition: { x: 100, y: 2 },
      targetPosition: { x: 10, y: 50 },
    })

    // wait for the 100ms resizing debounce to take action in react-resizable-panels
    await page.waitForTimeout(105)
    await page.reload()

    // when all panels expanded
    await expandPanel(page, 'panel-5')
    await expect(storybook.storyContainer).toHaveScreenshot()

    // when only have panel3 and panel5
    await collapsePanel(page, 'panel 4')
    await expect(storybook.storyContainer).toHaveScreenshot()

    // when only have panel3 and panel4
    await expandPanel(page, 'panel-4')
    await collapsePanel(page, 'panel 5')
    await expect(storybook.storyContainer).toHaveScreenshot()
  })
})

test.describe('layout when all panels initially collapsed', () => {
  test.beforeEach(async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'PersistedNestedPanels')

    // start with all panels collapsed
    const panelTitles = ['panel 1', 'panel 2', 'panel 3', 'panel 4', 'panel 5']

    for (let i = 0; i < panelTitles.length; i++) {
      await collapsePanel(page, panelTitles[i])
    }
  })

  test('should have all panels collapsed after reload', async ({
    storybook,
    page,
  }) => {
    await page.reload()

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('should have persistent layout after expand one panel', async ({
    storybook,
    page,
  }) => {
    await expandPanel(page, 'panel-4')

    await page.reload()

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test('should have persistent layout after expand last 2 panels in the same group with resizing', async ({
    page,
    storybook,
  }) => {
    await expandPanel(page, 'panel-4')
    await expandPanel(page, 'panel-5')

    const panel4 = await getPanel(page, 'panel 4')
    // the handle for panel4, which is in PanelGroup3 (index 2),
    // and the handle index 0 because it's the only visible handle in the group
    const handle4 = await resizeHandle(page, 2, 0)
    // drag handle4 towards panel4
    await handle4.dragTo(panel4, {
      sourcePosition: { x: 100, y: 2 },
      targetPosition: { x: 30, y: 70 },
    })

    // wait for the 100ms resizing debounce to take action in react-resizable-panels
    await page.waitForTimeout(105)
    await page.reload()

    await expect(storybook.storyContainer).toHaveScreenshot()
  })

  test("should collapse tabs inside panels when there isn't room to show them", async ({
    page,
    storybook,
  }) => {
    await storybook.gotoHidden(componentName, 'CollapsibleTabsInPanels')
    await expect(page.locator('.collapsible-tabs-button')).toHaveText('+3')
  })

  test('should collapse tabs inside panels correctly when controlled tabs are used', async ({
    page,
    storybook,
  }) => {
    await storybook.gotoHidden(
      componentName,
      'ControlledCollapsibleTabsInPanels'
    )

    await page.getByRole('tab', { name: 'Messages' }).click()
    await page.getByRole('tab', { name: 'Settings' }).click()

    const handle = await resizeHandle(page, 0, 0)
    await handle.dragTo(page.locator('[data-panel-group]'), {
      targetPosition: { x: 300, y: 200 },
    })

    await page.waitForTimeout(105)
    await expect(page.locator('.collapsible-tabs-button')).toHaveText('+5')
  })
})

test.describe('Spacing style props', () => {
  test('should be applied for fixed Panel', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'FixedPanel')

    const panel = await getPanel(page, 'Panel content')

    await expect(panel).toHaveCSS('padding', '20px')
    await expect(panel).toHaveCSS('margin', '12px')
    await expect(panel).toHaveCSS('height', '300px')
  })

  test('should be applied for collapsed Panel', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'CollapsiblePanel')

    const panel = await getPanel(page, 'Panel content')

    await expect(panel).toHaveCSS('padding', '20px')
    await expect(panel).toHaveCSS('margin', '12px')
    await expect(panel).toHaveCSS('height', '300px')
  })

  test('should be applied for PanelContent', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'WithPanelContent')

    const panel = page.getByText('Panel content')

    await expect(panel).toHaveCSS('padding', '20px')
    await expect(panel).toHaveCSS('margin', '12px')
    await expect(panel).toHaveCSS('height', '300px')
  })
})

const panelHeader = async (page: Page, panelTitle: string) =>
  page.getByTestId('panel-header').filter({ hasText: panelTitle })

/** data-testid won't be passed to resizable panel */
const collapsePanel = async (page: Page, panelTitle: string) =>
  await (await panelHeader(page, panelTitle))
    .getByTestId('collapse-panel')
    .click()

/** data-testid will be applied to collapsed panel */
const expandPanel = async (page: Page, panelTestId: string) =>
  page.getByTestId(panelTestId).getByTestId('expand-panel').click()

const getPanel = async (
  page: Page,
  panelContent: /* either title or content */ string
) => {
  return page.locator('[data-panel]', { hasText: panelContent }).last() // so that when it's a panel in group panel, it select tha actual panel
}

const resizeHandle = async (
  page: Page,
  panelGroupIndex: number,
  handleIndex: number
) => {
  return page
    .locator('[data-panel-group]')
    .nth(panelGroupIndex)
    .getByRole('separator')
    .nth(handleIndex)
}
