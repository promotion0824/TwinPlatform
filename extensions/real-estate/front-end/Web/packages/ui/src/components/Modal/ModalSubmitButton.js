import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Portal from 'components/Portal/Portal'
import { useTranslation } from 'react-i18next'
import { useModal } from './ModalContext'

export default function ModalSubmitButton({
  showCancelButton = true,
  showSubmitButton = true,
  children,
  ...rest
}) {
  const modal = useModal()
  const { t } = useTranslation()

  function handleCancelClick() {
    modal.close()
  }

  return (
    <Portal target={modal?.modalSubmitButtonRef}>
      <Flex horizontal size="small">
        {showCancelButton && (
          <Button color="transparent" onClick={handleCancelClick}>
            {t('plainText.cancel', { defaultValue: 'Cancel' })}
          </Button>
        )}
        {showSubmitButton && (
          <Button
            color="purple"
            data-testid="modalButton-submit"
            type="submit"
            {...rest}
          >
            {children}
          </Button>
        )}
      </Flex>
    </Portal>
  )
}
