import { useMutation } from 'react-query'
import { DashboardConfigForm } from '../../components/Reports/DashboardModal/DashboardConfigForm'
import { putWidget } from '../../services/Widgets/WidgetsService'

export default function usePutWidget() {
  return useMutation(
    ({ id, formData }: { id: string; formData: DashboardConfigForm }) =>
      putWidget(id, formData)
  )
}
