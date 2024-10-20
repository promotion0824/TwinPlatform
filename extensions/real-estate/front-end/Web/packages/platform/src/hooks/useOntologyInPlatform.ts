import { useQuery } from 'react-query'
import { getOntology } from '@willow/common/twins/hooks/useOntology'
import { useSites } from '../providers/sites/SitesContext'

/**
 * A wrapper function to useOntology from @willow/common/twins/hooks/useOntology
 * which includes the logic to find a site ID.
 *
 * In future even the site ID should be unnecessary, though. See
 * - https://dev.azure.com/willowdev/Unified/_workitems/edit/136718
 * - https://dev.azure.com/willowdev/Unified/_workitems/edit/136719
 * - https://dev.azure.com/willowdev/Unified/_workitems/edit/136720
 */
export default function useOntologyInPlatform() {
  const sites = useSites()
  const siteId = sites?.[0]?.id

  return useQuery(['models-platform'], () => {
    return getOntology(siteId)
  })
}
