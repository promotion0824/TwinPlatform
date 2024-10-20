import cx from 'classnames'
import styles from './Text.css'

export default function Text({
  type,
  color,
  className,
  children,
  whiteSpace = 'nowrap',
  ...rest
}) {
  let nextColor = color
  if (color === undefined) {
    if (type === 'h4' || type === 'label') {
      nextColor = 'muted'
    }
  }

  const cxClassName = cx(
    styles.text,
    {
      [styles.h1]: type === 'h1',
      [styles.h3]: type === 'h3',
      [styles.h4]: type === 'h4',
      [styles.label]: type === 'label',
      [styles.value]: type === 'value',
      [styles.muted]: nextColor === 'muted',
      [styles.whiteSpaceNoWrap]: whiteSpace === 'nowrap',
      [styles.whiteSpaceNormal]: whiteSpace === 'normal',
    },
    className
  )

  return (
    <span {...rest} className={cxClassName}>
      {children}
    </span>
  )
}
