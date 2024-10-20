import { baseTokens, darkTheme, darkThemeTokens } from '@willowinc/theme'
import { Pre } from '@storybook/components'
import { get } from 'lodash'
import InlineCode from './InlineCode'
import { mapPrimativeValuesDeep, NestedRecord, TokenValue } from '../util'

const getThead = (columns: string[]) => (
  <tr>
    {columns.map((text, index) => (
      <th key={index}>{text}</th>
    ))}
  </tr>
)

const headings = {
  shadow: ['Example', 'Token', 'Value', 'Usage'],
  spacing: ['Example', 'Token', 'Value'],
  color: ['Example', 'Token', 'Value', 'Usage'],
  zIndex: ['Token', 'Value'],
  font: ['Example', 'Token', 'Value'],
}

type TokenType = keyof typeof headings

/**
 * Style used for example display
 */
const getStyle = (type: TokenType, value: string | NestedRecord<string>) => {
  switch (type) {
    case 'color':
      return {
        backgroundColor: value,
        width: '100%',
        height: '40px',
        borderRadius: '2px',
      }
    case 'spacing':
      return {
        backgroundColor: 'grey',
        width: value,
        height: '20px',
      }
    case 'shadow':
      return {
        backgroundColor: 'black',
        width: '100%',
        height: '40px',
        borderRadius: '2px',
        boxShadow: value,
      }
    case 'font':
      return value as NestedRecord<string>
    case 'zIndex':
      return {
        zIndex: value,
      }
  }
}

/**
 * Mapping of column heading to the display of column cell
 * for the provided token type.
 */
const getColumn = (
  type: TokenType,
  column: string,
  config: TokenValue,
  accumulatedKeys: string[]
) => {
  const value = get(darkTheme[type], accumulatedKeys)
  switch (column) {
    case 'Example':
      return type === 'font' ? (
        <p style={getStyle(type, value)}>Test</p>
      ) : (
        <div style={getStyle(type, value)} />
      )
    case 'Token':
      return <InlineCode text={`${type}.${accumulatedKeys.join('.')}`} />
    case 'Usage':
      return config.description
    case 'Value':
      return typeof value === 'object' ? (
        <Pre>
          {Object.entries(config.value)
            .map(([key, val]) => `${key}: ${val};`)
            .join('\n')}
        </Pre>
      ) : (
        JSON.stringify(config.value)
      )
  }
  return null
}

const getRow = (
  type: TokenType,
  config: TokenValue,
  accumulatedKeys: string[]
) => (
  <tr key={accumulatedKeys.join('.')}>
    {headings[type].map((column) => (
      <td key={column}>{getColumn(type, column, config, accumulatedKeys)}</td>
    ))}
  </tr>
)

const getRows = (type: TokenType) => {
  return {
    headings: getThead(headings[type]),
    rows: mapPrimativeValuesDeep(
      type === 'color' || type === 'shadow'
        ? darkThemeTokens[type]
        : baseTokens[type],
      (config, _key, accumulatedKeys) => getRow(type, config, accumulatedKeys)
    ),
  }
}

export const color = getRows('color')
export const spacing = getRows('spacing')
export const shadow = getRows('shadow')
export const zIndex = getRows('zIndex')
export const font = getRows('font')
