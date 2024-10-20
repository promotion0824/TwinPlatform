import type { Meta, StoryObj } from '@storybook/react'

import { Box } from '.'
import { Stack } from '../../layout/Stack'
import { Link } from '../../navigation/Link'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Box> = {
  title: 'Box',
  component: Box,
}
export default meta

type Story = StoryObj<typeof Box>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    component: 'div',
    children: 'Box',
  },
}

export const Component: Story = {
  render: () => (
    <Stack>
      <Box component="div">Div</Box>
      <Box component="a" href="/">
        Link
      </Box>
      <div>
        <Box component="input" defaultValue="Text" />
      </div>
      <div>
        <Box component="select">
          <option>Option 1</option>
          <option>Option 2</option>
          <option>Option 3</option>
        </Box>
      </div>
    </Stack>
  ),
}

export const HiddenFrom: Story = {
  render: () => (
    <Stack>
      <h2>Resize your screen to test</h2>
      <Box component="div" hiddenFrom="mobile">
        Box 1 (hidden from mobile size)
      </Box>
      <Box component="div" hiddenFrom="tabletPortrait">
        Box 2 (hidden from tabletPortrait size)
      </Box>
      <Box component="div" hiddenFrom="tabletLandscape">
        Box 3 (hidden from tabletLandscape size)
      </Box>
      <Box component="div" hiddenFrom="monitor">
        Box 4 (hidden from monitor size)
      </Box>
      <Box component="div">Box 5</Box>
    </Stack>
  ),
}

/**
 * `Box` can be rendered with any root element using `renderRoot` prop:
 */
export const RenderRoot: Story = {
  render: () => (
    <Box
      renderRoot={(props) => (
        <Link
          {...props}
          href="https://storybook.willowinc.com/"
          target="_blank"
        />
      )}
    >
      Render as custom Link component
    </Box>
  ),
}
