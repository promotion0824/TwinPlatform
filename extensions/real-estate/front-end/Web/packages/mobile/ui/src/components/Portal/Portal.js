import ReactDOM from 'react-dom'

export default function Portal(props) {
  const { target, children } = props

  if (target === null) {
    return null
  }

  return ReactDOM.createPortal(children, target ?? document.body)
}
