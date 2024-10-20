---
'@willowinc/theme': minor
'@willowinc/ui': minor
---

Breaking change: upgrade styled-components from 5 to 6

- You might need to install 'babel-plugin-styled-components' if not already installed;
- You might need to uninstall '@types/styled-components' if already installed;
- You need to update styles that are using pseudo selectors like ':hover'.
  See this doc for details https://willow.atlassian.net/wiki/x/BYCSow
