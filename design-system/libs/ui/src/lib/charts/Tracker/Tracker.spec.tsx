import userEvent from '@testing-library/user-event'
import { Tracker } from '.'
import { render, screen, waitFor } from '../../../jest/testUtils'

function getTooltip() {
  return screen.getByRole('tooltip')
}

describe('Tracker', () => {
  it('should use the "intention" variant when number values are provided', () => {
    const { baseElement } = render(
      <Tracker data={[{ value: 50 }, { value: 75 }, { value: 100 }]} />
    )

    const blocks = baseElement.querySelectorAll('.mantine-Group-root div')

    expect(blocks[0].getAttribute('style')).toContain(
      'background: rgb(215, 117, 112)'
    )
    expect(blocks[1].getAttribute('style')).toContain(
      'background: rgb(224, 115, 51)'
    )
    expect(blocks[2].getAttribute('style')).toContain(
      'background: rgb(53, 166, 53)'
    )
  })

  it('should change the block colors based on the intention thresholds ', () => {
    const { baseElement } = render(
      <Tracker
        data={[{ value: 50 }, { value: 75 }, { value: 100 }]}
        intentThresholds={{ noticeThreshold: 50, positiveThreshold: 75 }}
      />
    )

    const blocks = baseElement.querySelectorAll('.mantine-Group-root div')

    expect(blocks[0].getAttribute('style')).toContain(
      'background: rgb(224, 115, 51)'
    )
    expect(blocks[1].getAttribute('style')).toContain(
      'background: rgb(53, 166, 53)'
    )
    expect(blocks[2].getAttribute('style')).toContain(
      'background: rgb(53, 166, 53)'
    )
  })

  it('should use the "status" variant when boolean values are provided', () => {
    const { baseElement } = render(
      <Tracker data={[{ value: true }, { value: true }, { value: false }]} />
    )

    const blocks = baseElement.querySelectorAll('.mantine-Group-root div')

    expect(blocks[0].getAttribute('style')).toContain(
      'background: rgb(155, 129, 230)'
    )
    expect(blocks[1].getAttribute('style')).toContain(
      'background: rgb(155, 129, 230)'
    )
    expect(blocks[2].getAttribute('style')).toContain(
      'background: rgb(145, 145, 145)'
    )
  })

  it('should show tooltips on hover when labels are provided', async () => {
    const { baseElement } = render(
      <Tracker
        data={[
          { label: 'Block 1', value: 50 },
          { label: 'Block 2', value: 75 },
          { label: 'Block 3', value: 100 },
        ]}
      />
    )

    const block = baseElement.querySelector('.mantine-Group-root div')
    await userEvent.hover(block as Element)

    await waitFor(() => expect(getTooltip()).toBeInTheDocument())
    expect(getTooltip()).toHaveTextContent('Block 1')
  })
})
