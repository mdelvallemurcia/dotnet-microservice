import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from './pages/LoginPage';

const Dashboard = () => (
    <div className="p-8">
        <h1 className="text-2xl font-bold">Bienvenido al Dashboard</h1>
        <p className="text-gray-600">Has iniciado sesión correctamente.</p>
    </div>
);

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Ruta pública */}
                <Route path="/login" element={<LoginPage />} />

                {/* Ruta privada (simulada por ahora) */}
                <Route path="/dashboard" element={<Dashboard />} />

                {/* Redirección por defecto: si no sabe a dónde ir, al login */}
                <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
        </BrowserRouter>
    );    
}

export default App;