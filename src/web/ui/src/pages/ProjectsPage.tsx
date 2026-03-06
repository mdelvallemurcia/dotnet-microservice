import { useState } from 'react';
import { useFetch } from '../hooks/useFetch';
import { PaginatedTable } from '../components/PaginatedTable';
import type { PagedData } from '../types/pagedData'
import type { ProjectEntity } from '../types/projects'

export const ProjectsPage = () => {
    const [page, setPage] = useState(1);

    // Llamada a tu API de .NET (ej: http://localhost:5000/api/projects)
    //const { data, loading, error } = useFetch<ApiResponse>('/v1/projects'{
    //    page,
    //    pageSize: 5
    //});
    const { loading, data  } = useFetch<PagedData<ProjectEntity>>(
        '/v1/projects',
        { method: 'GET' }
    );

    const columns: { header: string; key: keyof ProjectEntity }[] = [
        { header: 'ID'              , key: 'id'           },
        { header: 'Name'            , key: 'name'         },        
        { header: 'Creation date'   , key: 'creationDate' },
    ];

    //if (error) return <div className="text-red-500">Error: {error}</div>;

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