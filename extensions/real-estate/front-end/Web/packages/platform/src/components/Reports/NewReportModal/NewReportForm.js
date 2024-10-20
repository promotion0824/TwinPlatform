import { useState, useEffect } from 'react'
import { Flex, Form, ModalSubmitButton, useApi } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import NewReportDetails from './NewReportDetails'
import NewReportConnections from './NewReportConnections'
import NotificationSettingsProvider from '../../../views/Admin/NotificationSettings/NotificationSettingsProvider'

function NewReportForm({ categories, setNewReportModal, getUpdatedData }) {
  const { t } = useTranslation()
  const api = useApi()
  const [isDisabled, setIsDisabled] = useState(true)
  const [newReportData, setNewReportData] = useState({
    metadata: {
      embedLocation: 'reportsTab',
      embedPath: undefined,
      groupId: undefined,
      reportId: undefined,
    },
    positions: [],
    type: 'sigmaReport',
  })
  const [positionData, setPositionData] = useState([])

  useEffect(() => {
    if (newReportData && newReportData.metadata) {
      const { name, embedLocation, embedPath, reportId, groupId } =
        newReportData.metadata
      if (name && embedLocation && (embedPath || reportId || groupId)) {
        setIsDisabled(false)
      } else {
        setIsDisabled(true)
      }
    }
  }, [newReportData])

  const getReportDetails = (data) => ({
    name: data.metadata.name,
    sites: data.sites,
    category: data.metadata.category,
  })

  const getConnectionDetails = (data) => ({
    embedLocation: data.metadata.embedLocation,
    embedPath: data.metadata.embedPath,
    reportId: data.metadata.reportId,
    groupId: data.metadata.groupId,
    type: data.type,
  })

  const addReport = async () => {
    newReportData.positions = positionData
    delete newReportData.sites
    await api.post(`/api/dashboard`, newReportData)
    setNewReportModal(false)
    getUpdatedData()
  }

  const getPositions = (data) => {
    setPositionData(data)
  }

  return (
    <Form>
      {() => (
        <Flex fill="header">
          <Flex>
            <>
              <NotificationSettingsProvider>
                <NewReportDetails
                  onChange={(newReportDetails) =>
                    setNewReportData((prevData) => ({
                      ...prevData,
                      metadata: {
                        ...newReportData.metadata,
                        name: newReportDetails?.name,
                        category: newReportDetails?.category,
                      },
                      sites: newReportDetails?.sites,
                    }))
                  }
                  value={getReportDetails(newReportData)}
                  getPositionsData={getPositions}
                  categoriesList={categories}
                />

                <NewReportConnections
                  onChange={(newConnectionDetails) =>
                    setNewReportData((prevData) => ({
                      ...prevData,
                      metadata: {
                        ...newReportData.metadata,
                        embedLocation: newConnectionDetails.embedLocation
                          ? newConnectionDetails?.embedLocation
                          : 'reportsTab',
                        embedPath: newConnectionDetails?.embedPath,
                        reportId: newConnectionDetails?.reportId,
                        groupId: newConnectionDetails?.groupId,
                      },
                      type: newConnectionDetails.type
                        ? newConnectionDetails?.type
                        : 'sigmaReport',
                    }))
                  }
                  value={getConnectionDetails(newReportData)}
                />
              </NotificationSettingsProvider>
            </>
          </Flex>
          <ModalSubmitButton onClick={addReport} disabled={isDisabled}>
            {t('plainText.save')}
          </ModalSubmitButton>
        </Flex>
      )}
    </Form>
  )
}

export default NewReportForm
