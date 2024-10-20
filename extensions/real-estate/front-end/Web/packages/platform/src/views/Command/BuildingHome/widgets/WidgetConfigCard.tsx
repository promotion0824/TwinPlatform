import { Badge, Box, Button, Card, Group, Icon, Stack } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'

interface WidgetConfigCardProps {
  widgetId: string
  getTitle: () => string
  description: string
  imageSrc: string
  added: boolean
  onAdd: (widgetId: string) => void
}

const WidgetConfigCard = ({
  widgetId,
  added = true,
  getTitle,
  description,
  imageSrc,
  onAdd,
}: WidgetConfigCardProps) => {
  const { t } = useTranslation()
  const title = getTitle()

  return (
    <Card bg="neutral.bg.accent.default" miw={0}>
      <Stack p="s12" gap="s16">
        <Group justify="space-between" wrap="nowrap">
          <Title>{title}</Title>
          {added && (
            <Badge
              variant="subtle"
              color="green"
              prefix={<Icon icon="check_circle" size={16} />}
              css={{
                textTransform: 'capitalize',
                flexShrink: 0,
              }}
            >
              {t('plainText.added')}
            </Badge>
          )}
        </Group>

        <Box
          component="img"
          p="s8"
          bg="neutral.bg.panel.default"
          maw="100%"
          src={imageSrc}
          alt={title}
          css={css(({ theme }) => ({
            borderRadius: theme.radius.r4,
            border: `1px solid ${theme.color.neutral.border.default}`,
          }))}
        />

        <p>{description}</p>

        <Button
          onClick={() => onAdd(widgetId)}
          variant="primary"
          prefix={<Icon icon="add" />}
          disabled={added}
          ml="auto"
        >
          {t('labels.addToHome')}
        </Button>
      </Stack>
    </Card>
  )
}

const Title = styled.h4(({ theme }) => ({
  ...theme.font.heading.sm,
  color: theme.color.neutral.fg.default,
  margin: 0,
  textOverflow: 'ellipsis',
  overflow: 'hidden',
  whiteSpace: 'nowrap',
}))

export default WidgetConfigCard
