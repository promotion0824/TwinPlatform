import { Tree } from '@nx/devkit'
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing'

import generator from './generator'
import { UiComponentGeneratorSchema } from './schema'

describe('willow-ui-component-generator', () => {
  const componentName = 'testComponent'
  const groupName = 'inputs'
  const componentPath = 'ui/src/lib'
  const visualTestPath = '../apps/ui-e2e/src/visual-regression'

  let appTree: Tree
  const options: UiComponentGeneratorSchema = {
    componentName,
    groupName,
  }

  beforeEach(() => {
    appTree = createTreeWithEmptyWorkspace()
  })

  it('should have desired component files exist', async () => {
    await generator(appTree, options)

    expect(
      appTree.exists(
        `${componentPath}/${groupName}/${componentName}/${componentName}.spec.tsx`
      )
    ).toBeTruthy()
    expect(
      appTree.exists(
        `${componentPath}/${groupName}/${componentName}/${componentName}.stories.tsx`
      )
    ).toBeTruthy()
    expect(
      appTree.exists(
        `${componentPath}/${groupName}/${componentName}/visualTestOnly.stories.tsx`
      )
    ).toBeTruthy()
    expect(
      appTree.exists(
        `${componentPath}/${groupName}/${componentName}/${componentName}.tsx`
      )
    ).toBeTruthy()
    expect(
      appTree.exists(`${componentPath}/${groupName}/${componentName}/index.tsx`)
    ).toBeTruthy()
    expect(
      appTree.exists(`${componentPath}/${groupName}/${componentName}/Docs.mdx`)
    ).toBeTruthy()
    expect(
      appTree.exists(`${componentPath}/${groupName}/${componentName}/Props.mdx`)
    ).toBeTruthy()
  })

  it('should have desired test files exist', async () => {
    await generator(appTree, options)

    expect(
      appTree.exists(
        `${visualTestPath}/${componentName}/${componentName}.spec.ts`
      )
    ).toBeTruthy()
  })
})
