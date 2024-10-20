import { forwardRef } from 'react'
import cx from 'classnames'
import Icon from 'components/Icon/Icon'
import Link from 'components/Link/Link'
import Spacing from 'components/Spacing/Spacing'
import Ripples from './Ripples/Ripples'
import useButton from './useButton'
import useButtonEvents from './useButtonEvents'
import styles from './Button.css'

export default forwardRef(function Button(props, forwardedRef) {
  const button = useButton(props, forwardedRef)
  const buttonEvents = useButtonEvents(button)

  const cxClassName = cx(
    styles.button,
    {
      [styles.isDisabled]: button.isDisabledOrReadOnly,
      [styles.isSelected]: button.selected,
      [styles.isColorButton]: button.isColorButton,
      [styles.isIconButton]: button.isIconButton,
      [styles.hasIcon]: button.hasIcon,
      [styles.colorButtonSizeSmall]:
        button.isColorButton && button.size === 'small',
      [styles.colorButtonSizeMedium]:
        button.isColorButton && button.size === 'medium',
      [styles.colorButtonSizeLarge]:
        button.isColorButton && button.size === 'large',
      [styles.iconButtonSizeMedium]:
        button.isIconButton && button.size === 'medium',
      [styles.iconButtonSizeLarge]:
        button.isIconButton && button.size === 'large',
      [styles.blue]: button.color === 'blue',
      [styles.red]: button.color === 'red',
      [styles.grey]: button.color === 'grey',
      [styles.white]: button.color === 'white',
      [styles.colorButtonDisabled]: button.isColorButton && button.disabled,
      [styles.width100Percent]: button.width === '100%',
      [styles.widthLarge]: button.width === 'large',
    },
    button.className
  )
  const cxIconClassName = cx(styles.icon, button.iconClassName)

  const content = (
    <>
      {button.loading && (
        <Spacing
          position="absolute"
          align="center middle"
          className={styles.loading}
        >
          <Icon icon="progress" />
        </Spacing>
      )}
      {button.success && (
        <Spacing
          position="absolute"
          align="center middle"
          className={styles.success}
        >
          <Icon icon="ok" />
        </Spacing>
      )}
      {button.error && (
        <Spacing
          position="absolute"
          align="center middle"
          className={styles.error}
        >
          <Icon icon="close" />
        </Spacing>
      )}
      {button.icon != null && (
        <Icon
          icon={button.icon}
          size={button.iconSize}
          className={cxIconClassName}
        />
      )}
      {button.children}
      <Ripples ref={button.ripplesRef} ripple={button.ripple} />
    </>
  )

  return button.isLink ? (
    <Link
      {...button.rest}
      ref={button.ref}
      role="button"
      to={button.to}
      href={button.href}
      disabled={button.isDisabled}
      tabIndex={button.tabIndex}
      className={cxClassName}
      onClick={buttonEvents.handleClick}
      onPointerDown={buttonEvents.handlePointerDown}
      onPointerUp={buttonEvents.handlePointerUp}
      onPointerLeave={buttonEvents.handlePointerLeave}
      data-is-selected={button.selected}
    >
      {content}
    </Link>
  ) : (
    // eslint-disable-next-line
    <button
      {...button.rest}
      ref={button.ref}
      type={button.type} // eslint-disable-line
      disabled={button.isDisabled}
      tabIndex={button.tabIndex}
      className={cxClassName}
      onClick={buttonEvents.handleClick}
      onPointerDown={buttonEvents.handlePointerDown}
      onPointerUp={buttonEvents.handlePointerUp}
      onPointerLeave={buttonEvents.handlePointerLeave}
      data-is-selected={button.selected}
    >
      {content}
    </button>
  )
})
