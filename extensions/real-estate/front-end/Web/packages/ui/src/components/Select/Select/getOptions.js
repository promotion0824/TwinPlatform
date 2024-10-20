import { Children } from 'react'

export default function getOptions(children) {
  return Children.map(children, (child) => ({
    key:
      child?.props?.value === undefined
        ? child?.props?.children
        : child.props.value,
    value: child?.props?.children,
  }))
}
