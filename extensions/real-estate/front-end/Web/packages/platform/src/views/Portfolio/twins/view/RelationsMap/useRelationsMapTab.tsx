import { useTranslation } from 'react-i18next'
import { Ontology } from '@willow/common/twins/view/models'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { Tabs, TabAndPanel } from '@willowinc/ui'

import RelationsMapTabPanel from './RelationsMapTabPanel'
import { TwinWithIds } from './types'
import { capabilityModelId } from '../../shared'

export default function useRelationsMapTab({
  initialTwin,
  modelsOfInterest,
  ontology,
}: {
  initialTwin: TwinWithIds & { metadata: { modelId: string } }
  modelsOfInterest: ModelOfInterest[]
  ontology: Ontology
}): TabAndPanel | undefined {
  const { t } = useTranslation()

  const showRelationsMap =
    // We exclude "isCapabilityOf" relationships and their associated
    // nodes, which means that if the initial node is a capability,
    // the graph is empty. So rather than render an empty graph we just hide
    // the tab entirely.
    !ontology
      .getModelAncestors(initialTwin.metadata.modelId)
      .includes(capabilityModelId)

  if (!showRelationsMap) {
    return undefined
  }

  return [
    <Tabs.Tab data-testid="twin-relationshipsMap-tab" value="relationsMap">
      {t('plainText.relationsMap')}
    </Tabs.Tab>,
    <Tabs.Panel value="relationsMap">
      <RelationsMapTabPanel
        initialTwin={initialTwin}
        modelsOfInterest={modelsOfInterest}
        ontology={ontology}
      />
    </Tabs.Panel>,
  ]
}
