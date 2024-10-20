import { Header, Input, NotFound, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function UsersHeader({ sites, selectedSite, search, onChange }) {
  const { t } = useTranslation()
  return (
    <Header>
      <Input
        icon="search"
        placeholder={t('labels.search')}
        value={search}
        onChange={(nextSearch) =>
          onChange((prevState) => ({
            ...prevState,
            search: nextSearch,
          }))
        }
      />
      <Select
        placeholder={t('placeholder.sites')}
        width="medium"
        value={selectedSite}
        onChange={(nextSelectedSite) =>
          onChange((prevState) => ({
            ...prevState,
            selectedSite: nextSelectedSite,
          }))
        }
      >
        {sites.map((site) => (
          <Option key={site.id} value={site}>
            {site.name}
          </Option>
        ))}
        {sites.length === 0 && (
          <NotFound>{t('plainText.noSitesFound')}</NotFound>
        )}
      </Select>
    </Header>
  )
}
