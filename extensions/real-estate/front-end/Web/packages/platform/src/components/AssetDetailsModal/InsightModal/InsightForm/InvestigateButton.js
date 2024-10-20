import { useModal, useUser, Button } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { titleCase } from '@willow/common'

export default function InvestigateButton({ insight, dataSegmentPropPage }) {
  const modal = useModal()
  const user = useUser()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  let investigateUrl = `/sites/${insight.siteId}`
  if (insight.floorId != null) {
    investigateUrl = `/sites/${insight.siteId}/floors/${insight.floorId}`

    if (insight.asset?.id != null) {
      investigateUrl += `?assetId=${insight.asset?.id}`
    }
  }

  function handleInvestigateButtonClick() {
    user.saveLocalOptions('insight', {
      id: insight.id,
      priority: insight.priority,
      siteId: insight.siteId,
      sequenceNumber: insight.sequenceNumber,
      screenshots: user.localOptions.insight?.screenshots ?? [],
      status: insight.status,
      type: insight.type,
    })

    modal?.close?.()
  }

  return (
    <Button
      color="purple"
      width="medium"
      to={investigateUrl}
      onClick={handleInvestigateButtonClick}
      data-segment="Insight Investigated"
      data-segment-props={JSON.stringify({
        type: insight.type,
        sourceName: insight.sourceName,
        priority: insight.priority,
        page: dataSegmentPropPage,
      })}
    >
      {titleCase({
        text: t('plainText.viewInViewer'),
        language,
      }).replace('3d', '3D')}
    </Button>
  )
}
