import Icon from './Icon'
import 'twin.macro'
import * as icons from './icons'

const meta = {
  component: Icon,
  argTypes: {
    size: {
      defaultValue: 'medium',
      options: ['small', 'medium'],
      control: 'select',
    },
    color: {
      options: [
        'green',
        'greenDark',
        'yellow',
        'yellowDark',
        'orange',
        'orangeDark',
        'red',
        'redDark',
      ],
      control: 'select',
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
