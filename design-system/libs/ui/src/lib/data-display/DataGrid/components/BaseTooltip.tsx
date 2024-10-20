import { Tooltip, TooltipProps } from '../../../overlays/Tooltip'

const BaseTooltip = ({
  title,
  children,
}: {
  title: TooltipProps['label']
  children: TooltipProps['children']
}) => {
  if (!title) {
    return children
  }

  // only wrapped in tooltip if there is any content to show
  return (
    // By default the Mantine tooltip will be rendered near the trigger
    // element, but it will cause a bug when using the tooltip in DataGrid,
    // that the tooltip position is calculated wrong and will be hidden
    // with overflow. See details in
    // https://dev.azure.com/willowdev/Unified/_workitems/edit/83737
    <Tooltip label={title} withinPortal>
      {children}
    </Tooltip>
  )
}

export default BaseTooltip
