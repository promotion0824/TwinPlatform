import { useEffect } from 'react'

export default function Component({ onMount = () => {} }) {
  useEffect(() => onMount(), [])

  return null
}
