import { render, screen } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import DisabledWarning from './DisabledWarning'

describe('DisabledWarning', () => {
  const title = 'hello'
  test('display title', () => {
    render(<DisabledWarning title={title} />, { wrapper: Wrapper })

    expect(screen.queryByText(title)).toBeInTheDocument()
  })

  test('display a password icon as default when there is no icon prop', () => {
    const { container } = render(<DisabledWarning title={title} />, {
      wrapper: Wrapper,
    })

    expect(
      container.querySelector('svg[data-file-name="SvgPassword"]')
    ).toBeInTheDocument()
  })

  test('display a custom icon when there is an icon prop', () => {
    const { container } = render(
      <DisabledWarning title={title} icon="reset" />,
      {
        wrapper: Wrapper,
      }
    )

    expect(
      container.querySelector('svg[data-file-name="SvgReset"]')
    ).toBeInTheDocument()
  })

  test('display a default description when there is no children prop', () => {
    render(<DisabledWarning title={title} />, {
      wrapper: Wrapper,
    })

    expect(
      screen.queryByText('For access, please submit a request through', {
        exact: false,
      })
    ).toBeInTheDocument()
  })

  test('display a custom description when there is a children prop', () => {
    const descText = 'description'

    render(
      <DisabledWarning title={title}>
        <div>{descText}</div>
      </DisabledWarning>,
      {
        wrapper: Wrapper,
      }
    )

    expect(screen.queryByText(descText)).toBeInTheDocument()
  })
})
