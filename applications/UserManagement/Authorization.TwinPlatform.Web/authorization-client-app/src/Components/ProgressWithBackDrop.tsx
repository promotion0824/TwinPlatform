import * as React from 'react';
import Backdrop from '@mui/material/Backdrop';
import CircularProgress from '@mui/material/CircularProgress';
import { useContext } from 'react';
import { MainContext } from '../types/Context';
import { Box } from '@mui/material';

export const ProgressWithBackDrop = () => {

    const [context] = useContext(MainContext);

    return (
        <div>
            <Backdrop
                sx={{ color: '#fff', zIndex: (theme) => theme.zIndex.drawer + 1 }}
                open={context.showLoader}
            >
                <CircularProgress color="inherit" />
                <Box margin={2}>
                    {context.actionName}
                </Box>
            </Backdrop>
        </div>
    );
}

