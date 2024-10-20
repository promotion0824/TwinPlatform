import { Badge } from '../../data-display/Badge'
import { Icon } from '../../misc/Icon'
import { BaseTreeNodeProps, TreeNode, TreeNodeProps } from '../Tree'
import {
  deselectAllParents,
  flattenTree,
  getAllItemsNode,
  synchronizeParents,
} from '../Tree/treeUtils'

export interface MultiSelectTreeNodeProps extends BaseTreeNodeProps {}

const onClick: TreeNodeProps['onClick'] = ({ node, onChange, onChangeIds }) => {
  const { children, isClosed, isSelected } = node
  const hasChildren = children?.length
  const allItemsNode = getAllItemsNode(node)

  if (node.data.isAllItemsNode) {
    // All other nodes use multi select, but the "All Items" node should
    // cause all other nodes to be deselected. This node should not be able
    // to be deselected when being clicked again.
    node.select()
  } else {
    // Deselect the All Items node before proceeding (if selected) so that this
    // change is reflected in any further select events.
    if (allItemsNode && node.tree.selectedIds.has(allItemsNode.id)) {
      node.tree.deselect(allItemsNode.id)
    }

    if (hasChildren) {
      const existingSelectionIds = Array.from(node.tree.selectedIds)
      const branchIds = flattenTree([node.data]).map((node) => node.id)

      if (isClosed) {
        // If a parent node is closed, open it and select all nodes.
        node.open()
        node.tree.setSelection({
          ids: Array.from(new Set([...existingSelectionIds, ...branchIds])),
          anchor: null,
          mostRecent: null,
        })
        synchronizeParents(node)
      } else if (isSelected) {
        // If a parent node is open and selected, close it and deselect all nodes.
        node.close()
        node.tree.setSelection({
          ids: existingSelectionIds.filter((id) => !branchIds.includes(id)),
          anchor: null,
          mostRecent: null,
        })
        deselectAllParents(node)
      } else {
        // If a parent node is open and not selected, select all nodes.
        node.tree.setSelection({
          ids: Array.from(new Set([...existingSelectionIds, ...branchIds])),
          anchor: null,
          mostRecent: null,
        })
        synchronizeParents(node)
      }
    } else if (isSelected) {
      node.deselect()
      deselectAllParents(node)
    } else {
      node.selectMulti()
      synchronizeParents(node)
    }
  }

  // If the All Items node is provided, automatically select it when every other node
  // has been deselected.
  if (allItemsNode && node.tree.selectedIds.size === 0) {
    node.tree.select(allItemsNode.id)
  }

  onChange?.(node.tree.selectedNodes.map((node) => node.data))
  onChangeIds?.(Array.from(node.tree.selectedIds))
}

const renderCountIndicator = (selectedChildrenCount: number) => (
  <Badge
    bg="intent.secondary.bg.subtle.default"
    c="core.gray.fg.default"
    prefix={<Icon icon="check" />}
    style={{ flexShrink: 0 }}
  >
    {selectedChildrenCount}
  </Badge>
)

export const MultiSelectTreeNode = (props: MultiSelectTreeNodeProps) => {
  return (
    <TreeNode
      onClick={onClick}
      renderCountIndicator={renderCountIndicator}
      suffixIcon="check"
      {...props}
    />
  )
}
