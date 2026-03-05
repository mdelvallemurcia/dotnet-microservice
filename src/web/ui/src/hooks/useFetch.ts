import { useState, useEffect, useCallback } from 'react';
import { useAuth } from './useAuth';

interface FetchOptions {
    page?: number;
    pageSize?: number;
}

export function useFetch<T>(url: string, options: FetchOptions = { page: 1, pageSize: 10 }) {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const { logout } = useAuth();

    const fetchData = useCallback(async () => {
        setLoading(true);
        const token = localStorage.getItem('auth_token');

        try {
            // Construimos la URL con parámetros de paginación
            const fullUrl = new URL(url, window.location.origin);
            fullUrl.searchParams.append('page', String(options.page));
            fullUrl.searchParams.append('pageSize', String(options.pageSize));

            const response = await fetch(fullUrl.toString(), {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                },
            });

            if (response.status === 401) {
                logout(); // Token expirado o inválido
                return;
            }

            if (!response.ok) throw new Error('Error getting data');

            const result = await response.json();
            setData(result);
            setError(null);
        } catch (err: any) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, [url, options.page, options.pageSize, logout]);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    return { data, loading, error, refetch: fetchData };
}