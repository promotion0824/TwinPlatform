import { useQuery } from 'react-query'
import { getAutoDeskFileManifest } from '../../../services/AutoDesk/AutoDeskService'

export default function useAutoDeskFileManifest(
  { urn, accessToken, tokenType },
  options = {}
) {
  const authorization = `${tokenType} ${accessToken}`
  return useQuery(
    [urn],
    () => getAutoDeskFileManifest(urn, authorization),
    options
  )
}
