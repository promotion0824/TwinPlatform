import { render } from '../../../jest/testUtils'

import { Modal } from '.'
import { useDisclosure } from '../../hooks'

const ExampleModal = () => {
  const [opened, { open, close }] = useDisclosure(false)

  return (
    <>
      <Modal opened={opened} onClose={close} header="Modal Header">
        Modal Content
      </Modal>

      <button onClick={open}>Open Modal</button>
    </>
  )
}

describe('Modal', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<ExampleModal />)
    expect(baseElement).toBeTruthy()
  })
})
