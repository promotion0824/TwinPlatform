import { Autocomplete, CircularProgress, TextField } from "@mui/material";
import { Fragment, useState } from "react";
import { Controller, Control, FieldValues, FieldErrorsImpl } from "react-hook-form";

export default function FormAutoComplete<T extends FieldValues, U>({ control, fieldName, label, errors, options, OnUpdateInput, getOptionLabel, isOptionEqToValue, groupBy }:
    {
        control: Control<T, any>,
        fieldName: any, errors: Partial<FieldErrorsImpl<T>>,
        label: string
        options: U[],
        OnUpdateInput: (searchKey: string) => {},
        getOptionLabel: (option: U) => string
        isOptionEqToValue: (option: U, value: U) => boolean,
        groupBy?:((option:U)=>string)|undefined
    }) {

    const [open, setOpen] = useState(false);
    const loading = open && options.length > 1;

    let requestTimer: NodeJS.Timeout | null = null;
    const requestTimeout = 500;//ms
    const throttleRequest = (key: string) => {
        if (requestTimer != null)
            clearTimeout(requestTimer);
        requestTimer = setTimeout(() => { OnUpdateInput(key) }, requestTimeout);
    };

    return (
        <Controller
            control={control}
            name={fieldName}
            render={({ field: { onChange, value } }) => (
                <Autocomplete
                    onChange={(event, item) => {
                        onChange(item);
                    }}
                    value={value}
                    isOptionEqualToValue={isOptionEqToValue}
                    options={options}
                    filterOptions={(x) => x}
                    onInputChange={(event, val, reason) => {
                        throttleRequest(val);
                    }}
                    getOptionLabel={getOptionLabel}
                    open={open}
                    loading={(loading)}
                    onOpen={() => {
                        setOpen(true);
                        throttleRequest('');
                    }}
                    onClose={() => {
                        setOpen(false);
                    }}
                    groupBy={groupBy}
                    renderInput={(params) => (
                        <TextField
                            {...params}
                            label={label}
                            margin="normal"
                            variant="outlined"
                            error={!!errors[fieldName]}
                            helperText={errors[fieldName] && (errors[fieldName]?.message) as string}
                            placeholder="Type to search"
                            InputProps={{
                                ...params.InputProps,
                                endAdornment: (
                                    <Fragment>
                                        {loading ? <CircularProgress color="inherit" size={20} /> : null}
                                        {params.InputProps.endAdornment}
                                    </Fragment>
                                ),
                            }}
                        />
                    )}
                />
            )}
        />
    );
}
