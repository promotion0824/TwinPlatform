/* eslint-disable complexity */
import { titleCase } from '@willow/common'
import { WALMART_ALERT } from '@willow/common/insights/insights/types'
import {
  NotFound,
  useSnackbar,
  useUser,
  TooltipWhenTruncated,
  getContainmentHelper,
} from '@willow/ui'
import {
  Card,
  Checkbox,
  CheckboxGroup,
  Group,
  Icon,
  IconButton,
  Loader,
  Panel,
  PanelContent,
  PanelGroup,
  Stack,
  useTheme,
} from '@willowinc/ui'
import _ from 'lodash'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { css } from 'styled-components'
import { TicketSourceType } from '../../../../../../../../../services/Tickets/TicketsService'
import { useFloor } from '../../../../FloorContext'
import PanelFooter from '../PanelFooter/PanelFooter'
import AddModelModal from './AddModelModal'
import DeleteModelModal from './DeleteModelModal'
import LegacyGroup from './Group'
import Layer from './Layer'
import styles from './Toolbar.css'

const toolbarContainer = 'toolbarContainer'
const { containerName, getContainerQuery } =
  getContainmentHelper(toolbarContainer)

export default function Toolbar({ isHidden, iframeRef }) {
  const theme = useTheme()
  const { customer } = useUser()
  const floor = useFloor()
  const snackbar = useSnackbar()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const [showAddModal, setShowAddModal] = useState(false)
  const [selectedLayer, setSelectedLayer] = useState()

  function toggle(id) {
    floor.setSelectedLayerIds((prevSelectedLayerIds) =>
      _.xor(prevSelectedLayerIds, [id])
    )
  }

  function select(id) {
    floor.setSelectedLayerIds((prevSelectedLayerIds) =>
      prevSelectedLayerIds.some(
        (prevSelectedLayerId) => prevSelectedLayerId !== id
      )
        ? [...prevSelectedLayerIds, id]
        : prevSelectedLayerIds
    )
  }

  function deselect(id) {
    floor.setSelectedLayerIds((prevSelectedLayerIds) =>
      prevSelectedLayerIds.filter(
        (prevSelectedLayerId) => prevSelectedLayerId !== id
      )
    )
  }

  useEffect(() => {
    window.onViewerLoaded = () => {
      const preSelectedLayerIds = floor.modules3D
        .filter((layer) => layer.isDefault)
        .map(({ id }) => id)

      const equipmentModelId = floor.getMain3dLayerForModuleTypeName(
        floor.equipment?.moduleTypeNamePath
      )?.id

      floor.setSelectedLayerIds((prevSelectedLayerIds) => [
        ...new Set([
          ...prevSelectedLayerIds,
          ...preSelectedLayerIds,
          ...(equipmentModelId ? [equipmentModelId] : []),
        ]),
      ])

      if (
        floor.equipment?.id != null &&
        floor.equipment?.forgeViewerModelId != null
      ) {
        const nextAsset = {
          assetId: floor.equipment.id,
          forgeViewerAssetId:
            floor.equipment.forgeViewerModelId?.toLowerCase?.(),
        }

        floor.iframeRef.current?.contentWindow?.selectAsset?.(nextAsset)
      }
    }

    window.getSelectedLayers = () => {
      const equipmentModelId = floor.modules3D.find(
        (layer) => layer.typeName === floor.equipment?.moduleTypeNamePath
      )?.id

      const selectedLayerIds = [
        ...new Set([...floor.selectedLayerIds, equipmentModelId]),
      ]

      return floor.modules3D
        .filter((layer) => selectedLayerIds.includes(layer.id))
        .map((layer) => ({
          url: layer.url,
          name: layer.typeName,
        }))
    }

    window.deselectLayer = (url) => {
      const layer = floor.modules3D.find((module3D) => module3D.url === url)

      if (layer != null) {
        snackbar.show(`${t('plainText.errorLoading3DModel')} "${layer.name}"`)

        deselect(layer.id)
      }
    }

    window.selectLayer = (url) => {
      const layer = floor.modules3D.find((module3D) => module3D.url === url)

      if (layer != null) {
        select(layer.id)
      }
    }
  }, [])

  let content = null
  const useNewSortingFeature = !!floor.disciplinesSortOrder?.sortOrder3d
  if (useNewSortingFeature) {
    const allGroups = []
    const sortedLayers = _(floor.modules3D)
      .orderBy((layer) =>
        floor.disciplinesSortOrder?.sortOrder3d?.indexOf(layer.moduleTypeId)
      )
      .value()
    for (let i = 0; i < sortedLayers.length; i++) {
      const layer = sortedLayers[i]
      const existingLayerGroup = allGroups.find(
        (group) =>
          group.id === layer.moduleGroup?.id &&
          layer.groupType !== '' &&
          layer.groupType !== ' '
      )
      if (existingLayerGroup) {
        existingLayerGroup.layersInGroup.push(layer)
      } else {
        allGroups.push({
          id: layer.moduleGroup?.id ?? layer.id,
          header: layer.groupType,
          layersInGroup: [layer],
        })
      }
    }

    content = (
      <>
        {_.map(allGroups, (group, key) => {
          const isGroup = group.layersInGroup.length > 1

          if (isGroup) {
            return (
              <LegacyGroup
                key={key}
                header={group.header}
                isReadOnly={floor.isReadOnly}
                visibleLayersCount={group.layersInGroup.reduce(
                  (count, layer) =>
                    count + floor.selectedLayerIds.includes(layer.id),
                  0
                )}
              >
                {group.layersInGroup.map((layer) => (
                  <Layer
                    key={layer.id}
                    layer={layer}
                    isSelected={floor.selectedLayerIds.includes(layer.id)}
                    iframeWindow={iframeRef.current?.contentWindow}
                    onClick={() => toggle(layer.id)}
                    onDeselect={() => deselect(layer.id)}
                    onDeleteClick={() => setSelectedLayer(layer)}
                    isInsideGroup
                  />
                ))}
              </LegacyGroup>
            )
          }

          const layer = group.layersInGroup[0]
          return (
            <Layer
              key={layer.id}
              layer={layer}
              isSelected={floor.selectedLayerIds.includes(layer.id)}
              iframeWindow={iframeRef.current?.contentWindow}
              onClick={() => toggle(layer.id)}
              onDeselect={() => deselect(layer.id)}
              onDeleteClick={() => setSelectedLayer(layer)}
            />
          )
        })}
      </>
    )
  } else {
    const allGroups = _(floor.modules3D)
      .orderBy((layer) => [
        layer.sortOrder,
        !layer.typeName.startsWith('Architectur'),
      ])
      .groupBy((layer) => layer.groupType)
      .value()

    const groups = _.pickBy(
      allGroups,
      (layers, key) => layers.length > 1 && key !== '' && key !== ' '
    )

    const ungroupedLayers = _(allGroups)
      .filter((layer, key) => layer.length === 1 || key === '' || key === ' ')
      .flatMap()
      .value()

    content = (
      <>
        {_.map(groups, (group, key) => (
          <LegacyGroup
            key={key}
            header={key}
            isReadOnly={floor.isReadOnly}
            visibleLayersCount={group.reduce(
              (count, layer) =>
                count + floor.selectedLayerIds.includes(layer.id),
              0
            )}
          >
            {group.map((layer) => (
              <Layer
                key={layer.id}
                layer={layer}
                isSelected={floor.selectedLayerIds.includes(layer.id)}
                iframeWindow={iframeRef.current?.contentWindow}
                onClick={() => toggle(layer.id)}
                onDeselect={() => deselect(layer.id)}
                onDeleteClick={() => setSelectedLayer(layer)}
                isInsideGroup
              />
            ))}
          </LegacyGroup>
        ))}
        {_.size(groups) > 0 && ungroupedLayers.length > 0 && <hr />}
        {ungroupedLayers.map((layer) => (
          <Layer
            key={layer.id}
            layer={layer}
            isSelected={floor.selectedLayerIds.includes(layer.id)}
            iframeWindow={iframeRef.current?.contentWindow}
            onClick={() => toggle(layer.id)}
            onDeselect={() => deselect(layer.id)}
            onDeleteClick={() => setSelectedLayer(layer)}
          />
        ))}
      </>
    )
  }

  // Only walmart can have self-generated insights, the sub insight source filter is only for walmart
  const isWalmart = customer?.name?.toLowerCase().includes('walmart')
  const isWalmartRetail = customer?.name
    ?.toLowerCase()
    .includes('walmart retail')

  return (
    <>
      <Panel
        id="3d-layers-outer-panel"
        title={titleCase({
          text: t('plainText.viewControls'),
          language,
        })}
        collapsible
        defaultSize={30}
        key={String(isHidden)}
        css={`
          display: ${isHidden ? 'none' : 'block'};
          container-type: size;
          container-name: ${containerName};
        `}
        hideHeaderBorder
      >
        <PanelGroup
          direction="vertical"
          css={`
            & > * {
              border: none;
              border-top: 1px solid ${theme.color.neutral.border.default};
              flex: none;
              max-height: 50%;
            }
            & > div:not(:first-child) {
              margin-top: 0px;
            }
          `}
        >
          <ToolbarPanel
            id="3d-module-layer-panel"
            title={t('headers.layers')}
            titleIcon="layers"
          >
            {content}
            {floor.modules3D.length === 0 && (
              <NotFound className={styles.notFound}>
                {t('plainText.noLayersFound')}
              </NotFound>
            )}
            {!floor.isReadOnly && (
              <PanelFooter>
                <IconButton
                  icon="add"
                  kind="secondary"
                  background="transparent"
                  onClick={() => setShowAddModal(true)}
                />
              </PanelFooter>
            )}
          </ToolbarPanel>

          <ToolbarPanel
            title={titleCase({
              text: t('plainText.dataLayers'),
              language,
            })}
            titleIcon="timeline"
            id="3d-insight-layer-panel"
          >
            <LegacyGroup
              header={t('headers.insights')}
              visibleLayersCount={floor.isInsightStatsLayerOn ? 1 : 0}
              isReadOnly
            >
              <ResponsiveCard
                h={floor.isInsightStatsLayerOn && isWalmart ? 104 : 40}
                background={floor.isInsightStatsLayerOn ? 'accent' : 'panel'}
              >
                <ResponsiveStack>
                  <SourceFilterHeaderGroup
                    pl={floor.statsQuery.isFetching ? 0 : 26}
                    onClick={() => {
                      // If insight stat layer is off and user clicks on the icon
                      // to turn it on, reset the insight layer sources
                      if (!floor.isInsightStatsLayerOn) {
                        floor.onResetInsightLayerSources()
                      }
                      floor.onInsightStatsLayerOpenChange(
                        !floor.isInsightStatsLayerOn
                      )
                    }}
                  >
                    {floor.statsQuery.isFetching && <Loader />}
                    <IconButton
                      size="large"
                      icon="emoji_objects"
                      kind="secondary"
                      background="transparent"
                      css={`
                        color: ${floor.isInsightStatsLayerOn
                          ? theme.color.intent.primary.fg.default
                          : theme.color.neutral.fg.default};
                      `}
                    />
                    <TextWithTooltip
                      text={titleCase({
                        text: t('headers.activeInsights'),
                        language,
                      })}
                    />
                  </SourceFilterHeaderGroup>
                  {isWalmartRetail && floor.isInsightStatsLayerOn && (
                    <CheckboxGroup
                      onChange={(sources) => {
                        floor.onInsightLayerSourcesChange(sources)
                        // Close the layer if no sources are selected
                        if (sources.length === 0) {
                          floor.onInsightStatsLayerOpenChange(false)
                        }
                      }}
                      pl={65}
                      label={t('labels.source')}
                      value={floor.insightLayerSources}
                    >
                      <Group>
                        {/* Brand names, no translations */}
                        <Checkbox label="Activate" value="activate" />
                        <Checkbox label="Walmart" value={WALMART_ALERT} />
                      </Group>
                    </CheckboxGroup>
                  )}
                </ResponsiveStack>
              </ResponsiveCard>
            </LegacyGroup>

            <LegacyGroup
              header={t('headers.tickets')}
              visibleLayersCount={floor.isTicketStatsLayerOn ? 1 : 0}
              isReadOnly
            >
              <ResponsiveCard
                h={floor.isTicketStatsLayerOn ? 104 : 40}
                background={floor.isTicketStatsLayerOn ? 'accent' : 'panel'}
              >
                <ResponsiveStack>
                  <SourceFilterHeaderGroup
                    pl={floor.statsQuery.isFetching ? 0 : 26}
                    onClick={() => {
                      if (!floor.isTicketStatsLayerOn) {
                        floor.onResetTicketLayerSources()
                      }
                      floor.onTicketStatsLayerOpenChange(
                        !floor.isTicketStatsLayerOn
                      )
                    }}
                  >
                    {floor.statsQuery.isFetching && <Loader />}
                    <IconButton
                      size="large"
                      icon="assignment"
                      kind="secondary"
                      background="transparent"
                      css={`
                        color: ${floor.isTicketStatsLayerOn
                          ? theme.color.intent.primary.fg.default
                          : theme.color.neutral.fg.default};
                      `}
                    />
                    <TextWithTooltip
                      text={titleCase({
                        text: t('headers.activeTickets'),
                        language,
                      })}
                    />
                  </SourceFilterHeaderGroup>
                  {floor.isTicketStatsLayerOn && (
                    <CheckboxGroup
                      pl={65}
                      label={t('labels.source')}
                      value={floor.ticketLayerSources}
                      onChange={(sources) => {
                        floor.onTicketLayerSourcesChange(sources)
                        if (sources.length === 0) {
                          floor.onTicketStatsLayerOpenChange(false)
                        }
                      }}
                    >
                      <Group>
                        <Checkbox
                          label={titleCase({
                            text: t('plainText.willow'),
                            language,
                          })}
                          value={TicketSourceType.willow}
                        />
                        {isWalmartRetail && (
                          <Checkbox
                            /* Walmart's Mapped Ticket Brand names, no translations */
                            label="Service Channel"
                            value={TicketSourceType.mapped}
                          />
                        )}
                        <Checkbox
                          label={titleCase({
                            text: t('plainText.other'),
                            language,
                          })}
                          value={[
                            TicketSourceType.app,
                            TicketSourceType.dynamics,
                          ].join(',')}
                        />
                      </Group>
                    </CheckboxGroup>
                  )}
                </ResponsiveStack>
              </ResponsiveCard>
            </LegacyGroup>
          </ToolbarPanel>
        </PanelGroup>
      </Panel>

      {showAddModal && <AddModelModal onClose={() => setShowAddModal(false)} />}
      {selectedLayer != null && (
        <DeleteModelModal
          layer={selectedLayer}
          onClose={() => setSelectedLayer()}
        />
      )}
    </>
  )
}

