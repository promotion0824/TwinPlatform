import { Client } from '../services/Clients';
import { endpoints } from '../config';
import { GetTlmClient } from '../axiosWithToken';

const useApi = (contentType?: string) => {
  let axiosClient = GetTlmClient(contentType);
  return new Client(
    endpoints.tlmApi,
    axiosClient);
}

export default useApi;
