import { useState } from 'react';
import { useFetch } from '../hooks/useFetch';
import { PaginatedTable } from '../components/PaginatedTable';

interface Project {
    id: string;
    name: string;    
    createdAt: string;
}

interface ApiResponse {
    items: Project[];
    totalPages: number;
}

export const ProjectsPage = () => {
    const [page, setPage] = useState(1);

    // Llamada a tu API de .NET (ej: http://localhost:5000/api/projects)
    const { data, loading, error } = useFetch<ApiResponse>('/api/projects', {
        page,
        pageSize: 5
    });

    const columns = [
        { header: 'ID', key: 'id' as keyof Project },
        { header: 'Nombre del Proyecto', key: 'name' as keyof Project },        
        { header: 'Fecha', key: 'createdAt' as keyof Project },
    ];

    if (error) return <div className="text-red-500">Error: {error}</div>;

    return (
        <div className="space-y-6">
            <h1 className="text-2xl font-bold text-gray-800">Proyects list</h1>

            <PaginatedTable
                data={data?.items || []}
                columns={columns}
                currentPage={page}
                totalPages={data?.totalPages || 1}
                onPageChange={(newPage) => setPage(newPage)}
                isLoading={loading}
            />
        </div>
    );
};