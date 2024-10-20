import { Label } from '@willow/mobile-ui'
import noop from 'utils/noop'
import List from 'components/List/List'
import Image from './Image'
import ImageButton from './ImageButton'
import styles from './Images.css'

export default function Images({
  addImageText = 'Add Images',
  allowAdd = true,
  error,
  onAddImage = noop,
  onDeleteImage = noop,
  images = [],
  label,
}) {
  const listItemProps = { onDeleteImage, allowRemove: allowAdd }

  return (
    <div className={styles.container}>
      {label != null && <Label label={label} />}
      <List
        horizontal
        className={styles.imageList}
        activeIndex={-1}
        data={images}
        ListItem={Image}
        listItemProps={listItemProps}
      />
      {allowAdd && (
        <ImageButton
          error={error}
          onAddImage={onAddImage}
          text={addImageText}
        />
      )}
    </div>
  )
}
