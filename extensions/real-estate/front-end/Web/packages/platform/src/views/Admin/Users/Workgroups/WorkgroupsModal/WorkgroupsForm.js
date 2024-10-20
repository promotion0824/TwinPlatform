import { useState } from 'react'
import {
  useFetchRefresh,
  Fieldset,
  Flex,
  Form,
  ValidationError,
  Input,
  ModalSubmitButton,
  Select,
  Option,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import DeleteWorkgroupModal from './DeleteWorkgroupModal'
import Users from './Users'

export default function WorkgroupsForm({ workgroup, sites, users, modal }) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()

  const [selectedWorkgroup, setSelectedWorkgroup] = useState()

  const isNewWorkgroup = workgroup.id == null

  function handleSiteChange(siteId, form) {
    form.setData((prevData) => ({
      ...prevData,
      siteId,
      users: [],
    }))
  }

  function handleSubmit(form) {
    if (form.data.siteId == null) {
      throw new ValidationError({
        name: 'siteId',
        message: t('messages.siteGiven'),
      })
    }

    if (!isNewWorkgroup) {
      return form.api.put(
        `/api/management/sites/${form.data.siteId}/workgroups/${form.data.id}`,
        {
          name: form.data.name,
          memberIds: form.data.users.map((user) => user.id),
        }
      )
    }

    return form.api.post(
      `/api/management/sites/${form.data.siteId}/workgroups`,
      {
        name: form.data.name,
        memberIds: form.data.users.map((user) => user.id),
      }
    )
  }

  function handleSubmitted() {
    modal.close()

    fetchRefresh('workgroups')
  }

  return (
    <>
      <Form
        defaultValue={workgroup}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        {(form) => (
          <Flex fill="header">
            <Fieldset>
              <Select
                name="siteId"
                label={t('labels.site')}
                required
                readOnly={!isNewWorkgroup}
                onChange={(siteId) => handleSiteChange(siteId, form)}
              >
                {sites.map((site) => (
                  <Option key={site.siteId} value={site.siteId}>
                    {site.siteName}
                  </Option>
                ))}
              </Select>
              {form.data.siteId != null && (
                <>
                  <Input name="name" label={t('labels.name')} required />
                  <Users users={users} />
                </>
              )}
              {!isNewWorkgroup && (
                <Button
                  kind="negative"
                  onClick={() => setSelectedWorkgroup(workgroup)}
                  css={`
                    align-self: end;
                  `}
                >
                  {t('headers.deleteWorkgroup')}
                </Button>
              )}
            </Fieldset>
            <ModalSubmitButton>{t('plainText.save')}</ModalSubmitButton>
          </Flex>
        )}
      </Form>
      {selectedWorkgroup != null && (
        <DeleteWorkgroupModal
          workgroup={selectedWorkgroup}
          onClose={() => setSelectedWorkgroup()}
        />
      )}
    </>
  )
}
