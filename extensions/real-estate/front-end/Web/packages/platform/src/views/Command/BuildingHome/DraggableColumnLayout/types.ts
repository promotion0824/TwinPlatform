import { ForwardRefExoticComponent, RefAttributes } from 'react'

export type Cols = Record<number, number>
export type DraggableItem = {
  id: string
}
export type Column = {
  id: string
  items: DraggableItem[]
}
export type Layout = Column[]

export type DraggableContent<T = string> = ForwardRefExoticComponent<
  {
    draggableRef: React.RefObject<HTMLButtonElement>
    canDrag: boolean
    isEditingMode: boolean
    id: T
  } & RefAttributes<HTMLDivElement>
>

export type ComponentMap = Record<
  string,
  {
    defaultHeight: number
    component: DraggableContent
  }
>
