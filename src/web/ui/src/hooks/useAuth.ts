import { createContext, useContext } from 'react';

interface AuthContextType {
    isAuthenticated: boolean;
    login: (token: string) => void;
    logout: () => void;
}

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error("useAuth should be used into an AuthProvider");
    return context;
};

export const AuthContext = createContext<AuthContextType | undefined>(undefined);