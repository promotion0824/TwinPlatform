import { useEffect, useState } from 'react'
import { useSnackbar, useUser, Progress } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function GetAttachments({ loadAttachments, children }) {
  const snackbar = useSnackbar()
  const user = useUser()
  const { t } = useTranslation()

  const [attachments, setAttachments] = useState()

  useEffect(() => {
    async function loadAttachment(screenshot, i) {
      const response = await window.fetch(screenshot)
      const blob = await response.blob()

      return new File([blob], `screenshot-${i + 1}.png`)
    }

    async function load() {
      try {
        if (!loadAttachments) {
          setAttachments([])
          return
        }

        const screenshots = user.localOptions.insight?.screenshots ?? []

        const nextAttachments = await Promise.all(
          screenshots.map((screenshot, i) => loadAttachment(screenshot, i))
        )

        setAttachments(nextAttachments)
      } catch (err) {
        snackbar.show(t('plainText.errorIntoTicket'))

        setAttachments([])
      }
    }

    load()
  }, [])

  if (attachments == null) {
    return <Progress />
  }

  return children(attachments)
}
