import cx from 'classnames'
import { Button, Flex, Text } from '@willow/ui'
import { css } from 'styled-components'
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
          <Button
            icon="trash"
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
