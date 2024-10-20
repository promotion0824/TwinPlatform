import { Avatar } from '@willowinc/ui';
import { Colors } from '@willowinc/ui/src/lib/common';
import { CustomTooltip } from './CustomTooltip';

function stringToColor(string: string) {

  // There is no convenient way to convert a string union to an array.
  // This should probably be moved to the design system.
  const colors: Colors[] = ['gray', 'red', 'orange', 'yellow', 'teal', 'green', 'cyan', 'blue', 'purple', 'pink'];

  const index = string.length % colors.length;

  return colors[index];
  /* eslint-enable no-bitwise */
}

function stringAvatar(name: string, color?: Colors) {
  return {
    color: color ?? stringToColor(name),
    children: `${name.split(' ')[0][0]}${name.split(' ').length > 1 ? name.split(' ')[1][0] : ''}`.toLocaleUpperCase(),
  };
}



const CustomAvatar = ({ name, color }: { name: string, color?: Colors }) => {

  return (
    name !== undefined ? <CustomTooltip title={name}>
      <Avatar {...stringAvatar(name, color)} size="lg" style={{ cursor: "pointer" }} />
    </CustomTooltip>
      :
    <></>
  );
}

export default CustomAvatar;
