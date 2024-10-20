import { render } from '../../../jest/testUtils'

import { Loader } from '.'

describe('Loader', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Loader />)
    expect(baseElement).toBeTruthy()
  })
})
