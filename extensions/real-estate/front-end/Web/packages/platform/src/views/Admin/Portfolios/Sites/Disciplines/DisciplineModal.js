import { useState } from 'react'
import { useParams } from 'react-router'
import {
  useSnackbar,
  Fieldset,
  Flex,
  Form,
  Input,
  Modal,
  ModalSubmitButton,
  Select,
  Option,
  ValidationError,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import DeleteDisciplineModal from './DeleteDisciplineModal'

export default function DisciplineModal({
  disciplines,
  discipline,
  sortedDisciplineGroups,
  updateDisciplinesSortOrder,
  onClose,
}) {
  const snackbar = useSnackbar()
  const params = useParams()
  const { t } = useTranslation()

  const [showDeleteDisciplineModal, setShowDeleteDisciplineModal] =
    useState(false)

  const isNewDiscipline = discipline.id == null

  async function handleSubmit(form) {
    if (!form.data.name?.trim()) {
      throw new ValidationError({
        name: 'name',
        message: t('messages.nameRequired'),
      })
    }

    if (!form.data.prefix?.trim()) {
      throw new ValidationError({
        name: 'prefix',
        message: t('messages.codeRequired'),
      })
    }

    // Check for duplicates of "prefix" discipline code
    // Same "prefix" can exist as long as it belongs to 2D and 3D respectively
    const existingDiscipline = disciplines.find(
      (x) =>
        x.prefix.toLowerCase() === form.data.prefix.trim().toLowerCase() &&
        x.id !== discipline.id &&
        x.is3D === form.data.is3D
    )
    if (existingDiscipline) {
      throw new ValidationError({
        name: 'prefix',
        message: t('messages.codeUniquePerType'),
      })
    }

    if (!isNewDiscipline) {
      const updatedModuleType = await form.api.put(
        `/api/sites/${params.siteId}/ModuleTypes/${discipline.id}`,
        {
          name: form.data.name.trim(),
          prefix: form.data.prefix.trim(),
          moduleGroup: form.data.moduleGroup?.trim(),
          is3D: form.data.is3D,
          canBeDeleted: form.data.canBeDeleted,
          isDefault: form.data.isDefault,
          sortOrder: discipline.sortOrder,
        }
      )

      let nextDisciplineGroups
      if (discipline.group?.id === updatedModuleType.group?.id) {
        // If updated module type belongs to same group (or it didn't have group in the first place)
        // just update with new values
        nextDisciplineGroups = [...sortedDisciplineGroups]
        let disciplineToUpdate = null
        for (let i = 0; i < nextDisciplineGroups.length; i++) {
          const disciplineGroup = nextDisciplineGroups[i]
          if (disciplineGroup.id === discipline.id) {
            disciplineToUpdate = disciplineGroup
            break
          }
          for (let j = 0; j < disciplineGroup.disciplinesInGroup?.length; j++) {
            const disciplineInGroup = disciplineGroup.disciplinesInGroup[j]
            if (disciplineInGroup.id === discipline.id) {
              disciplineToUpdate = disciplineInGroup
              break
            }
          }
          if (disciplineToUpdate) break
        }
        if (disciplineToUpdate) {
          disciplineToUpdate.name = updatedModuleType.name
          disciplineToUpdate.prefix = updatedModuleType.prefix
          disciplineToUpdate.canBeDeleted = updatedModuleType.canBeDeleted
          disciplineToUpdate.isDefault = updatedModuleType.isDefault
        }
      } else {
        // If module type now belongs to different group we first need to remove previous value
        nextDisciplineGroups = [
          ...sortedDisciplineGroups.flatMap((prevDisciplineGroup) => {
            if (prevDisciplineGroup.id !== discipline.id) {
              const convertedGroup = {
                ...prevDisciplineGroup,
                ...(prevDisciplineGroup.disciplinesInGroup && {
                  disciplinesInGroup:
                    prevDisciplineGroup.disciplinesInGroup.filter(
                      (prevDiscipline) => prevDiscipline.id !== discipline.id
                    ),
                }),
              }
              if (
                !convertedGroup.disciplinesInGroup ||
                convertedGroup.disciplinesInGroup.length
              )
                return [convertedGroup]
            }
            // Skip and remove element from array
            return []
          }),
        ]
        // And then add newly created module type to appropriate position
        if (updatedModuleType.group) {
          // First check if group already exist. If it does add newly added module type to existing group.
          const existingGroup = nextDisciplineGroups.find(
            (group) => group.id === updatedModuleType.group.id
          )
          if (existingGroup) {
            existingGroup.disciplinesInGroup.push(updatedModuleType)
          } else {
            // If it doesn't create new group.
            nextDisciplineGroups = [
              ...nextDisciplineGroups,
              {
                isModuleGroupParent: true,
                disciplinesInGroup: [updatedModuleType],
                ...updatedModuleType.group,
              },
            ]
          }
        } else {
          // Add newly added module type with highest sort order
          nextDisciplineGroups = [...nextDisciplineGroups, updatedModuleType]
        }
      }

      updateDisciplinesSortOrder(nextDisciplineGroups)
      return
    }

    const newModuleType = await form.api.post(
      `/api/sites/${params.siteId}/ModuleTypes`,
      {
        name: form.data.name.trim(),
        prefix: form.data.prefix.trim(),
        moduleGroup: form.data.moduleGroup?.trim(),
        is3D: form.data.is3D,
        canBeDeleted: form.data.canBeDeleted,
        isDefault: form.data.isDefault,
        sortOrder: 0,
      }
    )
    let nextDisciplineGroups
    if (newModuleType.group) {
      // First check if group already exist. If it does add newly added module type to existing group.
      const existingGroup = sortedDisciplineGroups.find(
        (group) => group.id === newModuleType.group.id
      )
      if (existingGroup) {
        existingGroup.disciplinesInGroup.push(newModuleType)
        nextDisciplineGroups = [...sortedDisciplineGroups]
      } else {
        // If it doesn't create new group.
        nextDisciplineGroups = [
          ...sortedDisciplineGroups,
          {
            isModuleGroupParent: true,
            disciplinesInGroup: [newModuleType],
            ...newModuleType.group,
          },
        ]
      }
    } else {
      // Add newly added module type with highest sort order
      nextDisciplineGroups = [...sortedDisciplineGroups, newModuleType]
    }
    updateDisciplinesSortOrder(nextDisciplineGroups)
  }

  function handleSubmitted(form) {
    if (form.response?.message != null) {
      snackbar.show(form.response.message, {
        icon: 'ok',
      })
    }

    form.modal.close()
  }

  return (
    <Modal
      header={
        isNewDiscipline
          ? t('headers.addNewDiscipline')
          : t('headers.editDiscipline')
      }
      size="small"
      onClose={onClose}
    >
      <Form
        defaultValue={{ ...discipline }}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        {(form) => (
          <>
            <Flex fill="header">
              <div>
                <Fieldset>
                  <Input
                    name="name"
                    label={t('labels.disciplineName')}
                    required
                  />
                  <Input
                    name="prefix"
                    label={t('labels.disciplineCode')}
                    required
                  />
                  <Input name="moduleGroup" label={t('labels.group')} />
                  <Select
                    name="canBeDeleted"
                    label={t('labels.deletable')}
                    value={
                      form.data.canBeDeleted
                        ? t('plainText.yes')
                        : t('plainText.no')
                    }
                  >
                    <Option value={false}>{t('plainText.no')}</Option>
                    <Option value>{t('plainText.yes')}</Option>
                  </Select>
                  <Select
                    name="is3D"
                    label={t('labels.type')}
                    value={
                      form.data.is3D ? t('plainText.3D') : t('plainText.2D')
                    }
                    disabled
                  >
                    <Option value={false}>{t('plainText.2D')}</Option>
                    <Option value>{t('plainText.3D')}</Option>
                  </Select>
                  <Select
                    name="isDefault"
                    label={t('labels.defaultDisplay')}
                    value={
                      form.data.isDefault
                        ? t('plainText.yes')
                        : t('plainText.no')
                    }
                  >
                    <Option value={false}>{t('plainText.no')}</Option>
                    <Option value>{t('plainText.yes')}</Option>
                  </Select>
                </Fieldset>
                {!isNewDiscipline && (
                  <>
                    <hr />
                    <Flex padding="extraLarge">
                      <Button
                        kind="negative"
                        onClick={() => setShowDeleteDisciplineModal(true)}
                        css={`
                          align-self: end;
                        `}
                      >
                        {t('headers.deleteDiscipline')}
                      </Button>
                    </Flex>
                  </>
                )}
              </div>
              <ModalSubmitButton>{t('plainText.save')}</ModalSubmitButton>
            </Flex>
            {showDeleteDisciplineModal && (
              <DeleteDisciplineModal
                discipline={discipline}
                onClose={() => setShowDeleteDisciplineModal(false)}
              />
            )}
          </>
        )}
      </Form>
    </Modal>
  )
}
