import { StoryFlexContainer } from './StoryContainers'

/**
 * A horizontal flex container decorator for arranging multiple components
 * in a single story. The container will restrict the width so that
 * the screenshot size is minimal. The container contains a test id which could be
 * selected via `storybook.storyContainer`in Playwright.
 */
export const FlexDecorator = (Story: React.ComponentType) => (
  <StoryFlexContainer>
    <Story />
  </StoryFlexContainer>
)
