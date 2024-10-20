import styled from "@emotion/styled";
import { useOverviewContext } from "../OverviewProvider";
import ReactECharts from "echarts-for-react";
import { Loader } from "@willowinc/ui";
import ErrorMessage from "../../../components/error/ErrorMessage";
import { useMemo } from "react";
import { CenterContainer } from "../../../components/Styled/CenterContainer";

export default function TrendChart() {
  const { getStatisticsQuery } = useOverviewContext();
  const { data, isLoading, isError } = getStatisticsQuery || {};
  const { commandTrends } = data || {};

  const { categories = [], dataset = {} } = commandTrends || {};

  const chartDataset = useMemo(() => {
    return {
      source: [
        ["Category", ...Object.values(dataset).map(({ name }) => name)],
        ...categories.map((category, index) => [
          category,
          ...Object.values(dataset).map(({ data = [] }) => data[index]),
        ]),
      ],
    };
  }, [dataset, categories]);

  const series = useMemo(
    () =>
      Object.values(dataset).map(() => ({
        smooth: false,
        type: "line",
        symbol: "rect",
      })),
    [dataset]
  );

  // https://echarts.apache.org/en/option.html
  const option = {
    dataset: chartDataset,
    legend: {
      left: 0,
      top: "bottom",
      align: "left",
    },
    grid: {
      left: "20px",
      show: true,
      right: "20px",
      top: "20px",
      bottom: "40px",
      borderColor: "#3B3B3B",
    },
    series,
    tooltip: {
      trigger: "axis",
    },
    xAxis: {
      boundaryGap: true,
      type: "category",
      splitLine: { show: true },
    },
    yAxis: { type: "value", axisLine: { show: true }, minInterval: 1 },
  };

  if (isLoading)
    return (
      <CenterContainer>
        <Loader size="md" />
      </CenterContainer>
    );

  if (isError)
    return (
      <CenterContainer>
        <ErrorMessage />
      </CenterContainer>
    );

  return (
    <ReactECharts
      option={option}
      theme="willow"
      style={{ height: "100%", width: "100%" }}
    />
  );
}
