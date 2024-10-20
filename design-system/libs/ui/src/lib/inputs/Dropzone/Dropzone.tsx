import {
  Dropzone as MantineDropzone,
  DropzoneProps as MantineDropzoneProps,
  MIME_TYPES,
} from '@mantine/dropzone'
import { forwardRef } from 'react'
import styled from 'styled-components'
import { Loader } from '../../feedback/Loader'
import { Stack } from '../../layout/Stack'
import { Icon } from '../../misc/Icon'
import {
  CommonInputProps,
  extractStyleProps,
  WillowStyleProps,
} from '../../utils'
import { Field } from '../Field'
import dropzoneDescription from './dropzoneDescription'

export interface DropzoneProps
  extends WillowStyleProps,
    Omit<MantineDropzoneProps, keyof WillowStyleProps | 'accept'>,
    Pick<
      CommonInputProps<File | File[]>,
      | 'description'
      | 'descriptionProps'
      | 'disabled'
      | 'label'
      | 'labelProps'
      | 'labelWidth'
      | 'layout'
      | 'required'
    > {
  /** Types of files that the dropzone can accept. By default, all file types are accepted. */
  acceptedFiles?: (keyof typeof MIME_TYPES)[]
  /**
   * Shows the dropzone in an invalid state.
   * @default false
   */
  invalid?: boolean
  /**
   * Determines whether a loading overlay should be displayed over the dropzone.
   * @default false
   */
  loading?: MantineDropzoneProps['loading']
  /** Maximum number of files that can be picked at once. */
  maxFiles?: MantineDropzoneProps['maxFiles']
  /** Maximum file size in bytes. */
  maxSize?: MantineDropzoneProps['maxSize']
  /**	Called when valid files are dropped to the dropzone. */
  onDrop: MantineDropzoneProps['onDrop']
  /** Called when any files are dropped to the dropzone. */
  onDropAny?: MantineDropzoneProps['onDropAny']
  /** Called when user closes the file selection dialog with no selection. */
  onFileDialogCancel?: MantineDropzoneProps['onFileDialogCancel']
  /** Called when the user opens the file selection dialog. */
  onFileDialogOpen?: MantineDropzoneProps['onFileDialogOpen']
  /** Called when dropped files do not meet file restrictions. */
  onReject?: MantineDropzoneProps['onReject']
}

const BodyText = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
}))

const StyledDropzone = styled(MantineDropzone)<{
  $invalid: DropzoneProps['invalid']
}>(({ disabled, $invalid, loading, theme }) => ({
  backgroundColor: theme.color.intent.primary.bg.subtle.default,
  border: `1px dashed ${theme.color.intent.primary.border.default}`,
  borderRadius: theme.radius.r2,
  color: theme.color.neutral.fg.default,
  cursor: 'pointer',

  "&[data-accept='true']": {
    background: theme.color.intent.primary.bg.subtle.hovered,
    border: `1px solid ${theme.color.intent.primary.border.hovered}`,
  },

  "&[data-reject='true']": {
    background: theme.color.intent.negative.bg.subtle.default,
    border: `1px solid ${theme.color.intent.negative.border.default}`,
  },

  '&:focus-visible': {
    // This is removed first so that the default style doesn't display
    // when dragging a file onto the dropzone.
    outline: 'none',

    "&:not(&[data-accept='true']):not(&[data-reject='true'])": {
      outline: `1px solid ${theme.color.state.focus.border}`,
      outlineOffset: '-1px',
    },
  },

  "&:hover:not(&[data-accept='true']):not(&[data-reject='true'])": {
    backgroundColor: theme.color.intent.primary.bg.subtle.hovered,
    borderColor: theme.color.intent.primary.border.default,
  },

  '.mantine-LoadingOverlay-overlay': {
    background: `${theme.color.neutral.bg.base.default}${theme.transparent.t60}`,
  },

  ...(disabled && {
    backgroundColor: theme.color.intent.secondary.bg.subtle.default,
    borderColor: theme.color.state.disabled.border,
    color: theme.color.state.disabled.fg,
    cursor: 'not-allowed',

    span: {
      color: `${theme.color.state.disabled.fg} !important`,
    },

    "&:hover:not(&[data-accept='true']):not(&[data-reject='true'])": {
      backgroundColor: theme.color.intent.secondary.bg.subtle.default,
    },
  }),

  ...($invalid && {
    backgroundColor: theme.color.intent.negative.bg.subtle.default,
    borderColor: theme.color.intent.negative.border.default,
  }),

  ...(loading && {
    cursor: 'not-allowed',

    "&:hover:not(&[data-accept='true']):not(&[data-reject='true'])": {
      backgroundColor: theme.color.intent.primary.bg.subtle.default,
    },
  }),
}))

/** `Dropzone` captures files using drag and drop. */
export const Dropzone = forwardRef<HTMLDivElement, DropzoneProps>(
  (
    {
      acceptedFiles,
      description,
      descriptionProps,
      disabled,
      invalid,
      label,
      labelProps,
      labelWidth,
      layout,
      maxFiles,
      maxSize,
      required,
      ...props
    },
    ref
  ) => {
    const { styleProps, restProps } = extractStyleProps(props)

    const internalDescription =
      description ??
      dropzoneDescription({
        acceptedFiles,
        maxFiles,
        maxSize,
      })

    return (
      <Field
        description={internalDescription}
        descriptionProps={descriptionProps}
        label={label}
        labelProps={labelProps}
        labelWidth={labelWidth}
        layout={layout}
        required={required}
        {...styleProps}
      >
        <StyledDropzone
          accept={acceptedFiles?.map((type) => MIME_TYPES[type])}
          disabled={disabled}
          $invalid={invalid}
          loaderProps={{ children: <Loader /> }}
          maxFiles={maxFiles}
          maxSize={maxSize}
          mt={label ? 8 : 0}
          mb={internalDescription ? 8 : 0}
          pos="relative"
          ref={ref}
          {...restProps}
        >
          <Stack align="center" gap="s8" pb="s12" pl="s16" pr="s16" pt="s12">
            <Icon c="neutral.fg.muted" icon="upload" />
            <BodyText>Drag files here or click to select</BodyText>
          </Stack>
        </StyledDropzone>
      </Field>
    )
  }
)
