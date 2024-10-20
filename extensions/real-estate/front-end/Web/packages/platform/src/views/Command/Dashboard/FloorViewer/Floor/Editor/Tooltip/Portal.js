import ReactDOM from 'react-dom'

export default function Portal({ target, children }) {
  if (target === null) {
    return null
  }

  return ReactDOM.createPortal(children, target ?? document.body)
}
