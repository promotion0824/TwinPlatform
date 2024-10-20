import { render } from '../../../jest/testUtils'

import { PillGroup } from '.'

describe('PillGroup', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<PillGroup />)
    expect(baseElement).toBeTruthy()
  })
})
