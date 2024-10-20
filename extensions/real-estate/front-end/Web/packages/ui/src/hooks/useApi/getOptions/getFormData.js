import _ from 'lodash'

function getValues(obj, key = '') {
  if (Array.isArray(obj)) {
    return obj.flatMap((item, i) =>
      Array.isArray(item) || (_.isObject(item) && !(item instanceof File))
        ? getValues(item, `${key}[${i}]`)
        : getValues(item, `${key}`)
    )
  }

  if (_.isObject(obj) && !(obj instanceof File)) {
    return Object.entries(obj).flatMap(([entryKey, entryValue]) =>
      getValues(entryValue, `${key === '' ? '' : `${key}.`}${entryKey}`)
    )
  }

  if (obj == null) {
    return []
  }

  return [[key, obj]]
}

export default function getFormData(data) {
  const formData = new FormData()

  const values = getValues(data)

  values.forEach(([key, value]) => {
    formData.append(key, value)
  })

  return formData
}
