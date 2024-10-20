import { Button as SubmitButton, Flex, useModal, Portal } from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'

export default function ScheduleModalButton({ onClick, children, isSubmit }) {
  const modal = useModal()
  const { t } = useTranslation()

  function handleCancelClick() {
    modal.close()
  }

  return (
    <Portal target={modal.modalSubmitButtonRef}>
      <Flex horizontal size="small">
        <Button kind="secondary" onClick={handleCancelClick}>
          {t('plainText.cancel')}
        </Button>

        {isSubmit ? (
          <SubmitButton color="purple" type="submit">
            {children}
          </SubmitButton>
        ) : (
          <Button onClick={onClick}>{children}</Button>
        )}
      </Flex>
    </Portal>
  )
}
