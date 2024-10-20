// copied from Mantine v6
// https://github.com/mantinedev/mantine/blob/v6/src/mantine-utils/src/ForwardRefWithStaticComponents.ts
export type ForwardRefWithStaticComponents<
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  Props extends Record<string, any>,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  Static extends Record<string, any>
> = ((props: Props) => React.ReactElement) &
  Static & {
    displayName: string
  }
