import { FullSizeLoader, titleCase } from '@willow/common'
import { Badge, Button, Group, Icon, Stack, useTheme } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useNotificationSettingsContext } from '../../NotificationSettingsContext'
import CategoryList from './CategoryList'

const CategoriesModalContent = () => {
  const {
    selectedCategories = [],
    onCategoriesChange,
    categories,
    queryStatus,
  } = useNotificationSettingsContext()

  const theme = useTheme()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const allFilteredSelected = selectedCategories.length === categories.length

  return (
    <Group css={{ overflowY: 'auto', display: 'block' }} w="100%" h="380px">
      {queryStatus === 'loading' ? (
        <FullSizeLoader />
      ) : (
        queryStatus === 'success' &&
        categories?.length > 0 && (
          <>
            <Group
              w="100%"
              css={{
                padding: '8px 16px',
                borderBottom: `1px solid ${theme.color.state.disabled.bg}`,
              }}
            >
              <Stack
                css={{
                  flex: '1 1',
                  alignSelf: 'stretch',
                  justifyContent: 'center',
                  ...theme.font.heading.sm,
                }}
              >
                <span>
                  {t('plainText.categories')}
                  <Badge color="gray" ml="s4">
                    {categories?.length}
                  </Badge>
                </span>
              </Stack>
              <Stack w="100px" mr="s8">
                <Button
                  w="100%"
                  kind="secondary"
                  prefix={
                    <Icon icon={allFilteredSelected ? 'remove' : 'add'} />
                  }
                  onClick={() => {
                    onCategoriesChange(
                      allFilteredSelected
                        ? []
                        : categories.map((category) =>
                            _.camelCase(category.value)
                          )
                    )
                  }}
                  css={{
                    alignSelf: 'stretch',
                    justifyContent: 'center',
                  }}
                >
                  {titleCase({
                    text: t(
                      `plainText.${
                        allFilteredSelected ? 'removeAll' : 'addAll'
                      }`
                    ),
                    language,
                  })}
                </Button>
              </Stack>
            </Group>
            {categories.map((category) => (
              <CategoryList
                isModal
                category={category.value}
                isSelected={selectedCategories.includes(
                  _.camelCase(category.value)
                )}
                onCategoryChange={() => {
                  onCategoriesChange(
                    _.xor(selectedCategories, [_.camelCase(category.value)])
                  )
                }}
              />
            ))}
          </>
        )
      )}
    </Group>
  )
}

export default CategoriesModalContent
