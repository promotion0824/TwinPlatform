import _ from 'lodash'
import cx from 'classnames'
import { useUniqueIdNew } from 'hooks'
import { LabelContext } from './LabelContext'
import styles from './Label.css'

export default function Label({
  labelId,
  label,
  readOnly,
  className,
  labelClassName,
  children,
  ...rest
}) {
  const nextLabelId = useUniqueIdNew()
  const nextId = labelId ?? nextLabelId

  const cxClassName = cx(
    styles.label,
    {
      [styles.readOnly]: readOnly,
    },
    className
  )
  const cxLabelClassName = cx(styles.labelControl, labelClassName)

  const context = {
    id: nextId,
  }

  return (
    <LabelContext.Provider value={context}>
      <span className={cxClassName}>
        {label != null && (
          <label htmlFor={nextId} {...rest} className={cxLabelClassName}>
            {label}
          </label>
        )}
        {_.isFunction(children) ? children(context) : children}
      </span>
    </LabelContext.Provider>
  )
}
