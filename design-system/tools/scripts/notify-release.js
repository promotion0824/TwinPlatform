const https = require('https')
const fs = require('fs')

const requiredArgs = ['webhook_uri', 'commit_message']

/**
 * Process the arguments
 * - webhook_uri: The URI of the incoming webhook connector to send a POST request to.
 * - commit_message: The body of the commit message.
 *
 * Currently, we use Microsoft Teams to communicate the releases via the Incoming Webhooks
 * connector. For more details on obtaining and creating an incoming webhook in Teams, see
 * https://learn.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook?tabs=dotnet#create-incoming-webhooks-1
 */
const getArgs = () => {
  let args = {}
  process.argv.slice(2).map((arg) => {
    if (arg.slice(0, 2) === '--') {
      const [flag, value] = arg.slice(2).split('=')
      args[flag] = value
    }
  })

  if (!requiredArgs.every((requiredArg) => args[requiredArg] != null)) {
    console.error('Missing webhook_uri or commit_message')
    console.error(
      'Usage: node notify-release.js --webhook_uri=<URI> --commit_message=<COMMIT_MESSAGE>'
    )
    process.exit(1)
  }

  return args
}

/**
 * Get Outlook Message card data to be posted on Teams.
 * NOTE: Use hexcode for emojis to prevent sending BAD REQUEST (400).
 * See https://learn.microsoft.com/en-us/outlook/actionable-messages/message-card-reference
 */
const getMessageCard = (packageVersion, commitMessage) =>
  JSON.stringify({
    '@type': 'MessageCard',
    '@context': 'http://schema.org/extensions',
    themeColor: '5945d7',
    title: `&#x2728; Release ${packageVersion} &#x2728;`,
    text: `&#x1F4E2; ${commitMessage}`,
    potentialAction: [
      {
        '@type': 'OpenUri',
        name: '&#x1F4D6; View release notes',
        targets: [
          {
            os: 'default',
            // URI of the release notes
            uri: `https://storybook.willowinc.com/${packageVersion}/?path=/docs/release-notes-ui--docs#${packageVersion.replace(
              '.',
              ''
            )}`,
          },
        ],
      },
    ],
  })

const notifyRelease = () => {
  const args = getArgs()
  const uiPackage = JSON.parse(
    fs.readFileSync('libs/ui/package.json').toString()
  )
  const data = getMessageCard(uiPackage.version, args['commit_message'])
  const options = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json; charset=utf-8',
      'Content-Length': data.length,
    },
  }

  return new Promise((resolve, reject) => {
    const req = https
      .request(args['webhook_uri'], options, (res) => {
        let body = ''

        res.on('data', (chunk) => {
          body += chunk
        })

        res.on('end', () => {
          console.log(data)

          if (res.statusCode === 200) {
            resolve(data)
          } else {
            console.error(
              `Status Code [${res.statusCode}]: ${JSON.stringify(data)}`
            )
            reject()
          }
        })
      })
      .on('error', (err) => {
        console.error(err)
        reject()
      })

    req.write(data)
    req.end()
  })
}

console.log('Sending POST request to Webhook...')

notifyRelease()
  .then(() => {
    console.log(
      `Notification successfully sent to Platform UI Releases Channel.`
    )
    process.exit()
  })
  .catch(() => {
    console.error('Failed to send release notification.')
    process.exit(1)
  })
