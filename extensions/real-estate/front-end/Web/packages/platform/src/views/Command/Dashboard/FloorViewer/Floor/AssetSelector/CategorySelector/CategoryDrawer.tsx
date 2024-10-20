import { Box, Drawer } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { styled, css } from 'twin.macro'
import { Typeahead, TypeaheadButton } from '@willow/ui'
import { titleCase } from '@willow/common'
import { useState } from 'react'
import { useAssetSelector } from '../AssetSelectorContext'
import SearchItem from '../../../../../../Portfolio/twins/results/page/ui/Search/SearchItem'

type Category = {
  assetCount: number
  id: string
  name?: string
  modelId?: string
  hasChildren: boolean
  childCategories: Category[]
}

const CategoryDrawer = ({
  categories = [],
  isOpen,
  onToggle,
}: {
  categories: Category[]
  isOpen: boolean
  onToggle: (isOpen: boolean) => void
}) => {
  const [filterInput, setFilterInput] = useState<string>('')
  const assetSelector = useAssetSelector()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const filteredCategories = filterInput
    ? categories.filter((category) =>
        (category?.name ?? '').toLowerCase().includes(filterInput.toLowerCase())
      )
    : categories

  return (
    <Drawer
      opened={isOpen}
      onClose={() => onToggle(false)}
      header={t('headers.filters')}
      position="left"
    >
      <Box p="s16">
        <StyledTitle>
          {titleCase({ text: t('labels.twinCategory'), language })}
        </StyledTitle>
        <StyledTypeahead
          noFetch
          onChange={setFilterInput}
          onSelect={(categoryName: string) => {
            const selectedCategory = categories.find(({ name }) =>
              (name ?? '').toLowerCase().includes(filterInput.toLowerCase())
            )
            setFilterInput(categoryName)
            assetSelector.toggleCategory(selectedCategory)
          }}
          placeholder={t('placeholder.filterCategories')}
          preservePlaceholder
          selected={false}
          value={filterInput}
          zIndex="300"
        >
          {filteredCategories.map((category) => (
            <TypeaheadButton key={category.id} value={category.name}>
              {category.name}
            </TypeaheadButton>
          ))}
        </StyledTypeahead>
        <Box pl="s16" pt="s12">
          <SearchItem
            indented
            isSelected={assetSelector.categories.length === 0}
            css={css(({ theme }) => ({
              '&&& > button': {
                marginTop: theme.spacing.s12,
              },
            }))}
            onClick={() => {
              setFilterInput('')
              assetSelector.setCategories([])
            }}
          >
            {t('plainText.allCategories')}
          </SearchItem>
        </Box>
        <ul
          css={css`
            padding-left: 48px;
          `}
        >
          {assetSelector.categories.length === 0 &&
            categories.map((category) => (
              <SearchItem
                indented={false}
                isSelected={false}
                key={category.id}
                onClick={() => {
                  setFilterInput('')
                  assetSelector.toggleCategory(category)
                }}
              >
                {category.name}
              </SearchItem>
            ))}
          {assetSelector.categories.map((selectedCategory, index) => {
            const isLastCategory = index === assetSelector.categories.length - 1
            return (
              <>
                <SearchItem
                  indented={false}
                  isSelected={isLastCategory}
                  key={selectedCategory.id}
                  onClick={() => assetSelector.toggleCategory(selectedCategory)}
                >
                  {selectedCategory.name}
                </SearchItem>
                {isLastCategory &&
                  selectedCategory.childCategories.map((childCategory) => (
                    <SearchItem
                      indented
                      isSelected={false}
                      key={childCategory.id}
                      onClick={() =>
                        assetSelector.toggleCategory(childCategory, index + 1)
                      }
                    >
                      {childCategory.name}
                    </SearchItem>
                  ))}
              </>
            )
          })}
        </ul>
      </Box>
    </Drawer>
  )
}

const StyledTitle = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
  marginBottom: theme.spacing.s8,
}))

const StyledTypeahead = styled(Typeahead)({
  width: '100%',
})

export default CategoryDrawer
