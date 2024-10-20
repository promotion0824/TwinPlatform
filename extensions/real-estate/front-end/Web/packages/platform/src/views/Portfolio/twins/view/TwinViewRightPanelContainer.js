import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { api, Progress } from '@willow/ui'
import { Panel } from '@willowinc/ui'
import { useQuery } from 'react-query'

import useOntology from '../../../../hooks/useOntologyInPlatform'
import TwinViewRightPanel from './TwinViewRightPanel'

const TwinViewRightPanelContainer = ({
  id,
  modelInfo,
  onChangeTab,
  selectedTab,
  twin,
}) => {
  const modelsOfInterest = useModelsOfInterest().data.items
  const ontology = useOntology().data

  // It is possible for a twin to not have associated siteId, e.g. a Campus, or a Land
  // In this case, the only information we can show is relationships tab, nothing else
  const { siteID, uniqueID } = twin

  const { data: asset } = useQuery(
    ['asset', siteID, uniqueID],
    async () => {
      const response = await api.get(`/sites/${siteID}/assets/${uniqueID}`)
      return response.data
    },
    {
      enabled: !!siteID,
    }
  )

  const isLoading =
    (!!siteID && !!uniqueID && asset == null) ||
    modelsOfInterest == null ||
    ontology == null

  // We wait for the asset query to finish before displaying any tabs, to avoid
  // tabs moving around when the data comes in
  return isLoading ? (
    <Panel id={id}>
      <Progress />
    </Panel>
  ) : (
    <TwinViewRightPanel
      asset={asset}
      id={id}
      modelInfo={modelInfo}
      modelsOfInterest={modelsOfInterest}
      onTabChange={onChangeTab}
      ontology={ontology}
      selectedTab={selectedTab}
      twin={twin}
    />
  )
}

export default TwinViewRightPanelContainer
