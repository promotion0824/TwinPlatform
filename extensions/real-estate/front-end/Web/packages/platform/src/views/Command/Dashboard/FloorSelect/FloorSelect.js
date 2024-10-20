import { useState } from 'react'
import { useParams } from 'react-router'
import cx from 'classnames'
import { useTranslation } from 'react-i18next'
import { Button, Icon, Text } from '@willow/ui'
import FloorSelectModal from './FloorSelectModal'
import styles from './FloorSelect.css'

export default function FloorSelect({ floors }) {
  const params = useParams()
  const { t } = useTranslation()

  const [isModalVisible, setIsModalVisible] = useState(false)

  const floor = floors.find((prevFloor) => prevFloor.id === params.floorId)

  const cxClassName = cx(styles.floorSelect, {
    [styles.placeholder]: floor == null,
    [styles.colorRed]: floor?.insightsHighestPriority === 1,
    [styles.colorOrange]: floor?.insightsHighestPriority === 2,
    [styles.colorYellow]: floor?.insightsHighestPriority === 3,
  })

  return (
    <>
      <Button className={cxClassName} onClick={() => setIsModalVisible(true)}>
        <Text size="large" color="white" className={styles.content}>
          {floor?.code ?? t('headers.selectFloor')}
        </Text>
        <Icon icon="chevron" className={styles.chevron} />
      </Button>
      {isModalVisible && (
        <FloorSelectModal
          floors={floors}
          onClose={() => setIsModalVisible(false)}
        />
      )}
    </>
  )
}
