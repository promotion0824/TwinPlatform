import { useForm, Fieldset, FilesSelectNew } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function Attachments() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Fieldset icon="attachment" legend={t('plainText.attachments')}>
      <FilesSelectNew
        errorName={
          form.data.id == null ? 'attachmentFiles' : 'newAttachmentFiles'
        }
        value={form.data.attachments.map((file) =>
          file instanceof File ? file : { ...file, name: file.fileName }
        )}
        onChange={(attachments) =>
          form.setData((prevData) => ({
            ...prevData,
            attachments,
            isAttachmentModified: true,
          }))
        }
      />
    </Fieldset>
  )
}
