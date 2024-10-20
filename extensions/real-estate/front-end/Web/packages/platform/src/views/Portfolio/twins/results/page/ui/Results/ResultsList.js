import _ from 'lodash'
import { styled } from 'twin.macro'
import {
  getModelOfInterest,
  levelModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import { Text, Progress, TwinChip, getUrl } from '@willow/ui'

import {
  documentModelId,
  TwinModelChip,
  TwinLinkListItem,
} from '../../../../shared'
import List from '../../../../shared/ui/List'
import { useSearchResults as InjectedSearchResults } from '../../state/SearchResults'
import FileListItem, {
  ContainmentWrapper,
} from '../../../../shared/ui/FileListItem'
import SiteChip from '../../../../page/ui/SiteChip'

export function getFloorName(twin) {
  return (
    twin.floorName ??
    twin.outRelationships?.find((relationship) => relationship.floorName)
      ?.floorName
  )
}

const ButtonContainer = styled.div({
  margin: 'auto 0 auto auto',
})

const Chips = styled.div({
  margin: '0.5rem 0.5rem 0 0',
  display: 'flex',
  flexWrap: 'wrap',
  gap: '0.5rem',
})

const StyledTwinLinkListItem = styled(TwinLinkListItem)(({ theme }) => ({
  display: 'flex',
  flexWrap: 'nowrap',
  padding: theme.spacing.s16,
}))

const TwinLinkListItemContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexWrap: 'wrap',
  gap: theme.spacing.s8,
}))

export default function ResultsList({
  disableLinks = false,
  endOfPageRef,
  // If we were using TypeScript in all these files, this would be set to undefined by default
  /** An optional button element to be rendered at the end of each row. */
  ResultsButton = ({ twin }) => <></>,
  useSearchResults = InjectedSearchResults,
}) {
  const {
    t,
    ontology,
    modelsOfInterest,
    hasNextPage,
    isLoadingNextPage,
    twins,
    modelId,
    searchType,
  } = useSearchResults()

  const levelModel = modelsOfInterest.find(
    (modelOfInterest) => modelOfInterest.modelId === levelModelId
  )

  return (
    <ContainmentWrapper>
      <List data-testid="scrolling-list">
        {twins.map((twin) =>
          modelId === documentModelId ? (
            <FileListItem
              data-testid="twin-search-result-item"
              key={twin.id}
              fileId={twin.id}
              fileName={twin.name || t('plainText.unnamedTwin')}
              siteId={twin.siteId}
              downloadUrl={getUrl(
                `/api/sites/${twin.siteId}/twins/${twin.id}/download?inline=true`
              )}
            >
              <Chips>
                <RelatedTwins
                  ontology={ontology}
                  modelsOfInterest={modelsOfInterest}
                  relationships={twin.inRelationships}
                />
                <TwinModelChip
                  twin={twin}
                  ontology={ontology}
                  modelsOfInterest={modelsOfInterest}
                />
              </Chips>
            </FileListItem>
          ) : (
            <li key={twin.id} data-testid="result-list">
              <StyledTwinLinkListItem disabled={disableLinks} twin={twin}>
                <TwinLinkListItemContainer>
                  <Text type="h2" css={{ flex: '0 0 100%' }} weight="medium">
                    {twin.name || t('plainText.unnamedTwin')}
                  </Text>
                  <TwinModelChip
                    twin={twin}
                    ontology={ontology}
                    modelsOfInterest={modelsOfInterest}
                  />
                  {searchType === 'sensors' && (
                    <RelatedTwins
                      modelsOfInterest={modelsOfInterest}
                      ontology={ontology}
                      relationships={twin.outRelationships?.filter(
                        (relationship) => relationship.name === 'isCapabilityOf'
                      )}
                    />
                  )}
                  <SiteChip siteName={twin.siteName} />
                  {getFloorName(twin) && (
                    <TwinChip
                      variant="instance"
                      modelOfInterest={levelModel}
                      text={getFloorName(twin)}
                    />
                  )}
                </TwinLinkListItemContainer>

                <ButtonContainer>
                  <ResultsButton twin={twin} />
                </ButtonContainer>
              </StyledTwinLinkListItem>
            </li>
          )
        )}
        <li>
          {isLoadingNextPage ? (
            <Progress />
          ) : hasNextPage ? (
            // to be detected inside an inner div, it needs to have content or content after
            // so put a non-breaking space in here
            <div ref={endOfPageRef}>{'\u00a0'}</div>
          ) : null}
        </li>
      </List>
    </ContainmentWrapper>
  )
}

/**
 * Show the twins from a set of relationships.
 *
 * We group by model ID, sort by the number of twins in each group descending,
 * and then sort by the ordering in modelsOfInterest (the same order used in the
 * explore sidebar) and display a maximum of 5 groups.
 *
 * For each group, if there is more than one twin in the group, we display
 * the model chip with a count of the number of twins matching that model. Otherwise
 * we display the chip for the single twin in the group.
 */
export function RelatedTwins({ relationships, ontology, modelsOfInterest }) {
  const groups = _(relationships)
    .groupBy((r) => r.modelId)
    .values()
    .orderBy((twins) => twins.length, 'desc')
    .orderBy((twins) => {
      const modelOfInterest = getModelOfInterest(
        twins[0].modelId,
        ontology,
        modelsOfInterest
      )
      return modelsOfInterest.indexOf(modelOfInterest)
    })
    .value()
    .slice(0, 5)

  return groups.map((modelTwins) =>
    modelTwins.length > 1 ? (
      <TwinModelChip
        key={modelTwins[0].modelId}
        twin={modelTwins[0]}
        ontology={ontology}
        count={modelTwins.length}
        modelsOfInterest={modelsOfInterest}
      />
    ) : (
      <TwinChip
        key={modelTwins[0].modelId}
        variant="instance"
        modelOfInterest={getModelOfInterest(
          modelTwins[0].modelId,
          ontology,
          modelsOfInterest
        )}
        text={modelTwins[0].twinName}
        ontology={ontology}
      />
    )
  )
}
