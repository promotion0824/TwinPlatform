import { useEffect } from 'react'
import Autodesk from 'autodesk' // eslint-disable-line
import * as THREE from 'three'
import { useAutodeskViewer } from '../AutodeskViewerContext'
import ShowColorfulModel from './ShowModel/ShowColorfulModel'

export default function Model({ model }) {
  const autodeskViewer = useAutodeskViewer()

  useEffect(() => {
    const urn = !model.url.startsWith('urn:') ? `urn:${model.url}` : model.url

    let nextModel

    async function handleGeometryLoaded(e) {
      if (nextModel === e.model) {
        try {
          autodeskViewer.viewer.removeEventListener(
            Autodesk.Viewing.GEOMETRY_LOADED_EVENT,
            handleGeometryLoaded
          )

          const db = nextModel.getPropertyDb()
          const dbNodes = await db.executeUserFunction(function userFunction(
            pdb
          ) {
            const dbIds = []
            pdb.enumObjects((dbId) => {
              const props = pdb.getObjectProperties(dbId)
              const prop = props.properties.find(
                (el) => el.displayName === 'GUID'
              )

              if (prop?.displayValue) {
                dbIds.push({
                  dbId: props.dbId,
                  forgeViewerAssetId: prop.displayValue,
                })
              }
            })
            return dbIds
          })

          const fragmentList = nextModel.getFragmentList()

          const nodes = dbNodes.map((node) => {
            const fragIds = []
            nextModel.getInstanceTree().enumNodeFragments(
              node.dbId,
              (fragId) => {
                fragIds.push(fragId)
              },
              true
            )

            const box = new THREE.Box3()
            fragIds.forEach((fragId) => {
              const fragmentBox = new THREE.Box3()
              fragmentList.getWorldBounds(fragId, fragmentBox)
              box.union(fragmentBox)
            })

            return {
              ...node,
              box,
            }
          })

          autodeskViewer.handleModelLoaded(model.url, nextModel, nodes)
        } catch (err) {
          autodeskViewer.viewer.removeEventListener(
            Autodesk.Viewing.GEOMETRY_LOADED_EVENT,
            handleGeometryLoaded
          )

          autodeskViewer.handleModelError(model.url)
        }
      }
    }

    autodeskViewer.viewer.addEventListener(
      Autodesk.Viewing.GEOMETRY_LOADED_EVENT,
      handleGeometryLoaded
    )

    Autodesk.Viewing.Document.load(
      urn,
      async (doc) => {
        try {
          nextModel = await autodeskViewer.viewer.loadDocumentNode(
            doc,
            doc.getRoot().getDefaultGeometry(),
            {
              acmSessionId: doc.acmSessionId,
              applyRefPoint: true,
              globalOffset: autodeskViewer.viewer.model?.getData().globalOffset,
              keepCurrentModels: true,
              preserveView: false,
              // The following setting will ensure edges are displayed for all models
              // reference: https://stackoverflow.com/questions/71087702/how-can-i-use-setdisplayedges-for-multi-model-loading
              createWireframe: true,
            }
          )

          autodeskViewer.viewer.hideModel(nextModel.id)

          autodeskViewer.handleModelLoaded(model.url, nextModel, [])
        } catch (err) {
          autodeskViewer.viewer.removeEventListener(
            Autodesk.Viewing.GEOMETRY_LOADED_EVENT,
            handleGeometryLoaded
          )

          autodeskViewer.handleModelError(model.url)
        }
      },
      () => {
        autodeskViewer.viewer.removeEventListener(
          Autodesk.Viewing.GEOMETRY_LOADED_EVENT,
          handleGeometryLoaded
        )

        autodeskViewer.handleModelError(model.url)
      }
    )

    return () => {
      autodeskViewer.viewer.removeEventListener(
        Autodesk.Viewing.GEOMETRY_LOADED_EVENT,
        handleGeometryLoaded
      )
    }
  }, [])

  return (
    <>
      {model.isVisible && model.model != null && model.nodes != null && (
        <ShowColorfulModel key={model.url} model={model} />
      )}
    </>
  )
}
