import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { DashboardLayout } from '../layouts/DashboardLayout';

export const ProtectedRoute = () => {
    const { isAuthenticated } = useAuth();

    if (!isAuthenticated) {
        // Redirigir al login si no está autenticado
        return <Navigate to="/login" replace />;
    }

    // Si está autenticado, renderiza el contenido hijo (Dashboard)
    return (
        <DashboardLayout>
            <Outlet />
        </DashboardLayout>
    );
};