import { TwinChip } from '@willow/ui'
import TwinModelChip from '@willow/common/twins/view/TwinModelChip'
import {
  buildingModelId,
  buildingModelOfInterest,
  useModelsOfInterest,
} from '@willow/common/twins/view/modelsOfInterest'

/**
 * Renders a chip for a site. If the user has configured a model of interest
 * for the Building model, it is used, otherwise a default styling is used.
 * If `siteName` is provided, we render an instance chip using the site name,
 * otherwise we render a model chip.
 */
export default function SiteChip({ siteName }: { siteName?: string }) {
  const modelsOfInterest = useModelsOfInterest()

  if (!modelsOfInterest.isSuccess) {
    return null
  }

  const modelOfInterest =
    modelsOfInterest.data.items.find((m) => m.modelId === buildingModelId) ??
    buildingModelOfInterest

  return siteName != null ? (
    <TwinChip
      variant="instance"
      modelOfInterest={modelOfInterest}
      text={siteName}
    />
  ) : (
    <TwinModelChip modelOfInterest={modelOfInterest} />
  )
}
