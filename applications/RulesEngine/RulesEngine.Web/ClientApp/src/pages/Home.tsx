import { CircularProgress, Stack } from "@mui/material";
import { useQuery } from "react-query";
import useApi from "../hooks/useApi";
import { SystemSummaryDto } from "../Rules";
import PieChart from "../components/chart/PieChart";
import HomeDiagram from "../components/graphs/HomeDiagram";
import { ErrorBoundary } from "react-error-boundary";
import FlexTitle from "../components/FlexPageTitle";

const Summary = (props: { summary: SystemSummaryDto }) => {
  if (!props.summary) return <div></div>;
  return (
    <div>
      <HomeDiagram summary={props.summary!} />
      <ErrorBoundary fallback={<p>Unable to display pie charts</p>}>
        <PieChart summary={props.summary!}></PieChart>
      </ErrorBoundary>
    </div>
  );
};

const Home = () => {
  const apiclient = useApi();

  const user = useQuery(["user"], async () => {
    const data = await apiclient.getUserInfo("me");
    console.log("User", user);
    return data;
  });

  const environment = useQuery("environment", async () => {
    const data = await apiclient.getEnvironmentInfo();
    return data;
  });

  const summary = useQuery("summary", async () => {
    const data = await apiclient.systemSummary();
    console.log("Summary", data);
    return data;
  });


  const progressQuery = useQuery(
    ["progress-summary"],
    async (_x: any) => {
      const progressData = await apiclient.progress();
      return progressData;
    },
    {
      refetchOnMount: "always",
      onSuccess: (data) => { },
    }
  );

  return (

    <Stack spacing={2}>
      <FlexTitle>
        <>Willow Activate:{" "}
        {environment.isFetched ? (
          <>{environment.data?.environmentName}</>
        ) : environment.isError ? (
          <>Error</>
        ) : (
          <>...</>
        )}</>
      </FlexTitle>

      {summary.isFetched && summary.data && (
        <Summary summary={summary.data} />
      )}

      {progressQuery.isFetched && (
        <></>
      )}

      {!progressQuery.isFetched && <CircularProgress />}
    </Stack>
  );
};

export default Home;
