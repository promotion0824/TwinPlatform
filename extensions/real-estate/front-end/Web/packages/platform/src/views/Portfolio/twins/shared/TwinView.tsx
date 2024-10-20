/* eslint-disable react/destructuring-assignment, react/jsx-boolean-value, react/require-default-props, react/no-unused-prop-types */
import {
  getEnumValueLabel,
  getSchemaType,
  Schema,
} from '@willow/common/twins/view/models'
import { Json } from '@willow/common/twins/view/twinModel'
import {
  Button,
  DatePicker,
  DurationInput,
  Input,
  Option,
  Select,
  TextArea,
} from '@willow/ui'
import {
  DurationState,
  parseIsoDuration,
} from '@willow/ui/components/DurationInput/DurationInput'
import { InputGroup } from '@willowinc/ui'
import { DateTime } from 'luxon'
import { forwardRef } from 'react'
import { Controller } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import tw, { styled } from 'twin.macro'

import TimePicker from '../../../../components/TimePicker/TimePicker'
import { fontStyle, subheadingStyle } from '../view/styles'
import { useTwinEditor } from '../view/TwinEditorContext'
import { validate } from './validation'

export const PropertiesInner = styled.div({
  display: 'grid',
  gridTemplateColumns: '50% 50%',
})

export const Property = tw.div`my-1 pr-8`

export const PropertyName = styled.div({
  textTransform: 'uppercase',
  color: 'var(--lighter)',
  font: '11px/30px Poppins',
})

type FieldState = 'normal' | 'error' | 'conflict'

/* eslint-disable @typescript-eslint/no-explicit-any */
// Would be nice to make this into `type FieldProps<T>` to eliminate the `any`,
// but not sure how to make { [key: string]: FieldProps<different Ts can go here> }.
type FieldProps = {
  controllerName: string
  groupName?: string
  /**
   * Widgets should attach this ID to their "main" DOM element - this is the element
   * that the field label will focus when it's clicked.
   */
  id: string
  idPrefix?: string
  /**
   * Widgets should attach this to the `aria-labelledby` prop on their main DOM element.
   */
  ariaLabelledBy: string
  isAnnotatedBy?: any
  value: any
  onChange: (val: any) => void
  onBlur: () => void
  placeholder?: string
  readOnly?: boolean
  state: FieldState
  schema: Schema
}
/* eslint-enable @typescript-eslint/no-explicit-any */

/**
 * Render an appropriate widget for the input, based on its type (specified in
 * the `schema` prop).
 */
const PropertyInput = ({
  controllerName,
  id,
  idPrefix,
  ariaLabelledBy,
  groupName,
  isAnnotatedBy,
  readOnly = false,
  state,
  schema,
}: {
  controllerName: string
  id: string
  idPrefix: string
  ariaLabelledBy: string
  groupName: string
  isAnnotatedBy?: any
  readOnly?: boolean
  state: FieldState
  schema: Schema
}) => {
  const { t } = useTranslation()
  const { control } = useTwinEditor()

  // Note: we use onBlur validation (see TwinEditorContext), so if validation
  // should be done on a widget, make sure it uses its `onBlur` prop.

  // Use InputGroupField if the component has a matching annotation property.
  // If not, check for a matching schema type.
  // The 'customProperties' group has extra fields to handle showing JSON objects.
  const Component = isAnnotatedBy
    ? InputGroupField
    : widgetSet[getSchemaType(schema)] ??
      (groupName === 'customProperties' ? CustomPropertiesField : FallbackField)

  const rules = {
    validate: (ob: Json) => {
      const result = validate(ob, schema)
      if (typeof result === 'string') {
        return t(result)
      } else {
        return result
      }
    },
  }

  return (
    <Controller
      name={controllerName}
      control={control}
      rules={rules}
      render={({ field }) => (
        <Component
          {...field}
          controllerName={controllerName}
          groupName={groupName}
          id={id}
          idPrefix={idPrefix}
          ariaLabelledBy={ariaLabelledBy}
          isAnnotatedBy={isAnnotatedBy}
          readOnly={readOnly}
          state={state}
          schema={schema}
        />
      )}
    />
  )
}

