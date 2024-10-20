import { useForm, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function GroupSelect({ siteId }) {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      data-cy="inspection-group-select"
      errorName="assignedWorkgroupId"
      label={t('labels.assignGroup')}
      placeholder={t('placeholder.selectGroup')}
      unselectable
      url={`/api/management/sites/${siteId}/workgroups`}
      header={() => form.data.assignedWorkgroupName}
      value={
        form.data.assignedWorkgroupId != null
          ? {
              id: form.data.assignedWorkgroupId,
              name: form.data.assignedWorkgroupName,
            }
          : null
      }
      onChange={(workgroup) => {
        form.setData((prevData) => ({
          ...prevData,
          assignedWorkgroupId: workgroup?.id,
          assignedWorkgroupName: workgroup?.name ?? '',
        }))
      }}
    >
      {(workgroups) =>
        workgroups.map((workgroup) => (
          <Option
            data-cy="inspection-group-option"
            key={workgroup.id}
            value={workgroup}
          >
            {workgroup.name}
          </Option>
        ))
      }
    </Select>
  )
}
