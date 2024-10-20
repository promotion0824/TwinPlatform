import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Portal from 'components/Portal/Portal'
import { useModal } from './ModalContext'

export default function ModalActionButtons({
  showSubmitButton = true,
  children,
  ...rest
}) {
  const modal = useModal()

  return (
    <Portal target={modal.modalSubmitButtonRef}>
      <Flex horizontal size="small">
        {showSubmitButton && (
          <Button color="purple" type="submit" {...rest}>
            {children}
          </Button>
        )}
      </Flex>
    </Portal>
  )
}
