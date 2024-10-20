import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { useGetReportsConfig } from 'hooks'
import {
  AddButton,
  BackButton,
  Flex,
  Text,
  DataPanel,
  Message,
} from '@willow/ui'
import LayoutHeaderPanel from 'views/Layout/Layout/LayoutHeaderPanel'
import ReportsTable from '../../../../components/Reports/ReportsTable'
import NewReportModal from '../../../../components/Reports/NewReportModal/NewReportModal'
import NewReportForm from '../../../../components/Reports/NewReportModal/NewReportForm'
import ReportModal from '../../../../components/Reports/ReportModal/ReportModal'

export default function ReportsConfig() {
  const { t } = useTranslation()
  const { portfolioId } = useParams()
  const reportsConfigurations = useGetReportsConfig(portfolioId)
  const { isLoading, isError } = reportsConfigurations
  const [newReportModal, setNewReportModal] = useState(false)
  const [categories, setCategories] = useState([])
  const [selectedReport, setSelectedReport] = useState(null)
  const { refetch } = reportsConfigurations

  useEffect(() => {
    const listOfCategories = reportsConfigurations?.data?.widgets.map(
      (widget) => widget.metadata.category
    )
    const filteredCategories = [...new Set(listOfCategories)].filter(
      (category) => category
    )
    const sortedCategories = filteredCategories.sort()
    setCategories(sortedCategories)
  }, [reportsConfigurations?.data?.widgets])

  return (
    <>
      <LayoutHeaderPanel fill="content">
        <BackButton />
        <Flex horizontal align="middle" size="medium" padding="large">
          <Text type="h2">Reports Configuration</Text>
        </Flex>
        <Flex align="middle" padding="0 large">
          <AddButton onClick={() => setNewReportModal(true)}>
            {t('plainText.addReport', { defaultValue: 'Add Report' })}
          </AddButton>
        </Flex>
      </LayoutHeaderPanel>
      {isError ? (
        <Message tw="h-full">{t('plainText.errorOccurred')}</Message>
      ) : (
        <DataPanel isLoading={isLoading}>
          <ReportsTable
            reports={reportsConfigurations?.data?.widgets}
            categories={categories}
            onRowClick={(report) => {
              setSelectedReport(report)
            }}
            selectedReport={selectedReport}
          />
        </DataPanel>
      )}
      {newReportModal && (
        <NewReportModal onClose={() => setNewReportModal(false)}>
          <NewReportForm
            categories={categories}
            setNewReportModal={setNewReportModal}
            getUpdatedData={() => refetch()}
          />
        </NewReportModal>
      )}
      {selectedReport && (
        <ReportModal
          report={selectedReport}
          onClose={() => setSelectedReport(null)}
          getUpdatedData={() => refetch()}
          categories={categories}
        />
      )}
    </>
  )
}
