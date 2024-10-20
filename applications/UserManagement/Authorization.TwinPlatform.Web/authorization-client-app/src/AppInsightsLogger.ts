import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { ReactPlugin } from '@microsoft/applicationinsights-react-js';
import { createBrowserHistory } from "history";
import { AppInsightConfigModel } from './types/ConfigModel';

export const InitializeAppInsights = (appInsightSetting: AppInsightConfigModel) => {
  const browserHistory = createBrowserHistory();
  let reactPlugin = new ReactPlugin();
  let appInsights = new ApplicationInsights({
    config: {
      connectionString: appInsightSetting.connectionString,
      extensions: [reactPlugin],
      extensionConfig: {
        [reactPlugin.identifier]: { history: browserHistory }
      }
    }
  });
  appInsights.loadAppInsights();
  appInsights.trackPageView();
}
