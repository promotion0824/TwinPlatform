import { convertToFahrenheit, TemperatureUnit } from '@willow/common'
import { SiteWeather } from '@willow/common/site/site/types'
import { useUser } from '@willow/ui'
import { Badge } from '@willowinc/ui'
import WeatherbitIcon from '../../../components/WeatherbitIcon/WeatherbitIcon'

export default function WeatherBadge({
  siteWeather,
}: {
  siteWeather: SiteWeather
}) {
  const user = useUser()

  const { temperatureUnit } = user.options
  // Celsius is specifically checked for first in both cases so that fahrenheit
  // can be fallen back to if temperatureUnit is undefined.
  const temperatureSymbol =
    temperatureUnit === TemperatureUnit.celsius ? 'C' : 'F'
  const temperature = Math.round(
    temperatureUnit === TemperatureUnit.celsius
      ? siteWeather.temperature
      : convertToFahrenheit(siteWeather.temperature)
  )

  return (
    <Badge
      prefix={
        <WeatherbitIcon code={siteWeather.code} iconCode={siteWeather.icon} />
      }
      size="md"
    >
      {`${temperature}°${temperatureSymbol}`}
    </Badge>
  )
}