export { PropertyInput }

/**
 * Note all our `*Field` components in this file follow the same interface, because
 * they are used in the same way by `PropertyInput`. So we will expect some props
 * to remain unused.
 */
const TextField = forwardRef<HTMLInputElement, FieldProps>(
  (
    {
      id,
      ariaLabelledBy,
      value,
      onChange,
      onBlur,
      placeholder,
      readOnly,
      state,
      schema,
    },
    ref
  ) => {
    const schemaType = getSchemaType(schema)
    let inputMode: string | undefined
    if (schemaType === 'integer' || schemaType === 'long') {
      inputMode = 'numeric'
    } else if (schemaType === 'float' || schemaType === 'double') {
      inputMode = 'decimal'
    }

    return (
      <StyledInput
        ref={ref}
        id={id}
        aria-labelledby={ariaLabelledBy}
        value={value ?? ''}
        onChange={onChange}
        onBlur={onBlur}
        preservePlaceholder
        placeholder={placeholder}
        readOnly={readOnly}
        inputMode={inputMode}
        maxLength={255}
        $state={state}
      />
    )
  }
)

const DatePickerField = forwardRef(
  (
    {
      id,
      ariaLabelledBy,
      value,
      onChange,
      onBlur,
      readOnly,
      state,
      schema,
    }: FieldProps,
    ref
  ) => (
    <StyledDatePicker
      id={id}
      aria-labelledby={ariaLabelledBy}
      value={value}
      type={schema === 'date' ? 'date' : 'date-time'}
      onChange={(val: string) => {
        if (schema === 'date' && val != null) {
          onChange(DateTime.fromISO(val).toFormat('yyyy-MM-dd'))
        } else {
          onChange(val)
        }
      }}
      onBlur={onBlur}
      readOnly={readOnly}
      $state={state}
      tw="w-full"
    />
  )
)

const TimePickerField = forwardRef(
  (
    { id, ariaLabelledBy, value, onChange, readOnly, state }: FieldProps,
    ref
  ) => (
    <StyledTimePicker
      id={id}
      ariaLabelledBy={ariaLabelledBy}
      value={value}
      onChange={onChange}
      readOnly={readOnly}
      $state={state}
      tw="w-full"
    />
  )
)

/**
 * Duration input that works directly with ISO 8601 duration strings instead of
 * our internal Duration type, since we don't yet transform the data on load.
 */
const DurationField = forwardRef(
  (
    {
      id,
      ariaLabelledBy,
      value,
      onChange,
      onBlur,
      readOnly,
      state,
    }: FieldProps,
    ref
  ) => (
    <StyledDurationInput
      id={id}
      ariaLabelledBy={ariaLabelledBy}
      value={typeof value === 'string' ? parseIsoDuration(value) : value}
      onChange={(val: DurationState) => {
        onChange(val)

        // Explicity call onBlur to trigger validation.
        onBlur()
      }}
      readOnly={readOnly}
      $state={state}
      tw="w-full"
    />
  )
)

/**
 * A boolean field has three options: true, false and not set (null).
 */
const BooleanField = forwardRef(
  (
    { id, ariaLabelledBy, value, onChange, readOnly, state }: FieldProps,
    ref
  ) => {
    const { t } = useTranslation()
    return (
      <StyledSelect
        id={id}
        aria-labelledby={ariaLabelledBy}
        value={value}
        onChange={onChange}
        readOnly={readOnly}
        $state={state}
        tw="w-full"
      >
        <Option value={null}>{t('plainText.notSet')}</Option>
        <Option value={true}>{t('plainText.true')}</Option>
        <Option value={false}>{t('plainText.false')}</Option>
      </StyledSelect>
    )
  }
)

/**
 * An enum field has one option per value in the enum, plus an option for null.
 */
