import { useTranslation } from 'react-i18next'
import { Modal } from '@willow/ui'
import { styled } from 'twin.macro'
import ModelOfInterestForm from './ModelOfInterestForm'
import { FormMode } from '../types'

/**
 * Modal component that contains form to add new MOI or edit existing MOI
 */
export default function ModelOfInterestModal({
  formMode,
  onClose,
}: {
  formMode: FormMode
  onClose: () => void
}) {
  const { t } = useTranslation()

  return (
    <Modal
      header={
        <HeaderText>
          {formMode === 'add'
            ? t('plainText.addModelsOfInterest')
            : t('plainText.editModelsOfInterest')}
        </HeaderText>
      }
      size="medium"
      onClose={onClose}
      isNoOverflow
    >
      <ModelOfInterestForm />
    </Modal>
  )
}

const HeaderText = styled.span({
  color: '#D9D9D9',
  font: 'normal 500 18px/27px Poppins',
})
