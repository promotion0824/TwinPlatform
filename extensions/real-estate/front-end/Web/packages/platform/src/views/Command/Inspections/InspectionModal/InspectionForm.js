import { useRef, useState } from 'react'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import {
  useDateTime,
  DatePicker,
  Fieldset,
  Flex,
  Form,
  Input,
  ModalSubmitButton,
  NumberInput,
  Select,
  MessagePanel,
  Option,
  useSnackbar,
  ValidationError,
  useFeatureFlag,
} from '@willow/ui'
import { titleCase, formatDateTime } from '@willow/common'
import {
  Button,
  ButtonGroup,
  DataGrid,
  Icon,
  IconButton,
  Popover,
} from '@willowinc/ui'
import { useSites } from '../../../../providers'
import AddCheckModal from './AddCheckModal/AddCheckModal'
import AssetInput from './AssetInput'
import FloorSelect from './FloorSelect'
import GroupSelect from './GroupSelect'
import PauseModal from './PauseModal'
import styles from './InspectionModal.css'
import MultiAssets from './MultiAssets'

function areDependenciesValid(checks) {
  for (let i = 0; i < checks.length; i++) {
    const check = checks[i]

    if (check.dependencyId) {
      const dependencyIndex = checks.findIndex(
        (c) => c.id === check.dependencyId
      )

      if (dependencyIndex >= i) return false
    }
  }

  return true
}

const FrequencyContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  alignItems: 'end',
  gap: '1rem',
  width: '50%',
})

const ScheduleContainer = styled.div(({ theme }) => ({
  ...theme.font.body.sm.regular,
}))

const FrequencyValue = styled(NumberInput)({
  width: '85px',
})
const FrequencyUnitSelect = styled(Select)({
  width: '180px',
  textTransform: 'capitalize',
})

const FrequencyUnitOption = styled(Option)({
  textTransform: 'capitalize',
})

const StyledDataGrid = styled(DataGrid)({
  whiteSpace: 'nowrap',

  '&& .MuiDataGrid-cell:focus': {
    outline: 'none',
  },

  '.MuiDataGrid-row': {
    cursor: 'pointer',
  },

  // Using this instead of setting a height on an outer container
  // allows the DataGrid to have a dynamic height.
  '.MuiDataGrid-main': {
    '> div:nth-of-type(2)': {
      maxHeight: '500px',
    },
  },

  // Needed so that the "No checks found" message can be seen.
  // Needs to be set like this because there's no outer container providing a height.
  '.MuiDataGrid-virtualScrollerContent': {
    minHeight: '100px !important',
  },
})

const dayOptions = [
  { label: 'Mon', value: 'monday' },
  { label: 'Tue', value: 'tuesday' },
  { label: 'Wed', value: 'wednesday' },
  { label: 'Thu', value: 'thursday' },
  { label: 'Fri', value: 'friday' },
  { label: 'Sat', value: 'saturday' },
  { label: 'Sun', value: 'sunday' },
]

