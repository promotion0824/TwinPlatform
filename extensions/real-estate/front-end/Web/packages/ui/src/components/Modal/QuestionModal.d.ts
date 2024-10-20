import { FormProps } from '../Form/Form'
import { ModalProps } from './Modal'

export interface QuestionModalProps extends ModalProps {
  children: React.ReactNode
  FormProps?: FormProps
  header: React.ReactNode
  icon?: string
  onSubmit?: (form) => Promise<any>
  onSubmitted?: (modal) => void
  question?: string
  submitButtonDisabled?: boolean
  submitText?: string
}

export default function QuestionModal(props: QuestionModalProps): ReactElement
