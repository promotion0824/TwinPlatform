import { Client } from '../Rules';
import axios from 'axios';
import env from '../services/EnvService';

const useApi = () => {
    const baseApi = env.baseapi();
    // remove trailing slash
    return new Client(baseApi.replace(/\/$/g, ''), axios);
};

export default useApi;
