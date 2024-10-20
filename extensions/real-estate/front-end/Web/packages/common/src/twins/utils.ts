/* eslint-disable import/prefer-default-export */
import { UseTranslationResponse } from 'react-i18next'
import { getModelDisplayName, Model, Ontology } from './view/models'
import { getModelOfInterest, ModelOfInterest } from './view/modelsOfInterest'

export function getModelInfo(
  model: Model,
  ontology: Ontology,
  modelsOfInterest: ModelOfInterest[],
  translation: UseTranslationResponse<'translation', undefined>
) {
  const modelId = model['@id']
  return {
    id: modelId,
    model,
    expandedModel: ontology.getExpandedModel(modelId),
    displayName: getModelDisplayName(model, translation),
    modelOfInterest: getModelOfInterest(modelId, ontology, modelsOfInterest),
  }
}
