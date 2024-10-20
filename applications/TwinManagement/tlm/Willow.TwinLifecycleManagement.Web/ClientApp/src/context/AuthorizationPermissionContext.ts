import { createContext } from "react";
import { AuthorizationResponseDto } from "../services/Clients";

export type AuthorizationDataContext = { response: AuthorizationResponseDto, isLoading: boolean };

export class AuthDataContext implements AuthorizationDataContext {
  response = new AuthorizationResponseDto();
  isLoading = true;
};
export const AuthorizationPermissionContext = createContext(new AuthDataContext());