const EnumField = forwardRef(
  (
    {
      id,
      ariaLabelledBy,
      value,
      onChange,
      readOnly,
      state,
      schema,
    }: FieldProps,
    ref
  ) => {
    const { t } = useTranslation()

    if (typeof schema !== 'object' || !('enumValues' in schema)) {
      throw new Error('EnumField used with a schema that is not an enum')
    }

    return (
      <StyledSelect
        id={id}
        aria-labelledby={ariaLabelledBy}
        value={value}
        onChange={onChange}
        readOnly={readOnly}
        header={(val: string | number | null) => {
          const enumValue = schema.enumValues.find((v) => v.name === val)
          if (enumValue != null) {
            return getEnumValueLabel(enumValue)
          } else {
            return val ?? t('plainText.notSet')
          }
        }}
        $state={state}
        tw="w-full"
      >
        <Option value={null}>{t('plainText.notSet')}</Option>
        {schema.enumValues.map((val) => (
          <Option key={val.enumValue} value={val.enumValue}>
            {getEnumValueLabel(val)}
          </Option>
        ))}
      </StyledSelect>
    )
  }
)

/**
 * If we don't know how to handle a field, show it as a read-only text input
 * with the JSON-stringified content of the field in it.
 */
const FallbackField = forwardRef<HTMLInputElement, FieldProps>(
  ({ id, ariaLabelledBy, value }, ref) => (
    <StyledInput
      ref={ref}
      id={id}
      aria-labelledby={ariaLabelledBy}
      value={typeof value === 'string' ? value : JSON.stringify(value ?? null)}
      readOnly
    />
  )
)

/**
 * A grouping of a text input with an annotation input (either another text input or an enum,
 * depending on the annotation's schema).
 */
const InputGroupField = forwardRef<HTMLDivElement, FieldProps>(
  (
    {
      ariaLabelledBy,
      controllerName,
      groupName,
      id,
      idPrefix,
      isAnnotatedBy,
      onBlur,
      onChange,
      readOnly,
      schema,
      state,
      value,
    },
    ref
  ) => {
    const { control } = useTwinEditor()
    const { t } = useTranslation()

    const annotatedByControllerName = groupName
      ? `${groupName}.${isAnnotatedBy.name}`
      : isAnnotatedBy.name

    return (
      <InputGroup ref={ref}>
        <TextField
          ariaLabelledBy={ariaLabelledBy}
          controllerName={controllerName}
          id={id}
          onBlur={onBlur}
          onChange={onChange}
          placeholder={t('labels.value')}
          readOnly={readOnly}
          schema={schema}
          state={state}
          value={value}
        />

        <Controller
          control={control}
          name={annotatedByControllerName}
          render={({ field }) =>
            isAnnotatedBy.schema === 'string' ? (
              <TextField
                ariaLabelledBy={ariaLabelledBy}
                controllerName={controllerName}
                id={`${idPrefix}-${isAnnotatedBy.name}-input`}
                placeholder={t('placeholder.unit')}
                readOnly={readOnly}
                schema={isAnnotatedBy.schema}
                state={state}
                {...field}
              />
            ) : (
              <EnumField
                ariaLabelledBy={ariaLabelledBy}
                controllerName={controllerName}
                id={`${idPrefix}-${isAnnotatedBy.name}-input`}
                readOnly={readOnly}
                schema={isAnnotatedBy.schema}
                state={state}
                {...field}
              />
            )
          }
        />
      </InputGroup>
    )
  }
)

const CustomPropertiesField = forwardRef(
  ({ id, ariaLabelledBy, value }: FieldProps, ref) => {
    if (typeof value === 'object') {
      const formatted = Object.entries(value)
        .map(([key, mapValue]) => `${key}: ${mapValue}`)
        .join('\n')

      return (
        <StyledTextArea
          ref={ref}
          id={id}
          aria-labelledby={ariaLabelledBy}
          value={formatted}
          readOnly
        />
      )
    } else {
      return (
        <StyledInput
          ref={ref}
          id={id}
          aria-labelledby={ariaLabelledBy}
          value={value}
          readOnly
        />
      )
    }
  }
)

