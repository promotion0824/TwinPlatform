import { getModelDisplayName, Model } from '@willow/common/twins/view/models'
import { IconNew, NotFound, Typeahead, TypeaheadButton } from '@willow/ui'
import { Control, Controller } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import Categories from '../../../../../Portfolio/twins/results/page/ui/Search/Categories'
import { FormMode, PartialModelOfInterest } from '../../../types'
import { FieldSet, FieldValidationText } from '../shared'
import SearchResults, { useSearchResults } from './SearchResults'

/**
 * Input fields to select a model's id and display name.
 * There are two ways users are able to select a model:
 *   1. Search input where the input value will filter and display a list of models from ontology
 *   2. Choose from categories.
 * Both these input components are in-sync, so selecting a model in one component will select a model
 * in the other one.
 */
export default function ChooseMOISection({
  control,
  setSelectedModelOfInterest,
  selectedModelOfInterest,
  formMode,
}: {
  control: Control<PartialModelOfInterest>
  setSelectedModelOfInterest: (modelOfInterest: PartialModelOfInterest) => void
  selectedModelOfInterest?: PartialModelOfInterest
  formMode: FormMode
}) {
  const { t } = useTranslation()

  return (
    <FieldSet label={t('plainText.chooseMOI')}>
      <SearchResults
        setSelectedModelOfInterest={setSelectedModelOfInterest}
        selectedModelOfInterest={selectedModelOfInterest}
        formMode={formMode}
      >
        <SearchInputContainer>
          <Controller
            name="modelId"
            control={control}
            render={({ fieldState }) => {
              const { error } = fieldState
              return <SearchInput error={!!error} />
            }}
            rules={{ required: true }}
          />
        </SearchInputContainer>

        <OrText>{`-${t('plainText.or')}-`}</OrText>

        <Text>{t('plainText.chooseCategoriesBelow')}</Text>

        <IconLabel />
        <SearchCategories />
      </SearchResults>
    </FieldSet>
  )
}
const textStyle = { font: '500 11px/18px Poppins', color: '#959595' }

const Text = styled.div(textStyle)

const OrText = styled.div({
  ...textStyle,
  textTransform: 'uppercase',
  paddingTop: '18px',
  paddingBottom: '18px',
})

function SearchInput({ error }) {
  const translation = useTranslation()
  const { t } = translation
  const {
    searchInput,
    setSearchInput,
    filteredModels,
    changeModelId,
    modelId,
  } = useSearchResults()
  return (
    <>
      <ValidatableTypeahead
        $hasError={!!error}
        label={t('labels.search')}
        selected={false}
        value={searchInput}
        onChange={(search: string) => {
          setSearchInput(search)
        }}
        onSelect={(model: Model) => {
          changeModelId(model['@id'])
          setSearchInput(getModelDisplayName(model, translation))
        }}
        // clear search input if users haven't selected a MOI
        onBlur={() => {
          if (modelId) {
            changeModelId(modelId)
          } else {
            setSearchInput('')
          }
        }}
        noFetch
      >
        {filteredModels.length === 0 && searchInput !== '' && (
          <NotFound>{t('plainText.noModelsFound')}</NotFound>
        )}
        {filteredModels.length > 0 &&
          searchInput !== '' &&
          filteredModels.map((model) => (
            <TypeaheadButton
              key={model['@id']}
              value={model}
              data-testid={`search-option-${getModelDisplayName(
                model,
                translation
              )}`}
            >
              {getModelDisplayName(model, translation)}
            </TypeaheadButton>
          ))}
      </ValidatableTypeahead>
      {!!error && (
        <FieldValidationText>
          {t('plainText.requiredField')}
        </FieldValidationText>
      )}
    </>
  )
}

function SearchCategories() {
  const {
    searchInput,
    setSearchInput,

    modelId,
    changeModelId,
  } = useSearchResults()

  return (
    <CategoriesContainer>
      <Categories
        filterInput={searchInput}
        modelId={modelId}
        onFilterChange={setSearchInput}
        onModelIdChange={changeModelId}
        showCategoriesFilter={false}
      />
    </CategoriesContainer>
  )
}

const SearchInputContainer = styled.div({ width: '193px' })

const ValidatableTypeahead = styled(Typeahead)<{ $hasError: boolean }>(
  ({ $hasError }) => ({
    '> span': {
      border: $hasError ? 'solid 1px var(--red) !important' : undefined,
    },
  })
)

function IconLabel() {
  const { t } = useTranslation()
  return (
    <Label>
      <StyledIcon icon="assets" />
      {t('plainText.categories')}
    </Label>
  )
}

const Label = styled.div({
  display: 'flex',
  alignItems: 'center',

  textTransform: 'uppercase',
  font: '800 10px/15px Poppins',
  color: '#7E7E7E',

  marginTop: '18px',

  '&:first-child': {
    marginTop: '1rem',
  },
})

const StyledIcon = styled(IconNew)({
  color: '#6D6D6D',
  marginRight: '0.5rem',
  marginBottom: '1px',
})

const CategoriesContainer = styled.div({
  width: '250px',
  '> ul': { margin: 'unset' },
})
