import Button from 'components/Button/Button'
import Spacing from 'components/Spacing/Spacing'
import TextNew from 'components/Text/Text'
import styles from './OptGroup.css'

export default function OptGroup({ label, children }) {
  return (
    <>
      <Button className={styles.optGroup} onClick={(e) => e.preventDefault}>
        <Spacing padding="medium">
          <TextNew type="h4">{label}</TextNew>
        </Spacing>
      </Button>
      {children ?? null}
    </>
  )
}