export default function InspectionForm({ inspection, readOnly }) {
  const dateTime = useDateTime()
  const featureFlags = useFeatureFlag()
  const sites = useSites()

  const snackbar = useSnackbar()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  // This key is based on a number so that it can be incremented when the table needs
  // to be redrawn due to invalid row reorder operation.
  const [dataGridKeyNum, setDataGridKeyNum] = useState(0)

  const isNewInspection = inspection.id == null

  const [selectedCheck, setSelectedCheck] = useState()
  const [pauseCheck, setPauseCheck] = useState()
  const latestTargetButton = useRef(null)
  const site = sites.find((s) => s.id === inspection.siteId)

  function handleAddCheckClick() {
    setSelectedCheck({
      name: '',
      type: null,
      typeValue: '',
      decimalPlaces: null,
      minValue: null,
      maxValue: null,
      multiplier: null,
      dependencyName: null,
      dependencyValue: null,
      pauseStartDate: null,
      pauseEndDate: null,
    })
  }

  const frequencyConstraints = {
    hours: { min: 1, max: 24 },
    days: { min: 1, max: 7 },
    weeks: { min: 1, max: 52 },
    months: { min: 1, max: 12 },
    years: { min: 1, max: 10 },
  }

  function validateFrequency(form) {
    const { frequency, frequencyUnit } = form.data
    if (!Number.isInteger(frequency)) {
      throw new ValidationError({
        name: 'frequency',
        message: t('plainText.mustBeInteger'),
      })
    }

    const { min, max } = frequencyConstraints[frequencyUnit]
    if (frequency < min || frequency > max) {
      throw new ValidationError({
        name: 'frequency',
        message: t('interpolation.inspectionFrequencyError', {
          frequencyUnit,
          min,
          max,
        }),
      })
    }
  }

  function handleSubmit(form) {
    validateFrequency(form)
    const startDate = dateTime(form.data.startDate).format('dateTimeLocal')
    const endDate = dateTime(form.data.endDate).format('dateTimeLocal')

    const frequencyDaysOfWeek = form.data.buttons
      .filter((btn) => btn.selected)
      .map((btn) => btn.value)

    if (!isNewInspection) {
      const checks = form.data.checks.map((check) => ({
        id: check.id,
        name: check.name,
        type: check.type,
        typeValue: Array.isArray(check.typeValue)
          ? check.typeValue.join('|')
          : check.typeValue,
        decimalPlaces: check.decimalPlaces,
        minValue: check.minValue,
        maxValue: check.maxValue,
        multiplier: check.multiplier ?? '1',
        dependencyId: check.dependencyId,
        dependencyName: check.dependencyName,
        dependencyValue: check.dependencyValue,
        pauseStartDate: check.pauseStartDate,
        pauseEndDate: check.pauseEndDate,
        canGenerateInsight: check.canGenerateInsight,
      }))

      return form.api.put(
        `/api/sites/${inspection.siteId}/inspections/${form.data.id}`,
        {
          name: form.data.name,
          frequency: form.data.frequency,
          frequencyUnit: form.data.frequencyUnit,
          frequencyDaysOfWeek: frequencyDaysOfWeek ?? [],
          startDate,
          endDate,
          assignedWorkgroupId: form.data.assignedWorkgroupId,
          checks: checks.length > 0 ? checks : null,
        }
      )
    } else {
      const checks = form.data.checks.map((check) => ({
        name: check.name,
        type: check.type,
        typeValue: Array.isArray(check.typeValue)
          ? check.typeValue.join('|')
          : check.typeValue,
        decimalPlaces: check.decimalPlaces,
        minValue: check.minValue,
        maxValue: check.maxValue,
        multiplier: check.multiplier ?? '1',
        dependencyName: check.dependencyName,
        dependencyValue: check.dependencyValue,
        pauseStartDate: check.pauseStartDate,
        pauseEndDate: check.pauseEndDate,
        canGenerateInsight: check.canGenerateInsight,
      }))

      return form.api.post(
        `/api/sites/${inspection.siteId}/inspections/batch-create`,
        {
          name: form.data.name,
          frequency: form.data.frequency,
          startDate,
          endDate,
          frequencyUnit: form.data.frequencyUnit,
          frequencyDaysOfWeek: frequencyDaysOfWeek ?? [],
          zoneId: inspection.zoneId,
          assetList: form.data.assets,
          assignedWorkgroupId: form.data.assignedWorkgroupId,
          checks: checks.length > 0 ? checks : null,
        }
      )
    }
  }

  function hasDependentChecks(check) {
    return inspection.checks.some((x) => x.dependencyId === check.id)
  }

  const getErrorMessage = (form, key) =>
    form.errors.find((formError) => formError.name?.toLowerCase() === key)
      ?.message

  const getScheduleText = (form) => {
    const scheduledDays = form.data.buttons
      .filter((btn) => btn.selected)
      .map((btn) => btn.label)
      .join(', ')

    const translationParams = {
      frequency: form.data.frequency,
      unit: form.data.frequencyUnit,
      days: scheduledDays
        ? `${t('plainText.on').toLowerCase()} ${scheduledDays}`
        : '',
      date: formatDateTime({
        value: form.data.startDate,
        language,
        timeZone: site.timeZone,
      }),
    }

    const shouldTranslate =
      form.data.frequency && form.data.frequencyUnit && form.data.startDate

    const scheduleText = shouldTranslate
      ? t('interpolation.daySelectionWithDate', translationParams)
      : ''

    return scheduleText
  }

  return (
    <Form
      defaultValue={{
        ...inspection,
        buttons: dayOptions.map((day) => ({
          label: day.label,
          value: day.value,
          selected: !!inspection.frequencyDaysOfWeek?.includes(day.value),
          disabled: !['weeks'].includes(inspection.frequencyUnit),
        })),
        checks: inspection.checks.map((check) => ({
          canGenerateInsight: false,
          ...check,
          localId: _.uniqueId(),
          typeValue:
            check.type === 'List'
              ? check.typeValue.split('|')
              : check.typeValue,
        })),
      }}
      readOnly={readOnly}
      onSubmit={handleSubmit}
      onSubmitted={(form) => form.modal.close('submitted')}
    >
      {(form) => (
        <>
          <Flex fill="header">
            <Flex>
              <Fieldset legend={t('plainText.assetAndLocation')}>
                <Input label={t('labels.site')} value={site.name} readOnly />
                {/**
                 * For new inspection, user can link multiple assets to single inspection
                 * For updating an existing inspection, we can see the corresponding floor and asset linked to it
                 */}
                {isNewInspection ? (
                  <Fieldset
                    padding="0"
                    legend={t('headers.assets')}
                    required={(form.data.assets || []).length === 0}
                    error={getErrorMessage(form, 'assetlist')}
                  >
                    <MultiAssets />
                  </Fieldset>
                ) : (
                  <Flex horizontal fill="equal" size="large">
                    <FloorSelect siteId={inspection.siteId} />
                    <AssetInput siteId={inspection.siteId} />
                  </Flex>
                )}
              </Fieldset>
              <Fieldset>
                <Flex horizontal fill="equal" size="large">
                  <Input name="name" label={t('labels.name')} />
                  <div />
                </Flex>
              </Fieldset>
              <Fieldset legend={t('plainText.assign')}>
                <Flex horizontal fill="equal" size="large">
                  <GroupSelect siteId={inspection.siteId} />
                  <div />
                </Flex>
              </Fieldset>
              <Fieldset legend={t('plainText.schedule')}>
                <Flex horizontal fill="equal" size="large">
                  <DatePicker
                    data-cy="inspection-start-date"
                    name="startDate"
                    label={t('labels.startDateAndTime')}
                    type="date-time"
                    placeholder={t('placeholder.selectStartTime')}
                    onChange={(nextStartDate) => {
                      form.setData((prevData) => {
                        const isStartDateGreater =
                          dateTime(nextStartDate).differenceInMilliseconds(
                            prevData.endDate
                          ) >= 0

                        return {
                          ...prevData,
                          startDate: nextStartDate,
                          endDate: isStartDateGreater ? null : prevData.endDate,
                        }
                      })
                    }}
                  />
                  <DatePicker
                    name="endDate"
                    label={t('labels.endDateAndTime')}
                    min={form.data.startDate}
                    type="date-time"
                    placeholder={t('placeholder.selectEndTime')}
                  />
                </Flex>

                <FrequencyContainer>
                  <FrequencyValue
                    name="frequency"
                    label={t('labels.frequency')}
                    min={frequencyConstraints[form.data.frequencyUnit]?.min}
                    max={frequencyConstraints[form.data.frequencyUnit]?.max}
                    format="0"
                    onChange={(nextFrequency) => {
                      form.setData((prevData) => ({
                        ...prevData,
                        frequency: nextFrequency,
                      }))
                    }}
                  />
                  <FrequencyUnitSelect
                    name="frequencyUnit"
                    onChange={(nextFrequencyUnit) => {
                      form.setData((prevData) => ({
                        ...prevData,
                        frequencyUnit: nextFrequencyUnit,
                        buttons: form.data.buttons.map((button) => ({
                          ...button,
                          selected: false,
                          disabled: !['weeks'].includes(nextFrequencyUnit),
                        })),
                      }))
                    }}
                  >
                    <FrequencyUnitOption value="hours" iconHidden="true">
                      {t('plainText.hours')}
                    </FrequencyUnitOption>
                    <FrequencyUnitOption value="days" iconHidden="true">
                      {t('plainText.days')}
                    </FrequencyUnitOption>
                    <FrequencyUnitOption value="weeks" iconHidden="true">
                      {t('plainText.weeks')}
                    </FrequencyUnitOption>
                    <FrequencyUnitOption value="months" iconHidden="true">
                      {t('plainText.months')}
                    </FrequencyUnitOption>
                    <FrequencyUnitOption value="years" iconHidden="true">
                      {t('plainText.years')}
                    </FrequencyUnitOption>
                  </FrequencyUnitSelect>
                </FrequencyContainer>

                <ScheduleContainer>
                  <div tw="mb-[10px]">
                    {titleCase({ text: t('labels.daySelection'), language })}
                  </div>
                  <ButtonGroup>
                    {form.data.buttons.map(({ label, selected, disabled }) => (
                      <Button
                        key={label}
                        size="medium"
                        disabled={disabled}
                        kind={selected ? 'primary' : 'secondary'}
                        onClick={() => {
                          form.setData((prevData) => ({
                            ...prevData,
                            buttons: form.data.buttons.map((btn) =>
                              btn.label === label
                                ? { ...btn, selected: !btn.selected }
                                : btn
                            ),
                          }))
                        }}
                      >
                        {label}
                      </Button>
                    ))}
                  </ButtonGroup>
                  <div tw="mt-[20px]">{getScheduleText(form) || <br />}</div>
                </ScheduleContainer>
              </Fieldset>
              <Flex size="medium" pharaoh="tes">
                <Fieldset
                  legend={t('plainText.checks')}
                  required={form.data.checks.length === 0}
                  error={getErrorMessage(form, 'checks')}
                  classNameChildrenCtn={styles.checksFieldset}
                >
                  <Flex fill="header" size="large">
                    <StyledDataGrid
                      columns={[
                        {
                          field: 'name',
                          headerName: t('labels.title'),
                          sortable: false,
                        },
                        {
                          field: 'type',
                          headerName: t('labels.type'),
                          renderCell: ({ value }) =>
                            /* Can only be one of these string value: Numeric, Total, List, or Date */
                            t('interpolation.plainText', {
                              key: value.toLowerCase(),
                            }),
                          sortable: false,
                        },
                        {
                          field: 'typeValue',
                          headerName: t('labels.value'),
                          renderCell: ({ value }) =>
                            Array.isArray(value) ? (
                              <Flex size="tiny">
                                {value.map((item, i) => (
                                  // eslint-disable-next-line react/no-array-index-key
                                  <div key={i}>{item}</div>
                                ))}
                              </Flex>
                            ) : (
                              value
                            ),
                          sortable: false,
                        },
                        {
                          field: 'minValue',
                          headerName: t('labels.min'),
                          sortable: false,
                        },
                        {
                          field: 'maxValue',
                          headerName: t('labels.max'),
                          sortable: false,
                        },

                        {
                          field: 'multiplier',
                          headerName: titleCase({
                            text: t('labels.multiplier'),
                            language,
                          }),
                          sortable: false,
                        },

                        {
                          field: 'dependencyName',
                          headerName: t('labels.dependency'),
                          renderCell: ({
                            row: { dependencyName, dependencyValue },
                          }) => (
                            <>
                              {dependencyName}
                              {dependencyName != null &&
                                dependencyValue != null && (
                                  <span>&nbsp;-&nbsp;</span>
                                )}
                              {dependencyValue}
                            </>
                          ),
                          sortable: false,
                        },
                        ...(!readOnly
                          ? [
                              {
                                field: 'actions',
                                headerName: t('plainText.actions'),
                                renderCell: ({ row }) => {
                                  const [popoverOpened, setPopoverOpened] =
                                    useState(false)

                                  return (
                                    <>
                                      <Popover
                                        closeOnClickOutside={false}
                                        onChange={setPopoverOpened}
                                        opened={
                                          pauseCheck === undefined &&
                                          popoverOpened
                                        }
                                        position="bottom-end"
                                        withinPortal
                                      >
                                        <Popover.Target>
                                          <IconButton
                                            icon={
                                              row.isPaused
                                                ? 'play_arrow'
                                                : 'pause'
                                            }
                                            kind="secondary"
                                            background="transparent"
                                            onClick={(e) => {
                                              e.stopPropagation()
                                              // We need to navigate up the DOM tree from svg because it will be replaced
                                              // with opposite icon
                                              latestTargetButton.current =
                                                e.target.parentNode.parentNode

                                              if (row.isPaused) {
                                                form.setData((prevData) => ({
                                                  ...prevData,
                                                  checks: prevData.checks.map(
                                                    (prevCheck) =>
                                                      prevCheck.localId ===
                                                      row.localId
                                                        ? {
                                                            ...prevCheck,
                                                            isPaused: false,
                                                            pauseStartDate:
                                                              null,
                                                            pauseEndDate: null,
                                                          }
                                                        : prevCheck
                                                  ),
                                                }))

                                                setPopoverOpened(
                                                  hasDependentChecks(row)
                                                )
                                              } else {
                                                setPauseCheck(row)
                                                setPopoverOpened(
                                                  hasDependentChecks(row)
                                                )
                                              }
                                            }}
                                          />
                                        </Popover.Target>
                                        <Popover.Dropdown>
                                          <MessagePanel
                                            icon="insights"
                                            iconColor="red"
                                            title={
                                              row.isPaused
                                                ? 'Pause Check'
                                                : 'Unpause Check'
                                            }
                                            onClose={() =>
                                              setPopoverOpened(false)
                                            }
                                          >
                                            {row.isPaused
                                              ? 'Pausing a check with dependent checks will also pause those checks'
                                              : 'Unpausing a check with dependent checks will also unpause those checks'}
                                          </MessagePanel>
                                        </Popover.Dropdown>
                                      </Popover>
                                      <IconButton
                                        icon="delete"
                                        kind="secondary"
                                        background="transparent"
                                        onClick={(e) => {
                                          e.stopPropagation()

                                          form.setData((prevData) => ({
                                            ...prevData,
                                            checks: prevData.checks
                                              .filter(
                                                (prevCheck) =>
                                                  prevCheck.localId !==
                                                  row.localId
                                              )
                                              .map((prevCheck) => ({
                                                ...prevCheck,
                                                dependencyName:
                                                  prevCheck.dependencyName !==
                                                  row.name
                                                    ? prevCheck.dependencyName
                                                    : null,
                                                dependencyValue:
                                                  prevCheck.dependencyName !==
                                                  row.name
                                                    ? prevCheck.dependencyValue
                                                    : null,
                                              })),
                                          }))
                                        }}
                                      />
                                    </>
                                  )
                                },
                                sortable: false,
                              },
                            ]
                          : []),
                      ]}
                      disableRowSelectionOnClick
                      getRowHeight={() => 'auto'}
                      getRowId={(row) => row.localId}
                      key={`data-grid-${dataGridKeyNum}`}
                      noRowsOverlayMessage={t('plainText.noChecks')}
                      onRowClick={({ row }) => {
                        if (!readOnly) setSelectedCheck(row)
                      }}
                      onRowOrderChange={({ oldIndex, row, targetIndex }) => {
                        const checks = [...form.data.checks]
                        checks.splice(oldIndex, 1)
                        checks.splice(targetIndex, 0, row)

                        if (!areDependenciesValid(checks)) {
                          snackbar.show(t('plainText.dependencyOrderError'))
                          // DataGrid does have a way to cancel a reordering event, so instead
                          // we are incrementing the key of the DataGrid so that it redraws
                          // with its rows in the order from before the reordering event began.
                          setDataGridKeyNum((prevKeyNum) => prevKeyNum + 1)
                          return
                        }

                        form.setData((prevData) => ({
                          ...prevData,
                          checks,
                        }))
                      }}
                      rowReordering
                      rows={form.data.checks.map((check) => ({
                        ...check,
                        __reorder__: check.name,
                      }))}
                    />
                    {!readOnly && (
                      <div>
                        <Button
                          data-cy="inspection-add-check"
                          onClick={handleAddCheckClick}
                          prefix={<Icon icon="add" />}
                          size="large"
                        >
                          {t('headers.addCheck')}
                        </Button>
                      </div>
                    )}
                  </Flex>
                </Fieldset>
              </Flex>
            </Flex>
            <ModalSubmitButton showSubmitButton={!readOnly}>
              {t('plainText.save')}
            </ModalSubmitButton>
          </Flex>
          {selectedCheck != null && (
            <AddCheckModal
              check={selectedCheck}
              checks={form.data.checks}
              onClose={() => setSelectedCheck()}
            />
          )}
          {pauseCheck != null && (
            <PauseModal check={pauseCheck} onClose={() => setPauseCheck()} />
          )}
        </>
      )}
    </Form>
  )
}
