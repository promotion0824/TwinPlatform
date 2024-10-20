import { titleCase } from '@willow/common'
import { useTimer } from '@willow/ui'
import { Button, ButtonGroup, Modal } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import type { User } from './types'

const ButtonContainer = styled.div({
  display: 'flex',
  justifyContent: 'flex-end',
})

const ModalContent = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  padding: theme.spacing.s16,
}))

export default function ResetProfileModal({
  onClose,
  opened,
  user,
}: {
  onClose: () => void
  opened: boolean
  user: User
}) {
  const timer = useTimer()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [isProcessing, setIsProcessing] = useState(false)

  return (
    <Modal
      centered
      header={titleCase({ language, text: t('headers.resetProfile') })}
      opened={opened}
      onClose={onClose}
    >
      <ModalContent>
        <div>
          {`${t('questions.sureDoThis')} ${t('plainText.sureToResetProfile')}`}
        </div>
        <ButtonContainer>
          <ButtonGroup>
            <Button kind="secondary" onClick={() => onClose()}>
              {t('plainText.cancel')}
            </Button>
            <Button
              kind="negative"
              loading={isProcessing}
              onClick={async () => {
                setIsProcessing(true)
                user.clearAllOptions()
                await timer.sleep(500)
                document.location.reload()
              }}
            >
              {titleCase({ language, text: t('headers.resetProfile') })}
            </Button>
          </ButtonGroup>
        </ButtonContainer>
      </ModalContent>
    </Modal>
  )
}
