import {
  caseInsensitiveSort,
  useForm,
  NotFound,
  Pill,
  Select,
  Option,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function Assignee() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      errorName="assigneeId"
      label={t('labels.assignee')}
      placeholder={t('plainText.unassigned')}
      url={`/api/sites/${form.data.siteId}/possibleTicketAssignees`}
      header={(assignee) =>
        assignee != null ? (
          <Pill>{assignee.name}</Pill>
        ) : (
          <Pill>{t('plainText.unassigned')}</Pill>
        )
      }
      isPillSelect
      value={
        form.data.assigneeId != null
          ? {
              id: form.data.assigneeId,
              type: form.data.assigneeType,
              name: form.data.assigneeName,
            }
          : null
      }
      onChange={(assignee) => {
        form.setData((prevData) => ({
          ...prevData,
          assigneeId: assignee?.id,
          assigneeType: assignee?.id != null ? assignee.type : '',
          assigneeName: assignee?.id != null ? assignee.name : '',
        }))
      }}
    >
      {(assignees) => {
        const orderedAssignees = assignees.sort(
          caseInsensitiveSort((assignee) => assignee.name)
        )
        const customerUsers = orderedAssignees.filter(
          (assignee) => assignee.type === 'customerUser'
        )
        const workgroups = orderedAssignees.filter(
          (assignee) => assignee.type === 'workGroup'
        )

        return (
          <>
            <Option value={null}>- {t('plainText.unassigned')} -</Option>
            {customerUsers.length > 0 && (
              <>
                <Option type="header">{t('plainText.assignees')}</Option>
                {customerUsers.map((assignee) => (
                  <Option key={assignee.id} value={assignee}>
                    {assignee.name}
                  </Option>
                ))}
              </>
            )}
            {workgroups.length > 0 && (
              <>
                <Option type="header">{t('headers.workgroups')}</Option>
                {workgroups.map((assignee) => (
                  <Option key={assignee.id} value={assignee}>
                    {assignee.name}
                  </Option>
                ))}
              </>
            )}
            {assignees.length === 0 && (
              <NotFound>{t('plainText.noAssigneesFound')}</NotFound>
            )}
          </>
        )
      }}
    </Select>
  )
}
