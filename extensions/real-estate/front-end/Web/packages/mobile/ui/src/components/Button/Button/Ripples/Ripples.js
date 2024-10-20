import { forwardRef, useImperativeHandle, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import Timeout from 'components/Timeout/Timeout'
import styles from './Ripples.css'

export default forwardRef(function Ripples({ ripple }, forwardedRef) {
  const ripplesRef = useRef()
  const [ripples, setRipples] = useState([])

  useImperativeHandle(forwardedRef, () => ({
    ripple(e) {
      const rect = ripplesRef.current.getBoundingClientRect()

      let x
      let y
      let size
      if (ripple === 'center') {
        x = rect.width / 2
        y = rect.height / 2

        size = rect.width
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
      }

      setRipples((prevRipples) => [...prevRipples, nextRipple])
    },

    clear() {
      setRipples((prevRipples) =>
        prevRipples.map((prevRipple) => ({
          ...prevRipple,
          leaving: true,
        }))
      )
    },
  }))

  function removeRipple(rippleId) {
    setRipples((prevRipples) =>
      prevRipples.filter((prevRipple) => prevRipple.rippleId !== rippleId)
    )
  }

  return (
    <div ref={ripplesRef} className={styles.ripples}>
      {ripples.map((rippleObj) => {
        const cxClassName = cx(styles.ripple, {
          [styles.center]: ripple === 'center',
          [styles.leaving]: rippleObj.leaving,
        })

        return (
          <div key={rippleObj.rippleId} className={cxClassName}>
            <div
              className={styles.child}
              style={{
                top: rippleObj.top,
                left: rippleObj.left,
                width: rippleObj.size,
                height: rippleObj.size,
              }}
            />
            {rippleObj.leaving && (
              <Timeout
                ms={300}
                onTimeout={() => removeRipple(rippleObj.rippleId)}
              />
            )}
          </div>
        )
      })}
    </div>
  )
})
