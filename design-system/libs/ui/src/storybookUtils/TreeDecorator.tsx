import { storyContainerTestId } from './StoryContainers'

export const TreeDecorator = (Story: React.ComponentType) => (
  <div data-testid={storyContainerTestId} style={{ width: 260 }}>
    <Story />
  </div>
)
