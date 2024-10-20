const extensionUri = 'tlm-web';

const baseUrl = (extensionName) => {
  if (window.location.origin.includes('localhost') || !window.location.pathname.includes(extensionName)) {
    return window.location.origin;
  }

  const urlArray = window.location.href.split(extensionName);
  return urlArray[0] + extensionName;
};

export const modelsImport = {
  folderPath: [
        { name: 'Real Estate', value: 'Building' },
        { name: 'Airport', value: 'Airport' },
  ],
  branchRefs: ['Latest', 'Custom'],
};

export const endpoints = {
  tlmApi: process.env.NODE_ENV === 'development' ? 'https://localhost:7071' : (baseUrl(extensionUri)),
  userGuideLink: 'https://willow.atlassian.net/wiki/spaces/DOCS/pages/2173010136/Twin+Lifecycle+Management+User+Guide',
  supportLink: 'mailto:support@willowinc.com'
};
