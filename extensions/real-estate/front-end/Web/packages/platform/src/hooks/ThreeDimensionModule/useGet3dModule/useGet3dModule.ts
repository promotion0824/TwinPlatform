import { useQuery } from 'react-query'
import {
  get3dModule,
  get3dModuleFile,
} from '../../../services/ThreeDimensionModule/ThreeDimensionModuleService'

export default function useGet3dModule(siteId, options) {
  return useQuery(['3dModule', siteId], () => get3dModule(siteId), options)
}

export function useGet3dModuleFile(moduleData, options) {
  const { url: urn = undefined, name = undefined } = moduleData || {}
  return useQuery([urn, name], () => get3dModuleFile(urn, name), options)
}
