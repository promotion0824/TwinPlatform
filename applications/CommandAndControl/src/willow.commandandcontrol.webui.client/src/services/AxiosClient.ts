import axios, { AxiosInstance } from "axios";
import { endpoint } from "../config";

let _client: AxiosInstance | undefined = undefined;

const getAxiosClient = (contentType = "application/json") => {
  if (!_client) {
    _client = axios.create({
      baseURL: endpoint,
      headers: {
        "Content-Type": contentType,
        Authorization: localStorage.getItem("token") ?? "",
        "Access-Control-Allow-Origin": "*",
      },
    });
  } else {
    _client.defaults.headers.common["Content-Type"] = contentType;
    _client.defaults.headers.common["Authorization"] =
      localStorage.getItem("token") ?? "";
  }
  return _client;
};
export { getAxiosClient };
