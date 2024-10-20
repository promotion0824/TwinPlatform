import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router';
import { configService } from './services/ConfigService';
import { ApplicationInsights, IConfiguration } from '@microsoft/applicationinsights-web';
import { ReactPlugin } from '@microsoft/applicationinsights-react-js';
import { createBrowserHistory } from 'history';

import { Layout } from './components/Layout';

// route-based code splitting to reduce bundle sizes and load faster
const HomePage = lazy(() => import('./pages/Home'));
const NoPageFound = lazy(() => import('./pages/NoPageFound'));
const ImportModelsPage = lazy(() => import('./pages/Models/ImportModelsPage'));
const ImportTwinsPage = lazy(() => import('./pages/ImportTwinsPage'));
const DeleteSiteIdTwinsPage = lazy(() => import('./pages/DeleteSiteIdTwinsPage'));
const DeleteAllModelsPage = lazy(() => import('./pages/Models/DeleteAllModelsPage'));
const ErrorPage = lazy(() => import('./pages/ErrorPage'));
const DeleteAllTwinsPage = lazy(() => import('./pages/DeleteAllTwinsPage'));
const DeleteFileTwinsPage = lazy(() => import('./pages/DeleteFileTwinsPage'));
const ExportSiteIdTwinsPage = lazy(() => import('./pages/ExportTwinsPage'));
const ModelsPage = lazy(() => import('./pages/Models/ModelsPage'));
const DQUploadRulePage = lazy(() => import('./pages/DataQuality/ManageRules/DQUploadRulesPage'));
const DataQualityResultsPage = lazy(() => import('./pages/DataQuality/Results/DQResults'));
const DQTriggerScanPage = lazy(() => import('./pages/DataQuality/NewScan/DQTriggerScan'));
const TwinsPage = lazy(() => import('./pages/Twins/TwinsPage'));
const DQRuleManagmentPage = lazy(() => import('./pages/DataQuality/ManageRules/DQRuleManagmentPage'));
const DocumentsUploadPage = lazy(() => import('./pages/Documents/DocumentsUploadPage'));
const DocumentsPage = lazy(() => import('./pages/Documents/DocumentsPage'));
const DQJobsPage = lazy(() => import('./pages/Jobs/DQJobsPage'));
const AboutPage = lazy(() => import('./pages/About'));
const MappingsPage = lazy(() => import('./pages/ReviewTwins/ReviewTwinsPage'));
const ImportTimeSeriesPage = lazy(() => import('./pages/TimeSeries/ImportTimeSeriesPage'));
const TimeSeriesJobResultsPage = lazy(() => import('./pages/TimeSeries/TimeSeriesJobResults'));
const DocumentsSearchPage = lazy(() => import('./pages/Documents/DocumentsSearch'));
const CopilotChatPage = lazy(() => import('./pages/Copilot/CopilotChat'));
const MtiJobsPage = lazy(() => import('./pages/Jobs/MtiJobsPage'));
const UnifiedJobsPage = lazy(() => import('./pages/UnifiedJobs/UnifiedJobsPage'));

const App = () => {
  const browserHistory = createBrowserHistory();
  let reactPlugin = new ReactPlugin();

  let configuration: IConfiguration = {
    connectionString: configService.config.appInsights.connectionString,
    extensions: [reactPlugin],
    extensionConfig: {
      [reactPlugin.identifier]: { history: browserHistory },
    },
  };

  let appInsights = new ApplicationInsights({
    config: configuration,
  });
  appInsights.loadAppInsights();
  appInsights.trackPageView();

  return (
    <Layout>
      <Suspense fallback={<div>Loading...</div>}>
        <Routes>
          <Route index element={<HomePage />} />

          <Route path="models" element={<ModelsPage />} />
          <Route path="import-models" element={<ImportModelsPage />} />
          <Route path="delete-all-models" element={<DeleteAllModelsPage />} />

          <Route path="twins" element={<TwinsPage />} />

          {/* Approve&Accept is only available if the isMappedDisabled feature flag is false, This flag is set in TLM's appsetting */}
          {!configService.config.mtiOptions.isMappedDisabled && (
            <Route path="review-twins" element={<MappingsPage />} />
          )}

          <Route path="export-twins" element={<ExportSiteIdTwinsPage />} />
          <Route path="import-twins" element={<ImportTwinsPage />} />
          <Route path="delete-all-twins" element={<DeleteAllTwinsPage />} />
          <Route path="delete-siteId-twins" element={<DeleteSiteIdTwinsPage />} />
          <Route path="delete-file-twins" element={<DeleteFileTwinsPage />} />

          <Route path="copilot" element={<CopilotChatPage />} />
          <Route path="upload-documents" element={<DocumentsUploadPage />} />
          <Route path="documents" element={<DocumentsPage />} />
          <Route path="documents/search" element={<DocumentsSearchPage />} />

          <Route path="import-time-series" element={<ImportTimeSeriesPage />} />

          <Route path="data-quality/results" element={<DataQualityResultsPage />} />
          <Route path="data-quality/rules" element={<DQRuleManagmentPage />} />
          <Route path="data-quality/upload-rules" element={<DQUploadRulePage />} />

          <Route path="data-quality/new-scan" element={<DQTriggerScanPage />} />

          <Route path="jobs/data-quality" element={<DQJobsPage />} />
          <Route path="jobs/time-series/:jobId" element={<TimeSeriesJobResultsPage />} />
          <Route path="jobs/mti" element={<MtiJobsPage />} />

          <Route path="about" element={<AboutPage />} />

          <Route path="jobs" element={<UnifiedJobsPage />} />
          <Route path="jobs/:jobId/details" element={<UnifiedJobsPage />} />

          <Route path="/error" element={<ErrorPage />} />
          <Route path="*" element={<NoPageFound />} />
        </Routes>
      </Suspense>
    </Layout>
  );
};

export default App;
