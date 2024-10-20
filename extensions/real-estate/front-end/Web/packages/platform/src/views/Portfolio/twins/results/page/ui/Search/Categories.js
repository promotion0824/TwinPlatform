import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import { getModelDisplayName } from '@willow/common/twins/view/models'
import { Message, Progress } from '@willow/ui'

import CategoriesFilter from './CategoriesFilter'
import useOntologyInjected from '../../../../../../../hooks/useOntologyInPlatform'
import SearchItem from './SearchItem'

function makeDisplayNameComparator(translation) {
  return (a, b) =>
    getModelDisplayName(a, translation).localeCompare(
      getModelDisplayName(b, translation)
    )
}

const SearchItemList = styled.ul(({ theme }) => ({
  paddingLeft: theme.spacing.s32,
}))

const Categories = ({
  /** Override the label used for the All Categories option. */
  allCategoriesLabel = undefined,
  /** The model that should be selected when the All Categories option is selected. */
  allCategoriesModelId = undefined,
  /** Value of the search field. */
  filterInput,
  /** The ID of the currently selected model. */
  modelId,
  /** Function called when the filter changed. */
  onFilterChange,
  /** Function called when an item is selected. */
  onModelIdChange,
  /** Toggles whether the categories filter is displayed. */
  showCategoriesFilter = true,
  /** An array of category IDs to be displayed as the top level list of categories. */
  topCategoryIds = [
    'dtmi:com:willowinc:Asset;1',
    'dtmi:com:willowinc:BuildingComponent;1',
    'dtmi:com:willowinc:Collection;1',
    'dtmi:com:willowinc:Component;1',
    'dtmi:com:willowinc:Space;1',
    'dtmi:com:willowinc:Structure;1',
  ],
  /** Used to override the z-index of all typeaheads. Useful when used inside a Modal.  */
  typeaheadZIndex = 'var(--z-dropdown)',
  useOntology = useOntologyInjected,
}) => {
  const ontologyQuery = useOntology()
  const translation = useTranslation()
  const { t } = translation

  if (ontologyQuery.isError) {
    return <Message icon="error">{t('plainText.errorOccurred')}</Message>
  }
  if (ontologyQuery.isLoading) {
    return <Progress />
  }

  const ontology = ontologyQuery.data

  const categoriesFilter = (
    <CategoriesFilter
      filterInput={filterInput}
      onFilterChange={onFilterChange}
      onModelIdChange={onModelIdChange}
      ontology={ontology}
      translation={translation}
      topCategories={topCategoryIds}
      typeaheadZIndex={typeaheadZIndex}
    />
  )

  if (!modelId || modelId === allCategoriesModelId) {
    return (
      <>
        {showCategoriesFilter && categoriesFilter}
        <SearchItemList>
          <SearchItem isSelected>
            {allCategoriesLabel || t('plainText.allCategories')}
          </SearchItem>
          {topCategoryIds.map((childId) => {
            const child = ontology?.getModelById(childId)
            return (
              child && (
                <SearchItem
                  indented
                  key={child['@id']}
                  onClick={() => onModelIdChange(childId)}
                >
                  {getModelDisplayName(child, translation)}
                </SearchItem>
              )
            )
          })}
        </SearchItemList>
      </>
    )
  }

  const ancestorsId = ontology.getModelAncestorsIdBetween(
    modelId,
    topCategoryIds
  )
  const children = ontology.getModelChildren(modelId)

  return (
    <>
      {showCategoriesFilter && categoriesFilter}
      <ul>
        <SearchItem
          onClick={() => {
            onModelIdChange(allCategoriesModelId)
            if (showCategoriesFilter) onFilterChange('')
          }}
        >
          {allCategoriesLabel || t('plainText.allCategories')}
        </SearchItem>
        {ancestorsId.map((parentId) => {
          const parent = ontology.getModelById(parentId)
          return (
            parent && (
              <SearchItem
                key={parentId}
                onClick={() => onModelIdChange(parentId)}
              >
                {getModelDisplayName(parent, translation)}
              </SearchItem>
            )
          )
        })}

        {children.length ? (
          // if there are children, display them
          <>
            <SearchItem isSelected>
              {getModelDisplayName(ontology.getModelById(modelId), translation)}
            </SearchItem>
            {children
              .sort(makeDisplayNameComparator(translation))
              .map((child) => (
                <SearchItem
                  indented
                  key={child['@id']}
                  onClick={() => onModelIdChange(child['@id'])}
                >
                  {getModelDisplayName(child, translation)}
                </SearchItem>
              ))}
          </>
        ) : ancestorsId.length > 0 ? (
          // if there are no children, display its siblings next to it
          ontology
            .getModelChildren(ancestorsId[ancestorsId.length - 1])
            .sort(makeDisplayNameComparator(translation))
            .map((sibling) => (
              <SearchItem
                key={sibling['@id']}
                indented
                isSelected={sibling['@id'] === modelId}
                onClick={() => onModelIdChange(sibling['@id'])}
              >
                {getModelDisplayName(sibling, translation)}
              </SearchItem>
            ))
        ) : null}
      </ul>
    </>
  )
}

export default Categories
