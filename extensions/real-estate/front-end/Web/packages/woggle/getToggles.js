(async () => {
  const cookies = Object.fromEntries(
    document.cookie
      .split(';')
      .map((cookie) => cookie.trim().split('='))
      .filter((entry) => entry[0].startsWith('wp-'))
      .map((entry) => [entry[0], entry[1] === 'true' ? true : false])
  );

  async function getFrontendToggles() {
    const js = document.head.innerHTML.match(/<script.*?src=\"(\/public\/main.*?\.js)\"><\/script>/)?.[1];
    const uiJs = document.head.innerHTML.match(/<script.*?src=\"(\/public\/ui.*?\.js)\"><\/script>/)?.[1];

    if (js == null || uiJs == null) {
      chrome.runtime.sendMessage({
        action: 'getToggles',
        toggles: [],
      });

      return;
    }

    const file = `${window.location.origin}${js}`;
    const uiFile = `${window.location.origin}${uiJs}`;

    const response = await window.fetch(file);
    const uiResponse = await window.fetch(uiFile);

    const text = await response.text();
    const uiText = await uiResponse.text();

    const toggleNames = [...new Set([
      ...(text.match(/wp-[^ $]+?(\-enabled|\-disabled)/g) ?? []),
      ...(uiText.match(/wp-[^ $]+?(\-enabled|\-disabled)/g) ?? []),
    ])].sort();

    return toggleNames.map((name) => ({
      type: 'frontend',
      name,
      enabled: cookies[name] ?? false,
      hasCookie: cookies[name] != null,
    }));
  }

  async function getApiToggles() {
    try {
      const api = document.cookie
        .split(';')
        .map((cookie) => cookie.trim().split('='))
        .find((entry) => entry[0] === 'api')?.[1];

      if (api == null) {
        return [];
      }

      const response = await window.fetch(`/${api}/api/me`);
      if (response.status < 200 || response.status >= 300) {
        return [];
      }
      const json = await response.json();

      return Object.entries(json.customer.features).map(([key, value]) => {
        const name = `wp-${key}`;

        return {
          type: 'api',
          name,
          enabled: cookies[name] ?? value,
          hasCookie: cookies[name] != null,
        };
      });
    } catch (err) {
      console.error(`${err}`);

      return [];
    }
  }

  const frontendToggles = await getFrontendToggles();
  const apiToggles = await getApiToggles();

  chrome.runtime.sendMessage({
    action: 'getToggles',
    toggles: [...apiToggles, ...frontendToggles],
  });
})();
