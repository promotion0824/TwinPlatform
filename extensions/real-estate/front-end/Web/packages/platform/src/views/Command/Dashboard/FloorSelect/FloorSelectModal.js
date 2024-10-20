import { Modal, NotFound } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import FloorSelectButton from './FloorSelectButton'

export default function FloorSelectModal({ floors, onClose }) {
  const { t } = useTranslation()

  return (
    <Modal
      header={t('headers.selectFloor')}
      size="small"
      type="left"
      onClose={onClose}
    >
      {floors.map((floor) => (
        <FloorSelectButton key={floor.id} floor={floor} />
      ))}
      {floors.length === 0 && (
        <NotFound>{t('plainText.noFloorsFound')}</NotFound>
      )}
    </Modal>
  )
}
