import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

import { titleCase } from '@willow/common'
import { Ontology } from '@willow/common/twins/view/models'
import {
  getModelOfInterest,
  ModelOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'
import { TwinChip, useConfig, useFeatureFlag } from '@willow/ui'
import {
  Button,
  ButtonProps,
  Group,
  Icon,
  IconButton,
  Stack,
} from '@willowinc/ui'

import SummaryCount, {
  SummaryType,
} from '../../../../LocationCard/SummaryCount'
import { LaidOutNode } from '../funcs/layoutGraph'
import { LayoutDirection, NodeState } from '../types'
import {
  useNavigateToTwinInsightsLocation,
  useNavigateToTwinTicketsLocation,
} from './useTwinLocations'

const oppositeDirections: { [key in LayoutDirection]: LayoutDirection } = {
  [LayoutDirection.UP]: LayoutDirection.DOWN,
  [LayoutDirection.DOWN]: LayoutDirection.UP,
  [LayoutDirection.LEFT]: LayoutDirection.RIGHT,
  [LayoutDirection.RIGHT]: LayoutDirection.LEFT,
}

const Count = styled.div(({ theme }) => ({
  ...theme.font.body.sm.regular,
  color: theme.color.neutral.fg.default,
}))

const Overlay = styled(Stack)(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: theme.radius.r2,
  boxShadow: theme.shadow.s3,
  boxSizing: 'content-box',
  maxWidth: '530px',
  zIndex: theme.zIndex.overlay,
}))

const OverlayContainer = styled.div(({ theme }) => ({
  bottom: theme.spacing.s20,
  display: 'flex',
  justifyContent: 'center',
  paddingLeft: '10%',
  paddingRight: '10%',
  position: 'absolute',
  width: '100%',
}))

const StyledButton = styled(Button)({
  '.mantine-Button-section': {
    width: 'auto',
  },
})

const StyledTwinChip = styled(TwinChip)({
  width: 'fit-content',
})

const Subheading = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

const TwinName = styled.div(({ theme }) => ({
  ...theme.font.heading.lg,
  color: theme.color.neutral.fg.highlight,
  wordBreak: 'break-word',
}))

/**
 * Displays an overlay with some basic info on the selected twin, and some
 * operations the user can perform on it.
 */
