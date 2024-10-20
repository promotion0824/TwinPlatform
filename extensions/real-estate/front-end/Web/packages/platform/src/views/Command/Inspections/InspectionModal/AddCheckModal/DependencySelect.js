import { useForm, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function DependencyCheck({ checks }) {
  const form = useForm()
  const { t } = useTranslation()

  const currentIndex = checks.findIndex(
    (check) => check.localId === form.data.localId
  )

  const listChecks = checks
    .filter((check, i) => currentIndex === -1 || i < currentIndex)
    .filter((check) => check.type === 'List')
    .filter((check) => check.localId !== form.data.localId)

  if (listChecks.length === 0) {
    return null
  }

  const dependencyCheck = listChecks.find(
    (listCheck) => listCheck.name === form.data.dependencyName
  )

  return (
    <>
      <Select
        name="dependencyName"
        label={t('labels.dependency')}
        placeholder={t('placeholder.selectDependency')}
        onChange={(dependencyName) => {
          form.setData((prevData) => ({
            ...prevData,
            dependencyName,
            dependencyValue: null,
          }))
        }}
      >
        {listChecks.map((check) => (
          <Option key={check.localId} value={check.name}>
            {check.name}
          </Option>
        ))}
      </Select>
      {dependencyCheck != null && (
        <Select
          name="dependencyValue"
          label={t('labels.value')}
          placeholder={t('placeholder.selectValue')}
        >
          {dependencyCheck.typeValue.map((value) => (
            <Option key={value}>{value}</Option>
          ))}
        </Select>
      )}
    </>
  )
}
