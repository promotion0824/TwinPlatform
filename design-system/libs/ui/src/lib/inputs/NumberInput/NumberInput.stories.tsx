import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'

import { NumberInput } from '.'
import { Icon } from '../../misc/Icon'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof NumberInput> = {
  title: 'NumberInput',
  component: NumberInput,
  args: {
    defaultValue: 0,
  },
}
export default meta

type Story = StoryObj<typeof NumberInput>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
}

export const Label: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: 'Number of Items',
  },
}

export const Description: Story = {
  ...storybookAutoSourceParameters,
  args: {
    description: 'A count of your items.',
    label: 'Number of Items',
  },
}

export const Placeholder: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: undefined,
    placeholder: 'Please enter a number',
  },
}

export const Required: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: 'Number of Items',
    required: true,
  },
}

export const Invalid: Story = {
  ...storybookAutoSourceParameters,
  args: {
    error: true,
  },
}

export const Error: Story = {
  ...storybookAutoSourceParameters,
  args: {
    error: 'Something went wrong',
  },
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Number of Items',
    error: 'Something went wrong',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Number of Items',
    labelWidth: 300,
    error: 'Something went wrong',
  },
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    disabled: true,
  },
}

export const ReadOnly: Story = {
  ...storybookAutoSourceParameters,
  args: {
    readOnly: true,
    value: 50,
  },
}

export const Controlled: Story = {
  render: () => {
    const [value, setValue] = useState<number | string>(0)
    return <NumberInput onChange={setValue} value={value} />
  },
}

export const MinAndMax: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: undefined,
    min: 10,
    max: 20,
    placeholder: 'Accepts values between 10 and 20',
  },
}

export const ClampBehavior: Story = {
  args: {
    clampBehavior: 'none',
    defaultValue: undefined,
    min: 10,
    max: 20,
    placeholder: 'Accepts values between 10 and 20',
  },
  parameters: {
    docs: {
      description: {
        story:
          'By default, the value is clamped when the input is blurred. If you set `clampBehavior="strict"`, it will not be possible to enter value outside of min/max range. Note that this option may cause issues if you have tight `min` and `max`, for example `min={10}` and `max={20}`. If you need to disable value clamping entirely, set `clampBehavior="none"`.',
      },
      source: {
        type: 'auto',
      },
    },
  },
}

export const Prefix: Story = {
  ...storybookAutoSourceParameters,
  args: {
    prefix: <Icon icon="info" />,
  },
}

export const Suffix: Story = {
  args: {
    suffix: <Icon icon="info" />,
  },
  parameters: {
    docs: {
      description: {
        story: 'Note that using a suffix will hide the controls.',
      },
      source: {
        type: 'auto',
      },
    },
  },
}

export const TextPrefix: Story = {
  ...storybookAutoSourceParameters,
  args: {
    textPrefix: '$',
  },
}

export const TextSuffix: Story = {
  ...storybookAutoSourceParameters,
  args: {
    textSuffix: '%',
  },
}

export const AllowNegative: Story = {
  args: {
    allowNegative: false,
  },
  parameters: {
    docs: {
      description: {
        story:
          'By default, negative numbers are allowed. Set `allowNegative={false}` to only allow positive numbers.',
      },
      source: {
        type: 'auto',
      },
    },
  },
}

export const AllowDecimal: Story = {
  args: {
    allowDecimal: false,
  },
  parameters: {
    docs: {
      description: {
        story:
          'By default, decimal numbers are allowed. Set `allowDecimal={false}` to only allow whole numbers.',
      },
      source: {
        type: 'auto',
      },
    },
  },
}

export const DecimalScale: Story = {
  args: {
    decimalScale: 2,
    defaultValue: 0.25,
  },
  parameters: {
    docs: {
      description: {
        story:
          '`decimalScale` controls how many decimal places can be entered.',
      },
      source: {
        type: 'auto',
      },
    },
  },
}

export const FixedDecimalScale: Story = {
  args: {
    decimalScale: 2,
    defaultValue: 0.2,
    fixedDecimalScale: true,
  },
  parameters: {
    docs: {
      description: {
        story:
          'Use `fixedDecimalScale` to always display the specified number of decimal places.',
      },
      source: {
        type: 'auto',
      },
    },
  },
}
