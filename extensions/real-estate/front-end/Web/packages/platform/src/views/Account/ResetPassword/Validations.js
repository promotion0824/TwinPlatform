import { Flex } from '@willow/ui'
import Validation from './Validation'
import rules from './rules'
import styles from './Validations.css'

export default function Validations({ value }) {
  return (
    <Flex size="tiny" className={styles.validations}>
      {rules.map((rule) => (
        <Validation
          key={rule.description}
          isValid={rule.isValid(value)}
          title={rule.title}
        >
          {rule.description}
        </Validation>
      ))}
    </Flex>
  )
}
