# Tabs

Shows tabs with content.

## Usage

```html
<Tabs>
  <Tab header="Header"> Content </Tab>
  <Tab header="Header 2"> Content 2 </Tab>
</Tabs>
```

```html
<div>
  <Tabs color="light|dark" type="normal|header">
    <Tab header="Header" selected="{false|true}" autoFocus="{false|true}" />
  </Tabs>
  <TabsContent color="light|dark"> Content </TabsContent>
</div>
```

```html
<Tabs>
  <Tab header="header" to="/test" root="/test" />
  <Tab header="header 2" to="/test/something" root="/test" />
</Tabs>
```
