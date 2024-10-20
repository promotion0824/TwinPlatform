import { forwardRef, useImperativeHandle, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import styles from './Ripples.css'

export default forwardRef(function Ripples({ type }, forwardedRef) {
  const ripplesRef = useRef()

  const [ripples, setRipples] = useState([])

  useImperativeHandle(forwardedRef, () => ({
    ripple(e) {
      const rect = ripplesRef.current.getBoundingClientRect()

      let x
      let y
      let size

      if (type === 'center') {
        x = rect.width / 2
        y = rect.height / 2
        size = Math.max(rect.width, rect.height)
      } else {
        x = e.clientX - rect.left
        y = e.clientY - rect.top

        const sizeX =
          Math.max(Math.abs(ripplesRef.current.clientWidth - x), x) * 2 + 2
        const sizeY =
          Math.max(Math.abs(ripplesRef.current.clientWidth - y), y) * 2 + 2
        size = Math.sqrt(sizeX * sizeX + sizeY * sizeY)
      }

      const nextRipple = {
        rippleId: _.uniqueId(),
        top: y - size / 2,
        left: x - size / 2,
        size,
        isLeaving: false,
      }

      setRipples((prevRipples) => [...prevRipples, nextRipple])
    },

    clear() {
      setRipples((prevRipples) =>
        prevRipples.map((prevRipple) => ({
          ...prevRipple,
          isLeaving: true,
        }))
      )
    },
  }))

  return (
    <div ref={ripplesRef} className={styles.ripples}>
      {ripples.map((ripple) => {
        const cxRippleClassName = cx(styles.ripple, {
          [styles.isLeaving]: ripple.isLeaving,
        })

        return (
          <div key={ripple.rippleId} className={cxRippleClassName}>
            <div
              className={styles.content}
              style={{
                top: ripple.top || undefined,
                left: ripple.left || undefined,
                width: ripple.size || undefined,
                height: ripple.size || undefined,
              }}
            />
          </div>
        )
      })}
    </div>
  )
})
