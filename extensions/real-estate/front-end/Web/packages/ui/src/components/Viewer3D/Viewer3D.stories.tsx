import type { Meta, StoryObj } from '@storybook/react'
import Viewer3D from './Viewer3D'

const meta: Meta<typeof Viewer3D> = {
  component: Viewer3D,
}

export default meta
type Story = StoryObj<typeof Viewer3D>

const urns = [
  'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNGU1ZmMyMjktZmZkOS00NjJhLTg4MmItMTZiNGE2M2IyYThhLXVhdC9BUkMtSU5UX0w2OF9CQl8yMDIxMDIwOTE0MTA1NS5ud2Q=',
  'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNGU1ZmMyMjktZmZkOS00NjJhLTg4MmItMTZiNGE2M2IyYThhLXVhdC9BUkMtRVhUX0w2OF9CQmFfMjAyMTAyMDkxNDEwNTQubndk',
]

// API: /forge/oauth/token
const token =
  'eyJhbGciOiJSUzI1NiIsImtpZCI6IlU3c0dGRldUTzlBekNhSzBqZURRM2dQZXBURVdWN2VhIn0.eyJzY29wZSI6WyJ2aWV3YWJsZXM6cmVhZCJdLCJjbGllbnRfaWQiOiJaT0VrcWhESFI4MFpLU3k1Z1NTMzN4UGlUOHBXNFJGSCIsImF1ZCI6Imh0dHBzOi8vYXV0b2Rlc2suY29tL2F1ZC9hand0ZXhwNjAiLCJqdGkiOiJYdWNMN05RZ1lHSUN4bUFVYkZvdFEzMWgza1IyZzdISmozM3hPN2dwYU5MZ3M2ZkcwaWxwMEJKdk9TSDhIUGs4IiwiZXhwIjoxNjUwOTEwOTkyfQ.f22TQAsIu3CBjBNqyAzjXPDKeYcjW6VtwfWiOGQg3t6vxiqU5bJ46gXx_TDotyOmoer6wIXa91nc2xEmUzs8fSO9kjKF0VEkwdYmovEMZV62oQNYKlsSHgyq7ALJQpmHgVM1qvF6kuLWQpIYeUSWFAJeFLQxNYuNqrKsJN2J2mSe5pXIu7-faPd_7sI5_6lVdCO5WprsLcADmgPEqftLkEGnaguljAXXKuC2fOnC166R5GHiJuZlMP-B1F6BWo6TAA3YlRQQKa_wROnJCC26g1F6LzRgvDeVHR4wjedUffVMHw12Hm-i4q-flG7gCwVt1vguy5fB8mfwJP6gQrfTjQ'

// model reference / floor dict
const layers = [
  {
    '02d19b7a-7976-46f3-9e69-23dd01ae6416': {
      priority: 1,
      floorCode: 'L1',
      name: 'L1',
    },
    '9fcb8dd2-b3f3-53e5-873f-179ec1ab1427': {
      priority: 2,
      floorCode: 'L2',
      name: 'L2',
    }, // no color appears
    '53f0cca5-104e-40bc-b49a-d0e294f7f4fb': {
      priority: 3,
      floorCode: 'L3',
      name: 'L3',
    },
    '53f0cca5-104e-40bc-b49a-d0e294f7ff49': {
      priority: 4,
      floorCode: 'L4',
      name: 'L4',
    },
    '058adc68-4073-4e31-a36c-5b01cf0052f5': {
      priority: 1,
      floorCode: 'L5',
      name: 'L5',
    },
    '058adc68-4073-4e31-a36c-5b01cf005477': {
      priority: 2,
      floorCode: 'L6',
      name: 'L6',
    },
    '0308c043-1e19-44d2-9b31-589ff70629bb': {
      priority: 3,
      floorCode: 'L7',
      name: 'L7',
    },
    'e9fb4b31-5829-4147-b7ef-3ef1490e6f87': {
      priority: 4,
      floorCode: 'L8',
      name: 'L8',
    },
    'e9fb4b31-5829-4147-b7ef-3ef1490e79fd': {
      priority: 1,
      floorCode: 'L9',
      name: 'L9',
    },
  },
]

function onClick({ guids }) {
  if (guids.length > 0) {
    window.alert(`User should be navigated to the floor page ${guids[0]}`)
  }
}

export const Basic: Story = {
  args: { urns, token, layers, onClick },
}
