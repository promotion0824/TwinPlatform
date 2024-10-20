import { kebabCase } from 'lodash'
import React from 'react'

type ToMantinePropsProps = {
  componentName?: string
  linkToMantineProps?: string
}

export const ToMantineProps = ({
  componentName,
  linkToMantineProps,
}: ToMantinePropsProps) => {
  const link = componentName
    ? // for example: TextInput to text-input
      `https://mantine.dev/core/${kebabCase(componentName)}/?t=props`
    : linkToMantineProps
  return (
    <div>
      <p>
        Should you require further customization not provided in our Props
        table, we suggest you to refer to{' '}
        <a href={link} target="_blank" rel="noreferrer">
          Mantine's documentation
        </a>{' '}
        for additional customization options. However, please be aware that
        using these properties is at your own risk, as they may be removed or
        altered in the future and are not maintained by us. We only assure the
        maintenance of the properties detailed in the above table. If you find
        the need for any of these properties, please raise a feature request to
        us, and we will consider adding them into our supported properties.
      </p>
    </div>
  )
}
