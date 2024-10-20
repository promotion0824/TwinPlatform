import { useMutation } from 'react-query'
import { DashboardConfigForm } from '../../components/Reports/DashboardModal/DashboardConfigForm'
import { postWidget } from '../../services/Widgets/WidgetsService'

export default function usePostWidget() {
  return useMutation((formData: DashboardConfigForm) => postWidget(formData))
}
