import { useMutation } from 'react-query'
import { deleteModelOfInterest } from '../../services/Customers/ModelsOfInterestServices'

export default function useDeleteModelOfInterest({ customerId, options }) {
  return useMutation(
    ({ id, etag }: { id: string; etag: string }) =>
      deleteModelOfInterest(customerId, id, etag),
    options
  )
}
