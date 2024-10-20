import { useState } from 'react'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { Flex, NotFound, Select, Option } from '@willow/ui'
import HistoryItem from './HistoryItem'
import styles from './History.css'

export default function History({ history }) {
  const { t } = useTranslation()
  const [order, setOrder] = useState('desc')

  if (history == null || history.length === 0) {
    return <NotFound>{t('plainText.noHistoryFound')}</NotFound>
  }

  const commands = _.orderBy(history, ['createdAt'], [order])

  return (
    <Flex size="large" className={styles.history}>
      <Flex align="left">
        <Select
          width="medium"
          unselectable
          value={order}
          onChange={(nextOrder) => setOrder(nextOrder)}
        >
          <Option value="desc">{t('plainText.mostRecent')}</Option>
          <Option value="asc">{t('plainText.oldest')}</Option>
        </Select>
      </Flex>
      {commands.map((command) => (
        <HistoryItem key={command.id} command={command} />
      ))}
    </Flex>
  )
}
