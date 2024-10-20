import { useParams } from 'react-router'
import { NotFound } from '@willow/ui'
import { useSite } from 'providers'
import { useTranslation } from 'react-i18next'

export default function ValidateSiteId({ children }) {
  const params = useParams()
  const site = useSite()
  const { t } = useTranslation()

  if (params.siteId !== site.id) {
    return <NotFound>{t('plainText.noSiteFound')}</NotFound>
  }

  return children
}
