import { useMemo, useState } from 'react'
import { UseTranslationResponse } from 'react-i18next'
import { styled } from 'twin.macro'
import { getModelDisplayName, Ontology } from '@willow/common/twins/view/models'
import { Typeahead, TypeaheadButton } from '@willow/ui'

type Category = {
  id: string
  displayName: string
}

const StyledTypeahead = styled(Typeahead)(({ theme }) => ({
  marginTop: theme.spacing.s12,
  width: '100%',
}))

export default function CategoriesFilter({
  filterInput,
  onFilterChange,
  onModelIdChange,
  ontology,
  translation,
  topCategories,
  typeaheadZIndex,
}: {
  filterInput?: string
  onFilterChange: (filterInput: string) => void
  onModelIdChange: (id: string) => void
  ontology: Ontology
  translation: UseTranslationResponse<'translation', undefined>
  topCategories: string[]
  typeaheadZIndex?: string
}) {
  const [selectedCategory, setSelectedCategory] = useState<Category>()
  const { t } = translation

  const allCategories = useMemo(
    () =>
      [...topCategories, ...ontology.getModelDescendants(topCategories)]
        .filter(
          (category, index, categories) =>
            categories.lastIndexOf(category) === index
        )
        .map((modelId) => {
          const model = ontology.getModelById(modelId)
          return {
            id: model['@id'],
            displayName: getModelDisplayName(model, translation),
          }
        })
        .sort((a, b) =>
          a.displayName.toLowerCase() < b.displayName.toLowerCase() ? -1 : 1
        ),
    [ontology, translation, topCategories]
  )

  const filteredCategories = !filterInput
    ? allCategories
    : allCategories.filter((category) =>
        category.displayName.toLowerCase().includes(filterInput.toLowerCase())
      )

  return (
    <StyledTypeahead
      noFetch
      onBlur={() => onFilterChange(selectedCategory?.displayName ?? '')}
      onChange={onFilterChange}
      onSelect={(category: Category) => {
        onModelIdChange(category.id)
        setSelectedCategory(category)
        onFilterChange(category.displayName)
      }}
      placeholder={t('placeholder.filterCategories')}
      preservePlaceholder
      selected={false}
      value={filterInput}
      zIndex={typeaheadZIndex}
    >
      {filteredCategories.map((category) => (
        <TypeaheadButton key={category.id} value={category}>
          {category.displayName}
        </TypeaheadButton>
      ))}
    </StyledTypeahead>
  )
}
