# useLayoutEffectOnUpdate.md

Runs `useLayoutEffect` on updates, not on load.

#### Usage:

```js
useLayoutEffectOnUpdate(() => {
  console.log('called when "name" updates, not on load')
}, [name])
```
