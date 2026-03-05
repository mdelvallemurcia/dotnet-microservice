interface Props<T> {
    data: T[];
    columns: { header: string; key: keyof T }[];
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
    isLoading?: boolean;
}

export function PaginatedTable<T>({ data, columns, currentPage, totalPages, onPageChange, isLoading }: Props<T>) {
    return (
        <div className="bg-white rounded-lg shadow overflow-hidden">
            <table className="w-full text-left border-collapse">
                <thead className="bg-gray-50 border-b">
                    <tr>
                        {columns.map((col) => (
                            <th key={String(col.key)} className="p-4 text-sm font-semibold text-gray-700">
                                {col.header}
                            </th>
                        ))}
                    </tr>
                </thead>
                <tbody className="divide-y">
                    {isLoading ? (
                        <tr><td colSpan={columns.length} className="p-10 text-center">Cargando datos...</td></tr>
                    ) : (
                        data.map((item, idx) => (
                            <tr key={idx} className="hover:bg-gray-50">
                                {columns.map((col) => (
                                    <td key={String(col.key)} className="p-4 text-sm text-gray-600">
                                        {String(item[col.key])}
                                    </td>
                                ))}
                            </tr>
                        ))
                    )}
                </tbody>
            </table>

            {/* Controles de Paginación */}
            <div className="p-4 flex items-center justify-between border-t bg-gray-50">
                <button
                    disabled={currentPage === 1}
                    onClick={() => onPageChange(currentPage - 1)}
                    className="px-4 py-2 border rounded disabled:opacity-50 bg-white"
                >
                    Anterior
                </button>
                <span className="text-sm text-gray-600">Página {currentPage} de {totalPages}</span>
                <button
                    disabled={currentPage === totalPages}
                    onClick={() => onPageChange(currentPage + 1)}
                    className="px-4 py-2 border rounded disabled:opacity-50 bg-white"
                >
                    Siguiente
                </button>
            </div>
        </div>
    );
}