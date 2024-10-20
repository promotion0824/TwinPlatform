import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { ALL_LOCATIONS } from '@willow/ui'
import getScopeSelectorModel from '@willow/ui/components/ScopeSelector/getScopeSelectorModel'
import { useTranslation } from 'react-i18next'
import { titleCase } from '@willow/common'
import { Group } from '@willowinc/ui'
import { Tree } from './Tree'
import { useNotificationSettingsContext } from '../NotificationSettingsContext'

const LocationSelector = ({
  locations = [],
  searchText,
}: {
  locations: LocationNode[]
  searchText?: string
}) => {
  const {
    tempSelectedLocationIds,
    onTempNodesChange,
    onTempLocationIdsChange,
  } = useNotificationSettingsContext()

  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <Group css={{ overflowY: 'auto', display: 'block' }} w="100%" h="380px">
      <Tree
        allItemsNode={{
          twin: {
            id: ALL_LOCATIONS,
            name: titleCase({
              language,
              text: t(getScopeSelectorModel(ALL_LOCATIONS).name),
            }),
            siteId: '',
            metadata: {
              modelId: ALL_LOCATIONS,
            },
            userId: '',
          },
        }}
        data={locations}
        onChange={onTempNodesChange}
        onChangeIds={onTempLocationIdsChange}
        selection={tempSelectedLocationIds}
        searchTerm={searchText}
      />
    </Group>
  )
}

export default LocationSelector

export const formatLocations = (locations: LocationNode[]) =>
  locations.map((location) => ({
    ...location,
    id: location.twin.id,
    name: location.twin.name,
    children: formatLocations(location.children ?? []),
  }))
