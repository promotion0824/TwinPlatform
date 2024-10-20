import * as echarts from 'echarts'
import { darkTheme, lightTheme, ThemeName } from '@willowinc/theme'

export const gridSize = 0

export let willowIntentTheme: string[] = []

function configureEchartsTheme(themeName: ThemeName) {
  const theme = themeName === 'dark' ? darkTheme : lightTheme

  willowIntentTheme = [
    theme.color.intent.positive.fg.default,
    theme.color.intent.notice.fg.default,
    theme.color.intent.negative.fg.default,
  ]

  const axisCommon = function () {
    return {
      axisLabel: {
        fontSize: theme.font.body.xs.regular.fontSize,
      },
      axisLine: {
        lineStyle: {
          color: theme.color.neutral.border.default,
        },
      },
      splitLine: {
        lineStyle: {
          color: theme.color.neutral.border.default,
        },
      },
      splitArea: {
        areaStyle: {
          color: ['rgba(255,255,255,0.02)', 'rgba(255,255,255,0.05)'],
        },
      },
      minorSplitLine: {
        lineStyle: {
          color: theme.color.neutral.border.default,
        },
      },
    }
  }

  const colorPalette = [
    theme.color.data.qualitative.q1,
    theme.color.data.qualitative.q2,
    theme.color.data.qualitative.q3,
    theme.color.data.qualitative.q4,
    theme.color.data.qualitative.q5,
    theme.color.data.qualitative.q6,
    theme.color.data.qualitative.q7,
    theme.color.data.qualitative.q8,
    theme.color.data.qualitative.q9,
    theme.color.data.qualitative.q10,
  ]

  echarts.registerTheme('willow', {
    backgroundColor: 'transparent',

    titleColor: theme.color.neutral.fg.default,

    color: colorPalette,

    axisPointer: {
      shadowStyle: {
        color: theme.color.intent.secondary.bg.subtle.default,
      },
    },

    tooltip: {
      backgroundColor: theme.color.neutral.bg.panel.default,
      borderColor: theme.color.neutral.border.default,
      textStyle: {
        fontSize: theme.font.body.md.regular.fontSize,
        color: theme.color.neutral.fg.default,
      },
    },

    grid: {
      left: gridSize,
      top: gridSize,
      right: gridSize,
      bottom: gridSize,
      containLabel: true,
    },

    legend: {
      align: 'left',
      itemHeight: 12,
      itemWidth: 12,

      textStyle: {
        color: theme.color.neutral.fg.default,
        fontFamily: theme.font.body.xs.regular.fontFamily,
        fontSize: theme.font.body.xs.regular.fontSize,
        fontWeight: theme.font.body.xs.regular.fontWeight,
      },
    },

    textStyle: {
      // Using our font causes axis labels to be cut off sometimes.
      // Will revisit once we update to our new theme font.
      // fontFamily: theme.font.body.md.regular.fontFamily,
      color: theme.color.neutral.fg.default,
    },

    visualMap: {
      align: 'left',
      orient: 'horizontal',
      itemHeight: 12,
      itemWidth: 12,

      textStyle: {
        color: theme.color.neutral.fg.default,
        fontFamily: theme.font.body.xs.regular.fontFamily,
        fontSize: theme.font.body.xs.regular.fontSize,
        fontWeight: theme.font.body.xs.regular.fontWeight,
      },
    },

    timeAxis: axisCommon(),

    logAxis: axisCommon(),

    valueAxis: axisCommon(),

    categoryAxis: axisCommon(),

    graph: {
      color: colorPalette,
    },
  })
}

export default configureEchartsTheme
