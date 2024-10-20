import { Ontology } from '@willow/common/twins/view/models'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { Panel, TabAndPanel, Tabs, useCombineTabs } from '@willowinc/ui'

import _ from 'lodash'
import useFilePreviewTab from './FilePreview/useFilePreviewTab'
import useRelationsMapTab from './RelationsMap/useRelationsMapTab'
import useThreeDModelTab from './ThreeDModel/useThreeDModelTab'
import useTimeSeriesTab from './useTimeSeriesTab'

function TwinViewRightPanel({
  asset,
  id,
  modelInfo,
  modelsOfInterest,
  onTabChange,
  ontology,
  selectedTab,
  twin,
}: {
  asset: any
  id: string
  modelInfo: any
  modelsOfInterest: ModelOfInterest[]
  onTabChange: (tab: string) => void
  ontology: Ontology
  selectedTab: string
  twin: any
}) {
  const { geometrySpatialReference, siteID } = twin
  const isFile = modelInfo?.modelOfInterest?.name === 'File'

  const defaultTab =
    selectedTab || isFile
      ? 'filePreview'
      : asset?.hasLiveData
      ? 'timeSeries'
      : 'relationsMap'

  const threeDModelTab = useThreeDModelTab({
    asset,
    geometrySpatialReference,
    modelInfo,
    siteID,
    twin,
  })
  const filePreviewTab = useFilePreviewTab({ twin: isFile ? twin : undefined })
  const timeSeriesTab = useTimeSeriesTab({ asset, twin })
  const relationsMapTab = useRelationsMapTab({
    initialTwin: twin,
    modelsOfInterest,
    ontology,
  })

  const [tabs, tabsPanels] = useCombineTabs([
    ...(filePreviewTab ? [filePreviewTab] : []),
    ...(timeSeriesTab ? ([timeSeriesTab] as TabAndPanel[]) : []),
    ...(relationsMapTab ? [relationsMapTab] : []),
    ...(threeDModelTab ? [threeDModelTab] : []),
  ])

  return (
    <Panel
      collapsible
      id={id}
      // The resize events are called so that the 3D Model tab redraws itself
      onCollapse={dispatchResizeEvent}
      onResize={debounceResizeEvent}
      tabs={
        <Tabs
          defaultValue={defaultTab}
          keepMounted={false}
          onTabChange={onTabChange}
          value={selectedTab}
        >
          <Tabs.List>{tabs}</Tabs.List>
          {tabsPanels}
        </Tabs>
      }
    />
  )
}

export default TwinViewRightPanel
function dispatchResizeEvent() {
  window.dispatchEvent(new Event('resize'))
}

const debounceResizeEvent = _.debounce(dispatchResizeEvent, 100)
