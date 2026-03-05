import { createContext } from 'react';

export interface AuthContextType {
    isAuthenticated: boolean;
    accessToken: string | null;
    afterLoginActions: (token: string) => void;
    afterLogoutActions: () => void;    
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);