import { useRef } from 'react'

export default function useButton(
  {
    type = 'button',
    to,
    href,
    icon,
    iconSize = 'medium',
    color,
    size,
    width,
    disabled = false,
    loading = false,
    success = false,
    error = false,
    selected = false,
    readOnly = false,
    preventDefault = false,
    ripple = false,
    tabIndex,
    className,
    iconClassName,
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

  function getButtonType() {
    const isDisabled = disabled || loading || success || error
    const isDisabledOrReadOnly = isDisabled || readOnly
    const isLink = to != null || href != null
    const isColorButton = color != null
    const isIconButton = icon != null && children == null
    const hasIcon = icon != null && children != null

    return {
      isDisabled,
      isDisabledOrReadOnly,
      isLink,
      isColorButton,
      isIconButton,
      hasIcon,
    }
  }

  function getNextSize(buttonType) {
    if (size === undefined) {
      if (buttonType.isColorButton || buttonType.isIconButton) {
        return 'medium'
      }
    }

    return size
  }

  function getNextRipple(buttonType) {
    if (
      ripple === undefined &&
      (buttonType.isColorButton || buttonType.isIconButton)
    ) {
      return buttonType.isIconButton ? 'center' : true
    }

    return ripple
  }

  function getNextTabIndex(buttonType) {
    if (buttonType.isDisabled) {
      return -1
    }

    return tabIndex
  }

  function getNextButton(buttonType) {
    return {
      size: getNextSize(buttonType),
      ripple: getNextRipple(buttonType),
      tabIndex: getNextTabIndex(buttonType),
    }
  }

  const buttonType = getButtonType()
  const nextButton = getNextButton(buttonType)

  return {
    type,
    to,
    href,
    icon,
    iconSize,
    color,
    size: nextButton.size,
    width,
    disabled,
    loading,
    success,
    error,
    selected,
    readOnly,
    preventDefault,
    ripple: nextButton.ripple,
    tabIndex: nextButton.tabIndex,
    className,
    iconClassName,
    children,
    onPointerDown,
    onPointerUp,
    onPointerLeave,
    onClick,
    rest,

    ref: forwardedRef,
    ripplesRef,
    isDisabled: buttonType.isDisabled,
    isDisabledOrReadOnly: buttonType.isDisabledOrReadOnly,
    isLink: buttonType.isLink,
    isColorButton: buttonType.isColorButton,
    isIconButton: buttonType.isIconButton,
    hasIcon: buttonType.hasIcon,
  }
}
