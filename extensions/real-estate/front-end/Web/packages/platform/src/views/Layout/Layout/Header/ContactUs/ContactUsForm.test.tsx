import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { act } from 'react-dom/test-utils'
import ContactUsForm from './ContactUsForm'

const handler = [
  rest.get(`/api/contactus/categories`, (_req, res, ctx) => res(ctx.json([]))),
  rest.post('/api/contactus', (req, res, ctx) => res.once(ctx.json({}))),
]
const server = setupServer(...handler)
beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

describe('ContactUsForm', () => {
  const mockedOnClose = jest.fn()

  it('should render the form with all fields', () => {
    setup({
      mockedOnClose,
    })

    // Checking if all form fields are present
    expect(screen.getByText(name)).toBeInTheDocument()
    expect(screen.getByText(emailAddress)).toBeInTheDocument()
    expect(screen.getByText(category)).toBeInTheDocument()
    expect(screen.getByText(comment)).toBeInTheDocument()
    expect(screen.getByText(submit)).toBeInTheDocument()
  })

  it('expect email and name fields to be filled when email is valid', () => {
    setup({
      mockedOnClose,
      email: validEmail,
    })

    const textInputs = screen.queryAllByRole('textbox')
    expect(
      textInputs.find(
        (input) => input.getAttribute('name') === 'requestorsName'
      )
    ).toHaveValue(fullName)
    expect(
      textInputs.find(
        (input) => input.getAttribute('name') === 'requestorsEmail'
      )
    ).toHaveValue(validEmail)
  })

  it('expect email and name fields not to be filled when email is not valid', async () => {
    setup({
      mockedOnClose,
      email: forbiddenEmail,
    })

    const textInputs = screen.queryAllByRole('textbox')
    const nameInput = textInputs.find(
      (input) => input.getAttribute('name') === 'requestorsName'
    )
    const emailInput = textInputs.find(
      (input) => input.getAttribute('name') === 'requestorsEmail'
    )
    expect(nameInput).toHaveValue('')
    expect(emailInput).toHaveValue('')

    // fill in form with forbidden email, click on submit button
    await act(async () => {
      userEvent.type(nameInput!, fullName)
      userEvent.type(emailInput!, forbiddenEmail)
      userEvent.type(screen.getByText(comment), 'I need help with something.')
    })
    await act(async () => {
      userEvent.click(screen.getByText(submit))
    })
    // expect to see forbidden email error message
    expect(screen.getByText(forbiddenEmailError)).toBeInTheDocument()

    // expect to see invalid email error message when email is invalid
    await act(async () => {
      userEvent.clear(emailInput!)
      userEvent.type(emailInput!, invalidEmail)
    })
    expect(screen.getByText(invalidEmailError)).toBeInTheDocument()

    // expect to see no error message when email is valid
    await act(async () => {
      userEvent.clear(emailInput!)
      userEvent.type(emailInput!, validEmail)
    })
    expect(screen.queryByText(invalidEmailError)).toBeNull()
    expect(screen.queryByText(forbiddenEmailError)).toBeNull()
  })

  it('should submit the form with valid data', async () => {
    setup({
      mockedOnClose,
    })

    // Fill in the form fields with valid data
    await act(async () => {
      userEvent.type(screen.getByText(name), 'John Doe')
      userEvent.type(screen.getByText(emailAddress), 'john.doe@example.com')
      userEvent.type(screen.getByText(comment), 'I need help with something.')
    })

    // Submit the form with all the required field data
    await act(async () => {
      userEvent.click(screen.getByText(submit))
    })

    server.use(
      rest.post('/api/contactus', (_req, res, ctx) =>
        res((ctx.status(201), ctx.json({ message: requestSubmitted })))
      )
    )

    // Checking if the form is closed once the ticket is created
    await act(async () => {
      expect(mockedOnClose).toBeCalled()
    })
  })
})

const setup = ({
  mockedOnClose,
  email,
}: {
  mockedOnClose?: () => void
  email?: string
}) =>
  render(<ContactUsForm isFormOpen onClose={mockedOnClose} />, {
    wrapper: getWrapper({ email }),
  })

const name = 'Name'
const emailAddress = 'Email Address'
const category = 'Category'
const comment = 'How can we help you?'
const submit = 'Submit'
const nameRequired = 'Name is required'
const emailRequired = 'Email is required'
const commentRequired = 'Comment is required'
const requestSubmitted = 'Support request submitted.'
const validEmail = 'valid@iamvalid.com'
const forbiddenEmail = 'support@willowinc.com'
const invalidEmail = 'invalid@iaminvalid'
const firstName = 'Ellen'
const lastName = 'Langer'
const fullName = `${firstName} ${lastName}`
const invalidEmailError = 'Enter a valid email address'
const forbiddenEmailError =
  "Please use an email address other than 'support@willowinc.com'"

function getWrapper({ email }: { email?: string }) {
  return ({ children }) => (
    <BaseWrapper
      i18nOptions={{
        resources: {
          en: {
            translation: {
              'labels.name': name,
              'labels.emailAddress': emailAddress,
              'labels.category': category,
              'plainText.howCanWeHelpYou': comment,
              'plainText.submit': submit,
              'messages.nameRequired': nameRequired,
              'validationError.ERR_EMAIL_REQUIRED': emailRequired,
              'plainText.commentIsRequired': commentRequired,
              'plainText.supportRequestSubmitted': requestSubmitted,
              'plainText.validEmailAddressError': invalidEmailError,
            },
          },
        },
        lng: 'en',
        fallbackLng: ['en'],
      }}
      user={
        {
          email,
          firstName,
          lastName,
        } as any
      }
    >
      {children}
    </BaseWrapper>
  )
}
