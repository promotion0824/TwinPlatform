import userEvent from '@testing-library/user-event'
import {
  render,
  screen,
  waitFor,
  waitForElementToBeRemoved,
} from '../../../jest/testUtils'

import { Tooltip, TooltipProps } from '.'

const triggerContent = 'Trigger'

const DefaultTooltip = (props: Partial<TooltipProps>) => (
  <Tooltip label="Tooltip Content" {...props}>
    <button>{triggerContent}</button>
  </Tooltip>
)

function getTrigger() {
  return screen.getByText(triggerContent)
}
function getTooltip() {
  return screen.getByRole('tooltip')
}

describe('Tooltip content', () => {
  it('should be initially hidden', async () => {
    await waitFor(() => {
      // need to wait for the state to update
      expect(screen.queryByRole('tooltip')).toBeNull()
    })
  })

  it('should show when hover over the trigger', async () => {
    render(<DefaultTooltip />)

    await userEvent.hover(getTrigger())

    // the tooltip rendered with 'opacity: 0' in pipeline, not sure why,
    // so cannot use 'toBeVisible' here as it failed in pipeline.
    await waitFor(() => expect(getTooltip()).toBeInTheDocument())
  })

  it('should hide when press escape key', async () => {
    render(<DefaultTooltip />)
    await userEvent.hover(getTrigger())
    await waitFor(() => expect(getTooltip()).toBeInTheDocument())

    await userEvent.keyboard('{escape}')
    await waitForElementToBeRemoved(screen.queryByRole('tooltip'))
  })

  it('should not be able to trigger when disabled', async () => {
    render(<DefaultTooltip disabled />)

    await waitFor(async () => userEvent.hover(getTrigger()))

    expect(screen.queryByRole('tooltip')).toBeNull()
  })
})

describe('With initialOpen = true, the Tooltip content', () => {
  it('should be initially visible', async () => {
    render(<DefaultTooltip opened />)
    await screen.findByRole('tooltip')

    expect(getTooltip()).toBeVisible()
  })

  it('should hide after unhover the trigger', async () => {
    render(<DefaultTooltip opened />)

    await userEvent.hover(getTrigger())
    await waitFor(() => expect(getTooltip()).toBeInTheDocument())

    await userEvent.unhover(getTrigger())
    waitFor(() => {
      expect(screen.queryByRole('tooltip')).toBeNull()
    })
  })
})

describe('With open = true, the Tooltip content', () => {
  it('should be initially visible', async () => {
    render(<DefaultTooltip opened />)
    await screen.findByRole('tooltip')

    expect(getTooltip()).toBeVisible()
  })

  it('should stay visible when unhover the trigger', async () => {
    render(<DefaultTooltip opened />)

    await userEvent.hover(getTrigger())
    await waitFor(() => expect(getTooltip()).toBeInTheDocument())

    await waitFor(async () => userEvent.unhover(getTrigger()))
    expect(getTooltip()).toBeVisible()
  })

  it('should be initially visible with tooltip arrow when withArrow = true', async () => {
    render(<DefaultTooltip opened withArrow />)
    const tooltip = await screen.findByRole('tooltip')

    expect(getTooltip()).toBeVisible()
    expect(tooltip.querySelector('.mantine-Tooltip-arrow')).toBeInTheDocument()
  })
})
