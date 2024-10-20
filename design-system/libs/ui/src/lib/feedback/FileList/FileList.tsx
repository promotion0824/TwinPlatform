import styled, { css } from 'styled-components'
import { factory, Factory } from '@mantine/core'

import { Stack, StackProps } from '../../layout/Stack'
import { WillowStyleProps } from '../../utils'
import { File } from './File'

export interface FileListProps extends WillowStyleProps, StackProps {
  /**
   * Determine whether the component should have a border or not.
   * @default false
   */
  withBorder?: boolean
}

type FileListFactory = Factory<{
  props: FileListProps
  ref: HTMLDivElement
  staticComponents: {
    File: typeof File
  }
}>

/**
 * `FileList` is a component that displays information for a list of uploaded files.
 */
const FileList = factory<FileListFactory>(
  ({ withBorder = false, ...restProps }, ref) => {
    return <Container $withBorder={withBorder} {...restProps} ref={ref} />
  }
)

const Container = styled(Stack)<{ $withBorder?: boolean }>(
  ({ theme, $withBorder }) => css`
    overflow-y: auto;
    ${$withBorder
      ? {
          padding: theme.spacing.s8,
          border: `1px solid ${theme.color.neutral.border.default}`,
          borderRadius: theme.radius.r4,
        }
      : undefined};
    ${theme.font.body.md.regular};
  `
)

FileList.File = File

export { FileList }
