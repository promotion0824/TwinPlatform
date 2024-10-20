import { useTranslation } from 'react-i18next'
import { Input, Fieldset, Flex, Select, Option } from '@willow/ui'

function NewConnections({ value, onChange }) {
  const { t } = useTranslation()

  return (
    <>
      <Fieldset icon="assets" legend={t('plainText.connections')}>
        <>
          <Flex horizontal fill="equal" size="large">
            <Select
              name="type"
              label={t('labels.reportType')}
              cache
              value={value?.type || 'sigmaReport'}
              onChange={(type) => onChange({ ...value, type })}
            >
              <Option value="sigmaReport">Sigma</Option>
              <Option value="powerBIReport">PowerBi</Option>
            </Select>
            <Select
              name="embedLocation"
              label={t('labels.embedLocation')}
              placeholder={t('plainText.unspecified')}
              cache
              value={value?.embedLocation || 'reportsTab'}
              onChange={(embedLocation) =>
                onChange({ ...value, embedLocation })
              }
              disabled
            >
              <Option value="reportsTab">Reports Tab</Option>
              <Option value="kpiDashboard">KPI Dashboard</Option>
            </Select>
          </Flex>

          {value.type === 'sigmaReport' || value.type == null ? (
            <Flex fill="equal" horizontal>
              <Input
                name="link"
                label={t('labels.link')}
                value={value?.embedPath}
                onChange={(embedPath) => onChange({ ...value, embedPath })}
              />
            </Flex>
          ) : (
            <>
              <Flex fill="equal" horizontal>
                <Input
                  name="groupId"
                  label={t('labels.groupId')}
                  value={value?.groupId}
                  onChange={(groupId) => onChange({ ...value, groupId })}
                />
              </Flex>
              <Flex fill="equal" horizontal>
                <Input
                  name="reportId"
                  value={value?.reportId}
                  label={t('labels.reportId')}
                  onChange={(reportId) => onChange({ ...value, reportId })}
                />
              </Flex>
            </>
          )}
        </>
      </Fieldset>
    </>
  )
}

export default NewConnections
