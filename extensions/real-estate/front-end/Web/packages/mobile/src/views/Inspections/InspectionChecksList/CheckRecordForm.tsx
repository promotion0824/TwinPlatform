import {
  Controller,
  ControllerFieldState,
  ControllerRenderProps,
  useForm,
  SubmitHandler,
  SubmitErrorHandler,
  Path,
} from 'react-hook-form'
import { css, styled } from 'twin.macro'
import {
  Button,
  DatePicker,
  InputNew as Input,
  FormatedNumberInput,
  SelectNew as Select,
  OptionNew as Option,
  TextAreaNew as TextArea,
  stringUtils,
} from '@willow/mobile-ui'
import Images, { ImageFile } from '../../../components/Images/Images'
import { AttachmentEntry, Check, CheckRecord, CheckRecordInput } from './types'

export type FormValue = {
  checkRecord: CheckRecordInput
  attachmentEntries: AttachmentEntry[]
}

const NumberInputSuffix = styled.span({
  margin: 'auto',
  paddingRight: 'var(--padding)',
  verticalAlign: 'middle',
})

const StyledForm = styled.form<{ $isHistorical?: boolean }>(
  ({ $isHistorical = false }) => ({
    padding: '0 var(--padding-large) var(--padding-large)',
    display: 'flex',
    flexDirection: 'column',

    '& > :not(:first-child)': {
      marginTop: 'var(--padding-extra-large)',
    },

    '& label': {
      color: $isHistorical ? '#959595' : undefined,
    },

    '& label + *': {
      backgroundColor: $isHistorical ? '#1C1C1C !important' : undefined,
      color: $isHistorical ? '#D9D9D9' : undefined,
    },
  })
)

/**
 * Get the property name of the check record "Entry" based on the
 * check type.
 */
export const getFieldName = (checkType: Check['type']) => {
  switch (checkType) {
    case 'list':
      return 'stringValue'
    case 'date':
      return 'dateValue'
    case 'numeric':
    case 'total':
      return 'numberValue'
    default:
      throw new Error(`Unexpected check type: ${checkType}`)
  }
}

/**
 * Get the default value for the check record "Entry" based
 * on the check type.
 */
export const getDefaultEntry = (check: Check, checkRecord?: CheckRecord) => {
  const checkRecordValue = checkRecord?.[getFieldName(check.type)]
  if (checkRecordValue != null) {
    return checkRecordValue
  } else if (
    check.type === 'total' &&
    check?.lastSubmittedRecord?.numberValue != null
  ) {
    return check.lastSubmittedRecord.numberValue
  }
  return null
}

const Entry = ({
  controllerField,
  fieldState,
  readOnly,
  check,
}: {
  check: Check
  controllerField: ControllerRenderProps<FormValue>
  fieldState: ControllerFieldState
  readOnly: boolean
}) => {
  if (check.type === 'numeric' || check.type === 'total') {
    return (
      <>
        <FormatedNumberInput
          {...controllerField}
          onChange={(value: string) =>
            controllerField.onChange(value ? parseFloat(value) : null)
          }
          defaultValue={controllerField.value}
          label="Entry"
          fixedDecimalScale
          decimalScale={check.decimalPlaces}
          inputMode="decimal"
          step={
            check.decimalPlaces != null
              ? 1 / 10 ** check.decimalPlaces
              : undefined
          }
          readOnly={readOnly}
          content={<NumberInputSuffix>{check.typeValue}</NumberInputSuffix>}
          error={fieldState.error?.message}
        />
        {check.type === 'total' && (
          <div
            css={css(({ theme }) => ({
              display: 'flex',
              gap: theme.spacing.s16,
              flexWrap: 'wrap',
              '& > span': {
                width: '45%',
                flexGrow: 1,
              },
            }))}
          >
            {[
              {
                label: 'Minimum %',
                value: check.minValue,
              },
              {
                label: 'Maximum %',
                value: check.maxValue,
              },
              {
                label: 'Multiplier',
                value: check.multiplier,
              },
              {
                label: 'Calculated',
                value:
                  ((controllerField?.value ?? 0) as number) *
                  (check.multiplier || 1),
              },
            ]
              .filter(({ value }) => value != undefined && value >= 0)
              .map(({ label, value }) => (
                <Input key={label} label={label} readOnly value={value} />
              ))}
          </div>
        )}
      </>
    )
  } else if (check.type === 'list') {
    return (
      <Select
        name={controllerField.name}
        value={controllerField.value}
        onChange={controllerField.onChange}
        label="Entry"
        placeholder="Select value"
        error={fieldState.error?.message}
        readOnly={readOnly}
      >
        {(check.typeValue ?? '').split('|').map((item) => (
          <Option key={item} value={item}>
            {stringUtils.capitalizeFirstLetter(item)}
          </Option>
        ))}
      </Select>
    )
  } else {
    return (
      <DatePicker
        name={controllerField.name}
        value={controllerField.value}
        onChange={controllerField.onChange}
        error={fieldState.error?.message}
        label="Date"
        placeholder="Date"
        readOnly={readOnly}
      />
    )
  }
}

/**
 * Check record form with validation containing:
 * - The entry field based on the check type,
 * - Notes field, and
 * - Attachments field
 */
