import { render } from '../../../jest/testUtils'

import { Collapse } from '.'

const content = 'test content'

describe('Collapse', () => {
  it('should hide content when opened=false', () => {
    const { getByText } = render(<Collapse opened={false}>{content}</Collapse>)

    expect(getByText(content)).not.toBeVisible()
  })

  it('should show content when opened=true', () => {
    const { getByText } = render(<Collapse opened>{content}</Collapse>)

    expect(getByText(content)).toBeInTheDocument()
  })
})
