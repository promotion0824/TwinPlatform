import { useRef } from 'react'
import { useParams } from 'react-router'
import _ from 'lodash'
import { useEffectOnceMounted } from '@willow/common'
import { useApi } from '@willow/ui'
import useDebounce from './useDebounce'

function getFloor(floor) {
  return {
    ...floor,
    layerGroups: floor.layerGroups.map((layerGroup) =>
      _.omit(layerGroup, 'id')
    ),
  }
}

function getLayerGroupData(layerGroup) {
  const tags = _(layerGroup.equipments)
    .flatMap((equipment) => equipment.pointTags)
    .filter((tag) => tag.feature === '2d')
    .uniqBy((tag) => tag.name)
    .value()

  return {
    name: layerGroup.name,
    zones: layerGroup.zones.map((zone) => ({
      id: zone.id,
      geometry: zone.points,
      equipmentIds: [],
    })),
    equipments: layerGroup.equipments.map((equipment) => ({
      id: equipment.id,
      name: equipment.name,
      geometry: equipment.points,
      tags: equipment.pointTags.map((tag) => tag.id),
    })),
    layers: tags.map((tag) => ({
      name: tag.name,
      tagName: tag.name,
    })),
  }
}

export default function useSaveFloor({
  floor,
  updateLayerGroup,
  onSaving,
  onSaved,
  onError,
}) {
  const api = useApi()
  const params = useParams()

  const lastSavedFloorRef = useRef(_.cloneDeep(floor))

  async function updateFloorName(name) {
    await api.put(`/api/sites/${params.siteId}/floors/${floor.floorId}`, {
      name,
    })
  }

  async function deleteImages(images) {
    for (let i = 0; i < images.length; i++) {
      const image = images[i]

      // eslint-disable-next-line
      await api.delete(
        `/api/sites/${params.siteId}/floors/${floor.floorId}/module/${image.id}`
      )
    }
  }

  async function addLayerGroups(layerGroups) {
    for (let i = 0; i < layerGroups.length; i++) {
      const layerGroup = layerGroups[i]

      // eslint-disable-next-line
      const response = await api.post(
        `/api/sites/${params.siteId}/floors/${floor.floorId}/layerGroups`,
        getLayerGroupData(layerGroup)
      )

      lastSavedFloorRef.current = updateLayerGroup(
        lastSavedFloorRef.current,
        layerGroup,
        response
      )
    }
  }

  async function editLayerGroups(layerGroups) {
    for (let i = 0; i < layerGroups.length; i++) {
      const layerGroup = layerGroups[i]

      // eslint-disable-next-line
      const response = await api.put(
        `/api/sites/${params.siteId}/floors/${floor.floorId}/layerGroups/${layerGroup.id}`,
        getLayerGroupData(layerGroup)
      )

      lastSavedFloorRef.current = updateLayerGroup(
        lastSavedFloorRef.current,
        layerGroup,
        response
      )
    }
  }

  async function deleteLayerGroups(layerGroups) {
    for (let i = 0; i < layerGroups.length; i++) {
      const layerGroup = layerGroups[i]

      // eslint-disable-next-line
      await api.delete(
        `/api/sites/${params.siteId}/floors/${floor.floorId}/layerGroups/${layerGroup.id}`
      )
    }
  }

  async function saveFloorLayerGroup(layerGroup) {
    await api.put(
      `/api/sites/${params.siteId}/floors/${floor.floorId}/geometry`,
      {
        geometry: JSON.stringify(layerGroup.zones.map((zone) => zone.points)),
      }
    )
  }

  const save = useDebounce(async () => {
    try {
      if (_.isEqual(getFloor(lastSavedFloorRef.current), getFloor(floor))) {
        return
      }

      const imageIds = floor.modules2D.map((image) => image.id)
      const imagesToDelete = lastSavedFloorRef.current.modules2D.filter(
        (image) => !imageIds.includes(image.id)
      )

      const layerGroupsToAdd = floor.layerGroups
        .filter((layerGroup) => layerGroup.id !== 'floor_layer')
        .filter((layerGroup) => layerGroup.id == null)

      const layerGroupsToEdit = floor.layerGroups
        .filter((layerGroup) => layerGroup.id !== 'floor_layer')
        .filter((layerGroup) => {
          const savedLayerGroup = lastSavedFloorRef.current.layerGroups.find(
            (lastSavedLayerGroup) => lastSavedLayerGroup.id === layerGroup.id
          )

          return (
            savedLayerGroup != null && !_.isEqual(layerGroup, savedLayerGroup)
          )
        })

      const layerGroupIds = floor.layerGroups
        .filter((layerGroup) => layerGroup.id !== 'floor_layer')
        .map((layerGroup) => layerGroup.id)

      const layerGroupsToDelete = lastSavedFloorRef.current.layerGroups
        .filter((layerGroup) => layerGroup.id !== 'floor_layer')
        .filter((layerGroup) => !layerGroupIds.includes(layerGroup.id))

      const hasFloorChanged =
        floor.floorName !== lastSavedFloorRef.current.floorName
      const floorLayerGroup = floor.layerGroups.find(
        (layerGroup) => layerGroup.id === 'floor_layer'
      )
      const oldFloorLayerGroup = lastSavedFloorRef.current.layerGroups.find(
        (layerGroup) => layerGroup.id === 'floor_layer'
      )
      const hasFloorLayerGroupChanged =
        oldFloorLayerGroup.hasLoaded &&
        !_.isEqual(floorLayerGroup, oldFloorLayerGroup)

      lastSavedFloorRef.current = _.cloneDeep(floor)

      const shouldSave =
        hasFloorChanged ||
        imagesToDelete.length > 0 ||
        layerGroupsToAdd.length > 0 ||
        layerGroupsToEdit.length > 0 ||
        layerGroupsToDelete.length > 0 ||
        hasFloorLayerGroupChanged

      if (shouldSave) {
        onSaving()

        if (hasFloorChanged) {
          await updateFloorName(floor.floorName)
        }
        await deleteImages(imagesToDelete)
        await addLayerGroups(layerGroupsToAdd)
        await editLayerGroups(layerGroupsToEdit)
        await deleteLayerGroups(layerGroupsToDelete)
        await saveFloorLayerGroup(floorLayerGroup)

        onSaved()
      }
    } catch (err) {
      onError()
    }
  }, 1000)

  useEffectOnceMounted(() => {
    save()
  }, [floor])
}