const ToolbarPanel = ({ children, title, titleIcon }) => (
  <Panel
    id={`3d-viewer-toolbar-${title}`}
    collapsible
    hideHeaderBorder
    title={
      <div
        css={css(
          ({ theme }) => `
      height: 100%;
      display: flex;
      line-height: ${theme.spacing.s20};
      gap: ${theme.spacing.s4};
    `
        )}
      >
        <Icon icon={titleIcon} />
        {title}
      </div>
    }
  >
    <PanelContent>{children}</PanelContent>
  </Panel>
)

const TextWithTooltip = ({ text }) => (
  <TooltipWhenTruncated label={text}>
    <span
      css={`
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      `}
    >
      {text}
    </span>
  </TooltipWhenTruncated>
)

const SourceFilterHeaderGroup = ({ children, pl, onClick }) => (
  <Group
    h={40}
    pl={pl}
    wrap="nowrap"
    className="source-filter-group"
    onClick={onClick}
    css={css(
      ({ theme }) =>
        `
      cursor: pointer;
      &:hover {
        color: ${theme.color.neutral.fg.highlight};
      }
    `
    )}
  >
    {children}
  </Group>
)

const ResponsiveStack = ({ children }) => (
  <Stack
    gap={0}
    css={css(
      ({ theme }) => `
      ${getContainerQuery(`width < 300px`)} {
        & .mantine-CheckboxGroup-root {
          padding-left: ${theme.spacing.s8} !important;
        }
        ,
        & .source-filter-group {
          padding-left: 0 !important;
        }
      }
    `
    )}
  >
    {children}
  </Stack>
)

const ResponsiveCard = ({ children, h, background }) => (
  <Card
    pl={20}
    h={h}
    background={background}
    css={css(
      ({ theme }) => `
      border: none;
      ${getContainerQuery('width < 360px')} {
        height: fit-content !important;
        padding-bottom: ${theme.spacing.s8};
        }`
    )}
  >
    {children}
  </Card>
)
