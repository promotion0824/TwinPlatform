import { useState } from 'react'
import { FloorContext } from './FloorContext'

export default function FloorsProvider({ children }) {
  const [floor, setFloor] = useState(null)
  const [categories, setCategories] = useState([])
  const [selectedCategoryIds, setSelectedCategoryIds] = useState([])
  const [assetSearch, setAssetSearch] = useState('')

  const context = {
    floor,
    setFloor,
    categories,
    setCategories,
    selectedCategoryIds,
    setSelectedCategoryIds,
    assetSearch,
    setAssetSearch,
    isReadOnly: true,
  }

  return (
    <FloorContext.Provider value={context}>{children}</FloorContext.Provider>
  )
}
