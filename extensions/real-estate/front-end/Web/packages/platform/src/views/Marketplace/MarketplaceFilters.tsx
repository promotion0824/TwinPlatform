import { titleCase } from '@willow/common'
import { useLanguage } from '@willow/ui'
import {
  Checkbox,
  CheckboxGroup,
  PanelContent,
  SearchInput,
} from '@willowinc/ui'
import { orderBy } from 'lodash'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import useGetAppCategories from '../../hooks/Marketplace/useGetAppCategories'
import activatePacks from './activatePacks'

const FilterPanelContent = styled(PanelContent)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s12,
  padding: theme.spacing.s16,
}))

export default function MarketplaceFilters({
  search,
  selectedCategories,
  selectedStatuses,
  selectedActivatePacks,
  onSearchChange,
  onSelectedCategoriesChange,
  onSelectedStatusesChange,
  onSelectedActivatePacksChange,
  hideSearchInput = false,
}: {
  search: string
  selectedCategories: string[]
  selectedStatuses: string[]
  selectedActivatePacks: string[]
  onSearchChange: (search: string) => void
  onSelectedCategoriesChange: (selectedCategoryIds: string[]) => void
  onSelectedStatusesChange: (selectedStatuses: string[]) => void
  onSelectedActivatePacksChange: (selectedActivatePacks: string[]) => void
  hideSearchInput?: boolean
}) {
  const appCategoriesRequest = useGetAppCategories()
  const { t } = useTranslation()
  const { language } = useLanguage()

  const appCategories = orderBy(
    appCategoriesRequest.isSuccess ? appCategoriesRequest.data : [],
    (category) => category.name
  )

  const sortedActivatePacks = orderBy(activatePacks, (activatePack) =>
    t(activatePack.translationKey)
  )

  return (
    <FilterPanelContent>
      {!hideSearchInput && (
        <SearchInput
          label={t('labels.search')}
          onChange={(event) => onSearchChange(event.currentTarget.value)}
          placeholder={t('placeholder.searchConnectors')}
          value={search}
        />
      )}

      <CheckboxGroup
        label={t('labels.status')}
        onChange={onSelectedStatusesChange}
        value={selectedStatuses}
      >
        <Checkbox label={t('plainText.installed')} value="installed" />
      </CheckboxGroup>

      <CheckboxGroup
        label={titleCase({ text: t('labels.activatePack'), language })}
        onChange={onSelectedActivatePacksChange}
        value={selectedActivatePacks}
      >
        {sortedActivatePacks.map((activatePack) => (
          <Checkbox
            key={activatePack.id}
            label={t(activatePack.translationKey)}
            value={activatePack.id}
          />
        ))}
      </CheckboxGroup>

      <CheckboxGroup
        label={t('plainText.categories')}
        onChange={onSelectedCategoriesChange}
        value={selectedCategories}
      >
        {appCategories.map((category) => (
          <Checkbox
            key={category.id}
            label={category.name}
            value={category.id}
          />
        ))}
      </CheckboxGroup>
    </FilterPanelContent>
  )
}
