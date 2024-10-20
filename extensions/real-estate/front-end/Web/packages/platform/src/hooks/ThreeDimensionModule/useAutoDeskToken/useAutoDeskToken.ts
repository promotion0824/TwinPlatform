import { useQuery } from 'react-query'
import { getAutoDeskAccessToken } from '../../../services/AutoDesk/AutoDeskService'

export default function useAutoDeskToken(options = {}) {
  return useQuery(['autoDeskToken'], () => getAutoDeskAccessToken(), options)
}
