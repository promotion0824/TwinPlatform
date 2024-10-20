import { AvatarGroup } from '@mui/material';
import CustomAvatar from './CustomAvatar';

interface ICustomAvatarProps<T> {
    data: T[],
    getName: (record: T) => string
}

export const CustomAvatarGroup = <T,>({ data, getName }: ICustomAvatarProps<T>): JSX.Element => {
    const maxAvatarToDisplay: number = 5;

    return (
        <AvatarGroup max={maxAvatarToDisplay} total={data.length}>
        {data.map((val, index) => (
          <CustomAvatar key={"avatar_"+index} name={getName(val)}></CustomAvatar>
            ))}
        </AvatarGroup>
    );
}
