import { useEffect, useRef, useState } from 'react'
import { Error, Progress } from '@willow/ui'

export default function LoadImage({ src, children }) {
  const hasCancelledRef = useRef(false)
  const [dimensions, setDimensions] = useState()
  const [error, setError] = useState()

  useEffect(() => {
    const img = new Image()
    img.onload = () => {
      if (!hasCancelledRef.current) {
        setDimensions({
          width: img.width,
          height: img.height,
        })
      }
    }
    img.onerror = (e) => {
      if (!hasCancelledRef.current) {
        setError(e)
      }
    }
    img.src = src

    return () => {
      hasCancelledRef.current = true
    }
  }, [])

  if (error != null) {
    return <Error />
  }

  if (dimensions == null) {
    return <Progress />
  }

  return children(dimensions)
}
