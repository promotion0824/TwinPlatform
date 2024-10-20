import Icon from './Icon'
import 'twin.macro'
import * as icons from './icons'

const meta = {
  component: Icon,
  argTypes: {
    size: {
      defaultValue: 'medium',
      options: ['extraTiny', 'tiny', 'small', 'medium', 'large', 'xxl'],
      control: 'select',
    },
    color: {
      options: [
        'purple',
        'red',
        'redGraph',
        'orange',
        'yellow',
        'yellowGraph',
        'green',
        'greenGraph',
        'white',
        'dark',
        'darkGraph',
      ],
      control: 'select',
    },
    className: {
      table: { disable: true },
    },
    style: {
      table: { disable: true },
    },
  },
}

export default meta

const iconNames = Object.keys(icons).sort()

export const AllIcons = {
  render: (args) => (
    <div tw="flex flex-wrap gap-6 justify-content[space-between]">
      {iconNames.map((icon) => (
        <div key={icon} tw="text-center" alt={icon}>
          <Icon icon={icon} {...args} />
          <div>{icon}</div>
        </div>
      ))}
    </div>
  ),
}
