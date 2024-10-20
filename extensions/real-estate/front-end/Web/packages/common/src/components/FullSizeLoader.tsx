import { Loader, LoaderProps, Stack } from '@willowinc/ui'

/**
 * an adaptation of the Loader component from @willowinc/ui
 * to be used across the app
 */
export default function FullSizeLoader({
  intent = 'secondary',
  size = 'md',
  ...rest
}: {
  /** @default 'secondary' */
  intent?: LoaderProps['intent']
  /** @default 'md' */
  size?: LoaderProps['size']
}) {
  return (
    <Stack h="100%" w="100%" align="center" justify="center" {...rest}>
      <Loader intent={intent} size={size} />
    </Stack>
  )
}
