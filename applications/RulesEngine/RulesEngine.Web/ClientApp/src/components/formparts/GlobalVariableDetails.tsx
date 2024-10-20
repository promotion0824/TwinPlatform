import { Grid } from '@mui/material';
import { FieldValues, UseFormRegister, UseFormSetValue } from 'react-hook-form';
import { GlobalVariableDto } from '../../Rules';
import DescriptionField from '../fields/DescriptionField';
import TitleField from '../fields/TitleField';
import { GetGlobalVariableTypeText } from '../GlobalVariableTypeFormatter';
import TagsEditor from '../../components/TagsEditor';
import useApi from '../../hooks/useApi';

// See https://medium.com/terria/typescript-transforming-optional-properties-to-required-properties-that-may-be-undefined-7482cb4e1585
type Complete<T> = { [P in keyof Required<T>]: Pick<T, P> extends Required<Pick<T, P>> ? T[P] : (T[P] | undefined); }

const GlobalVariableDetails = (params: {
  global: GlobalVariableDto,
  register: UseFormRegister<FieldValues>,
  setValue: UseFormSetValue<FieldValues>,
  errors: { [x: string]: any; },
  validate: (global: GlobalVariableDto) => void
}) => {

  const global = params.global as Complete<GlobalVariableDto>;
  const { register, errors, validate } = params;
  const apiclient = useApi();

  const tagsEditorProps = {
    id: "gv_Tags",
    key: "gv_TagsKey",
    queryKey: "gv_TagsQuery",
    defaultValue: global.tags,
    queryFn: async (_: any): Promise<string[]> => {
      try {
        const tags = await apiclient.globalVariableTags();
        return tags;
      } catch (error) {
        return [];
      }
    },
    valueChanged: (newValue: string[]) => { global.tags = newValue; }
  };

  return (
    <Grid container spacing={2} sx={{ mt: 1, maxWidth: '120ch' }} >
      <Grid item xs={12} sm={8}>
        <TitleField rule={global} valueChangedEvent={() => validate(global)} register={register} errors={errors} label={`${GetGlobalVariableTypeText(global)} Name`} placeholder={`Enter a name for the ${GetGlobalVariableTypeText(global)}`} />
      </Grid>
      <Grid item xs={12} sm={8}>
        <DescriptionField rule={global} register={register} required={true} errors={errors} label={`Description`} placeholder={`Enter a description for the ${GetGlobalVariableTypeText(global)}`} />
      </Grid>
      <Grid item xs={12} sm={8}>
        <TagsEditor {...tagsEditorProps} />
      </Grid>
    </Grid>
  )
}

export default GlobalVariableDetails;
