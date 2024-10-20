import { Alert, Autocomplete, FormControl, Stack, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { FieldValues, UseFormRegister } from "react-hook-form";
import { useQuery } from "react-query";
import useApi from "../../hooks/useApi";
import { ModelSimpleDto, ModelSimpleGraphDto, RuleDto } from "../../Rules"

const ModelPickerfield = (params: {
  rule: RuleDto,
  register?: UseFormRegister<FieldValues>,
  errors?: { [x: string]: any; },
  isRequired?: boolean,
  primaryModelIdChanged?: (id: string | undefined) => void;
}) => {
  const { register, errors, primaryModelIdChanged } = params;
  const isRequired = params.isRequired ?? true;
  const ruleDto = params.rule;
  const apiclient = useApi();
  const [isCalculatedPoint, _] = useState(ruleDto.isCalculatedPoint);
  const [primaryModelId, setPrimaryModelId] = useState(ruleDto.primaryModelId);
  const [relatedModelId, setRelatedModelId] = useState(ruleDto.relatedModelId);

  const [outputModels, setoutputModels] = useState<ModelSimpleDto[]>();
  const [equipmentSpaceModels, setEquipmentSpaceModels] = useState<ModelSimpleDto[]>();

  const modelsQuery = useQuery(['modelsautocomplete'], async () => {
    try {
      const models = await apiclient.modelsAutocomplete();
      return models;
    }
    catch
    {
      let result = new ModelSimpleGraphDto();
      result.init({ nodes: [], relationships: [] });
      return result;
    }
  });

  useEffect(() => {
    const allModelsSorted = modelsQuery?.data?.nodes?.sort((a, b) => ((a.label === b.label) ? 0 : (a.label! > b.label! ? 1 : -1))) ?? [];

    if (isCalculatedPoint) {
      setEquipmentSpaceModels(allModelsSorted.filter(m => !m.isCapability) ?? []);
      setoutputModels(allModelsSorted.filter(m => m.isCapability) ?? []);
    } else {
      setoutputModels(allModelsSorted ?? []);
    }
  }, [modelsQuery.data]);

  //Handle Change Events (Reason for value checks)
  //Distinguishes between initialization and user interaction.
  //When fields are initially empty, it prioritizes initializing ruleDto.
  //When fields are not empty, it prioritizes user interaction.
  //Way to keep the component state and ruleDto synchronized
  const handlePrimaryModelIdChange = (_: any, value: ModelSimpleDto | null) => {
    if (!ruleDto.primaryModelId) {
      ruleDto.primaryModelId = value?.modelId;
    }
    else {
      setPrimaryModelId(value?.modelId);
    }

    if (primaryModelIdChanged) {
      primaryModelIdChanged(value?.modelId)
    }
  };

  const handleRelatedModelIdChange = (_: any, value: ModelSimpleDto | null) => {
    if (!ruleDto.relatedModelId) {
      ruleDto.relatedModelId = value?.modelId;
    }
    else {
      setRelatedModelId(value?.modelId);
    }
  };

  useEffect(() => {
    ruleDto.primaryModelId = primaryModelId;
    ruleDto.relatedModelId = relatedModelId;
  }, [primaryModelId, relatedModelId])

  useEffect(() => {
    setPrimaryModelId(ruleDto.primaryModelId);
    setRelatedModelId(ruleDto.relatedModelId);
  }, [ruleDto])

  if (modelsQuery.isFetched && (modelsQuery.data?.nodes?.length ?? 0) == 0 && !ruleDto.id) {
    return (
      <Stack spacing={2}>
        <TextField
          {...params}
          defaultValue={"Failed to load equipment..."}
          fullWidth
          autoComplete='off'
          error={(errors !== undefined) ? !!errors.primaryModelId : false}
          placeholder="Failed to load equipment..."
          label="Select equipment*"
          disabled={true}
          InputProps={{
            readOnly: true,
          }}
        />
        <Alert sx={{ width: '100%' }} variant="filled" severity={"error"}>
          Failed to load models. Please run ADT cache to select equipment.
        </Alert>
      </Stack>)
  }

  if (modelsQuery.isFetched) {
    return (
      <Stack spacing={2}>
        {ruleDto.isCalculatedPoint &&
          <FormControl key={"relatedModelId"} fullWidth>
            <Autocomplete
              disablePortal
              key={relatedModelId}
              options={equipmentSpaceModels ?? []}
              isOptionEqualToValue={(a: any, b: any) => (a.id == b.id)}
              getOptionLabel={e => `${e.label} (${e.modelId?.replace(/^dtmi:com:|;1$/g, '')})` ?? e.id ?? "Missing label"}
              sx={{ flexShrink: 0 }}
              value={modelsQuery.data?.nodes?.find(x => x.modelId == relatedModelId)}
              renderInput={params => (
                <TextField
                  {...params}
                  id={"relatedModelId"}
                  {...(register !== undefined ? register("relatedModelId", { required: isRequired, value: ruleDto.relatedModelId }) : {})}
                  fullWidth
                  autoComplete='off'
                  error={(errors !== undefined) ? !!errors.relatedModelId : false}
                  label="Select equipment or space*"
                  placeholder="Select equipment or space"
                />
              )}
              onChange={handleRelatedModelIdChange}
            />
          </FormControl>}
        <FormControl key={"primaryModelId"} fullWidth>
          <Autocomplete
            disablePortal
            key={primaryModelId}
            options={outputModels ?? []}
            isOptionEqualToValue={(a: any, b: any) => (a.id == b.id)}
            getOptionLabel={e => `${e.label} (${e.modelId?.replace(/^dtmi:com:|;1$/g, '')})` ?? e.id ?? "Missing label"}
            sx={{ flexShrink: 0 }}
            value={modelsQuery.data?.nodes?.find(x => x.modelId == primaryModelId)}
            renderInput={params => (
              <TextField
                {...params}
                id={"primaryModelId"}
                {...(register !== undefined ? register("primaryModelId", { required: isRequired, value: ruleDto.primaryModelId }) : {})}
                fullWidth
                autoComplete='off'
                error={(errors !== undefined) ? !!errors.primaryModelId : false}
                label={ruleDto.isCalculatedPoint ? "Select output model*" : "Select primary model*"}
                placeholder={ruleDto.isCalculatedPoint ? "Select output model" : "Select primary model"}
              />
            )}
            onChange={handlePrimaryModelIdChange}
          />
        </FormControl>
      </Stack>
    )
  }
  else {
    return (<div>Loading...</div>)
  }
};

export default ModelPickerfield;
