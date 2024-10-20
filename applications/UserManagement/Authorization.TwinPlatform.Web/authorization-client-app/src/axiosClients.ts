import axios, { AxiosInstance } from 'axios';
import { endpoints } from './Config';

let authAxiosClient: AxiosInstance | undefined = undefined;

export const getAuthClient = (contentType = 'application/json') => {
    if (!authAxiosClient) {
        authAxiosClient = axios.create({
            baseURL: endpoints.authApi,
            headers: {
                'Content-Type': contentType,
                'Access-Control-Allow-Origin': '*',
            },
        });
    } else {
        authAxiosClient.defaults.headers.common['Content-Type'] = contentType;
    }
    return authAxiosClient;
};
