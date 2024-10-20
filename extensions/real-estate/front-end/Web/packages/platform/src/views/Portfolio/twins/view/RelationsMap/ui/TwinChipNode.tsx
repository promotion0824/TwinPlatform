import { Handle, Position } from 'react-flow-renderer'
import { styled } from 'twin.macro'

import { Ontology } from '@willow/common/twins/view/models'
import {
  getModelOfInterest,
  ModelOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import { TwinChip, useConfig, useFeatureFlag } from '@willow/ui'
import SummaryCount, {
  getTotalCount,
  SummaryType,
} from '../../../../LocationCard/SummaryCount'
import { LaidOutNode } from '../funcs/layoutGraph'
import { RELATIONS_MAP_NODE_SIZE } from '../funcs/utils'
import CountChip from './CountChip'
import {
  useNavigateToTwinInsightsLocation,
  useNavigateToTwinTicketsLocation,
} from './useTwinLocations'

export default function TwinChipNode({
  data,
  ontology,
  modelsOfInterest,
  onTwinClick,
  onToggleOutClick,
  onToggleInClick,
}: {
  data: LaidOutNode['data']
  ontology: Ontology
  modelsOfInterest: ModelOfInterest[]
  onTwinClick: () => void
  onToggleOutClick: (expanded: boolean) => void
  onToggleInClick: (expanded: boolean) => void
}) {
  if (data.type !== 'twin') {
    throw new Error("TwinChipNode requires a node of type 'twin'")
  }

  const { isSingleTenant } = useConfig()
  // This feature will only be available in Single Tenant environments,
  // because of the implementations of useTwinViewPath and fetchGraph
  const summaryCountsEnabled =
    useFeatureFlag().hasFeatureToggle('insightsCountInRelationsMap') &&
    isSingleTenant
  const navigateToTwinInsightsLocation = useNavigateToTwinInsightsLocation(
    data.id
  )
  const navigateToTwinTicketsLocation = useNavigateToTwinTicketsLocation(
    data.id
  )

  // In case failed to get the twin statics, we will not show the summary counts
  const additionalInfo =
    summaryCountsEnabled && data.insightsStats && data.ticketStatsByStatus
      ? [
          // do not render the SummaryCount if total count is 0
          getTotalCount(
            SummaryType.insights,
            'critical',
            data.insightsStats
          ) && (
            <SummaryCount
              counts={data.insightsStats}
              summaryType={SummaryType.insights}
              variant="critical"
              showSummaryName={false}
              onClick={navigateToTwinInsightsLocation}
              key={`${data.id}_critical_counts`}
            />
          ),
          getTotalCount(
            SummaryType.insights,
            'priority',
            data.insightsStats
          ) && (
            <SummaryCount
              counts={data.insightsStats}
              summaryType={SummaryType.insights}
              variant="priority"
              showSummaryName={false}
              onClick={navigateToTwinInsightsLocation}
              key={`${data.id}_insights_counts`}
            />
          ),
          getTotalCount(
            SummaryType.tickets,
            'status',
            data.ticketStatsByStatus
          ) && (
            <SummaryCount
              counts={data.ticketStatsByStatus}
              summaryType={SummaryType.tickets}
              showSummaryName={false}
              onClick={navigateToTwinTicketsLocation}
              key={`${data.id}_tickets_counts`}
            />
          ),
        ]
      : undefined

  // React Flow gets upset if we don't have handles, but we don't want them to
  // to actually be visible so we create them with opacity of zero.
  return (
    <Container data-testid={`TwinChipNode-${data.id}`}>
      <Handle type="target" position={Position.Bottom} style={{ opacity: 0 }} />
      <ExpandNodeButton
        // We apply this class name to the button to tell React Flow not to
        // attach d3-drag to it, which breaks tests that try to use the button.
        // See https://github.com/wbkd/react-flow/issues/2461
        className="nodrag"
        onToggle={onToggleOutClick}
        tw="mb-1"
        isExpanded={data.state?.out}
        count={data.edgeOutCount}
        data-testid="expand-out-button"
      />
      <StyledTwinChip
        className="nodrag"
        data-testid="chip"
        variant="instance"
        text={data.name}
        additionalInfo={additionalInfo}
        modelOfInterest={getModelOfInterest(
          data.modelId,
          ontology,
          modelsOfInterest
        )}
        $selected={data.selected}
        onClick={onTwinClick}
        highlightOnHover
      />
      <ExpandNodeButton
        className="nodrag"
        onToggle={onToggleInClick}
        tw="mt-1"
        isExpanded={data.state?.in}
        count={data.edgeInCount}
        data-testid="expand-in-button"
      />
      <Handle type="source" position={Position.Top} style={{ opacity: 0 }} />
    </Container>
  )
}

const Container = styled.div({
  width: RELATIONS_MAP_NODE_SIZE,
  textAlign: 'center',
})

/**
 * Button to fetch more of the graph in a particular direction from a node.
 */
function ExpandNodeButton({
  count,
  isExpanded,
  onToggle,
  className,
  'data-testid': dataTestId,
}: {
  /**
   * How many nodes are there in this direction? This is displayed on the
   * label. If `count` is zero the button will be invisible (but still use the
   * same amount of space, so our positioning algorithms don't need to worry).
   */
  count: number

  /**
   * Is the graph already expanded in this direction?
   */
  isExpanded: boolean
  onToggle: (expanded: boolean) => void
  className?: string
  'data-testid'?: string
}) {
  return (
    <ExpandNodeContainer
      onClick={() => onToggle(!isExpanded)}
      className={className}
      $visible={count > 0}
      data-testid={dataTestId}
    >
      <CountChip isExpanded={isExpanded}>+{count}</CountChip>
    </ExpandNodeContainer>
  )
}

const ExpandNodeContainer = styled.div<{ $visible: boolean }>(
  ({ $visible }) => ({
    textAlign: 'center',
    // We use `visibility` to hide invisible expand buttons so they still take
    // the same amount of space.
    visibility: $visible ? undefined : 'hidden',
  })
)

const StyledTwinChip = styled(TwinChip)<{ $selected: boolean }>(
  ({ $selected }) => ({
    border: $selected ? '1px solid var(--primary5)' : undefined,
    borderRadius: $selected ? 2 : undefined,
    cursor: 'pointer',
    maxWidth: RELATIONS_MAP_NODE_SIZE,
  })
)
