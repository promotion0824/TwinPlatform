import { initializeI18n } from '@willow/common'
import ReactDOM from 'react-dom/client'
import Site from './Site'

initializeI18n().then(() => {
  ReactDOM.createRoot(document.getElementById('root')).render(<Site />)
})
