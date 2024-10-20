import { Page, expect, test } from '@willowinc/playwright'

const componentName = 'DataGrid'

test.describe('Pinned column', async () => {
  test.beforeEach(async ({ storybook, page }) => {
    await page.setViewportSize({ width: 400, height: 100 })
    await storybook.gotoHidden(componentName, 'ColumnPinning')
  })

  test('background color should match for header cell', async ({
    storybook,
    page,
  }) => {
    const headerCell = firstHeaderCell(page)

    await storybook.testInteractions(
      'pinnedHeaderCell',
      headerCell,
      headerCell,
      ['default', 'hover']
    )
  })

  test('background color should match for column cell', async ({
    storybook,
    page,
  }) => {
    const firstCellAtFirstColumn = page.getByRole('cell').first()

    await storybook.testInteractions(
      'pinnedColumnCell',
      firstCellAtFirstColumn,
      firstCellAtFirstColumn,
      ['default', 'hover']
    )
  })

  test('border color should match for the pinned column', async ({ page }) => {
    // no good way to screenshot the pinned header cell to include the border
    // applied by boxShadow, so screenshot the whole header row instead.
    // It is not a single rowgroup like normal table
    const headerRow = page.getByText(
      'idarrow_upwardDeskarrow_upwardCommodityarrow_upwardTrader Namearrow_upwardQuanti'
    )
    await expect(headerRow).toHaveScreenshot('headerRow.png')

    // no good way to screenshot the table row as a whole, so skip the test for now
  })
})
test.describe('Column resizing', async () => {
  test.beforeEach(async ({ storybook, page }) => {
    await page.setViewportSize({ width: 600, height: 100 })
    await storybook.gotoHidden(componentName, 'Default')
  })

  test('should have correct handle style when hover the row', async ({
    page,
  }) => {
    await selectHeaderRow(page).hover()

    await expect(selectNthResizeHandle(page)).toHaveScreenshot(
      'column-resize-handle-on-row-hover.png'
    )
  })

  test('should have correct handle style when hover the handle', async ({
    page,
  }) => {
    // show the handle
    await selectHeaderRow(page).hover()

    const handle = selectNthResizeHandle(page)
    await handle.hover()

    await expect(handle).toHaveScreenshot(
      'column-resize-handle-on-handle-hover.png'
    )
  })

  test('should have correct handle style when dragging the handle', async ({
    page,
  }) => {
    // show the handle
    await selectHeaderRow(page).hover()

    const thirdHandle = selectNthResizeHandle(page, 3)
    await thirdHandle.hover()
    await page.mouse.down()
    await firstHeaderCell(page).hover()

    await expect(thirdHandle).toHaveScreenshot(
      'column-resize-handle-on-handle-drag.png'
    )
  })
})

const firstHeaderCell = (page: Page) => page.getByRole('columnheader').first()
const selectHeaderRow = (page: Page) => page.getByRole('rowgroup').first()
const selectNthResizeHandle = (page: Page, order = 1) =>
  page.locator('.MuiDataGrid-columnSeparator').nth(order)
