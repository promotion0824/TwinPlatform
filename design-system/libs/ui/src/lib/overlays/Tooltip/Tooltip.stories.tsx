import type { Meta, StoryObj } from '@storybook/react'
import styled, { css } from 'styled-components'
import { StoryFlexContainer } from '../../../storybookUtils'

import { Tooltip } from '.'
import { Button } from '../../buttons/Button'

const meta: Meta<typeof Tooltip> = {
  title: 'Tooltip',
  component: Tooltip,
}
export default meta

type Story = StoryObj<typeof Tooltip>

const SingleTooltipDecorator = (Story: React.ComponentType) => (
  <StoryFlexContainer
    css={css`
      height: 50px;
      align-items: end;
    `}
  >
    <Story />
  </StoryFlexContainer>
)

export const Playground: Story = {
  render: () => (
    <Tooltip label="Tooltip Content">
      <Button>Trigger</Button>
    </Tooltip>
  ),
  decorators: [SingleTooltipDecorator],
}

const StyledWrapper = styled.div`
  padding-top: 40px;
  margin: auto;
  width: fit-content;
  display: grid;
  grid-template-columns: 80px repeat(3, 150px) 100px;
  grid-template-rows: repeat(5, 60px);
  grid-template-areas:
    '. top-start top top-end .'
    'left-start . . . right-start'
    'left . . . right'
    'left-end . . . right-end'
    '. bottom-start bottom bottom-end .';
`

export const Placement: Story = {
  render: () => (
    <>
      {(
        [
          'top-start',
          'top',
          'top-end',
          'left-start',
          'left',
          'left-end',
          'right-start',
          'right',
          'right-end',
          'bottom-start',
          'bottom',
          'bottom-end',
        ] as const
      ).map((placement) => (
        <div
          key={placement}
          css={{
            gridArea: placement,
          }}
        >
          <Tooltip label="Tooltip Content" position={placement} opened>
            <Button>{placement}</Button>
          </Tooltip>
        </div>
      ))}
    </>
  ),
  decorators: [
    (Story) => (
      <StyledWrapper>
        <Story />
      </StyledWrapper>
    ),
  ],
}

export const PlacementWithArrow: Story = {
  render: () => (
    <>
      {(
        [
          'top-start',
          'top',
          'top-end',
          'left-start',
          'left',
          'left-end',
          'right-start',
          'right',
          'right-end',
          'bottom-start',
          'bottom',
          'bottom-end',
        ] as const
      ).map((placement) => (
        <div
          key={placement}
          css={{
            gridArea: placement,
          }}
        >
          <Tooltip
            withArrow
            label="Tooltip Content"
            position={placement}
            opened
          >
            <Button>{placement}</Button>
          </Tooltip>
        </div>
      ))}
    </>
  ),
  decorators: [
    (Story) => (
      <StyledWrapper>
        <Story />
      </StyledWrapper>
    ),
  ],
}

export const DefaultOpen: Story = {
  render: () => (
    <Tooltip label="Tooltip Content" opened>
      <Button>Trigger</Button>
    </Tooltip>
  ),
  decorators: [SingleTooltipDecorator],
}

export const TriggerOnFocus: Story = {
  render: () => (
    <Tooltip
      label="Tooltip Content"
      events={{ hover: false, focus: true, touch: false }}
    >
      <Button>Trigger</Button>
    </Tooltip>
  ),
  decorators: [SingleTooltipDecorator],
}

export const Disabled: Story = {
  render: () => (
    <Tooltip label="Tooltip Content" disabled>
      <Button>Trigger</Button>
    </Tooltip>
  ),
}

export const Multiline: Story = {
  render: () => (
    <Tooltip
      multiline
      width={220}
      label="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
    >
      <Button>Trigger</Button>
    </Tooltip>
  ),
  decorators: [
    (Story) => (
      <StoryFlexContainer
        css={css`
          height: 120px;
          align-items: end;
        `}
      >
        <Story />
      </StoryFlexContainer>
    ),
  ],
}

export const Inline: Story = {
  render: () => (
    <p css={{ color: 'white' }}>
      Lorem ipsum dolor sit amet, consectetur adipiscing elit,{' '}
      <Tooltip label="Tooltip Content" inline>
        <mark>inline trigger</mark>
      </Tooltip>{' '}
      sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
    </p>
  ),
}
