import { Modal } from '@willow/ui'

function ReportModal({ onClose, header, children }) {
  return (
    <Modal header={header} size="medium" onClose={onClose}>
      {children}
    </Modal>
  )
}

export default ReportModal
