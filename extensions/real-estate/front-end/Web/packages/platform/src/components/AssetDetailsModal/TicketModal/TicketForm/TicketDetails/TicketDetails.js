import styled from 'styled-components'
import _ from 'lodash'
import {
  useForm,
  Fieldset,
  Select,
  Option,
  Input,
  TextArea,
  caseInsensitiveSort,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import Assignee from './Assignee'
import CategorySelect from './CategorySelect'
import PrioritySelect from '../../../../PrioritySelect/PrioritySelect'
import TicketStatusSelect from '../../../../TicketStatusSelect/TicketStatusSelect.tsx'
import ScheduledTicketTasks from './ScheduledTicketTasks'
import JobTypeSelect from './JobTypeSelect'

export default function TicketDetails({
  isMappedEnabled = false,
  ticket,
  categories = {},
  twins = [],
  submitted = false,
  ticketSubStatus = [],
}) {
  const form = useForm()
  const { t } = useTranslation()
  const { jobTypes = [], servicesNeeded = [] } = categories
  const selectedTwinId =
    twins.find(({ twin }) => twin.siteId === form.data.siteId)?.twin?.id ?? ''
  const filteredServicesNeeded =
    servicesNeeded.find(
      ({ spaceTwinId }) =>
        selectedTwinId === spaceTwinId || spaceTwinId.length === 0
    )?.serviceNeededList ?? []

  return (
    <Fieldset icon="details" legend={t('plainText.ticketDetails')}>
      <FlexContainer>
        <PrioritySelect />
        <TicketStatusSelect
          readOnly={form.data.id == null || form.readOnly}
          onChange={(statusCode) => {
            form.setData((prevData) => ({
              ...prevData,
              statusCode,
              cause: form.initialData.cause !== '' ? prevData.cause : '',
              solution: form.initialData.cause !== '' ? prevData.solution : '',
            }))
          }}
          initialStatusCode={ticket?.statusCode}
          nextValidStatus={ticket?.nextValidStatus}
        />
      </FlexContainer>
      <FlexContainer>
        {/**
         * Displaying Job type and Sub Status for Mapped enabled customers only
         * Sub Status is required field only if ticket status is On Hold
         * Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/92786
         */}
        {isMappedEnabled && (
          <>
            <Select
              name="subStatus"
              label={_.startCase(t('plainText.subStatus'))}
              required={form.data.statusCode === 35}
              value={form.data.subStatusId}
              onChange={(subStatusId) =>
                form.setData((prevData) => ({
                  ...prevData,
                  subStatusId,
                }))
              }
            >
              {ticketSubStatus.map(({ id, name }) => (
                <Option key={id} value={id}>
                  {_.startCase(name)}
                </Option>
              ))}
            </Select>
            <JobTypeSelect
              jobTypesList={jobTypes}
              isFormSubmitted={submitted}
            />
          </>
        )}
      </FlexContainer>

      <FlexContainer>
        <CategorySelect
          isCategoryRequired={isMappedEnabled}
          submitted={submitted}
        />
        {/**
         * Displaying Job type and service needed dropdowns for Mapped enabled customers only
         */}
        {isMappedEnabled && (
          <Select
            name="servicesNeeded"
            label={_.startCase(t('plainText.serviceNeeded'))}
            disabled={filteredServicesNeeded.length === 0}
            value={form.data.serviceNeededId}
            required
            error={
              submitted &&
              !form.data.serviceNeededId &&
              t('messages.servicesNeededRequired')
            }
            onChange={(serviceNeededId) => {
              form.setData((prevData) => ({
                ...prevData,
                spaceTwinId: selectedTwinId,
                serviceNeededId,
              }))
            }}
          >
            {filteredServicesNeeded
              .filter(
                (x) => !x.categoryId || x.categoryId === form.data.categoryId
              )
              .sort(caseInsensitiveSort((service) => service.name))
              .map(({ id, name }) => (
                <Option key={id} value={id}>
                  {_.startCase(name)}
                </Option>
              ))}
          </Select>
        )}
      </FlexContainer>
      <FlexContainer>
        <AssigneeContainer>
          <Assignee />
        </AssigneeContainer>
      </FlexContainer>

      <FlexContainer>
        <Input
          name="summary"
          data-cy="ticketDetails-ticket-summary"
          label={t('labels.summary')}
          required
        />
      </FlexContainer>
      {ticket?.latitude && ticket?.longitude && (
        <FlexContainer>
          <Input
            name="latitude"
            data-cy="ticketDetails-ticket-latitude"
            label={t('labels.latitude')}
            readOnly
          />
          <Input
            name="longitude"
            data-cy="ticketDetails-ticket-longitude"
            label={t('labels.longitude')}
            readOnly
          />
        </FlexContainer>
      )}
      <TextArea
        name="description"
        data-cy="ticketDetails-ticket-description"
        label={t('labels.description')}
        required
      />
      {form.data.template && <ScheduledTicketTasks ticket={ticket} />}
    </Fieldset>
  )
}

const AssigneeContainer = styled.div({
  width: '100%',
  '& > *': {
    width: '100%',
  },
})

const FlexContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexFlow: 'row',
  width: '100%',

  '& > *': {
    width: '50%',
  },

  '& > *:not(:first-child)': {
    paddingLeft: theme.spacing.s16,
  },
}))
