const fs = require('fs')
const path = require('path')
const HtmlPlugin = require('html-webpack-plugin')
const config = require('../../webpack.config')

/**
 * This is used when we run `npm run build-mobile`, which runs Webpack from the
 * command line. We inject an arbitrary apiUrl since the apiUrl doesn't matter
 * in build since we do not run a server.
 */
async function main() {
  return makeConfig({
    apiUrl: 'https://localhost:5001',
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
      '^/au/api': '/',
      '^/eu/api': '/',
      '^/us/api': '/',
    }
  }

  // Might be nicer to move this out of `makeConfig` to make `makeConfig` pure,
  // but then we might want to expose a separate setup method to call from
  // `runSingleTenantInstance.js` to make sure that it gets called both from
  // `npm run build` and from `npm start` etc.
  fs.copyFileSync(
    `${path.dirname(require.resolve('pdfjs-dist'))}/pdf.worker.min.js`,
    `${__dirname}/src/public/pdf.worker.min.js`
  )

  return config({
    dirname: __dirname,

    entry: {
      main: './src/index.js',
      '3d-viewer':
        './src/views/Command/Dashboard/FloorViewer/Floor/Editor/3dViewer/index.js',
    },

    plugins: [
      new HtmlPlugin({
        template: './src/index.html',
        favicon: './src/public/favicon.ico',
        excludeChunks: ['3d-viewer'],
      }),
      new HtmlPlugin({
        template:
          './src/views/Command/Dashboard/FloorViewer/Floor/Editor/3dViewer/3d-viewer.html',
        filename: 'public/3d.html',
        chunks: ['3d-viewer'],
      }),
    ],

    externals: {
      autodesk: 'window.Autodesk',
    },

    devServer: {
      port: 8080,
      headers: {
        'Content-Security-Policy': [
          'default-src',
          "'unsafe-eval'",
          "'unsafe-inline'",
          "'self'",
          'data:',
          'blob:',
          'https://wildevpltaue1contentsto.blob.core.windows.net/',
          'https://wildevplteu21contentsto.blob.core.windows.net/',
          'https://wilprdpltaue2contentsto.blob.core.windows.net/',
          'https://wilprdplteu22contentsto.blob.core.windows.net/',
          'https://wilprdpltweu2contentsto.blob.core.windows.net/',
          'https://wiluatpltaue1contentsto.blob.core.windows.net/',
          'https://wiluatplteu21contentsto.blob.core.windows.net/',
          'https://wiluatpltaue1sto.blob.core.windows.net/',
          'https://*.dfwairport.com/',
          'https://*.arcgisonline.com/',
          'https://*.arcgis.com/',
          'https://*.configcat.com',
          'https://developer.api.autodesk.com',
          'https://cdn.derivative.autodesk.com',
          'https://*.auth0.com',
          'https://willowdevb2c.b2clogin.com/',
          'https://willowidentity.b2clogin.com/',
          'https://login.microsoftonline.com/',
          'https://fonts.autodesk.com',
          'https://us.otgs.autodesk.com',
          'https://*.azure.com',
          'https://*.azureedge.net',
          'https://*.azurewebsites.net',
          'https://stats.g.doubleclick.net',
          'https://*.fullstory.com',
          'https://*.getbeamer.com',
          'https://raw.githubusercontent.com',
          'https://www.google.com',
          'https://www.google.com.au',
          'http://www.google-analytics.com',
          'https://*.googleapis.com',
          'https://*.mapbox.com',
          'https://api-js.mixpanel.com',
          'https://*.msecnd.net',
          'http://cdn.mxpnl.com',
          'https://cdn.segment.com',
          'https://api.segment.io',
          'https://*.s-microsoft.com',
          'https://*.zdassets.com',
          'https://willowinc.zendesk.com',
          'https://widget-mediator.zopim.com',
          'https://v2assets.zopim.io',
          'wss://us.otgs.autodesk.com',
          'wss://*.getbeamer.com',
          'wss://willowinc.zendesk.com',
          'wss://widget-mediator.zopim.com',
          'https://localhost:8081;',
          'frame-src',
          '*',
        ].join(' '),

        // This allows our mock service worker to run with a scope of "/" even
        // though the file is accessed via a path beginning with "/public".
        'Service-Worker-Allowed': '/',
      },
      proxy: [
        {
          context: [
            '/api/marketplace/images/',
            '/au/api/',
            '/eu/api/',
            '/us/api/',
          ],
          target: apiUrl,
          headers: {
            host: apiUrl,
          },
          pathRewrite: pathRewrite,
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
