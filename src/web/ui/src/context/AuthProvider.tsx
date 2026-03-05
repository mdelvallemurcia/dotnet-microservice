import { useState, useEffect, type ReactNode } from 'react';
import { AuthContext } from './AuthContext';

const API_URL =
    import.meta.env.VITE_API_URL ||
    import.meta.env.API_HTTPS ||
    'https://localhost:7035';

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
    const [isInitialLoading, setIsInitialLoading] = useState(true);

    const afterLoginActions = (token: string) => {
        setAccessToken(token);
        setIsAuthenticated(true);

        localStorage.setItem("refreshTokenPresent", "true");
    };

    const afterLogoutActions = async () => {
        //TODO useFetch 

        try {
            await fetch(`${API_URL}/v1/logout`, {
                method: "POST",
                credentials: "include"
            });
        } catch (err) {
            console.error(err);
        }

        setAccessToken(null);
        setIsAuthenticated(false);

        localStorage.removeItem("refreshTokenPresent");
    };

    useEffect(() => {
        const silentRefresh = async () => {

            const hasRefreshToken = localStorage.getItem("refreshTokenPresent");

            if (!hasRefreshToken) {
                setIsInitialLoading(false);
                return;
            }

            try {
                const response = await fetch(`${API_URL}/v1/refresh`, {
                    method: "POST",
                    credentials: "include",
                    headers: { "Content-Type": "application/json" }
                });

                if (response.ok) {
                    const data = await response.json();
                    afterLoginActions(data.accessToken);
                } else {
                    localStorage.removeItem("refreshTokenPresent");
                    setIsAuthenticated(false);
                }
            } catch {
                setIsAuthenticated(false);
            } finally {
                setIsInitialLoading(false);
            }
        };

        silentRefresh();
    }, []);

    if (isInitialLoading) return <div>Loading session...</div>;

    return (
        <AuthContext.Provider value={{ isAuthenticated, accessToken, afterLoginActions, afterLogoutActions }}>
            {children}
        </AuthContext.Provider>
    );
};

//const refreshAccessToken = async (): Promise<string | null> =>
//{
//    const hasRefreshToken = localStorage.getItem("refreshTokenPresent");
//    if (!hasRefreshToken) return null;
//    try {
//        const response = await fetch(`${API_URL}/v1/refresh`, {
//            method: "POST",
//            credentials: "include"
//        });
//        if (!response.ok) return null;
//        const data = await response.json();
//        setAccessToken(data.accessToken);
//        setIsAuthenticated(true);
//        return data.accessToken;
//    }
//    catch {
//        return null;
//    }
//};