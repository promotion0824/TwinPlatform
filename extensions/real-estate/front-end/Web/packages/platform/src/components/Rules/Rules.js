import { Fetch, Flex } from '@willow/ui'
import styles from './Rules.css'

export default function Rules() {
  return (
    <Flex fill="header" padding="small 0">
      <Fetch url="/api/rulingEngine/sites/url">
        {(response) => (
          <iframe
            title="Rules Engine"
            src={response.url}
            className={styles.iframe}
          />
        )}
      </Fetch>
    </Flex>
  )
}
