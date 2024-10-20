import { useLayoutEffect, useState } from 'react'
import ReactDOM from 'react-dom'

export default function PortalToElement({ target, children }) {
  const [shouldRender, setShouldRender] = useState(false)

  useLayoutEffect(() => {
    if (target.current != null) {
      setShouldRender(true)
    }
  }, [target.current])

  if (!shouldRender) {
    return null
  }

  return ReactDOM.createPortal(children, target.current)
}
