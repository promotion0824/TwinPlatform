import Fetch from './Fetch/Fetch'

export default function FetchComponent({
  method,
  url,
  params,
  body,
  headers,
  responseType,
  notFound,
  cache,
  handleAbort,
  mock,
  mockTimeout,
  children,
  ...rest
}) {
  const urls = Array.isArray(url) ? url : [url]

  const requests = urls.map((nextUrl, i) => ({
    method: Array.isArray(method) ? method[i] : method,
    url: nextUrl,
    params: Array.isArray(params) ? params[i] : params,
    body: Array.isArray(body) ? body[i] : body,
    headers: Array.isArray(headers) ? headers[i] : headers,
    responseType: Array.isArray(responseType) ? responseType[i] : responseType,
    notFound: Array.isArray(notFound) ? notFound[i] : notFound,
    cache: Array.isArray(cache) ? cache[i] : cache,
    handleAbort: Array.isArray(handleAbort) ? handleAbort[i] : handleAbort,
    mock: Array.isArray(url) && Array.isArray(mock) ? mock[i] : mock,
    mockTimeout: Array.isArray(mockTimeout) ? mockTimeout[i] : mockTimeout,
  }))

  const key = `${JSON.stringify(url)} ${JSON.stringify(params)}`

  return (
    <Fetch
      {...rest}
      key={key}
      requests={requests}
      shouldReturnArray={Array.isArray(url)}
    >
      {children}
    </Fetch>
  )
}
