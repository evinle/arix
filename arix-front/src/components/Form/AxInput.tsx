import React from "react";
import {
  Controller,
  type Control,
  type FieldPath,
  type FieldValues,
  type UseControllerProps
} from "react-hook-form";

const className = {
  input:
    "rounded-md text-lg outline-1 outline-pink-300 px-2",
  textfieldContainer:
    "flex justify-start items-center gap-4"
};

export const AxInput = <
  TFieldValues extends FieldValues = FieldValues,
  TName extends
    FieldPath<TFieldValues> = FieldPath<TFieldValues>
>({
  control,
  name,
  type,
  rules
}: {
  control: Control<TFieldValues, any, TFieldValues>;
  name: TName;
  rules?: UseControllerProps<
    TFieldValues,
    TName,
    TFieldValues
  >["rules"];
  type?: React.HTMLInputTypeAttribute;
}) => {
  return (
    <div className={className.textfieldContainer}>
      <label>
        Username <sup>*</sup>
      </label>
      <Controller
        key={"username"}
        name={name}
        control={control}
        rules={rules}
        render={({ field, formState: { errors } }) => (
          <div className="relative">
            <input
              className={className.input}
              type={type}
              {...field}
              aria-invalid={errors[name] ? "true" : "false"}
            />
            {errors[name] && (
              <sub
                className={`
                  absolute top-[150%] left-0 w-max
                  max-w-full break-after-all leading-4
                  text-wrap wrap-normal text-red-700
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
