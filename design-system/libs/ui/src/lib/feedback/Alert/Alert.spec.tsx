import { render } from '../../../jest/testUtils'

import { Alert } from '.'

describe('Alert', () => {
  it('should render successfully', () => {
    const { getByText } = render(<Alert>Test Alert</Alert>)
    expect(getByText('Test Alert')).toBeInTheDocument()
  })
})
