import _ from 'lodash'
import cx from 'classnames'
import { useUniqueId } from 'hooks'
import { LabelContext } from './LabelContext'
import styles from './Label.css'

export default function Label({
  labelId,
  label,
  readOnly,
  error,
  showError = true,
  value,
  required,
  className,
  labelClassName,
  children,
  ...rest
}) {
  const nextLabelId = useUniqueId()
  const nextId = labelId ?? nextLabelId

  const cxClassName = cx(
    styles.label,
    {
      [styles.readOnly]: readOnly,
      [styles.hasError]: error != null,
    },
    className
  )
  const cxLabelClassName = cx(styles.labelControl, labelClassName)

  const context = {
    id: nextId,
  }

  function getLabelProperties() {
    return {
      showLabel: label != null || (showError && error != null),
      showRequired: required && (value == null || value === '') && !readOnly,
    }
  }

  const labelProperties = getLabelProperties()

  return (
    <LabelContext.Provider value={context}>
      {labelProperties.showLabel && (
        <span className={cxClassName}>
          <label htmlFor={nextId} {...rest} className={cxLabelClassName}>
            {showError && error != null ? (
              error
            ) : (
              <>
                {label}
                {labelProperties.showRequired && (
                  <span className={styles.required}> *</span>
                )}
              </>
            )}
          </label>
          {_.isFunction(children) ? children(context) : children}
        </span>
      )}
      {!labelProperties.showLabel &&
        (_.isFunction(children) ? children(context) : children)}
    </LabelContext.Provider>
  )
}
