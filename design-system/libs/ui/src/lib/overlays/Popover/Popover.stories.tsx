import type { Meta, StoryObj } from '@storybook/react'
import { StoryFlexContainer } from '../../../storybookUtils'

import { useState } from 'react'
import styled, { css } from 'styled-components'

import { Popover } from '.'
import { Button } from '../../buttons/Button'
import { useDisclosure } from '../../hooks'

const meta: Meta<typeof Popover> = {
  title: 'Popover',
  component: Popover,
}
export default meta

type Story = StoryObj<typeof Popover>

const SinglePopoverDecorator = (Story: React.ComponentType) => (
  <StoryFlexContainer
    css={css`
      height: 50px;
    `}
  >
    <Story />
  </StoryFlexContainer>
)

export const Playground: Story = {
  render: () => {
    return (
      <Popover>
        <Popover.Target>
          <Button>Toggle popover</Button>
        </Popover.Target>
        <Popover.Dropdown>
          <div
            css={css(({ theme }) => ({
              padding: theme.spacing.s8,
            }))}
          >
            This is the content of the popover
          </div>
        </Popover.Dropdown>
      </Popover>
    )
  },
  decorators: [SinglePopoverDecorator],
}

export const DefaultOpen: Story = {
  render: () => (
    <Popover defaultOpened>
      <Popover.Target>
        <Button>Toggle popover</Button>
      </Popover.Target>
      <Popover.Dropdown>
        <div
          css={css(({ theme }) => ({
            padding: theme.spacing.s8,
          }))}
        >
          This is the content of the popover
        </div>
      </Popover.Dropdown>
    </Popover>
  ),
  decorators: [SinglePopoverDecorator],
}
export const WithArrow: Story = {
  render: () => {
    return (
      <Popover withArrow>
        <Popover.Target>
          <Button>Toggle popover</Button>
        </Popover.Target>
        <Popover.Dropdown>
          <div
            css={css(({ theme }) => ({
              padding: theme.spacing.s8,
            }))}
          >
            This is the content of the popover
          </div>
        </Popover.Dropdown>
      </Popover>
    )
  },
  decorators: [SinglePopoverDecorator],
}

const StyledWrapper = styled.div`
  padding-top: 100px;
  margin: auto;
  height: 450px;
  width: fit-content;
  display: grid;
  justify-items: center;
  grid-template-columns: 100px repeat(3, 150px) 100px;
  grid-template-rows: repeat(5, 80px);
  grid-template-areas:
    '. top-start top top-end .'
    'left-end . . . right-end'
    'left . . . right'
    'left-start . . . right-start'
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
          <Popover width={100} position={placement} withArrow opened>
            <Popover.Target>
              <Button>{placement}</Button>
            </Popover.Target>
            <Popover.Dropdown>
              <div
                css={css(({ theme }) => ({
                  padding: theme.spacing.s8,
                }))}
              >
                This is the content of the popover
              </div>
            </Popover.Dropdown>
          </Popover>
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

export const Disabled: Story = {
  render: () => (
    <Popover disabled>
      <Popover.Target>
        <Button>Toggle popover</Button>
      </Popover.Target>
      <Popover.Dropdown>This is the content of the popover</Popover.Dropdown>
    </Popover>
  ),
}

export const Controlled: Story = {
  render: () => {
    const [opened, setOpened] = useState(false)

    return (
      <Popover
        opened={opened}
        onChange={setOpened}
        onClose={() => console.log('closed')}
      >
        <Popover.Target>
          <Button onClick={() => setOpened((open) => !open)}>
            Toggle popover
          </Button>
        </Popover.Target>
        <Popover.Dropdown>
          <div
            css={css(({ theme }) => ({
              padding: theme.spacing.s8,
            }))}
          >
            This is the content of the popover
          </div>
        </Popover.Dropdown>
      </Popover>
    )
  },
  decorators: [SinglePopoverDecorator],
}

export const ControlledWithMouseEvent: Story = {
  render: () => {
    // import { useDisclosure } from '@willowinc/ui'
    const [opened, { close, open }] = useDisclosure(false)

    return (
      <Popover opened={opened} onClose={() => console.log('closed')}>
        <Popover.Target>
          <Button onMouseEnter={open} onMouseLeave={close}>
            Hover Over Trigger
          </Button>
        </Popover.Target>
        <Popover.Dropdown>
          <div
            css={css(({ theme }) => ({
              padding: theme.spacing.s8,
            }))}
          >
            This is the content of the popover
          </div>
        </Popover.Dropdown>
      </Popover>
    )
  },
  decorators: [SinglePopoverDecorator],
}
