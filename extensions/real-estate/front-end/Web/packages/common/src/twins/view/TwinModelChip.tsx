import { useTranslation } from 'react-i18next'
import { TwinChip } from '@willow/ui'
import { getModelDisplayName, Model } from './models'
import { ModelOfInterest } from './modelsOfInterest'

export default function TwinModelChip({
  model,
  modelOfInterest,
  count,
  highlightOnHover,
  className,
}: {
  model?: Model
  modelOfInterest?: ModelOfInterest
  count?: number
  highlightOnHover?: boolean
  className?: string
}) {
  const translation = useTranslation()

  if (modelOfInterest != null) {
    return (
      <TwinChip
        modelOfInterest={modelOfInterest}
        // Don't double up the model name if the twin's model *is*
        // the model of interest.
        gappedText={
          model != null && model['@id'] !== modelOfInterest.modelId
            ? getModelDisplayName(model, translation)
            : null
        }
        count={count}
        highlightOnHover={highlightOnHover}
        className={className}
      />
    )
  } else {
    return (
      <TwinChip
        text={
          model != null ? getModelDisplayName(model, translation) : undefined
        }
        count={count}
        highlightOnHover={highlightOnHover}
      />
    )
  }
}
