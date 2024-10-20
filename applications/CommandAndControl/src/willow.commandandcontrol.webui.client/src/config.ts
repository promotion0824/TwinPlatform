const extensionUri = "activecontrol";

const baseUrl = (extensionName: string) => {
  if (
    window.location.origin.includes("localhost") ||
    !window.location.pathname.includes(extensionName)
  ) {
    return window.location.origin;
  }

  const urlArray = window.location.href.split(extensionName);
  return urlArray[0] + extensionName;
};

export const endpoint =
  process.env.NODE_ENV === "development"
    ? "https://localhost:7294"
    : baseUrl(extensionUri);
