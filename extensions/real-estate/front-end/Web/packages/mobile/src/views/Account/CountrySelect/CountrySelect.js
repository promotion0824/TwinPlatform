import { useEffect, useState } from 'react'
import {
  cookie,
  setApiGlobalPrefix,
  Select,
  Option,
  useUser,
  useApi,
} from '@willow/mobile-ui'
import { useDeviceId } from 'providers'
import styles from './CountrySelect.css'

const countries = [
  {
    value: 'au',
    text: 'AUS',
  },
  {
    value: 'us',
    text: 'USA',
  },
  {
    value: 'eu',
    text: 'EU',
  },
]

export default function CountrySelect({ ...rest }) {
  const [country, setCountry] = useState(() => cookie.get('api') ?? 'us')
  const [firstLoad, setFirstLoad] = useState(true)
  const user = useUser()
  const deviceId = useDeviceId()
  const api = useApi()

  useEffect(() => {
    async function changeRegion() {
      setApiGlobalPrefix(country)

      if (firstLoad) {
        setFirstLoad(false)
      } else {
        if (deviceId != null) {
          try {
            await api.delete(`/api/installations?pnsHandle=${deviceId}`)
          } catch (err) {
            // do nothing
          }
        }
        await user.logout()
      }
    }

    changeRegion()
  }, [country])

  return (
    <Select
      icon="globe"
      iconClassName={styles.icon}
      {...rest}
      value={country}
      text={(value) =>
        countries.find((nextValue) => nextValue.value === value)?.text
      }
      onChange={setCountry}
    >
      {countries.map((nextValue) => (
        <Option key={nextValue.value} value={nextValue.value}>
          {nextValue.text}
        </Option>
      ))}
    </Select>
  )
}
