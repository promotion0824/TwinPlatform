import { Guid } from "./Guid";
import * as Yup from 'yup';

export const UserFieldNames = {
  id: { field: "id", label: "Id" },
  firstName: { field: "firstName", label: "First Name" },
  lastName: { field: "lastName", label: "Last Name" },
  email: { field: "email", label: "Email" },
  status: { field: "status", label: "Status" },
}

export type UserType = {
  id: string,
  firstName: string,
  lastName: string,
  email: string,
  emailConfirmed: boolean,
  status: 0 | 1,
  isAdmin: boolean
};

export class UserModel implements UserType {
  id: string = Guid.Empty.ToString();
  firstName: string = '';
  lastName: string = '';
  email: string = '';
  emailConfirmed: boolean = false;
  status: 0 | 1 = 0;
  isAdmin: boolean = false;

  static MapModel(apiRow: any): UserModel {
    let newRow = new UserModel();
    Object.assign(newRow, apiRow);
    return newRow;
  }

  static validationSchema = Yup.object().shape({
    firstName: Yup.string().required('First Name is required').max(100, 'First Name must be less than 100 characters in length'),
    lastName: Yup.string().required('Last Name is required').max(100, 'Last Name must be less than 150 characters in length'),
    email: Yup.string().required('Email is required').matches(/^\S+@\S+\.\S+$/, 'Email is invalid').max(100, 'Email must be less than 100 characters in length'),
  });

  static GetFullName(model: UserModel): string {
    return model.firstName + ' ' + model.lastName;
  }

  static GetAutocompleteLabel(model: UserModel): string {
    return model.firstName + ' ' + model.lastName + ' (' + model.email + ')';
  }
}
