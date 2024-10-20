import { useEffect } from 'react'

export default function Component(props) {
  const { onMount = () => {} } = props

  useEffect(() => onMount(), [])

  return null
}
