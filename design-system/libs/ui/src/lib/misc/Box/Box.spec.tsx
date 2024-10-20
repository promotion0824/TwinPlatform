import { render, screen } from '../../../jest/testUtils'

import { Box } from '.'

describe('Box', () => {
  it('should render successfully', () => {
    render(<Box>Hello</Box>);
    expect(screen.getByText(/Hello/)).toBeInTheDocument();
  });

  it('should render span element when the component is span', () => {
    render(<Box component="span">Hello</Box>);
    expect(screen.getByText(/Hello/).tagName).toBe('SPAN');
  });

  it('should render a element with href when the component is a', () => {
    render(<Box component="a" href="https://example.com">Hello</Box>);
    expect(screen.getByText(/Hello/).tagName).toBe('A');
    expect(screen.getByText(/Hello/)).toHaveAttribute('href', 'https://example.com');
  });
})
