import React from "react";
import {
  Controller,
  type Control,
  type FieldPath,
  type FieldValues,
  type UseControllerProps
} from "react-hook-form";

export const AxInput = <
  TFieldValues extends FieldValues = FieldValues,
  TName extends
    FieldPath<TFieldValues> = FieldPath<TFieldValues>
>({
  control,
  name,
  type,
  label,
  rules
}: {
  control: Control<TFieldValues, any, TFieldValues>;
  name: TName;
  label: string;
  rules?: UseControllerProps<
    TFieldValues,
    TName,
    TFieldValues
  >["rules"];
  type?: React.HTMLInputTypeAttribute;
}) => {
  return (
    <div className="grid grid-cols-1 items-center gap-2">
      <label>
        {label} {rules?.required && <sup>*</sup>}
      </label>

      <Controller
        key={"username"}
        name={name}
        control={control}
        rules={rules}
        render={({ field, formState: { errors } }) => (
          <div
            className={`
              flex flex-col items-start justify-center
            `}
          >
            <input
              className={`
                w-full rounded-md px-2 text-lg outline-1
                outline-pink-300
              `}
              type={type}
              {...field}
              aria-invalid={errors[name] ? "true" : "false"}
            />
            {errors[name] && (
              <sub
                className={`
                  z-10 w-max max-w-full break-after-all
                  leading-4 text-wrap wrap-normal
                  text-red-700
                `}
              >
                <>{errors[name].message}</>
              </sub>
            )}
          </div>
        )}
      ></Controller>
    </div>
  );
};

export default AxInput;
