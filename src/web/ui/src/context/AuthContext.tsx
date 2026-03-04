import { useState, type ReactNode } from 'react';
import { AuthContext } from '../hooks/useAuth'

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    // Inicializamos comprobando si hay un token guardado
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(
        !!localStorage.getItem('auth_token')
    );

    const login = (token: string) => {
        localStorage.setItem('auth_token', token);
        setIsAuthenticated(true);
    };

    const logout = () => {
        localStorage.removeItem('auth_token');
        setIsAuthenticated(false);
    };

    return (
        <AuthContext.Provider value={{ isAuthenticated, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};
