import cx from 'classnames'
import { Label } from '@willow/ui'
import styles from './TextLabel.css'

export default function TextLabel({
  labelId,
  label,
  type,
  className,
  children,
  ...rest
}) {
  const cxClassName = cx(
    styles.textLabel,
    {
      [styles.typeTextArea]: type === 'textarea',
    },
    className
  )

  return (
    <Label label={label}>
      <span {...rest} className={cxClassName}>
        {children}
      </span>
    </Label>
  )
}
