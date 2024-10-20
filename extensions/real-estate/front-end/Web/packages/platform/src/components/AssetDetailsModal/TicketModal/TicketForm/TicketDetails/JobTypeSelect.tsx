import _ from 'lodash'
import { useForm, caseInsensitiveSort } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { Select } from '@willowinc/ui'
import { useEffect } from 'react'
import { JobType } from './types'

export default function JobTypeSelect({
  jobTypesList = [],
  isFormSubmitted = false,
}: {
  jobTypesList?: JobType[]
  isFormSubmitted?: boolean
}) {
  const form = useForm()
  const { t } = useTranslation()
  // this is temporary solution to set default job type
  // the default job type should be set in the backend
  const defaultJobTypeName = 'Corrective Maintenance'
  // if default job type is found, the default job type will be auto selected
  // and will filter out all other job types
  const defaultJobType = jobTypesList.find(
    (jobType) => jobType.name === defaultJobTypeName
  )
  useEffect(() => {
    if (defaultJobType) {
      form.setData((prevData) => ({
        ...prevData,
        jobTypeId: defaultJobType.id,
      }))
    }
  }, [form, defaultJobType])

  return (
    <Select
      data={jobTypesList
        .filter((x) => !defaultJobType || x.id === defaultJobType.id)
        .sort(caseInsensitiveSort((jobType) => jobType.name))
        .map(({ id, name }) => ({ label: _.startCase(name), value: id }))}
      name="jobType"
      label={_.startCase(t('plainText.jobType'))}
      value={form.data.jobTypeId}
      required
      error={
        isFormSubmitted && !form.data.jobTypeId && t('messages.jobTypeRequired')
      }
      onChange={(jobTypeId) =>
        form.setData((prevData) => ({
          ...prevData,
          jobTypeId,
        }))
      }
      defaultValue={defaultJobType ? defaultJobType.id : undefined}
    />
  )
}
