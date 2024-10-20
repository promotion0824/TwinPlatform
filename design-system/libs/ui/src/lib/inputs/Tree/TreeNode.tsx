import { CSSProperties, ReactElement, useState } from 'react'
import { NodeApi } from 'react-arborist'
import styled from 'styled-components'
import { IconButton } from '../../buttons/Button'
import { Icon, IconName, IconProps } from '../../misc/Icon'
import { InternalTreeData, TreeData, flattenTree } from './treeUtils'

export interface TreeNodeProps {
  node: NodeApi<InternalTreeData>
  onChange?: (nodes: TreeData[]) => void
  onChangeIds?: (nodeIds: string[]) => void
  /** Function to the called when the node is clicked on. */
  onClick: ({
    node,
    onChange,
    onChangeIds,
  }: {
    node: NodeApi<InternalTreeData>
    onChange: TreeNodeProps['onChange']
    onChangeIds: TreeNodeProps['onChangeIds']
  }) => void
  /**
   * Called when an element is needed to be rendered when one or more child nodes
   * have been selected but their parent node has been collapsed. The count of the
   * selected children is passed to this function.
   */
  renderCountIndicator: (selectedChildrenCount: number) => ReactElement
  /** Icon displayed when nodes are selected or hovered. */
  suffixIcon: IconName
  /**
   * The size of the suffix icon.
   * @default 20
   */
  suffixIconSize?: IconProps['size']
  style: CSSProperties
}

export interface BaseTreeNodeProps
  extends Omit<
    TreeNodeProps,
    'onClick' | 'renderCountIndicator' | 'suffixIcon' | 'suffixIconSize'
  > {}

export const Name = styled.div({
  overflowX: 'hidden',
  textOverflow: 'ellipsis',
  textWrap: 'nowrap',
  width: '100%',
})

export const Spacer = styled.div(({ theme }) => ({
  flexShrink: 0,
  width: theme.spacing.s20,
}))

const ToggleButton = styled(IconButton)({
  padding: 0,
  flexShrink: 0,
})

export const TreeNodeContainer = styled.div<{ $isSelected: boolean }>(
  ({ $isSelected, theme }) => ({
    ...theme.font.body.md.regular,

    alignItems: 'center',
    color: theme.color.neutral.fg.muted,
    cursor: 'pointer',
    display: 'flex',
    height: '100%',
    userSelect: 'none',

    '&:hover': {
      color: theme.color.neutral.fg.default,
    },

    ...($isSelected && {
      color: theme.color.neutral.fg.default,
    }),
  })
)

export const TreeNode = ({
  node,
  onChange,
  onChangeIds,
  onClick,
  renderCountIndicator,
  suffixIcon,
  suffixIconSize,
  style,
  ...restProps
}: TreeNodeProps) => {
  const [isHovered, setIsHovered] = useState(false)

  const { children, isAllItemsNode, name } = node.data

  const hasChildren = children?.length
  const allChildrenIds = flattenTree(children || []).map((child) => child.id)
  const selectedChildrenCount = Array.from(node.tree.selectedIds).filter((id) =>
    allChildrenIds.includes(id)
  ).length
  const notAllChildrenSelected =
    children?.length &&
    selectedChildrenCount > 0 &&
    selectedChildrenCount < allChildrenIds?.length

  const Prefix = () =>
    children?.length ? (
      <ToggleButton
        background="transparent"
        kind="secondary"
        onClick={(event) => {
          event.stopPropagation()
          node.toggle()
        }}
        tabIndex={-1}
        icon={node.isOpen ? 'arrow_drop_down' : 'arrow_right'}
      />
    ) : (
      <Spacer />
    )

  const Suffix = () =>
    isHovered || node.isSelected ? (
      <Icon
        c={node.isSelected ? 'intent.primary.fg.default' : 'state.disabled.fg'}
        icon={suffixIcon}
        size={suffixIconSize}
      />
    ) : hasChildren && !node.isOpen && notAllChildrenSelected ? (
      renderCountIndicator(selectedChildrenCount)
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
      {...restProps}
    >
      {!isAllItemsNode && <Prefix />}
      <Name>{name}</Name>
      <Suffix />
    </TreeNodeContainer>
  )
}
