import { useAnalytics } from '@willow/ui'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'

import { useDropdown } from 'components/Dropdown/Dropdown'

/**
 * @deprecated Please use `MoreButtonDropdown` instead.
 */
export default function MoreDropdownButton({
  icon,
  iconStyle,
  children,
  onClick,
  'data-segment': dataSegment,
  'data-segment-props': dataSegmentProps,
  ...rest
}) {
  const analytics = useAnalytics()
  const dropdown = useDropdown()

  function handleClick(e) {
    if (dataSegment != null) {
      try {
        analytics.track(dataSegment, JSON.parse(dataSegmentProps))
      } catch (err) {
        // do nothing
      }
    }

    e.stopPropagation()

    dropdown.close()

    onClick?.(e)
  }

  return (
    <Button
      data-testid="more-dropdown-button"
      ripple
      {...rest}
      onClick={handleClick}
    >
      <Flex
        horizontal
        fill="header"
        size="large"
        align="middle"
        padding="medium"
        width="100%"
      >
        <Flex>{children}</Flex>
        {icon != null && <Icon icon={icon} style={{ stroke: 'none' }} />}
      </Flex>
    </Button>
  )
}
