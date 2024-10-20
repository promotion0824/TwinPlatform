import { Header, Input, NotFound, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function UsersHeader({
  portfolios,
  sites,
  selectedPortfolio,
  selectedSite,
  search,
  onChange,
}) {
  const { t } = useTranslation()
  return (
    <Header>
      <Input
        icon="search"
        placeholder={t('labels.search')}
        value={search}
        debounce
        onChange={(nextSearch) =>
          onChange((prevState) => ({
            ...prevState,
            search: nextSearch,
          }))
        }
      />
      <Select
        placeholder={t('headers.portfolios')}
        header={() => selectedPortfolio?.name}
        width="medium"
        value={selectedPortfolio}
        onChange={(portfolio) =>
          onChange((prevState) => ({
            ...prevState,
            selectedPortfolio: portfolio,
            selectedSite: undefined,
          }))
        }
      >
        {portfolios.map((nextPortfolio) => (
          <Option key={nextPortfolio.id} value={nextPortfolio}>
            {nextPortfolio.name}
          </Option>
        ))}
        {portfolios.length === 0 && (
          <NotFound>{t('plainText.noPortfoliosFound')}</NotFound>
        )}
      </Select>
      <Select
        placeholder={t('placeholder.sites')}
        header={() => selectedSite?.name}
        width="medium"
        value={selectedSite}
        onChange={(site) =>
          onChange((prevState) => ({
            ...prevState,
            selectedPortfolio: undefined,
            selectedSite: site,
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
