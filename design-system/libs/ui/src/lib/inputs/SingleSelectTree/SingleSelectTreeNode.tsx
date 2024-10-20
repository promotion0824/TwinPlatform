import { Icon } from '../../misc/Icon'
import { BaseTreeNodeProps, TreeNode, TreeNodeProps } from '../Tree'

export interface SingleSelectTreeNodeProps extends BaseTreeNodeProps {}

const onClick: TreeNodeProps['onClick'] = ({ node, onChange, onChangeIds }) => {
  node.select()
  onChange?.(node.tree.selectedNodes.map((node) => node.data))
  onChangeIds?.(Array.from(node.tree.selectedIds))
}

const renderCountIndicator = () => (
  <Icon c={'state.disabled.fg'} icon="fiber_manual_record" size={16} />
)

export const SingleSelectTreeNode = (props: SingleSelectTreeNodeProps) => {
  return (
    <TreeNode
      onClick={onClick}
      renderCountIndicator={renderCountIndicator}
      suffixIcon="fiber_manual_record"
      suffixIconSize={16}
      {...props}
    />
  )
}
