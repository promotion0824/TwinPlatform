import cx from 'classnames'
import styles from './Text.css'

export default function Text({
  type,
  color,
  textTransform,
  whiteSpace,
  className,
  children,
  ...rest
}) {
  let Component = 'span'
  if (type === 'label') Component = 'label'

  const cxClassName = cx(
    styles.text,
    {
      [styles.typeLabel]: type === 'label',
      [styles.typeGroup]: type === 'group',
      [styles.colorDark]: color === 'dark',
      [styles.textTransformUppercase]: textTransform === 'uppercase',
      [styles.whiteSpaceNowrap]: whiteSpace === 'nowrap',
    },
    className
  )

  return (
    <Component {...rest} className={cxClassName}>
      {children}
    </Component>
  )
}
