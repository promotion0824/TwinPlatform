import { titleCase, priorities } from '@willow/common'
import { useForm, Select, Option, getPriorityTranslatedName } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import PriorityPill from '../PriorityPill/PriorityPill'

export default function PrioritySelect() {
  const form = useForm()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <Select
      name="priority"
      label={t('labels.priority')}
      required
      header={() =>
        form.data.priority != null && (
          <PriorityPill priorityId={form.data.priority} />
        )
      }
      unselectable
      isPillSelect
    >
      {priorities.map((priority) => (
        <Option key={priority.id} value={priority.id}>
          {titleCase({
            text: getPriorityTranslatedName(t, priority.id) ?? '',
            language,
          })}
        </Option>
      ))}
    </Select>
  )
}
