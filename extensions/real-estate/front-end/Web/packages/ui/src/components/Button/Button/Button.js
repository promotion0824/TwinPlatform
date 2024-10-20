import { forwardRef, useRef } from 'react'
import useButtonEvents from './useButtonEvents'
import ButtonContent from './ButtonContent'

export default forwardRef(function Button(
  {
    type = 'button',
    color,
    to,
    href,
    icon,
    isIconButton: isIconButtonProp,
    loading,
    successful,
    error,
    readOnly,
    disabled,
    preventDefault = true,
    ripple,
    className,
    children,
    onClick = () => {},
    onPointerDown = () => {},
    onPointerUp = () => {},
    onPointerLeave = () => {},
    ...rest
  },
  forwardedRef
) {
  const ripplesRef = useRef()

  const isLink = to != null || href != null
  const isColorButton = color != null
  const isIconButton = (icon != null && children == null) || isIconButtonProp
  const isBlocked = loading || successful || error || disabled

  let nextRipple = ripple
  if (ripple === undefined) {
    if (isColorButton) {
      nextRipple = 'normal'
    }
    if (isIconButton) {
      nextRipple = 'center'
    }
  }

  const buttonEvents = useButtonEvents({
    type,
    readOnly,
    isBlocked,
    isLink,
    preventDefault,
    ripple: nextRipple,
    ripplesRef,
    onClick,
    onPointerDown,
    onPointerUp,
    onPointerLeave,
  })

  return (
    <ButtonContent
      ref={forwardedRef}
      type={type}
      color={color}
      to={to}
      href={href}
      icon={icon}
      loading={loading}
      successful={successful}
      error={error}
      readOnly={readOnly}
      disabled={disabled}
      isLink={isLink}
      isColorButton={isColorButton}
      isIconButton={isIconButton}
      isBlocked={isBlocked}
      ripple={nextRipple}
      ripplesRef={ripplesRef}
      className={className}
      onClick={buttonEvents.handleClick}
      onPointerDown={buttonEvents.handlePointerDown}
      onPointerUp={buttonEvents.handlePointerUp}
      onPointerLeave={buttonEvents.handlePointerLeave}
      {...rest}
    >
      {children}
    </ButtonContent>
  )
})
