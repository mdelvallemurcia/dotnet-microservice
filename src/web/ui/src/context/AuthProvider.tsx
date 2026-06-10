import { useState, useEffect, type ReactNode } from 'react';
import { AuthContext } from './AuthContext';
import { logout, refreshAccessToken } from "../api/authClient";

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
        await logout(accessToken);

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
                const newToken = await refreshAccessToken();

                if (newToken) {
                    afterLoginActions(newToken);
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