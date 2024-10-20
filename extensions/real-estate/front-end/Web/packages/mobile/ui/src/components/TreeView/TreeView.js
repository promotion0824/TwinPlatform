import _ from 'lodash'
import Fetch from 'components/Fetch/Fetch'
import NotFound from 'components/NotFound/NotFound'
import TreeViewComponent from './TreeView/TreeView'

export { default as TreeViewItem } from './TreeView/TreeViewItem'

export default function TreeView({
  itemIds,
  notFound,
  children,
  onChange,
  ...rest
}) {
  return (
    <Fetch {...rest}>
      {(response) => {
        if (_.isEqual(response, []) && notFound != null) {
          return <NotFound>{notFound}</NotFound>
        }

        return (
          <TreeViewComponent
            response={response}
            itemIds={itemIds}
            onChange={onChange}
          >
            {children}
          </TreeViewComponent>
        )
      }}
    </Fetch>
  )
}
