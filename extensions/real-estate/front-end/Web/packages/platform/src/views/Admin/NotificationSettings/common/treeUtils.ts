import { NodeApi } from 'react-arborist'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'

export interface InternalTreeData extends LocationNode {
  isAllItemsNode?: boolean
}

export function deselectAllParents(node: NodeApi) {
  if (node.parent) {
    node.parent.deselect()
    deselectAllParents(node.parent)
  }
}

export function flattenTree(data: InternalTreeData[]): InternalTreeData[] {
  return data.reduce<InternalTreeData[]>((acc, node) => {
    const { children, ...withoutChildren } = node

    if (children) {
      return [...acc, { ...withoutChildren }, ...flattenTree(children)]
    }

    return [...acc, { ...withoutChildren }]
  }, [])
}

export function getAllChildSiteTwinIds(
  node: NodeApi<InternalTreeData>
): string[] {
  if (node.children?.length === 0) {
    return node.data.twin.siteId ? [node.data.twin.id] : []
  }

  return (node.children ?? []).reduce<string[]>(
    (acc, child) => [...acc, ...getAllChildSiteTwinIds(child)],
    []
  )
}

export function isAllLeafsSelected(node: NodeApi<InternalTreeData>): boolean {
  if (node.children?.length === 0) {
    return node.isSelected
  }

  return (node.children ?? []).every((child) => isAllLeafsSelected(child))
}

export function getAllItemsNode(
  node: NodeApi<InternalTreeData>
): NodeApi<InternalTreeData> | undefined {
  const firstNode = node.tree.root.children?.[0]
  return firstNode?.data.isAllItemsNode ? firstNode : undefined
}

/**
 * Check all of the siblings from the provided node. If they are all selected,
 * select the parent node and repeat from the parent.
 * @param node The node to start checking the siblings from.
 */
export function synchronizeParents(node: NodeApi) {
  if (!node.parent) return
  const siblings = node.parent?.children ?? []

  if (siblings.every((sibling) => sibling.isSelected)) {
    node.parent.selectMulti()
    synchronizeParents(node.parent)
  }
}
