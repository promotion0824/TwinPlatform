import { initializeI18n } from '@willow/common'
import ReactDOM from 'react-dom/client'
import App from './App'
import setupErrorHandlers from './setupErrorHandlers'

try {
  setupErrorHandlers()
} catch (e) {
  // eslint-disable-next-line no-console
  console.error('Error in setting up exception handlers', e)
}

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

initializeI18n()
  .then(() =>
    ReactDOM.createRoot(document.getElementById('root')).render(<App />)
  )
  .catch((err) => console.error(err))
