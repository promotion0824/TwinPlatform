import { FullSizeContainer, FullSizeLoader, titleCase } from '@willow/common'
import {
  Message,
  DocumentTitle,
  caseInsensitiveEquals,
  useScopeSelector,
} from '@willow/ui'
import { NavList, Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import { useEffect } from 'react'
import { UseQueryResult } from 'react-query'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { Link } from 'react-router-dom'
import styled from 'styled-components'
import { Inspection } from '../../../../services/Inspections/InspectionsServices'
import { useInspections } from '../InspectionsProvider'
import ExportCsvButton from './ExportCsvButton/ExportCsvButton.js'
import { InspectionCheck } from './InspectionChecks/InspectionChecks'
import makeScopedInspectionsPath from '../makeScopedInspectionsPath'

/**
 * This is the Inspection component incorporating the scope selector feature
 * to display check details, check history and times series for an inspection.
 *
 * TODO: remove packages\platform\src\views\Command\Inspections\InspectionHistory\InspectionHistory.js
 * once scope selector feature is complete.
 */
export default function ScopedInspectionHistory({
  inspectionQuery,
}: {
  inspectionQuery: UseQueryResult<Inspection>
}) {
  const { checkId, scopeId } = useParams<{
    checkId?: string
    scopeId?: string
  }>()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const { sharedTimeRange: selectedTimeRange, resetTimeRange } =
    useInspections()
  const activeCheck = inspectionQuery.data?.checks?.find(
    (check) => check.id === checkId
  )
  const { locationName } = useScopeSelector()

  useEffect(
    () => resetTimeRange(),
    // reset shared time range when load so it will start fresh
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  )

  const isGraphDisabled =
    caseInsensitiveEquals(activeCheck?.type, 'list') ||
    caseInsensitiveEquals(activeCheck?.type, 'date') ||
    checkId === 'all'

  return inspectionQuery.isError ? (
    <FullSizeContainer>
      <Message icon="error">{t('plainText.errorOccurred')}</Message>
    </FullSizeContainer>
  ) : inspectionQuery.isLoading ? (
    <FullSizeLoader />
  ) : (
    <>
      {inspectionQuery?.data != null && (
        <Container>
          <DocumentTitle
            scopes={[
              inspectionQuery?.data.name,
              t('headers.inspections'),
              locationName,
            ]}
          />
          <PanelGroup>
            <Panel title={t('headers.checks')} defaultSize={320} collapsible>
              <PanelContent>
                <NavList>
                  {[
                    { id: 'all', name: t('placeholder.all') },
                    ...(inspectionQuery?.data?.checks || []),
                  ].map((check) => {
                    const isAllCheck = check.id === 'all'
                    return (
                      <UnstyledLink
                        key={check.id}
                        to={makeScopedInspectionsPath(scopeId, {
                          inspectionId: `inspection/${inspectionQuery.data.id}`,
                          pageItemId: isAllCheck ? 'all' : check.id,
                          pageName: 'check',
                        })}
                      >
                        <NavList.Item
                          label={
                            isAllCheck
                              ? titleCase({ text: check.name ?? '', language })
                              : check.name
                          }
                          active={
                            isAllCheck
                              ? !activeCheck
                              : check.id === activeCheck?.id
                          }
                        />
                      </UnstyledLink>
                    )
                  })}
                </NavList>
              </PanelContent>
            </Panel>

            <PanelGroup resizable>
              <Panel
                title={t('headers.checkHistory')}
                collapsible
                headerControls={
                  <ExportCsvButton
                    times={selectedTimeRange}
                    inspection={inspectionQuery.data}
                    check={activeCheck}
                    css={{
                      /* adjust to same size as design system Button */
                      width: 28,
                      height: 28,
                    }}
                  />
                }
              >
                <StyledPanelContent>
                  <InspectionCheck
                    inspection={inspectionQuery.data}
                    isGraphActive={false}
                    times={selectedTimeRange}
                  />
                </StyledPanelContent>
              </Panel>

              {isGraphDisabled ? (
                <></>
              ) : (
                <Panel title={t('headers.timeSeries')} collapsible>
                  <StyledPanelContent>
                    <InspectionCheck
                      inspection={inspectionQuery.data}
                      isGraphActive
                      times={selectedTimeRange}
                    />
                  </StyledPanelContent>
                </Panel>
              )}
            </PanelGroup>
          </PanelGroup>
        </Container>
      )}
    </>
  )
}

const Container = styled.div(({ theme }) => ({
  padding: theme.spacing.s16,
}))

const UnstyledLink = styled(Link)`
  text-decoration: none;
`
const StyledPanelContent = styled(PanelContent)`
  height: 100%;
`
