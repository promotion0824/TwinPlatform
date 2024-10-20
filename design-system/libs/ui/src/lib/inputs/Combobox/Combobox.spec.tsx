import { render } from '../../../jest/testUtils'

import { Combobox } from '.'

describe('Combobox', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Combobox />)
    expect(baseElement).toBeTruthy()
  })
})
