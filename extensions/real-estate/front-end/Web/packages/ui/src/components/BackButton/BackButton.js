import { useHistory } from 'react-router'
import cx from 'classnames'
import { useTranslation } from 'react-i18next'
import Button from 'components/Button/Button'
import styles from './BackButton.css'

export default function BackButton({
  to,
  href,
  className,
  onClick,
  disabled,
  ...rest
}) {
  const history = useHistory()
  const { t } = useTranslation()
  const isClickable = to != null || href != null || onClick != null

  const cxClassName = cx(styles.backButton, className)

  function handleClick(e) {
    if (!isClickable) {
      history.goBack()
    }

    onClick?.(e)
  }

  return (
    <Button
      icon="left"
      ripple
      {...rest}
      to={to}
      href={href}
      className={cxClassName}
      onClick={handleClick}
      disabled={
        disabled ||
        history.length <= 1 /* history length includes current page */
      }
      role="button"
      aria-label="backButton"
    >
      {t('plainText.back')}
    </Button>
  )
}
