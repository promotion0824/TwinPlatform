import { BasicDigitalTwin, DigitalTwinMetadata } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function usePutTwin() {
  const api = useApi();

  async function saveTwin({ newTwin }: { newTwin: BasicDigitalTwin }) {
    let { $etag, $lastUpdateTime, lastUpdateTime, ...rest } = newTwin; // exclude props to make twin valid for ADT CreateOrReplaceDigitalTwinAsync

    // prepare twin for putTwin request
    let twin = new BasicDigitalTwin(rest);
    twin.$metadata = new DigitalTwinMetadata(twin.$metadata);

    return await api.putTwin(twin!);
  }

  return { saveTwin };
}
