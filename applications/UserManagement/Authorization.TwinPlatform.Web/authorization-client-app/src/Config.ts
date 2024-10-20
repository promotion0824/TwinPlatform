
//Copied logic from TLM
export const AppConstants = {
  extensionUri: 'authrz-web',
  correlationHeaderName: 'x-correlation-id'
}

const getBaseUrl = (extensionName: string) => {
  if (window.location.origin.includes('localhost') || !window.location.pathname.includes(AppConstants.extensionUri)) {
    return window.location.origin;
  }

  const url: string = window.location.href;
  const urlArray = url.split(extensionName);
  return urlArray[0] + extensionName;
};

export const endpoints = {
  authApi: process.env.NODE_ENV === 'development' ? 'https://localhost:7184/api' : getBaseUrl(AppConstants.extensionUri) + '/api',
  baseName: 'comes from config endpoint',
};
