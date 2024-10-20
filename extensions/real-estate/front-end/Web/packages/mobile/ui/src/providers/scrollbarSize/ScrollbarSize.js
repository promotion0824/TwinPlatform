import { useLayoutEffect, useRef } from 'react'
import { useWindowEventListener } from 'hooks'
import styles from './ScrollbarSize.css'

export default function ScrollbarSize(props) {
  const { onChange = () => {} } = props

  const ref = useRef()
  const scrollbarSizeRef = useRef({})

  function refresh() {
    const prevWidth = scrollbarSizeRef.current.width
    const prevHeight = scrollbarSizeRef.current.height

    scrollbarSizeRef.current = {
      width: ref.current.offsetWidth - ref.current.clientWidth,
      height: ref.current.offsetHeight - ref.current.clientHeight,
    }

    if (
      prevWidth !== scrollbarSizeRef.current.width ||
      prevHeight !== scrollbarSizeRef.current.height
    ) {
      onChange(scrollbarSizeRef.current)
    }
  }

  useWindowEventListener('resize', () => {
    refresh()
  })

  useLayoutEffect(() => {
    refresh()
  }, [])

  return <div ref={ref} className={styles.scrollbarSize} />
}
