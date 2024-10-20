import cx from 'classnames'
import { useUser } from '@willow/ui'
import Button from 'components/Button/Button'
import styles from './SiteFavoriteButton.css'

export default function SiteFavoriteButton({
  siteId,
  className,
  onClick,
  user = undefined,
  ...rest
}) {
  const userContext = useUser() || user

  const isSelected = userContext?.options?.favoriteSiteId === siteId

  const cxClassName = cx(
    styles.favoriteButton,
    {
      [styles.selected]: isSelected,
    },
    className
  )

  function handleFavoriteClick(e) {
    e.preventDefault()
    e.stopPropagation()

    if (isSelected) {
      userContext.clearOptions('favoriteSiteId')
    } else {
      userContext.saveOptions('favoriteSiteId', siteId)
    }

    onClick?.()
  }

  return (
    <Button
      icon="star"
      iconSize="small"
      tabIndex={-1}
      {...rest}
      className={cxClassName}
      onClick={handleFavoriteClick}
    />
  )
}
