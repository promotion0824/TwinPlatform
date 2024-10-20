const fs = require('fs')
const path = require('path')

const { execSync } = require('child_process')

const requiredArgs = ['packages', 'dir']
const distDir = 'dist'

/**
 * Colored logger
 * - Red for error.
 * - Green for info.
 * - Blue for log.
 */
const logger = {
  error: (...msg) => console.error('\x1b[31m', ...msg, '\x1b[0m'),
  info: (...msg) => console.info('\x1b[32m', ...msg, '\x1b[0m'),
  log: (...msg) => console.info('\x1b[34m', ...msg, '\x1b[0m'),
}

const getArgs = () => {
  let args = {}
  process.argv.slice(2).map((arg) => {
    if (arg.slice(0, 2) === '--') {
      const [flag, value] = arg.slice(2).split('=')
      if (flag === 'packages') {
        args[flag] = value.split(',')
      } else {
        args[flag] = value || true
      }
    }
  })

  if (args.help) {
    logger.info(
      'Usage:\n\tnode custom-install.js --packages=<PACKAGE_NAMES> --dir=<PATH/TO/TARGET_PROJECT>\n'
    )
    logger.info('Example:')
    logger.info(
      '\tnode custom-install.js --packages=theme,ui --dir=extensions/real-estate/front-end/Web'
    )
    logger.info(
      '\tnode custom-install.js --packages=palette,mui-theme --dir=core/RulesEngine/RulesEngine.Web/ClientApp'
    )
    process.exit(1)
  }

  if (!requiredArgs.every((requiredArg) => args[requiredArg] != null)) {
    logger.error(
      '✗ Missing required args: packages or dir. Use --help for more details.'
    )
    process.exit(1)
  }

  return args
}

const updateDependencies = (projectDir, packages) => {
  const workspaceRoot = path.resolve(__dirname, '../../../') // root for TwinPlatform
  const designSystemRoot = path.resolve(workspaceRoot, 'design-system') // root for design-system
  const willowTwinRoot = path.join(workspaceRoot, projectDir) // root for Real Estate web app

  const projectPackageJsonPath = path.join(willowTwinRoot, 'package.json')

  const getPackageVersion = (packageName) => {
    const packageJsonPath = path.join(
      designSystemRoot,
      `libs/${packageName}`,
      'package.json'
    )
    try {
      const { version } = JSON.parse(
        fs.readFileSync(packageJsonPath).toString()
      )

      return version
    } catch (error) {
      logger.error(`✗ Package "${packageName}" not found`)
    }
  }

  const relativePathToPackages = path.relative(
    willowTwinRoot,
    path.join(designSystemRoot, distDir)
  )

  const dependenciesToUpdate = {}

  // Pack the package and get the fileName of the packed package.
  packages.forEach((pkg) => {
    logger.log(`⚒  npm pack ${pkg}`)
    execSync(`npm pack ${distDir}/libs/${pkg} --pack-destination ${distDir}`)

    dependenciesToUpdate[`@willowinc/${pkg}`] = `file:${path.join(
      relativePathToPackages,
      `willowinc-${pkg}-${getPackageVersion(pkg)}.tgz`
    )}`
  })

  try {
    const projectPackageJson = JSON.parse(
      fs.readFileSync(projectPackageJsonPath).toString()
    )

    projectPackageJson.dependencies = {
      ...projectPackageJson.dependencies,
      ...dependenciesToUpdate,
    }

    fs.writeFileSync(
      projectPackageJsonPath,
      JSON.stringify(projectPackageJson, null, 2)
    )
    logger.info(
      `☑ Successfully updated dependencies with custom package in ${projectDir}/package.json`
    )
  } catch (error) {
    logger.error(`✗ Project path "${projectDir}" not found.`)
  }
}

const getUninstallPackages = (packages) => {
  const uninstallPackages = packages.map((pkg) => `@willowinc/${pkg}`)

  return uninstallPackages.join(' ')
}

/**
 * Custom install steps:
 *
 * 1) Build all packages
 * 2) Remove the old @willowinc packages from target project as they might
 *    cause conflict with the new installation
 * 3) Update the dependencies in the target project's package.json
 * 4) Custom install dependencies for target project.
 */
const args = getArgs()

logger.log('⚒  Building projects with tag [ci:build]')
execSync('npx nx run-many --target=build --projects=tag:ci:build')

logger.log('⚒  Remove old packages from target project')
execSync(
  `npm uninstall ${getUninstallPackages(args.packages)} --prefix ../${args.dir}`
)

logger.log(
  `⚒  Update dependencies for ${args.packages} for ${args.dir}/package.json`
)
updateDependencies(args.dir, args.packages)

logger.log(`⚒  Installing custom packages on ${args.dir}`)
execSync(`npm install --prefix ../${args.dir} --legacy-peer-deps`)

logger.info('☑ Custom install completed.')
