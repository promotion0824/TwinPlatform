import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Input, Fieldset, Flex, Select, Option, useForm } from '@willow/ui'

function Connections({ onChange, report }) {
  const { t } = useTranslation()
  const { data, setData } = useForm()
  const [isSigma, setIsSigma] = useState(false)
  const [reportType, setReportType] = useState(data?.type)
  const [newEmbedPath, setNewEmbedPath] = useState(data?.metadata?.embedPath)
  const [newGroupId, setNewGroupId] = useState(data?.metadata?.groupId)
  const [newReportId, setNewReportId] = useState(data?.metadata?.reportId)

  const setType = (rt) => {
    if (rt === 'sigmaReport') {
      setIsSigma(true)
    } else {
      setIsSigma(false)
    }
  }

  const handleReportTypeChange = (type) => {
    setData((prevData) => {
      const { metadata } = prevData
      if (type === 'sigmaReport') {
        setNewGroupId('')
        setNewReportId('')
        return {
          ...prevData,
          metadata: {
            ...metadata,
            reportId: '',
            groupId: '',
          },
          type,
        }
      }
      setNewEmbedPath('')
      return {
        ...prevData,
        metadata: {
          ...metadata,
          embedPath: '',
        },
        type,
      }
    })
    setReportType(type)
    setType(type)
  }

  const handleEmbedPathChange = (path) => {
    setData((prevData) => {
      const { metadata } = prevData
      return {
        ...prevData,
        metadata: {
          ...metadata,
          embedPath: path,
        },
      }
    })
    setNewEmbedPath(path)
  }

  const handleGroupIdChange = (gid) => {
    if (gid) {
      setData((prevData) => {
        const { metadata } = prevData
        return {
          ...prevData,
          metadata: {
            ...metadata,
            groupId: gid,
          },
        }
      })
      setNewGroupId(gid)
    } else {
      setNewGroupId(gid)
      setData((prevData) => {
        const newObj = { ...prevData }
        delete newObj.metadata.groupId
        return newObj
      })
    }
  }

  const handleReportIdChange = (rid) => {
    if (rid) {
      setData((prevData) => {
        const { metadata } = prevData
        return {
          ...prevData,
          metadata: {
            ...metadata,
            reportId: rid,
          },
        }
      })
      setNewReportId(rid)
    } else {
      setNewReportId(rid)
      setData((prevData) => {
        const newObj = { ...prevData }
        delete newObj.metadata.reportId
        return newObj
      })
    }
  }

  useEffect(() => {
    setType(data.type)
    onChange(data)
  }, [
    data,
    onChange,
    newGroupId,
    newReportId,
    reportType,
    newEmbedPath,
    report.metadata.embedPath,
  ])

  return (
    <>
      <Fieldset icon="assets" legend={t('plainText.connections')}>
        <>
          <Flex horizontal fill="equal" size="large">
            <Select
              name="type"
              label={t('labels.reportType')}
              value={reportType}
              onChange={handleReportTypeChange}
            >
              <Option value="sigmaReport">Sigma</Option>
              <Option value="powerBIReport">PowerBi</Option>
            </Select>
            <Select
              name="embedLocation"
              label={t('labels.embedLocation')}
              placeholder={t('plainText.unspecified')}
              value={data?.metadata?.embedLocation}
              disabled
            >
              <Option value="reportsTab">Reports Tab</Option>
              <Option value="kpiDashboard">KPI Dashboard</Option>
            </Select>
          </Flex>

          {isSigma ? (
            <Flex fill="equal" horizontal>
              <Input
                name="link"
                label={t('labels.link')}
                value={newEmbedPath}
                onChange={handleEmbedPathChange}
              />
            </Flex>
          ) : (
            <>
              <Flex fill="equal" horizontal>
                <Input
                  name="groupId"
                  label={t('labels.groupId')}
                  value={newGroupId}
                  onChange={handleGroupIdChange}
                />
              </Flex>
              <Flex fill="equal" horizontal>
                <Input
                  name="reportId"
                  value={newReportId}
                  label={t('labels.reportId')}
                  onChange={handleReportIdChange}
                />
              </Flex>
            </>
          )}
        </>
      </Fieldset>
    </>
  )
}

export default Connections
