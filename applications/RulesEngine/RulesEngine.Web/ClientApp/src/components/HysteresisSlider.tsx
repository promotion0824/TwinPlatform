import * as React from 'react';
import { Slider, Tooltip } from '@mui/material';

interface HysteresisSliderProps {
    title: string;
    units: string;
    resolution: number;
    min: number;
    max: number;
    leftDb: number;
    rightDb: number;
    setMin: any;
    setMax: any;
    setLeftDb: any;
    setRightDb: any;
}

interface TooltipProps {
    children: React.ReactElement;
    open: boolean;
    value: number;
}

const HysteresisSlider: React.FC<HysteresisSliderProps> = ({ title, units, resolution, min, max, leftDb, rightDb, setMin, setLeftDb, setRightDb, setMax }) => {

    let values = [min, max, leftDb, rightDb];

    const updateValues = (e: any, newValue: any) => {
        let i = 0;
        if (!Number.isNaN(min)) {
            setMin(newValue[i]);
            i++;
        }
        if (!Number.isNaN(leftDb)) {
            setLeftDb(newValue[i]);
            i++;
        }
        if (!Number.isNaN(rightDb)) {
            setRightDb(newValue[i]);
            i++;
        }
        if (!Number.isNaN(max)) {
            setMax(newValue[i]);
            i++;
        }
    };

    const valuetext = (value: number): string => {
        return `${value}${units}`;
    };

    const marks = [
        {
            value: 0,
            label: valuetext(0),
        },
        {
            value: 10,
            label: valuetext(10),
        },
    ];

    const ValueLabelComponent = (props: TooltipProps) => {
        const { children, open, value } = props;

        return (
            <Tooltip open={open} enterTouchDelay={0} placement="top" title={value}>
                {children}
            </Tooltip>
        );
    }

    return (
        <div>
            <h3>{title}</h3>
            <br></br>
            <Slider
                value={values.filter((value) => !Number.isNaN(value))}
                min={0}
                max={10}
                step={resolution}
                marks={marks}
                valueLabelDisplay="on"
                aria-labelledby="range-slider"
                getAriaValueText={valuetext}
                onChange={updateValues}
                valueLabelFormat={(value: number, index: number) => {
                    const labels = ['Min', 'Left', 'Right', 'Max'];
                    return labels[index] + ': ' + value + units;
                }}
                // ValueLabelComponent={ValueLabelComponent}
            />
        </div>
    );
}

export default HysteresisSlider;
