import { FullSizeContainer, FullSizeLoader } from '@willow/common'
import { Group, Modal } from '@willowinc/ui'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import Message from '../Message/Message'

/**
 * A modal that displays an image, where "opened" state is controlled.
 */
export default function ImgModal({
  src,
  name,
  onClose,
}: {
  src: string
  name: string
  onClose: () => void
}) {
  const { t } = useTranslation()
  const [isError, setIsError] = useState(false)
  const [imageSrc, setImageSrc] = useState<string | undefined>(undefined)

  useEffect(() => {
    const img = new Image()
    img.src = src
    img.onload = () => {
      setImageSrc(src)
    }
    img.onerror = () => {
      setIsError(true)
    }
  }, [src])

  return (
    <Modal
      header={name}
      onClose={onClose}
      size="xl"
      opened
      // Ensure body of Modal takes up full height that isn't taken up by the header
      styles={{
        content: { height: '100%', display: 'flex', flexDirection: 'column' },
        body: {
          flex: 1,
        },
      }}
    >
      {isError ? (
        <FullSizeContainer>
          <Message icon="error">{t('plainText.errorOccurred')}</Message>
        </FullSizeContainer>
      ) : !imageSrc ? (
        <FullSizeLoader />
      ) : (
        <Group h="100%" w="100%" justify="center" align="center">
          <img src={imageSrc} alt={name} />
        </Group>
      )}
    </Modal>
  )
}
