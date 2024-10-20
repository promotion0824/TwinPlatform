import { NodeApi } from 'react-arborist'

export interface TreeData {
  children?: TreeData[]
  id: string
  level?: number
  name: string
  parent?: TreeData
}

export interface InternalTreeData extends TreeData {
  isAllItemsNode?: boolean
}

export function deselectAllParents(node: NodeApi) {
  if (node.parent) {
    node.parent.deselect()
    deselectAllParents(node.parent)
  }
}

export function findNodeById(
  data: InternalTreeData[],
  id: string
): InternalTreeData | undefined {
  for (const node of data) {
    if (node.id === id) return node

    if (node.children) {
      const found = findNodeById(node.children, id)
      if (found) return found
    }
  }

  return undefined
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
