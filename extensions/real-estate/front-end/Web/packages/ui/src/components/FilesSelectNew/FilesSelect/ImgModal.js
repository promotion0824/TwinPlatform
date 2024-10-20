import Modal from 'components/Modal/Modal'
import styles from './ImgModal.css'

export default function ImgModal({ file, src, onClose }) {
  return (
    <Modal header={file.name} type="center" onClose={onClose}>
      <div className={styles.imgContainer}>
        <img src={src} alt={file.name} className={styles.img} />
      </div>
    </Modal>
  )
}
