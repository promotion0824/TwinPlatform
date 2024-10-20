import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'

import { WidgetId } from '../../../../../store/buildingHomeSlice'
import { useClassicExplorerLandingPath } from '../../../../Layout/Layout/Header/utils'
import { DraggableContent } from '../../DraggableColumnLayout'
import BuildingHomeWidgetCard from '../BuildingHomeWidgetCard'
import ThreeDViewModel from './ThreeDModelView'

const ThreeDModelWidget: DraggableContent<WidgetId> = forwardRef(
  ({ canDrag, id, ...props }, ref) => {
    const { t } = useTranslation()
    const classicExplorerLandingPath = useClassicExplorerLandingPath()

    return (
      <BuildingHomeWidgetCard
        {...props}
        ref={ref}
        isDraggingMode={canDrag}
        {...(classicExplorerLandingPath
          ? {
              navigationButtonContent: t('interpolation.goTo', {
                value: t('plainText.3dViewer'),
              }),
              navigationButtonLink: classicExplorerLandingPath,
            }
          : {})}
        title={t('twinExplorer.3dModel')}
        id={id}
      >
        <ThreeDViewModel
          css={({ theme }) => ({
            height: 582 /* fixed height */,
            backgroundColor: theme.color.neutral.bg.accent.default,

            border: `1px solid ${theme.color.neutral.border.default}`,
            '&, .adsk-viewing-viewer': {
              borderRadius: theme.radius.r4,
            },

            '.forge-spinner': {
              // the position of the spinner is not correct and overflows the Widget component,
              // which will cause the dragging component includes the neighbor component content that is overlap with
              // this spinner. So we hide it before get fixed.
              // !important required to override style attribute.
              display: 'none !important',
            },
          })}
        />
      </BuildingHomeWidgetCard>
    )
  }
)

export default ThreeDModelWidget
