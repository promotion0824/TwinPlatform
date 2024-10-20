import { useEffect, useState } from 'react'
import {
  useApi,
  cookie,
  setApiGlobalPrefix,
  Select,
  Option,
  useUser,
} from '@willow/ui'
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

export default function CountrySelect() {
  const [country, setCountry] = useState(() => cookie.get('api') ?? 'us')
  const [firstLoad, setFirstLoad] = useState(true)
  const user = useUser()
  const api = useApi()

  useEffect(() => {
    async function changeRegion() {
      setApiGlobalPrefix(country)

      if (firstLoad) {
        setFirstLoad(false)
      } else {
        await api.post('/api/me/signout')
        await user.logout()
      }
    }

    changeRegion()
  }, [country])

  return (
    <Select
      icon="country"
      value={country}
      unselectable
      onChange={setCountry}
      className={styles.countrySelect}
    >
      {countries.map((nextValue) => (
        <Option key={nextValue.value} value={nextValue.value}>
          {nextValue.text}
        </Option>
      ))}
    </Select>
  )
}
