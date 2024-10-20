import Flex from 'components/Flex/Flex'
import Form from 'components/Form/Form'
import Message from 'components/Message/Message'
import Modal from './Modal'
import ModalSubmitButton from './ModalSubmitButton'

export default function QuestionModal({
  header,
  submitText,
  icon = 'error',
  question,
  children,
  onSubmit,
  onSubmitted = (modal) => modal.close(),
  submitButtonDisabled = false,
  FormProps,
  ...rest
}) {
  return (
    <Modal size="small" {...rest} header={header}>
      {(modal) => (
        <Form
          onSubmit={onSubmit}
          onSubmitted={() => onSubmitted(modal)}
          {...FormProps}
        >
          <Flex fill="header">
            <Flex padding="large">
              <Message align="center" icon={icon}>
                <Flex size="medium">
                  {question != null && <div>{question}</div>}
                  <div>{children}</div>
                </Flex>
              </Message>
            </Flex>
          </Flex>
          <ModalSubmitButton disabled={submitButtonDisabled}>
            {submitText ?? header}
          </ModalSubmitButton>
        </Form>
      )}
    </Modal>
  )
}
