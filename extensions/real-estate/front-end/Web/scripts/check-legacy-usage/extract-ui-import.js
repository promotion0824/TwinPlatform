// eslint-disable-next-line import/no-extraneous-dependencies
const babelParser = require('@babel/parser')
const fs = require('fs')
const path = require('path')

const skipDirectories = ['../../packages/ui']

async function traverseDirectory(dir = '../../packages') {
  const results = []
  try {
    if (skipDirectories.some((skipDir) => dir.includes(skipDir))) {
      return []
    }

    const files = await fs.promises.readdir(dir)

    for (const file of files) {
      const filePath = path.join(dir, file)
      // eslint-disable-next-line no-await-in-loop
      const stats = await fs.promises.stat(filePath)
      if (stats.isDirectory()) {
        // Recursively traverse subdirectories
        // eslint-disable-next-line no-await-in-loop
        results.push(...(await traverseDirectory(filePath)))
      } else if (stats.isFile() && /\.(js|jsx|ts|tsx)$/.test(file)) {
        results.push(filePath)
      }
    }
  } catch (error) {
    console.error('Error while traversing directory:', error)
  }
  return results
}

const getAst = async (filePath) => {
  const file = await fs.readFileSync(filePath, 'utf-8')
  return babelParser.parse(file, {
    sourceType: 'module',
    plugins: ['jsx', 'typescript'],
  })
}

const getExportedUIComponents = async (
  filePath = '../../packages/ui/src/components/index.ts'
) => {
  const ast = await getAst(filePath)

  return ast.program.body
    .filter((node) => node.type === 'ExportNamedDeclaration')
    .map((exportDeclaration) =>
      // @ts-ignore // inferred type not correct
      exportDeclaration.specifiers.map((specifier) => specifier.exported.name)
    )
    .flat()
}

const getImportedComponents = (ast) =>
  ast.program.body
    .filter(
      (node) =>
        node.type === 'ImportDeclaration' && node.source.value === '@willow/ui'
    )
    .map((importDeclaration) =>
      importDeclaration.specifiers.map((specifier) => specifier.imported.name)
    )
    .flat()

traverseDirectory().then(async (allPaths) => {
  try {
    const results = []
    const targetComponents = await getExportedUIComponents()

    await Promise.all(
      allPaths.map(async (filePath) => {
        const ast = await getAst(filePath)
        const importedComponents = await getImportedComponents(ast)
        const components = importedComponents.filter((c) =>
          targetComponents.includes(c)
        )
        results.push({ components, filePath })
      })
    )

    const outputObject = {}

    results.forEach((item) => {
      item.components.forEach((component) => {
        if (!outputObject[component]) {
          outputObject[component] = {
            counts: 1,
            filePaths: [item.filePath],
          }
        } else {
          // eslint-disable-next-line no-plusplus
          outputObject[component].counts++
          outputObject[component].filePaths.push(item.filePath)
        }
      })
    })

    const sortedComponents = Object.entries(outputObject)
      .sort((a, b) => b[1].counts - a[1].counts)
      .reduce((acc, [component, details]) => {
        acc[component] = details
        return acc
      }, {})

    // list out the result in console
    console.log(sortedComponents)
  } catch (error) {
    console.error(error)
  }
})
