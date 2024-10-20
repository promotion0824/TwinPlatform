import { Link } from 'react-router-dom'
import { GridColTypeDef } from '../../data-display/DataGrid'

type LinkColumnTypeProps = {
  /** The name of the property that should be used as the URL. */
  urlPropertyName: string
}

export const linkColumnType = ({
  urlPropertyName,
}: LinkColumnTypeProps): GridColTypeDef<string> => ({
  renderCell: ({ row, value }) => {
    const url = row[urlPropertyName]
    return value && url && <Link to={url}>{value}</Link>
  },
})
