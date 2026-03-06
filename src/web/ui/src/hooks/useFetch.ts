import { useState, useEffect, useCallback } from 'react';
import { useAuth } from './useAuth';

interface FetchOptions {
    method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
    body?: Record<string, unknown> | unknown;
    params?: Record<string, string | number>;
}

const API_URL = import.meta.env.VITE_API_URL;

//const controller = new AbortController();
//setTimeout(() => controller.abort(), 5000);
//fetch(url, { signal: controller.signal });

export function useFetch<T>(url: string, options: FetchOptions = {}) {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const { accessToken, afterLoginActions, afterLogoutActions } = useAuth();

    const executeFetch = useCallback(
        async (manualOptions?: FetchOptions, isRetry = false): Promise<T | null> => {
            setLoading(true);
            const currentOptions = { ...options, ...manualOptions };

            try {
                const fullUrl = new URL(url.startsWith('http') ? url : `${API_URL}${url}`);

                // Agregar params si existen
                if (currentOptions.params) {
                    Object.entries(currentOptions.params).forEach(([key, val]) => {
                        fullUrl.searchParams.append(key, String(val));
                    });
                }

                const fetchConfig: RequestInit = {
                    method: currentOptions.method || 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
                    },
                    credentials: 'include',
                };

                if (currentOptions.body && currentOptions.method !== 'GET') {
                    fetchConfig.body = JSON.stringify(currentOptions.body);
                }

                const response = await fetch(fullUrl.toString(), fetchConfig);

                // --- MANEJO DE TOKEN EXPIRADO ---
                if (response.status === 401 && !isRetry) {
                    console.log("Access Token expired, trying to refresh...");

                    const refreshRes = await fetch(`${API_URL}/v1/refresh`, {
                        method: 'POST',
                        credentials: 'include',
                    });

                    if (refreshRes.ok) {
                        const refreshData = await refreshRes.json();
                        afterLoginActions(refreshData.accessToken); // Guardamos el nuevo token en memoria

                        // REINTENTO: Llamamos de nuevo a executeFetch pasando isRetry = true
                        return await executeFetch(manualOptions, true);
                    } else {
                        // Si el refresh falla, la sesión es irrecuperable
                        afterLogoutActions();
                        throw new Error("Session expired. Please login again.");
                    }
                }

                if (!response.ok) {
                    const errorData = await response.json().catch(() => ({}));
                    throw new Error(errorData.message || `Error ${response.status}`);
                }

                const result = await response.json();
                setData(result);
                setError(null);
                return result;
            } catch (err: unknown) {
                if (err instanceof Error) {
                    if (err.message == 'Error 400')
                        setError('Validation error, please check your data');
                    else
                        setError(err.message);
                    console.error("Request error:", err.message);
                }
                else if (typeof err === 'string') {
                    setError(err);
                }
                else {
                    setError("Unhandled error");
                }
                return null;
            } finally {
                setLoading(false);
            }
        },
        [url, options, accessToken, afterLoginActions, afterLogoutActions]
    );

    useEffect(() => {
        // No ejecutamos automáticamente si no hay URL o si es una acción manual (como login)
        // Pero para listas de datos, se ejecuta al montar:
        if (url !== '/v1/login') {
            executeFetch();
        }
    }, [url, JSON.stringify(options.params)]);

    return { data, loading, error, refetch: executeFetch };
}