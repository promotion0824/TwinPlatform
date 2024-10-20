/* eslint-disable complexity, no-else-return, arrow-body-style */
import { useGetEquipment } from 'hooks'
import { pickBy } from 'lodash'
import React, { useMemo, Fragment } from 'react'
import { Helmet } from 'react-helmet-async'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { css, styled } from 'twin.macro'

import { titleCase } from '@willow/common'
import { createSearchParams } from '@willow/common/hooks/useSearchParams'
import TwinModelChip from '@willow/common/twins/view/TwinModelChip'
import {
  fileModelId,
  sensorModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import { getTwinRogueAttributes } from '@willow/common/twins/view/twinModel'
import {
  FileIcon,
  Message,
  Progress,
  DocumentTitle,
  getUrl,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import {
  Badge,
  Button,
  Icon,
  PageTitle,
  PageTitleItem,
  Panel,
  PanelContent,
  PanelGroup,
  Tabs,
} from '@willowinc/ui'

import SelectedPointsProvider from '../../../../components/MiniTimeSeries/SelectedPointsProvider'
import useOntology from '../../../../hooks/useOntologyInPlatform'
import routes from '../../../../routes'
import HeaderWithTabs from '../../../Layout/Layout/HeaderWithTabs'
import { useSearchResults } from '../results/page/state/SearchResults'
import AssetHistory from './AssetHistory'
import FilesTab from './FilesTab'
import RelationshipsTab from './RelationshipsTab'
import SensorsTab from './SensorsTab'
import SummaryTab from './SummaryTab'
import { TwinEditorProvider, useTwinEditor } from './TwinEditorContext.tsx'
import TwinRelationships from './TwinRelationships'
import { TwinViewProvider, useTwinView } from './TwinViewContext'
import TwinViewRightPanelContainer from './TwinViewRightPanelContainer'

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

/**
 * View & edit data for a particular twin.
 *
 * XD design link: https://xd.adobe.com/view/3f163df5-e478-4d23-88af-0c05da027914-6a45
 */
export default function TwinView() {
  const { siteId, twinId } = useParams()
  const user = useUser()

  // Surround everything with this key so we make sure to reload everything
  // when we change twin.
  return (
    <React.Fragment key={`${siteId}-${twinId}`}>
      <TwinViewProvider siteId={siteId} twinId={twinId}>
        <TwinEditorProvider user={user} siteId={siteId} twinId={twinId}>
          <SelectedPointsProvider>
            <TwinViewInner />
          </SelectedPointsProvider>
        </TwinEditorProvider>
      </TwinViewProvider>
    </React.Fragment>
  )
}

export function TwinViewInner() {
  const { relationships, files } = useTwinView()
  const { status, twin, modelInfo, missingSensors } = useTwinEditor()

  return (
    <TwinViewContent
      relationships={relationships}
      files={files}
      status={status}
      twin={twin}
      modelInfo={modelInfo}
      missingSensors={missingSensors}
    />
  )
}

export function TwinViewContent({
  status,
  twin,
  modelInfo,
  relationships,
  files,
  missingSensors,
}) {
  const { isScopeSelectorEnabled, location, locationName } = useScopeSelector()
  const { t, language } = useTranslation()
  const featureFlags = useFeatureFlag()
  const searchResults = useSearchResults()
  const queryString = createSearchParams(
    pickBy(searchResults?.storedParams)
  ).toString()

  const {
    tab,
    setTab,
    rightTab,
    setRightTab,
    modelsOfInterest,
    type,
    setType,
    setInsightId,
  } = useTwinView()
  const ontologyQuery = useOntology()
  const ontology = ontologyQuery.data
  const { data: equipment } = useGetEquipment(twin?.siteID, twin?.uniqueID)
  const sensorsCount = equipment?.points?.length

  const showSensorTab = sensorsCount > 0 || missingSensors.length > 0

  const hasSensorsFeature = featureFlags.hasFeatureToggle(
    'twinExplorerSensorSearch'
  )

  // Disable asset history tab for file and sensor twins.
  // ignoreModelsForAssetHistoryTab is a Set that contains all the models that inherits file and sensor models.
  const ignoreModelsForAssetHistoryTab = useMemo(() => {
    const ignoreModels = [fileModelId, sensorModelId]
    ignoreModels.push(...(ontology?.getModelDescendants(ignoreModels) || []))
    return new Set(ignoreModels)
  }, [ontology])

  const isAssetHistoryEnabled = !ignoreModelsForAssetHistoryTab.has(
    twin?.metadata?.modelId
  )

  // We will run into trouble if we try to render the form and the twin has
  // attributes which don't exist on its model (because we will not know how to
  // render them). So we check this once up front and if we find any, we
  // display an error page.
  const rogueAttributes = useMemo(() => {
    if (modelInfo != null) {
      return getTwinRogueAttributes(twin, modelInfo.expandedModel)
    } else {
      return []
    }
  }, [twin, modelInfo])

  const shownRelationships = useMemo(() => {
    if (twin == null || relationships == null || ontology == null) {
      return null
    }

    return relationships
      .map((r) => {
        // For the purposes of the relationships displays (in the left
        // sidebar and the relationships tab), we only care about the end of
        // the relationship that is not the current twin. So if the
        // relationship targets the twin, we reverse the relationship
        // direction so the displays can assume the target is the other end.
        // We should revisit this as this is changing the meaning of the
        // relationship to no longer be technically correct.
        if (r.target.id === twin.id) {
          return {
            ...r,
            source: r.target,
            target: r.source,
          }
        } else {
          return r
        }
      })
      .filter((r) => {
        // We exclude files, and if the twinExplorerSensorSearch feature
        // flag is not enabled, we exclude sensors.
        const modelId = r.target.modelOfInterest?.modelId
        return (
          modelId !== fileModelId &&
          (modelId !== sensorModelId || hasSensorsFeature)
        )
      })
  }, [ontology, relationships, twin, hasSensorsFeature])

  const isDocument = modelInfo?.modelOfInterest?.modelId === fileModelId

  const twinName = twin?.name || t('plainText.unnamedTwin')

  return status === 'loading' ? (
    <Progress />
  ) : status === 'error' ? (
    // There was an error loading this twin
    <Message icon="error">{t('plainText.errorLoadingTwin')}</Message>
  ) : status === 'not_found' ? (
    // Sorry, we could not find this twin
    <Message icon="error">{t('plainText.notFindTwin')}</Message>
  ) : status === 'no_permission' ? (
    <Message icon="error">
      {t('plainText.insufficientPrivilegesForTwin')}
    </Message>
  ) : rogueAttributes.length > 0 ? (
    <Message icon="error">
      <p>{t('plainText.twinWithFieldsNotInModel')}</p>
      <ul>
        {rogueAttributes.map((e, i) => (
          <li key={i}>{e.join('.')}</li>
        ))}
      </ul>
    </Message>
  ) : (
    <>
      <DocumentTitle
        scopes={[twinName, t('headers.searchAndExplore'), locationName]}
      />
      <HeaderWithTabs
        titleRow={[
          <TwinViewPageTitle
            key="pageTitle"
            pages={[
              {
                title: t('headers.searchAndExplore'),
                href:
                  isScopeSelectorEnabled && location?.twin?.id
                    ? `${routes.portfolio_twins_scope__scopeId_results(
                        location.twin.id
                      )}?${queryString}`
                    : `${routes.portfolio_twins_results}?${queryString}`,
              },
              {
                title: twinName,
                href: window.location.pathname,
                suffix: (
                  <SuffixContainer>
                    <TwinModelChip
                      model={modelInfo.model}
                      modelOfInterest={modelInfo.modelOfInterest}
                    />
                    {isDocument && (
                      <FileIconContainer>
                        <FileIcon filename={twin.name} />
                      </FileIconContainer>
                    )}
                  </SuffixContainer>
                ),
              },
            ]}
          />,
          <Fragment key="buttonContainer">
            {isDocument && (
              <Button
                kind="secondary"
                onClick={() =>
                  window.open(
                    getUrl(
                      `/api/sites/${twin.siteID}/twins/${twin.id}/download`
                    ),
                    '_blank'
                  )
                }
                prefix={<Icon icon="file_download" />}
              >
                {titleCase({ text: t('labels.downloadFile'), language })}
              </Button>
            )}
          </Fragment>,
        ]}
      />

      <StyledPanelGroup>
        <TwinViewLeftPanel
          shownRelationships={shownRelationships}
          modelsOfInterest={modelsOfInterest}
        />

        <PanelGroup resizable>
          <Panel
            collapsible
            tabs={
              <Tabs onTabChange={setTab} value={tab}>
                <Tabs.List>
                  <Tabs.Tab data-testid="tab-summary" value="summary">
                    {t('labels.summary')}
                  </Tabs.Tab>

                  <Tabs.Tab
                    data-testid="tab-relatedTwins"
                    suffix={<Badge>{shownRelationships?.length}</Badge>}
                    value="relationships"
                  >
                    {t('headers.relationships')}
                  </Tabs.Tab>

                  {files?.length > 0 && (
                    <Tabs.Tab
                      data-testid="tab-files"
                      suffix={<Badge>{files?.length}</Badge>}
                      value="files"
                    >
                      {t('headers.files')}
                    </Tabs.Tab>
                  )}

                  {showSensorTab && (
                    <Tabs.Tab
                      data-testid="tab-sensors"
                      suffix={<Badge>{sensorsCount}</Badge>}
                      value="sensors"
                    >
                      {t('headers.sensors')}
                    </Tabs.Tab>
                  )}

                  {isAssetHistoryEnabled && (
                    <Tabs.Tab
                      data-testid="tab-assetHistory"
                      value="assetHistory"
                    >
                      {t('plainText.assetHistory')}
                    </Tabs.Tab>
                  )}
                </Tabs.List>

                <Tabs.Panel value="summary">
                  <SummaryTab />
                </Tabs.Panel>

                <Tabs.Panel value="relationships">
                  <RelationshipsTab
                    twin={twin}
                    relationships={shownRelationships}
                    modelsOfInterest={modelsOfInterest}
                  />
                </Tabs.Panel>

                <Tabs.Panel value="files">
                  <FilesTab twin={twin} files={files ?? []} />
                </Tabs.Panel>

                {showSensorTab && (
                  <Tabs.Panel value="sensors">
                    <SensorsTab
                      twin={twin}
                      count={sensorsCount}
                      missingSensors={missingSensors}
                      selectTimeSeriesTab={() => {
                        if (rightTab !== 'timeSeries') {
                          setRightTab('timeSeries')
                        }
                      }}
                    />
                  </Tabs.Panel>
                )}

                {isAssetHistoryEnabled && (
                  <Tabs.Panel
                    value="assetHistory"
                    css={css({
                      display: 'flex',
                      flexDirection: 'column',
                      '&&&': {
                        overflowY: 'hidden',
                      },
                    })}
                  >
                    <AssetHistory
                      siteId={twin.siteID}
                      assetId={twin.uniqueID}
                      twinId={twin.id}
                      filterType={type}
                      setFilterType={setType}
                      setInsightId={setInsightId}
                    />
                  </Tabs.Panel>
                )}
              </Tabs>
            }
          />

          <TwinViewRightPanelContainer
            modelInfo={modelInfo}
            onChangeTab={setRightTab}
            selectedTab={rightTab}
            twin={twin}
          />
        </PanelGroup>
      </StyledPanelGroup>
    </>
  )
}

const TwinViewPageTitle = ({ pages }) => (
  <PageTitle>
    {pages.map(({ title, href, suffix }) => (
      <PageTitleItem key={title} suffix={suffix}>
        {href ? <Link to={href}>{title}</Link> : title}
      </PageTitleItem>
    ))}
  </PageTitle>
)

const SuffixContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: ${theme.spacing.s4};
  `
)

const FileIconContainer = styled.div({
  display: 'inline-flex',

  svg: {
    height: '26px',
    width: '26px',
  },
})

/**
 * Left panel component of Twin view that displays the twin's id, its twin chip, related twins, and a back button.
 */
function TwinViewLeftPanel({ shownRelationships, modelsOfInterest }) {
  const { t } = useTranslation()
  return (
    <Panel
      collapsible
      defaultSize={320}
      // This instant timeout allows the 3D model tab to resize as soon as this panel
      // is collapsed/expanded.
      onCollapse={() =>
        setTimeout(() => window.dispatchEvent(new Event('resize')))
      }
      title={t('headers.relatedTwins')}
    >
      <PanelContent>
        <TwinRelationshipsContainer>
          <TwinRelationships
            relationships={shownRelationships}
            modelsOfInterest={modelsOfInterest}
          />
        </TwinRelationshipsContainer>
      </PanelContent>
    </Panel>
  )
}

const TwinRelationshipsContainer = styled.div({
  padding: '0 1rem',
  overflow: 'auto',
})
