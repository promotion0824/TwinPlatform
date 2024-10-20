import { Stack } from '@mui/material';
import { FieldValues, UseFormRegister } from "react-hook-form";
import { RuleDto } from '../../Rules';
import Recommendationsfield from '../fields/Recommendationsfield';
import CategoryField from '../fields/CategoryField';
import TitleField from '../fields/TitleField';
import ModelPickerField from '../fields/ModelPickerField';
import DescriptionEditorField from '../fields/DescriptionEditorField';
import TagsEditor from '../TagsEditor';
import useApi from '../../hooks/useApi';

// See https://medium.com/terria/typescript-transforming-optional-properties-to-required-properties-that-may-be-undefined-7482cb4e1585
type Complete<T> = { [P in keyof Required<T>]: Pick<T, P> extends Required<Pick<T, P>> ? T[P] : (T[P] | undefined); }

const RuleDetails = (params: {
  rule: RuleDto,
  register: UseFormRegister<FieldValues>,
  errors: { [x: string]: any; }
}) => {

  const rule = params.rule as Complete<RuleDto>;
  const { register, errors } = params;

  const apiclient = useApi();

  const tagsEditorProps = {
    id: "r_Tags",
    key: "r_TagsKey",
    queryKey: "r_TagsQuery",
    defaultValue: rule.tags,
    queryFn: async (_: any): Promise<string[]> => {
      try {
        const tags = await apiclient.ruleTags();
        return tags;
      } catch (error) {
        return [];
      }
    },
    valueChanged: (newValue: string[]) => { rule.tags = newValue; }
  };

  return (
    <Stack spacing={2}>
      <TitleField rule={rule} register={register} errors={errors} />
      <DescriptionEditorField rule={rule} register={register} errors={errors} />
      {!rule.isCalculatedPoint &&
        <>
          <CategoryField rule={rule} register={register} errors={errors} />
          <Recommendationsfield rule={rule} register={register} errors={errors} />
        </>
      }
      <ModelPickerField rule={rule} register={register} errors={errors} />
      <TagsEditor {...tagsEditorProps} />
    </Stack>
  )
}

export default RuleDetails;
