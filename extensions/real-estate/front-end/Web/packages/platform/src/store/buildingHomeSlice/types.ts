import { ReactNode } from 'react'
import { WidgetId } from './widgetId'

export type WidgetCardMap<T extends ReactNode> = Record<
  Exclude<WidgetId, WidgetId.Location>,
  {
    useTitle: () => string
    description: string
    imageSrc: string
    defaultHeight: number
    component: T
  }
>

export type WidgetLayout = Array<Array<{ id: string }>>
