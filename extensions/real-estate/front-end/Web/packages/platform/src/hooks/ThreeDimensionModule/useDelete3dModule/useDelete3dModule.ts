import { useMutation } from 'react-query'
import { delete3dModule } from '../../../services/ThreeDimensionModule/ThreeDimensionModuleService'

export default function useDelete3dModule() {
  return useMutation((siteId) => delete3dModule(siteId))
}
