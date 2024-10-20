import { useMutation } from 'react-query'
import { deleteWidget } from '../../services/Widgets/WidgetsService'

export default function useDeleteWidget() {
  return useMutation((id: string) => deleteWidget(id))
}
