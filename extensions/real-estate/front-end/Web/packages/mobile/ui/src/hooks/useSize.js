import { useLayoutEffect, useState } from 'react'

export default function useSize(ref) {
  const [size, setSize] = useState({
    height: 0,
    width: 0,
  })

  useLayoutEffect(() => {
    const resizeObserver = new ResizeObserver((entries) => {
      const { contentRect } = entries[0]

      if (
        size.height !== contentRect.height ||
        size.width !== contentRect.width
      ) {
        setSize({
          height: contentRect.height,
          width: contentRect.width,
        })
      }
    })

    if (ref.current != null) {
      resizeObserver.observe(ref.current)
    }

    return () => resizeObserver.disconnect()
  }, [ref])

  return size
}
