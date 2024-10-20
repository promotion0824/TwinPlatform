import { Checkbox, Combobox, useCombobox } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'

export default function AssigneesSelect({
  assignees,
  selectedAssignees,
  onToggle,
}: {
  assignees: string[]
  selectedAssignees: string[]
  onToggle: (assignee?: string) => void
}) {
  const { t } = useTranslation()
  const combobox = useCombobox({
    onDropdownClose: () => combobox.resetSelectedOption(),
    onDropdownOpen: () => combobox.updateSelectedOptionIndex('active'),
  })

  const hasValue = selectedAssignees.length > 0
  const inputSummary =
    selectedAssignees.length > 1
      ? t('plainText.multiple')
      : selectedAssignees[0]

  return (
    <Combobox store={combobox} onOptionSubmit={onToggle}>
      <Combobox.Target>
        <Combobox.InputBase
          label={t('labels.assignee')}
          component="button"
          type="button"
          pointer
          suffix={
            hasValue ? (
              <Combobox.ClearButton
                onClear={onToggle}
                aria-label="clear"
                onMouseDown={(event) => event.preventDefault()}
                // @ts-expect-error // can remove after alpha.61 when we have default size set for ClearButton
                size="xs"
              />
            ) : (
              <Combobox.Chevron
                // can remove after alpha.61 when we have default size set for Chevron
                size="xs"
              />
            )
          }
          suffixPointerEvents={hasValue ? 'all' : 'none'}
          onClick={() => combobox.toggleDropdown()}
        >
          {inputSummary || (
            <Combobox.InputPlaceholder
              css={{
                textTransform: 'capitalize',
              }}
            >
              {t('labels.selectAssignee')}
            </Combobox.InputPlaceholder>
          )}
        </Combobox.InputBase>
      </Combobox.Target>

      <Combobox.Dropdown>
        <Combobox.Options>
          {assignees.map((assignee) => (
            <Combobox.Option
              value={assignee}
              key={assignee}
              active={selectedAssignees.includes(assignee)}
            >
              <Checkbox
                checked={selectedAssignees.includes(assignee)}
                label={assignee}
                value={assignee}
                aria-hidden
                style={{
                  pointerEvents: 'none',
                }}
                // need this to remove HTML warning
                onChange={() => undefined}
              />
            </Combobox.Option>
          ))}
        </Combobox.Options>
      </Combobox.Dropdown>
    </Combobox>
  )
}
