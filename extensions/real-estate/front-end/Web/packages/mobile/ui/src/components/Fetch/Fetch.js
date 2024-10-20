import FetchContent from './FetchContent'

export { useFetch } from './FetchContext'

export default function Fetch(props) {
  const { url, params, data, handleKey = true, match, ...rest } = props

  const key = handleKey
    ? `${url} ${JSON.stringify(params)} ${JSON.stringify(data)}`
    : undefined

  return (
    <FetchContent
      key={key}
      {...rest}
      url={url}
      params={params}
      data={data}
      handleKey={handleKey}
    />
  )
}
