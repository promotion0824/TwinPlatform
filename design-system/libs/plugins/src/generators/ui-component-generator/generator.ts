import {
  formatFiles,
  generateFiles,
  getWorkspaceLayout,
  offsetFromRoot,
  Tree,
} from '@nx/devkit'
import { camelCase, kebabCase, upperFirst } from 'lodash'
import * as path from 'path'
import { UiComponentGeneratorSchema } from './schema'

const COMPONENT_DIRECTORY = '/ui/src/lib'
const VISUAL_TEST_DIRECTORY = '../apps/ui-e2e/src/visual-regression'
interface NormalizedOptions extends UiComponentGeneratorSchema {
  targetDirectory: string
  templateDirectory: string
}

enum FolderType {
  // Those are the folder names under ./files
  Component = 'component',
  VisualRegression = 'visual-regression',
}

function getTemplateDirectory(templateSubFolder: FolderType) {
  return path.join(__dirname, 'files', templateSubFolder)
}

function getTargetDirectory(
  tree: Tree,
  options: UiComponentGeneratorSchema,
  folderType: FolderType
) {
  const {
    componentName,
    groupName,
    componentDirectory = COMPONENT_DIRECTORY,
    visualTestDirectory = VISUAL_TEST_DIRECTORY,
  } = options
  const folderDirectory =
    folderType === FolderType.Component
      ? `${componentDirectory}/${kebabCase(groupName)}`
      : visualTestDirectory
  const targetDirectory = `${folderDirectory}/${componentName}`
  const componentRoot = `${getWorkspaceLayout(tree).libsDir}/${targetDirectory}`

  return componentRoot
}

function normalizeOptions(
  tree: Tree,
  options: UiComponentGeneratorSchema,
  folderType: FolderType
): NormalizedOptions {
  const targetDirectory = getTargetDirectory(tree, options, folderType)
  const templateDirectory = getTemplateDirectory(folderType)

  return {
    ...options,
    groupName:
      folderType === FolderType.Component
        ? upperFirst(camelCase(options.groupName))
        : options.groupName,
    templateDirectory,
    targetDirectory,
  }
}

function addFiles(tree: Tree, options: NormalizedOptions) {
  const fileName = options.fileName ?? options.componentName
  const templateOptions = {
    ...options,
    fileName,
    offsetFromRoot: offsetFromRoot(options.targetDirectory),
    template: '',
  }

  generateFiles(
    tree,
    options.templateDirectory,
    options.targetDirectory,
    templateOptions
  )
}

export default async function (
  tree: Tree,
  options: UiComponentGeneratorSchema
) {
  addFiles(tree, normalizeOptions(tree, options, FolderType.Component))
  addFiles(tree, normalizeOptions(tree, options, FolderType.VisualRegression))

  await formatFiles(tree)
}
