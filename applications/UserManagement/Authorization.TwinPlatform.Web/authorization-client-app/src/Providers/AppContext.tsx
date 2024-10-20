import React, { useState } from "react";
import { IAppContext, MainContext } from "../types/Context";

export const AppContext = ({ children }: { children: React.ReactNode }) => {

    const [context, setContext] = useState({showLoader:false,actionName:''} as IAppContext)

    return (

        <MainContext.Provider value={[context,setContext]}>
            {children}
        </MainContext.Provider>
    );

}