/* eslint-disable complexity */
import { CSSProperties } from 'react'
import { NodeApi } from 'react-arborist'
import styled from 'styled-components'
import { IconButton, Icon, Button, Group } from '@willowinc/ui'
import 'twin.macro'
import { useTranslation, TFunction } from 'react-i18next'
import { titleCase } from '@willow/common'
import { getModelInfo } from '@willow/common/twins/utils'
import ScopeSelectorAvatar from '@willow/ui/components/ScopeSelector/ScopeSelectorAvatars'
import { TooltipWhenTruncated } from '@willow/ui'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { Ontology } from '@willow/common/twins/view/models'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { InternalTreeData, isAllLeafsSelected } from './treeUtils'
import { theme } from 'twin.macro'

export interface TreeNodeProps {
  node: NodeApi<InternalTreeData>
  onChange?: (nodes: LocationNode[]) => void
  onChangeIds?: (nodeIds: string[]) => void
  /** Function to the called when the node is clicked on. */
  onClick: ({
    buttonName,
    node,
    onChange,
    onChangeIds,
    t,
  }: {
    buttonName: string
    node: NodeApi<InternalTreeData>
    onChange: TreeNodeProps['onChange']
    onChangeIds: TreeNodeProps['onChangeIds']
    t?: TFunction
  }) => void
  style: CSSProperties
  treeType?: string
  allLocations?: LocationNode
  ontology?: Ontology
  modelsOfInterest?: ModelOfInterest[]
  isViewOnly?: boolean
}

const SiteName = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))

const ClickableRegion = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s8,
  minWidth: 0,
  paddingRight: theme.spacing.s8,
  width: '100%',
}))

const Spacer = styled.div({
  flexShrink: 0,
  width: '28px',
})

const StyledButton = styled(Button)({
  '& *:hover': {
    backgroundColor: 'transparent',
  },
})

const ToggleButton = styled(IconButton)({
  flexShrink: 0,
})

const TreeNodeContainer = styled.div<{ $isSelected: boolean }>(
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
  onClick,
  onChange,
  onChangeIds,
  style,
  treeType,
  allLocations,
  ontology,
  modelsOfInterest,
  isViewOnly = false,
  ...restProps
}: TreeNodeProps) => {
  const { children, twin } = node.data
  const translation = useTranslation()
  const {
    i18n: { language },
    t,
  } = translation

  const model = ontology?.getModelById(node.data.twin.metadata.modelId)

  const modelInfo =
    model && ontology && modelsOfInterest
      ? getModelInfo(model, ontology, modelsOfInterest, translation)
      : undefined

  // display "All Locations" Button only for LocationReport Modal
  const isAllLocations =
    treeType === 'locationReportTree'
      ? node.id === allLocations?.twin.id
        ? true
        : !!(node.children && node.children.length > 0)
      : false

  const isAllLeafs = isAllLocations
    ? node.data.isAllItemsNode
      ? node.tree.visibleNodes.every((item) => isAllLeafsSelected(item))
      : isAllLeafsSelected(node)
    : false

  const Prefix = () =>
    children?.length ? (
      <ToggleButton
        background="transparent"
        kind="secondary"
        onClick={(event) => {
          event.stopPropagation()
          node.toggle()
        }}
        icon={node.isOpen ? 'arrow_drop_down' : 'arrow_right'}
      />
    ) : (
      <Spacer />
    )

  const Suffix = () => (
    <>
      {treeType === 'locationReportTree' && (
        <Group
          w="200px"
          mr="s16"
          css={({ theme }) => ({
            justifyContent: 'flex-end',
            color: theme.color.neutral.fg.muted,
            ...theme.font.body.xs.regular,
          })}
        >
          {titleCase({
            text: modelInfo?.displayName ?? t('headers.portfolio'),
            language,
          })}
        </Group>
      )}

      <StyledButton
        css={{
          flexGrow: 1,
          visibility: isAllLocations ? 'visible' : 'hidden',
        }}
        mr="s16"
        kind="secondary"
        onClick={() =>
          onClick({
            buttonName: t('headers.allLocations'),
            node,
            onChange,
            onChangeIds,
            t,
          })
        }
        prefix={<Icon icon={isAllLeafs ? 'remove' : 'add'} />}
        suffix={
          <Icon
            icon="apartment"
            css={({ theme }) => ({
              color: theme.color.core.purple.fg.default,
            })}
          />
        }
      >
        {titleCase({ text: t('headers.allLocations'), language })}
      </StyledButton>

      <div
        css={{
          width: '180px',
          textAlign: 'end',
        }}
      >
        <StyledButton
          css={{ boxSizing: 'border-box', width: '90px' }}
          disabled={isViewOnly}
          onClick={() =>
            onClick({
              buttonName: node.isSelected
                ? t('plainText.remove')
                : t('plainText.add'),
              node,
              onChange,
              onChangeIds,
              t,
            })
          }
          kind={node.isSelected ? 'secondary' : 'primary'}
          prefix={<Icon icon={node.isSelected ? 'remove' : 'add'} />}
        >
          {titleCase({
            language,
            text: node.isSelected ? t('plainText.remove') : t('plainText.add'),
          })}
        </StyledButton>
      </div>
    </>
  )

  return (
    <TreeNodeContainer
      $isSelected={node.isSelected}
      style={{ ...style, paddingRight: '4px' }}
      {...restProps}
    >
      <Prefix />
      <ClickableRegion role="button">
        <ScopeSelectorAvatar modelId={node.data.twin.metadata.modelId} />
        <Group miw="0px" css={{ flexGrow: '1' }}>
          <TooltipWhenTruncated label={twin.name}>
            <SiteName>{twin.name}</SiteName>
          </TooltipWhenTruncated>
        </Group>
      </ClickableRegion>
      <Suffix />
    </TreeNodeContainer>
  )
}
