import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const ProtectedRoute = () => {
    const { isAuthenticated } = useAuth();

    if (!isAuthenticated) {
        // Redirigir al login si no está autenticado
        return <Navigate to="/login" replace />;
    }

    // Si está autenticado, renderiza el contenido hijo (Dashboard)
    return <Outlet />;
};