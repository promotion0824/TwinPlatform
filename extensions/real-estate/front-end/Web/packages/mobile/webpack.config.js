const HtmlPlugin = require('html-webpack-plugin')
const config = require('../../webpack.config')

/**
 * Eg. makeCspString({"default-src": ["a", "b"], "script-src": ["c", "d"]})
 * produces "default-src a b; script-src c d".
 * We could also use this in platform.
 */
function makeCspString(rules) {
  return Object.entries(rules)
    .map(([ruleName, values]) => `${ruleName} ${values.join(' ')}`)
    .join('; ')
}

/**
 * This is used when we run `npm run build-mobile`, which runs Webpack from the
 * command line. We inject an arbitrary apiUrl since the apiUrl doesn't matter
 * in build since we do not run a server.
 */
async function main() {
  return makeConfig({
    apiUrl: 'https://localhost:5002',
  })
}

module.exports = main

/**
 * @param options {{
 *   // The server URL to proxy requests to.
 *   apiUrl: string,
 * }}
 */
async function makeConfig({ apiUrl }) {
  let pathRewrite
  if (apiUrl.includes('localhost')) {
    pathRewrite = {
      '^/au/api': '',
      '^/us/api': '',
    }
  }

  return config({
    dirname: __dirname,

    output: {
      publicPath: '/mobile-web/public',
    },

    plugins: [
      new HtmlPlugin({
        publicPath: '/mobile-web',
        base: { href: '/mobile/web/' },
        template: './src/index.html',
        favicon: './src/public/favicon.ico',
      }),
    ],

    devServer: {
      port: 8082,
      headers: {
        // CSP header is slightly different in that it has "default-src 'unsafe-eval'"
        // eslint-disable-next-line
        'Content-Security-Policy': makeCspString({
          'default-src': [
            "'unsafe-eval'",
            "'unsafe-inline'",
            "'self'",
            'data:',
            'blob:',
          ],

          'script-src': [
            "'unsafe-eval'",
            "'self'",
            'https://static.zdassets.com',
            'https://cdn.segment.com',
            'http://www.google-analytics.com',
            'https://cdn.mxpnl.com',
            'https://api-js.mixpanel.com',
            'https://app.getbeamer.com/js/beamer-embed.js',
          ],

          'script-src-elem': [
            "'unsafe-eval'",
            "'self'",
            'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.4.456/pdf.worker.js',
            'https://static.zdassets.com',
            'https://cdn.segment.com',
            'http://www.google-analytics.com',
            'https://cdn.mxpnl.com',
            'https://api-js.mixpanel.com',
            'https://app.getbeamer.com/js/beamer-embed.js',
          ],

          'img-src': [
            "'self'",
            'data:',
            'blob:',
            'https://*.azure.com',
            'https://*.azureedge.net',
            'https://*.azurewebsites.net',
            'https://*.s-microsoft.com',
            'https://*.msecnd.net',
            'https://www.google.com',
            'http://www.google-analytics.com',
            'https://stats.g.doubleclick.net',
            'https://app.getbeamer.com',
          ],

          'connect-src': [
            "'self'",
            'https://ekr.zdassets.com',
            'https://willowinc.zendesk.com',
            'https://widget-mediator.zopim.com',
            'https://*.auth0.com',
            'https://willowdevb2c.b2clogin.com/',
            'https://willowidentity.b2clogin.com/',
            'https://api.segment.io',
            'https://api-js.mixpanel.com',
            'https://backend.getbeamer.com',
            'https://*.configcat.com',
          ],

          'worker-src': ["'self'", 'blob:'],

          'frame-ancestors': ["'none'"],

          'form-action': ["'none'"],

          'frame-src': ['*'],

          'manifest-src': ["'self'"],
        }),
      },
      proxy: [
        // Mobile web URLs now look like:
        //
        // (local): http://localhost:8082/mobile-web/...
        //          http://localhost:8082/mobile-web/au/api/... (or us, etc)
        // - The `mobile-web` component is stripped by the `mobile-web` entry
        //   in the config below
        //
        // (ST):    https://customer.app.willowinc.com/mobile-web/...
        //          https://customer.app.willowinc.com/mobile-web/au/api/...
        // - The `mobile-web` component is stripped by the Envoy config.
        {
          context: ['/au/api/', '/us/api/'],
          target: apiUrl,
          headers: {
            host: apiUrl,
          },
          pathRewrite,
          secure: false,
          changeOrigin: true,
          onProxyRes: (proxyRes) => {
            // Remove the 'secure' flag from cookies for development
            if (Array.isArray(proxyRes.headers['set-cookie'])) {
              // eslint-disable-next-line
              proxyRes.headers['set-cookie'] = proxyRes.headers[
                'set-cookie'
              ].map((cookie) =>
                cookie
                  .split(';')
                  .filter((item) => item.trim().toLowerCase() !== 'secure')
                  .join('; ')
              )
            }
          },
        },
        {
          context: ['/mobile-web/'],
          target: 'http://localhost:8082',
          pathRewrite: {
            '^/mobile-web': '/',
          },
        },
        {
          context: ['/mixpanel/'],
          target: 'https://api-js.mixpanel.com',
          secure: false,
          changeOrigin: true,
          pathRewrite: {
            '^/mixpanel': '/',
          },
        },
      ],
    },
  })
}

module.exports.makeConfig = makeConfig
