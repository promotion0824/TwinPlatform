import Fetch from 'components/Fetch/Fetch'
import TableComponent from './Table/Table'

export { default as Body } from './Table/Body'
export { default as Head } from './Table/Head'
export { default as Row } from './Table/Row'
export { default as Cell } from './Table/Cell'

export default function Table({
  name,
  url,
  params,
  cache,
  mock,
  items,
  notFound,
  onResponse,
  children,
  ...rest
}) {
  return (
    <Fetch
      name={name}
      url={url}
      params={params}
      cache={cache}
      mock={mock}
      notFound={notFound}
      onResponse={onResponse}
    >
      {(response) => (
        <TableComponent
          {...rest}
          response={items ?? response}
          notFound={notFound}
        >
          {children}
        </TableComponent>
      )}
    </Fetch>
  )
}
