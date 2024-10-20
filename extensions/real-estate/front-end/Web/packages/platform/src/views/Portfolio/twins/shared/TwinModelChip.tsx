import { Ontology } from '@willow/common/twins/view/models'
import {
  ModelOfInterest,
  getModelOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import { default as BaseTwinModelChip } from '@willow/common/twins/view/TwinModelChip'

export default function TwinModelChip({
  twin,
  ontology,
  modelsOfInterest,
  count,
}: {
  twin: { modelId: string }
  ontology: Ontology
  modelsOfInterest: ModelOfInterest[]
  count?: number
}) {
  const { modelId } = twin
  const model = ontology.getModelById(modelId)
  const modelOfInterest = getModelOfInterest(
    modelId,
    ontology,
    modelsOfInterest
  )

  return (
    <BaseTwinModelChip
      model={model}
      modelOfInterest={modelOfInterest}
      count={count}
    />
  )
}
