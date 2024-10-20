import Autodesk from 'autodesk' // eslint-disable-line
import Viewer from './Viewer/Viewer'
import AutodeskViewerProvider from './AutodeskViewerProvider'
import Model from './Model/Model'
import Tooltip from './Tooltip/Tooltip'

export default function AutodeskViewer({ token }) {
  return (
    <Viewer token={token}>
      {(viewer) => (
        <AutodeskViewerProvider viewer={viewer}>
          {(autodeskViewer) => (
            <>
              {autodeskViewer.models.map((model) => (
                <Model key={model.url} model={model} />
              ))}
              {autodeskViewer.selectedAsset != null && <Tooltip />}
            </>
          )}
        </AutodeskViewerProvider>
      )}
    </Viewer>
  )
}
