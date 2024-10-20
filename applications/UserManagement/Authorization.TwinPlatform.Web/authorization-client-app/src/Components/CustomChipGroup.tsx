  import { Chip, Stack } from '@mui/material';
import * as React from 'react';
import { ReactElement } from 'react';
import { CustomTooltip } from './CustomTooltip';

interface ICustomChipGroupProps<T> {
    data: T[],
    getName: (record: T) => string,
    icon: ReactElement
}

export const CustomChipGroup = <T,>({ data, getName, icon }: ICustomChipGroupProps<T>): JSX.Element => {
    const maxChipsToDisplay: number = 2;

    let additionalInfo: ReactElement = <></>;
    if ((data.length - maxChipsToDisplay) > 0) {
        additionalInfo = <Chip icon={icon} label={'+' + (data.length - maxChipsToDisplay) + ' more'} variant="outlined" style={{fontSize: "inherit"}} />
    }

    return (
        <Stack direction="row" spacing={1}>
        {data.slice(0, maxChipsToDisplay).map((val, index) => (
          <CustomTooltip title={getName(val)}>
            <Chip key={"chip_" + index} icon={icon} label={getName(val)} variant="outlined" style={{ fontSize: "inherit" }} />
          </CustomTooltip>
            ))}
            {additionalInfo}
        </Stack>

    );
}
