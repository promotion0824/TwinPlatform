import { Fragment } from 'react'
import _ from 'lodash'
import { useForm, Checkbox, Input, NumberInput } from '@willow/ui'
import columnOrder from './columnOrder'

export default function ConnectorColumns({ columns, className }) {
  const form = useForm()

  const orderedColumns = _.orderBy(
    columns,
    columnOrder.map((name) => (column) => column.name !== name)
  )

  const unitOrderedColumns = orderedColumns.map((label) => {
    switch (label.name) {
      case 'Timeout':
        return {
          ...label,
          unitLabel: 'Timeout (ms)',
        }

      case 'InitDelay':
        return {
          ...label,
          unitLabel: 'Initial Delay (ms)',
        }

      case 'ScanInterval':
        return {
          ...label,
          unitLabel: 'Scan Interval (s)',
        }

      case 'Interval':
        return {
          ...label,
          unitLabel: 'Interval (s)',
        }

      default:
        return {
          ...label,
        }
    }
  })

  return (
    <>
      {unitOrderedColumns.map((column) => (
        <Fragment key={column.name}>
          {column.type.toLowerCase() === 'boolean' && (
            <Checkbox
              name={column.name}
              label={_.startCase(column.name)}
              required={column.isRequired}
              className={className}
              error={
                column.isRequired && form.data[column.name] == null
                  ? `${column.name} is required`
                  : null
              }
            />
          )}
          {column.type.toLowerCase() === 'number' && (
            <NumberInput
              name={column.name}
              label={column.unitLabel || _.startCase(column.name)}
              required={column.isRequired}
              className={className}
              error={
                column.isRequired === true && form.data[column.name] == null
                  ? `${column.unitLabel || column.name} is required`
                  : null
              }
            />
          )}
          {column.type.toLowerCase() === 'string' && (
            <Input
              name={column.name}
              label={_.startCase(column.name)}
              required={column.isRequired}
              className={className}
              error={
                column.isRequired === true && form.data[column.name] == null
                  ? `${column.name} is required`
                  : null
              }
            />
          )}
        </Fragment>
      ))}
    </>
  )
}
