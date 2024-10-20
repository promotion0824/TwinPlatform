import { render } from '../../../jest/testUtils'

import { Radio } from '.'

describe('Radio', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Radio label="Label" />)
    expect(baseElement).toBeTruthy()
  })

  it('height should remain 16px when error is true', () => {
    const { container } = render(<Radio label="Label" error />)
    expect(container).toHaveStyle('height: 16')
  })
})
