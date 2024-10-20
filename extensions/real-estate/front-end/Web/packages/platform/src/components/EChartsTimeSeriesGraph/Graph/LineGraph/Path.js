import { useLayoutEffect, useRef, useState } from 'react'
import { useTimer } from '@willow/ui'
import styles from './Path.css'

export default function Path({ ...rest }) {
  const timer = useTimer()

  const pathRef = useRef()
  const [style, setStyle] = useState()

  useLayoutEffect(() => {
    async function load() {
      const length = pathRef.current.getTotalLength()

      setStyle({
        strokeDasharray: length,
        strokeDashoffset: length,
      })

      await timer.sleep(300)

      setStyle()
    }

    load()
  }, [])

  return (
    <path
      {...rest}
      ref={pathRef}
      className={styles.path}
      style={style}
      onAnimationEnd={() => {
        setStyle()
      }}
    />
  )
}
