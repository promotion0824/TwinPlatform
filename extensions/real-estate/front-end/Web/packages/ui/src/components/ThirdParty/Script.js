import { useEffect, useState } from 'react'

export default function Script({ children, ...rest }) {
  const [hasLoaded, setHasLoaded] = useState(false)

  useEffect(() => {
    const script = document.createElement('script')

    Object.keys(rest).forEach((key) => {
      script[key] = rest[key]
    })

    script.onload = () => {
      setHasLoaded(true)
    }

    document.body.appendChild(script)

    return () => {
      document.body.removeChild(script)
    }
  }, [])

  if (!hasLoaded) {
    return null
  }

  return children ?? null
}
