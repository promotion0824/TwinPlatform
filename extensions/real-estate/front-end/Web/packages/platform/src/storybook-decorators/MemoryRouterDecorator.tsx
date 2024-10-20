import { MemoryRouter } from 'react-router-dom'

const MemoryRouterDecorator = (Story: React.ComponentType) => (
  <MemoryRouter>
    <Story />
  </MemoryRouter>
)

export default MemoryRouterDecorator
