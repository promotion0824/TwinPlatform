import { titleCase } from '@willow/common'
import { TooltipWhenTruncated } from '@willow/ui'
import { Icon, IconButton } from '@willowinc/ui'
import { CSSProperties, useEffect, useRef } from 'react'
import { NodeApi, RowRendererProps, Tree, TreeApi } from 'react-arborist'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

import getScopeSelectorModel from './getScopeSelectorModel'
import type { LocationNode } from './ScopeSelector'
import ScopeSelectorAvatar from './ScopeSelectorAvatars'

type TreeVariant = 'locations' | 'portfolios' | 'search'

const MAX_ROWS_DISPLAYED = 10 as const
const ROW_HEIGHT = 36 as const

function selectNode(node: NodeApi<LocationNode>) {
  node.select()
  node.activate()
}

function countChildren(node: LocationNode, countAllNodes = false): number {
  // Unless countAllNodes is set to true, only count nodes whose
  // models have the includeInCounts property
  const count =
    countAllNodes ||
    getScopeSelectorModel(node.twin.metadata.modelId).includeInCounts
      ? 1
      : 0

  return node.children
    ? node.children.reduce(
        (acc, child) => acc + countChildren(child, countAllNodes),
        count
      )
    : count
}

const ClickableRegion = styled.div<{ $variant: TreeVariant }>(
  ({ theme, $variant }) => ({
    alignItems: 'center',
    display: 'flex',
    gap: theme.spacing.s8,
    minWidth: 0,
    paddingRight: theme.spacing.s8,
    width: '100%',

    ...(($variant === 'portfolios' || $variant === 'search') && {
      paddingLeft: theme.spacing.s8,
    }),
  })
)

const Details = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  marginLeft: 'auto',
  whiteSpace: 'nowrap',
}))

const Parents = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.muted,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))

const SiteName = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))

const SiteNameContainer = styled.div({
  minWidth: 0,
  width: '100%',
})

const Spacer = styled.div({
  flexShrink: 0,
  width: '28px',
})

const StyledTree = styled(Tree)(({ theme }) => ({
  'div[role="treeitem"]': {
    '&:focus-visible': {
      outline: `1px solid ${theme.color.state.focus.border}`,
      outlineOffset: '-1px',
    },
  },
}))

const TreeNode = styled.div<{ $isSelected: boolean }>(
  ({ $isSelected, theme }) => ({
    alignItems: 'center',
    backgroundColor: theme.color.neutral.bg.panel.default,
    borderRadius: theme.radius.r4,
    color: theme.color.neutral.fg.default,
    display: 'flex',
    gap: theme.spacing.s4,
    height: ROW_HEIGHT,
    padding: `${theme.spacing.s4} 0`,

    ...($isSelected && {
      backgroundColor: theme.color.neutral.bg.panel.activated,
    }),

    ':hover': {
      backgroundColor: theme.color.neutral.bg.panel.hovered,
    },
  })
)

function Node({
  node,
  selectedLocation,
  style,
  variant,
}: {
  node: NodeApi<LocationNode>
  selectedLocation: LocationNode
  style: CSSProperties
  variant: TreeVariant
}) {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const locationCount = countChildren(node.data)

  return (
    <TreeNode $isSelected={node.id === selectedLocation?.twin.id} style={style}>
      {variant === 'locations' &&
        (node.children?.length ? (
          <IconButton
            background="transparent"
            kind="secondary"
            onClick={() => node.toggle()}
          >
            <Icon icon={node.isOpen ? 'arrow_drop_down' : 'arrow_right'} />
          </IconButton>
        ) : (
          <Spacer />
        ))}

      <ClickableRegion
        onClick={() => selectNode(node)}
        role="button"
        $variant={variant}
      >
        <ScopeSelectorAvatar modelId={node.data.twin.metadata.modelId} />

        <SiteNameContainer>
          <TooltipWhenTruncated label={node.data.twin.name}>
            <SiteName>{node.data.twin.name}</SiteName>
          </TooltipWhenTruncated>
          {variant === 'search' && (
            <Parents>{node.data.parents?.join(' / ')}</Parents>
          )}
        </SiteNameContainer>

        <Details>
          {node.children?.length
            ? titleCase({
                language,
                text: t('plainText.locationsWithCount', {
                  count: locationCount,
                }),
              })
            : titleCase({
                language,
                text: t(
                  getScopeSelectorModel(node.data.twin.metadata.modelId).name
                ),
              })}
        </Details>
      </ClickableRegion>
    </TreeNode>
  )
}

// The default row renderer without the global onClick handler.
function RowRenderer<T>({ attrs, children, innerRef }: RowRendererProps<T>) {
  return (
    <div
      {...attrs}
      onFocus={(e) => e.stopPropagation()}
      onKeyPress={() => selectNode(children.props.node)}
      ref={innerRef}
      role="treeitem"
      tabIndex={0}
    >
      {children}
    </div>
  )
}

export default function ScopeSelectorTree({
  data,
  onSelect,
  searchTerm,
  selectedLocation,
  variant = 'locations',
}: {
  data: LocationNode[]
  onSelect: (selectedLocation: LocationNode) => void
  searchTerm?: string
  selectedLocation: LocationNode
  variant?: TreeVariant
}) {
  const treeRef = useRef<TreeApi<LocationNode>>()

  const totalNodes = data.reduce(
    (acc, node) => acc + countChildren(node, true),
    0
  )

  const height =
    variant === 'portfolios'
      ? data.length * ROW_HEIGHT
      : totalNodes >= MAX_ROWS_DISPLAYED
      ? MAX_ROWS_DISPLAYED * ROW_HEIGHT
      : totalNodes * ROW_HEIGHT

  useEffect(() => {
    const tree = treeRef.current

    // Only scroll to the selected location in the portfolio tree
    // if the selected model is a portfolio.
    if (
      variant !== 'portfolios' ||
      selectedLocation.twin.metadata.modelId ===
        'dtmi:com:willowinc:Portfolio;1'
    ) {
      tree?.scrollTo(selectedLocation.twin.id)
    }
  }, [
    selectedLocation.twin.id,
    selectedLocation.twin.metadata.modelId,
    variant,
  ])

  return (
    <StyledTree
      data={data}
      idAccessor={(node: LocationNode) => node.twin.id}
      disableMultiSelection
      height={height}
      indent={12}
      onSelect={(nodes: NodeApi<LocationNode>[]) => {
        if (!nodes.length) return
        onSelect(nodes[0].data)
      }}
      openByDefault={false}
      renderRow={RowRenderer}
      ref={treeRef}
      rowHeight={ROW_HEIGHT}
      searchMatch={(node: NodeApi<LocationNode>, term: string) =>
        node.data.twin.name.toLowerCase().includes(term.toLowerCase())
      }
      searchTerm={searchTerm}
      width="100%"
    >
      {({ node, style }) => (
        <Node
          node={node as NodeApi<LocationNode>}
          selectedLocation={selectedLocation}
          style={style}
          variant={variant}
        />
      )}
    </StyledTree>
  )
}
