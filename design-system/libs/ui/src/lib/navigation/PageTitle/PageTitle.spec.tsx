import { render } from '../../../jest/testUtils'

import { PageTitle } from '.'

describe('PageTitle', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<PageTitle />)
    expect(baseElement).toBeTruthy()
  })
})
