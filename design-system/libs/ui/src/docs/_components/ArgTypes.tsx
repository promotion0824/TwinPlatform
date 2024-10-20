import { ArgTypes as StorybookArgTypes } from '@storybook/blocks'
import { colorProps, spacingProps } from '../../lib/utils'

export default function ArgTypes({
  exclude = [],
  excludeStyleProps = false,
  ...restProps
}: {
  /**
   * Specifies which arg types to exclude from the args table. Any arg types whose names
   * are part of the array will be left out.
   * @default []
   */
  exclude?: string[]
  /**
   * Exclude Willow style props from the props page, and display information containing
   * a link to the Style Props documentation.
   * @default false
   */
  excludeStyleProps?: boolean
}) {
  return (
    <>
      <StorybookArgTypes
        exclude={
          excludeStyleProps
            ? exclude
            : [...exclude, ...colorProps, ...spacingProps]
        }
        {...restProps}
      />

      {!excludeStyleProps && (
        <p>
          This component also supports all of our style props to provide a
          simple way to add inline styles. Please see the{' '}
          <a href={'/?path=/docs/design-system-style-props--docs'}>
            Style Props
          </a>{' '}
          page for more information.
        </p>
      )}
    </>
  )
}
