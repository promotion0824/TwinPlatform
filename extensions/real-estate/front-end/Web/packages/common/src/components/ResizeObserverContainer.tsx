import { debounce } from 'lodash'
import { HTMLAttributes, ReactNode, useEffect, useRef } from 'react'

export default function ResizeObserverContainer({
  children,
  onContainerWidthChange,
  ...rest
}: {
  children: ReactNode
  onContainerWidthChange: (width: number) => void
} & HTMLAttributes<HTMLDivElement>) {
  const ref = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    const element = ref.current

    if (element) {
      const resizeObserver = new ResizeObserver(
        debounce(([entry]) => {
          onContainerWidthChange(entry.contentRect.width)
        }, 100)
      )

      resizeObserver.observe(element)

      return () => {
        resizeObserver.unobserve(element)
      }
    }

    return undefined
  }, [ref, onContainerWidthChange])

  return (
    <div ref={ref} {...rest}>
      {children}
    </div>
  )
}
