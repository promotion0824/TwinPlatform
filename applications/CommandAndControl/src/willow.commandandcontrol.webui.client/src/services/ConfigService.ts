import { getAxiosClient } from "./AxiosClient";

export interface IApplicationOptions {
  azureAppOptions: IAzureAppOptions;
}

export interface IAzureAppOptions {
  clientId: string;
  baseUrl: string;
  authority: string;
  knownAuthorities: string[];
  b2CScopes: string[];
  version: string;
}

class ConfigService {
  private _config!: IApplicationOptions;

  public get config(): IApplicationOptions {
    return this._config;
  }

  async init() {
    this._config = await this.getConfig();
    return this._config;
  }

  private async getConfig(): Promise<IApplicationOptions> {
    const response = await getAxiosClient().get<IApplicationOptions>(
      "api/config/"
    );
    return response.data;
  }
}

export const configService = new ConfigService();
