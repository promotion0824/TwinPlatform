import { titleCase } from '@willow/common'
import { Progress, useAnalytics, useFeatureFlag, Viewer3D } from '@willow/ui'
import { ViewerControls } from '@willow/ui/components/Viewer3D/types'
import { Box, Button, Group, useSnackbar } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import useAutoDeskFileManifest from '../../../../../hooks/ThreeDimensionModule/useAutoDeskFileManifest/useAutoDeskFileManifest'
import useAutoDeskToken from '../../../../../hooks/ThreeDimensionModule/useAutoDeskToken/useAutoDeskToken'
import useGet3dModule from '../../../../../hooks/ThreeDimensionModule/useGet3dModule/useGet3dModule'
import { useDashboard } from '../../../Dashboard/DashboardContext'
import Floors from '../../Floors/Floors'

const DEFAULT_URN_INDEX = 0

const Subheading = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  marginLeft: theme.spacing.s4,
}))

const ThreeDViewModel = (props) => {
  const analytics = useAnalytics()
  const featureFlags = useFeatureFlag()
  const { site, floors } = useDashboard()
  const snackbar = useSnackbar()
  const history = useHistory()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const layers = getLayers(floors)

  const handleErrorLoading3DModel = () => {
    snackbar.show({
      title: t('plainText.failedToLoad3D'),
      intent: 'negative',
    })
  }

  const { data: autoDeskData, isLoading: isTokenLoading } = useAutoDeskToken({
    onError: handleErrorLoading3DModel,
  })
  const { data: moduleData, isLoading: isModuleLoading } = useGet3dModule(
    site.id,
    {
      enabled: !!site.id,
      staleTime: 0,
      onError: handleErrorLoading3DModel,
    }
  )
  const isModelExist = !!moduleData?.url && !!autoDeskData?.access_token

  const { data: autoDeskManifest, isLoading: isFileManifestLoading } =
    useAutoDeskFileManifest(
      {
        urn: moduleData?.url,
        accessToken: autoDeskData?.access_token,
        tokenType: autoDeskData?.token_type,
      },
      {
        enabled: isModelExist,
        onError: handleErrorLoading3DModel,
      }
    )
  const isModelLoading =
    isTokenLoading || isModuleLoading || isFileManifestLoading

  const is3dModelReady =
    isModelExist &&
    autoDeskManifest &&
    autoDeskManifest?.progress === 'complete'

  const [viewerControls, setViewerControls] = useState<ViewerControls>()

  const handleViewerClick = ({ guids }) => {
    if (guids) {
      const [modelReference] = guids
      const displayError = () =>
        snackbar.show({
          title: t('plainText.selectedFloorPageNotFound'),
          intent: 'negative',
        })
      if (modelReference) {
        const clickedFloor = floors.find(
          (floor) => floor.modelReference === modelReference
        )
        if (!clickedFloor) {
          displayError()
        } else {
          handleFloorClick(clickedFloor)
        }
      } else {
        displayError()
      }
    }
  }

  const handleInitControls = (controls) => {
    setViewerControls(controls)
  }

  const handleFloorClick = (floor: any) => {
    if (floor.id) {
      const { id: siteId } = site
      const { id: floorId } = floor
      analytics.track('Floor select from building', {
        siteId,
        floorId,
      })
      history.push(`/sites/${siteId}/floors/${floorId}`)
    } else {
      snackbar.show({
        title: t('plainText.floorIdDoesNotExist'),
        intent: 'negative',
      })
    }
  }
  const handleResetButtonClick = () => {
    if (viewerControls) {
      viewerControls.reset()
    }
  }

  return (
    /* by adding key, it will ensure the updated 3d model is shown when site id changes */
    <div
      key={site.id}
      css={{
        width: '100%',
      }}
      {...props}
    >
      {isModelLoading ? (
        <Progress />
      ) : is3dModelReady ? (
        <Viewer3DWrapper
          urns={[moduleData.url]}
          defaultDisplayUrnIndices={[DEFAULT_URN_INDEX]}
          token={autoDeskData.access_token}
          layers={[layers]}
          onClick={handleViewerClick}
          onInit={handleInitControls}
        >
          <Box pos="absolute" w="100%">
            <Group justify="space-between" m="s8">
              <Subheading>
                {featureFlags.hasFeatureToggle(
                  'buildingHomeDragAndDropRedesign'
                )
                  ? titleCase({ language, text: t('headers.activeInsights') })
                  : ''}
              </Subheading>
              <Button kind="secondary" onClick={handleResetButtonClick}>
                {titleCase({ language, text: t('plainText.resetView') })}
              </Button>
            </Group>
          </Box>
        </Viewer3DWrapper>
      ) : (
        <Floors floors={floors} />
      )}
    </div>
  )
}

function getLayers(floors) {
  const layers = {}
  const combineModelReferenceAndLayer = (floor) => {
    const { modelReference, insightsHighestPriority, code, name } = floor
    layers[modelReference] = {
      name,
      floorCode: code,
      priority: insightsHighestPriority,
    }
  }

  floors
    .filter((floor) => floor.modelReference)
    .forEach(combineModelReferenceAndLayer)

  return layers
}

const Viewer3DWrapper = styled(Viewer3D)({
  width: '100%',
  height: '100%',
  position: 'relative',
})

export default ThreeDViewModel
