import { useEffect, useRef, useState } from 'react'
import cx from 'classnames'
import Autodesk from 'autodesk' // eslint-disable-line
import { useUserAgent } from '@willow/ui'
import styles from './Viewer.css'

export default function Viewer({ token, children }) {
  const userAgent = useUserAgent()

  const viewerRef = useRef()
  const [state, setState] = useState({
    viewer: undefined,
  })

  useEffect(() => {
    Autodesk.Viewing.Initializer(
      {
        getAccessToken(onSuccess) {
          onSuccess(token.access_token)
        },
      },
      () => {
        const viewer = new Autodesk.Viewing.Private.GuiViewer3D(
          viewerRef.current
        )

        function initialize() {
          viewer.impl.renderer().setAOOptions(30.0, 0.8)
          viewer.impl.setOptimizeNavigation(true)
          viewer.setProgressiveRendering(false)
          viewer.setQualityLevel(false, true)
          viewer.prefs.tag('ignore-producer')
          viewer.setGroundShadow(false)
          viewer.hidePoints(true)
          viewer.hideLines(true)
          viewer.setLightPreset(4)
          viewer.setQualityLevel(false, false)
          viewer.setGhosting(true)
          viewer.setGroundShadow(true)
          viewer.setGroundReflection(false)
          viewer.setEnvMapBackground(false)
          viewer.setProgressiveRendering(true)
          viewer.setBackgroundColor(43, 43, 43, 43, 43, 43)
          viewer.navigation.toPerspective()
          viewer.navigation.setReverseZoomDirection(true)
          viewer.autocam.setHomeViewFrom(viewer.navigation.getCamera())
          viewer.setSelectionMode(Autodesk.Viewing.SelectionMode.LAST_OBJECT)
        }

        viewer.addEventListener(Autodesk.Viewing.VIEWER_INITIALIZED, initialize)
        viewer.addEventListener(
          Autodesk.Viewing.MODEL_ROOT_LOADED_EVENT,
          initialize
        )

        viewer.start()

        setState({ viewer })

        // Attempt to ensure edges are displayed, we had received feedback
        // that edges are not turned on IPad
        viewer.setDisplayEdges(true)

        // Post message to parent window to notify that viewer is initialized
        window.parent.postMessage(
          {
            type: 'viewerInitialized',
            data: true,
          },
          window.location.origin
        )
      }
    )
  }, [])

  const cxClassName = cx({
    [styles.hideFullscreen]: userAgent.isIpad,
  })

  return (
    <div ref={viewerRef} className={cxClassName}>
      {state.viewer != null && children(state.viewer)}
    </div>
  )
}
