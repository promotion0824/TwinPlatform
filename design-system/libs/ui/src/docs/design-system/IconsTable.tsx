import { Icon, IconName } from '../..'

export const IconsTable = ({ icons }: { icons: IconName[] }) => {
  return (
    <table>
      <thead>
        <tr>
          <th>Icon</th>
          <th>Code</th>
          <th>Icon Name</th>
        </tr>
      </thead>
      <tbody>
        {icons.map((iconName) => (
          <tr id={iconName}>
            <td
              /* this is required to fix bug https://dev.azure.com/willowdev/Unified/_workitems/edit/80328 */
              className="sb-unstyled"
            >
              <Icon icon={iconName} />
            </td>
            <td>
              <code
                /* this classname is same across storybook prod and storybook dev and localhost */
                className="css-1cuv9yu"
              >
                &lt;Icon icon="{iconName}" /&gt;
              </code>
            </td>
            <td>{iconName}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
