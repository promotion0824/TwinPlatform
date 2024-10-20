import { Component } from 'react'
import Message from 'components/Message/Message'

export default class ErrorBoundary extends Component {
  constructor(props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  render() {
    const { children } = this.props
    const { hasError } = this.state

    if (hasError) {
      return <Message icon="error">An error has occurred</Message>
    }

    return children
  }
}
