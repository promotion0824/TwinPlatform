import { Handle, Position } from 'react-flow-renderer'

import { UnstyledButton } from '@willow/ui'
import {
  getModelOfInterest,
  ModelOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import { Ontology } from '@willow/common/twins/view/models'

import TwinModelChip from '@willow/common/twins/view/TwinModelChip'

export default function ModelChipNode({
  modelId,
  ontology,
  count,
  onClick,
  modelsOfInterest,
}: {
  modelId: string
  ontology: Ontology
  count: number
  onClick: () => void | undefined
  modelsOfInterest: ModelOfInterest[]
}) {
  const model = ontology.getModelById(modelId)
  const modelOfInterest = getModelOfInterest(
    modelId,
    ontology,
    modelsOfInterest
  )

  // React Flow gets upset if we don't have handles, but we don't want them to
  // to actually be visible so we create them with opacity of zero.
  return (
    <div data-testid={`ModelChipNode-${modelId}`}>
      <Handle type="target" position={Position.Bottom} style={{ opacity: 0 }} />
      <UnstyledButton
        onClick={onClick}
        // We apply this class name to the button to tell React Flow not to
        // attach d3-drag to it, which breaks tests that try to use the button.
        // See https://github.com/wbkd/react-flow/issues/2461
        className="nodrag"
      >
        <TwinModelChip
          modelOfInterest={modelOfInterest}
          model={model}
          count={count}
          highlightOnHover
        />
      </UnstyledButton>
      <Handle type="source" position={Position.Top} style={{ opacity: 0 }} />
    </div>
  )
}
