import { Button, Group, Stack, Icon, Avatar } from '@willowinc/ui'
import { styled } from 'twin.macro'
import { titleCase, Workgroup } from '@willow/common'
import { useTranslation } from 'react-i18next'
import { TooltipWhenTruncated } from '@willow/ui'

const WorkgroupList = ({
  workgroups,
  selectedIds = [],
  isModal,
  onSelectIds,
  updateWorkGroup,
  disabled,
}: {
  workgroups?: Workgroup[]
  selectedIds?: string[]
  isModal: boolean
  onSelectIds?: (selectedIds: string[]) => void
  updateWorkGroup?: (selectedIds: string[]) => void
  disabled?: boolean
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <>
      {workgroups?.map(({ name, memberIds, id }) => (
        <Group w="100%">
          <Stack
            ml="20px"
            css={{
              flex: '1 1 0',
              alignSelf: 'stretch',
              justifyContent: 'center',
            }}
          >
            <Group>
              <Avatar color="teal" shape="rectangle" size="md" />
              <TooltipWhenTruncated label={name}>
                <WorkgroupText $fullWidth={isModal}>{name}</WorkgroupText>
              </TooltipWhenTruncated>
            </Group>
          </Stack>
          <Stack
            w="50px"
            css={{
              alignSelf: 'stretch',
              justifyContent: 'center',
            }}
          >
            <UserText>
              <span>{memberIds?.length}</span>
              <span>{titleCase({ text: t('labels.users'), language })}</span>
            </UserText>
          </Stack>
          <Stack w="100px" mr="8px">
            <Button
              disabled={disabled}
              prefix={
                <Icon
                  icon={
                    isModal
                      ? selectedIds.includes(id)
                        ? 'remove'
                        : 'add'
                      : 'remove'
                  }
                />
              }
              kind={
                isModal
                  ? selectedIds.includes(id)
                    ? 'secondary'
                    : 'primary'
                  : 'secondary'
              }
              onClick={() =>
                isModal
                  ? selectedIds.includes(id)
                    ? onSelectIds?.(selectedIds.filter((item) => item !== id))
                    : onSelectIds?.([...selectedIds, id])
                  : updateWorkGroup?.(
                      workgroups
                        ?.filter((workgroup) => workgroup.id !== id)
                        .map((workgroup) => workgroup.id) ?? []
                    )
              }
              css={{
                width: '100%',
                alignSelf: 'stretch',
                justifyContent: 'center',
              }}
            >
              {titleCase({
                text: isModal
                  ? selectedIds.includes(id)
                    ? t('plainText.remove')
                    : t('plainText.add')
                  : t('plainText.remove'),
                language,
              })}
            </Button>
          </Stack>
        </Group>
      ))}
    </>
  )
}

export const WorkgroupText = styled.div<{ $fullWidth: boolean }>(
  ({ theme, $fullWidth }) => ({
    ...theme.font.heading.sm,
    color: theme.color.neutral.fg.muted,
    width: $fullWidth ? '500px' : '300px',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  })
)

export const UserText = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.muted,
  display: 'flex',
  gap: '4px',
}))

export default WorkgroupList
