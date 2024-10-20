import { qs } from '@willow/common'
import { Fetch } from '@willow/ui'
import { NavList, Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { Link } from 'react-router-dom'
import styled from 'styled-components'
import useCommandAnalytics from '../../useCommandAnalytics'
import { useInspections } from '../InspectionsProvider'
import { getInspectionsPageTitle } from '../getPageTitles'
import ExportCsvButton from './ExportCsvButton/ExportCsvButton.js'
import {
  InspectionCheck,
  getCheckHistoryUrl,
} from './InspectionChecks/InspectionChecks'

export default function InspectionHistory() {
  const params = useParams()
  const { t } = useTranslation()
  const {
    sharedTimeRange: selectedTimeRange,
    setPageTitles,
    resetTimeRange,
  } = useInspections()

  const selectedSiteId = params?.siteId
  const inspectionSiteId = selectedSiteId ?? qs.get('site')
  const [inspection, setInspection] = useState(null)
  const commandAnalytics = useCommandAnalytics(params?.siteId)

  const activeCheck = inspection?.checks?.find(
    (check) => check.id === params.checkId
  )

  useEffect(
    () => {
      resetTimeRange()
    },
    // reset shared time range when load so it will start fresh
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  )

  useEffect(() => {
    if (!inspection || !activeCheck) {
      return
    }

    setPageTitles([
      getInspectionsPageTitle({
        siteId: selectedSiteId,
        title: t('headers.inspections'),
      }),
      {
        title: inspection.name,
        // always just use the current url as there are multiple routes to get here
        href: window.location.pathname + window.location.search,
      },
    ])
  }, [activeCheck, inspection, setPageTitles, selectedSiteId, t])

  const isGraphDisabled =
    activeCheck &&
    (activeCheck.type.toLowerCase() === 'list' ||
      activeCheck.type.toLowerCase() === 'date')

  useEffect(() => {
    commandAnalytics.pageInspections('history')
  }, [commandAnalytics])

  return (
    <>
      <Fetch
        name="inspection"
        url={`/api/sites/${inspectionSiteId}/inspections/${params.inspectionId}`}
        onResponse={(response) => setInspection(response)}
      >
        {inspection != null && (
          <Container>
            <PanelGroup>
              <Panel title={t('headers.checks')} defaultSize={320} collapsible>
                <PanelContent>
                  <NavList>
                    {inspection.checks.map((check) => (
                      <UnstyledLink
                        key={check.id}
                        to={getCheckHistoryUrl({
                          siteId: params.siteId,
                          inspectionId: inspection.id,
                          checkId: check.id,
                        })}
                      >
                        <NavList.Item
                          label={check.name}
                          active={check.id === activeCheck.id}
                        />
                      </UnstyledLink>
                    ))}
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
                      inspection={inspection}
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
                      inspection={inspection}
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
                        inspection={inspection}
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
      </Fetch>
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
