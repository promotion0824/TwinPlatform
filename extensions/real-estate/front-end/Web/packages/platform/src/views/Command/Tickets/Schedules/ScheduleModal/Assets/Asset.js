import cx from 'classnames'
import { css } from 'styled-components'
import { Flex, Text } from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import styles from './Asset.css'

export default function Asset({
  asset,
  selected,
  children,
  isReadOnly,
  onRemoveClick,
}) {
  const cxClassName = cx(styles.asset, {
    [styles.selected]: selected,
  })

  return (
    <Flex
      key={asset.id}
      horizontal
      fill="header"
      align="middle"
      size="medium"
      className={cxClassName}
    >
      <Flex padding="medium">
        {asset?.identifier && (
          <Text
            type="message"
            size="tiny"
            css={css(({ theme }) => ({
              color: theme.color.neutral.fg.subtle,
            }))}
          >
            {asset.identifier}
          </Text>
        )}
        <Text
          css={css(({ theme }) => ({
            color: theme.color.neutral.fg.default,
          }))}
        >
          {asset.name}
        </Text>
      </Flex>
      {children}
      {onRemoveClick != null && (
        <Flex padding="0 tiny">
          <IconButton
            icon="delete"
            kind="secondary"
            background="transparent"
            disabled={isReadOnly}
            onClick={() => {
              onRemoveClick(asset)
            }}
          />
        </Flex>
      )}
    </Flex>
  )
}
