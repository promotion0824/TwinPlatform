import { CapabilityDto } from "../../Rules";
import { groupBy } from "../graphs/GraphBase";
import TrendsBase, { TrendLookup } from "../chart/TrendsBase";
import { stripPrefix } from "../LinkFormatters";

/**
 * Plots the trend lines for multiple capabilties
 */
const CapabilitiesTrends = (props: { capabilities: CapabilityDto[], timezone?: string }) => {

  const grouped = groupBy(props.capabilities, (d) => d.unit ?? stripPrefix(d.modelId ?? "No unit"));

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

    const sorted = items.sort((a, b) => {
      return a.id!.localeCompare(b.id!, 'en', { sensitivity: 'base' })
    });

    for (const itemKey in sorted) {
      const item = items[itemKey];
      initialData.push({
        isParent: false,
        name: item.name!,
        label: `${item.name!}, ${item.id!}`,
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

  return (<TrendsBase trendItems={initialData} timezone={props.timezone} />);
}

export default CapabilitiesTrends;
