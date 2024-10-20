import { titleCase } from '@willow/common'
import {
  Fieldset,
  Flex,
  Form,
  Input,
  Modal,
  ModalSubmitButton,
  Option,
  Select,
  useFetchRefresh,
  useSnackbar,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import DeleteFloorModal from './DeleteFloorModal'

export default function FloorModal({ floor, onClose }) {
  const fetchRefresh = useFetchRefresh()
  const snackbar = useSnackbar()
  const params = useParams()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [showDeleteFloorModal, setShowDeleteFloorModal] = useState(false)

  const isNewFloor = floor.id == null

  function handleSubmit(form) {
    if (!isNewFloor) {
      return form.api.put(`/api/sites/${params.siteId}/floors/${floor.id}`, {
        name: form.data.name,
        code: form.data.code,
        modelReference: form.data.modelReference,
        isSiteWide: form.data?.isSiteWide ?? false,
      })
    }

    return form.api.post(`/api/sites/${params.siteId}/floors`, {
      name: form.data.name,
      code: form.data.code,
      modelReference: form.data.modelReference,
      isSiteWide: form.data?.isSiteWide ?? false,
    })
  }

  function handleSubmitted(form) {
    if (form.response?.message != null) {
      snackbar.show(form.response.message, {
        icon: 'ok',
      })
    }

    form.modal.close()

    fetchRefresh('floors')
  }

  return (
    <Modal
      header={isNewFloor ? t('headers.addNewFloor') : t('headers.editFloor')}
      size="small"
      onClose={onClose}
    >
      <Form
        defaultValue={{ ...floor }}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        <Flex fill="header">
          <div>
            <Fieldset>
              <Input name="code" label={t('labels.floorCode')} required />
              <Input name="name" label={t('labels.floorName')} required />
              <Input name="modelReference" label={t('plainText.modelRef')} />
              <Select
                name="isSiteWide"
                label={titleCase({ language, text: t('labels.sitewide') })}
                placeholder={t('plainText.no')}
                unselectable
              >
                <Option value>{t('plainText.yes')}</Option>
                <Option value={false}>{t('plainText.no')}</Option>
              </Select>
            </Fieldset>
            {!isNewFloor && (
              <>
                <hr />
                <Flex padding="extraLarge">
                  <Button
                    kind="negative"
                    onClick={() => setShowDeleteFloorModal(true)}
                    css={`
                      align-self: end;
                    `}
                  >
                    {t('headers.deleteFloor')}
                  </Button>
                </Flex>
              </>
            )}
          </div>
          <ModalSubmitButton>{t('plainText.save')}</ModalSubmitButton>
        </Flex>
        {showDeleteFloorModal && (
          <DeleteFloorModal
            floor={floor}
            onClose={() => setShowDeleteFloorModal(false)}
          />
        )}
      </Form>
    </Modal>
  )
}
