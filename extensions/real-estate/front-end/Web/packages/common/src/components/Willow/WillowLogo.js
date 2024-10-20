import { Link } from '@willow/ui'
import WillowLogoWhite from './WillowLogo_White.svg'

export default function WillowLogo(props) {
  return (
    <Link to="/" {...props}>
      <WillowLogoWhite
        css={{
          width: 120,
          height: 32,
        }}
      />
    </Link>
  )
}
