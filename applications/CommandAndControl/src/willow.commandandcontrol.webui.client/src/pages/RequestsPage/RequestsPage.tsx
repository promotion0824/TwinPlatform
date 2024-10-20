import { useParams } from "react-router-dom";
import Requests from "./components/Requests";
import RequestsProvider from "./RequestsProvider";
import { RequestDetails } from "./components/RequestDetails/RequestDetails";
import { TwinId } from "../../../types/TwinId";

export const RequestsPage = () => {

  const { connectorId, twinId } = useParams();

  const id:TwinId | undefined = connectorId && twinId ? {connectorId, twinId} : undefined;

  return (
    <RequestsProvider selectedId={id}>
      {id ? <RequestDetails /> : <Requests />}
    </RequestsProvider>
  );
}

export default RequestsPage;
