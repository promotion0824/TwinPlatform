import styled from 'styled-components'
import { GridColTypeDef } from '../../data-display/DataGrid'
import { Progress } from '../../feedback/Progress'
import { IntentThresholds } from '../utils/chartUtils'

const Container = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s8,
  width: '100%',
}))

const ProgressContainer = styled.div({
  display: 'block',
  width: '100%',
})

type ProgressColumnTypeProps = {
  /**
   * Set the thresholds where the progress colors should change.
   * @default { positiveThreshold: 100, noticeThreshold: 75 }
   */
  intentThresholds?: IntentThresholds
}

export const progressColumnType = ({
  intentThresholds = {
    positiveThreshold: 100,
    noticeThreshold: 75,
  },
}: ProgressColumnTypeProps): GridColTypeDef<number> => ({
  flex: 1,
  renderCell: ({ value }) =>
    value && (
      <Container>
        <ProgressContainer>
          <Progress
            intent={
              value >= intentThresholds.positiveThreshold
                ? 'positive'
                : value >= intentThresholds.noticeThreshold
                ? 'notice'
                : 'negative'
            }
            value={value}
          />
        </ProgressContainer>
        <div>{value}</div>
      </Container>
    ),
  type: 'number',
})
