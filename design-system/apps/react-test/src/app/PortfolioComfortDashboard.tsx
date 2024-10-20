import {
  AreaChart,
  BarChart,
  ChartCard,
  MetricCard,
  PageTitle,
  PageTitleItem,
} from '@willowinc/ui'
import styled, { css } from 'styled-components'

export const Dashboard = () => {
  return (
    <Container>
      <PageTitle>
        <PageTitleItem
          css={{
            color: '#c6c6c6',
          }}
        >
          Portfolio Comfort Dashboard
        </PageTitleItem>
      </PageTitle>

      <MetricContainer>
        <MetricCard
          title="Comfort Score"
          trendDifference={1.5}
          trendDirection="upwards"
          trendSentiment="positive"
          trendValue={1.5}
          value={75}
        />
      </MetricContainer>

      <ChartsContainer>
        <ChartCard
          chart={
            <BarChart
              dataset={[
                {
                  data: [87, 82, 64, 58, 35],
                  name: 'Building 1',
                },
              ]}
              labels={[
                '1 Manhattan West',
                '200 Liberty St',
                '5 Manhattan West',
                '4 Manhattan West',
                '1 Liberty Plaza',
              ]}
              intentThresholds={{ noticeThreshold: 40, positiveThreshold: 80 }}
              theme="willow-intent"
            />
          }
          title="Score Breakdown by Building"
          css={{
            height: 400,
          }}
        />

        <ChartCard
          chart={
            <AreaChart
              dataset={[
                {
                  data: [
                    65, 79, 81, 83, 84, 73, 61, 61, 75, 76, 77, 76, 71, 64, 75,
                    79, 81, 78, 79, 73, 62, 64, 70, 70, 71, 72, 66, 58, 70, 73,
                  ],
                  name: 'Score',
                },
              ]}
              labels={[
                'Dec 25 Mon',
                'Dec 26 Tue',
                'Dec 27 Wed',
                'Dec 28 Thu',
                'Dec 29 Fri',
                'Dec 30 Sat',
                'Dec 31 Sun',
                'Jan 01 Mon',
                'Jan 02 Tue',
                'Jan 03 Wed',
                'Jan 04 Thu',
                'Jan 05 Fri',
                'Jan 06 Sat',
                'Jan 07 Sun',
                'Jan 08 Mon',
                'Jan 09 Tue',
                'Jan 10 Wed',
                'Jan 11 Thu',
                'Jan 12 Fri',
                'Jan 13 Sat',
                'Jan 14 Sun',
                'Jan 15 Mon',
                'Jan 16 Tue',
                'Jan 17 Wed',
                'Jan 18 Thu',
                'Jan 19 Fri',
                'Jan 20 Sat',
                'Jan 21 Sun',
                'Jan 22 Mon',
                'Jan 23 Tue',
              ]}
            />
          }
          title="Daily Comfort Score"
          css={{
            height: 400,
          }}
        />
      </ChartsContainer>
    </Container>
  )
}

const Container = styled.div(
  ({ theme }) => css`
    height: 100%;
    width: 100%;
    padding: ${theme.spacing.s16};
    display: flex;
    flex-direction: column;
    gap: ${theme.spacing.s16};
    overflow-y: auto;
  `
)

const MetricContainer = styled.div(
  ({ theme }) => css`
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: ${theme.spacing.s16};
  `
)

const ChartsContainer = styled.div(
  ({ theme }) => css`
    width: 100%;
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: ${theme.spacing.s16};
  `
)
