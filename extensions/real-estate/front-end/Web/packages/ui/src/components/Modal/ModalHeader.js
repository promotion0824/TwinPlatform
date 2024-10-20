import Flex from 'components/Flex/Flex'
import Portal from 'components/Portal/Portal'
import { useModal } from './ModalContext'
import styles from './ModalHeader.css'

export default function ModalHeader({ children, ...rest }) {
  const modal = useModal()

  return (
    <Portal target={modal.modalHeaderRef}>
      <Flex {...rest} padding="medium 0" className={styles.modalHeader}>
        {children}
      </Flex>
    </Portal>
  )
}
