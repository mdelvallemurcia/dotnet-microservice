const API_URL = import.meta.env.VITE_API_URL;

// Single in-flight refresh shared by every caller. Concurrent 401s (or React StrictMode's
// double-mount) reuse this one promise instead of each firing its own /v1/refresh, which would
// rotate the refresh token several times in parallel and trip server-side reuse detection.
let refreshPromise: Promise<string | null> | null = null;

export function refreshAccessToken(): Promise<string | null> {
    refreshPromise ??= fetch(`${API_URL}/v1/refresh`, {
        method: 'POST',
        credentials: 'include',
    })
        .then((res) => (res.ok ? res.json() : null))
        .then((data) => (data?.accessToken as string) ?? null)
        .catch(() => null)
        .finally(() => {
            refreshPromise = null;
        });

    return refreshPromise;
}
