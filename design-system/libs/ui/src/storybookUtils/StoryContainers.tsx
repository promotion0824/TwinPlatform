import { PropsWithChildren } from 'react'

/** Reminder: to reuse the same test id if you add more story Containers. */
export const storyContainerTestId = 'story-container'

/**
 * A horizontal flex container for arranging multiple components in a
 * single story. It will restrict the width so that the screenshot size is minimal.
 * And it contains a test id which could be selected via `storybook.storyContainer`
 * in Playwright.
 */
export const StoryFlexContainer = (props: PropsWithChildren<unknown>) => (
  <div
    css={{ display: 'flex', gap: '1rem', width: 'fit-content' }}
    data-testid={storyContainerTestId}
    {...props}
  />
)
