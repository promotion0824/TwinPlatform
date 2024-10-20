import cx from 'classnames'
import Dropdown from 'components/Dropdown/Dropdown'
import IconNew from 'components/IconNew/Icon'
import { useTranslation } from 'react-i18next'
import styles from './MoreButton.css'

export { default as MoreDropdownButton } from './MoreDropdownButton'

export default function MoreButton({
  type = 'vertical',
  className,
  iconClassName,
  showTooltipArrow = false,
  contentStyle,
  children,
  onClick,
  ...rest
}) {
  const { t } = useTranslation()
  const cxClassName = cx(styles.more, className)
  const cxIconClassName = cx(styles.icon, iconClassName)

  function handleClick(e) {
    e.preventDefault()
    e.stopPropagation()

    onClick?.(e)
  }

  return (
    <Dropdown
      customHeader
      icon={type === 'vertical' ? 'more' : undefined}
      header={type !== 'vertical' ? <IconNew icon="more" /> : undefined}
      ripple="center"
      {...rest}
      className={cxClassName}
      contentClassName={[styles.content, showTooltipArrow && styles.tooltip]}
      contentStyle={contentStyle}
      iconClassName={cxIconClassName}
      onClick={handleClick}
      title={t('plainText.more')}
    >
      {children}
    </Dropdown>
  )
}
