import { ParamsDict } from '@willow/common/hooks/useMultipleSearchParams'
import { SearchInput, SearchInputProps } from '@willowinc/ui'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

/**
 * This component wraps Platform UI's "SearchInput", and debounce the search input
 */
const DebouncedSearchInput = ({
  onDebouncedSearchChange,
  value,
  ...rest
}: SearchInputProps & {
  onDebouncedSearchChange?: (params: ParamsDict) => void
}) => {
  const { t } = useTranslation()
  const [searchText, setSearchText] = useState(value)
  useEffect(() => {
    let timerId
    if (value !== searchText) {
      timerId = setTimeout(
        () =>
          onDebouncedSearchChange?.({
            // If searchText is falsy, setting it undefined to essentially resets it, as
            // that is how searchSearchParams from useMultipleSearchParams works
            search: searchText ? searchText?.toString() : undefined,
          }),
        1000
      )
    }
    return () => clearTimeout(timerId)
  }, [searchText, onDebouncedSearchChange, value])

  return (
    <SearchInput
      data-testid="search-input"
      onChange={(e) => {
        setSearchText(e.target.value)
      }}
      value={searchText}
      placeholder={t('labels.search')}
      {...rest}
    />
  )
}

export default DebouncedSearchInput
