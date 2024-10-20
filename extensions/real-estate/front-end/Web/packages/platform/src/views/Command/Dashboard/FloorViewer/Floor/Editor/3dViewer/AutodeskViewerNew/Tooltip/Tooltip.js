import { qs, useLatest } from '@willow/common'
import { api, useSize } from '@willow/ui'
import { Group, Loader } from '@willowinc/ui'
import Autodesk from 'autodesk' // eslint-disable-line
import { useLayoutEffect, useRef, useState } from 'react'
import { useQuery } from 'react-query'
import { css } from 'styled-components'
import * as THREE from 'three'
import { useAutodeskViewer } from '../AutodeskViewerContext'

export default function Tooltip() {
  const autodeskViewer = useAutodeskViewer()

  const tooltipRef = useRef()
  const [style, setStyle] = useState()
  const [siteId] = useState(() => qs.get('siteId'))

  const size = useSize(tooltipRef)

  const forgeViewerAssetQuery = useQuery(
    ['forgeViewerAsset', autodeskViewer.selectedAsset.forgeViewerAssetId],
    async () => {
      const response = await api.get(
        `/sites/${siteId}/assets/byforgeviewermodelid/${autodeskViewer?.selectedAsset?.forgeViewerAssetId}`
      )

      return response.data
    },
    {
      enabled:
        siteId != null &&
        autodeskViewer?.selectedAsset?.forgeViewerAssetId != null,
    }
  )

  const box = autodeskViewer.models
    .flatMap((model) => model.nodes ?? [])
    .find(
      (node) =>
        node.forgeViewerAssetId ===
        autodeskViewer.selectedAsset.forgeViewerAssetId
    )?.box
  const position = box?.getCenter(new THREE.Vector3())
  position?.setZ(box.max.z)

  const refresh = useLatest(() => {
    if (position == null) {
      return
    }

    const { x, y } = autodeskViewer.viewer.worldToClient(position)

    setStyle({
      left: x,
      top: y - tooltipRef.current.offsetHeight,
    })
  })

  useLayoutEffect(() => {
    autodeskViewer.viewer.addEventListener(
      Autodesk.Viewing.CAMERA_CHANGE_EVENT,
      refresh
    )
    autodeskViewer.viewer.addEventListener(
      Autodesk.Viewing.ISOLATE_EVENT,
      refresh
    )
    autodeskViewer.viewer.addEventListener(Autodesk.Viewing.HIDE_EVENT, refresh)
    autodeskViewer.viewer.addEventListener(Autodesk.Viewing.SHOW_EVENT, refresh)
    refresh()

    return () => {
      autodeskViewer.viewer.removeEventListener(
        Autodesk.Viewing.CAMERA_CHANGE_EVENT,
        refresh
      )
      autodeskViewer.viewer.removeEventListener(
        Autodesk.Viewing.ISOLATE_EVENT,
        refresh
      )
      autodeskViewer.viewer.removeEventListener(
        Autodesk.Viewing.HIDE_EVENT,
        refresh
      )
      autodeskViewer.viewer.removeEventListener(
        Autodesk.Viewing.SHOW_EVENT,
        refresh
      )
    }
  }, [size])

  if (position == null || siteId == null) {
    return null
  }

  return (
    <div
      css={css(({ theme }) => ({
        background: theme.color.neutral.bg.base.default,
        border: `1px solid ${theme.color.neutral.border.default}`,
        borderRadius: theme.spacing.s4,
        color: theme.color.neutral.fg.default,
        position: 'absolute',
        whiteSpace: 'nowrap',
        zIndex: theme.zIndex.overlay,
        // Move the tooltip up a bit as we will be displaying another tooltip below this one.
        transform: 'translate(-50%, -75%)',
        ...theme.font.heading.sm,
      }))}
      ref={tooltipRef}
      style={style}
    >
      <Group p="s6">
        {forgeViewerAssetQuery.isLoading ? (
          <Loader size="sm" intent="secondary" />
        ) : (
          forgeViewerAssetQuery.data?.name && (
            <>{forgeViewerAssetQuery.data.name}</>
          )
        )}
      </Group>
    </div>
  )
}
