import { render } from '../../../jest/testUtils'

import { File } from '.'

describe('File', () => {
  test.each([[{}], [{ loading: true }], [{ failed: true }]])(
    'should trigger onClose function when close button clicked for %s state',
    (state) => {
      const onClose = jest.fn()
      const { getByRole } = render(
        <File title="filename.pdf" onClose={onClose} {...state}>
          123KB
        </File>
      )
      const closeButton = getByRole('button', { name: 'close' })
      closeButton.click()
      expect(onClose).toHaveBeenCalled()
    }
  )

  it('should trigger onRetry function when retry button clicked', () => {
    const onRetry = jest.fn()
    const { getByRole } = render(
      <File title="filename.pdf" failed onRetry={onRetry}>
        123KB
      </File>
    )
    const retryButton = getByRole('button', { name: 'autorenew Retry' })
    retryButton.click()
    expect(onRetry).toHaveBeenCalled()
  })

  it('should replace the retry button text when retryButtonProps is provided', () => {
    const { getByRole } = render(
      <File
        title="filename.pdf"
        failed
        retryButtonProps={{ children: 'Custom Retry', prefix: null }}
      >
        123KB
      </File>
    )
    const retryButton = getByRole('button', { name: 'Custom Retry' })
    expect(retryButton).toBeInTheDocument()
  })
})
