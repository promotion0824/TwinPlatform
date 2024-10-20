import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import _ from 'lodash'
import {
  BackButton,
  Flex,
  Text,
  DataPanel,
  Message,
  Tabs,
  Tab,
  useSnackbar,
  DocumentTitle,
} from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'

import { styled } from 'twin.macro'
import usePutWidget from '../../../../hooks/Widgets/usePutWidget'
import useDeleteWidget from '../../../../hooks/Widgets/useDeleteWidget'
import usePostWidget from '../../../../hooks/Widgets/usePostWidget'
import LayoutHeaderPanel from '../../../Layout/Layout/LayoutHeaderPanel'
import {
  EmbedLocation,
  ReportConfig,
} from '../../../../components/Reports/ReportsLayout'
import { useSites } from '../../../../providers/index'
import NewReportModal from '../../../../components/Reports/NewReportModal/NewReportModal'
import routes from '../../../../routes'
import ReportModal from '../../../../components/Reports/ReportModal/ReportModal'
import { ConfirmModal } from '../../../../components/Reports/ReportModal/ReportForm/Actions'
import useGetReportsConfig from '../../../../hooks/ReportsConfig/useGetReportsConfig'
import { Widget } from 'packages/platform/src/services/Widgets/WidgetsService'
import { Site } from '@willow/common/site/site/types'

const StyledBorderFlex = styled(Flex)({
  borderTop: '1px solid #383838',
  marginTop: '25px',
})

const StyledButton = styled(Button)({
  margin: '25px 25px 0 0',
})
function DeleteReportButtonField({ onClick }: { onClick: () => void }) {
  const { t } = useTranslation()
  return (
    <StyledBorderFlex align="right">
      <StyledButton kind="negative" onClick={onClick}>
        {t('plainText.deleteReportConfiguration')}
      </StyledButton>
    </StyledBorderFlex>
  )
}

