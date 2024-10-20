// Polyfill `globalThis` and `Array.prototype.at` which are not available on
// some recent iPad browsers.
import 'core-js/actual/array/at'
import 'core-js/actual/global-this'

import { initializeI18n } from '@willow/common'
import ReactDOM from 'react-dom/client'
import ResizeObserver from 'resize-observer-polyfill'
import 'tailwindcss/tailwind.css'
import App from './App'

window.ResizeObserver = window.ResizeObserver || ResizeObserver

if (process.env.MOCK_SERVICE_WORKER === 'true') {
  const makeWorker = require('./mockServer').default
  const worker = makeWorker()
  worker.start({
    serviceWorker: {
      url: '/public/mockServiceWorker.js',
      options: {
        scope: '/',
      },
    },
  })
}

/*
initializeI18n will return a promise,
so we wait till it resolve and then render the app
*/
initializeI18n()
  .then(() => {
    ReactDOM.createRoot(document.getElementById('root')).render(<App />)
  })
  .catch((reason) => console.error(reason))
