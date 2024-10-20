import tw from 'twin.macro'
import { Marker } from '@willow/ui'
import { Ontology } from '@willow/common/twins/view/models'
import {
  ModelOfInterest,
  getModelOfInterest,
  buildingModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import {
  MapViewItem,
  isMapViewTwin,
  passengerBoardingBridgesModelId,
} from './types'
import LoadingIcon from '../SiteIcon/SiteIcon'
import MapViewPopup from './MapViewPopup'
import {
  MapViewIconWithCount,
  colorMap,
  twinWithInsightColor,
} from './icons/MapViewIcon'

export default function MapViewMarker({
  size = 'large',
  icon = 'site',
  isLoading = false,
  feature,
  isSelected = false,
  onClick,
  count = 0,
  ontology,
  modelsOfInterest,
  colorKey,
  headerChip,
}: {
  size?: 'small' | 'medium' | 'large'
  icon?: string
  isLoading?: boolean
  feature: {
    properties: MapViewItem
    geometry: { coordinates: [number, number] }
  }
  isSelected?: boolean
  onClick?: (item?: MapViewItem) => void
  count?: number
  ontology?: Ontology
  modelsOfInterest?: ModelOfInterest[]
  colorKey?: string
  headerChip?: React.ReactNode
}) {
  const { properties } = feature
  const modelOfInterest =
    ontology &&
    modelsOfInterest &&
    getModelOfInterest(
      isMapViewTwin(properties)
        ? passengerBoardingBridgesModelId
        : buildingModelId,
      ontology,
      modelsOfInterest
    )

  const derivedColors = (colorKey && colorMap[colorKey]) || twinWithInsightColor

  return (
    <Marker
      feature={feature}
      isSelected={isSelected}
      popup={
        <MapViewPopup
          name={properties.name}
          insights={properties.insights}
          item={properties}
          modelOfInterest={modelOfInterest}
          headerChip={headerChip}
        />
      }
      onClick={() => {
        onClick?.(!isSelected ? feature.properties : undefined)
      }}
      closeButtonOnPopup
    >
      {isLoading ? (
        <LoadingIcon
          size={size}
          value={0}
          color="dark"
          selected={false}
          icon={icon}
          isLoading
        />
      ) : (
        <MapViewIconWithCount
          icon={icon}
          size={size}
          color={derivedColors.color}
          fill={derivedColors.color}
          backgroundColor={derivedColors.background}
          count={count}
        />
      )}
    </Marker>
  )
}
