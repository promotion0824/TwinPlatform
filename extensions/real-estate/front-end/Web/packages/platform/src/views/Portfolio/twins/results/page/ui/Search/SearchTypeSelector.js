import { styled } from 'twin.macro'
import { RadioButton, useAnalytics } from '@willow/ui'

import { useTranslation } from 'react-i18next'

import { useSearchResults as useSearchResultsInjected } from '../../state/SearchResults'

const Fieldset = styled.fieldset({
  padding: 0,
  margin: 0,
  marginTop: '1rem',
  border: 'none',
})

const Label = styled.label({
  marginRight: '1rem',
  fontSize: '11px',
  fontWeight: '500',
  color: '#959595',
})

export default function SearchTypeSelector({
  useSearchResults = useSearchResultsInjected,
}) {
  const analytics = useAnalytics()
  const { t } = useTranslation()
  const {
    hasFileSearch,
    hasSensorSearch,
    searchType,
    setFilterInput,
    changeSearchType,
  } = useSearchResults()

  const handleSearchTypeChange = (e) => {
    const { value } = e.target

    analytics.track('Search & Explore - Search Type Changed', {
      'Selected Search Type': value,
    })

    changeSearchType(value)
    setFilterInput('')
  }

  return (
    <Fieldset>
      <Label>
        <RadioButton
          value="twins"
          checked={searchType === 'twins'}
          onChange={handleSearchTypeChange}
        />
        {t('labels.twins')}
      </Label>

      {hasFileSearch && (
        <Label>
          <RadioButton
            value="files"
            checked={searchType === 'files'}
            onChange={handleSearchTypeChange}
          />
          {t('labels.files')}
        </Label>
      )}

      {hasSensorSearch && (
        <Label>
          <RadioButton
            value="sensors"
            checked={searchType === 'sensors'}
            onChange={handleSearchTypeChange}
          />
          {t('labels.sensors')}
        </Label>
      )}
    </Fieldset>
  )
}
