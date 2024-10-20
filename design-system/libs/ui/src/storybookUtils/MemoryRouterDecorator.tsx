import { MemoryRouter } from 'react-router-dom'

export const MemoryRouterDecorator = (Story: React.ComponentType) => (
  <MemoryRouter>
    <Story />
  </MemoryRouter>
)