const widgetSet = {
  string: TextField,
  float: TextField,
  double: TextField,
  long: TextField,
  integer: TextField,
  date: DatePickerField,
  dateTime: DatePickerField,
  time: TimePickerField,
  duration: DurationField,
  boolean: BooleanField,
  Enum: EnumField,
}

export function stateColor(state: FieldState) {
  if (state === 'error') {
    return 'var(--red)'
  } else if (state === 'conflict') {
    return 'var(--primary5)'
  } else {
    return undefined
  }
}

function stateBorder(state: FieldState) {
  const color = stateColor(state)
  if (color != null) {
    return `solid 1px ${color} !important`
  } else {
    return undefined
  }
}

const readOnlyStyles = {
  backgroundColor: 'var(--disabled-background)',
  borderColor: 'var(--disabled-background)',
}

const StyledDurationInput = styled(DurationInput)<{
  $state: FieldState
  readOnly?: boolean
}>(({ $state, readOnly }) => ({
  border: stateBorder($state),
  ...(readOnly ? readOnlyStyles : {}),
  ...fontStyle,
}))

const StyledTimePicker = styled(TimePicker)<{
  $state: FieldState
  readOnly?: boolean
}>(({ $state, readOnly }) => ({
  border: stateBorder($state),
  ...(readOnly ? readOnlyStyles : {}),
  // `"&&": fontStyle` rather than `...fontStyle` to make the selectors longer
  // so we override the default disabled styling in the dropdown.
  '&&': fontStyle,
}))

const StyledSelect = styled(Select)<{ $state: FieldState }>(
  ({ $state, theme }) => ({
    border: stateBorder($state),
    backgroundColor: theme.color.neutral.bg.panel.default,
    ...fontStyle,
  })
)

const StyledDatePicker = styled(DatePicker)<{ $state: FieldState }>(
  ({ $state, theme }) => ({
    border: stateBorder($state),
    backgroundColor: theme.color.neutral.bg.panel.default,
    ...fontStyle,
  })
)

export const StyledInput = styled(Input)<{ $state?: FieldState }>(
  ({ $state, theme }) => ({
    display: 'block',
    width: '100%',
    ...($state && { border: stateBorder($state) }),
    backgroundColor: theme.color.neutral.bg.panel.default,
    ...fontStyle,
  })
)

const StyledTextArea = styled(TextArea)<{ $state?: FieldState }>(
  ({ $state, theme }) => ({
    display: 'block',
    width: '100%',
    ...($state && { border: stateBorder($state) }),
    backgroundColor: theme.color.neutral.bg.panel.default,
    ...fontStyle,
  })
)

export const PropertyValue = styled.div<{
  hasBeenEdited?: boolean
  isDisabled?: boolean
}>(({ hasBeenEdited, isDisabled, theme }) => [
  {
    ...fontStyle,
    color: isDisabled ? theme.color.neutral.fg.subtle : 'var(--light)',
    wordWrap: 'break-word',
  },
  ...(hasBeenEdited
    ? [
        {
          display: 'flex',
          background: 'rgba(255, 98, 0, 0.168627)',
          '&:before': { content: '" "', whiteSpace: 'pre' },
          '&:after': { content: '" "', whiteSpace: 'pre' },
          width: 'fit-content',
        },
      ]
    : []),
  tw`mt-0.5 ml-2`,
])

export const PropertyText = styled.div<{ isNullChange: boolean }>(
  ({ isNullChange }) => [
    { textDecoration: isNullChange ? 'line-through' : 'unset' },
  ]
)

export const GroupHeading = styled.span({
  fontSize: 'var(--font-small)',
  color: 'var(--light)',
  fontWeight: 600,
})

export const Subheading = styled.div({
  ...subheadingStyle,
  display: 'flex',
  alignItems: 'center',
  fontWeight: 600,
})

export const MoreLessButton = styled(Button)({
  fontWeight: 600,
})
