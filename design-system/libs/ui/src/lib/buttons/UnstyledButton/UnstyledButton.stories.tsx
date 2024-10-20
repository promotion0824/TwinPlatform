import type { Meta, StoryObj } from '@storybook/react'
import { useRef } from 'react'
import { UnstyledButton } from '.'
import { Link } from '../../navigation/Link'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof UnstyledButton> = {
  title: 'UnstyledButton',
  component: UnstyledButton,
}
export default meta

type Story = StoryObj<typeof UnstyledButton>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    children: 'Unstyled button',
    onClick: () => {
      window.alert('UnstyledButton clicked')
    },
  },
}

/**
 * `UnstyledButton` is a polymorphic component – its default root element is button,
 * but it can be changed to any other element or component with `component` prop:
 */
export const PolymorphicComponent: Story = {
  render: () => {
    const ref = useRef<HTMLAnchorElement | null>(null)
    return (
      <UnstyledButton
        component="a"
        ref={ref}
        href="https://storybook.willowinc.com/"
        target="_blank"
      >
        Render as HTML Anchor element
      </UnstyledButton>
    )
  },
}

/**
 * `UnstyledButton` can be rendered with any root element using `renderRoot` prop:
 */
export const RenderRoot: Story = {
  render: () => (
    <UnstyledButton
      renderRoot={(props) => (
        <Link
          {...props}
          href="https://storybook.willowinc.com/"
          target="_blank"
        />
      )}
    >
      Render as custom Link component
    </UnstyledButton>
  ),
}
