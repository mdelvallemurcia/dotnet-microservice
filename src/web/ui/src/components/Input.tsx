import type { ChangeEventHandler } from "react";

export const Input = ({ label, type, placeholder, value, onChange, required }: { label: string, type: string, placeholder: string, value: string, onChange: ChangeEventHandler<T, HTMLInputElement>, required: boolean}) => (
    <div className="mb-4">
        <label className="block text-gray-700 text-sm font-bold mb-2">{label}</label>
        <input
            type={type}
            placeholder={placeholder}
            value={value}
            onChange={onChange}
            required={required}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
    </div>
);