export default function TwinOverlay({
  twin,
  state,
  modelsOfInterest,
  ontology,
  direction,
  onToggleInClick,
  onToggleOutClick,
  onToggleBothClick,
  onGoToTwinClick,
  onClose,
}: {
  /**
   * The twin to display the information and operations for
   */
  twin: LaidOutNode
  /**
   * The node state for the specified twin
   */
  state: NodeState
  modelsOfInterest: ModelOfInterest[]
  ontology: Ontology
  /**
   * The layout direction. Used to make sure the labels and icons
   * on the buttons make sense.
   */
  direction: LayoutDirection
  /**
   * Called when the user hits "Expand up" or "Collapse up"
   */
  onToggleInClick: () => void
  /**
   * Called when the user hits "Expand down" or "Collapse down"
   */
  onToggleOutClick: () => void
  /**
   * Called when the user hits "Expand both" or "Collapse both"
   */
  onToggleBothClick: () => void
  /**
   * Called when the user clicks "Go to twin"
   */
  onGoToTwinClick: () => void
  /**
   * Called when the user closes the overlay
   */
  onClose: () => void
}) {
  const {
    i18n: { language },
    t,
  } = useTranslation()
  if (twin.data.type !== 'twin') {
    throw new Error('`twin` was not a twin')
  }

  const { isSingleTenant } = useConfig()
  // This feature will only be available in Single Tenant environments,
  // because of the implementations of useTwinViewPath and fetchGraph
  const summaryCountsEnabled =
    useFeatureFlag().hasFeatureToggle('insightsCountInRelationsMap') &&
    isSingleTenant
  const navigateToTwinInsightsLocation = useNavigateToTwinInsightsLocation(
    twin.data.id
  )
  const navigateToTwinTicketsLocation = useNavigateToTwinTicketsLocation(
    twin.data.id
  )

  return (
    <OverlayContainer>
      <Overlay gap="s12" p="s16">
        <Group align="self-start" wrap="nowrap">
          <Group align="self-start" w="100%">
            <Stack gap="s4" mr="auto">
              <TwinName>{twin.data.name}</TwinName>

              <StyledTwinChip
                modelOfInterest={getModelOfInterest(
                  twin.data.modelId,
                  ontology,
                  modelsOfInterest
                )}
              />
            </Stack>

            <Button
              kind="secondary"
              onClick={onGoToTwinClick}
              suffix={<Icon icon="arrow_forward" />}
            >
              {titleCase({ language, text: t('plainText.goToTwin') })}
            </Button>
          </Group>

          <IconButton
            background="transparent"
            icon="close"
            kind="secondary"
            onClick={onClose}
          />
        </Group>

        {/* In case failed to get the twin statics, we will not show the summary counts */}
        {summaryCountsEnabled &&
          twin.data.insightsStats &&
          twin.data.ticketStatsByStatus && (
            <Group>
              <SummaryCount
                counts={twin.data.insightsStats}
                summaryType={SummaryType.insights}
                variant="critical"
                onClick={navigateToTwinInsightsLocation}
              />
              <SummaryCount
                counts={twin.data.insightsStats}
                summaryType={SummaryType.insights}
                variant="priority"
                onClick={navigateToTwinInsightsLocation}
              />
              <SummaryCount
                counts={twin.data.ticketStatsByStatus}
                summaryType={SummaryType.tickets}
                onClick={navigateToTwinTicketsLocation}
              />
            </Group>
          )}

        <Stack>
          <Subheading>
            {titleCase({ language, text: t('plainText.groupings') })}
          </Subheading>

          <Group>
            {twin.data.edgeOutCount > 0 && (
              <ExpandContractButton
                count={twin.data.edgeOutCount}
                data-testid="expand-contract-out"
                direction={direction}
                isExpanded={state?.out}
                onClick={onToggleOutClick}
              />
            )}
            {twin.data.edgeInCount > 0 && (
              <ExpandContractButton
                count={twin.data.edgeInCount}
                data-testid="expand-contract-in"
                direction={oppositeDirections[direction]}
                isExpanded={state?.in}
                onClick={onToggleInClick}
              />
            )}

            {/* If we only have in or only have out, there's no point having a "both" button */}
            {twin.data.edgeInCount > 0 && twin.data.edgeOutCount > 0 && (
              <ExpandContractButton
                data-testid="expand-contract-both"
                direction="both"
                isExpanded={state?.in && state?.out}
                onClick={onToggleBothClick}
              />
            )}
          </Group>
        </Stack>
      </Overlay>
    </OverlayContainer>
  )
}

/**
 * A button that says either "Expand <some direction>" or "Collapse <some direction>"
 * depending on whether we are already expanded in that direction.
 */
function ExpandContractButton({
  count,
  direction,
  isExpanded = false,
  onClick,
  ...restProps
}: ButtonProps & {
  count?: number
  direction: LayoutDirection | 'both'
  isExpanded?: boolean
}) {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  return (
    <StyledButton
      kind={isExpanded ? 'secondary' : 'primary'}
      onClick={onClick}
      suffix={
        <Group gap={0} wrap="nowrap">
          <Icon
            icon={
              direction === 'UP'
                ? 'arrow_upward'
                : direction === 'DOWN'
                ? 'arrow_downward'
                : 'swap_vert'
            }
          />
          {count != null && direction !== 'both' && <Count>{count}</Count>}
        </Group>
      }
      w={160}
      {...restProps}
    >
      {titleCase({
        language,
        text: t(
          isExpanded
            ? 'interpolation.collapseDirection'
            : 'interpolation.expandDirection',
          { direction: t(`plainText.${direction.toLowerCase()}`) }
        ),
      })}
    </StyledButton>
  )
}
