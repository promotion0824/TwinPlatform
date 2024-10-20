import React, { HTMLProps, ReactElement } from 'react'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import TwinChip from './TwinChip'

export default function TwinChip(
  props: HTMLProps & {
    variant: string
    modelOfInterest?: ModelOfInterest
    text?: string
    gappedText?: string
    icon?: ReactElement
    count?: number
    isSelected: boolean
    className?: string
    title?: string
    onClick?: () => void
    highlightOnHover?: boolean
    additionalInfo?: ReactElement[]
  }
): ReactElement
