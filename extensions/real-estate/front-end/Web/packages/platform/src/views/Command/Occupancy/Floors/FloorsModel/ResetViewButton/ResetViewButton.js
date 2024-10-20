import cx from 'classnames'
import { useTranslation } from 'react-i18next'
import { Flex } from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useFloors } from '../../FloorsContext'
import styles from './ResetViewButton.css'

export default function ResetViewButton() {
  const floorsContext = useFloors()
  const { t } = useTranslation()

  const cxClassName = cx(styles.resetViewButton, {
    [styles.isVisible]: floorsContext.hasCameraMoved,
  })

  return (
    <Flex size="tiny" padding="medium" className={cxClassName}>
      <Button
        kind="secondary"
        onClick={() => floorsContext.setResetCamera(true)}
      >
        {t('plainText.resetView')}
      </Button>
    </Flex>
  )
}
