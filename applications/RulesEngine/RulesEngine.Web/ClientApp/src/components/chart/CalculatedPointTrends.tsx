import { CalculatedPointDto, NamedPointDto } from "../../Rules";
import { TrendLookup } from "../chart/TrendsBase";
import { groupBy } from "../graphs/GraphBase";
import SingleTrend from "./SingleTrend";

/**
 * Plots the trend line for a calculated point
 * @param props
 * @returns
 */
const CalculatedPointTrends = (props: { calculatedPoint: CalculatedPointDto, namedPoints: NamedPointDto[] }) => {

  const dataAxes = [{
    key: props.calculatedPoint.unit ?? "result",
    title: props.calculatedPoint.unit ?? "result",
    longName: 'yaxis',
    shortName: 'y',
  }];

  const grouped = groupBy(props.namedPoints, (d) => d.unit ?? "No Unit");

  const initialData: TrendLookup[] = [];
  for (const key in grouped) {
    const items = grouped[key];
    initialData.push({
      isParent: true,
      name: `[${key}]`,
      label: `[${key}]`,
      id: key,
      trendLine: "",
      selected: false,
      axisKey: key,
      loading: false,
      notFound: false,
      preLoad: false
    });

    for (const itemKey in items.sort((a, b) => {
      return a.id!.localeCompare(b.id!, 'en', { sensitivity: 'base' })
    })) {
      const item = items[itemKey];
      initialData.push({
        isParent: false,
        name: item.variableName!,
        label: `${item.variableName}, ${item.id!}`,
        id: item.id!,
        trendLine: "",
        selected: false,
        axisKey: item.unit!,
        loading: false,
        notFound: false,
        preLoad: false
      });
    }
  }

  return (<SingleTrend
    id={props.calculatedPoint.id!}
    trendItems={initialData}
    existingAxis={dataAxes}
    timezone={props.calculatedPoint.timeZone} />)
}

export default CalculatedPointTrends;
