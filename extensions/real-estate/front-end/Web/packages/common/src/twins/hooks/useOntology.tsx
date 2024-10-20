import { useQuery } from 'react-query'
import { api } from '@willow/ui'
import { getModelLookup, Ontology } from '../view/models'

/**
 * Return a React Query that resolves to an `Ontology` object.
 * Note that we need a site ID to access the `/{siteId}/models` endpoint, but
 * since we only have one ontology, it doesn't actually matter which site ID.
 */
export default function useOntology(siteId: string) {
  return useQuery(['models'], () => getOntology(siteId))
}

export async function getOntology(siteId: string) {
  const { data: modelsList } = await api.get(`/sites/${siteId}/models`)
  return new Ontology(getModelLookup(modelsList))
}
