import { Modal } from '@willow/ui'

function NewReportModal({ onClose, header, children }) {
  return (
    <Modal
      header={header}
      size="medium"
      onClose={onClose}
      closeOnClickOutside={false}
    >
      {children}
    </Modal>
  )
}

export default NewReportModal
