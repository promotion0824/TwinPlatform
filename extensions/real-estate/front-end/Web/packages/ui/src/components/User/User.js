import tw from 'twin.macro'
import { useTranslation } from 'react-i18next'
import cx from 'classnames'
import styles from './User.css'

export default function User({ user, displayAsText = false, ...rest }) {
  const { t } = useTranslation()
  const className = cx({
    [styles.user]: !displayAsText,
  })
  const firstName =
    user.name != null ? user.name.split(' ')[0] ?? '' : user.firstName ?? ''

  const lastName =
    user.name != null ? user.name.split(' ')[1] ?? '' : user.lastName ?? ''

  const firstInitial = firstName?.[0] ?? ''
  const lastInitial = lastName?.[0] ?? ''

  const name = `${firstName} ${lastName}`

  const textContent = displayAsText
    ? `${firstName}${lastInitial ? ` ${lastInitial}.` : ''}`
    : `${firstInitial}${lastInitial}`

  return (
    <div
      tw="truncate"
      data-tooltip={
        textContent === 'Unassigned' ? t('plainText.unassigned') : name
      }
      data-tooltip-position="top"
      {...rest}
      className={className}
    >
      {textContent === 'Unassigned' ? t('plainText.unassigned') : textContent}
    </div>
  )
}
