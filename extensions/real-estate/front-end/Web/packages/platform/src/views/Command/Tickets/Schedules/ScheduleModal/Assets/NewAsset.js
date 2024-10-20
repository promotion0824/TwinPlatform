import { Flex, Text } from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import { styled, css } from 'twin.macro'
import styles from './Asset.css'

const StyledFlexContainer = styled(Flex)(
  (props) => css`
    background-color: #8779e22b !important;
    & > div:nth-child(1) {
      flex: 0;
    }

    color: var(--light);

    ${props.isSubmittedNewAsset &&
    `
      background-color: var(--theme-color-neutral-bg-accent-default) !important;
    `}
  `
)

const StyledFlexNoOverflow = styled(Flex)`
  overflow: unset !important;
`

const StyledFlexWidth90 = styled(Flex)`
  width: 90%;
  margin-left: 3px !important;
`

const PurpleDot = styled.span`
  height: 6px;
  width: 6px;
  background-color: var(--purple);
  border-radius: 50%;
  position: relative;
  left: -16px;
`
export default function NewAsset({
  asset,
  isReadOnly,
  onRemoveClick,
  isSubmittedNewAsset,
}) {
  return (
    <>
      <StyledFlexContainer
        key={asset.id}
        horizontal
        fill="header"
        align="middle"
        size="medium"
        className={styles.asset}
        isSubmittedNewAsset={isSubmittedNewAsset}
      >
        <StyledFlexNoOverflow>
          <PurpleDot />
        </StyledFlexNoOverflow>

        <StyledFlexWidth90>
          <Text>{asset.name}</Text>
        </StyledFlexWidth90>

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
      </StyledFlexContainer>
    </>
  )
}
