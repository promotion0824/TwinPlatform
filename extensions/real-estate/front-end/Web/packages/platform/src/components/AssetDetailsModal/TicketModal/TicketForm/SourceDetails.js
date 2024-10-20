import { getSyncStatus, titleCase } from '@willow/common'
import { Fieldset, Input, SyncStatusComponent, useForm } from '@willow/ui'
import { Group, Stack } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'

function SourceDetails({ ticket, isPolling }) {
  const form = useForm()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  // isPolling, checks if its a walmart user + mappedEnabled && ticketSync FF are turned on.
  const syncStatus = isPolling
    ? ticket.id
      ? getSyncStatus(ticket)
      : null
    : null

  return (
    <div>
      <Fieldset icon="reset" legend={t('labels.sourceDetails')}>
        {form.data?.id != null && (
          <>
            <Input name="sourceName" label={t('labels.source')} readOnly />
            <Input name="externalId" label={t('plainText.sourceId')} readOnly />
            {syncStatus && (
              <Stack>
                <Group>
                  {titleCase({
                    text: t('plainText.syncStatus'),
                    language,
                  })}
                </Group>
                <Group>
                  <SyncStatusComponent syncStatus={syncStatus} />
                </Group>
              </Stack>
            )}
          </>
        )}
      </Fieldset>
    </div>
  )
}

export default SourceDetails
