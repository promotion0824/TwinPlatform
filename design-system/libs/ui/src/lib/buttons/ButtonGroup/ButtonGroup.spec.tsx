import { render } from '../../../jest/testUtils'

import { ButtonGroup } from './'

describe('ButtonGroup', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<ButtonGroup />)
    expect(baseElement).toBeTruthy()
  })
})
