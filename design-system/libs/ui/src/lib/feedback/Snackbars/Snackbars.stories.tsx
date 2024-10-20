import type { Meta, StoryObj } from '@storybook/react'
import { StoryFlexContainer } from '../../../storybookUtils'

import { Button } from '../../buttons/Button'
import { Intent } from '../../common'
import { Link } from '../../navigation/Link'
import { Snackbars, useSnackbar } from './index'

const meta: Meta<typeof Snackbars> = {
  title: 'Snackbars',
  component: Snackbars,
  decorators: [
    (Story) => (
      <StoryFlexContainer>
        <Snackbars />
        <Story />
      </StoryFlexContainer>
    ),
  ],
}
export default meta

type Story = StoryObj<typeof Snackbars>

export const Playground: Story = {
  render: () => {
    const snackbar = useSnackbar()
    return (
      <Button
        onClick={() =>
          snackbar.show({
            title: 'Snackbar title',
            description: 'Snackbar description',
            intent: 'primary',
          })
        }
      >
        Show snackbar
      </Button>
    )
  },
}

const allIntent: Intent[] = [
  'primary',
  'secondary',
  'positive',
  'negative',
  'notice',
]

export const ShowAllIntentSnackbars: Story = {
  render: () => {
    const snackbar = useSnackbar()
    return (
      <>
        <Button
          onClick={() => {
            allIntent.forEach((intent, index) => {
              setTimeout(() => {
                snackbar.show({
                  title: `Snackbar with ${intent} intent`,
                  intent,
                })
              }, 200 * index)
            })
          }}
        >
          Show snackbars
        </Button>

        <Button kind="negative" onClick={() => snackbar.clean()}>
          Clean all
        </Button>
      </>
    )
  },
}

export const ShowAllIntentWithDescription: Story = {
  render: () => {
    const snackbar = useSnackbar()
    return (
      <>
        <Button
          onClick={() => {
            allIntent.forEach((intent, index) => {
              setTimeout(() => {
                snackbar.show({
                  title: `Snackbar with ${intent} intent`,
                  description: 'Snackbar description',
                  intent,
                })
              }, 200 * index)
            })
          }}
        >
          Show snackbars
        </Button>

        <Button kind="negative" onClick={() => snackbar.clean()}>
          Clean all
        </Button>
      </>
    )
  },
}

export const ShowAllIntentWithActions: Story = {
  render: () => {
    const snackbar = useSnackbar()
    return (
      <>
        <Button
          onClick={() => {
            allIntent.forEach((intent, index) => {
              setTimeout(() => {
                snackbar.show({
                  title: `Snackbar with ${intent} intent`,
                  actions: <Link href="/">Action</Link>,
                  intent,
                })
              }, 200 * index)
            })
          }}
        >
          Show snackbars
        </Button>

        <Button kind="negative" onClick={() => snackbar.clean()}>
          Clean all
        </Button>
      </>
    )
  },
}

export const ShowAllIntentWithAllProps: Story = {
  render: () => {
    const snackbar = useSnackbar()
    return (
      <>
        <Button
          onClick={() => {
            allIntent.forEach((intent, index) => {
              setTimeout(() => {
                snackbar.show({
                  title: `Snackbar with ${intent} intent`,
                  description: 'Snackbar description',
                  actions: <Link href="/">Action</Link>,
                  intent,
                })
              }, 200 * index)
            })
          }}
        >
          Show snackbars
        </Button>

        <Button kind="negative" onClick={() => snackbar.clean()}>
          Clean all
        </Button>
      </>
    )
  },
}

export const ShowAllIntentSnackbarsInLoadingState: Story = {
  render: () => {
    const snackbar = useSnackbar()
    return (
      <Button
        onClick={() => {
          allIntent.forEach((intent, index) => {
            setTimeout(() => {
              snackbar.show({
                id: intent,
                title: `Snackbar with ${intent} intent`,
                intent,
                loading: true,
                autoClose: false,
                withCloseButton: false,
              })
            }, 200 * index)
          })

          setTimeout(() => {
            allIntent.forEach((intent) => {
              setTimeout(() => {
                snackbar.update({
                  id: intent,
                  title: `Snackbar with ${intent} intent`,
                  intent,
                  autoClose: 4000,
                  withCloseButton: true,
                })
              })
            })
          }, 6000)
        }}
      >
        Show snackbars
      </Button>
    )
  },
}

export const ShowAllIntentWithDescriptionInLoadingState: Story = {
  render: () => {
    const snackbar = useSnackbar()

    const handleSync = () => {
      allIntent.forEach((intent, index) => {
        setTimeout(() => {
          snackbar.show({
            id: intent,
            title: `Loading with ${intent} intent`,
            description: 'Please wait while we fetch the data.',
            actions: <Link href="/">Action</Link>,
            intent,
            loading: true,
            autoClose: false,
            withCloseButton: false,
          })
        }, 200 * index)
      })

      setTimeout(() => {
        allIntent.forEach((intent) => {
          setTimeout(() => {
            snackbar.update({
              id: intent,
              title: `Completed with ${intent} intent`,
              description: 'Data fetched successfully.',
              actions: <Link href="/">Action</Link>,
              intent,
              autoClose: 4000,
              withCloseButton: true,
            })
          })
        })
      }, 6000)
    }

    return <Button onClick={handleSync}>Show snackbars</Button>
  },
}
