import { useMutation } from 'react-query'
import { postModelOfInterest } from '../../services/Customers/ModelsOfInterestServices'
import { PartialModelOfInterest } from '../../views/Admin/ModelsOfInterest/types'

export default function usePostSelectedModelOfInterest({
  customerId,
  options,
}) {
  return useMutation(
    ({
      modelOfInterest,
      etag,
    }: {
      modelOfInterest: PartialModelOfInterest
      etag: string
    }) => postModelOfInterest(customerId, modelOfInterest, etag),
    options
  )
}
