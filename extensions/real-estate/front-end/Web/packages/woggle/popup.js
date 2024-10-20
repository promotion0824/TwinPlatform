function load() {
  const message = document.getElementById('message');

  document.addEventListener('click', (e) => {
    const name = e.target.getAttribute('data-name');
    const type = e.target.getAttribute('data-type');

    let value = null;
    if (e.target.checked != null) {
      if (e.target.checked) {
        value = true;
      } else if (type === 'api') {
        value = false;
      }
    }

    if (name != null) {
      chrome.tabs.executeScript(null, {
        code: `window.cookieToSet = ${JSON.stringify({ name, value })}`,
      }, () => {
        chrome.tabs.executeScript(null, {
          file: 'setCookie.js',
        }, () => {
          window.setTimeout(() => {
            window.location.reload();
          }, 1000);
        });
      });
    }
  });

  chrome.runtime.onMessage.addListener((request) => {
    if (request.action === 'getToggles') {
      const html = request.toggles.map((toggle) => (
        `<div>
          <input type="checkbox" ${toggle.enabled ? 'checked' : ''} data-type=${toggle.type} data-name=${toggle.name} />
          <span>${toggle.name}</span>
          ${toggle.hasCookie
            ? `<button type="text" data-name=${toggle.name}>Clear</button>`
            : ''}
        </div>`
      )).join('');

      message.innerHTML = html;
    }
  });

  chrome.tabs.executeScript(null, {
    file: 'getToggles.js',
  });
}

window.onload = function onWindowLoad() {
  load();
};
