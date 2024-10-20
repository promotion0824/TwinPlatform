import { useTranslation } from 'react-i18next'
import { Modal, Message, IconNew, Portal, useModal, Button } from '@willow/ui'
import { styled } from 'twin.macro'
import { useManageModelsOfInterest } from '../Provider/ManageModelsOfInterestProvider'

/**
 * Confirmation modal to delete a model of interest.
 */
export default function DeleteModelOfInterestModal({
  modelName,
  onClose,
}: {
  modelName: string
  onClose: () => void
}) {
  const { t } = useTranslation()

  return (
    <Modal
      header={<HeaderText>{t('plainText.deleteMOI')}</HeaderText>}
      size="medium"
      onClose={onClose}
    >
      <ModalButtons>{t('plainText.deleteMOI')}</ModalButtons>

      <MessageContainer>
        <IconNew icon="warning" color="red" />
        <Message align="center">
          <TextContainer>
            <div>{t('questions.sureToDelete')}</div>
            <div>{`"${modelName}" ?`}</div>
          </TextContainer>
        </Message>
      </MessageContainer>
    </Modal>
  )
}

const HeaderText = styled.span({
  color: '#D9D9D9',
  font: 'normal 500 18px/28px Poppins',
})

const MessageContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  height: '75px',
  marginTop: '24px',
  textAlign: 'center',
})

const TextContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
  font: '400 12px/20px Poppins',
  marginTop: 'var(--padding)',
})

function ModalButtons({ children, ...rest }) {
  const modal = useModal()
  const { t } = useTranslation()
  const { deleteModelOfInterestMutation, deleteModelOfInterest } =
    useManageModelsOfInterest()
  function handleCancelClick() {
    modal.close()
  }
  return (
    <Portal target={modal.modalSubmitButtonRef}>
      <ButtonContainer>
        <StyledButton color="transparent" onClick={handleCancelClick} {...rest}>
          {t('plainText.cancel', { defaultValue: 'Cancel' })}
        </StyledButton>

        <MarginLeftButton
          color="purple"
          data-cy="modalButton-submit"
          type="submit"
          onClick={deleteModelOfInterest}
          loading={deleteModelOfInterestMutation.isLoading}
          successful={deleteModelOfInterestMutation.isSuccess}
          error={deleteModelOfInterestMutation.isError}
          {...rest}
        >
          {children}
        </MarginLeftButton>
      </ButtonContainer>
    </Portal>
  )
}

const ButtonStyle = { padding: '8px 24px' }
const StyledButton = styled(Button)(ButtonStyle)
const MarginLeftButton = styled(Button)({ ...ButtonStyle, marginLeft: '15px' })
const ButtonContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  font: '600 11px/16px Poppins',
})
