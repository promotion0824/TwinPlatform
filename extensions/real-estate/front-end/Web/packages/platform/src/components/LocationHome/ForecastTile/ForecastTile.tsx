import { TemperatureUnit, titleCase } from '@willow/common'
import { SiteWeather } from '@willow/common/site/site/types'
import { Group } from '@willowinc/ui'
import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import { Tile, TileProps } from '../common'
import ForecastFileItem from './ForecastTileItem'

export type Forecast = [
  SiteWeather,
  SiteWeather,
  SiteWeather,
  SiteWeather,
  SiteWeather
]

export interface ForecastTileProps extends TileProps {
  /** Weather information for the next five days. */
  forecast: Forecast
  /** Unit to display the temperature in. */
  temperatureUnit: keyof typeof TemperatureUnit
}

const Label = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
}))

export const ForecastTile = forwardRef<HTMLDivElement, ForecastTileProps>(
  ({ forecast, temperatureUnit, ...restProps }, ref) => {
    const {
      i18n: { language },
      t,
    } = useTranslation()

    const forecastDates = forecast.map((_, index) => {
      const date = new Date()
      date.setDate(date.getDate() + index)
      return date
    })

    const label = titleCase({ language, text: t('headers.forecast') })

    return (
      <Tile ref={ref} title={label} {...restProps}>
        <Label>{label}</Label>

        <Group gap="s12" wrap="nowrap">
          {forecast.map((weather, i) => (
            <ForecastFileItem
              date={forecastDates[i]}
              highlighted={i === 0}
              key={forecastDates[i].toString()}
              temperatureUnit={temperatureUnit}
              weather={weather}
            />
          ))}
        </Group>
      </Tile>
    )
  }
)
