import { CSSProperties, ReactNode } from 'react'
import {
  InputWrapperProps as MantineInputWrapperProps,
  MantineStyleProp,
} from '@mantine/core'
import { rem } from './rem'

export interface CommonInputProps<ValueType> {
  /** Initial value for uncontrolled components. Overridden by value prop. */
  defaultValue?: ValueType
  /** Value of input for controlled components. */
  value?: ValueType
  /** Called when value changes. */
  onChange?: (value: ValueType) => void
  /**
   * Determines the layout orientation of component and its associated label and description/error message.
   * @default 'vertical'
   */
  layout?: 'vertical' | 'horizontal'
  /**
   * The width of the label.
   * @default 'auto'
   */
  labelWidth?: CSSProperties['width']
  /** Input or field label. Displayed before input. */
  label?: ReactNode
  /** Props spread to label element. */
  labelProps?: MantineInputWrapperProps['labelProps']
  /**
   * Input or field description, displayed after input.
   * Will be hidden when error message is also provided.
   */
  description?: ReactNode
  descriptionProps?: MantineInputWrapperProps['descriptionProps']
  /**
   * Displays error message after input or field.
   * Use `true` to add invalid style if no error message is needed.
   */
  error?: ReactNode
  errorProps?: MantineInputWrapperProps['errorProps']

  /**
   * Adds required attribute to the input or field and red asterisk on the right side
   * of label
   * @default false
   */
  required?: boolean
  /**
   * Disables input or field.
   * @default false
   */
  disabled?: boolean
  /**
   * Makes input or field readonly.
   * @default false
   */
  readOnly?: boolean
}

/**
 * Common props include error, description and inputWrapperOrder.
 * Where description will be hidden when there is an error message.
 * And it also includes merged style and className props if layout is horizontal.
 */
export const getCommonInputProps = <ValueType,>({
  className,
  style,
  layout,
  labelWidth,
  description,
  error,
}: CommonInputProps<ValueType> & {
  className?: string
  style?: MantineStyleProp
}) => ({
  ...getDescriptionAndErrorProps(description, error),
  ...getCommonInputStyle({ className, style, layout, labelWidth }),
  inputWrapperOrder: inputWrapperOrder,
})

/**
 * Display description when description provided.
 * Display error message when error message is provided.
 *
 * **Note**:
 * Description will only be hidden when there is an error message or component.
 * And it won't hide description when `error = true`
 */
function getDescriptionAndErrorProps(description: ReactNode, error: ReactNode) {
  return {
    description: error && error !== true ? undefined : description,
    error,
  }
}

/**
 * Merges `style` and `className` props for a horizontal layout configuration.
 * Utilizes `--willow-form-input-label-width` for styling, as defined in `/libs/ui/src/lib/theme/customizedMantineStyles/commonInputStyles.tsx`.
 * Refer to the mentioned file for implementation details of the horizontal layout.
 */
const getCommonInputStyle = ({
  className,
  style,
  layout,
  labelWidth = '1fr',
}: CommonInputProps<unknown> & {
  className?: string
  style?: MantineStyleProp
}) => {
  if (layout === 'horizontal')
    return {
      style: {
        ...style,
        '--willow-form-input-label-width':
          typeof labelWidth === 'number' ? rem(labelWidth) : labelWidth,
      },
      className: `${className ?? ''} horizontal`.trim(),
    }
  return {}
}

const inputWrapperOrder: ('label' | 'input' | 'description' | 'error')[] = [
  'label',
  'input',
  'description',
  'error',
]
