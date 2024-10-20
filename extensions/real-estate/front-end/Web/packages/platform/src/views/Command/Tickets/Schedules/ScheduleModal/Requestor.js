import {
  caseInsensitiveSort,
  useForm,
  Typeahead,
  TypeaheadContent,
  TypeaheadButton,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function Requestor() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Typeahead
      type="text"
      name="reporterName"
      data-test="ticket-requestor"
      label={t('labels.requestor')}
      icon="add"
      required
      selected={form.data.reporterId != null}
      url={`/api/sites/${form.data.siteId}/requestors`}
      onChange={(reporterName) => {
        form.clearError('reporterId')
        form.clearError('reporterName')
        form.clearError('reporterEmail')
        form.clearError('reporterCompany')
        form.setData((prevData) => ({
          ...prevData,
          reporterId: undefined,
          reporterName,
          reporterPhone: '',
          reporterEmail: '',
          reporterCompany: '',
        }))
      }}
      onSelect={(reporter) => {
        form.clearError('reporterId')
        form.clearError('reporterName')
        form.clearError('reporterEmail')
        form.clearError('reporterCompany')
        form.setData((prevData) => ({
          ...prevData,
          reporterId: reporter.id,
          reporterName: reporter.name,
          reporterPhone: reporter.phone,
          reporterEmail: reporter.email,
          reporterCompany: reporter.company,
        }))
      }}
    >
      {(persons) => {
        const sortedPersons = persons.sort(
          caseInsensitiveSort((person) => person.name)
        )

        const requestors = sortedPersons
          .filter(
            (person) =>
              form.data.reporterId != null ||
              person.name
                .toLowerCase()
                .includes(form.data.reporterName.toLowerCase())
          )
          .filter((person) => person.type === 'reporter')
          .map((person) => ({
            id: person.id,
            name: person.name,
            email: person.email,
            phone: person.phone,
            company: person.company,
          }))

        const willowUsers = sortedPersons
          .filter(
            (person) =>
              form.data.reporterId != null ||
              person.name
                .toLowerCase()
                .includes(form.data.reporterName.toLowerCase())
          )
          .filter((person) => person.type !== 'reporter')
          .map((person) => ({
            id: person.id,
            name: person.name,
            email: person.email,
            phone: person.phone,
            company: person.company,
          }))

        const filteredReporters = sortedPersons.filter(
          (reporter) =>
            form.data.reporterId != null ||
            reporter.name
              .toLowerCase()
              .includes(form.data.reporterName.toLowerCase())
        )

        if (filteredReporters.length === 0) {
          return null
        }

        return (
          <TypeaheadContent>
            {requestors.length > 0 && (
              <TypeaheadButton type="header" disabled>
                {t('headers.requestors')}
              </TypeaheadButton>
            )}
            {requestors.map((person) => (
              <TypeaheadButton key={person.id} value={person}>
                {person.name}
              </TypeaheadButton>
            ))}
            {willowUsers.length > 0 && (
              <TypeaheadButton type="header" disabled>
                {t('plainText.willowUsers')}
              </TypeaheadButton>
            )}
            {willowUsers.map((person) => (
              <TypeaheadButton key={person.id} value={person}>
                {person.name}
              </TypeaheadButton>
            ))}
          </TypeaheadContent>
        )
      }}
    </Typeahead>
  )
}
