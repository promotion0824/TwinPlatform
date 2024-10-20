import { render } from '../../../jest/testUtils'

import { Badge } from '.'

describe('Badge', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Badge>Label</Badge>)
    expect(baseElement).toBeTruthy()
  })
})
