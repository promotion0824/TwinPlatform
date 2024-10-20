import { GetTlmClient } from '../axiosWithToken';
import { WillowContext } from '../types/WillowContext';

export interface IApplicationOptions {
  appInsights: IApplicationInsightsOptions;
  azureAppOptions: IAzureAppOptions;
  tlmAssemblyVersion: string;
  willowContext: WillowContext;
  mtiOptions: IMtiOptions;
}
interface IMtiOptions {
  enableSyncToMapped: boolean;
  isMappedDisabled: boolean;
}

export interface IApplicationInsightsOptions {
  instrumentationKey: string;
  connectionString: string;
}

export interface IAzureAppOptions {
  clientId: string;
  baseUrl: string;
  authority: string;
  knownAuthorities: string[];
  frontendB2CScopes: string[];
  backendB2CScopes: string[];
  tlmVersion: string;
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
    const response = await GetTlmClient().get<IApplicationOptions>('api/config/');
    return response.data;
  }
}

export const configService = new ConfigService();
