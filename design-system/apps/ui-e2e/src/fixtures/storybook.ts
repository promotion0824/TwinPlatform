import { Locator, Page, expect } from '@playwright/test'
import {
  makeHiddenStoryTitle,
  storyContainerTestId,
} from '@willowinc/storybookUtils'
import { kebabCase } from 'lodash'

const componentPathPrefix = 'components-'
const storyPathPrefix = 'iframe.html?id='
const docPathPrefix = '?path=/docs/'

/** Fixture to provide methods to interact with Storybook */
export class Storybook {
  constructor(readonly page: Page) {}

  private getStoryPath(
    storyTitle: string,
    groupName: string,
    storyName: string,
    componentPrefix: string = componentPathPrefix
  ) {
    return (
      storyPathPrefix +
      componentPrefix +
      kebabCase(groupName) +
      '-' +
      storyTitle.toLowerCase() +
      '--' +
      kebabCase(storyName)
    )
  }

  private getDocPath(
    storyTitle: string,
    groupName: string,
    componentPrefix: string = componentPathPrefix
  ) {
    return (
      docPathPrefix +
      componentPrefix +
      kebabCase(groupName) +
      '-' +
      storyTitle.toLowerCase()
    )
  }

  /**
   * Navigates to a single story or a doc page
   */
  goto(storyTitle: string, groupName: string, storyName = '') {
    return storyName
      ? this.page.goto(this.getStoryPath(storyTitle, groupName, storyName))
      : this.page.goto(this.getDocPath(storyTitle, groupName))
  }

  /**
   * Navigates to a single story that is hidden from the sidebar
   * @param storyTitle The parameter passed in when create the hidden
   * story, no need to care about the hidden prefix.
   * @param storyName
   */
  gotoHidden(storyTitle: string, storyName = '') {
    return this.page.goto(
      storyPathPrefix +
        makeHiddenStoryTitle(storyTitle.toLowerCase()) +
        '--' +
        kebabCase(storyName)
    )
  }

  /**
   * Run visual regression tests for locator with default, hover, focus and active status.
   * @note Need to await for this method when using it.
   */
  async testInteractions(
    snapshotPrefix: string,
    locator: Locator,
    screenshotTarget: Locator = locator,
    interactions: ('default' | 'hover' | 'focus' | 'active' | 'click')[] = [
      'default',
      'hover',
      'focus',
      'active',
      'click',
    ]
  ) {
    if (interactions.includes('default')) {
      await expect(screenshotTarget).toHaveScreenshot(
        `${snapshotPrefix}-default.png`
      )
    }

    if (interactions.includes('hover')) {
      await locator.hover()
      await expect(screenshotTarget).toHaveScreenshot(
        `${snapshotPrefix}-hover.png`
      )
      await this.page.locator('body').hover() // remove hover so no hover style left
    }
    if (interactions.includes('focus')) {
      await locator.focus()
      await expect(screenshotTarget).toHaveScreenshot(
        `${snapshotPrefix}-focus.png`
      )
      await locator.blur() // remove focus so no focus style left
    }

    if (interactions.includes('active')) {
      await locator.hover() // move mouse over the element so mouse down can be performed
      await this.page.mouse.down()
      await expect(screenshotTarget).toHaveScreenshot(
        `${snapshotPrefix}-active.png`
      )
    }

    if (interactions.includes('click')) {
      await locator.click()
      await expect(screenshotTarget).toHaveScreenshot(
        `${snapshotPrefix}-click.png`
      )
    }
  }

  /** Returns the component container for a story */
  get storyRoot() {
    return this.page.locator('#storybook-root')
  }

  /** Returns the container component used to organize a story. */
  get storyContainer() {
    return this.storyRoot.getByTestId(storyContainerTestId)
  }
}
