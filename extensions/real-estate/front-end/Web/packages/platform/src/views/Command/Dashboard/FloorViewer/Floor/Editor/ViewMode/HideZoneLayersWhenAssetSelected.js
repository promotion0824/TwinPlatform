import { useEffect } from 'react'
import { useFloor } from '../../FloorContext'

export default function HideZoneLayersWhenSelected({ asset }) {
  const floor = useFloor()

  useEffect(() => {
    if (asset.geometry?.length === 2) {
      floor.hideLayerGroups()
    }
  }, [])

  return null
}