export default function ReportsContainer({
  TableComponent,
  ReportFormComponent,
  ReportEditFormComponent,
  embedLocation,
}: {
  TableComponent: React.ElementType
  ReportFormComponent: React.ElementType
  ReportEditFormComponent: React.ElementType
  embedLocation: EmbedLocation
}) {
  const { t } = useTranslation()
  const { portfolioId } = useParams<{ portfolioId: string }>()
  const snackbar = useSnackbar()
  const reportsConfigurations = useGetReportsConfig(portfolioId)
  const { isLoading, isError, refetch } = reportsConfigurations
  const {
    isSuccess: isPostWidgetSuccess,
    isError: isPostWidgetError,
    mutate: postWidget,
  } = usePostWidget()
  const {
    isSuccess: isPutWidgetSuccess,
    isError: isPutWidgetError,
    mutate: putWidget,
  } = usePutWidget()
  const {
    isSuccess: isDeleteWidgetSuccess,
    isError: isDeleteWidgetError,
    mutate: deleteWidget,
  } = useDeleteWidget()
  const sites = useSites()
  const [newReportModal, setNewReportModal] = useState(false)
  const [categories, setCategories] = useState<string[]>([])
  const [selectedReport, setSelectedReport] = useState<ReportConfig>()
  const [reports, setReports] = useState<Widget[]>([])
  const [showConfirmModal, setShowConfirmModal] = useState(false)

  useEffect(() => {
    const listOfCategories =
      reportsConfigurations?.data?.widgets.map(
        (widget) => widget.metadata.category
      ) ?? []

    const filteredCategories: string[] = _.uniq(listOfCategories).filter(
      (category: string | undefined): category is string => category != null
    )
    filteredCategories.sort()
    setCategories(filteredCategories)

    const filteredReports =
      reportsConfigurations?.data?.widgets.filter(
        ({ metadata }) => metadata.embedLocation === embedLocation
      ) ?? []
    setReports(filteredReports)
  }, [reportsConfigurations?.data?.widgets, embedLocation])

  useEffect(() => {
    if (isPostWidgetSuccess || isPutWidgetSuccess || isDeleteWidgetSuccess) {
      refetch()
    }
  }, [isPostWidgetSuccess, isPutWidgetSuccess, isDeleteWidgetSuccess, refetch])

  useEffect(() => {
    if (isPostWidgetError || isPutWidgetError || isDeleteWidgetError) {
      snackbar.show(t('plainText.errorOccurred'))
    }
  }, [isPostWidgetError, isPutWidgetError, isDeleteWidgetError, snackbar, t])

  const confirmationText = [
    selectedReport?.id,
    t('headers.dashboard'),
    t('headers.configuration'),
  ].join(' ')
  const addButtonText =
    embedLocation === 'reportsTab'
      ? t('plainText.addReport')
      : `${t('plainText.add')} ${t('headers.dashboard')}`
  const addConfigText =
    embedLocation === 'reportsTab'
      ? t('plainText.addReport')
      : `${t('plainText.add')} ${t('headers.dashboard')}`
  const updateConfigText =
    embedLocation === 'reportsTab'
      ? `${t('plainText.update')} report`
      : `${t('plainText.update')} ${t('headers.dashboard')}`
  const documentTitle = `${t(
    embedLocation === 'reportsTab' ? 'headers.reports' : 'headers.dashboards'
  )} ${t('headers.configuration')}`

  const siteSelectFields = sites.map(({ id, name }: Site) => ({ id, name }))
  return (
    <>
      <DocumentTitle scopes={[documentTitle, t('headers.admin')]} />

      <LayoutHeaderPanel fill="content">
        <FlexColumn>
          <FlexRow tw="justify-between">
            {/* This FlexRow and the setNewReportModal Button will be the same row,
            so it cannot take 100% width of the parent. Otherwise the setNewReportModal
            button will be shriked. */}
            <FlexRow css={{ width: 'auto' }}>
              <BackButton />
              <Flex horizontal align="middle" size="medium" padding="large">
                <Text type="h2">Reports Configuration</Text>
              </Flex>
            </FlexRow>
            <Flex align="middle" padding="0 large">
              <Button
                onClick={() => setNewReportModal(true)}
                prefix={<Icon icon="add" />}
              >
                {addButtonText}
              </Button>
            </Flex>
          </FlexRow>
          <TabsWrapper>
            <StyledBoader />
            <StyledTabs>
              <Tab
                header="Reports"
                to={routes.admin_portfolios__portfolioId_reportsConfig(
                  portfolioId
                )}
              />
              <Tab
                header="Dashboards"
                to={routes.admin_portfolios__portfolioId_dashboardsConfig(
                  portfolioId
                )}
              />
            </StyledTabs>
          </TabsWrapper>
        </FlexColumn>
      </LayoutHeaderPanel>
      {isError ? (
        <Message tw="h-full">{t('plainText.errorOccurred')}</Message>
      ) : (
        <DataPanel isLoading={isLoading}>
          <TableComponent
            reports={reports}
            categories={categories}
            onRowClick={(report: ReportConfig) => {
              setSelectedReport(report)
            }}
            selectedReport={selectedReport}
          />
        </DataPanel>
      )}
      {newReportModal && (
        <NewReportModal
          onClose={() => setNewReportModal(false)}
          header={addConfigText}
        >
          <ReportFormComponent
            categories={categories}
            setNewReportModal={setNewReportModal}
            getUpdatedData={() => refetch()}
            portfolioId={portfolioId}
            siteSelectFields={siteSelectFields}
            onSubmit={(formData: any) => {
              postWidget(formData)
            }}
          />
        </NewReportModal>
      )}
      {selectedReport && (
        <ReportModal
          onClose={() => setSelectedReport(undefined)}
          header={updateConfigText}
        >
          <ReportEditFormComponent
            report={selectedReport}
            onClose={() => setSelectedReport(undefined)}
            getUpdatedData={() => refetch()}
            categories={categories}
            portfolioId={portfolioId}
            siteSelectFields={siteSelectFields}
            onSubmit={(formData: any) => {
              const { id, ...rest } = formData
              putWidget({ id, formData: rest })
            }}
            isCategoryReadOnly
          >
            <DeleteReportButtonField
              onClick={() => {
                setShowConfirmModal(true)
              }}
            />
          </ReportEditFormComponent>
        </ReportModal>
      )}
      {showConfirmModal && selectedReport && (
        <ConfirmModal
          header={t('plainText.deleteReportConfiguration')}
          question={t('questions.sureToDelete')}
          text={confirmationText}
          onClose={() => {
            setShowConfirmModal(false)
            setSelectedReport(undefined)
          }}
          onSubmit={() => {
            deleteWidget(selectedReport.id)
          }}
        />
      )}
    </>
  )
}

const FlexRow = styled.div({
  display: 'flex',
  flexDirection: 'row',
  width: '100%',
})

const FlexColumn = styled.div({
  display: 'flex',
  flexDirection: 'column',
  width: '100%',
})

const StyledBoader = styled.div({
  display: 'inline-block',
  width: '100%',
  height: '4px',
  background: '#000',
})

const TabsWrapper = styled.div({
  display: 'flex',
  flexDirection: 'column',
})
const StyledTabs = styled(Tabs)({
  '& > div::before': {
    backgroundColor: 'transparent',
  },
})
