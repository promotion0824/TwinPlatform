import _ from 'lodash'
import { useState } from 'react'
import { Flex, Form, ModalSubmitButton, useApi, useSnackbar } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import Actions from './Actions'
import Connections from './Connections'
import Header from './Header'
import ReportDetails from './ReportDetails'

function ReportForm({ report, onClose, getUpdatedData, categories }) {
  const { t } = useTranslation()
  const api = useApi()
  const snackbar = useSnackbar()
  const [isDisabled, setIsDisabled] = useState(false)
  const [editReportData, setEditReportData] = useState({})

  // this function checks if anything in the form is changed to enable/disable the update button.
  const checkEquality = (r, d) => {
    if (
      _.isEqual(r, d) ||
      (d.type === 'powerBIReport' &&
        (!d.metadata.groupId || !d.metadata.reportId)) ||
      (d.type === 'sigmaReport' && !d.metadata.embedPath)
    ) {
      setIsDisabled(true)
    } else {
      setIsDisabled(false)
    }
  }

  const getDetailsData = (detailsData) => {
    checkEquality(report, detailsData)
    setEditReportData(detailsData)
  }

  const getConnectionData = (connectionsData) => {
    checkEquality(report, connectionsData)
    setEditReportData(connectionsData)
  }

  const handleUpdateReport = async () => {
    try {
      await api.put(`/api/dashboard/${report.id}`, editReportData)
      onClose(false)
      getUpdatedData()
    } catch (err) {
      snackbar.show(t('plainText.errorOccurred'))
    }
  }

  return (
    <Form defaultValue={report}>
      {() => (
        <Flex fill="header">
          <Flex>
            <Header />
            {report.id != null && (
              <>
                <ReportDetails
                  report={report}
                  onChange={getDetailsData}
                  categoriesList={categories}
                />
                <Connections report={report} onChange={getConnectionData} />
                <Actions
                  report={report}
                  onClose={onClose}
                  getUpdatedData={getUpdatedData}
                />
              </>
            )}
          </Flex>
          {report.status !== 'closed' && (
            <ModalSubmitButton
              disabled={isDisabled}
              onClick={handleUpdateReport}
            >
              {t('plainText.update')}
            </ModalSubmitButton>
          )}
        </Flex>
      )}
    </Form>
  )
}

export default ReportForm
