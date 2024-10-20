import { forwardRef } from 'react'
import Button from 'components/Button/Button'
import Icon from 'components/Icon/Icon'
import cx from 'classnames'
import styles from './Checkbox.css'

export default forwardRef(function Checkbox(
  {
    value = false,
    readOnly,
    className,
    children,
    onChange,
    onClick,
    height,
    width,
    padding = null,
    absolute = false,
    icon = 'ok',
    iconSize = 'tiny',
    boxNoBackground,
    iconColor = null,
    fontSmall,
    fontLight = false,
    ...rest
  },
  forwardedRef
) {
  const cxClassName = cx(
    styles.checkbox,
    {
      [styles.readOnly]: readOnly,
      [styles.selected]: value,
      [styles.heightTiny]: height === 'tiny',
      [styles.width100Percent]: width === 'full',
      [styles.paddingLarge]: padding === 'large',
      [styles.widthSmall]: width === 'small',
      [styles.fontLight]: fontLight === true,
    },
    className
  )

  function handleClick(e) {
    onChange?.(!value)
    onClick?.(e)
  }

  return (
    <Button
      {...rest}
      ref={forwardedRef}
      readOnly={readOnly}
      className={cxClassName}
      onClick={handleClick}
    >
      <div
        className={absolute ? styles.boxContainerAbsolute : styles.boxContainer}
      >
        <div className={boxNoBackground ? styles.boxNoBackground : styles.box}>
          {icon && (
            <Icon
              icon={icon}
              size={iconSize}
              className={iconColor ? styles[iconColor] : styles.icon}
            />
          )}
        </div>
      </div>
      {children != null && (
        <div
          className={`${styles.content} ${fontSmall && styles.fontSmall} ${
            fontLight && styles.fontLight
          }`}
        >
          {children}
        </div>
      )}
    </Button>
  )
})
