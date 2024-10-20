import { forwardRef } from 'react'
import cx from 'classnames'
import styles from './Panel.css'

export default forwardRef(function Panel(
  { color = 'normal', className, children, ...rest },
  forwardedRef
) {
  const cxClassName = cx(
    styles.panel,
    {
      [styles.colorNormal]: color === 'normal',
      [styles.colorDark]: color === 'dark',
      [styles.colorLight]: color === 'light',
      [styles.colorBright]: color === 'bright',
    },
    className
  )

  return (
    <div {...rest} ref={forwardedRef} className={cxClassName}>
      {children}
    </div>
  )
})
