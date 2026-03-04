import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from './pages/LoginPage';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';

const Dashboard = () => (
    <div className="p-8">
        <h1 className="text-2xl font-bold">Bienvenido al Dashboard</h1>
        <p className="text-gray-600">Has iniciado sesión correctamente.</p>
    </div>
);

function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />

                    <Route element={<ProtectedRoute />}>
                        <Route path="/dashboard" element={<Dashboard />} />
                    </Route>                    
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );    
}

export default App;