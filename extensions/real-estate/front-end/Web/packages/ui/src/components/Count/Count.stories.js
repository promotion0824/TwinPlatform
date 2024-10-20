import tw from 'twin.macro'
import Count from './Count'

const meta = {
  component: Count,
  argTypes: {
    value: {
      defaultValue: 10,
      control: 'number',
    },
  },
}

export default meta

export const Basic = {
  render: ({ value }) => (
    <div tw="inline-block relative padding[8px 16px]">
      My counter
      <Count>{value}</Count>
    </div>
  ),
}
