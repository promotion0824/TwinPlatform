import { useMutation } from 'react-query'
import { putModelOfInterest } from '../../services/Customers/ModelsOfInterestServices'
import { ExistingModelOfInterest } from '../../views/Admin/ModelsOfInterest/types'

export default function usePutModelOfInterest({ customerId, options }) {
  return useMutation(
    ({
      modelOfInterest,
      etag,
    }: {
      modelOfInterest: ExistingModelOfInterest
      etag: string
    }) => putModelOfInterest(customerId, modelOfInterest, etag),
    options
  )
}
