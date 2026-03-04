import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthProvider';
import { ProtectedRoute } from './components/ProtectedRoute';

import { LoginPage } from './pages/LoginPage';
import { HomePage } from './pages/HomePage';
import { ProjectsPage } from './pages/ProjectsPage';

function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />

                    <Route element={<ProtectedRoute />}>
                        <Route path="/home"     element={<HomePage />}      />
                        <Route path="/projects" element={<ProjectsPage />}  />
                    </Route>                    
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );    
}

export default App;