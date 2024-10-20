import { forwardRef } from 'react'
import cx from 'classnames'
import Link from 'components/Link/Link'
import ButtonIcons from './ButtonIcons'
import Ripples from './Ripples/Ripples'
import styles from './Button.css'

export default forwardRef(function ButtonContent(
  {
    type,
    color,
    to,
    href,
    icon,
    iconSize,
    selected,
    loading,
    successful,
    error,
    readOnly,
    disabled,
    isLink,
    isColorButton,
    isIconButton,
    isBlocked,
    ripple,
    ripplesRef,
    height,
    width,
    className,
    iconClassName,
    children,
    onClick,
    onPointerDown,
    onPointerUp,
    onPointerLeave,
    ...rest
  },
  forwardedRef
) {
  let nextHeight = height
  if ((isColorButton || isIconButton) && height == null) {
    nextHeight = 'medium'
  }

  function getClassName() {
    return cx(
      styles.button,
      {
        [styles.isColorButton]: isColorButton,
        [styles.isIconButton]: isIconButton,
        [styles.isBlockedOrReadOnly]: isBlocked || readOnly,
        [styles.selected]: selected,
        [styles.readOnly]: readOnly,
        [styles.disabled]: disabled,
        [styles.colorPurple]: color === 'purple',
        [styles.colorRed]: color === 'red',
        [styles.colorGrey]: color === 'grey',
        [styles.colorGreyLight]: color === 'greyLight',
        [styles.colorTransparent]: color === 'transparent',
        [styles.heightTiny]: nextHeight === 'tiny',
        [styles.heightSmall]: nextHeight === 'small',
        [styles.heightMedium]: nextHeight === 'medium',
        [styles.heightLarge]: nextHeight === 'large',
        [styles.heightExtraLarge]: nextHeight === 'extraLarge',
        [styles.iconButtonHeightTiny]: isIconButton && nextHeight === 'tiny',
        [styles.iconButtonHeightSmall]: isIconButton && nextHeight === 'small',
        [styles.iconButtonHeightMedium]:
          isIconButton && nextHeight === 'medium',
        [styles.iconButtonHeightLarge]: isIconButton && nextHeight === 'large',
        [styles.iconButtonHeightExtraLarge]:
          isIconButton && nextHeight === 'extraLarge',
        [styles.widthSmall]: width === 'small',
        [styles.widthMedium]: width === 'medium',
        [styles.widthLarge]: width === 'large',
        [styles.width100Percent]: width === '100%',
      },
      className
    )
  }

  const cxClassName = getClassName()

  const ButtonComponent = isLink ? Link : 'button'

  return (
    <ButtonComponent
      {...rest}
      ref={forwardedRef}
      type={!isLink ? type : 'button'}
      to={to}
      href={href}
      disabled={!isLink ? isBlocked || readOnly : undefined}
      className={cxClassName}
      data-is-selected={selected ? true : undefined}
      onClick={onClick}
      onPointerDown={onPointerDown}
      onPointerUp={onPointerUp}
      onPointerLeave={onPointerLeave}
    >
      <ButtonIcons
        icon={icon}
        iconSize={iconSize}
        loading={loading}
        successful={successful}
        error={error}
        iconClassName={iconClassName}
      />
      {children}
      <Ripples ref={ripplesRef} type={ripple} />
    </ButtonComponent>
  )
})
