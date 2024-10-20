import { clamp } from 'lodash'
import { IconButton, Group, Button, IconName } from '@willowinc/ui'
import styled, { css } from 'styled-components'
import { NumberInput } from '@willow/ui'
import { useCallback } from 'react'
import { useTranslation } from 'react-i18next'

const ControlsContainer = styled(Group)(
  ({ theme }) => css`
    z-index: 999;
    border-radius: ${theme.radius.r4};
    border: 1px solid ${theme.color.neutral.border.default};
    padding: ${theme.spacing.s8};
    background-color: ${theme.color.neutral.bg.panel.default};

    position: absolute;
    width: fit-content;
    left: 0;
    right: 0;
    margin: auto;
    bottom: ${theme.spacing.s16};
  `
)
const SCALE_STEP = 0.5
export const DEFAULT_PDF_SCALE = 1
const MIN_PDF_SCALE = 0.5

export default function FilePreviewControls({
  currentPage,
  onPageChange,
  pageCount,
  currentScale,
  onScaleChange,
}: {
  currentPage: number
  onPageChange: (number: number | ((number: number) => void)) => void
  pageCount: number
  currentScale: number
  onScaleChange: (number: number | ((number: number) => void)) => void
}) {
  const { t } = useTranslation()

  const scaleUp = useCallback(
    () =>
      onScaleChange((curScale: number) => {
        if (curScale - SCALE_STEP <= 0) {
          // cannot be smaller than minimum scale
          return MIN_PDF_SCALE
        }
        return curScale - SCALE_STEP
      }),
    [onScaleChange]
  )
  const scaleDown = useCallback(
    () => onScaleChange((curScale: number) => curScale + SCALE_STEP),
    [onScaleChange]
  )
  const resetScale = useCallback(
    () => onScaleChange(DEFAULT_PDF_SCALE),
    [onScaleChange]
  )

  return (
    <ControlsContainer gap="s12">
      <Group gap="s4">
        {currentScale !== DEFAULT_PDF_SCALE && (
          <Button kind="secondary" onClick={resetScale}>
            {t('plainText.reset')}
          </Button>
        )}
        <IconButton
          kind="secondary"
          onClick={scaleUp}
          icon="zoom_out"
          disabled={currentScale === MIN_PDF_SCALE}
        />
        <IconButton kind="secondary" onClick={scaleDown} icon="zoom_in" />
      </Group>
      <Group gap="s8">
        {/* TODO replace as @willowinc/ui NumberInput once available */}
        {/* @ts-expect-error //children is required in NumberInput, but won't bother to fix the legacy component */}
        <NumberInput
          css={{ width: 96 }}
          min={1}
          max={pageCount}
          format="0"
          value={currentPage}
          onChange={onPageChange}
        />
        <IconButton
          kind="secondary"
          onClick={() => onPageChange(clamp(currentPage - 1, 1, pageCount))}
          icon={'navigate_before' as IconName}
          disabled={currentPage === 1}
        />
        <IconButton
          kind="secondary"
          onClick={() => onPageChange(clamp(currentPage + 1, 1, pageCount))}
          icon={'navigate_next' as IconName}
          disabled={currentPage === pageCount}
        />

        <PageNumberDisplay>{`Page ${currentPage} of ${pageCount}`}</PageNumberDisplay>
      </Group>
    </ControlsContainer>
  )
}

const PageNumberDisplay = styled.div(
  ({ theme }) => css`
    ${theme.font.body.sm}
    color: ${theme.color.neutral.fg.default};
  `
)
