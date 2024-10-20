import { forwardRef } from 'react'
import { BaseTreeProps, Tree } from '../Tree'
import { SingleSelectTreeNode } from './SingleSelectTreeNode'

export interface SingleSelectTreeProps extends BaseTreeProps {}

export const SingleSelectTree = forwardRef<
  HTMLDivElement,
  SingleSelectTreeProps
>((props, ref) => {
  return <Tree ref={ref} renderTreeNode={SingleSelectTreeNode} {...props} />
})
