import { Progress, Text, useFeatureFlag } from '@willow/ui'
import { styled } from 'twin.macro'

import { sensorModelId } from '@willow/common/twins/view/modelsOfInterest'
import { useSearchResults } from '../../results/page/state/SearchResults'
import TwinTypeTile from './TwinTypeTile'

const ExploreContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  alignItems: 'center',
})

const Sections = styled.div({
  display: 'flex',
  // Roughly max 7 items per row, but can be fewer.
  maxWidth: '60em',
  flexWrap: 'wrap',
  justifyContent: 'center',
  marginTop: '1rem',
  gap: '1rem',
  marginBottom: '1rem',
})

/**
 * Displays a search box, and chips for each of the user's models of interest.
 * The documents model is always included, and the sensors model is included if
 * and only if the twinExplorerSensorSearch feature flag is enabled.
 */
export default function Explore({ useContext = useSearchResults, onSearch }) {
  const { t, modelsOfInterest, changeModelId } = useContext()
  const featureFlags = useFeatureFlag()

  const hasSensorsFeature = featureFlags.hasFeatureToggle(
    'twinExplorerSensorSearch'
  )

  return (
    <ExploreContainer>
      <Text
        type="h2"
        size="huge"
        weight="medium"
        color="light"
        style={{ margin: '5rem 0 0.5rem' }}
        tw="h-full"
      >
        {t('plainText.explore')}
      </Text>
      <Sections>
        {modelsOfInterest != null ? (
          modelsOfInterest.map((modelOfInterest) => {
            if (
              modelOfInterest.modelId !== sensorModelId ||
              hasSensorsFeature
            ) {
              return (
                <TwinTypeTile
                  key={modelOfInterest.modelId}
                  modelOfInterest={modelOfInterest}
                  onClick={() => {
                    // update the value in context
                    changeModelId(modelOfInterest.modelId)
                    // trigger the search function.
                    // When trigging this function, the modelId in context is not updated yet.
                    onSearch({ modelId: modelOfInterest.modelId })
                  }}
                />
              )
            } else {
              return null
            }
          })
        ) : (
          <Progress />
        )}
      </Sections>
    </ExploreContainer>
  )
}
