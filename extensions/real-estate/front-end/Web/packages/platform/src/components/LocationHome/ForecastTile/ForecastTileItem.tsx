import { convertToFahrenheit, TemperatureUnit } from '@willow/common'
import { SiteWeather } from '@willow/common/site/site/types'
import { Stack } from '@willowinc/ui'
import { forwardRef } from 'react'
import styled from 'styled-components'
import WeatherbitIcon from '../../WeatherbitIcon/WeatherbitIcon'

interface ForecastTileItemProps {
  /** Used to calculate the day to display. */
  date: Date
  /**
   * Changes the item's background color.
   * @default false
   */
  highlighted?: boolean
  /** Temperature unit to display the temperature. */
  temperatureUnit: keyof typeof TemperatureUnit
  /** Weather information. */
  weather: SiteWeather
}

const Container = styled(Stack)<{
  $highlighted: ForecastTileItemProps['highlighted']
}>(({ $highlighted, theme }) => ({
  backgroundColor: $highlighted
    ? theme.color.neutral.bg.panel.default
    : 'transparent',
  borderRadius: theme.radius.r4,
}))

const Label = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
}))

export default forwardRef<HTMLDivElement, ForecastTileItemProps>(
  (
    { date, highlighted = false, temperatureUnit, weather, ...restProps },
    ref
  ) => {
    const { code, icon, temperature } = weather

    const convertedTemperature = Math.round(
      temperatureUnit === TemperatureUnit.celsius
        ? temperature
        : convertToFahrenheit(weather.temperature)
    )

    const temperatureSymbol =
      temperatureUnit === TemperatureUnit.celsius ? 'C' : 'F'

    return (
      <Container
        align="center"
        gap="s4"
        $highlighted={highlighted}
        pt="s2"
        pb="s2"
        pl="s8"
        pr="s8"
        ref={ref}
        w="100%"
        {...restProps}
      >
        <Label>
          {date.toLocaleDateString(undefined, { weekday: 'short' })}
        </Label>

        <WeatherbitIcon code={code} iconCode={icon} />

        <Label>{`${convertedTemperature}°${temperatureSymbol}`}</Label>
      </Container>
    )
  }
)
