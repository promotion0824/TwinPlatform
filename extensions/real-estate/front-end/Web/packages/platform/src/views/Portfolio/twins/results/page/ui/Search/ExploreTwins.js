import { styled } from 'twin.macro'
import pluralize from 'pluralize'
import { TwinChip, Progress, UnstyledButton, useAnalytics } from '@willow/ui'
import {
  fileModelId,
  sensorModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import { useSearchResults as useSearchResultsInjected } from '../../state/SearchResults'

const ExploreChips = styled.div({
  display: 'flex',
  flexWrap: 'wrap',
  gap: '0.5rem',
  margin: '0.5rem 1rem 1rem 2rem',
})

const MaxWidthButton = styled(UnstyledButton)({ maxWidth: '15rem' })

const ExploreChip = ({ modelOfInterest, selectedModelId, onClick }) => (
  <MaxWidthButton onClick={onClick}>
    <TwinChip
      modelOfInterest={{
        ...modelOfInterest,
        name: pluralize(modelOfInterest.name),
      }}
      isSelected={modelOfInterest.modelId === selectedModelId}
      highlightOnHover
    />
  </MaxWidthButton>
)

export default function ExploreTwins({
  useSearchResults = useSearchResultsInjected,
}) {
  const { modelId, changeModelId, modelsOfInterest, modelsOfInterestQuery } =
    useSearchResults()
  const analytics = useAnalytics()

  const handleModelChange = (modelOfInterest) => {
    const newModelId = modelOfInterest.modelId

    if (newModelId === modelId) {
      changeModelId(null)
      analytics.track('Search & Explore - Twin Chip Changed', {
        'Selected Twin Chip': null,
      })
    } else {
      changeModelId(newModelId)
      analytics.track('Search & Explore - Twin Chip Changed', {
        'Selected Twin Chip': pluralize(modelOfInterest.name),
      })
    }
  }

  if (modelsOfInterestQuery.isLoading) {
    return <Progress />
  }

  return (
    <ExploreChips>
      {modelsOfInterest.map((modelOfInterest) => {
        if (![fileModelId, sensorModelId].includes(modelOfInterest.modelId)) {
          return (
            <ExploreChip
              key={modelOfInterest.modelId}
              modelOfInterest={modelOfInterest}
              selectedModelId={modelId}
              onClick={() => handleModelChange(modelOfInterest)}
            />
          )
        } else {
          return null
        }
      })}
    </ExploreChips>
  )
}
