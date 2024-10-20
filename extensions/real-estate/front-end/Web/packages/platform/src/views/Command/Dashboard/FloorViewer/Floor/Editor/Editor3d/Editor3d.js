import { ResizeObserverContainer, useLatest } from '@willow/common'
import { useAnalytics, useApi, useSnackbar } from '@willow/ui'
import { useCallback, useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { useFloor } from '../../FloorContext'
import styles from './Editor3d.css'

export default function Editor3d() {
  const analytics = useAnalytics()
  const api = useApi()
  const floor = useFloor()
  const params = useParams()
  const snackbar = useSnackbar()
  const { t } = useTranslation()
  useEffect(() => {
    analytics.page('Dashboard Floor 3D')
  }, [])

  const handleSelectingAsset = useLatest(() => {
    floor.selectAsset()
  })

  const handleSelectAsset = useLatest(async (forgeViewerModelId) => {
    if (forgeViewerModelId == null) {
      floor.selectAsset()
      floor.iframeRef.current?.contentWindow?.selectAssetError?.()

      snackbar.show(t('plainText.noAssetMapping'), {
        isToast: true,
        closeButtonLabel: t('plainText.dismiss'),
      })
      return
    }

    /*
    3dviewer is a separate app rendered inside willow app,
    i18n does save language in localStorage with key of i18nextLng, reference:
    https://github.com/i18next/i18next-browser-languageDetector#introduction
    */
    try {
      const response = await api.get(
        `/api/sites/${params.siteId}/assets/byforgeviewermodelid/${forgeViewerModelId}`,
        { headers: { language: localStorage.getItem('i18nextLng') || 'en' } }
      )

      analytics.track('Viewing Three Dimensional Asset', response)

      const moduleTypeNamePath = response.moduleTypeNamePath.includes('|')
        ? response.moduleTypeNamePath.split('|')
        : response.moduleTypeNamePath.split(',')
      floor.selectAsset({
        id: response.id,
        equipmentId: response.equipmentId,
        hasLiveData: response.hasLiveData,
        forgeViewerModelId,
        isEquipmentOnly: true,
        geometry: [],
        moduleTypeNamePath,
        twinId: response.twinId,
      })

      const layerId = floor.getMain3dLayerForModuleTypeName(
        response.moduleTypeNamePath
      )?.id

      const asset =
        response.id != null && forgeViewerModelId != null
          ? {
              assetId: response.id,
              forgeViewerAssetId: forgeViewerModelId?.toLowerCase?.(),
            }
          : undefined
      floor.iframeRef.current?.contentWindow?.selectForgeViewerAsset?.(asset)

      if (floor.selectedAsset?.id !== response.id) {
        if (layerId != null && !floor.selectedLayerIds.includes(layerId)) {
          floor.setSelectedLayerIds((prevSelectedLayerIds) => [
            ...prevSelectedLayerIds,
            layerId,
          ])
        }
      }
    } catch (err) {
      if (err.status === 404) {
        // Not all model features have mapped twins, this is fine.
        snackbar.show(t('plainText.noAssetMapping'), {
          isToast: true,
          closeButtonLabel: t('plainText.dismiss'),
        })
      } else {
        console.error(err)
      }
    }
  })

  useEffect(() => {
    window.selectingAssetFromViewer = () => handleSelectingAsset()
    window.selectAssetFromViewer = (guid) => handleSelectAsset(guid)
  }, [])

  useEffect(() => {
    const handleMessage = ({ data }) => {
      if (data.type === 'viewerInitialized') {
        floor.setViewerLoaded(true)
      }
    }
    window.addEventListener('message', handleMessage)

    if (floor?.iframeRef?.current?.contentWindow && floor.viewerLoaded) {
      // pass insight stats to 3d viewer iframe
      if (floor.statsQuery?.data) {
        floor.iframeRef.current.contentWindow.postMessage(
          {
            type: 'dataStatistics',
            data: floor.statsQuery.data,
            isInsightStatsOn: floor.isInsightStatsLayerOn,
            isTicketStatsOn: floor.isTicketStatsLayerOn,
          },
          window.location.origin
        )
      }
    }

    return () => {
      window.removeEventListener('message', handleMessage)
    }
  }, [
    floor.iframeRef,
    floor.statsQuery.data,
    floor.isInsightStatsLayerOn,
    floor.isTicketStatsLayerOn,
    floor.viewerLoaded,
  ])

  const timeoutRef = useRef()
  const handleWidthChange = useCallback((w) => {
    const iframe = floor?.iframeRef?.current
    if (iframe && w !== iframe?.width) {
      clearTimeout(timeoutRef.current)
      // Prevent pointer events on the iframe while resizing to prevent iframe to
      // capture mouse events and mess up resizing
      iframe.style.pointerEvents = 'none'
      iframe.style.width = `${w}px`

      // Set a timeout to reset the resizing state and re-enable pointer events
      timeoutRef.current = setTimeout(() => {
        iframe.style.pointerEvents = 'auto'
      }, 1000)
    }

    return () => {
      clearTimeout(timeoutRef.current)
      if (iframe) {
        iframe.style.pointerEvents = 'auto'
      }
    }
  }, [])

  return (
    // All the styles applied to various elements
    // below is to ensure the 3d viewer iframe takes up the entire space
    // and no scrollbars are shown
    <ResizeObserverContainer
      css={`
        height: 99%;
        position: relative;
      `}
      onContainerWidthChange={handleWidthChange}
    >
      <iframe
        css={`
          height: 100%;
          overflow: hidden;
        `}
        ref={floor.iframeRef}
        title="3d"
        src={`/public/3d.html?siteId=${params.siteId}`}
        className={styles.iframe}
        onLoad={() => {
          const iframe = floor.iframeRef.current
          if (iframe != null) {
            iframe.contentWindow.document.body.style.height = '100%'
            iframe.contentWindow.document.body.style.width = '100%'
            iframe.contentWindow.document.body.style.overflow = 'hidden'
          }
        }}
      />
    </ResizeObserverContainer>
  )
}
