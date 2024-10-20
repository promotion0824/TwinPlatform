import { useEffect } from 'react'
import { qs } from '@willow/common'
import { useFloor } from '../../FloorContext'
import DropEquipment from '../DropEquipment/DropEquipment'
import Equipment from '../Equipment/Equipment'
import Zone from '../Zone/Zone'
import Asset from './Asset'

export default function ViewMode() {
  const floor = useFloor()

  useEffect(() => {
    const equipmentId = qs.get('equipmentId')

    if (equipmentId != null) {
      floor.showSelectedAsset()
    }
  }, [])

  const zones = floor.isReadOnly
    ? floor.visibleLayerGroups.flatMap((layerGroup) => layerGroup.zones)
    : floor.layerGroup?.zones ?? []
  let equipments = floor.visibleLayerGroups.flatMap(
    (layerGroup) => layerGroup.equipments
  )
  if (
    floor.selectedAsset != null &&
    !equipments.some(
      (equipment) =>
        equipment.id === floor.selectedAsset.id ||
        equipment.id === floor.selectedAsset.equipmentId
    )
  ) {
    const selectedAsset = floor.layerGroups
      .flatMap((layerGroup) => layerGroup.equipments)
      .find(
        (equipment) =>
          equipment.id === floor.selectedAsset.id ||
          equipment.id === floor.selectedAsset.equipmentId
      )

    if (selectedAsset != null) {
      equipments = [...equipments, selectedAsset]
    }
  }

  return (
    <>
      {zones.map((floorZone) => (
        <Zone key={floorZone.localId} zone={floorZone} />
      ))}
      {equipments.map((equipment) => (
        <Equipment
          key={equipment.localId}
          type={floor.isReadOnly ? 'details' : undefined}
          equipment={equipment}
          clickable={floor.isReadOnly}
          selected={equipment.id === floor.selectedAsset?.id}
          onPointerDown={(e) => {
            const LEFT_BUTTON = 0
            if (e.button === LEFT_BUTTON && floor.isReadOnly) {
              floor.selectAsset({
                id: equipment.id,
                equipmentId: equipment.equipmentId,
                hasLiveData: equipment.hasLiveData,
                isEquipmentOnly: equipment.hasLiveData,
                tags: [],
                pointTags: [],
              })
            }
          }}
        />
      ))}
      {floor.selectedAsset?.geometry?.length === 2 && (
        <Asset key={floor.selectedAsset.id} asset={floor.selectedAsset} />
      )}
      {!floor.isReadOnly && <DropEquipment />}
    </>
  )
}
