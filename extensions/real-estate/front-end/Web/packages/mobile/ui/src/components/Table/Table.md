# Table

Shows a styled table.

## Usage

```html
<table>
  <head>
    <Row>
      <Cell>Column 1</Cell>
      <Cell>Column 2</Cell>
    </Row>
  </head>
  <body>
    {(items) => items.map((item) => (
    <Row>
      <Cell>{item.column1}</Cell>
      <Cell>{item.column2}</Cell>
    </Row>
    ))}
  </body>
</table>
```
