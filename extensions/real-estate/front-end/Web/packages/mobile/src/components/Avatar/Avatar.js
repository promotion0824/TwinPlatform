import { useMemo } from 'react'
import cx from 'classnames'

import { stringUtils } from '@willow/mobile-ui'

import styles from './Avatar.css'

const COLORS =
  'purple,grey,blue,green,priority1,priority2,priority3,priority4,priority5,priority6'.split(
    ','
  )

export default function Avatar({
  size = 'medium',
  firstName,
  lastName,
  image = undefined,
  color = 'grey',
  className = undefined,
}) {
  const fullName = `${firstName} ${lastName}`
  const nick = `${firstName[0]}${lastName[0] || ''}`

  const avatarColor = useMemo(() => {
    if (image) {
      return null
    }

    if (color) {
      return color
    }

    const hash = Math.abs(stringUtils.hashCode(nick))
    return COLORS[hash % COLORS.length]
  }, [nick, color, image])

  const cxClassNames = cx(styles.avatar, className, {
    [styles.large]: size === 'large',
    [styles.small]: size === 'small',
    [styles.extraLarge]: size === 'extraLarge',
    [styles.hasImage]: styles.image,
    [styles.purple]: avatarColor === 'purple',
    [styles.grey]: avatarColor === 'grey',
    [styles.blue]: avatarColor === 'blue',
    [styles.green]: avatarColor === 'green',
    [styles.priority1]: avatarColor === 'priority1',
    [styles.priority2]: avatarColor === 'priority2',
    [styles.priority3]: avatarColor === 'priority3',
    [styles.priority4]: avatarColor === 'priority4',
    [styles.priority5]: avatarColor === 'priority5',
    [styles.priority6]: avatarColor === 'priority6',
  })

  return image ? (
    <img className={cxClassNames} src={image} alt={fullName} />
  ) : (
    <span className={cxClassNames}> {nick}</span>
  )
}
