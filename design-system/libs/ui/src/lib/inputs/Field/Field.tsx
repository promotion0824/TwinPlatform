import { Box, BoxProps } from '@mantine/core'
import { forwardRef } from 'react'
import { Error } from '../Error'
import { Label } from '../Label'
import { CommonInputProps, getCommonInputProps } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface FieldProps
  extends WillowStyleProps,
    Pick<
      CommonInputProps<unknown>,
      | 'layout'
      | 'label'
      | 'labelWidth'
      | 'labelProps'
      | 'description'
      | 'descriptionProps'
      | 'error'
      | 'errorProps'
      | 'required'
      | 'readOnly'
    >,
    Omit<BoxProps, keyof WillowStyleProps | 'label'> {
  children: React.ReactNode
}

/**
 * `Field` is a form input wrapper that provides an optional label and error message.
 */
export const Field = forwardRef<HTMLDivElement, FieldProps>(
  (
    {
      label,
      labelWidth,
      labelProps = {},
      description: initialDescription,
      descriptionProps = {},
      error: initialError,
      errorProps = {},
      required = false,
      children,
      className: initialClassName = '',
      ...restProps
    },
    ref
  ) => {
    const { description, error, style, className } = getCommonInputProps({
      ...restProps,
      description: initialDescription,
      error: initialError,
      labelWidth,
      className: `${initialClassName ?? ''} mantine-InputWrapper-root`,
    })

    return (
      <Box
        className={className}
        style={style}
        w="100%"
        ref={ref}
        {...restProps}
        {...useWillowStyleProps(restProps)}
      >
        {label && (
          <Label required={required} {...labelProps}>
            {label}
          </Label>
        )}
        <Box className="mantine-Input-wrapper">{children}</Box>
        {description && (
          <p
            css={{
              margin: 0,
            }}
            className="mantine-InputWrapper-description"
            {...descriptionProps}
          >
            {description}
          </p>
        )}
        {error && <Error {...errorProps}>{error}</Error>}
      </Box>
    )
  }
)
