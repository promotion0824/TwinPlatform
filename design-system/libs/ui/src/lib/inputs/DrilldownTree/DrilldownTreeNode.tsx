import { useState } from 'react'
import { Icon } from '../../misc/Icon'
import {
  BaseTreeNodeProps,
  Name,
  Spacer,
  TreeNodeContainer,
  TreeNodeProps,
} from '../Tree'

export interface DrilldownTreeNodeProps extends BaseTreeNodeProps {
  selectedLevel: number
}

const onClick: TreeNodeProps['onClick'] = ({ node, onChange, onChangeIds }) => {
  if (node.isSelected) return
  node.tree.closeAll()
  node.select()
  node.open()
  onChange?.(node.tree.selectedNodes.map((node) => node.data))
  onChangeIds?.(Array.from(node.tree.selectedIds))
}

export const DrilldownTreeNode = ({
  node,
  onChange,
  onChangeIds,
  selectedLevel,
  style,
}: DrilldownTreeNodeProps) => {
  const [isHovered, setIsHovered] = useState(false)
  const { isAllItemsNode, level, name } = node.data

  const Prefix = () =>
    !isAllItemsNode && level !== undefined && level > selectedLevel ? (
      <Spacer />
    ) : null

  const Suffix = () =>
    isHovered || node.isSelected ? (
      <Icon
        c={node.isSelected ? 'intent.primary.fg.default' : 'state.disabled.fg'}
        icon="fiber_manual_record"
        size={16}
      />
    ) : (
      <Spacer />
    )

  return (
    <TreeNodeContainer
      $isSelected={node.isSelected}
      onClick={() => onClick({ node, onChange, onChangeIds })}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      style={style}
    >
      <Prefix />
      <Name>{name}</Name>
      <Suffix />
    </TreeNodeContainer>
  )
}
