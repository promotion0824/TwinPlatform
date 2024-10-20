import ReactDOM from 'react-dom'
import PortalToElement from './PortalToElement'

export default function Portal({ target = undefined, children }) {
  if (target == null) {
    return ReactDOM.createPortal(children, document.body)
  }

  return <PortalToElement target={target}>{children}</PortalToElement>
}
