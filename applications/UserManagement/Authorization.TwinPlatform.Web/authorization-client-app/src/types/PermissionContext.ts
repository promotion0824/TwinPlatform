import { createContext } from "react";
import { AuthorizationModel } from "./AuthorizationModel";

export const PermissionContext = createContext( new AuthorizationModel() );

