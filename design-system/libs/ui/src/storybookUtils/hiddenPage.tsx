/** The prefix that used to determine whether to hide an element */
// same as in /libs/ui/.storybook/manager-head.html
export const hiddenPrefix = 'hidden-test-components'

/**
 * @returns the story title that will be used to identify whether to hide the story
 */
export const makeHiddenStoryTitle = (storyTitle: string) =>
  hiddenPrefix + '-' + storyTitle