export default function CheckRecordForm({
  check,
  checkRecord,
  attachmentEntries,
  readOnly = false,
  isAttachmentEnabled,
  isHistorical,
  onSubmit = () => {},
  onSubmitError = () => {},
}: {
  check: Check
  checkRecord: CheckRecord | undefined
  attachmentEntries: AttachmentEntry[]
  readOnly: boolean
  isAttachmentEnabled: boolean
  /** Whether check record is a historical record */
  isHistorical?: boolean
  onSubmit?: SubmitHandler<FormValue>
  onSubmitError?: SubmitErrorHandler<FormValue>
}) {
  const entryFieldName = getFieldName(check.type)

  const { control, handleSubmit, formState, getValues } = useForm<FormValue>({
    mode: 'onBlur',
    defaultValues: {
      checkRecord: {
        [entryFieldName]: getDefaultEntry(check, checkRecord),
        notes: checkRecord?.notes ?? '',
      },
      attachmentEntries,
    },
  })

  return (
    <StyledForm
      role="form"
      onSubmit={(event) =>
        // This is unfortunately a bit convoluted. We use React Hook Form's
        // validation to display some warnings like "this value is outside
        // the expected range" but which should not prevent submitting the
        // form. We are not aware of a way to use React Hook Form's
        // validation to provide warning messages for fields without
        // marking the fields as invalid, which by default prevents saving.
        // So we let the warnings mark the form as invalid, but then we
        // catch the submit error event (the second callback below), and
        // submit anyway if none of the "errors" are real errors.
        handleSubmit(
          () => onSubmit(getValues(), event),
          (errors) => {
            if (
              !Object.values(errors.checkRecord ?? {}).some(
                (e) => typeof e === 'object' && e.type === 'required'
              )
            ) {
              return onSubmit(getValues(), event)
            } else if (onSubmitError != null) {
              return onSubmitError(errors)
            }
          }
        )(event)
      }
      $isHistorical={isHistorical}
    >
      <Controller
        control={control}
        name={`checkRecord.${getFieldName(check.type)}` as Path<FormValue>}
        rules={{
          required: 'Value is required',
          max:
            check.type === 'numeric' && check.maxValue !== undefined
              ? {
                  value: check.maxValue,
                  message: `This is above the ${check.maxValue} threshold value. Are you sure?`,
                }
              : undefined,
          min:
            check.type === 'numeric' && check.minValue !== undefined
              ? {
                  value: check.minValue,
                  message: `This is below the ${check.minValue} threshold value. Are you sure?`,
                }
              : undefined,
          pattern:
            check.type === 'total' || check.type === 'numeric'
              ? {
                  value: new RegExp(
                    check.decimalPlaces === 0
                      ? '(\\d+)$'
                      : `(\\d+)(\\.\\d{${check.decimalPlaces}})$`
                  ),
                  message: `Value should have ${check.decimalPlaces} decimal places`,
                }
              : undefined,
          validate: {
            totalValue: (v: string) =>
              check.type === 'total' &&
              check?.lastSubmittedRecord?.numberValue !== undefined
                ? parseFloat(v) >= check.lastSubmittedRecord.numberValue ||
                  `Total value cannot be lower than ${check.lastSubmittedRecord.numberValue}`
                : undefined,
          },
        }}
        render={({
          field,
          fieldState,
        }: {
          field: ControllerRenderProps<FormValue>
          fieldState: ControllerFieldState
        }) => (
          <Entry
            controllerField={field}
            fieldState={fieldState}
            check={check}
            readOnly={readOnly}
          />
        )}
      />
      <Controller
        name="checkRecord.notes"
        control={control}
        render={({ field }) => (
          <TextArea
            {...field}
            aria-labelledby="Notes"
            label="Notes"
            placeholder={!readOnly ? 'Add notes here' : ''}
            readOnly={readOnly}
          />
        )}
      />
      <Controller
        name="attachmentEntries"
        control={control}
        render={({ field, fieldState }) =>
          // Always render when check record is not historical (i.e. current).
          // If check record is historical, we render only when there is at least attachment.
          !isHistorical || field.value.length > 0 ? (
            <Images
              label={isHistorical ? 'Attachments' : undefined}
              error={fieldState.error?.message}
              images={field.value
                .filter((entry: AttachmentEntry) => entry.status !== 'deleted')
                .map((entry) => entry.attachment)}
              onAddImage={(image: ImageFile) =>
                field.onChange([
                  ...field.value,
                  { attachment: image, status: 'added' },
                ])
              }
              onDeleteImage={(imageId: string) => {
                const deletedImage = field.value.find(
                  (entry) => entry.attachment.id === imageId
                )
                // Hard delete newly added image and soft delete existing image from form field.
                if (deletedImage != null) {
                  field.onChange(
                    deletedImage.status === 'added'
                      ? field.value.filter(
                          (entry) => entry.attachment.id !== imageId
                        )
                      : field.value.map((entry) =>
                          entry.attachment.id === imageId
                            ? { ...entry, status: 'deleted' }
                            : entry
                        )
                  )
                }
              }}
              allowAdd={isAttachmentEnabled}
              addImageText="Add attachments or photos"
            />
          ) : (
            // for some reason the Typescript type of  `render` prop to
            // Controller doesn't like us returning null?
            <></>
          )
        }
      />
      {!readOnly && (
        <Button
          type="submit"
          color="blue"
          size="large"
          loading={formState.isSubmitting}
          aria-labelledby="Submit"
        >
          Submit
        </Button>
      )}
    </StyledForm>
  )
}
