import { Div } from '@storybook/components'
import type { StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Button, IconButton } from '.'
import { Icon } from '../../misc/Icon'

const defaultStory = {
  component: Div, // doesn't matter what element is
  title: 'button',
}

export default defaultStory

export const LargeIconButton: StoryObj<typeof IconButton> = {
  render: () => (
    <IconButton size="large">
      <Icon icon="info" />
    </IconButton>
  ),
}

export const LoadingPrefix: StoryObj<typeof Button> = {
  decorators: [FlexDecorator],
  render: () => (
    <>
      <Button kind="primary" loading prefix={<Icon icon="info" />}>
        Click Me
      </Button>
      <Button kind="secondary" loading prefix={<Icon icon="info" />}>
        Click Me
      </Button>
      <Button kind="negative" loading prefix={<Icon icon="info" />}>
        Click Me
      </Button>
      <Button
        background="transparent"
        kind="secondary"
        loading
        prefix={<Icon icon="info" />}
      >
        Click Me
      </Button>
    </>
  ),
}

export const LoadingSuffix: StoryObj<typeof Button> = {
  decorators: [FlexDecorator],
  render: () => (
    <>
      <Button kind="primary" loading suffix={<Icon icon="info" />}>
        Click Me
      </Button>
      <Button kind="secondary" loading suffix={<Icon icon="info" />}>
        Click Me
      </Button>
      <Button kind="negative" loading suffix={<Icon icon="info" />}>
        Click Me
      </Button>
      <Button
        background="transparent"
        kind="secondary"
        loading
        suffix={<Icon icon="info" />}
      >
        Click Me
      </Button>
    </>
  ),
}

export const LoadingLarge: StoryObj<typeof Button> = {
  decorators: [FlexDecorator],
  render: () => (
    <>
      <Button kind="primary" loading size="large">
        Click Me
      </Button>
      <Button kind="secondary" loading size="large">
        Click Me
      </Button>
      <Button kind="negative" loading size="large">
        Click Me
      </Button>
      <Button background="transparent" kind="secondary" loading size="large">
        Click Me
      </Button>
    </>
  ),
}
