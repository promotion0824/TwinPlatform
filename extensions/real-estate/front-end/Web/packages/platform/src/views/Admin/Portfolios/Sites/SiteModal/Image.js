import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi, useSnackbar } from '@willow/ui'
import styles from './Image.css'

export default function Image({ site }) {
  const api = useApi()
  const snackbar = useSnackbar()
  const { t } = useTranslation()

  const [url, setUrl] = useState()

  async function handleFileClick(e) {
    const logoImage = e.currentTarget.files[0]
    e.currentTarget.value = ''

    try {
      const response = await api.put(
        `/api/sites/${site.id}/logo`,
        {
          logoImage,
        },
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      )

      setUrl(response.logoUrl)
      snackbar.show(t('plainText.fileWasUploadedSuccessfully'), { icon: 'ok' })
    } catch (err) {
      snackbar.show(t('plainText.errorUploadImage'))
    }
  }

  return (
    /* eslint-disable-next-line */
    <label className={styles.image}>
      {(url ?? site?.logoUrl != null) && (
        <img src={url ?? site.logoUrl} alt="Logo" className={styles.img} />
      )}
      <input
        type="file"
        accept=".png"
        className={styles.input}
        onChange={handleFileClick}
      />
    </label>
  )
}
