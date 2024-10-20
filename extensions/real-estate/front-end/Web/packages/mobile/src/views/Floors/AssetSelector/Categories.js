import { useParams } from 'react-router'
import { TreeView, TreeViewItem } from '@willow/mobile-ui'
import { useFloor } from 'providers'

export default function Categories({
  dataSegmentLocation,
  selectedCategoryIds,
  setSelectedCategoryIds,
  setCategories,
}) {
  const params = useParams()
  const floorContext = useFloor()
  const { floor } = floorContext

  function renderItems(items) {
    return items.map((item) => (
      <TreeViewItem
        key={item.id}
        itemId={item.id}
        header={item.name}
        isLeaf={item.childCategories.length === 0}
        data-segment="Asset Category Clicked"
        data-segment-props={JSON.stringify({
          category: item.name,
          page: dataSegmentLocation,
        })}
      >
        {renderItems(item.childCategories)}
      </TreeViewItem>
    ))
  }

  return (
    <TreeView
      url={`/api/sites/${params.siteId}/assets/categories`}
      params={{
        floorId:
          floor.name !== 'BLDG' && floor.name !== 'SOFI CAMPUS OVERALL'
            ? floor.id
            : undefined,
        liveDataAssetsOnly: !floorContext.isReadOnly,
      }}
      notFound="No categories found"
      itemIds={selectedCategoryIds}
      onChange={setSelectedCategoryIds}
      onResponse={setCategories}
    >
      {(items) => renderItems(items)}
    </TreeView>
  )
}
