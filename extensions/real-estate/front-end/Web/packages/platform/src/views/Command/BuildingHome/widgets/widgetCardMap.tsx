import { useTranslation } from 'react-i18next'

import { WidgetId } from '../../../../store/buildingHomeSlice'
import { DraggableContent } from '../DraggableColumnLayout'
import InsightsWidget from './Insights/InsightsWidget'
import KPISummaryWidget from './KPISummary/KPISummaryWidget'
import ThreeDModelWidget from './ThreeDModel/ThreeDModelWidget'
import threeDModelThumbnail from './thumbnails/3d_model_thumbnail.png'
import insightsThumbnail from './thumbnails/insights_thumbnail.png'
import kpiSummaryThumbnail from './thumbnails/kpi_summary_thumbnail.png'
import ticketsThumbnail from './thumbnails/tickets_thumbnail.png'
import TicketsWidget from './Tickets/TicketsWidget'

const WidgetCardMap: Record<
  Exclude<WidgetId, WidgetId.Location>,
  {
    useTitle: () => string
    description: string
    imageSrc: string
    defaultHeight: number
    component: DraggableContent
  }
> = {
  [WidgetId.KpiSummary]: {
    useTitle: () => useTranslatedTitle('headers.kpiSummary'),
    // TODO: update description
    description:
      'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
    imageSrc: kpiSummaryThumbnail,
    defaultHeight: 210,
    component: KPISummaryWidget,
  },
  [WidgetId.Insights]: {
    useTitle: () => useTranslatedTitle('headers.insights'),
    // TODO: update description
    description:
      'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
    imageSrc: insightsThumbnail,
    defaultHeight: 384,
    component: InsightsWidget,
  },
  [WidgetId.Tickets]: {
    useTitle: () => useTranslatedTitle('headers.tickets'),
    // TODO: update description
    description:
      'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
    imageSrc: ticketsThumbnail,
    defaultHeight: 306,
    component: TicketsWidget,
  },
  [WidgetId.ThreeDModel]: {
    useTitle: () => useTranslatedTitle('twinExplorer.3dModel'),
    // TODO: update description
    description:
      'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
    imageSrc: threeDModelThumbnail,
    defaultHeight: 648,
    component: ThreeDModelWidget,
  },
}

const useTranslatedTitle = (translationString: string) => {
  const { t } = useTranslation()
  return t(translationString)
}

export default WidgetCardMap
