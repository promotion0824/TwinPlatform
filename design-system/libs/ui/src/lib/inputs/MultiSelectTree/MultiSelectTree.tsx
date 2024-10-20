import { forwardRef } from 'react'
import { BaseTreeProps, Tree } from '../Tree'
import { MultiSelectTreeNode } from './MultiSelectTreeNode'

export interface MultiSelectTreeProps extends BaseTreeProps {}

export const MultiSelectTree = forwardRef<HTMLDivElement, MultiSelectTreeProps>(
  (props, ref) => {
    return <Tree ref={ref} renderTreeNode={MultiSelectTreeNode} {...props} />
  }
)
