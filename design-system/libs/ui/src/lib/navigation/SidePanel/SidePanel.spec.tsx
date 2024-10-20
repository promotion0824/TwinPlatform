import { render } from '../../../jest/testUtils'

import { SidePanel } from '.'

describe('SidePanel', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<SidePanel>dummy content</SidePanel>)
    expect(baseElement).toBeTruthy()
  })
})
