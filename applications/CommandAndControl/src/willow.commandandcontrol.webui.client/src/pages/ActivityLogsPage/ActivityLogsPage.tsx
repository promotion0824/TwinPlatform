import { ActivityLogsProvider } from "./ActivityLogsProvider";
import { ActivityLogs } from "./components/ActivityLogs";

const ActivityLogsPage = () => (
  <ActivityLogsProvider>
    <ActivityLogs />
  </ActivityLogsProvider>
);

export default ActivityLogsPage;
