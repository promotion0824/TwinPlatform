export default function Icon(
  props: HTMLElement<SVGElement> & {
    icon: string
    color?: 'blue' | 'red' | 'white'
    size?: 'small' | 'medium' | 'large'
    className?: string
  }
)
