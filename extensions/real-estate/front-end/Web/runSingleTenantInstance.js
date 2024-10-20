// @ts-check
/* eslint-disable no-console */

const fs = require('fs')
const path = require('path')
const axios = require('axios')
const { selectInstance } = require('./singleTenantUtils')
const yargs = require('yargs/yargs')
const { hideBin } = require('yargs/helpers')
const webpack = require('webpack')
const WebpackDevServer = require('webpack-dev-server')
const {
  makeConfig: makePlatformConfig,
} = require('./packages/platform/webpack.config')
const {
  makeConfig: makeMobileConfig,
} = require('./packages/mobile/webpack.config')

/**
 * To be able to login successfully, the B2C details in the local config.json
 * file need to match the B2C details used by the server. So we can download
 * the correct values from a running instance.
 *
 * @param app {"platform" | "mobile"} which app to download the config for
 * @param serverUrl {string} the base server URL to download the config from
 */
async function downloadConfig(app, serverUrl) {
  const configFilePath = path.resolve(
    __dirname,
    'packages',
    app,
    'src',
    'public',
    'config.json'
  )
  const configUrl = `${serverUrl}/public/config.json`
  // @ts-ignore (doesn't find axios.get for some reason)
  const configJson = (await axios.get(configUrl)).data
  console.log(`Downloading config from ${configUrl} to ${configFilePath}`)
  fs.writeFileSync(configFilePath, JSON.stringify(configJson, null, 2))
}

/**
 * @param options {object}
 * @param options.app {"platform" | "mobile"} which app to run
 * @param options.apiUrl {string} The server URL to proxy requests to.
 *   User will be interactively asked for it if not provided.
 * @param options.downstreamServerUrl {string | undefined} The server URL to retrieve
 *   the config from. This is only needed if `apiUrl` is a local BFF URL that does
 *   not serve a config.json file. User will be interactively asked for it if
 *   the apiUrl is localhost and it is not provided.
 */
async function startServer({ app, apiUrl, downstreamServerUrl }) {
  if (apiUrl == null) {
    // eslint-disable-next-line no-param-reassign
    apiUrl = await selectInstance('Please select an instance:', app)
  }

  if (!apiUrl) {
    console.error('No apiUrl has been set. Exiting...')
    process.exit(1)
  }

  if (apiUrl.includes('localhost') && downstreamServerUrl == null) {
    // eslint-disable-next-line no-param-reassign
    downstreamServerUrl = await selectInstance(
      'Please select a downstream instance to retrieve config from:',
      app
    )
  }

  console.log(`apiUrl is ${apiUrl}`)

  let configMaker
  if (app === 'platform') {
    configMaker = makePlatformConfig
  } else if (app === 'mobile') {
    configMaker = makeMobileConfig
  } else {
    throw new Error('Invalid app name')
  }
  await downloadConfig(app, downstreamServerUrl ?? apiUrl)
  /** @type {any} */
  const config = await configMaker({ apiUrl, downstreamServerUrl })

  const compiler = webpack(config, async (err, stats) => {
    if (err || stats?.hasErrors()) {
      console.error('Failed')
      console.error(err)
    } else {
      const server = new WebpackDevServer(config.devServer, compiler)
      console.log(
        `Starting server on http://localhost:${config.devServer.port}`
      )
      await server.start()
    }
  })
}

function main() {
  const options = yargs(hideBin(process.argv))
    .strict()
    .option('app', {
      description: "Whether to run the main app ('platform') or the mobile app",
    })
    .option('api-url', {
      description: 'The server URL to proxy requests to',
    })
    .option('downstream-server-url', {
      description: 'The server URL to retrieve config from',
    })
    .choices('app', ['platform', 'mobile'])
    .demandOption(['app'])
    .parse()

  startServer({
    // @ts-ignore
    app: options.app,
    // @ts-ignore
    apiUrl: options.apiUrl,
    // @ts-ignore
    downstreamServerUrl: options.downstreamServerUrl,
  })
}

main()
