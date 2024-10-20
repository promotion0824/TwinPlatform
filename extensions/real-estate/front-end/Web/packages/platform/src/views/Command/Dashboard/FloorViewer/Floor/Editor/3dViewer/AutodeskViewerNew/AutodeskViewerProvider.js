import _ from 'lodash'
import { useEffect, useRef, useState } from 'react'
import Autodesk from 'autodesk' // eslint-disable-line
import { useLatest } from '@willow/common'
import getGuid from '@willow/ui/utils/getGuid'
import { AutodeskViewerContext } from './AutodeskViewerContext'

export default function AutodeskViewerProvider({ viewer, children }) {
  const selectedOutsideForgeViewerAssetIdRef = useRef()
  const [models, setModels] = useState([])
  const [selectedAsset, setSelectedAsset] = useState()
  const [dataStats, setDataStats] = useState()
  const [isInsightStatsOn, setIsInsightStatsOn] = useState(false)
  const [isTicketStatsOn, setIsTicketStatsOn] = useState(false)

  // initialise model object the result models will be passed into Models component
  function handleLoadModel(url, name) {
    setModels((prevModels) => {
      if (!prevModels.some((prevModel) => prevModel.url === url)) {
        const nextModel = {
          url,
          name,
          isVisible: true,
          model: undefined,
          nodes: undefined,
          hasBeenShown: false,
        }

        return [...prevModels, nextModel]
      }

      return prevModels.map((prevModel) =>
        prevModel.url === url
          ? {
              ...prevModel,
              isVisible: true,
            }
          : prevModel
      )
    })
  }

  function handleHideModel(url) {
    setSelectedAsset()
    setModels((prevModels) =>
      prevModels.map((prevModel) => ({
        ...prevModel,
        isVisible: prevModel.url === url ? false : prevModel.isVisible,
        hasBeenShown: false,
      }))
    )
  }

  // This method is called in Models.js, after model is loaded forgeviewerId is exctracted from dbID and passed into model object
  function handleModelLoaded(url, model, nodes) {
    const dbId = nodes.find(
      (node) => node.forgeViewerAssetId === selectedAsset?.forgeViewerAssetId
    )?.dbId

    if (dbId != null) {
      viewer.fitToView([dbId], model)
      viewer.select([dbId], model)
    }

    setModels((prevModels) =>
      prevModels.map((prevModel) =>
        prevModel.url === url
          ? {
              ...prevModel,
              model,
              nodes,
            }
          : prevModel
      )
    )
  }

  function handleModelError(url) {
    setModels((prevModels) =>
      prevModels.filter((prevModel) => prevModel.url !== url)
    )

    window.frameElement.ownerDocument.defaultView.deselectLayer?.(url)
  }

  function handleShowModel(model) {
    const dbId = model.nodes.find(
      (node) => node.forgeViewerAssetId === selectedAsset?.forgeViewerAssetId
    )?.dbId
    if (dbId != null) {
      if (!model.hasBeenShown) {
        viewer.fitToView([dbId], model.model)
      }
      viewer.select([dbId], model.model)
    }

    setModels((prevModels) =>
      prevModels.map((prevModel) =>
        prevModel.url === model.url
          ? {
              ...prevModel,
              hasBeenShown: true,
            }
          : prevModel
      )
    )
  }

  function selectAsset(asset, fitToView = true) {
    selectedOutsideForgeViewerAssetIdRef.current = asset?.forgeViewerAssetId
    setSelectedAsset(asset)

    viewer.clearSelection()

    const selectedModels = models
      .filter((model) => model.nodes != null)
      .map((model) => ({
        ...model,
        dbId: model.nodes.find(
          (node) => node.forgeViewerAssetId === asset?.forgeViewerAssetId
        )?.dbId,
      }))
      .filter((model) => model.dbId != null)

    selectedModels.forEach((selectedModel) => {
      if (selectedModel.isVisible) {
        selectedOutsideForgeViewerAssetIdRef.current = asset?.forgeViewerAssetId
        if (fitToView) {
          viewer.fitToView([selectedModel.dbId], selectedModel.model)
        }
        viewer.select([selectedModel.dbId], selectedModel.model)
      }
    })
  }

  const handleSelectAsset = useLatest((asset) => selectAsset(asset, true))
  const handleSelectForgeViewerAsset = useLatest((asset) => selectAsset(asset))
  const handleSelectAssetError = useLatest(() => viewer.clearSelection())

  const handleClickAsset = useLatest(async (e) => {
    if (selectedOutsideForgeViewerAssetIdRef.current != null) {
      selectedOutsideForgeViewerAssetIdRef.current = undefined
      return
    }

    setSelectedAsset()

    const selection = e.selections[0]
    if (selection != null) {
      window.parent.document.defaultView.selectingAssetFromViewer?.()

      const { guid } = await getGuid(selection.model, selection.dbIdArray[0])

      window.parent.document.defaultView.selectAssetFromViewer?.(guid)
    }
  })

  useEffect(() => {
    window.loadModel = (url, name) => handleLoadModel(url, name)
    window.hideModel = (url) => handleHideModel(url)
    window.selectAsset = (asset) => handleSelectAsset(asset)
    window.selectForgeViewerAsset = (asset) =>
      handleSelectForgeViewerAsset(asset)
    window.selectAssetError = () => handleSelectAssetError()

    viewer.addEventListener(
      Autodesk.Viewing.AGGREGATE_SELECTION_CHANGED_EVENT,
      (e) => {
        handleClickAsset(e)
      }
    )

    window.parent.document.defaultView.onViewerLoaded()

    const layers = window.parent.document.defaultView.getSelectedLayers()
    layers.forEach((layer) => handleLoadModel(layer.url, layer.name))
  }, [])

  useEffect(() => {
    const handleMessage = ({ data }) => {
      if (data.type === 'dataStatistics') {
        // Convert array to object with geometryViewerId as key
        // so access to stats is an O(1) operation as opposed to O(n)
        // with array.find()
        setDataStats(_.keyBy(data.data, 'geometryViewerId'))
        setIsInsightStatsOn(data.isInsightStatsOn)
        setIsTicketStatsOn(data.isTicketStatsOn)
      }
    }
    window.addEventListener('message', handleMessage)
    return () => {
      window.removeEventListener('message', handleMessage)
    }
  }, [])

  const context = {
    viewer,
    selectedAsset,

    models: models
      .map((model, i) => ({
        ...model,
        prevModel: models[i - 1],
      }))
      .filter(
        (model) => model.prevModel == null || model.prevModel.model != null
      ),

    handleModelLoaded,
    handleModelError,
    handleShowModel,
    dataStats,
    isInsightStatsOn,
    isTicketStatsOn,
    selectAsset,
  }

  return (
    <AutodeskViewerContext.Provider value={context}>
      {children(context)}
    </AutodeskViewerContext.Provider>
  )
}
