export default async function getGuid(model, id) {
  return new Promise((resolve) => {
    model.getProperties(id, async (result) => {
      let guid = result.properties.find(
        (property) => property.displayName === 'GUID'
      )?.displayValue
      let type = 'normal'

      if (guid == null) {
        const parentId = result.properties.find(
          (property) => property.displayName === 'parent'
        )?.displayValue
        if (parentId != null) {
          const nextGuidType = await getGuid(model, parentId)

          guid = nextGuidType.guid
          type = 'parent'
        }
      }

      resolve({ guid, type })
    })
  })
}
