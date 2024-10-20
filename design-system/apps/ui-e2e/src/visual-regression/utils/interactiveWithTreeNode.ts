import { expect, Page } from '@willowinc/playwright'

export const clickTreeNode = (page: Page, name: string) =>
  page.getByText(name).click()

export const getTreeNode = (page: Page, collapsed: boolean, name: string) =>
  page
    .getByRole('treeitem', {
      name: `${collapsed ? 'arrow_right' : 'arrow_drop_down'} ${name}`,
    })
    .getByRole('button')

export const openBuildingComponent = async (page: Page) => {
  const nodeName = 'Building Component'

  getTreeNode(page, true, nodeName).click({
    // this button takes a long time to reveal sometimes
    timeout: 8000,
  })

  return await expectTreeNode(page, nodeName, 'opened')
}

export const closeBuildingComponent = (page: Page) => {
  const nodeName = 'Building Component'

  getTreeNode(page, false, nodeName).click({
    // this button takes a long time to reveal sometimes
    timeout: 8000,
  })

  return expectTreeNode(page, nodeName, 'collapsed')
}

export const openArchitecturalBuildingComponent = async (page: Page) => {
  const nodeName = 'Architectural Building Component'

  getTreeNode(page, true, nodeName).click({
    // this button takes a long time to reveal sometimes
    timeout: 8000,
  })

  return await expectTreeNode(page, nodeName, 'opened')
}

const expectTreeNode = (
  page: Page,
  name: string,
  status: 'opened' | 'collapsed'
) =>
  expect(
    getTreeNode(page, status === 'opened' ? false : true, name)
  ).toBeVisible({
    // this ui takes a long time to update sometimes
    timeout: 8000,
  })
