import { Button, Group, Stack, Icon, useTheme } from '@willowinc/ui'
import { titleCase } from '@willow/common'
import { useTranslation } from 'react-i18next'
import { InsightType } from '@willow/common/insights/insights/types'
import { InsightTypeBadge } from '@willow/common/insights/component'
import _ from 'lodash'

const CategoryList = ({
  isModal,
  category,
  isSelected = false,
  onCategoryChange,
}: {
  category: InsightType
  isSelected?: boolean
  isModal: boolean
  onCategoryChange?: () => void
}) => {
  const theme = useTheme()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <Group
      w="100%"
      px="s16"
      py="s8"
      css={{
        borderBottom: isModal
          ? `1px solid ${theme.color.state.disabled.bg}`
          : 'none',
      }}
    >
      <Stack
        css={{
          flex: '1 1',
          alignSelf: 'stretch',
          justifyContent: 'center',
        }}
      >
        <InsightTypeBadge type={_.camelCase(category)} badgeSize="md" />
      </Stack>
      <Stack w="100px" mr="8px">
        <Button
          prefix={<Icon icon={isSelected ? 'remove' : 'add'} />}
          kind={isSelected ? 'secondary' : 'primary'}
          onClick={onCategoryChange}
          css={{
            width: '100%',
            alignSelf: 'stretch',
            justifyContent: 'center',
          }}
        >
          {titleCase({
            text: t(`plainText.${isSelected ? 'remove' : 'add'}`),
            language,
          })}
        </Button>
      </Stack>
    </Group>
  )
}

export default CategoryList
