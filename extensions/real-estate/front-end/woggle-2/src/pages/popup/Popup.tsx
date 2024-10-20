import { useEffect, useState } from 'react';
import withErrorBoundary from '@src/shared/hoc/withErrorBoundary';

type Setting = {
  settingKey: string,
  settingValue: boolean
}

type OverrideSettings = {
  [settingKey: string]: boolean
}

type OverrideValue = "on" | "off" | "default";

function getSettings() {
  const s = localStorage.getItem("configCatSettings");
  if (s != null) {
    return JSON.parse(s);
  }
  else {
    return {
      overrideSettings: {}
    };
  }
}

function updateSettings(overrideSettings: Setting[]) {
  // Copy the whole `getSettings` function because it won't be available in the
  // base page's context.
  function getSettings() {
    const s = localStorage.getItem("configCatSettings");
    if (s != null) {
      return JSON.parse(s);
    }
    else {
      return {
        overrideSettings: {}
      };
    }
  }

  const s = getSettings();
  localStorage.setItem("configCatSettings", JSON.stringify({
    ...s,
    overrideSettings,
  }));
}


const Popup = () => {
  const [baseSettings, setBaseSettings] = useState<Setting[] | undefined>();
  const [overrideSettings, _setOverrideSettings] = useState<OverrideSettings | undefined>();
  const [filter, setFilter] = useState('');

  useEffect(() => {
    async function f() {
      chrome.tabs.query({active:true, currentWindow:true}, tabs => {
        chrome.scripting.executeScript({
          target: { tabId: tabs[0].id },
          function: getSettings
        } as any, async results => {
          const {baseSettings: initialBaseSettings, overrideSettings: initialOverrideSettings} = results[0].result as any;
          initialBaseSettings.sort((a: Setting, b: Setting) => a.settingKey.localeCompare(b.settingKey));
          setBaseSettings(initialBaseSettings);
          setOverrideSettings(initialOverrideSettings);
        });
      });
    };
    f();
  }, []);

  function setOverrideSettings(nextOverrideSettings: OverrideSettings) {
    _setOverrideSettings(nextOverrideSettings);
    chrome.tabs.query({active:true, currentWindow:true}, tabs => {
      chrome.scripting.executeScript({
        target: { tabId: tabs[0].id },
        function: updateSettings,
        args: [nextOverrideSettings],
      } as any, async results => {
      });
    });
  }

  function handleChange(settingKey: string, value: "on" | "off" | "default") {
    const nextOverrideSettings = {...overrideSettings};
    if (value === 'default') {
      delete nextOverrideSettings[settingKey];
    }
    else if (value === 'on') {
      nextOverrideSettings[settingKey] = true;
    }
    else if (value === 'off') {
      nextOverrideSettings[settingKey] = false;
    }
    setOverrideSettings(nextOverrideSettings);
  }

  function handleResetAllClick() {
    setOverrideSettings({});
  }

  return (
    <div style={{margin: "1em"}}>
      {baseSettings != null && (
        <>
          <div style={{display: "flex", marginBottom: "1em"}}>
            <div style={{flex: 1}}>
              Search: <input
                type="text"
                placeholder="Filter"
                value={filter}
                onChange={(event) => setFilter(event.target.value)}
              />
            </div>
            <div style={{flex: 0, textAlign: "right", minWidth: "6em"}}>
              <button onClick={handleResetAllClick}>
                Reset all
              </button>
            </div>
          </div>
          <table>
            <thead>
              <tr>
                <th>Setting</th>
                <th>Base value</th>
                <th>Your value</th>
              </tr>
            </thead>
            <tbody>
              {baseSettings.map((v) => {
                if (filter !== '' && !v.settingKey.toLowerCase().includes(filter.toLowerCase())) {
                  return null;
                }
                const value = overrideSettings[v.settingKey] ?? v.settingValue;
                const selectValue = !(v.settingKey in overrideSettings) ? 'default' : value ? 'on' : 'off';
                return <tr key={v.settingKey}>
                  <td>{v.settingKey}</td>
                  <td>{formatBool(v.settingValue)}</td>
                  <td>
                    <select
                      style={{width: '8em'}}
                      onChange={(event) => handleChange(v.settingKey, event.target.value as OverrideValue)}
                      value={selectValue}
                    >
                      <option value="default">Default</option>
                      <option value="on">Force on</option>
                      <option value="off">Force off</option>
                    </select>
                  </td>
                </tr>
              })}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
};

function formatBool(bool: boolean) {
  return bool ? '☑' : '☐';
}

export default withErrorBoundary(
  Popup,
  <div>Some error occurred</div>
);
