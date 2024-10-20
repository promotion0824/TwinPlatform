const { exec } = require('child_process')
const { hideBin } = require('yargs/helpers')
const { selectInstance } = require('./singleTenantUtils')
const yargs = require('yargs/yargs')

async function main() {
  const options = yargs(hideBin(process.argv))
    .strict()
    .option('app', {
      description:
        "Whether to run the swagger ('platform') or the mobile swagger",
    })
    .option('url', {
      description: 'The url of the swagger doc',
    })
    .option('port', {
      description: 'The port to run the swagger ui on. Defaults to 9000',
    })
    .choices('app', ['platform', 'mobile'])
    .default('port', 9000)
    .demandOption(['app'])
    .parse()

  let apiUrl = options.url
  if (!apiUrl) {
    apiUrl = await selectInstance(
      'Select an instance to see Swagger doc',
      options.app,
      false
    )
  }

  console.log('Starting Swagger UI against:', apiUrl, 'on port:', options.port)

  exec(
    `npx open-swagger-ui --open ${apiUrl}/us/api/swagger/v1/swagger.json --port ${options.port}`,
    (error) => {
      if (error) {
        console.error(`Error starting swagger ui: ${error.message}`)
      }
    }
  )
}

main()
