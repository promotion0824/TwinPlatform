import { Fetch } from '@willow/ui'
import TableComponent from './Table/Table'

export { useTable } from './Table/TableContext'
export { default as Head } from './Table/Head'
export { default as Body } from './Table/Body'
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
        <TableComponent {...rest} items={items ?? response} notFound={notFound}>
          {children}
        </TableComponent>
      )}
    </Fetch>
  )
}
