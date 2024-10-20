import { Spacing, useDateTime } from '@willow/mobile-ui'
import Avatar from 'components/Avatar/Avatar'
import styles from './Comment.css'

export default function Comment({ text: comment, creator, createdDate }) {
  const dateTime = useDateTime()
  const { firstName, lastName } = creator

  return (
    <Spacing className={styles.comment} type="content">
      <Spacing horizontal>
        <Avatar firstName={firstName} lastName={lastName} size="large" />
        <Spacing className={styles.detail} type="content">
          <span className={styles.username}>{`${firstName} ${lastName}`}</span>
          <span className={styles.date}>
            {dateTime(createdDate).format('dateTimeLong')}
          </span>
        </Spacing>
      </Spacing>
      <div className={styles.content}>{comment}</div>
    </Spacing>
  )
}
