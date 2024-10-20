import { Icon, IconButton, Menu, Tooltip } from "@willowinc/ui";
import { useActivityLogs } from "../ActivityLogsProvider";

export const ActivityLogDownload = () => {

  const { downloadActivityLogsQuery } = useActivityLogs();

  return (
    <IconButton kind="secondary" icon="download" onClick={() => downloadActivityLogsQuery.query()} />
  );
}
