import { titleCase } from '@willow/common'
import { Button, Icon } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'

import {
  formatSensorForTimeSeries,
  getRelatedTwinFromSensorForTimeSeries,
} from '../../Portfolio/twins/results/page/ui/Results/ResultsTable'

import { useTimeSeries } from '../../../components/TimeSeries/TimeSeriesContext'
import { useSearchResults } from '../../Portfolio/twins/results/page/state/SearchResults'

type AssetAndSensor = {
  assetId: string
  sensorId: string
}

type Relationship = {
  modelId: string
  name: string
  sourceId: string
  targetId: string
  twinName: string
}

export type Twin = {
  externalId: string
  id: string
  inRelationships: Relationship[]
  modelId: string
  name: string
  outRelationships: Relationship[]
  rawTwin: string
  siteId: string
  siteName: string
  uniqueId: string
}

const StyledButton = styled(Button)({
  width: '85px',
})

export default function TimeSeriesSearchResultsAddRemoveButton({
  twin,
}: {
  twin: Twin
}) {
  const { modelsOfInterest, ontology, searchType } = useSearchResults()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const {
    assets,
    addOrRemoveAsset,
    addOrRemoveSensor,
    loadingSitePointIds,
    points,
  } = useTimeSeries()

  function hasSensors() {
    return twin.inRelationships?.some(
      (relationship) =>
        relationship.name === 'hostedBy' ||
        relationship.name === 'isCapabilityOf'
    )
  }

  const [isLoadingLocally, setIsLoadingLocally] = useState(false)

  const isAsset = searchType === 'twins'
  const selectedAssets = assets.map((asset) => asset.siteAssetId)
  const selectedPoints = points.map((point) => point.sitePointId)

  const timeSeriesId = isAsset
    ? `${twin.siteId}_${twin.uniqueId}`
    : // TODO: It's still being determined whether trendId should be a top level property or not.
      `${twin.siteId}_${JSON.parse(twin.rawTwin).customProperties.trendID}`

  const isLoading =
    isLoadingLocally || loadingSitePointIds.includes(timeSeriesId)

  const selected =
    !isLoading &&
    (isAsset
      ? selectedAssets.includes(timeSeriesId)
      : selectedPoints.includes(timeSeriesId))

  const isDisabled = isAsset
    ? !hasSensors()
    : getRelatedTwinFromSensorForTimeSeries(
        twin,
        ontology,
        modelsOfInterest
      ) === undefined

  const tooltip = isLoading
    ? `${t('plainText.loading')}...`
    : isDisabled
    ? isAsset
      ? t('plainText.noSensorsAvailable')
      : t('plainText.noRelatedTwinsAvailable')
    : null

  async function addOrRemove() {
    if (isAsset) {
      addOrRemoveAsset(timeSeriesId)
    } else {
      setIsLoadingLocally(true)

      const { assetId, sensorId } = (await formatSensorForTimeSeries({
        modelsOfInterest,
        ontology,
        sensor: twin,
      })) as AssetAndSensor

      addOrRemoveSensor(assetId, sensorId)

      // isLoadingLocally is used because the loadingSitePointIds takes a bit to kick in.
      setTimeout(() => setIsLoadingLocally(false), 2000)
    }
  }

  return (
    <div
      data-tooltip={tooltip}
      data-tooltip-position="top"
      data-tooltip-z-index="203"
    >
      <StyledButton
        disabled={isDisabled || isLoading}
        kind={!selected ? 'primary' : 'secondary'}
        onClick={addOrRemove}
        prefix={<Icon icon={!selected ? 'add' : 'remove'} />}
      >
        {!selected
          ? titleCase({ text: t('plainText.add'), language })
          : titleCase({ text: t('plainText.remove'), language })}
      </StyledButton>
    </div>
  )
}
