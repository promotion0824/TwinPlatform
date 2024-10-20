import _ from 'lodash'
import { v4 as uuidv4 } from 'uuid'
import { rest } from 'msw'
import { useQuery } from 'react-query'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { act, render, screen, waitFor } from '@testing-library/react'
import { setupServer } from 'msw/node'
import { Launcher, ChatApp } from '@willow/common'
import { supportDropdowns } from '@willow/ui/utils/testUtils/dropdown'
import userEvent from '@testing-library/user-event'

supportDropdowns()

const handler = [
  rest.post(`/api/copilot/response`, (_req, res, ctx) =>
    res(ctx.json(response))
  ),
]

const server = setupServer(...handler)
jest.mock('react-query', () => ({
  ...jest.requireActual('react-query'),
  useQuery: jest.fn(),
}))

beforeEach(() => {
  useQuery.mockClear()
})
beforeAll(() => server.listen())

afterEach(() => {
  server.resetHandlers()
  localStorage.clear()
  jest.clearAllMocks()
  useQuery.mockReset()
})

afterAll(() => server.close())

const response = {
  content: 'Please provide additional context.',
  citations: ['Sample-Page10.pdf', 'Sample-Page11.pdf', 'Sample-page12.pdf'],
}
const sampleUserMessage = 'Sample Question 2?'

const sampleMessages = [
  { content: 'Sample Question ?', id: uuidv4() },
  response,
]

const willowCopilot = 'Willow Copilot'
const copilot = 'Copilot'
const error = 'We encountered an issue processing your request.'
const resetChat = 'reset-chat'
const close = 'close'
const send = 'send'
const deleteText = 'delete'
const chatResponse = 'chat-response'

// TODO : "Test Cases Needs to Be modified to take API changes into consideration"
// Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/127205
describe.skip('Copilot', () => {
  test('expect to see toggle to allow user open and close copilot widget', async () => {
    useQuery.mockImplementation(jest.requireActual('react-query').useQuery)
    const { rerender } = setup({ enableCopilot: true })

    const launcher = await screen.findByText(copilot)
    expect(launcher).toBeInTheDocument()

    // toggle launcher
    userEvent.click(launcher)
    rerenderWithProps({ rerender, isActive: true, enableCopilot: true })

    // check if chat widget is opened
    await waitFor(() => {
      expect(screen.queryByText(willowCopilot)).toBeInTheDocument()
    })

    // toggle launcher
    userEvent.click(launcher)
    rerenderWithProps({ rerender, enableCopilot: true })

    await waitFor(() => {
      // This checks if Toggle Functionality of Launcher is working as expected
      expect(screen.queryByText(willowCopilot)).not.toBeInTheDocument()
    })

    // toggle launcher
    userEvent.click(launcher)
    rerenderWithProps({ rerender, isActive: true, enableCopilot: true })

    // check if chat widget is opened.
    await waitFor(() => {
      expect(screen.queryByText(willowCopilot)).toBeInTheDocument()
    })

    const closeIcon = await screen.findByText(close)
    expect(closeIcon).toBeInTheDocument()

    userEvent.click(closeIcon)
    rerenderWithProps({ rerender, enableCopilot: true })

    await waitFor(() => {
      // This checks if close functionality of close icon is working as expected
      expect(screen.queryByText(willowCopilot)).not.toBeInTheDocument()
    })
  })

  test('expect to see no  messages if trash icon is clicked', async () => {
    const mockDeleteFunction = jest.fn()
    const mockUseQuery = jest.fn().mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: false,
      data: [],
      error: null,
    })

    useQuery.mockImplementation((queryKey) => {
      if (Array.isArray(queryKey) && queryKey[0] === resetChat) {
        return mockUseQuery()
      }

      return {
        isLoading: false,
        isFetching: false,
        isError: false,
        data: response,
        error: null,
      }
    })

    const { rerender } = setup({ enableCopilot: true })
    rerenderWithProps({
      rerender,
      isActive: true,
      userMessage: '',
      messages: sampleMessages,
      onSetCitations: mockDeleteFunction,
      enableCopilot: true,
    })

    await waitFor(() => {
      expect(screen.queryByText(willowCopilot)).toBeInTheDocument()
    })

    // click on delete icon
    const deleteIcon = await screen.findByText(deleteText)
    userEvent.click(deleteIcon)

    // call mock function
    await waitFor(() => {
      expect(mockUseQuery).toHaveBeenCalled()
      expect(mockDeleteFunction).toHaveBeenCalled()
    })

    rerenderWithProps({ rerender, isActive: true, enableCopilot: true })

    // check if chat history is been cleared
    await waitFor(() => {
      expect(
        screen.queryByText(sampleMessages[0].content)
      ).not.toBeInTheDocument()

      expect(screen.queryByText(response.content)).not.toBeInTheDocument()
    })
  })

  test('expect to see error message, when response fails', async () => {
    const mockUseQuery = jest.fn().mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: true,
      data: undefined,
      error: null,
    })

    useQuery.mockImplementation((queryKey) => {
      if (Array.isArray(queryKey) && queryKey[0] === chatResponse) {
        return mockUseQuery()
      }

      return {
        isLoading: false,
        isFetching: false,
        isError: false,
        data: [],
        error: null,
      }
    })

    const { rerender } = setup({ enableCopilot: true, isActive: true })

    // check if chat widget is opened
    await waitFor(() => {
      expect(screen.queryByText(willowCopilot)).toBeInTheDocument()
    })

    setupServerWithReject()

    const sendIcon = await screen.findByText(send)
    userEvent.click(sendIcon)

    // call mock function
    await waitFor(() => {
      expect(mockUseQuery).toHaveBeenCalled()
    })

    rerenderWithProps({
      rerender,
      isActive: true,
      userMessage: '',
      messages: [
        ...sampleMessages,
        sampleUserMessage,
        {
          content: error,
          citations: [],
          isError: true,
        },
      ],
    })

    const errorResponse = await screen.findByText(error)
    expect(errorResponse).toBeInTheDocument()
  })
})

const setupServerWithReject = () =>
  server.use(
    ...[
      rest.post('/api/copilot/response', (req, res, ctx) =>
        res(ctx.status(400))
      ),
    ]
  )

const setup = ({ isActive = false, enableCopilot = false } = {}) =>
  render(
    <>
      <Launcher isActive={isActive} />
      {isActive && <ChatApp enableCopilot={enableCopilot} />}
    </>,
    {
      wrapper: getWrapper({ enableCopilot }),
    }
  )

const getWrapper =
  ({ enableCopilot }) =>
  ({ children }) =>
    (
      <BaseWrapper
        i18nOptions={{
          resources: {
            en: {
              translation: {
                'headers.willowCopilot': willowCopilot,
                'plainText.copilot': copilot,
                'plainText.errorGeneratingResponse': error,
              },
            },
          },
          lng: 'en',
          fallbackLng: ['en'],
        }}
        hasFeatureToggle={(featureFlag) =>
          enableCopilot || featureFlag === 'copilot'
        }
      >
        {children}
      </BaseWrapper>
    )

const rerenderWithProps = ({
  rerender,
  isActive = false,
  userMessage = '',
  messages = [],
  onSetCitations,
  enableCopilot = false,
} = {}) =>
  rerender(
    <>
      <Launcher isActive={isActive} />
      {isActive && (
        <ChatApp
          onToggle={() => jest.fn()}
          onSetCitations={onSetCitations}
          userMessage={userMessage}
          messages={messages}
          enableCopilot={enableCopilot}
        />
      )}
    </>
  )
