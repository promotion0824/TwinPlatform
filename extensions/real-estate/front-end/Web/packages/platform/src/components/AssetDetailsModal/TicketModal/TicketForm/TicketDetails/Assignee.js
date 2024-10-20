import {
  caseInsensitiveSort,
  useForm,
  NotFound,
  Pill,
  Select,
  Option,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { css } from 'twin.macro'

export default function Assignee() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      name="assignee"
      errorName="assigneeId"
      label={t('labels.assignee')}
      placeholder={t('plainText.unassigned')}
      disabled={!form.data.siteId}
      url={`/api/sites/${form.data.siteId}/possibleTicketAssignees`}
      header={(assignee) => (
        <Pill
          css={css(({ theme }) => ({
            '&&&': {
              color: theme.color.neutral.fg.default,
            },
          }))}
        >
          {assignee != null ? assignee.name : t('plainText.unassigned')}
        </Pill>
      )}
      isPillSelect
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
