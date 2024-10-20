import { useMutation } from 'react-query'
import postUser, { UserForm } from '../../services/User/UserService'

export default function usePostUser() {
  return useMutation(
    ({ id, formData, headers }: { id: string; formData: UserForm; headers }) =>
      postUser(id, formData, headers)
  )
}
