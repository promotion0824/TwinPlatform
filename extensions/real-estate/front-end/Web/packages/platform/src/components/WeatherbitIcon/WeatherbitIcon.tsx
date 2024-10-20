import { capitalize } from 'lodash'
import { useTranslation } from 'react-i18next'
import {
  WeatherbitCode,
  WeatherbitIconCode,
} from '../../../../common/src/site/site/weatherbitTypes'

const codeDescriptionMap: Record<WeatherbitCode, string> = {
  200: 'weather.thunderstormWithLightRain',
  201: 'weather.thunderstormWithRain',
  202: 'weather.thunderstormWithHeavyRain',
  230: 'weather.thunderstormWithLightDrizzle',
  231: 'weather.thunderstormWithDrizzle',
  232: 'weather.thunderstormWithHeavyDrizzle',
  233: 'weather.thunderstormWithHail',
  300: 'weather.lightDrizzle',
  301: 'weather.drizzle',
  302: 'weather.heavyDrizzle',
  500: 'weather.lightRain',
  501: 'weather.moderateRain',
  502: 'weather.heavyRain',
  511: 'weather.freezingRain',
  520: 'weather.lightShowerRain',
  521: 'weather.showerRain',
  522: 'weather.heavyShowerRain',
  600: 'weather.lightSnow',
  601: 'weather.snow',
  602: 'weather.heavySnow',
  610: 'weather.mixSnowRain',
  611: 'weather.sleet',
  612: 'weather.heavySleet',
  621: 'weather.snowShower',
  622: 'weather.heavySnowShower',
  623: 'weather.flurries',
  700: 'weather.mist',
  711: 'weather.smoke',
  721: 'weather.haze',
  731: 'weather.sandDust',
  741: 'weather.fog',
  751: 'weather.freezingFog',
  800: 'weather.clearSky',
  801: 'weather.fewClouds',
  802: 'weather.scatteredClouds',
  803: 'weather.brokenClouds',
  804: 'weather.overcastClouds',
  900: 'weather.unknownPrecipitation',
}

export default function WeatherIcon({
  code,
  iconCode,
  width = 16,
}: {
  code: WeatherbitCode
  iconCode: WeatherbitIconCode
  width?: number
}) {
  const { t } = useTranslation()

  return (
    <img
      alt={capitalize(t(codeDescriptionMap[code]))}
      src={`/public/weatherbit-icons/${iconCode}.png`}
      width={width}
    />
  )
}
