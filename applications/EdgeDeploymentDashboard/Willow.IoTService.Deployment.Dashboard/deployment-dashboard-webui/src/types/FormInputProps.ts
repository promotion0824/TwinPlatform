export interface FormInputProps {
  name: string;
  control: any;
  setValue?: any;
  label: string;
  filterValue?: string;
  sx?: any;
  moduleVersionChanged?: any;
  setOpenError?: (open: boolean) => void;
}

export interface FormInputModuleProps extends FormInputProps {
  index?: number;
  setOpenError: (open: boolean) => void;
}

export interface FormInputApplicationTypeProps extends FormInputProps {
  isAnyAllow?: boolean;
  setOpenError: (open: boolean) => void;
}
