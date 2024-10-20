import * as React from 'react';

export interface IAppContext {
    showLoader: boolean;
    actionName: string;
}


export const MainContext = React.createContext([{ showLoader: false, actionName: '' }, {}] as [IAppContext, any]);