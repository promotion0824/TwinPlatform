import { useState, useEffect } from 'react'
import tw, { styled } from 'twin.macro'
import Flex from '../Flex/Flex'
import Input from '../Input/Input'
import NotFound from '../NotFound/NotFound'

/**
 * Checks whether item provided matches the filter based on the custom filter function or
 * the value of the key of interest (as provided by the list of key).
 * This filters the item and returns a boolean value based on the following conditions:
 * 1) The custom filter function (if specified) that accepts the item and search text and returns boolean, OR
 * 2) The provided list of keys, iterating through the key to check the item's value for a boolean true OR
 * case-insensitvely contains the user specified searchText
 * @param {object} item The item
 * @param {string[]} keys The list of keys to search within the item object
 * @param {string} searchText The user specified search text
 * @param {requestCallback} [fn] Optional custom filter function that returns a boolean based on provided item object and search text
 * @returns {boolean} Whether the item matches the filter by filter function or values of the item's keys
 */
const matchesFilters = (item, keys, searchText, fn) =>
  (fn && fn(item, searchText)) ||
  keys?.some((key) => {
    if (typeof item[key] === 'boolean') {
      return item[key]
    }
    return !!`${item[key]}`
      .trim()
      .toLowerCase()
      .includes(searchText.trim().toLowerCase())
  })

// Sticky input as an alternative to container with only max-height and unknown height.
const SearchInput = styled(Input)(({ theme }) => ({
  ...tw`sticky top-0 z-10 border-0`,
  borderRadius: 0,
  backgroundColor: 'var(--theme-color-neutral-bg-accent-default)',
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  '&:hover': {
    backgroundColor: '#373739',
  },
  '&:focus-within': {
    boxShadow: 'none',
    borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  },
}))

/**
 * Composite component of search input and scrollable list. This component actively filters the
 * list as user types in the search text.
 * NOTE: To use this component on container without a defined height, the container must have
 * overflow: auto for this to work.
 */
const SearchList = ({
  inputPlaceholder,
  emptyMessage,
  items = [],
  searchKeys,
  filterFn,
  renderItem,
}) => {
  const [search, setSearch] = useState('')
  const [filteredItems, setFilteredItems] = useState(items)

  if (!(searchKeys && searchKeys.length) && !filterFn) {
    throw new Error('Either searchKeys or filterFn must be defined')
  }

  useEffect(() => {
    setFilteredItems(
      search.trim()
        ? items.filter((item) =>
            matchesFilters(item, searchKeys, search, filterFn)
          )
        : items
    )
  }, [search, items, searchKeys, filterFn])

  return (
    <Flex height="100%" tw="relative">
      <SearchInput
        icon="search"
        placeholder={inputPlaceholder}
        debounce
        value={search}
        onChange={setSearch}
      />
      <Flex flex="1" overflow="auto">
        {filteredItems.length ? (
          filteredItems.map((item, index) => renderItem(item, index))
        ) : (
          <NotFound>{emptyMessage}</NotFound>
        )}
      </Flex>
    </Flex>
  )
}

export default SearchList
