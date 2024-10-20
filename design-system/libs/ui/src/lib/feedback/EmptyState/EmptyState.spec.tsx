import { render } from '../../../jest/testUtils'

import { EmptyState } from '.'

describe('EmptyState', () => {
  it('should render icon component when icon has been provided', () => {
    const { getByText } = render(<EmptyState icon="info" />)

    expect(getByText('info')).toBeInTheDocument()
  })

  it('should pass iconProps to icon component when both of them have been provided', () => {
    const { getByTestId } = render(
      <EmptyState icon="info" iconProps={{ 'data-testid': 'icon' }} />
    )

    expect(getByTestId('icon')).toHaveTextContent('info')
  })

  it('should render illustration component when illustration has been provided', () => {
    const { getByAltText } = render(
      <EmptyState illustration="no-permissions" />
    )

    expect(getByAltText('no-permissions-dark')).toBeInTheDocument()
  })

  it('should render graphic component when both graphic and icon and illustration has been provided', () => {
    const { getByTestId } = render(
      <EmptyState
        graphic={<div data-testid="graphic" />}
        icon="info"
        illustration="no-permissions"
      />
    )

    expect(getByTestId('graphic')).toBeInTheDocument()
  })

  it('should render illustration component when both illustration and icon has been provided', () => {
    const { getByAltText } = render(
      <EmptyState illustration="no-permissions" icon="info" />
    )

    expect(getByAltText('no-permissions-dark')).toBeInTheDocument()
  })

  it('should pass illustrationProps to illustration component when illustration and illustrationProps has been provided', () => {
    const { getByAltText } = render(
      <EmptyState
        illustration="no-permissions"
        illustrationProps={{ alt: 'image', 'data-testid': 'image' }}
      />
    )

    expect(getByAltText('image')).toHaveAttribute('data-testid', 'image')
  })

  it('should render primary actions when provided', () => {
    const { getByText } = render(
      <EmptyState primaryActions={<button>action</button>} />
    )

    expect(getByText('action')).toBeInTheDocument()
  })

  it('should render secondary actions when provided', () => {
    const { getByText } = render(
      <EmptyState secondaryActions={<a href="/">link</a>} />
    )

    expect(getByText('link')).toBeInTheDocument()
  })

  it('should render primary actions and secondary actions when both provided', () => {
    const { getByText } = render(
      <EmptyState
        primaryActions={<button>action</button>}
        secondaryActions={<a href="/">link</a>}
      />
    )

    expect(getByText('action')).toBeInTheDocument()
    expect(getByText('link')).toBeInTheDocument()
  })
})
