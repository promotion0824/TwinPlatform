import { act, render, screen, waitFor } from '../../../jest/testUtils'

import { Drawer, DrawerProps } from '.'
import { Button } from '../../buttons/Button'
import { useDisclosure } from '../../hooks'

const header = 'Drawer Header'
const openButtonLabel = 'Open Drawer'
const drawerContent = 'Drawer Content'
const footerButtonLabel = 'Submit'

const ExampleDrawer = (props: Partial<DrawerProps>) => {
  const [opened, { open, close }] = useDisclosure(false)

  return (
    <>
      <Drawer
        opened={opened}
        onClose={close}
        footer={<Button>{footerButtonLabel}</Button>}
        {...props}
      >
        {drawerContent}
      </Drawer>

      <Button onClick={open}>{openButtonLabel}</Button>
    </>
  )
}

const openDrawer = () => {
  screen.getByRole('button', { name: openButtonLabel }).click()
}
const closeButton = () => screen.getByRole('button', { name: 'close' })

describe('Drawer header', () => {
  it('should render header and close button if header provided', async () => {
    const { getByText } = render(<ExampleDrawer header={header} />)
    act(() => {
      openDrawer()
    })

    expect(closeButton).toBeTruthy()
    await waitFor(() => expect(getByText(header)).toBeInTheDocument())
  })

  it('should show only close button if no header passed', async () => {
    const { getByRole } = render(<ExampleDrawer />)

    act(() => {
      openDrawer()
    })

    expect(closeButton).toBeTruthy()
    await waitFor(() => expect(getByRole('heading')).toBeEmptyDOMElement())
  })

  it('should render only header if withCloseButton is false', async () => {
    const { getByText, queryByRole } = render(
      <ExampleDrawer header={header} withCloseButton={false} />
    )

    act(() => {
      openDrawer()
    })

    await waitFor(() => {
      expect(queryByRole('button', { name: 'close' })).not.toBeInTheDocument()
      expect(getByText(header)).toBeInTheDocument()
    })
  })

  it('should not have header section if no header and withCloseButton is false', () => {
    const { queryByRole } = render(<ExampleDrawer withCloseButton={false} />)

    act(() => {
      openDrawer()
    })

    expect(queryByRole('heading')).not.toBeInTheDocument()
    expect(queryByRole('button', { name: 'close' })).not.toBeInTheDocument()
  })
})
