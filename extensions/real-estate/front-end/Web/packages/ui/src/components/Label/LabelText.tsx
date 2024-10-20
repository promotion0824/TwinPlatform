import cx from 'classnames'
import TextNew from '../../components/TextNew/Text'
import { ReactNode } from 'react'
import styles from './Label.css'

/**
 * Use this if you want to style text that looks like a form label
 * but doesn't need any of the behaviour.
 */
export default function LabelText({
  children,
  className,
  htmlFor,
  onClick,
  onPointerDown,
  error = false,
}: {
  children: ReactNode
  className?: string
  htmlFor?: string
  onClick?: (e: Event) => void
  onPointerDown?: (e: Event) => void
  /**
   * If true, the label will be styled in an error style (ie. red)
   */
  error?: boolean
}) {
  return (
    <TextNew
      type="label"
      htmlFor={htmlFor}
      className={cx(
        styles.labelControl,
        { [styles.hasError]: error },
        className
      )}
      onPointerDown={onPointerDown}
      onClick={onClick}
    >
      {children}
    </TextNew>
  )
}
