import axios, { AxiosInstance } from 'axios';
import { endpoints } from './config';
// axios instance for making in-app requests
let _tlmClient: AxiosInstance | undefined = undefined;
const GetTlmClient = (contentType = '') => {
  if (!_tlmClient) {
    _tlmClient = axios.create({
      baseURL: endpoints.tlmApi,
      headers: {
        'Content-Type': contentType,
        Authorization: localStorage.getItem('token') ?? '',
        'Access-Control-Allow-Origin': '*',
      },
    });
  } else {
    _tlmClient.defaults.headers.common['Content-Type'] = contentType;
    _tlmClient.defaults.headers.common['Authorization'] = localStorage.getItem('token') ?? '';
  }
  return _tlmClient;
};
export { GetTlmClient };
