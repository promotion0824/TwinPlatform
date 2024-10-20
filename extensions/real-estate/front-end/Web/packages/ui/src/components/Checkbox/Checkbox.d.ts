import React, { ReactNode, ReactElement } from 'react'

export default function Checkbox(props: {
  children?: ReactNode
  value: boolean
  readOnly?: boolean
  className?: string
  onChange?: () => void
  onClick?: (event?: React.MouseEvent) => void
}): ReactElement
