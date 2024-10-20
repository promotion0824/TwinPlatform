import { Group, Icon, IconName, Tooltip } from '@willowinc/ui'
import { forwardRef, ReactNode } from 'react'
import styled from 'styled-components'
import { Tile, TileProps } from '../common'

export type DataTileField = {
  icon: IconName
  /** @default true */
  iconFilled?: boolean
  label: string
  tooltip?: string
  value: ReactNode
}

export interface DataTileProps extends TileProps {
  /** The set of fields to be displayed. */
  fields: DataTileField[]
  /** Optional title to be displayed on the tile. */
  title?: string
}

const Label = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

const Title = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
}))

const Value = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))

const Fieldset = ({
  icon,
  iconFilled = true,
  label,
  tooltip,
  value,
}: DataTileField) => (
  <Group wrap="nowrap">
    <Tooltip
      disabled={!tooltip}
      label={tooltip}
      multiline
      position="top"
      w={350}
      withinPortal
    >
      <Group gap="s4" mr="auto" wrap="nowrap">
        <Icon icon={icon} filled={iconFilled} />
        <Label>{label}</Label>
      </Group>
    </Tooltip>
    <Value>{value}</Value>
  </Group>
)

export const DataTile = forwardRef<HTMLDivElement, DataTileProps>(
  ({ fields, title, ...restProps }, ref) => (
    <Tile ref={ref} title={title} {...restProps}>
      <Title>{title}</Title>

      {fields.map((field) => (
        <Fieldset key={field.label} {...field} />
      ))}
    </Tile>
  )
)
