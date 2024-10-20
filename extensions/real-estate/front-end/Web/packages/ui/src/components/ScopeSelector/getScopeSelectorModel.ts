import { AvatarProps, IconProps } from '@willowinc/ui'

const scopeSelectorModels: {
  [key: string]: {
    color: AvatarProps['color']
    icon: IconProps['icon']
    includeInCounts: boolean
    name: string
  }
} = {
  'All Locations': {
    color: 'blue',
    icon: 'public',
    includeInCounts: false,
    name: 'headers.allLocations',
  },
  'dtmi:com:willowinc:Building;1': {
    color: 'purple',
    icon: 'apartment',
    includeInCounts: true,
    name: 'adt.building',
  },
  'dtmi:com:willowinc:Land;1': {
    color: 'green',
    icon: 'map',
    includeInCounts: false,
    name: 'adt.land',
  },
  'dtmi:com:willowinc:airport:Airport;1': {
    color: 'green',
    icon: 'map',
    includeInCounts: false,
    name: 'adt.airport',
  },
  'dtmi:com:willowinc:airport:AirportTerminal;1': {
    color: 'purple',
    icon: 'apartment',
    includeInCounts: true,
    name: 'adt.airportTerminal',
  },
  'dtmi:com:willowinc:Portfolio;1': {
    color: 'pink',
    icon: 'business_center',
    includeInCounts: false,
    name: 'headers.portfolio',
  },
  'dtmi:com:willowinc:Region;1': {
    color: 'blue',
    icon: 'public',
    includeInCounts: false,
    name: 'labels.region',
  },
  'dtmi:com:willowinc:Substructure;1': {
    color: 'purple',
    icon: 'domain',
    includeInCounts: true,
    name: 'adt.substructure',
  },
  'dtmi:com:willowinc:OutdoorArea;1': {
    color: 'purple',
    icon: 'nature_people',
    includeInCounts: true,
    name: 'adt.outdoorArea',
  },
}

function getScopeSelectorModel(key: string) {
  const defaultModel = {
    color: 'purple',
    icon: 'apartment',
    includeInCounts: true,
    name: 'plainText.unknown',
  }

  return scopeSelectorModels[key] ?? defaultModel
}

export default getScopeSelectorModel
