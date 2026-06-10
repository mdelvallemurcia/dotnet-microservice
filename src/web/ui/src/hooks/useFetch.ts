import { useState, useEffect, useCallback } from 'react';
import { useAuth } from './useAuth';
import { refreshAccessToken } from '../api/authClient';
import type { ProblemDetails } from '../types/api';

interface FetchOptions {
    method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
    body?: Record<string, unknown> | unknown;
    params?: Record<string, string | number>;
}

const API_URL = import.meta.env.VITE_API_URL;

export function useFetch<T>(url: string, options: FetchOptions = {}) {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | ProblemDetails | null>(null);
    const { accessToken, afterLoginActions, afterLogoutActions } = useAuth();

    const executeFetch = useCallback(
        async (manualOptions?: FetchOptions, isRetry = false, overrideToken?: string): Promise<T | null> => {
            setLoading(true);
            const currentOptions = { ...options, ...manualOptions };
            // On a retry we pass the freshly refreshed token explicitly: the closure's `accessToken`
            // is stale within this execution (setState hasn't re-rendered this callback yet).
            const tokenToUse = overrideToken ?? accessToken;

            try
            {
                const fullUrl = new URL(url.startsWith('http') ? url : `${API_URL}${url}`);

                if (currentOptions.params) {
                    Object.entries(currentOptions.params).forEach(([key, val]) => {
                        fullUrl.searchParams.append(key, String(val));
                    });
                }

                const fetchConfig: RequestInit = {
                    method: currentOptions.method || 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...(tokenToUse ? { Authorization: `Bearer ${tokenToUse}` } : {}),
                    },
                    credentials: 'include',
                };

                if (currentOptions.body && currentOptions.method !== 'GET') {
                    fetchConfig.body = JSON.stringify(currentOptions.body);
                }

                const response = await fetch(fullUrl.toString(), fetchConfig);
                const hasRefreshToken = localStorage.getItem("refreshTokenPresent");

                if (response.status === 401 && !isRetry && hasRefreshToken) {
                    const newToken = await refreshAccessToken();

                    if (newToken) {
                        afterLoginActions(newToken);
                        return await executeFetch(manualOptions, true, newToken);
                    }
                    else {
                        afterLogoutActions();
                        throw new Error("Session expired. Please login again.");
                    }
                }

                const result = await response.json().catch(() => null);
                if (!response.ok) {
                    if (result && (result.title || result.errors)) {
                        setError(result as ProblemDetails);
                        throw new Error("API_PROBLEM_DETAILS");                        
                    }
                    throw new Error(`Error ${response.status}: ${response.statusText}`);
                }
                
                setData(result);
                setError(null);
                return result;
            }
            catch (err: unknown) {
                if (err instanceof Error) {                    
                    if (err.message !== "API_PROBLEM_DETAILS") {
                        setError(err.message);
                    }
                    console.error("Request error:", err.message);
                } else {
                    setError("Unhandled error");
                }
                return null;
            }
            finally {
                setLoading(false);
            }
        },
        [url, options, accessToken, afterLoginActions, afterLogoutActions]
    );

    useEffect(() => {
        // Skip auto-fetch for manual actions like login; data lists fetch on mount.
        if (url !== '/v1/login') {
            executeFetch();
        }
    }, [url, JSON.stringify(options.params)]);

    return { data, loading, error, refetch: executeFetch };
}