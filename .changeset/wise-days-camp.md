---
'@willowinc/ui': major
---

BREAKING CHANGES:

1. Does not support customized Select option.
2. Popover dropdown has its own styled scrollbar which is different to the global scrollbar.
3. Design for Loader has changed
4. Popover content is now using `box-sizing: border-box``, which means customized width will include border and margin size, etc.
5. `data` structure and type for Select options has changed from:

```
{
  group?: string,
  label: string,
  value: string,
  disabled?: boolean;
}[]
```

to

```
{
  group: string;
  items: ({
    value: string;
    label: string;
    disabled?: boolean;
  } | string)[];
}[]
```
